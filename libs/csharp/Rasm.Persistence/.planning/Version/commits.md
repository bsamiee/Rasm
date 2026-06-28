# [PERSISTENCE_VERSION_COMMITS]

Rasm.Persistence content-addressed history over the Marten event substrate: a content-addressed commit-DAG carrying commit messages, named branches, lightweight and annotated tags, remote-tracking refs, true merge commits, and maximal-antichain merge-base computation; a convergent op-based/delta-state CRDT replacing the LWW scalar with RGA sequence, add-wins observed-remove set, multi-value register, PN-counter, and LWW-by-HLC register types over the parametric DAG; an HLC `Hlc` stamp that is the one causal-ordering primitive the Marten event `Timestamp`, the changefeed projection, the CRDT merge, and the wire seam all read; a `CrdtOpWire` op/CRDT encoding that amends the one-wire-vocabulary law field-for-field across the version owner and the AppHost wire seam; and the version wire projection of commit, branch, version-vector, op, conflict, blame, and Merkle range shapes. The op-log changefeed (`Version/ledger#CHANGEFEED` `OpLogEntry`, HLC stamp, `Closure` manifest) is PROJECTED from the Marten events the `Element/graph#STREAM_GRAIN` streams hold, the content-addressed snapshot identity (`Element/codec#CONTENT_ADDRESS` `ContentAddress`, `XxHash128`), the codec profile, and the merge receipts (`ConflictReceipt`) arrive settled and compose inside the fences; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive from AppHost; `NodeId`, `GraphDelta`, and `ContentAddress` arrive from `Rasm.Element`. The `ContentParityCorpus` mints the Persistence leg of the `ONE_WIRE_FIXTURE_CORPUS` — the frozen HLC-cell, commit-key, CRDT-op, and element-set-receipt bytes every cross-runtime parity harness reconciles against the one `XxHash128` seed.

## [01]-[INDEX]

- [01]-[COMMIT_DAG]: content-addressed commit-DAG with commit messages, named branches, annotated tags, maximal-antichain merge-base, and version vectors.
- [02]-[CRDT_ALGEBRA]: RGA, OR-set, MV-register, PN-counter, LWW, and ephemeral-presence convergent CRDT.
- [03]-[CRDT_WIRE]: HLC stamp, `CrdtOp` codec, `CrdtOpWire` op-log payload amendment, and the cross-runtime parity corpus.

## [02]-[COMMIT_DAG]

