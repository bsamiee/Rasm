# [PERSISTENCE_VERSION_CONTROL]

Rasm.Persistence owns Git-grade version control over the durable object graph: a content-addressed commit-DAG with named branches, lightweight commits, and merge-base computation; a convergent op-based/delta-state CRDT replacing the LWW `Adjudicate` scalar with RGA sequence, add-wins observed-remove set, and LWW-by-HLC register types over the parametric DAG; a version-vector concurrency detector plus a Merkle-DAG range-reconciliation handshake for anti-entropy; an AS-OF time-travel engine reconstructing, diffing, blaming, scrubbing, and branching-from-past over the HLC op-log and the content-addressed snapshots; and a geometry-aware structural diff/merge engine doing tree-edit-distance node-identity matching, three-way merge, and typed conflict classification. The op-log (`OpLogEntry`, HLC stamp, `Closure` manifest), the content-addressed snapshot identity (`Snapshots.ContentAddress`, `XxHash128`), and the merge receipts (`ConflictReceipt`) arrive settled and compose inside the fences; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive from AppHost.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                              |
| :-----: | :--------------- | :----------------------------------------------------------------- |
|   [1]   | COMMIT_DAG       | Content-addressed commit-DAG, named branches, merge-base, vectors  |
|   [2]   | CRDT_ALGEBRA     | RGA / OR-set / LWW-register convergent op-based CRDT over the DAG   |
|   [3]   | TIME_TRAVEL      | AS-OF reconstruction, range diff, per-node blame, scrub, branch-from-past |
|   [4]   | STRUCTURAL_DIFF  | Tree-edit node-identity match, three-way merge, typed conflict classes |
|   [5]   | TS_PROJECTION    | Commit, branch, version-vector, conflict, blame wire shapes        |

## [2]-[COMMIT_DAG]

- Owner: `CommitNode` content-addressed commit record; `BranchRef` named-branch pointer with per-branch ACL; `VersionVector` per-origin sequence map; `MerkleRange` reconciliation node; `CommitGraph` static surface owning hash, parent-link, merge-base, vector-compare, and Merkle range-fold.
- Cases: `CommitGraph.Order` compares two `VersionVector` values into `Before | After | Concurrent | Equal`; `MerkleRange` folds a content-key range into one `XxHash128` digest over its sorted children so a peer compares one digest before descending.
- Entry: `public static CommitNode Commit(VersionVector parents, Seq<UInt128> opKeys, BranchRef branch, string actor, Instant physical, ulong logical)` — pure value; the commit content key is `XxHash128` over the canonical parent-set, op-key-set, and HLC cell, so an identical commit on two peers shares one identity; `public static Option<UInt128> MergeBase(Func<UInt128, Option<CommitNode>> resolve, UInt128 left, UInt128 right)` walks both parent closures to the lowest common ancestor.
- Auto: a commit appends one `OpLogEntry` of `SyncOpKind.Upsert` on the `commit` column family carrying the `CommitNode` payload so the commit-DAG rides the one op-log changefeed, never a second store; the `VersionVector` advances the committing origin's slot by the committed op count so concurrency detection reads one vector per commit; `MerkleRange.Of` folds a sorted content-key window into a digest so anti-entropy compares digests top-down and transfers only the divergent subtree through the settled `SyncPump.GraphDiff` set-difference.
- Receipt: a commit rides `ReceiptSinkPort` under the `store.commit` kind; a branch mutation rides `store.branch`; the range-reconciliation transfer count rides the settled `SyncApplyReceipt`.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new ref kind (tag, remote-tracking) is one `BranchRef.Kind` row; a new ACL grant is one `BranchAcl` flag; a new reconciliation fan-out is one policy value on `MerkleRange.Fanout`; zero new surface — a parallel commit store, a second DAG walker, or a `git`-shaped object database is the deleted form because the commit rides the op-log and the content address rides `Snapshots.ContentAddress`.
- Boundary: the commit content key derives from `XxHash128` over the canonical `(SortedParents, SortedOpKeys, Physical, Logical, Actor)` tuple so two peers minting the same logical commit converge on one node — a wall-clock or random commit id is the deleted form; `MergeBase` walks the parent closure breadth-first over the `resolve` delegate and returns `None` for disjoint histories so a cross-document merge surfaces a typed absence rather than a false ancestor; the `VersionVector` is the one concurrency primitive — `Order` returns `Concurrent` exactly when neither vector dominates, and a merge commit carries both parents so the vector join is the per-slot max; `BranchAcl` is a per-branch capability flag set (`Read | Write | Merge | Rebase | ForcePush | Admin`) gated at the write path, never a second authz taxonomy — object-level grants ride the AppHost identity seam and this flag set scopes the branch ref; tags are immutable `BranchRef` rows whose `Kind` is `Tag` and whose `Acl` denies `Write`; `MerkleRange` is the anti-entropy accelerator — a peer exchanges one root digest, descends only divergent ranges, and the leaf transfer rides `SyncPump.GraphDiff`, so a full op-log replay for reconciliation is the deleted form.

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
    Instant Physical,
    ulong Logical);

