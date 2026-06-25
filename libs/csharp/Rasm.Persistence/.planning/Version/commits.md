# [PERSISTENCE_VERSION_COMMITS]

Rasm.Persistence content-addressed history: a content-addressed commit-DAG carrying commit messages, named branches, lightweight and annotated tags, remote-tracking refs, true merge commits, and maximal-antichain merge-base computation; a convergent op-based/delta-state CRDT replacing the LWW `Adjudicate` scalar with RGA sequence, add-wins observed-remove set, multi-value register, PN-counter, and LWW-by-HLC register types over the parametric DAG; an HLC `Hlc` stamp that is the one causal-ordering primitive shared by the op-log, the CRDT merge, and the wire seam; a `CrdtOpWire` op/CRDT encoding (HLC cell, op kinds, causal metadata) that amends the one-wire-vocabulary law field-for-field across the version-control owner and the AppHost wire seam; and the version-control wire projection of commit, branch, version-vector, op, conflict, blame, and Merkle range shapes. The op-log (`OpLogEntry`, HLC stamp, `Closure` manifest), the content-addressed snapshot identity (`Snapshots.ContentAddress`, `XxHash128`), the MessagePack codec profile (`ThinktectureMessageFormatterResolver`, `GeneratedMessagePackResolver`, `Lz4BlockArray`, `UntrustedData` restore lane), and the merge receipts (`ConflictReceipt`) arrive settled and compose inside the fences; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive from AppHost. The `ContentParityCorpus` on `#CRDT_WIRE` mints the Persistence leg of the `ONE_WIRE_FIXTURE_CORPUS` — the frozen HLC-cell, commit-key, CRDT-op, element-set-receipt, and embedding-seed bytes every cross-runtime parity harness reconciles against the one `XxHash128` seed.

## [01]-[INDEX]

- [01]-[COMMIT_DAG]: content-addressed commit-DAG with commit messages, named branches, annotated tags, maximal-antichain merge-base, and version vectors.
- [02]-[CRDT_ALGEBRA]: RGA, OR-set, MV-register, PN-counter, and LWW convergent CRDT.
- [03]-[CRDT_WIRE]: HLC stamp, `CrdtOp` codec, and `CrdtOpWire` op-log payload amendment.
- [04]-[TS_PROJECTION]: commit, branch, version-vector, op, conflict, blame, and Merkle wire shapes.

## [02]-[COMMIT_DAG]