- Owner: `CommitNode` content-addressed commit record carrying its `CommitMessage`; `BranchRef` named-ref pointer with per-branch `Capability` ACL, upstream tracking, and annotated-tag payload; `CommitMessage` the `[ComplexValueObject]` summary/body pair; `RefKind` the `[SmartEnum<string>]` ref-class axis; `VersionVector` per-origin sequence map; `MerkleRange` reconciliation node; `CommitGraph` static surface owning hash, parent-link, maximal-antichain merge-base, vector-compare, Merkle range-fold, and the recursive anti-entropy descent.
- Cases: `RefKind` is `Branch | LightweightTag | AnnotatedTag | RemoteTracking`, each carrying its `Mutable`/`Annotated` policy; `CommitGraph.Order` compares two `VersionVector` values into `Before | After | Concurrent | Equal`; `MerkleRange` folds a content-key range into one `XxHash128` digest over its sorted children so a peer compares one digest before descending, and `CommitGraph.Reconcile` recursively bisects only divergent subranges.
- Entry: `public static CommitNode Commit(Seq<UInt128> parents, VersionVector inherited, Seq<UInt128> opKeys, BranchRef branch, string actor, Hlc cell, CommitMessage message)` is a pure value whose content key is `XxHash128` over the canonical `(SortedDistinctParents, SortedOpKeys, Hlc)` tuple; `public static Seq<UInt128> MergeBase(Func<UInt128, Option<CommitNode>> resolve, UInt128 left, UInt128 right)` returns the maximal common-ancestor antichain ordered nearest-first; `public static Seq<MerkleRange> Reconcile(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote)` returns the divergent leaf ranges to transfer.
- Auto: a commit appends one `Version/ledger#CHANGEFEED` `OpLogEntry` of `SyncOpKind.Upsert` on the `commit` column family carrying the `CommitNode` payload, so the commit-DAG rides the one changefeed projected off Marten and never a second store; `inherited` is the parent-vector join (`VersionVector.Join` is the per-slot max) advanced by the committed op count on the committing origin's slot, so a merge commit's vector dominates both parents; `MerkleRange.Of` folds a sorted content-key window into a digest so anti-entropy compares top-down and transfers only the divergent subtree.
- Receipt: a commit rides `ReceiptSinkPort` under `store.commit`; a branch mutation rides `store.branch`; the range-reconciliation transfer count rides `SyncApplyReceipt`.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new ref kind is one `RefKind` row; a new ACL grant is one `Capability` flag; a richer commit header is one field on `CommitMessage`/`CommitNode`; zero new surface — a parallel commit store, a second DAG walker, or a git-shaped object database is the deleted form because the commit rides the changefeed and the content address rides `ContentAddress`; a domain-minted commit (the `csharp:Rasm.Bim` `BimCommit`) is one wire-sourced `CommitNode` stored under its carried content key, never re-minted.
- Boundary: the commit content key derives from `XxHash128` over the canonical `(SortedDistinctParents, SortedOpKeys, Hlc)` tuple — `Commit` distinct-sorts parents so a duplicate-parent or reordered-parent merge converges on one node and a wall-clock or random commit id is the deleted form; the `CommitMessage` is NOT in the preimage so re-wording a commit is a fresh node; `MergeBase` is the true merge-base set (reachability rank intersect minus dominated), so a clean history yields one base, a criss-cross yields the two-or-more bases the three-way merge virtualizes, and disjoint histories yield the empty `Seq`; the `VersionVector` is the one concurrency primitive — `Order` returns `Concurrent` exactly when neither dominates; `BranchRef` grants ride the `Element/identity#AUTHORITY` `Capability` bit field narrowed to the branch lane (`Merge | Rebase | ForcePush`) through `Movable` — a two-sided gate (the ref must be `Kind.Mutable`, the actor's grant must hold `Write`, AND the branch ACL must admit `Write`) — never a parallel branch-only enum; a `LightweightTag` is an immutable `BranchRef`, an `AnnotatedTag` adds its `Annotation`/`Tagger`/`Target`, and a `RemoteTracking` ref carries its `Upstream`, all on the one ref shape; the durable commit-DAG is where the `csharp:Rasm.Bim` `BimCommit` federates and durably stores — a domain commit crosses at the wire as one `commit`-family `OpLogEntry` and lands as a generic `CommitNode` stored UNDER the wire-carried content key, never re-derived through `Commit`'s native preimage, and the Bim three-way merge bases against this owner's `MergeBase` antichain.

```csharp signature
public sealed class VersionKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<VersionKeyPolicy, string>]
[KeyMemberComparer<VersionKeyPolicy, string>]
public sealed partial class RefKind {
    public static readonly RefKind Branch = new("branch", mutable: true, annotated: false);
    public static readonly RefKind LightweightTag = new("tag", mutable: false, annotated: false);
    public static readonly RefKind AnnotatedTag = new("tag-annotated", mutable: false, annotated: true);
    public static readonly RefKind RemoteTracking = new("remote-tracking", mutable: true, annotated: false);
    public bool Mutable { get; }
    public bool Annotated { get; }
    private RefKind(string key, bool mutable, bool annotated) : this(key) => (Mutable, Annotated) = (mutable, annotated);
}

[SmartEnum]
public sealed partial class VectorOrder {
    public static readonly VectorOrder Before = new(ordered: true);
    public static readonly VectorOrder After = new(ordered: true);
    public static readonly VectorOrder Concurrent = new(ordered: false);
    public static readonly VectorOrder Equal = new(ordered: true);
    public bool Ordered { get; }
}

public readonly record struct VersionVector(HashMap<Guid, long> Slots) {
    public static readonly VersionVector Empty = new(HashMap<Guid, long>());
    public VersionVector Advance(Guid origin, long count) => new(Slots.AddOrUpdate(origin, e => e + count, count));
    public VersionVector Join(VersionVector other) => new(other.Slots.Fold(Slots, static (acc, s) => acc.AddOrUpdate(s.Key, e => long.Max(e, s.Value), s.Value)));
    public bool Dominates(VersionVector other) => other.Slots.ForAll(s => Slots.Find(s.Key).IfNone(0L) >= s.Value);
    public long At(Guid origin) => Slots.Find(origin).IfNone(0L);
}

[ComplexValueObject]
public sealed partial class CommitMessage {
    public static readonly CommitMessage Empty = new(string.Empty, string.Empty);
    public string Summary { get; }
    public string Body { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string summary, ref string body) {
        if (summary.Length > 4096) validationError = new ValidationError($"<commit-summary-length:{summary.Length}>");
    }
}

public sealed record BranchRef(string Name, RefKind Kind, UInt128 Head, Capability Acl, Guid Origin, Instant At, Option<string> Upstream, Option<UInt128> Target, CommitMessage Annotation, string Tagger) {
    public bool Movable(Capability actor) => Kind.Mutable && actor.Admits(Capability.Write) && Acl.Admits(Capability.Write);
}

public readonly record struct CommitNode(UInt128 ContentKey, Seq<UInt128> Parents, Seq<UInt128> OpKeys, string Branch, VersionVector Vector, string Actor, Hlc Cell, CommitMessage Message) {
    public bool IsMerge => Parents.Count > 1;
    public bool IsRoot => Parents.IsEmpty;
}

public readonly record struct MerkleRange(UInt128 Low, UInt128 High, UInt128 Digest, int Count) {
    public bool Leaf => Count <= CommitGraph.Fanout;
}

public static class CommitGraph {
    public const int Fanout = 16;

    public static CommitNode Commit(Seq<UInt128> parents, VersionVector inherited, Seq<UInt128> opKeys, BranchRef branch, string actor, Hlc cell, CommitMessage message) {
        var parentSet = toSeq(parents.Distinct().OrderBy(static k => k));
        var sortedKeys = toSeq(opKeys.OrderBy(static k => k));
        var canonical = new ArrayBufferWriter<byte>();
        Preimage(canonical, parentSet, sortedKeys, cell);
        return new CommitNode(XxHash128.HashToUInt128(canonical.WrittenSpan), parentSet, sortedKeys, branch.Name, inherited.Advance(branch.Origin, opKeys.Count), actor, cell, message);
    }

    public static void Preimage(IBufferWriter<byte> sink, Seq<UInt128> sortedDistinctParents, Seq<UInt128> sortedOpKeys, Hlc cell) {
        foreach (var parent in sortedDistinctParents) { BinaryPrimitives.WriteUInt128LittleEndian(sink.GetSpan(16), parent); sink.Advance(16); }
        foreach (var key in sortedOpKeys) { BinaryPrimitives.WriteUInt128LittleEndian(sink.GetSpan(16), key); sink.Advance(16); }
        cell.WriteTo(sink);
    }

    public static VectorOrder Order(VersionVector left, VersionVector right) =>
        (left.Slots.Equals(right.Slots), left.Dominates(right), right.Dominates(left)) switch {
            (true, _, _) => VectorOrder.Equal,
            (_, true, false) => VectorOrder.After,
            (_, false, true) => VectorOrder.Before,
            _ => VectorOrder.Concurrent,
        };

    public static Seq<UInt128> MergeBase(Func<UInt128, Option<CommitNode>> resolve, UInt128 left, UInt128 right) {
        var leftRanked = Rank(resolve, left);
        var rightRanked = Rank(resolve, right);
        var common = toSet(toSeq(rightRanked.Keys).Filter(leftRanked.ContainsKey));
        var dominated = common.Fold(Set<UInt128>(), (acc, candidate) => acc.Union(toSet(Rank(resolve, candidate).Keys).Remove(candidate).Intersect(common)));
        return toSeq(common.Filter(c => !dominated.Contains(c)).OrderBy(c => (leftRanked.Find(c).IfNone(0) + rightRanked.Find(c).IfNone(0), c)));
    }

    public static MerkleRange Of(Seq<UInt128> sortedKeys) {
        var buffer = new ArrayBufferWriter<byte>();
        foreach (var key in sortedKeys) { BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16), key); buffer.Advance(16); }
        return new MerkleRange(sortedKeys.HeadOrNone().IfNone(UInt128.Zero), sortedKeys.LastOrNone().IfNone(UInt128.Zero), XxHash128.HashToUInt128(buffer.WrittenSpan), sortedKeys.Count);
    }

    public static Seq<MerkleRange> Reconcile(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote) =>
        local.Digest == remote.Digest ? Seq<MerkleRange>()
        : remote.Leaf ? Seq(remote)
        : children(remote).Bind(child => Sibling(children(local), child) is { IsSome: true, Case: MerkleRange peer } ? Reconcile(children, peer, child) : Seq(child));

    static Option<MerkleRange> Sibling(Seq<MerkleRange> locals, MerkleRange remote) => locals.Find(c => c.Low <= remote.High && remote.Low <= c.High);

    static HashMap<UInt128, int> Rank(Func<UInt128, Option<CommitNode>> resolve, UInt128 root) {
        var depth = new System.Collections.Generic.Dictionary<UInt128, int>();
        var queue = new System.Collections.Generic.Queue<(UInt128 Key, int Generation)>([(root, 0)]);
        while (queue.TryDequeue(out var step))
            if (!depth.TryGetValue(step.Key, out var seen) || step.Generation > seen) {
                depth[step.Key] = step.Generation;
                resolve(step.Key).Iter(node => node.Parents.Iter(parent => queue.Enqueue((parent, step.Generation + 1))));
            }
        return toHashMap(depth.Select(static kv => (kv.Key, kv.Value)));
    }
}
```

| [INDEX] | [POLICY]               | [VALUE]                            | [BINDING]                                              |
| :-----: | :--------------------- | :--------------------------------- | :---------------------------------------------------- |
|  [01]   | commit column family   | `commit`                           | one `OpLogEntry` per commit on the changefeed         |
|  [02]   | merge-base resolution  | maximal common-ancestor antichain  | reachability intersect minus dominated; git multi-base|
|  [03]   | content-key preimage   | parents · op-keys · cell only      | `CommitMessage` excluded; re-word is a fresh node     |
|  [04]   | branch grant           | `Capability` narrowed to branch lane | `BranchRef.Movable` two-sided gate; no parallel enum |
|  [05]   | domain commit ingest   | wire-carried content key           | `BimCommit` lands as one `CommitNode`; bases on `MergeBase` |

## [03]-[CRDT_ALGEBRA]

- Owner: `CrdtField` `[Union]` the convergent op-based/delta-state field family carrying the six replicated data types; `CrdtOp` the delta payload a changefeed entry carries; `RgaCell` the one growable-array element; `Crdt` the merge-fold surface whose `Merge` is commutative, associative, and idempotent over the op multiset, plus the version-vector-gated tombstone compaction.
- Cases: `LwwRegister`, `MvRegister`, `OrSet`, `PnCounter`, `RgaSequence`, `EphemeralMap` on `CrdtField`; `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain | Beat | Leave` on `CrdtOp`.
- Entry: `public static CrdtField Merge(CrdtField left, CrdtField right)` is the join-semilattice least-upper-bound, total over the six cases and idempotent; `public static CrdtField Apply(CrdtField state, CrdtOp op)` folds one delta carrying its HLC cell; `public static CrdtField Compact(CrdtField state, VersionVector quiescent, Instant liveness)` reclaims `RgaSequence` tombstones the quiescence horizon proves unreferenceable and evicts `EphemeralMap` entries past the physical liveness deadline.
- Auto: a CRDT mutation rides one `OpLogEntry` carrying the `CrdtOp` delta as `Payload` so the convergent merge rides the changefeed projected off Marten, and a peer's `SyncMerge.Apply` dispatches the `column-family=crdt` row into `Crdt.Apply` rather than the LWW `Adjudicate` scalar; the OR-set merge is per-element tag-set union minus the union of observed-remove tombstones so add and concurrent remove resolve add-wins; the RGA element id IS the order key (the weave groups by causal predecessor, orders same-predecessor siblings descending by `ElementId`, depth-first-linearizes from the sentinel) so two concurrent inserts converge to one deterministic order on every peer; the PN-counter folds per-origin increments; the MV-register keeps every value its `VersionVector` context dominates none of; the `EphemeralMap` keeps one entry per origin where an incoming `Beat` supersedes only on a strictly-dominating `Hlc`, a `Leave` evicts add-wins-loses against a strictly-later `Beat`, and `Compact` evicts every entry whose last-beat physical instant precedes the liveness deadline — presence expiry reads the physical instant, never the op-count quiescence horizon.
- Receipt: a converged merge rides `SyncApplyReceipt`; a tombstone, live-element, compacted-tombstone, and live-presence count fold into the `store.crdt.merge` fact.
- Packages: NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new replicated type is one `CrdtField` case plus one `CrdtOp` arm plus one `Merge`/`Apply` arm; zero new surface — a per-type merge service, a second convergence engine, or an op-transform rebase is the deleted form because the join-semilattice subsumes idempotency, commutativity, and reorder tolerance.
- Boundary: `Merge` is a join-semilattice least-upper-bound so any partition of any permutation of the op multiset applied any number of times converges to identical state — the strict superset of the `Version/ledger#MERGE_LAW` LWW `Adjudicate`, which survives only as the `LwwRegister` arm; the `RgaSequence` carries tombstones so a deleted slot stays stable for later concurrent inserts and `Compact` reclaims only when the quiescence horizon dominates the cell's lamport stamp; the `OrSet` `Merge` is the per-element live-tag union minus the union of both tombstone sets through `Set.Except` (a `Set.Remove(Set)` is the rejected spelling); the `MvRegister` is a causal anti-chain keeping a value iff no other value's context strictly dominates it; the `PnCounter` is two grow-only per-origin maps monotone under merge; the RGA element id is `(Guid Origin, ulong Logical)` and IS the convergent order key, never positional, so the `RgaCell` carries no redundant `Hlc`; the `EphemeralMap` is per-origin-LWW-by-HLC under add-wins liveness — `Compact` is the durable-presence distinction (an entry whose last-beat physical instant precedes the liveness deadline is a peer that stopped beating, so eviction is convergence-correct and idempotent), presence liveness a physical-time horizon distinct from the RGA op-count tombstone-GC horizon, and the `Maintain` op carries BOTH (`Quiescent` for sequence GC, `Liveness` for presence expiry); `Crdt.Merge` reads no wall clock — the `Hlc` cell from the Marten event stamp is the only ordering input, so convergence is deterministic.

```csharp signature
public readonly record struct ElementId(Guid Origin, ulong Logical) : IComparable<ElementId> {
    public static readonly ElementId Head = new(Guid.Empty, 0UL);
    public int CompareTo(ElementId other) => Logical.CompareTo(other.Logical) is var byLogical && byLogical != 0 ? byLogical : Origin.CompareTo(other.Origin);
}

public readonly record struct RgaCell(ElementId Id, ElementId After, ReadOnlyMemory<byte> Value, bool Tombstone) {
    public static readonly ElementId Origin = ElementId.Head;
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
    public sealed record Maintain(string Field, VersionVector Quiescent, Instant Liveness) : CrdtOp;
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
    public sealed record RgaSequence(Seq<RgaCell> Cells) : CrdtField;
    public sealed record EphemeralMap(HashMap<Guid, (ReadOnlyMemory<byte> State, Hlc Cell)> Live) : CrdtField;
}

public static class Crdt {
    public static CrdtField Merge(CrdtField left, CrdtField right) => (left, right) switch {
        (CrdtField.LwwRegister l, CrdtField.LwwRegister r) => (r.Cell, r.Origin).CompareTo((l.Cell, l.Origin)) > 0 ? r : l,
        (CrdtField.MvRegister l, CrdtField.MvRegister r) => new CrdtField.MvRegister(AntiChain(l.Values + r.Values)),
        (CrdtField.OrSet l, CrdtField.OrSet r) when l.Tombstoned.Union(r.Tombstoned) is var graves =>
            new CrdtField.OrSet(r.Live.Fold(l.Live, static (acc, s) => acc.AddOrUpdate(s.Key, e => e.Union(s.Value), s.Value)).Map((_, tags) => tags.Except(graves)).Filter(static t => t.Count > 0), graves),
        (CrdtField.PnCounter l, CrdtField.PnCounter r) => new CrdtField.PnCounter(MergeMax(l.Positive, r.Positive), MergeMax(l.Negative, r.Negative)),
        (CrdtField.RgaSequence l, CrdtField.RgaSequence r) => new CrdtField.RgaSequence(Weave(l.Cells, r.Cells)),
        (CrdtField.EphemeralMap l, CrdtField.EphemeralMap r) => new CrdtField.EphemeralMap(r.Live.Fold(l.Live, static (acc, s) => acc.AddOrUpdate(s.Key, held => held.Cell.CompareTo(s.Value.Cell) >= 0 ? held : s.Value, s.Value))),
        _ => left,
    };

    public static CrdtField Apply(CrdtField state, CrdtOp op) => (state, op) switch {
        (CrdtField.LwwRegister reg, CrdtOp.Set set) => (set.Cell, set.Origin).CompareTo((reg.Cell, reg.Origin)) > 0 ? new CrdtField.LwwRegister(set.Value, set.Cell, set.Origin) : reg,
        (CrdtField.MvRegister mv, CrdtOp.Write w) => new CrdtField.MvRegister(AntiChain(mv.Values.Filter(h => !w.Context.Dominates(h.Context)).Add((w.Value, w.Context, w.Cell)))),
        (CrdtField.OrSet s, CrdtOp.Add add) => new CrdtField.OrSet(s.Live.AddOrUpdate(add.Element, e => e.Add(add.Tag), Set(add.Tag)), s.Tombstoned),
        (CrdtField.OrSet s, CrdtOp.Remove rem) when toSet(rem.ObservedTags) is var observed => new CrdtField.OrSet(s.Live.AddOrUpdate(rem.Element, e => e.Except(observed), Set<ElementId>()).Filter(static t => t.Count > 0), s.Tombstoned.Union(observed)),
        (CrdtField.PnCounter c, CrdtOp.Increment inc) when inc.Delta >= 0 => new CrdtField.PnCounter(c.Positive.AddOrUpdate(inc.Origin, h => h + inc.Delta, inc.Delta), c.Negative),
        (CrdtField.PnCounter c, CrdtOp.Increment dec) => new CrdtField.PnCounter(c.Positive, c.Negative.AddOrUpdate(dec.Origin, h => h - dec.Delta, -dec.Delta)),
        (CrdtField.RgaSequence seq, CrdtOp.InsertAfter ins) => new CrdtField.RgaSequence(Weave(seq.Cells, Seq(new RgaCell(ins.Id, ins.After, ins.Value, false)))),
        (CrdtField.RgaSequence seq, CrdtOp.Delete del) => new CrdtField.RgaSequence(seq.Cells.Map(c => c.Id == del.Id ? c with { Tombstone = true } : c)),
        (CrdtField.RgaSequence seq, CrdtOp.Maintain m) => Compact(seq, m.Quiescent, m.Liveness),
        (CrdtField.EphemeralMap map, CrdtOp.Beat b) => new CrdtField.EphemeralMap(map.Live.AddOrUpdate(b.Origin, h => h.Cell.CompareTo(b.Cell) >= 0 ? h : (b.State, b.Cell), (b.State, b.Cell))),
        (CrdtField.EphemeralMap map, CrdtOp.Leave l) => new CrdtField.EphemeralMap(map.Live.Find(l.Origin).Filter(h => h.Cell.CompareTo(l.Cell) > 0).IsSome ? map.Live : map.Live.Remove(l.Origin)),
        (CrdtField.EphemeralMap map, CrdtOp.Maintain m) => Compact(map, m.Quiescent, m.Liveness),
        _ => state,
    };

    public static CrdtField Compact(CrdtField state, VersionVector quiescent, Instant liveness) => state switch {
        CrdtField.RgaSequence seq => new CrdtField.RgaSequence(seq.Cells.Filter(c => !c.Tombstone || quiescent.At(c.Id.Origin) < (long)c.Id.Logical)),
        CrdtField.EphemeralMap map => new CrdtField.EphemeralMap(map.Live.Filter((_, slot) => slot.Cell.Physical >= liveness)),
        _ => state,
    };

    public static long Value(CrdtField.PnCounter counter) => counter.Positive.Values.Sum() - counter.Negative.Values.Sum();
    public static Seq<ReadOnlyMemory<byte>> Materialize(CrdtField.RgaSequence seq) => seq.Cells.Filter(static c => !c.Tombstone).Map(static c => c.Value);
    public static Seq<(Guid Origin, ReadOnlyMemory<byte> State)> Live(CrdtField.EphemeralMap map) => toSeq(map.Live.Map(static (o, s) => (o, s.State)));

    static HashMap<Guid, long> MergeMax(HashMap<Guid, long> left, HashMap<Guid, long> right) => right.Fold(left, static (acc, s) => acc.AddOrUpdate(s.Key, h => long.Max(h, s.Value), s.Value));

    static Seq<(ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell)> AntiChain(Seq<(ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell)> values) =>
        toSeq(values.Distinct()).Filter(c => !values.Exists(o => !o.Context.Equals(c.Context) && o.Context.Dominates(c.Context)));

    static Seq<RgaCell> Weave(Seq<RgaCell> left, Seq<RgaCell> right) {
        var merged = (left + right).GroupBy(static c => c.Id).Select(static g => g.Aggregate(static (a, b) => a with { Tombstone = a.Tombstone || b.Tombstone }));
        var children = toHashMap(merged.GroupBy(static c => c.After).Select(static g => (g.Key, toSeq(g.OrderByDescending(static c => c.Id)))));
        return Linearize(children, RgaCell.Origin, Seq<RgaCell>());
    }

    static Seq<RgaCell> Linearize(HashMap<ElementId, Seq<RgaCell>> children, ElementId after, Seq<RgaCell> woven) =>
        children.Find(after).IfNone(Seq<RgaCell>()).Fold(woven, (acc, cell) => Linearize(children, cell.Id, acc.Add(cell)));
}
```

| [INDEX] | [TYPE]       | [CRDT_CLASS]                          | [CONVERGENCE]                                                  |
| :-----: | :----------- | :------------------------------------ | :------------------------------------------------------------- |
|  [01]   | LwwRegister  | last-write-wins by (HLC, origin)      | total order on the stamp tuple; superset of `Adjudicate`      |
|  [02]   | MvRegister   | multi-value concurrent-keep           | causal anti-chain; dominated writes collapse                  |
|  [03]   | OrSet        | add-wins observed-remove set          | per-element tag-set union minus observed removes              |
|  [04]   | PnCounter    | positive-negative per-origin          | per-origin max of monotone partial counts                    |
|  [05]   | RgaSequence  | replicated growable array             | predecessor-keyed weave; `Compact` reclaims at quiescence     |
|  [06]   | EphemeralMap | add-wins observed-remove presence map | per-origin LWW-by-HLC; `Compact` self-expires at liveness     |

## [04]-[CRDT_WIRE]

- Owner: `Hlc` the hybrid-logical-clock stamp the Marten event `Timestamp`, the changefeed projection, the CRDT merge, the commit cell, and the wire all read; `CrdtOpWire` the `[MessagePack.Union]` op encoding the `OpLogEntry.Payload` carries for `column-family=crdt` rows; `CrdtWire` the static codec owning the byte-canonical content key, the `Encode`/`Decode` pair through the package `PersistenceResolver`, and the `UntrustedData` restore-lane decode; `ContentParityCorpus` the frozen-golden-bytes fixture minting the Persistence leg of the `ONE_WIRE_FIXTURE_CORPUS`.
- Cases: 10 op rows — `set | write | add | remove | increment | insertAfter | delete | maintain | beat | leave`; the `[Key]` sequence IS the wire schema, dense and append-only, a retired key never reassigned; the `beat`/`leave` arms carry the `EphemeralMap` presence delta.
- Entry: `public static UInt128 ContentKey(CrdtOp op)` is the byte-canonical content key over the `None`-compression companion encoding; `public static ReadOnlyMemory<byte> Encode(CrdtOp op)` writes the durable delta under `Lz4BlockArray`; `public static ReadOnlyMemory<byte> EncodeCompanion(CrdtOp op)` writes the same delta under `None` for the Python/TS consumers; `public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload)` reads under `UntrustedData` with the depth and decompressed-size ceilings.
- Auto: `Hlc.Observe` swaps the local cell forward past both the wall clock and the observed remote cell so a received op never rewinds the local logical counter; `CrdtWire.Encode` rides the codec profile so a `CrdtOp` delta crosses as `OpLogEntry.Payload` bytes the snapshot codec already verifies; the wire union and the `CrdtOp` union share one case vocabulary so a new op arm is one wire row plus one `CrdtOp` arm plus one map case.
- Receipt: an encoded delta carries no receipt (the `OpLogEntry` carries the codec, content key, and HLC cell); a decode failure folds into `store.crdt.decode` as a typed contract-drift rejection.
- Packages: MessagePack, Thinktecture.Runtime.Extensions.MessagePack, System.IO.Hashing, NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new op is one `CrdtOpWire` `[MessagePack.Union]` tag plus one `[Key]` member plus one `Map`/`Lift` arm; `Lift` over the owned `CrdtOp` `[Union]` is the generated total `Switch` so a new case breaks the build, while `Map` over the foreign wire union stays a language `switch` whose `_ => throw` is the contract-drift guard; zero new surface.
- Boundary: this is the flagship `CrdtOpWire` amendment to the one-wire-vocabulary law — `OpLogEntry.Payload` carries a `CrdtOpWire` union for `column-family=crdt` rows, LWW `Adjudicate` survives only as the `set` arm reconstructing `LwwRegister`, and the breaking descriptor change is owned at `AppHost/runtime-ports#WIRE_LAW` with the TS-web and Python companions decoding the amended payload; the `Hlc` is one packed `(Instant Physical, ulong Logical)` whose ordering is `Physical` then `Logical` so two peers compare causality without a wall clock and `WriteTo` emits the canonical 16-byte cell the commit content key and the op content key both hash; the wire `[Key]` sequence obeys the retirement law so contract drift is a build diagnostic through `MessagePackAnalyzer`; the restore lane reads under `UntrustedData` plus the object-graph depth ceiling AND a bounded decompressed-size cap because a synced delta admits a decompression bomb the depth cap alone never catches; `ContentKey` hashes the `None`-companion canonical bytes (never the LZ4 at-rest framing) so the op content key is byte-reproducible across the C#, Python, and TS runtimes and is the same `XxHash128` identity the structural diff and the federation keys consume; `ContentParityCorpus` freezes the `Hlc.WriteTo` 16-byte cell, the canonical commit-key preimage (the one `CommitGraph.Preimage` writer, never a re-implemented layout), and the `CrdtOpWire` `None`-companion encoding under the one seed convention, plus the cross-page `ElementSetAlgebra.Receipt` bytes CONTRIBUTED by their owner — so the Version owner never depends on a Query owner, the byte SHAPE and seed convention are ground truth authorable now, and the literal digest values stamp on the host-validation pass.

```csharp signature
public readonly record struct Hlc(Instant Physical, ulong Logical) : IComparable<Hlc> {
    public static readonly Hlc Zero = new(Instant.MinValue, 0UL);
    public int CompareTo(Hlc other) => Physical.CompareTo(other.Physical) is var byPhysical && byPhysical != 0 ? byPhysical : Logical.CompareTo(other.Logical);
    public Hlc Advance(Instant wall) => wall > Physical ? new Hlc(wall, 0UL) : new Hlc(Physical, Logical + 1UL);
    public Hlc Observe(Hlc remote, Instant wall) {
        var lead = Instant.Max(Instant.Max(Physical, remote.Physical), wall);
        return new Hlc(lead, (lead == Physical, lead == remote.Physical) switch {
            (true, true) => ulong.Max(Logical, remote.Logical) + 1UL,
            (true, false) => Logical + 1UL,
            (false, true) => remote.Logical + 1UL,
            _ => 0UL,
        });
    }
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
    [MessagePackObject] public sealed record Set([property: Key(0)] string Field, [property: Key(1)] ReadOnlyMemory<byte> Value, [property: Key(2)] long PhysicalTicks, [property: Key(3)] ulong Logical, [property: Key(4)] Guid Origin) : CrdtOpWire;
    [MessagePackObject] public sealed record Write([property: Key(0)] string Field, [property: Key(1)] ReadOnlyMemory<byte> Value, [property: Key(2)] (Guid Origin, long Seq)[] Context, [property: Key(3)] long PhysicalTicks, [property: Key(4)] ulong Logical, [property: Key(5)] Guid Origin) : CrdtOpWire;
    [MessagePackObject] public sealed record Add([property: Key(0)] string Field, [property: Key(1)] UInt128 Element, [property: Key(2)] Guid TagOrigin, [property: Key(3)] ulong TagLogical) : CrdtOpWire;
    [MessagePackObject] public sealed record Remove([property: Key(0)] string Field, [property: Key(1)] UInt128 Element, [property: Key(2)] (Guid Origin, ulong Logical)[] ObservedTags) : CrdtOpWire;
    [MessagePackObject] public sealed record Increment([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] long Delta) : CrdtOpWire;
    [MessagePackObject] public sealed record InsertAfter([property: Key(0)] string Field, [property: Key(1)] Guid PredOrigin, [property: Key(2)] ulong PredLogical, [property: Key(3)] Guid IdOrigin, [property: Key(4)] ulong IdLogical, [property: Key(5)] ReadOnlyMemory<byte> Value) : CrdtOpWire;
    [MessagePackObject] public sealed record Delete([property: Key(0)] string Field, [property: Key(1)] Guid IdOrigin, [property: Key(2)] ulong IdLogical) : CrdtOpWire;
    [MessagePackObject] public sealed record Maintain([property: Key(0)] string Field, [property: Key(1)] (Guid Origin, long Seq)[] Quiescent, [property: Key(2)] long LivenessTicks) : CrdtOpWire;
    [MessagePackObject] public sealed record Beat([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] ReadOnlyMemory<byte> State, [property: Key(3)] long PhysicalTicks, [property: Key(4)] ulong Logical) : CrdtOpWire;
    [MessagePackObject] public sealed record Leave([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] long PhysicalTicks, [property: Key(3)] ulong Logical) : CrdtOpWire;
}

public static class CrdtWire {
    static readonly MessagePackSerializerOptions Write = MessagePackSerializerOptions.Standard
        .WithResolver(CompositeResolver.Create(PersistenceResolver.Instance, StandardResolver.Instance))
        .WithCompression(MessagePackCompression.Lz4BlockArray);
    static readonly MessagePackSerializerOptions Restore = Write.WithSecurity(MessagePackSecurity.UntrustedData.WithMaximumObjectGraphDepth(64).WithMaximumDecompressedSize(1 << 20));
    static readonly MessagePackSerializerOptions Companion = Write.WithCompression(MessagePackCompression.None);

    public static CrdtOpWire Lift(CrdtOp op) => op.Switch<CrdtOpWire>(
        set: static s => new CrdtOpWire.Set(s.Field, s.Value, s.Cell.Physical.ToUnixTimeTicks(), s.Cell.Logical, s.Origin),
        write: static w => new CrdtOpWire.Write(w.Field, w.Value, [.. w.Context.Slots], w.Cell.Physical.ToUnixTimeTicks(), w.Cell.Logical, w.Origin),
        add: static a => new CrdtOpWire.Add(a.Field, a.Element, a.Tag.Origin, a.Tag.Logical),
        remove: static r => new CrdtOpWire.Remove(r.Field, r.Element, [.. r.ObservedTags.Map(static t => (t.Origin, t.Logical))]),
        increment: static i => new CrdtOpWire.Increment(i.Field, i.Origin, i.Delta),
        insertAfter: static ins => new CrdtOpWire.InsertAfter(ins.Field, ins.Predecessor.Origin, ins.Predecessor.Logical, ins.Id.Origin, ins.Id.Logical, ins.Value),
        delete: static d => new CrdtOpWire.Delete(d.Field, d.Id.Origin, d.Id.Logical),
        maintain: static m => new CrdtOpWire.Maintain(m.Field, [.. m.Quiescent.Slots], m.Liveness.ToUnixTimeTicks()),
        beat: static b => new CrdtOpWire.Beat(b.Field, b.Origin, b.State, b.Cell.Physical.ToUnixTimeTicks(), b.Cell.Logical),
        leave: static l => new CrdtOpWire.Leave(l.Field, l.Origin, l.Cell.Physical.ToUnixTimeTicks(), l.Cell.Logical));

    public static CrdtOp Map(CrdtOpWire op) => op switch {
        CrdtOpWire.Set s => new CrdtOp.Set(s.Field, s.Value, new Hlc(Instant.FromUnixTimeTicks(s.PhysicalTicks), s.Logical), s.Origin),
        CrdtOpWire.Write w => new CrdtOp.Write(w.Field, w.Value, new VersionVector(toHashMap(w.Context)), new Hlc(Instant.FromUnixTimeTicks(w.PhysicalTicks), w.Logical), w.Origin),
        CrdtOpWire.Add a => new CrdtOp.Add(a.Field, a.Element, new ElementId(a.TagOrigin, a.TagLogical)),
        CrdtOpWire.Remove r => new CrdtOp.Remove(r.Field, r.Element, toSeq(r.ObservedTags).Map(static t => new ElementId(t.Origin, t.Logical))),
        CrdtOpWire.Increment i => new CrdtOp.Increment(i.Field, i.Origin, i.Delta),
        CrdtOpWire.InsertAfter ins => new CrdtOp.InsertAfter(ins.Field, new ElementId(ins.PredOrigin, ins.PredLogical), new ElementId(ins.IdOrigin, ins.IdLogical), ins.Value),
        CrdtOpWire.Delete d => new CrdtOp.Delete(d.Field, new ElementId(d.IdOrigin, d.IdLogical)),
        CrdtOpWire.Maintain m => new CrdtOp.Maintain(m.Field, new VersionVector(toHashMap(m.Quiescent)), Instant.FromUnixTimeTicks(m.LivenessTicks)),
        CrdtOpWire.Beat b => new CrdtOp.Beat(b.Field, b.Origin, b.State, new Hlc(Instant.FromUnixTimeTicks(b.PhysicalTicks), b.Logical)),
        CrdtOpWire.Leave l => new CrdtOp.Leave(l.Field, l.Origin, new Hlc(Instant.FromUnixTimeTicks(l.PhysicalTicks), l.Logical)),
        _ => throw new MessagePack.MessagePackSerializationException("<crdt-wire-unmapped>"),
    };

    public static ReadOnlyMemory<byte> Encode(CrdtOp op) => MessagePackSerializer.Serialize(Lift(op), Write);
    public static ReadOnlyMemory<byte> EncodeCompanion(CrdtOp op) => MessagePackSerializer.Serialize(Lift(op), Companion);
    public static UInt128 ContentKey(CrdtOp op) => XxHash128.HashToUInt128(EncodeCompanion(op).Span);

    public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload) =>
        Try.lift(() => Map(MessagePackSerializer.Deserialize<CrdtOpWire>(payload, Restore))).Run()
            .MapFail(static error => Error.New(8261, $"<crdt-decode-drift:{error.Message}>"));
}
```

| [INDEX] | [POLICY]              | [VALUE]                               | [BINDING]                                                  |
| :-----: | :-------------------- | :------------------------------------ | :-------------------------------------------------------- |
|  [01]   | HLC stamp source      | the Marten event `Timestamp` cell     | one `Hlc` for op-log, CRDT merge, commit cell, wire       |
|  [02]   | wire schema           | `[Key]` sequence, append-only         | retired key never reassigned; `MessagePackAnalyzer` gate  |
|  [03]   | content key           | `None`-companion canonical bytes      | byte-reproducible across C#/Python/TS; never LZ4 at-rest  |
|  [04]   | restore guard         | `UntrustedData` + depth + size ceiling | decompression bomb caught beyond the depth cap           |
|  [05]   | parity corpus         | one `XxHash128` seed convention       | shape/seed ground truth; digest stamps on host validation |