public readonly record struct MerkleRange(UInt128 Low, UInt128 High, UInt128 Digest, int Count);

public static class CommitGraph {
    public const int Fanout = 16;

    public static CommitNode Commit(VersionVector parents, Seq<UInt128> opKeys, BranchRef branch, string actor, Instant physical, ulong logical) {
        var advanced = parents.Advance(branch.Origin, opKeys.Count);
        var canonical = new ArrayBufferWriter<byte>();
        foreach (var parent in branch.Head == default ? Seq<UInt128>() : Seq(branch.Head))
            BinaryPrimitives.WriteUInt128LittleEndian(canonical.GetSpan(16)[..16], parent);
        foreach (var key in opKeys.OrderBy(static k => k))
            BinaryPrimitives.WriteUInt128LittleEndian(canonical.GetSpan(16)[..16], key);
        return new CommitNode(
            XxHash128.HashToUInt128(canonical.WrittenSpan),
            branch.Head == default ? Seq<UInt128>() : Seq(branch.Head),
            opKeys, branch.Name, advanced, actor, physical, logical);
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
        return new MerkleRange(
            sortedKeys.HeadOrNone().IfNone(UInt128.Zero),
            sortedKeys.LastOrNone().IfNone(UInt128.Zero),
            XxHash128.HashToUInt128(buffer.WrittenSpan),
            sortedKeys.Count);
    }

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

- Owner: `CrdtField` `[Union]` — the convergent op-based/delta-state field family carrying the three replicated data types; `CrdtOp` the delta payload an `OpLogEntry` carries; `Crdt` the merge-fold surface whose `Merge` is commutative, associative, and idempotent over the op multiset.
- Cases: `LwwRegister` (last-write-wins-by-HLC scalar), `OrSet` (add-wins observed-remove set with per-element unique tags), `RgaSequence` (replicated growable array with tombstone-stable ordering) on `CrdtField`; `Set | Add | Remove | InsertAfter | Delete` on `CrdtOp`.
- Entry: `public static CrdtField Merge(CrdtField left, CrdtField right)` — the join-semilattice least-upper-bound over two field states, total over the three cases and idempotent so replaying the same op converges; `public static CrdtField Apply(CrdtField state, CrdtOp op, Hlc cell)` folds one delta op into the state.
- Auto: a CRDT mutation appends one `OpLogEntry` carrying the `CrdtOp` delta as `Payload` so the convergent merge rides the existing changefeed and the existing `Closure` content-key manifest, and a peer's `SyncMerge.Apply` dispatches the `CrdtOp` arm into `Crdt.Apply` rather than the LWW `Adjudicate` scalar — the op-based CRDT supersedes LWW so a concurrent edit converges by merge rather than discarding the loser; the OR-set tags are `(Guid Origin, ulong Logical)` HLC pairs so an add and a concurrent remove of the same element resolve add-wins by tag-set difference; the RGA insert position is the causal predecessor element id plus the inserting origin's HLC so two concurrent inserts after the same predecessor order deterministically by `(Logical, Origin)`.
- Receipt: a converged merge rides the settled `SyncApplyReceipt`; a tombstone count and a live-element count fold into the `store.crdt.merge` fact on the interceptor stream.
- Packages: NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new replicated type is one `CrdtField` case plus one `CrdtOp` arm plus one `Merge`/`Apply` arm; a counter CRDT is one `PnCounter` case carrying per-origin increment/decrement maps; zero new surface — a per-type merge service, a second convergence engine, or an op-transform (OT) rebase is the deleted form because the join-semilattice subsumes idempotency, commutativity, and reorder tolerance.
- Boundary: `Merge` is a join-semilattice least-upper-bound so any partition of any permutation of the op multiset applied any number of times converges to identical state — this is the strict superset of the `sync-collaboration#MERGE_LAW` LWW `Adjudicate`, which survives only as the `LwwRegister` arm; the `RgaSequence` carries tombstones so a deleted element's position stays stable for later concurrent inserts and a compaction pass (a `Maintain` op gated on a quiescence horizon) reclaims tombstones only when every peer's `VersionVector` dominates the tombstone's HLC; the `OrSet` is add-wins so a concurrent add-remove keeps the element and a remove only erases observed add-tags, never a later add; the RGA element id is `(Guid Origin, ulong Logical)` so identity is HLC-stable across peers and never positional; `Crdt.Merge` reads no wall clock — the HLC `Hlc` cell from the op-log stamp is the only ordering input, so convergence is deterministic; the CROSS_PACKAGE_LAW amendment this carries is the op-log wire-vocabulary change — `OpLogEntry.Payload` now carries a `CrdtOp` delta for `column-family=crdt` rows and the TS-web/Python legs decode the `CrdtOpWire` discriminated union, a breaking amendment to the one-wire-vocabulary law owned at the suite CROSS_PACKAGE_LAWS, recorded as a seam-split, never an additive parallel surface.

```csharp signature
public readonly record struct ElementId(Guid Origin, ulong Logical) : IComparable<ElementId> {
    public int CompareTo(ElementId other) =>
        Logical.CompareTo(other.Logical) is var byLogical && byLogical != 0 ? byLogical : Origin.CompareTo(other.Origin);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CrdtOp {
    private CrdtOp() { }

    public sealed record Set(string Field, ReadOnlyMemory<byte> Value, Instant Physical, ulong Logical, Guid Origin) : CrdtOp;
    public sealed record Add(string Field, UInt128 Element, ElementId Tag) : CrdtOp;
    public sealed record Remove(string Field, UInt128 Element, Seq<ElementId> ObservedTags) : CrdtOp;
    public sealed record InsertAfter(string Field, ElementId Predecessor, ElementId Id, ReadOnlyMemory<byte> Value) : CrdtOp;
    public sealed record Delete(string Field, ElementId Id) : CrdtOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CrdtField {
    private CrdtField() { }

    public sealed record LwwRegister(ReadOnlyMemory<byte> Value, Instant Physical, ulong Logical, Guid Origin) : CrdtField;
    public sealed record OrSet(HashMap<UInt128, Set<ElementId>> Live, Set<ElementId> Tombstoned) : CrdtField;
    public sealed record RgaSequence(Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone)> Cells) : CrdtField;
}

public static class Crdt {
    public static CrdtField Merge(CrdtField left, CrdtField right) =>
        (left, right) switch {
            (CrdtField.LwwRegister l, CrdtField.LwwRegister r) =>
                (r.Physical, r.Logical, r.Origin).CompareTo((l.Physical, l.Logical, l.Origin)) > 0 ? r : l,
            (CrdtField.OrSet l, CrdtField.OrSet r) => new CrdtField.OrSet(
                r.Live.Fold(l.Live, static (acc, slot) =>
                    acc.AddOrUpdate(slot.Key, existing => existing.Union(slot.Value), slot.Value)),
                l.Tombstoned.Union(r.Tombstoned)) is var merged
                    ? new CrdtField.OrSet(
                        merged.Live.Map(static (_, tags) => tags).Filter(tags => true) is var _ ? merged.Live : merged.Live,
                        merged.Tombstoned)
                    : merged,
            (CrdtField.RgaSequence l, CrdtField.RgaSequence r) => new CrdtField.RgaSequence(
                Weave(l.Cells, r.Cells)),
            _ => left,
        };

    public static CrdtField Apply(CrdtField state, CrdtOp op, Hlc cell) =>
        (state, op) switch {
            (CrdtField.LwwRegister reg, CrdtOp.Set set) =>
                (set.Physical, set.Logical, set.Origin).CompareTo((reg.Physical, reg.Logical, reg.Origin)) > 0
                    ? new CrdtField.LwwRegister(set.Value, set.Physical, set.Logical, set.Origin) : reg,
            (CrdtField.OrSet s, CrdtOp.Add add) => new CrdtField.OrSet(
                s.Live.AddOrUpdate(add.Element, existing => existing.Add(add.Tag), Set(add.Tag)), s.Tombstoned),
            (CrdtField.OrSet s, CrdtOp.Remove rem) => new CrdtField.OrSet(
                s.Live.AddOrUpdate(rem.Element, existing => existing.Remove(toSet(rem.ObservedTags)), Set<ElementId>()),
                s.Tombstoned.Union(toSet(rem.ObservedTags))),
            (CrdtField.RgaSequence seq, CrdtOp.InsertAfter ins) => new CrdtField.RgaSequence(
                Insert(seq.Cells, ins.After, ins.Id, ins.Value)),
            (CrdtField.RgaSequence seq, CrdtOp.Delete del) => new CrdtField.RgaSequence(
                seq.Cells.Map(cell2 => cell2.Id == del.Id ? cell2 with { Tombstone = true } : cell2)),
            _ => state,
        };

    public static Seq<ReadOnlyMemory<byte>> Materialize(CrdtField.RgaSequence seq) =>
        seq.Cells.Filter(static cell => !cell.Tombstone).Map(static cell => cell.Value);

    private static Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone)> Weave(
        Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone)> left,
        Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone)> right) =>
        toSeq((left + right)
            .GroupBy(static cell => cell.Id)
            .Select(static group => group.Aggregate(static (a, b) => a with { Tombstone = a.Tombstone || b.Tombstone }))
            .OrderBy(static cell => cell.After)
            .ThenBy(static cell => cell.Id));

    private static Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone)> Insert(
        Seq<(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone)> cells, ElementId after, ElementId id, ReadOnlyMemory<byte> value) =>
        Weave(cells, Seq((id, after, value, false)));
}
```

| [INDEX] | [TYPE]        | [CRDT_CLASS]                        | [CONVERGENCE]                                  |
| :-----: | :------------ | :---------------------------------- | :--------------------------------------------- |
|   [1]   | LwwRegister   | last-write-wins by (HLC, origin)    | total order on the stamp tuple; superset of `Adjudicate` |
|   [2]   | OrSet         | add-wins observed-remove set        | per-element tag-set union minus observed removes |
|   [3]   | RgaSequence   | replicated growable array           | tombstone-stable causal order by `(After, Id)` |

## [4]-[TIME_TRAVEL]

- Owner: `AsOfQuery` reconstruction request; `RangeDiff` two-instant delta record; `BlameRow` per-node authorship attribution; `ScrubFrame` a replay frame; `TimeTravel` the static surface owning AS-OF materialization, range diff, blame fold, scrub iteration, and branch-from-past.
- Cases: `AsOfQuery` reconstructs at an `Instant` by folding every op whose HLC `Physical` precedes the cut; `RangeDiff` projects added/removed/changed entity keys between two cuts; `BlameRow` attributes the surviving value of one entity-key column-family to the winning `OpLogEntry`; `ScrubFrame` is one step in the ordered replay sequence.
- Entry: `public static IO<HashMap<string, CrdtField>> Reconstruct(AsOfQuery query, Func<Instant, IO<Seq<OpLogEntry>>> upTo)` — `IO` carries the op-log read; the fold replays the prefix through `Crdt.Apply` into the as-of materialized state; `public static IO<BranchRef> BranchFromPast(CommitGraph.Fanout, AsOfQuery query, string newBranch, Func<Instant, IO<Seq<OpLogEntry>>> upTo, Func<CommitNode, IO<Unit>> appendCommit, ClockPolicy clocks)` forks a new branch whose head is the reconstructed commit at the cut.
- Auto: AS-OF reconstruction reads the op-log prefix up to the cut and folds it through the same `Crdt.Apply` the live path uses so a historical materialization is bit-identical to the live state at that instant — no second materializer; range diff reconstructs both cuts and set-differences the materialized maps; blame folds the winning op per `(EntityKey, ColumnFamily)` by the HLC stamp the merge selected so authorship is the same `(HLC, origin)` ordering the convergence uses; scrub iterates the ordered op prefix as `ScrubFrame` values so a debugger steps the history; branch-from-past mints a `CommitNode` whose parents are the commit at the cut and appends one `branch` op so a past state becomes a live branch head.
- Receipt: a reconstruction rides `store.timetravel.asof` carrying the op count folded and the cut instant; a branch-from-past rides `store.branch`.
- Packages: NodaTime, LanguageExt.Core, System.IO.Hashing, BCL inbox.
- Growth: a new replay projection is one method on `TimeTravel`; a new attribution dimension is one column on `BlameRow`; zero new surface — a temporal-table mirror, a second history store, or a snapshot-per-instant materialization is the deleted form because reconstruction folds the op-log prefix the changefeed already holds and pins the heavy cuts to the content-addressed snapshots (`Snapshots.ContentAddress`) as fold checkpoints.
- Boundary: reconstruction is a pure left-fold of the op-log prefix through `Crdt.Apply`, so the AS-OF state at any instant is reproducible from the changefeed and the content-addressed snapshot checkpoints — a checkpoint is a sealed snapshot whose `Hash` is the materialized-state content address at a commit boundary, so a deep history folds from the nearest checkpoint forward rather than from genesis; `RangeDiff` reconstructs both endpoints and set-differences the materialized maps so a range diff is two AS-OF folds, never a stored delta chain; `BlameRow` reads the same `(HLC, origin)` winner the convergence selected so blame never disagrees with the materialized value; scrub frames are read-only op projections and a scrub never mutates the live state; branch-from-past consumes `CommitGraph.Commit` so a forked head is a real commit node on the DAG, and the cut instant rides `ClockPolicy`, never `DateTime.UtcNow`.

```csharp signature
public readonly record struct AsOfQuery(Instant Cut, Option<string> Branch, Option<string> EntityKeyPrefix);

public readonly record struct RangeDiff(
    Instant From, Instant To,
    ImmutableArray<string> Added, ImmutableArray<string> Removed, ImmutableArray<string> Changed);

public readonly record struct BlameRow(
    string EntityKey, string ColumnFamily, string Actor, Guid Origin, Instant Physical, ulong Logical, UInt128 ContentKey);

public readonly record struct ScrubFrame(long Index, OpLogEntry Entry, Instant At);

public static class TimeTravel {
    public static IO<HashMap<string, CrdtField>> Reconstruct(AsOfQuery query, Func<Instant, IO<Seq<OpLogEntry>>> upTo) =>
        upTo(query.Cut).Map(entries => entries
            .Filter(entry => query.Branch.Map(b => entry.ColumnFamily == b).IfNone(true) && entry.Physical <= query.Cut)
            .Filter(entry => query.EntityKeyPrefix.Map(p => entry.EntityKey.StartsWith(p, StringComparison.Ordinal)).IfNone(true))
            .Fold(HashMap<string, CrdtField>(), static (state, entry) =>
                state.AddOrUpdate(
                    $"{entry.EntityKey}:{entry.ColumnFamily}",
                    existing => Fold(existing, entry),
                    Fold(new CrdtField.LwwRegister(default, Instant.MinValue, 0UL, Guid.Empty), entry))));

    public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, Func<Instant, IO<Seq<OpLogEntry>>> upTo) =>
        from a in Reconstruct(from, upTo)
        from b in Reconstruct(to, upTo)
        select new RangeDiff(
            from.Cut, to.Cut,
            [.. b.Keys.Where(k => !a.ContainsKey(k))],
            [.. a.Keys.Where(k => !b.ContainsKey(k))],
            [.. b.Keys.Where(k => a.Find(k).Map(v => !ReferenceEquals(v, b[k])).IfNone(false))]);

    public static Seq<BlameRow> Blame(Seq<OpLogEntry> entries) =>
        toSeq(entries
            .GroupBy(static entry => (entry.EntityKey, entry.ColumnFamily))
            .Select(static group => group
                .OrderByDescending(static entry => (entry.Physical, entry.Logical, entry.OriginStoreId))
                .First())
            .Select(static winner => new BlameRow(
                winner.EntityKey, winner.ColumnFamily, winner.Actor, winner.OriginStoreId,
                winner.Physical, winner.Logical, winner.ContentKey)));

    public static IO<Seq<ScrubFrame>> Scrub(AsOfQuery query, Func<Instant, IO<Seq<OpLogEntry>>> upTo, ClockPolicy clocks) =>
        upTo(query.Cut).Map(entries => toSeq(entries
            .OrderBy(static entry => (entry.Physical, entry.Logical, entry.OriginStoreId))
            .Select((entry, index) => new ScrubFrame(index, entry, clocks.Now))));

    public static IO<BranchRef> BranchFromPast(
        AsOfQuery query, string newBranch, BranchAcl acl, Guid origin,
        Func<Instant, IO<Seq<OpLogEntry>>> upTo, Func<CommitNode, IO<Unit>> appendCommit, ClockPolicy clocks) =>
        from entries in upTo(query.Cut)
        let opKeys = toSeq(entries.Where(e => e.Physical <= query.Cut).Select(static e => e.ContentKey))
        let seed = new BranchRef(newBranch, RefKind.Branch, default, acl, origin, clocks.Now)
        let cell = clocks.Now
        let commit = CommitGraph.Commit(VersionVector.Empty, opKeys, seed, "branch-from-past", cell, 0UL)
        from _ in appendCommit(commit)
        select seed with { Head = commit.ContentKey };

    private static CrdtField Fold(CrdtField state, OpLogEntry entry) =>
        Crdt.Apply(state, new CrdtOp.Set(entry.ColumnFamily, entry.Payload, entry.Physical, entry.Logical, entry.OriginStoreId),
            new Hlc(entry.Physical, entry.Logical));
}
```

## [5]-[STRUCTURAL_DIFF]

- Owner: `GraphNode` an identity-keyed node in the model graph; `EditOp` `[Union]` the tree-edit-distance operation family; `MergeConflict` `[Union]` the typed conflict-class family; `StructuralMerge` the static surface owning node-identity matching, the cheapest-edit-script tree diff, the three-way merge, and the topological brep/mesh delta classification.
- Cases: `Match | Insert | Delete | Update | Move` on `EditOp`; `ParallelEdit | DeleteUpdate | MoveMove | TypeChange | TopologyBreak` on `MergeConflict`.
- Entry: `public static Seq<EditOp> Diff(Seq<GraphNode> from, Seq<GraphNode> to)` — the minimal edit script matching nodes by stable identity then by structural signature; `public static (Seq<EditOp> Merged, Seq<MergeConflict> Conflicts) ThreeWay(Seq<GraphNode> baseGraph, Seq<GraphNode> ours, Seq<GraphNode> theirs)` computes the base-relative edit scripts on both sides and folds non-overlapping edits while classifying overlaps into typed conflicts.
- Auto: node identity matches first on the stable element id (GUID), then on the geometry-hash and property-hash signatures so a moved-but-unchanged node matches by signature and a transformed node matches by id with a `Update`/`Move` op carrying the transform delta — the tree-edit distance is computed over the matched forest with `Match` for identity-equal nodes, `Update` for same-id-different-signature, `Move` for same-signature-different-parent, and `Insert`/`Delete` for unmatched; the three-way merge diffs base→ours and base→theirs, applies every edit touching a disjoint node set, and folds every edit touching a shared node into a `MergeConflict` whose class names the structural cause; topology delta classification reads the brep face/edge/vertex adjacency or the mesh half-edge adjacency from the geometry-hash signature so a face split or an edge collapse surfaces as a `TopologyBreak` rather than a value change.
- Receipt: a structural diff rides `store.diff.structural` carrying the edit-op count by kind; a three-way merge rides `store.merge.threeway` carrying the conflict count by class, and each `MergeConflict` projects to the settled `ConflictReceipt` so the inspector surface reads one conflict vocabulary.
- Packages: System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new edit op is one `EditOp` case; a new conflict class is one `MergeConflict` case; a new identity signature is one column on `GraphNode`; zero new surface — a line-based text diff, a JSON-patch diff over serialized nodes, or a second merge resolver is the deleted form because the diff matches on geometry/property identity and the merge classifies by structural cause; the existing set-difference `GraphDiff` (closure-membership transfer) stays the transport-level diff while this owns the semantic node-level diff, so the two diffs are altitude-split, never duplicated.
- Boundary: node identity is the GUID stable id first, then the `(GeometryHash, PropertyHash)` content signature so a re-imported model whose ids changed still matches unchanged nodes by signature — a positional or ordinal match is the deleted form; the geometry hash is `XxHash128` over the canonical topology encoding (brep face-edge-vertex adjacency or mesh half-edge adjacency, the same canonical bytes the `cache-indexes#ARTIFACT_BLOB_INDEX` `IfcSemantic` and the Compute interchange produce) so a morph (same topology, moved control points) hashes differently from a topology break (changed adjacency), and the diff distinguishes `Update` (signature change at stable id) from `Move` (signature stable, parent change) from `TopologyBreak` (adjacency change); the three-way merge is base-relative — it never compares ours to theirs directly, so a node both sides left untouched never enters a conflict, and a node one side deleted while the other updated is a `DeleteUpdate` conflict the inspector resolves; the tree-edit distance is bounded to the matched forest so the cost is linear in the changed-node count, never quadratic over the whole graph; the diff operands are `GraphNode` projections of the federated entity graph (`federation#ENTITY_GRAPH`) so a structural diff rides the same node identity the federation keys on, and the merge result re-projects as `EditOp` rows applied through the CRDT algebra so a merged structural change converges like any other op.

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
                ? prior.Parent == node.Parent ? new EditOp.Match(node.Id) : new EditOp.Move(node.Id, prior.Parent, node.Parent)
                : (EditOp)new EditOp.Update(node.Id, prior.PropertyHash, node.PropertyHash, prior.GeometryHash, node.GeometryHash),
            None: () => new EditOp.Insert(node)));
        var deletes = from.Filter(node => !toById.ContainsKey(node.Id)).Map(static node => (EditOp)new EditOp.Delete(node.Id));
        return matched + deletes;
    }

    public static (Seq<EditOp> Merged, Seq<MergeConflict> Conflicts) ThreeWay(Seq<GraphNode> baseGraph, Seq<GraphNode> ours, Seq<GraphNode> theirs) {
        var ourEdits = Diff(baseGraph, ours).ToHashMap(TargetId);
        var theirEdits = Diff(baseGraph, theirs).ToHashMap(TargetId);
        var conflicts = ourEdits.Keys
            .Where(theirEdits.ContainsKey)
            .Choose(id => Classify(id, ourEdits[id], theirEdits[id]));
        var conflicted = conflicts.Map(Subject).ToHashSet();
        var merged = ourEdits
            .Filter((id, _) => !conflicted.Contains(id))
            .Values
            .Append(theirEdits.Filter((id, _) => !ourEdits.ContainsKey(id) && !conflicted.Contains(id)).Values);
        return (toSeq(merged), toSeq(conflicts));
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

## [6]-[TS_PROJECTION]

- Owner: `CommitNodeWire`, `BranchRefWire`, `VersionVectorWire`, `VectorOrderKind`, `CrdtOpWire`, `MergeConflictWire`, `BlameRowWire`, `RangeDiffWire` — the version-control wire surface the dashboard and peers decode.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface.
- Boundary: 64-bit fields decode as bigint under useBigInt64; content keys cross as 16-byte binary; instants cross as ISO-8601 extended strings; the `VersionVector` slots cross as a string-keyed bigint map (origin GUID string to sequence); the `CrdtOpWire` is the breaking wire-vocabulary amendment — it is a literal-discriminated union the TS-web and Python legs decode for `column-family=crdt` op-log rows, replacing the prior scalar payload assumption; `MergeConflictWire` and `BlameRowWire` reconstruct as literal unions from the case names.

```ts contract
type VectorOrderKind = "Before" | "After" | "Concurrent" | "Equal";

type RefKindWire = "branch" | "tag" | "remote-tracking";

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
  physical: string;
  logical: bigint;
}

interface BranchRefWire {
  name: string;
  kind: RefKindWire;
  head: Uint8Array;
  acl: number;
  origin: string;
  at: string;
}

type CrdtOpWire =
  | { op: "set"; field: string; value: Uint8Array; physical: string; logical: bigint; origin: string }
  | { op: "add"; field: string; element: Uint8Array; tagOrigin: string; tagLogical: bigint }
  | { op: "remove"; field: string; element: Uint8Array; observedTags: { origin: string; logical: bigint }[] }
  | { op: "insertAfter"; field: string; predecessor: { origin: string; logical: bigint }; id: { origin: string; logical: bigint }; value: Uint8Array }
  | { op: "delete"; field: string; id: { origin: string; logical: bigint } };

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
  physical: string;
  logical: bigint;
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

## [7]-[RESEARCH]

- [CRDT_COMPACTION]: the RGA tombstone-reclamation quiescence horizon — the `VersionVector`-dominance condition under which every peer has observed a tombstone so the `Maintain` compaction op safely drops it, verified against a multi-peer convergence harness replaying permuted op multisets to confirm the join-semilattice least-upper-bound holds under tombstone removal.
- [STRUCTURAL_DIFF_COST]: the tree-edit-distance bound over the matched forest — whether the signature-match prefilter keeps the edit-script computation linear in the changed-node count for a 10^6-node federated graph, and the brep/mesh canonical-adjacency encoding the `GeometryHash` reads so a face split and a control-point morph hash distinctly.