- Owner: `CommitNode` content-addressed commit record carrying its `CommitMessage`; `BranchRef` named-ref pointer with per-branch ACL, upstream tracking, and annotated-tag payload; `CommitMessage` the `[ComplexValueObject]` summary/body pair; `RefKind` the `[SmartEnum<string>]` ref-class axis; `VersionVector` per-origin sequence map; `MerkleRange` reconciliation node; `CommitGraph` static surface owning hash, parent-link, maximal-antichain merge-base, vector-compare, Merkle range-fold, and the recursive anti-entropy descent.
- Cases: `RefKind` is `Branch | LightweightTag | AnnotatedTag | RemoteTracking`, each carrying its `Mutable`/`Annotated` policy so an annotated tag's `Annotation`/`Tagger`/`Target` and a remote-tracking ref's `Upstream` ride the one ref shape; `CommitGraph.Order` compares two `VersionVector` values into `Before | After | Concurrent | Equal`, each carrying its `Ordered` predicate; `MerkleRange` folds a content-key range into one `XxHash128` digest over its sorted children so a peer compares one digest before descending, and `CommitGraph.Reconcile` recursively bisects only divergent subranges.
- Entry: `public static CommitNode Commit(Seq<UInt128> parents, VersionVector inherited, Seq<UInt128> opKeys, BranchRef branch, string actor, Hlc cell, CommitMessage message)` — pure value; `parents` is the real parent commit-key set (zero for a root, two for a merge), `inherited` is the per-slot join of the parent commits' vectors the caller resolves once, and the content key is `XxHash128` over the canonical `(SortedDistinctParents, SortedOpKeys, Hlc)` tuple so an identical commit on two peers shares one identity; `public static Seq<UInt128> MergeBase(Func<UInt128, Option<CommitNode>> resolve, UInt128 left, UInt128 right)` returns the maximal common-ancestor antichain — every common ancestor that is the proper ancestor of no other common ancestor — so a clean history yields one base, a criss-cross yields the two-or-more bases the three-way merge must virtualize, and disjoint histories yield the empty `Seq`; ordered nearest-first by summed generation depth so `HeadOrNone()` is the closest base when the consumer wants one; `public static Seq<MerkleRange> Reconcile(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote)` returns the divergent leaf ranges to transfer.
- Auto: a commit appends one `OpLogEntry` of `SyncOpKind.Upsert` on the `commit` column family carrying the `CommitNode` payload so the commit-DAG rides the one op-log changefeed, never a second store; `inherited` is the parent-vector join (`VersionVector.Join` is the per-slot max), advanced by the committed op count on the committing origin's slot, so a merge commit's vector dominates both parents and concurrency detection reads one vector per commit; `MerkleRange.Of` folds a sorted content-key window into a digest so anti-entropy compares digests top-down and transfers only the divergent subtree through the settled `SyncPump.GraphDiff` set-difference.
- Receipt: a commit rides `ReceiptSinkPort` under the `store.commit` kind; a branch mutation rides `store.branch`; the range-reconciliation transfer count rides the settled `SyncApplyReceipt`.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new ref kind is one `RefKind` row carrying its `Mutable`/`Annotated` policy; a new ACL grant is one `BranchAcl` flag; a new reconciliation fan-out is one policy value on `CommitGraph.Fanout`; a richer commit header is one field on `CommitMessage` or `CommitNode`; zero new surface — a parallel commit store, a second DAG walker, or a `git`-shaped object database is the deleted form because the commit rides the op-log and the content address rides `Snapshots.ContentAddress`; a domain-minted commit — the `csharp:Rasm.Bim/Review/versioning` `BimCommit` — is one wire-sourced `CommitNode` stored under its carried `CommitKey`, never a Bim-specific commit table and never a re-derived identity.
- Boundary: the commit content key derives from `XxHash128` over the canonical `(SortedDistinctParents, SortedOpKeys, Hlc)` tuple — `Commit` distinct-sorts parents so a duplicate-parent or reordered-parent merge converges on one node and a wall-clock or random commit id is the deleted form; the `CommitMessage` is NOT in the content-key preimage so re-wording a commit is a fresh node, and the summary/body split lets the projection truncate without re-hashing; `MergeBase` is the true merge-base set, not a depth heuristic — it builds the reachability rank of each ancestry over the `resolve` delegate, intersects the two key sets, then drops every common ancestor that is a proper ancestor of another common ancestor (a candidate is dominated iff it appears in some other common ancestor's reachability set), so the survivors are the maximal antichain git calls the merge bases: min-summed-generation-depth alone returns an arbitrary criss-cross winner by `UInt128` tiebreak and, because generation is a longest-path measure non-monotone over diamonds, can even return a proper ancestor of the real base that re-surfaces merged changes as spurious conflicts — both are the deleted form; the result is the empty `Seq` for disjoint histories so a cross-document merge surfaces a typed absence rather than a false ancestor, a singleton for the clean fast-forward/single-base case, and the full two-or-more base set for a criss-cross the consuming three-way merge resolves against a virtual base rather than silently picking one; the `VersionVector` is the one concurrency primitive — `Order` returns `Concurrent` exactly when neither vector dominates, and a merge commit's `inherited` join carries both parents so the vector is the per-slot max; `BranchAcl` is a per-branch capability flag set (`Read | Write | Merge | Rebase | ForcePush | Admin`) gated at the write path through `BranchRef.Movable`, a two-sided gate — the ref must be `Kind.Mutable`, the requesting actor's grant set must actually hold `Write` (`actor.HasFlag(BranchAcl.Write)`), AND the branch `Acl` must `Permit` `Write` — so an actor with no `Write` grant never moves a ref (masking the actor against `Write` and feeding the empty result to `Permits` is the deleted form because `Permits(None)` is vacuously true and inverts the gate); `Permits` itself rejects the empty `None` grant for the same reason; never a second authz taxonomy — object-level grants ride the AppHost identity seam and this flag set scopes the branch ref; a `LightweightTag` is an immutable `BranchRef` whose `Kind.Mutable` is false, an `AnnotatedTag` additionally carries its `Annotation` `CommitMessage`, `Tagger`, and `Target` commit key, and a `RemoteTracking` ref carries its `Upstream` ref name — all on the one ref shape, never a parallel tag or remote-ref table; `MerkleRange` is the anti-entropy accelerator — a peer exchanges one root digest, `Reconcile` pairs each remote child with its overlapping local sibling and descends only where digests differ (a remote child with no local sibling transfers whole, a `Leaf` range under `Fanout` transfers whole), and the leaf transfer rides `SyncPump.GraphDiff`, so a full op-log replay for reconciliation is the deleted form; the durable commit-DAG is the owner where the host-neutral content-addressed commit objects the `csharp:Rasm.Bim/Review/versioning#VERSION_GRAPH` `BimCommit` mints federate and durably store, and Persistence stays domain-agnostic and never names the `BimCommit` type — a domain-minted commit crosses at the wire as one `commit`-family `OpLogEntry` and lands as a generic `CommitNode` carrying the Bim `ParentKeys` as `Parents`, the changed-element fingerprint content-keys as `OpKeys`, and the WIRE-CARRIED `CommitKey` as `ContentKey`, stored UNDER the carried content-key and never re-minted through `CommitGraph.Commit`'s native `(parents, op-keys, cell)` preimage (a domain owner folds its authorship and its own fingerprint set into its key, so re-deriving here would split the durable identity from the wire-and-`Review/diff` identity the seam binds), so the durable store keys a domain commit by the same content-key the wire and diff carry and a second commit-identity scheme is the deleted form; the Bim `Version.Merge` three-way algebra resolves against this owner's `MergeBase` maximal common-ancestor antichain — the durable superset of Bim's single-nearest in-memory `CommonAncestor`, surfacing the criss-cross two-or-more-base set a peer-federated history accrues that an `Option<UInt128>` collapses to one arbitrary base — so a domain merge bases against a virtual ancestor across the durable DAG, and Bim minting its own durable DAG is the named seam violation.

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
    public static readonly VectorOrder Before = new(ordered: true);
    public static readonly VectorOrder After = new(ordered: true);
    public static readonly VectorOrder Concurrent = new(ordered: false);
    public static readonly VectorOrder Equal = new(ordered: true);

    public bool Ordered { get; }
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

[ComplexValueObject]
public sealed partial class CommitMessage {
    public static readonly CommitMessage Empty = new(string.Empty, string.Empty);

    public string Summary { get; }

    public string Body { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string summary, ref string body) {
        if (summary.Length > 4096)
            validationError = new ValidationError($"<commit-summary-length:{summary.Length}>");
    }
}

public sealed record BranchRef(
    string Name, RefKind Kind, UInt128 Head, BranchAcl Acl, Guid Origin, Instant At,
    Option<string> Upstream, Option<UInt128> Target, CommitMessage Annotation, string Tagger) {
    public bool Permits(BranchAcl grant) => grant != BranchAcl.None && (Acl & grant) == grant;

    public bool Movable(BranchAcl actor) => Kind.Mutable && actor.HasFlag(BranchAcl.Write) && Permits(BranchAcl.Write);
}

public readonly record struct CommitNode(
    UInt128 ContentKey,
    Seq<UInt128> Parents,
    Seq<UInt128> OpKeys,
    string Branch,
    VersionVector Vector,
    string Actor,
    Hlc Cell,
    CommitMessage Message) {
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
        return new CommitNode(
            XxHash128.HashToUInt128(canonical.WrittenSpan),
            parentSet, sortedKeys, branch.Name,
            inherited.Advance(branch.Origin, opKeys.Count), actor, cell, message);
    }

    public static void Preimage(IBufferWriter<byte> sink, Seq<UInt128> sortedDistinctParents, Seq<UInt128> sortedOpKeys, Hlc cell) {
        foreach (var parent in sortedDistinctParents) {
            BinaryPrimitives.WriteUInt128LittleEndian(sink.GetSpan(16), parent);
            sink.Advance(16);
        }
        foreach (var key in sortedOpKeys) {
            BinaryPrimitives.WriteUInt128LittleEndian(sink.GetSpan(16), key);
            sink.Advance(16);
        }
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
        var dominated = common.Fold(Set<UInt128>(), (acc, candidate) =>
            acc.Union(toSet(Rank(resolve, candidate).Keys).Remove(candidate).Intersect(common)));
        return toSeq(common.Filter(candidate => !dominated.Contains(candidate))
            .OrderBy(candidate => (leftRanked.Find(candidate).IfNone(0) + rightRanked.Find(candidate).IfNone(0), candidate)));
    }

    public static MerkleRange Of(Seq<UInt128> sortedKeys) {
        var buffer = new ArrayBufferWriter<byte>();
        foreach (var key in sortedKeys) {
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16), key);
            buffer.Advance(16);
        }
        return new MerkleRange(
            sortedKeys.HeadOrNone().IfNone(UInt128.Zero),
            sortedKeys.LastOrNone().IfNone(UInt128.Zero),
            XxHash128.HashToUInt128(buffer.WrittenSpan),
            sortedKeys.Count);
    }

    public static Seq<MerkleRange> Reconcile(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote) =>
        local.Digest == remote.Digest
            ? Seq<MerkleRange>()
            : remote.Leaf
                ? Seq(remote)
                : children(remote).Bind(child =>
                    Sibling(children(local), child) is { IsSome: true, Case: MerkleRange peer }
                        ? Reconcile(children, peer, child)
                        : Seq(child));

    private static Option<MerkleRange> Sibling(Seq<MerkleRange> locals, MerkleRange remote) =>
        locals.Find(child => child.Low <= remote.High && remote.Low <= child.High);

    private static HashMap<UInt128, int> Rank(Func<UInt128, Option<CommitNode>> resolve, UInt128 root) {
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

| [INDEX] | [POLICY]               | [VALUE]                          | [BINDING]                                            |
| :-----: | :--------------------- | :------------------------------- | :--------------------------------------------------- |
|  [01]   | commit column family   | `commit`                         | one `OpLogEntry` per commit on the changefeed        |
|  [02]   | branch column family   | `branch`                         | one `OpLogEntry` per ref mutation                    |
|  [03]   | merkle fan-out         | 16 children per range            | `CommitGraph.Fanout`; `MerkleRange.Leaf` cut         |
|  [04]   | merge-commit parentage | two parents, vector join         | `VersionVector.Join` is the per-slot max             |
|  [05]   | merge-base resolution  | maximal common-ancestor antichain | reachability intersect minus dominated; git multi-base |
|  [06]   | content-key preimage   | parents · op-keys · cell only    | `CommitMessage` excluded; re-word is a fresh node    |
|  [07]   | domain commit ingest   | wire-carried `CommitKey`, not re-derived | `csharp:Rasm.Bim` `BimCommit` lands as one `CommitNode`; meet-at-wire content-key; `Version.Merge` bases on `MergeBase` |


## [03]-[CRDT_ALGEBRA]

- Owner: `CrdtField` `[Union]` — the convergent op-based/delta-state field family carrying the six replicated data types; `CrdtOp` the delta payload an `OpLogEntry` carries; `RgaCell` the one growable-array element record (id, causal predecessor, value, tombstone) the `RgaSequence` and the weave share; `Crdt` the merge-fold surface whose `Merge` is commutative, associative, and idempotent over the op multiset, plus the version-vector-gated tombstone compaction.
- Cases: `LwwRegister` (last-write-wins-by-HLC scalar), `MvRegister` (concurrent-keep multi-value register), `OrSet` (add-wins observed-remove set with per-element unique tags), `PnCounter` (positive-negative per-origin counter), `RgaSequence` (replicated growable array of `RgaCell` with tombstone-stable causal order), `EphemeralMap` (the convergent presence type — an add-wins observed-remove map of `(Guid Origin) → (ReadOnlyMemory<byte> State, Hlc Cell)` whose entries self-evict at the liveness horizon so a live-multiplayer cursor/selection/camera/follow surface converges and a departed peer drops without a tombstone) on `CrdtField`; `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain | Beat | Leave` on `CrdtOp`.
- Entry: `public static CrdtField Merge(CrdtField left, CrdtField right)` — the join-semilattice least-upper-bound over two field states, total over the six cases and idempotent so replaying the same op converges; `public static CrdtField Apply(CrdtField state, CrdtOp op)` folds one delta op into the state carrying its HLC cell; `public static CrdtField Compact(CrdtField state, VersionVector quiescent, Instant liveness)` reclaims `RgaSequence` tombstones the op-count `quiescent` horizon proves no concurrent insert can reference and evicts `EphemeralMap` entries whose last-beat `Hlc.Physical` precedes the wall-clock `liveness` deadline — two horizons because tombstone GC is causal-quiescence-gated while presence expiry is physical-liveness-gated, and conflating them is the deleted form.
- Auto: a CRDT mutation appends one `OpLogEntry` carrying the `CrdtOp` delta as `Payload` so the convergent merge rides the existing changefeed and the existing `Closure` content-key manifest, and a peer's `SyncMerge.Apply` dispatches the `column-family=crdt` op-log row into `Crdt.Apply` rather than the LWW `Adjudicate` scalar — the op-based CRDT supersedes LWW so a concurrent edit converges by merge rather than discarding the loser; the OR-set tags are `(Guid Origin, ulong Logical)` HLC pairs and merge is per-element tag-set union minus the union of observed-remove tombstones (`Set.Union`/`Set.Except`), so an add and a concurrent remove of the same element resolve add-wins; the RGA element id IS the order key — the weave groups cells by causal predecessor (`RgaCell.After`), orders same-predecessor siblings descending by `ElementId` (`(Logical, Origin)`), and depth-first-linearizes from the `RgaCell.Origin` sentinel, so two concurrent inserts after the same predecessor converge to one deterministic order on every peer independent of merge order — `Linearize` reaches a cell only through its woven predecessor, so the convergence guarantee rests on the op-log's causal delivery (an `InsertAfter` rides the changefeed after the op that introduced its `Predecessor`, carried by the `Closure` content-key manifest), the one invariant that keeps a not-yet-rooted insert from silently dropping rather than a claim of arbitrary-arrival tolerance; the PN-counter folds per-origin increment maps so a counter converges by per-origin max of monotone partial counts; the MV-register `AntiChain` keeps every value its `VersionVector` context dominates none of, collapsing only a strictly-dominated write, so a later causal write supersedes but a concurrent pair survives for caller resolution; the `EphemeralMap` keeps one entry per origin, an incoming `Beat` supersedes the held entry for that origin only when its `Hlc` cell strictly dominates so a stale reorder never rewinds a peer's live state, a `Leave` evicts the origin's entry add-wins-loses against a strictly-later `Beat`, and `Compact` evicts every entry whose last-beat `Hlc.Physical` precedes the `Maintain.Liveness` wall-clock deadline (`now − liveness-window`) so a crashed peer that stops beating drops at the physical liveness horizon without a durable tombstone — presence expiry reads the physical instant, never the version-vector op-count (an op-count quiescence horizon and an HLC logical tiebreak that resets on every physical tick are incommensurable lattices, so gating presence on `quiescent.At(origin) < cell.Logical` evicts every live entry the moment any origin exceeds a handful of ops and is the deleted form) — the presence type is the one CRDT arm that converges live-multiplayer state on the wire vocabulary rather than the lossy awareness lane.
- Receipt: a converged merge rides the settled `SyncApplyReceipt`; a tombstone count, a live-element count, a compacted-tombstone count, and a live-presence count fold into the `store.crdt.merge` fact on the interceptor stream.
- Packages: NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new replicated type is one `CrdtField` case plus one `CrdtOp` arm plus one `Merge`/`Apply` arm; zero new surface — a per-type merge service, a second convergence engine, or an op-transform (OT) rebase is the deleted form because the join-semilattice subsumes idempotency, commutativity, and reorder tolerance.
- Boundary: `Merge` is a join-semilattice least-upper-bound so any partition of any permutation of the op multiset applied any number of times converges to identical state — this is the strict superset of the `Sync/collaboration#MERGE_LAW` LWW `Adjudicate`, which survives only as the `LwwRegister` arm; the `RgaSequence` carries `RgaCell` tombstones so a deleted element's slot stays stable for later concurrent inserts and `Compact` reclaims a tombstone only when the supplied `VersionVector` quiescence horizon dominates the cell's `ElementId.Logical` lamport stamp for that origin — the [CRDT_COMPACTION] proof harness replays permuted op multisets and confirms the LUB holds under tombstone removal; the `OrSet` is add-wins — `Merge` is the per-element live-tag union minus the union of both tombstone sets through `Set.Except`, so a concurrent add-remove keeps the element and a remove erases only the observed add-tags, never a later add (a `Set.Remove(Set)` set-difference is the rejected spelling — `Set.Remove` is single-element, `Except`/`Subtract` is the set operator); the `MvRegister` is a causal anti-chain — `AntiChain` keeps a value iff no other value's `VersionVector` context strictly dominates it (dominance, never a cell-inequality band-aid), so the register keeps every concurrent value and collapses only a causally-superseded one; the `PnCounter` is two grow-only per-origin maps whose value is positive-minus-negative, monotone under merge so no decrement is ever lost; the RGA element id is `(Guid Origin, ulong Logical)` and IS the convergent order key — identity is lamport-stable across peers and never positional, and the `RgaCell` carries no redundant `Hlc` because the `ElementId` already totally orders the weave (a parallel per-cell HLC clock is the deleted decorative field); the `EphemeralMap` is per-origin-LWW-by-HLC under add-wins liveness — `Merge` keeps the strictly-later `(State, Cell)` per origin so two peers observing each other's beats converge to identical live state, `Leave` carries the departing `Hlc` so a `Leave` erases an entry only when no strictly-later `Beat` survives, and `Compact` is the durable-presence distinction from the lossy awareness lane: an entry whose last-beat `Hlc.Physical` precedes the `Maintain.Liveness` deadline is a peer that stopped beating past the liveness window, so eviction is convergence-correct and idempotent (every peer applies the same physical deadline to the same last-beat instant), never a dropped beat — presence liveness is a physical-time horizon distinct from the `RgaSequence` op-count `quiescent` tombstone-GC horizon, and the `Maintain` op carries BOTH (`Quiescent` for sequence GC, `Liveness` for presence expiry) so one op drives both reclamations without conflating the two lattices; the lossy `Sync/collaboration#PRESENCE_AND_BLOB` `Awareness` lane stays the 60-Hz fire-and-forget cursor stream while this is the converging self-expiring presence map a late-joining peer reconstructs from the op-log prefix; `Crdt.Merge` reads no wall clock — the HLC `Hlc` cell from the op-log stamp is the only ordering input, so convergence is deterministic; the CROSS_PACKAGE_LAW amendment this carries is owned at `#CRDT_WIRE`.

```csharp signature
public readonly record struct ElementId(Guid Origin, ulong Logical) : IComparable<ElementId> {
    public static readonly ElementId Head = new(Guid.Empty, 0UL);

    public int CompareTo(ElementId other) =>
        Logical.CompareTo(other.Logical) is var byLogical && byLogical != 0 ? byLogical : Origin.CompareTo(other.Origin);
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
    public static CrdtField Merge(CrdtField left, CrdtField right) =>
        (left, right) switch {
            (CrdtField.LwwRegister l, CrdtField.LwwRegister r) =>
                (r.Cell, r.Origin).CompareTo((l.Cell, l.Origin)) > 0 ? r : l,
            (CrdtField.MvRegister l, CrdtField.MvRegister r) => new CrdtField.MvRegister(AntiChain(l.Values + r.Values)),
            (CrdtField.OrSet l, CrdtField.OrSet r) when l.Tombstoned.Union(r.Tombstoned) is var graves =>
                new CrdtField.OrSet(
                    r.Live.Fold(l.Live, static (acc, slot) =>
                        acc.AddOrUpdate(slot.Key, existing => existing.Union(slot.Value), slot.Value))
                        .Map((_, tags) => tags.Except(graves))
                        .Filter(static tags => tags.Count > 0),
                    graves),
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
            (CrdtField.OrSet s, CrdtOp.Remove rem) when toSet(rem.ObservedTags) is var observed => new CrdtField.OrSet(
                s.Live.AddOrUpdate(rem.Element, existing => existing.Except(observed), Set<ElementId>())
                    .Filter(static tags => tags.Count > 0),
                s.Tombstoned.Union(observed)),
            (CrdtField.PnCounter c, CrdtOp.Increment inc) when inc.Delta >= 0 => new CrdtField.PnCounter(
                c.Positive.AddOrUpdate(inc.Origin, held => held + inc.Delta, inc.Delta), c.Negative),
            (CrdtField.PnCounter c, CrdtOp.Increment dec) => new CrdtField.PnCounter(
                c.Positive, c.Negative.AddOrUpdate(dec.Origin, held => held - dec.Delta, -dec.Delta)),
            (CrdtField.RgaSequence seq, CrdtOp.InsertAfter ins) => new CrdtField.RgaSequence(
                Weave(seq.Cells, Seq(new RgaCell(ins.Id, ins.After, ins.Value, false)))),
            (CrdtField.RgaSequence seq, CrdtOp.Delete del) => new CrdtField.RgaSequence(
                seq.Cells.Map(cell => cell.Id == del.Id ? cell with { Tombstone = true } : cell)),
            (CrdtField.RgaSequence seq, CrdtOp.Maintain m) => Compact(seq, m.Quiescent, m.Liveness),
            (CrdtField.EphemeralMap map, CrdtOp.Beat beat) => new CrdtField.EphemeralMap(
                map.Live.AddOrUpdate(beat.Origin, held => held.Cell.CompareTo(beat.Cell) >= 0 ? held : (beat.State, beat.Cell), (beat.State, beat.Cell))),
            (CrdtField.EphemeralMap map, CrdtOp.Leave leave) => new CrdtField.EphemeralMap(
                map.Live.Find(leave.Origin).Filter(held => held.Cell.CompareTo(leave.Cell) > 0).IsSome ? map.Live : map.Live.Remove(leave.Origin)),
            (CrdtField.EphemeralMap map, CrdtOp.Maintain m) => Compact(map, m.Quiescent, m.Liveness),
            _ => state,
        };

    public static CrdtField Compact(CrdtField state, VersionVector quiescent, Instant liveness) =>
        state switch {
            CrdtField.RgaSequence seq => new CrdtField.RgaSequence(seq.Cells.Filter(cell =>
                !cell.Tombstone || quiescent.At(cell.Id.Origin) < (long)cell.Id.Logical)),
            CrdtField.EphemeralMap map => new CrdtField.EphemeralMap(map.Live.Filter((_, slot) =>
                slot.Cell.Physical >= liveness)),
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
        toSeq(values.Distinct()).Filter(candidate =>
            !values.Exists(other => !other.Context.Equals(candidate.Context) && other.Context.Dominates(candidate.Context)));

    private static Seq<RgaCell> Weave(Seq<RgaCell> left, Seq<RgaCell> right) {
        var merged = (left + right)
            .GroupBy(static cell => cell.Id)
            .Select(static group => group.Aggregate(static (a, b) => a with { Tombstone = a.Tombstone || b.Tombstone }));
        var children = toHashMap(merged
            .GroupBy(static cell => cell.After)
            .Select(static g => (g.Key, toSeq(g.OrderByDescending(static cell => cell.Id)))));
        return Linearize(children, RgaCell.Origin, Seq<RgaCell>());
    }

    private static Seq<RgaCell> Linearize(HashMap<ElementId, Seq<RgaCell>> children, ElementId after, Seq<RgaCell> woven) =>
        children.Find(after).IfNone(Seq<RgaCell>())
            .Fold(woven, (acc, cell) => Linearize(children, cell.Id, acc.Add(cell)));
}
```

| [INDEX] | [TYPE]       | [CRDT_CLASS]                          | [CONVERGENCE]                                                                                            |
| :-----: | :----------- | :------------------------------------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | LwwRegister  | last-write-wins by (HLC, origin)      | total order on the stamp tuple; superset of `Adjudicate`                                                 |
|  [02]   | MvRegister   | multi-value concurrent-keep           | causal anti-chain; dominated writes collapse                                                             |
|  [03]   | OrSet        | add-wins observed-remove set          | per-element tag-set union minus observed removes                                                         |
|  [04]   | PnCounter    | positive-negative per-origin          | per-origin max of monotone partial counts                                                                |
|  [05]   | RgaSequence  | replicated growable array             | predecessor-keyed weave, `ElementId`-ordered siblings; `Compact` reclaims at quiescence                  |
|  [06]   | EphemeralMap | add-wins observed-remove presence map | per-origin LWW-by-HLC; `Beat` supersedes, `Leave` evicts, `Compact` self-expires at the liveness horizon |


## [04]-[CRDT_WIRE]

- Owner: `Hlc` the hybrid-logical-clock stamp value — the one causal-ordering primitive the op-log stamp, the CRDT merge, the commit cell, and the wire all read; `CrdtOpWire` the `[MessagePack.Union]` op/CRDT encoding the `OpLogEntry.Payload` carries for `column-family=crdt` rows; `CrdtWire` the static codec surface owning the byte-canonical content key, the `Encode`/`Decode` pair through the one package `PersistenceResolver` landmark (the `Version/snapshots#CODEC_AXIS` `[CompositeResolver]` over `ThinktectureMessageFormatterResolver`→`GeneratedMessagePackResolver`, tailed by `StandardResolver`, never a second `CrdtOpWire`-only resolver and never a re-listed `ThinktectureMessageFormatterResolver` the landmark already composes), and the `UntrustedData` restore-lane decode; `ContentParityCorpus` the frozen-golden-bytes fixture owner minting the Persistence leg of the `ONE_WIRE_FIXTURE_CORPUS` — one `ParityVector` table keyed by the one `XxHash128` seed every cross-runtime parity harness reconciles against.
- Cases: 10 op rows — `set | write | add | remove | increment | insertAfter | delete | maintain | beat | leave` on `CrdtOpWire`; the `[Key]` sequence IS the wire schema, dense and append-only, a retired key never reassigned; the `beat`/`leave` arms carry the `EphemeralMap` presence delta so the TS projection version-vector and the UI presence surface decode live-multiplayer state on the one wire vocabulary, and the Python CRDT decode reconstructs the same self-expiring presence map.
- Entry: `public static UInt128 ContentKey(CrdtOpWire op)` — the byte-canonical content key over the `None`-compression companion encoding so an identical op on two peers shares one identity across runtimes (the LZ4 `Write` lane is not byte-reproducible against a wasm/cp315 consumer, so the identity hashes the uncompressed companion bytes); `public static ReadOnlyMemory<byte> Encode(CrdtOp op)` writes the durable delta through the version-control resolver under `Lz4BlockArray` (C#-internal at-rest framing); `public static ReadOnlyMemory<byte> EncodeCompanion(CrdtOp op)` writes the same delta under `MessagePackCompression.None` for the Python and TS consumers that decode uncompressed MessagePack; `public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload)` reads the delta under `MessagePackSecurity.UntrustedData` with the depth ceiling because a synced payload crossed a rest boundary.
- Auto: `Hlc.Advance` swaps the local cell forward past both the wall clock and the observed remote cell so a received op never rewinds the local logical counter — the same `ReceiptSinkPort.Advance` algebra the op-log stamp and every receipt envelope ride; `CrdtWire.Encode` rides the durability codec profile so a `CrdtOp` delta crosses as `OpLogEntry.Payload` bytes that the restore ladder and the snapshot codec already verify, never a second framing; the wire union and the `CrdtOp` union share one case vocabulary so a new op arm is one wire row plus one `CrdtOp` arm plus one map case, zero schema fork.
- Receipt: an encoded delta carries no receipt (the `OpLogEntry` carries the codec, content key, and HLC cell); a decode failure folds into the `store.crdt.decode` fault on the interceptor stream as a typed contract-drift rejection.
- Packages: MessagePack, Thinktecture.Runtime.Extensions.MessagePack, System.IO.Hashing, NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new op is one `CrdtOpWire` `[MessagePack.Union]` tag plus one `[Key]` member plus one `Map`/`Lift` arm; `Lift` over the owned `CrdtOp` `[Union]` is the generated total `Switch` so a new `CrdtOp` case breaks the build rather than throwing the `<crdt-op-unmapped>` default, while `Map` over the foreign `[MessagePack.Union]` `CrdtOpWire` stays a language `switch` whose `_ => throw` is the contract-drift guard at the wire-decode boundary, not a lossy owned-union default; a retired tag is never reassigned because reuse silently re-types history; zero new surface — a typeless payload, a JSON-array delta, or a per-type formatter beside the resolver chain is the deleted form.
- Boundary: this is the FLAGSHIP CrdtOpWire amendment to the one-wire-vocabulary law — `OpLogEntry.Payload` now carries a `CrdtOpWire` discriminated union for `column-family=crdt` rows, LWW `Adjudicate` survives only as the `set` arm reconstructing `LwwRegister`, and the breaking descriptor change is owned at `AppHost/runtime-ports#WIRE_LAW` with the TS-web `wire-consumption` leg and the Python `runtime/ServerHost` companion decoding the amended payload — recorded as a seam-split at the suite CROSS_PACKAGE_LAWS, never an additive parallel surface; the `Hlc` is one packed `(Instant Physical, ulong Logical)` whose ordering is `Physical` then `Logical` so two peers compare causality without a wall clock, and `WriteTo` emits the canonical 16-byte cell the commit content key and the op content key both hash; the wire `[Key]` sequence and the `[MessagePack.Union]` tags obey the durability retirement law so contract drift is a build diagnostic through the `MessagePackAnalyzer`, never a first-restore discovery; the restore lane reads under `UntrustedData` plus the object-graph depth ceiling (`WithMaximumObjectGraphDepth(64)`) AND a bounded `WithMaximumDecompressedSize(1 << 20)` because a synced delta's provenance is unprovable and its `Lz4BlockArray` framing admits a decompression bomb the depth cap alone never catches (a depth budget catches deep-narrow graphs, a decompressed-size budget catches the wide LZ4-expansion a malicious `Value`/`State` payload inflates to) — both budgets fault to a typed `<crdt-decode-drift>` rejection rather than exhausting memory, while the write lane keeps the trusted default at its unbounded size; `ContentKey` hashes the `None`-companion canonical bytes (never the LZ4 at-rest framing) so the op content key the `OpLogEntry` carries is byte-reproducible across the C#, Python, and TS runtimes and is the same `XxHash128` identity the structural diff and the federation keys consume; `ContentParityCorpus` freezes the Persistence leg of the one content-addressed golden corpus — the `Hlc.WriteTo` 16-byte cell (`Int64LE` `Physical.ToUnixTimeTicks()` then `UInt64LE` `Logical`, the VERIFIED layout), the canonical `(SortedParents, SortedOpKeys, Hlc)` commit-key preimage (the one `CommitGraph.Preimage` writer @ `#COMMIT_DAG` that `CommitGraph.Commit` and the corpus both call — never a re-implemented byte layout that could drift unseen from the production hash path), and the `CrdtOpWire` op-set `None`-companion MessagePack encoding (the uncompressed cross-runtime lane the Python and TS legs decode, never the C#-internal `Lz4BlockArray` at-rest lane) under the one seed convention (`SeedOrigin = Guid.Empty`, `SeedInstant = Instant.FromUnixTimeTicks(0)`, seed-zero content), plus the cross-page byte shapes admitted through the `Vectors(crossPage)` parameter — the sorted `ElementSet` receipt bytes (`Query/federation#ELEMENT_SET_ALGEBRA` `ElementSetAlgebra.Receipt`, `UInt128` keys distinct-sorted then `UInt128LE`-packed) and the `EmbeddingIdentity` seed (`Query/lanes#SEARCH_LANES` `EmbeddingIdentity.Of`, `XxHash128` over content × model-id × arity) each CONTRIBUTED by their owner rather than reverse-imported here, so the Version owner never depends on a `Query` owner (Query rides the Version changefeed, never the inverse) — so a Python-recomputed CRDT op-set, a TS-read content key, and a C#-minted commit key reconcile against one frozen corpus and an off-by-one-half HLC encoding or a `(SortedDistinctParents, SortedOpKeys)` sort-order drift fails a single corpus assertion rather than silently folding a fresh op as stale across runtimes; the byte SHAPE, the field order, and the seed convention are ground truth and authorable now, the literal `XxHash128` digest values stamping on the host-validation pass (the OBSERVED digest is frozen, never an un-run asserted value); the corpus is a production fixture-generator running the real canonical encoders (`Hlc.WriteTo`, the shared `CommitGraph.Preimage`, `CrdtWire.EncodeCompanion`), never a `.cs` test source file, and the Geometry `CSHARP_CANONICAL_ADJACENCY_FIXTURE` (the 52-byte single-triangle golden documented @ `#RESEARCH`) is a SIBLING corpus sharing the one `XxHash128` seed convention, never cross-authored here.

```csharp signature
public readonly record struct Hlc(Instant Physical, ulong Logical) : IComparable<Hlc> {
    public static readonly Hlc Zero = new(Instant.MinValue, 0UL);

    public int CompareTo(Hlc other) =>
        Physical.CompareTo(other.Physical) is var byPhysical && byPhysical != 0 ? byPhysical : Logical.CompareTo(other.Logical);

    public Hlc Advance(Instant wall) =>
        wall > Physical ? new Hlc(wall, 0UL) : new Hlc(Physical, Logical + 1UL);

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
    public sealed record Maintain([property: Key(0)] string Field, [property: Key(1)] (Guid Origin, long Seq)[] Quiescent, [property: Key(2)] long LivenessTicks) : CrdtOpWire;
    [MessagePackObject]
    public sealed record Beat([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] ReadOnlyMemory<byte> State, [property: Key(3)] long PhysicalTicks, [property: Key(4)] ulong Logical) : CrdtOpWire;
    [MessagePackObject]
    public sealed record Leave([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] long PhysicalTicks, [property: Key(3)] ulong Logical) : CrdtOpWire;
}

public static class CrdtWire {
    private static readonly MessagePackSerializerOptions Write = MessagePackSerializerOptions.Standard
        .WithResolver(CompositeResolver.Create(
            PersistenceResolver.Instance,
            StandardResolver.Instance))
        .WithCompression(MessagePackCompression.Lz4BlockArray);

    private static readonly MessagePackSerializerOptions Restore =
        Write.WithSecurity(MessagePackSecurity.UntrustedData
            .WithMaximumObjectGraphDepth(64)
            .WithMaximumDecompressedSize(1 << 20));

    private static readonly MessagePackSerializerOptions Companion =
        Write.WithCompression(MessagePackCompression.None);

    public static CrdtOpWire Lift(CrdtOp op) =>
        op.Switch<CrdtOpWire>(
            set:         static s => new CrdtOpWire.Set(s.Field, s.Value, s.Cell.Physical.ToUnixTimeTicks(), s.Cell.Logical, s.Origin),
            write:       static w => new CrdtOpWire.Write(w.Field, w.Value, [.. w.Context.Slots], w.Cell.Physical.ToUnixTimeTicks(), w.Cell.Logical, w.Origin),
            add:         static a => new CrdtOpWire.Add(a.Field, a.Element, a.Tag.Origin, a.Tag.Logical),
            remove:      static r => new CrdtOpWire.Remove(r.Field, r.Element, [.. r.ObservedTags.Map(static t => (t.Origin, t.Logical))]),
            increment:   static i => new CrdtOpWire.Increment(i.Field, i.Origin, i.Delta),
            insertAfter: static ins => new CrdtOpWire.InsertAfter(ins.Field, ins.Predecessor.Origin, ins.Predecessor.Logical, ins.Id.Origin, ins.Id.Logical, ins.Value),
            delete:      static d => new CrdtOpWire.Delete(d.Field, d.Id.Origin, d.Id.Logical),
            maintain:    static m => new CrdtOpWire.Maintain(m.Field, [.. m.Quiescent.Slots], m.Liveness.ToUnixTimeTicks()),
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
            CrdtOpWire.Maintain m => new CrdtOp.Maintain(m.Field, new VersionVector(toHashMap(m.Quiescent)), Instant.FromUnixTimeTicks(m.LivenessTicks)),
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
        XxHash128.HashToUInt128(MessagePackSerializer.Serialize(op, Companion));
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

    public static byte[] CommitKeyPreimage(Seq<UInt128> parents, Seq<UInt128> opKeys, Hlc cell) {
        var canonical = new ArrayBufferWriter<byte>();
        CommitGraph.Preimage(canonical,
            toSeq(parents.Distinct().OrderBy(static k => k)),
            toSeq(opKeys.OrderBy(static k => k)), cell);
        return canonical.WrittenSpan.ToArray();
    }

    public static ParityVector Of(string name, byte[] canonicalBytes) =>
        new(name, canonicalBytes, XxHash128.HashToUInt128(canonicalBytes));

    public static Seq<ParityVector> Vectors(Seq<(string Name, byte[] Bytes)> crossPage) =>
        Seq(
            Of("hlc.zero", HlcCell(SeedCell)),
            Of("commit.genesis", CommitKeyPreimage(Seq<UInt128>(), Seq<UInt128>(), SeedCell)),
            Of("crdt.set.seed", CrdtWire.EncodeCompanion(new CrdtOp.Set("f", new byte[] { 1 }, SeedCell, SeedOrigin)).ToArray()))
        + crossPage.Map(static cross => Of(cross.Name, cross.Bytes));
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
|  [04]   | `elementset.empty` | distinct-sorted `UInt128LE`-packed keys                                      | owner-contributed `crossPage`; `Query/federation#ELEMENT_SET_ALGEBRA` `ElementSetAlgebra.Receipt` |
|  [05]   | `embedding.seed`   | `EmbeddingIdentity.Of` `ContentHash` `UInt128LE` ‖ UTF8 model-id ‖ arity key | owner-contributed `crossPage`; `Query/lanes#SEARCH_LANES` `EmbeddingIdentity.Of`           |

The `XxHash128` digest column of each `ParityVector` is host-frozen: the byte SHAPE, field order, and seed convention above are ground truth and authorable now, the OBSERVED literal digest bytes stamping on the host-validation pass. The two cross-page vectors enter through the `Vectors(crossPage)` parameter — `Query/federation` and `Query/lanes` each contribute their own canonical bytes, so the Version owner never reverse-imports a `Query` owner (Query rides the Version changefeed, never the inverse). The Python (`runtime/evidence/identity`) and TS (`interchange/Codec/frame`) reconciliation legs land in their own units against this corpus.


## [05]-[TS_PROJECTION]

- Owner: `CommitNodeWire`, `CommitMessageWire`, `BranchRefWire`, `VersionVectorWire`, `HlcWire`, `VectorOrderKind`, `RefKindWire`, `CrdtOpWire`, `MerkleRangeWire`, `EditOpKindWire`, `ConflictSideWire`, `MergeConflictWire`, `BlameContributorWire`, `BlameRowWire`, `ChangeKindWire`, `KeyDeltaWire`, `RangeDiffWire` — the version-control wire surface the dashboard and peers decode, every union and record arm-for-arm with its canonical owner so the inspector never decodes a lossy subset.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface; a new ref kind is one `RefKindWire` literal; a new conflict class is one `MergeConflictWire` arm matching the `Version/diff#STRUCTURAL_DIFF` `MergeConflict` `[Union]` arm-for-arm; a new edit op is one `EditOpKindWire` literal matching the `EditOp` `KindName`; a new attribution dimension is one field on `BlameRowWire`/`BlameContributorWire` matching the `Version/timetravel#TIME_TRAVEL` `BlameRow`; a new change classification is one `ChangeKindWire` literal matching the `ChangeKind` row — the projection mirrors its owner case-for-case, so a wire arm the owner does not carry and an owner arm the wire drops are both the deleted form.
- Boundary: 64-bit fields decode as bigint under useBigInt64; content keys cross as 16-byte binary; instants cross as ISO-8601 extended strings; the `Hlc` crosses as `{ physical: string; logical: bigint }` so the TS leg compares causality without a wall clock; the `VersionVector` slots cross as a string-keyed bigint map (origin GUID string to sequence); `CommitNodeWire` carries the `CommitMessageWire` summary/body so the dashboard renders a commit subject without re-deriving it, and `BranchRefWire` carries `upstream` (the remote-tracking ref name, `null` for a local branch), `target`/`annotation`/`tagger` (the annotated-tag payload, `null`/empty for a lightweight tag or branch), and the 4-kind `RefKindWire` so a tag, an annotated tag, and a remote-tracking ref decode on the one ref shape; the `CrdtOpWire` is the breaking wire-vocabulary amendment — it is a literal-discriminated union the TS-web and Python legs decode for `column-family=crdt` op-log rows, replacing the prior scalar payload assumption and adding the `write`/`increment`/`maintain` arms for the multi-value register, PN-counter, and tombstone compaction, plus the `beat`/`leave` arms for the `EphemeralMap` presence type so the TS projection version-vector reconstructs the live-peer set and the UI presence surface renders each origin's converged cursor/selection/camera/follow state, a beat superseded by a strictly-later `cell` and an entry self-expiring when its last-beat `cell.physical` precedes the `maintain.liveness` wall-clock deadline, so a late-joining peer materializes presence from the op-log prefix rather than the lossy awareness lane; `MergeConflictWire` is the literal-discriminated projection of the canonical `Version/diff#STRUCTURAL_DIFF` `MergeConflict` `[Union]` arm-for-arm — all seven cases (`ParallelEdit | DeleteUpdate | MoveMove | ReorderReorder | TypeChange | TopologyBreak | ContainmentCycle`) cross, never the five-of-seven subset that drops `ReorderReorder`/`ContainmentCycle`, and every two-sided arm carries both `ConflictSideWire` `(cell, actor)` stamps the canonical conflict holds (`TopologyBreak` additionally both 16-byte geometry content keys, `MoveMove` both parent ids, `TypeChange` both kind strings, `ReorderReorder` both ordinals, `ContainmentCycle` the cycle-bearing `ancestor` and the single `ByOurs` side) so the inspector reads the SAME stamped held/incoming evidence the C# `ConflictReceipt` carries, never a zero-stamp subset; `EditOpKindWire` is the full seven-kind `EditOp` discriminant (`Match | Insert | Delete | Update | Move | Reorder | Retype`) matching the canonical generated `KindName` so a `ParallelEdit`'s competing edits never collapse a `Reorder`/`Retype` onto `Update`; `BlameRowWire` projects the canonical `Version/timetravel#TIME_TRAVEL` `BlameRow` `(EntityKey, Field)` cell key — `field` is the `CrdtOp.Field` the live merge keys on, never the `columnFamily` (conflating the CRDT data-type lane with the field is the deleted form at the owner) — and carries the winning `(actor, origin, cell, contentKey)` plus the `contributors` count and the `superseded` `BlameContributorWire` lineage so the authorship surface renders the full causal contributor chain the convergence superseded, never a winner-only row; `RangeDiffWire` projects the canonical `RangeDiff` as the per-key `KeyDeltaWire` stream carrying each cell's `ChangeKindWire` (`Added | Removed | Replaced | Converged`) and its `from`/`to` content keys, so the LWW/MV `Replaced` flip stays distinct from the set/counter/sequence `Converged` merge rather than collapsing both into one `changed` bucket; `MerkleRangeWire` carries the anti-entropy descent digest.

```ts contract
type VectorOrderKind = "Before" | "After" | "Concurrent" | "Equal";

type RefKindWire = "branch" | "tag" | "tag-annotated" | "remote-tracking";

interface HlcWire {
  physical: string;
  logical: bigint;
}

interface VersionVectorWire {
  slots: Record<string, bigint>;
}

interface CommitMessageWire {
  summary: string;
  body: string;
}

interface CommitNodeWire {
  contentKey: Uint8Array;
  parents: Uint8Array[];
  opKeys: Uint8Array[];
  branch: string;
  vector: VersionVectorWire;
  actor: string;
  cell: HlcWire;
  message: CommitMessageWire;
}

interface BranchRefWire {
  name: string;
  kind: RefKindWire;
  head: Uint8Array;
  acl: number;
  origin: string;
  at: string;
  upstream: string | null;
  target: Uint8Array | null;
  annotation: CommitMessageWire;
  tagger: string;
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
  | { op: "maintain"; field: string; quiescent: Record<string, bigint>; liveness: string }
  | { op: "beat"; field: string; origin: string; state: Uint8Array; cell: HlcWire }
  | { op: "leave"; field: string; origin: string; cell: HlcWire };

type EditOpKindWire = "Match" | "Insert" | "Delete" | "Update" | "Move" | "Reorder" | "Retype";

interface ConflictSideWire {
  cell: HlcWire;
  actor: string;
}

type MergeConflictWire =
  | { kind: "ParallelEdit"; key: Uint8Array; ours: ConflictSideWire; theirs: ConflictSideWire }
  | { kind: "DeleteUpdate"; key: Uint8Array; deletedByOurs: boolean; ours: ConflictSideWire; theirs: ConflictSideWire }
  | { kind: "MoveMove"; key: Uint8Array; ourParent: Uint8Array | null; theirParent: Uint8Array | null; ours: ConflictSideWire; theirs: ConflictSideWire }
  | { kind: "ReorderReorder"; key: Uint8Array; ourOrdinal: number; theirOrdinal: number; ours: ConflictSideWire; theirs: ConflictSideWire }
  | { kind: "TypeChange"; key: Uint8Array; ourKind: string; theirKind: string; ours: ConflictSideWire; theirs: ConflictSideWire }
  | { kind: "TopologyBreak"; key: Uint8Array; ourGeometry: Uint8Array; theirGeometry: Uint8Array; ours: ConflictSideWire; theirs: ConflictSideWire }
  | { kind: "ContainmentCycle"; key: Uint8Array; ancestor: Uint8Array; byOurs: boolean; ours: ConflictSideWire };

interface BlameContributorWire {
  actor: string;
  origin: string;
  cell: HlcWire;
  contentKey: Uint8Array;
}

interface BlameRowWire {
  entityKey: string;
  field: string;
  actor: string;
  origin: string;
  cell: HlcWire;
  contentKey: Uint8Array;
  contributors: number;
  superseded: readonly BlameContributorWire[];
}

type ChangeKindWire = "Added" | "Removed" | "Replaced" | "Converged";

interface KeyDeltaWire {
  key: string;
  kind: ChangeKindWire;
  from: Uint8Array | null;
  to: Uint8Array | null;
}

interface RangeDiffWire {
  from: HlcWire;
  to: HlcWire;
  deltas: readonly KeyDeltaWire[];
}
```

