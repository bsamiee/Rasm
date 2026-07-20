# [PERSISTENCE_VERSION_COMMITS]

`CommitGraph` owns content-addressed history, ref policy, vector order, merge bases, anti-entropy ranges, and append-only rewrites. `Crdt` owns the convergent field algebra; `CrdtWire` owns its bounded MessagePack encoding; `Hlc` supplies their shared causal cell. `ContentParityCorpus` derives every local fixture from the live writer and accepts foreign fixtures only through `Contribute`. Marten supplies the append substrate, `OpLogEntry` supplies the changefeed envelope, `GrantSet` supplies branch authorization, and `ContentHash.Of` supplies cross-runtime identity.

## [01]-[INDEX]

- [01]-[COMMIT_DAG]: content-addressed commit-DAG with commit messages, named branches, annotated tags, maximal-antichain merge-base, and version vectors.
- [02]-[CRDT_ALGEBRA]: RGA, OR-set, MV-register, PN-counter, LWW, and ephemeral-presence convergent CRDT.
- [03]-[CRDT_WIRE]: HLC stamp, `CrdtOp` codec, `CrdtOpWire` op-log payload amendment, and the cross-runtime parity corpus.

## [02]-[COMMIT_DAG]

- Owner: `CommitNode` content-addressed commit record carrying its `CommitMessage`; `BranchRef` named-ref pointer with a per-branch `Element/authority#GRANT_ALGEBRA` `GrantSet` ACL (the branch-lane narrowing of the one object-authorization vocabulary, never the disjoint AppHost `Capability`), upstream tracking, and annotated-tag payload; `CommitMessage` the `[ComplexValueObject]` summary/body pair; `RefKind` the `[SmartEnum<string>]` ref-class axis; `VersionVector` per-origin sequence map; `MerkleRange` reconciliation node; `HistoryRewrite` the `[Union]` append-only rewrite request family (`Revert | CherryPick | Rebase`) with `RewriteSeam` its delegate frame; `CommitGraph` static surface owning hash, parent-link, maximal-antichain merge-base, vector-compare, Merkle range-fold, the recursive anti-entropy descent, and the one polymorphic `Rewrite` entry.
- Cases: `RefKind` is `Branch | LightweightTag | AnnotatedTag | RemoteTracking`, each carrying its `Mutable`/`Annotated` policy; `CommitGraph.Order` compares two `VersionVector` values into `Before | After | Concurrent | Equal`; `MerkleRange` folds a content-key range into one `XxHash128` digest over its sorted children so a peer compares one digest before descending, and `CommitGraph.Reconcile` recursively bisects only divergent subranges; `HistoryRewrite` closes at `Revert | CherryPick | Rebase`, every case an append-only mint through the one `Commit` writer.
- Entry: `public static CommitNode Commit(Seq<UInt128> parents, VersionVector inherited, Seq<UInt128> opKeys, BranchRef branch, Guid origin, string actor, Hlc cell, CommitMessage message)` is a pure value whose content key is the kernel `ContentHash.Of` over the canonical `(parent-count, SortedDistinctParents, op-count, SortedOpKeys, Branch, VersionVector, Actor, Hlc, CommitMessage)` preimage, the vector advanced on the COMMITTING origin's slot (`origin` — the writer's store id off the session, never `branch.Origin`, which names the ref's minting peer and collapses every writer on one branch into one causal slot); `public static Seq<UInt128> MergeBase(Func<UInt128, Option<CommitNode>> resolve, UInt128 left, UInt128 right)` returns the maximal common-ancestor antichain ordered nearest-first — near-linear: two `Rank` passes plus ONE reverse-reachability generation-mark pass (`Reach`) whose reached-set intersected with the common set IS the dominated set, never a per-candidate `Rank` re-walk; `public static GrantSet AdvanceDemand(CommitNode commit, VersionVector head)` derives the branch-advance authorization demand off `Order` — a merge commit demands `Grant.Merge`, a head-dominating fast-forward `Grant.Write`, a non-dominating reset `Grant.ForcePush` — the demand value `BranchRef.Movable(actor, demand)` gates; `public static Seq<MerkleRange> Reconcile(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote)` returns the divergent leaf ranges to transfer; `public static IO<Seq<CommitNode>> Rewrite(HistoryRewrite rewrite, RewriteSeam seam, BranchRef branch, Guid origin, string actor, UInt128 onto, VersionVector head)` is the one polymorphic history-rewrite entry — the request case discriminates (a `Revert` commits the target's inverse op-key set off `seam.Invert`, a `CherryPick` transplants one commit's ops onto the head off `seam.Transplant`, a `Rebase` `FoldM`-threads the chain oldest-first onto `NewBase`, each transplant landing on the previously minted head) — every arm MINTING new `CommitNode`s through the one `Commit` writer (append-only; the source commits stay untouched) and `public static GrantSet RewriteDemand(HistoryRewrite rewrite)` derives the gate the caller runs through `branch.Movable(actor, RewriteDemand(rewrite))` — `Grant.Write` for the forward-commit `Revert`/`CherryPick`, `Grant.Rebase` for the linearizing rewrite, the entry the grant row exists for.
- Auto: a commit appends one `Version/ledger#CHANGEFEED` `OpLogEntry` of `SyncOpKind.Upsert` on the `commit` column family carrying the `CommitNode` payload, so the commit-DAG rides the one changefeed projected off Marten and never a second store; `inherited` is the parent-vector join (`VersionVector.Join` is the per-slot max) advanced by the committed op count on the COMMITTING origin's slot (the `origin` parameter — two writers on one branch occupy two distinct slots, so `Order` reads their concurrency truthfully), so a merge commit's vector dominates both parents; `MerkleRange.Of` folds a sorted content-key window into a digest so anti-entropy compares top-down and transfers only the divergent subtree.
- Receipt: a commit rides `ReceiptSinkPort` under `store.commit`; a branch mutation rides `store.branch`; the range-reconciliation transfer count rides `SyncApplyReceipt`.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` — the one federation hasher over the commit preimage), System.IO.Hashing (`XxHash128.Append`/`GetCurrentHashAsUInt128` — the incremental `MerkleRange` peer digest only, never a content-key mint), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new ref kind is one `RefKind` row; a new ACL grant is one `Element/authority#GRANT_ALGEBRA` `Grant` row the `GrantSet` admits (never a local flag); a richer commit header is one field on `CommitMessage`/`CommitNode`; a new rewrite verb is one `HistoryRewrite` case plus one `Rewrite` arm and its `RewriteDemand` grant row; zero new surface — a parallel commit store, a second DAG walker, or a git-shaped object database is the deleted form because the commit rides the changefeed and the content address rides `ContentAddress`; a domain-minted commit (the `csharp:Rasm.Bim` `BimCommit`) is one wire-sourced `CommitNode` stored under its carried content key, never re-minted.
- Boundary: the commit content key derives from the kernel `ContentHash.Of` over the canonical `(SortedDistinctParents, SortedOpKeys, Hlc, CommitMessage)` preimage — `Commit` distinct-sorts parents so a duplicate-parent or reordered-parent merge converges on one node and a wall-clock or random commit id is the deleted form; the `CommitMessage` IS in the preimage (length-framed UTF-8 summary+body after the cell) so re-wording a commit mints a fresh node exactly as an amend does in any content-addressed DAG — a preimage that omits a `CommitNode` identity member is the split-brain the identity/preimage agreement law forbids (the `Rasm.Bim` `BimCommit` key deliberately EXCLUDES message for federation-sync idempotency, and that key is wire-carried and stored verbatim, never re-derived here, so the two identity laws never collide); `MergeBase` is the true merge-base set (reachability intersect minus dominated) computed NEAR-LINEAR — two `Rank` passes for the common set and the nearest-first ordering, then ONE `Reach` reverse-reachability pass seeded with every common candidate's parents whose reached∩common set is exactly the dominated candidates (a per-candidate `Rank` re-walk is the deleted `O(candidates × graph)` form; the `Rasm.Bim` three-way merge is the named consumer) — so a clean history yields one base, a criss-cross yields the two-or-more bases the three-way merge virtualizes, and disjoint histories yield the empty `Seq`; the `VersionVector` is the one concurrency primitive — `Order` returns `Concurrent` exactly when neither dominates, and `AdvanceDemand` is its consumer: the branch-advance demand derives once off `(IsMerge, Order(commit.Vector, head))` so the `Movable` gate reads a derived `GrantSet` value, never a caller-guessed grant; `BranchRef` grants ride the `Element/authority#GRANT_ALGEBRA` `GrantSet` (the ONE object-authorization vocabulary narrowed to the branch lane under `AclScope.Branch`, NOT the disjoint AppHost effect-gating `Capability` whose name the authority owner forbids re-using across the strata) through `Movable(actor, demand)` — the demanded lane is `GrantSet.Of(Grant.Write)` for a fast-forward advance, `GrantSet.Of(Grant.Merge)` for a merge commit, `GrantSet.Of(Grant.Rebase)` for a history rewrite, and `GrantSet.Of(Grant.ForcePush)` for a non-fast-forward reset, so one polymorphic gate discriminates the authorization by the demanded value and the per-operation `MovableForMerge`/`MovableForForcePush` family is the deleted form; the gate is the conjunction of the mutability precondition (`Kind.Mutable`) and the two `GrantSet` sides (`actor.Admits(demand)` AND `Acl.Admits(demand)`) where `GrantSet.Admits` is `Admin`-superuser-aware so a branch maintainer carrying `Grant.Admin` passes every demand — never a flat `Write`-only gate that silently lets a `Read`-plus-`Write` actor force-push, and never a parallel branch-only enum; a `LightweightTag` is an immutable `BranchRef`, an `AnnotatedTag` adds its `Annotation`/`Tagger`/`Target`, and a `RemoteTracking` ref carries its `Upstream`, all on the one ref shape; every `HistoryRewrite` is APPEND-ONLY — a revert is the inverse delta as a NEW commit, a cherry-pick one commit's ops replayed onto another head via three-way against its parent, a rebase a sequential replay minting a fresh linear lineage over `NewBase` — history never mutates, the source commits stay reachable, the `Invert`/`Transplant` payload work rides the `Version/merge#STRUCTURAL_DIFF` and `Element/graph` owners behind the `RewriteSeam` delegates (the DAG stores op KEYS, so the seam returns keys and a delta inversion or replay conflict faults on ITS owner's rail BEFORE any commit mints — a half-applied rewrite cannot exist), and a mutating rewrite, a branch-ref force-moved without its `RewriteDemand` gate, or a manual counter-edit standing in for a revert is the deleted form; the durable commit-DAG is where the `csharp:Rasm.Bim` `BimCommit` federates and durably stores — a domain commit crosses at the wire as one `commit`-family `OpLogEntry` and lands as a generic `CommitNode` stored UNDER the wire-carried content key, never re-derived through `Commit`'s native preimage, and the Bim three-way merge bases against this owner's `MergeBase` antichain.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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

// The branch ACL is the `Element/authority#GRANT_ALGEBRA` `GrantSet` narrowed to the branch lane (`AclScope.Branch`) —
// the ONE object-authorization vocabulary, NOT the disjoint AppHost effect-gating `Capability` (a cross-stratum
// name the authority owner forbids sharing). `Movable` gates on a `GrantSet` demand the caller selects per operation
// (`GrantSet.Of(Grant.Write)` for a fast-forward, `Grant.Merge`/`Grant.Rebase`/`Grant.ForcePush` for the wider
// rewrites) so one polymorphic gate discriminates by the demanded value; `GrantSet.Admits` is `Admin`-superuser-aware.
public sealed record BranchRef(string Name, RefKind Kind, UInt128 Head, GrantSet Acl, Guid Origin, Instant At, Option<string> Upstream, Option<UInt128> Target, CommitMessage Annotation, string Tagger) {
    public bool Movable(GrantSet actor, GrantSet demand) =>
        Kind.Mutable && actor.Admits(demand) && Acl.Admits(demand);
}

public readonly record struct CommitNode(UInt128 ContentKey, Seq<UInt128> Parents, Seq<UInt128> OpKeys, string Branch, VersionVector Vector, string Actor, Hlc Cell, CommitMessage Message) {
    public bool IsMerge => Parents.Count > 1;
    public bool IsRoot => Parents.IsEmpty;
}

public readonly record struct MerkleRange(UInt128 Low, UInt128 High, UInt128 Digest, int Count) {
    public bool Leaf => Count <= CommitGraph.Fanout;
}

// The append-only history-rewrite request family: every rewrite MINTS new commits, history never mutates.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record HistoryRewrite {
    private HistoryRewrite() { }
    public sealed record Revert(UInt128 Target) : HistoryRewrite;
    public sealed record CherryPick(UInt128 Pick) : HistoryRewrite;
    public sealed record Rebase(Seq<UInt128> Chain, UInt128 NewBase) : HistoryRewrite;
}

// The rewrite delegate frame: `Resolve` the commit reader; `Invert` the inverse op-key set of one commit's delta
// (the `GraphDelta` inversion behind the ledger/merge owners — added↔removed, revised pairs flipped); `Transplant`
// one commit's ops replayed onto a new head (three-way against the commit's parent, the `Version/merge` owner
// behind the delegate); `Stamp` the one HLC atom (`OpLog.Stamp`'s cell, never a second clock). Keys in, keys out —
// payload replay is the delegate owners' concern, and their typed conflict faults surface BEFORE any commit mints.
public sealed record RewriteSeam(
    Func<UInt128, Option<CommitNode>> Resolve,
    Func<CommitNode, IO<Seq<UInt128>>> Invert,
    Func<CommitNode, UInt128, IO<Seq<UInt128>>> Transplant,
    IO<Hlc> Stamp);

public static class CommitGraph {
    public const int Fanout = 16;

    public static CommitNode Commit(Seq<UInt128> parents, VersionVector inherited, Seq<UInt128> opKeys, BranchRef branch, Guid origin, string actor, Hlc cell, CommitMessage message) {
        Seq<UInt128> parentSet = toSeq(parents.Distinct().OrderBy(static k => k));
        Seq<UInt128> sortedKeys = toSeq(opKeys.OrderBy(static k => k));
        VersionVector vector = inherited.Advance(origin, opKeys.Count);
        ArrayBufferWriter<byte> canonical = new();
        Preimage(canonical, parentSet, sortedKeys, branch.Name, vector, actor, cell, message);
        // The vector advances the COMMITTING origin's slot — the writer's store id, never branch.Origin (the
        // ref's minting peer), so two writers on one branch occupy two causal slots and Order reads Concurrent.
        return new CommitNode(ContentHash.Of(canonical.WrittenSpan), parentSet, sortedKeys, branch.Name, vector, actor, cell, message);
    }

    // The ONE commit-key preimage writer (the parity `commit-key` slot mints through it, never a re-implemented
    // layout): count-framed parents and op keys, branch, lowercase-N GUID vector slots, actor, cell, and message.
    public static void Preimage(IBufferWriter<byte> sink, Seq<UInt128> sortedDistinctParents, Seq<UInt128> sortedOpKeys, string branch, VersionVector vector, string actor, Hlc cell, CommitMessage message) {
        BinaryPrimitives.WriteInt32LittleEndian(sink.GetSpan(4), sortedDistinctParents.Count);
        sink.Advance(4);
        foreach (UInt128 parent in sortedDistinctParents) { BinaryPrimitives.WriteUInt128LittleEndian(sink.GetSpan(16), parent); sink.Advance(16); }
        BinaryPrimitives.WriteInt32LittleEndian(sink.GetSpan(4), sortedOpKeys.Count);
        sink.Advance(4);
        foreach (UInt128 key in sortedOpKeys) { BinaryPrimitives.WriteUInt128LittleEndian(sink.GetSpan(16), key); sink.Advance(16); }
        Framed(sink, branch);
        Seq<(Guid Key, long Value)> slots = toSeq(vector.Slots.OrderBy(static slot => slot.Key.ToString("N"), StringComparer.Ordinal).Select(static slot => (slot.Key, slot.Value)));
        BinaryPrimitives.WriteInt32LittleEndian(sink.GetSpan(4), slots.Count);
        sink.Advance(4);
        foreach ((Guid key, long value) in slots) {
            Framed(sink, key.ToString("N"));
            BinaryPrimitives.WriteInt64LittleEndian(sink.GetSpan(8), value);
            sink.Advance(8);
        }
        Framed(sink, actor);
        cell.WriteTo(sink);
        Framed(sink, message.Summary);
        Framed(sink, message.Body);
    }

    static void Framed(IBufferWriter<byte> sink, string text) {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        BinaryPrimitives.WriteInt32LittleEndian(sink.GetSpan(4), bytes.Length);
        sink.Advance(4);
        sink.Write(bytes);
    }

    public static VectorOrder Order(VersionVector left, VersionVector right) =>
        (left.Slots.Equals(right.Slots), left.Dominates(right), right.Dominates(left)) switch {
            (true, _, _) => VectorOrder.Equal,
            (_, true, false) => VectorOrder.After,
            (_, false, true) => VectorOrder.Before,
            _ => VectorOrder.Concurrent,
        };

    // The branch-advance authorization demand — the ONE consumer of Order/VectorOrder: a merge commit demands
    // Grant.Merge, a head-dominating (fast-forward) or equal advance Grant.Write, a non-dominating reset
    // Grant.ForcePush; the caller gates `branch.Movable(actor, AdvanceDemand(commit, head.Vector))`, never a
    // caller-guessed grant lane (a history rewrite demands Grant.Rebase at its own entry).
    public static GrantSet AdvanceDemand(CommitNode commit, VersionVector head) {
        VectorOrder order = Order(commit.Vector, head);
        return commit.IsMerge ? GrantSet.Of(Grant.Merge)
            : order == VectorOrder.After || order == VectorOrder.Equal ? GrantSet.Of(Grant.Write)
            : GrantSet.Of(Grant.ForcePush);
    }

    // The rewrite demand — AdvanceDemand's sibling over the rewrite family: Revert/CherryPick are forward commits
    // (Grant.Write); Rebase is the history rewrite the Grant.Rebase row exists for. The caller gates
    // `branch.Movable(actor, RewriteDemand(rewrite))` BEFORE Rewrite runs.
    public static GrantSet RewriteDemand(HistoryRewrite rewrite) => rewrite.Switch(
        revert: static _ => GrantSet.Of(Grant.Write),
        cherryPick: static _ => GrantSet.Of(Grant.Write),
        rebase: static _ => GrantSet.Of(Grant.Rebase));

    // ONE polymorphic rewrite entry — the request case discriminates, never three sibling verbs. Every arm mints
    // NEW CommitNodes through the one Commit writer: Revert commits the target's inverse op keys onto `onto`;
    // CherryPick transplants one commit's ops onto `onto`; Rebase FoldM-threads the chain OLDEST-FIRST onto
    // NewBase, each transplant landing on the previously minted head so the result is a fresh linear lineage.
    // `head` is the vector at `onto`; an unresolvable key faults CommitFault.RewriteAbsent (8262) typed.
    public static IO<Seq<CommitNode>> Rewrite(HistoryRewrite rewrite, RewriteSeam seam, BranchRef branch, Guid origin, string actor, UInt128 onto, VersionVector head) =>
        rewrite.Switch(
            revert: r => Transplanted(seam, r.Target, onto, head, branch, origin, actor, static (s, node, _) => s.Invert(node), node => new CommitMessage($"revert {node.ContentKey:x32}", string.Empty)).Map(Seq),
            cherryPick: c => Transplanted(seam, c.Pick, onto, head, branch, origin, actor, static (s, node, target) => s.Transplant(node, target), static node => node.Message).Map(Seq),
            rebase: rb => rb.Chain.FoldM(
                (Onto: rb.NewBase, Vector: head, Minted: Seq<CommitNode>()),
                (acc, key) => Transplanted(seam, key, acc.Onto, acc.Vector, branch, origin, actor, static (s, node, target) => s.Transplant(node, target), static node => node.Message)
                    .Map(minted => (minted.ContentKey, minted.Vector, acc.Minted.Add(minted))))
                .Map(static final => final.Minted).As());

    static IO<CommitNode> Transplanted(
        RewriteSeam seam, UInt128 source, UInt128 onto, VersionVector head, BranchRef branch, Guid origin, string actor,
        Func<RewriteSeam, CommitNode, UInt128, IO<Seq<UInt128>>> keysOf, Func<CommitNode, CommitMessage> messageOf) =>
        seam.Resolve(source).Match(
            Some: node => from keys in keysOf(seam, node, onto)
                          from cell in seam.Stamp
                          select Commit(Seq(onto), head, keys, branch, origin, actor, cell, messageOf(node)),
            None: () => IO.fail<CommitNode>(new CommitFault.RewriteAbsent(source)));

    // Near-linear merge-base: two Rank passes (common set + nearest-first metric), then ONE Reach pass seeded
    // with every common candidate's parents — reached ∩ common IS the dominated set (a common node strictly
    // reachable from another common node via parent edges). The per-candidate Rank re-walk is the deleted
    // O(candidates × graph) form; Rasm.Bim MergeBase is the named consumer.
    public static Seq<UInt128> MergeBase(Func<UInt128, Option<CommitNode>> resolve, UInt128 left, UInt128 right) {
        HashMap<UInt128, int> leftRanked = Rank(resolve, left);
        HashMap<UInt128, int> rightRanked = Rank(resolve, right);
        Set<UInt128> common = toSet(toSeq(rightRanked.Keys).Filter(leftRanked.ContainsKey));
        Set<UInt128> dominated = Reach(resolve, toSeq(common).Bind(c => resolve(c).Map(static n => n.Parents).IfNone(Seq<UInt128>()))).Intersect(common);
        return toSeq(common.Filter(c => !dominated.Contains(c)).OrderBy(c => (leftRanked.Find(c).IfNone(0) + rightRanked.Find(c).IfNone(0), c)));
    }

    public static MerkleRange Of(Seq<UInt128> sortedKeys) {
        using XxHash128 digest = new();
        Span<byte> word = stackalloc byte[16];
        foreach (UInt128 key in sortedKeys) { BinaryPrimitives.WriteUInt128LittleEndian(word, key); digest.Append(word); }
        return new MerkleRange(sortedKeys.Head.IfNone(UInt128.Zero), sortedKeys.Last.IfNone(UInt128.Zero), digest.GetCurrentHashAsUInt128(), sortedKeys.Count);
    }

    public static Seq<MerkleRange> Reconcile(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote) =>
        local.Digest == remote.Digest ? Seq<MerkleRange>()
        : remote.Leaf ? Seq(remote)
        : children(remote).Bind(child => Sibling(children(local), child) is { IsSome: true, Case: MerkleRange peer } ? Reconcile(children, peer, child) : Seq(child));

    static Option<MerkleRange> Sibling(Seq<MerkleRange> locals, MerkleRange remote) => locals.Find(c => c.Low <= remote.High && remote.Low <= c.High);

    // The EXPRESSION_SPINE named-kernel exemption: a longest-path BFS over the commit DAG — the work-queue re-enqueues
    // a node on finding a deeper path so the rank is the MAX generation (the nearest-first merge-base ordering metric),
    // a memoized graph traversal a monadic fold cannot express without re-walking, so the mutable work-list is the kernel.
    static HashMap<UInt128, int> Rank(Func<UInt128, Option<CommitNode>> resolve, UInt128 root) {
        System.Collections.Generic.Dictionary<UInt128, int> depth = [];
        System.Collections.Generic.Queue<(UInt128 Key, int Generation)> queue = new([(root, 0)]);
        while (queue.TryDequeue(out (UInt128 Key, int Generation) step))
            if (!depth.TryGetValue(step.Key, out int seen) || step.Generation > seen) {
                depth[step.Key] = step.Generation;
                resolve(step.Key).Iter(node => node.Parents.Iter(parent => queue.Enqueue((parent, step.Generation + 1))));
            }
        return toHashMap(depth.Select(static kv => (kv.Key, kv.Value)));
    }

    // The ONE reverse-reachability generation-mark pass (EXPRESSION_SPINE named-kernel exemption — a
    // visited-set BFS work-list): every key reachable via one-or-more parent edges from the seed frontier,
    // O(V+E) once regardless of candidate count.
    static Set<UInt128> Reach(Func<UInt128, Option<CommitNode>> resolve, Seq<UInt128> frontier) {
        System.Collections.Generic.HashSet<UInt128> seen = [];
        System.Collections.Generic.Queue<UInt128> queue = new(frontier);
        while (queue.TryDequeue(out UInt128 key))
            if (seen.Add(key))
                resolve(key).Iter(node => node.Parents.Iter(queue.Enqueue));
        return toSet(seen);
    }
}
```

| [INDEX] | [POLICY]              | [VALUE]                                | [BINDING]                                                           |
| :-----: | :-------------------- | :------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | commit column family  | `commit`                               | one `OpLogEntry` per commit on the changefeed                       |
|  [02]   | merge-base resolution | maximal common-ancestor antichain      | near-linear: two `Rank` + ONE `Reach` pass; git multi-base          |
|  [03]   | branch-advance demand | `AdvanceDemand` off `(IsMerge, Order)` | `Movable` gates the derived `GrantSet`; never a caller-guessed lane |
|  [04]   | content-key preimage  | parents · op-keys · cell · message     | identity/preimage agree; re-word is a fresh node                    |
|  [05]   | branch grant          | `Element/authority#GRANT_ALGEBRA`      | `Movable` gates `Grant.Write`/`Merge`/`Rebase`/`ForcePush`          |
|  [06]   | domain commit ingest  | wire-carried content key               | `BimCommit` lands as one `CommitNode`; bases on `MergeBase`         |
|  [07]   | history rewrite       | `Rewrite` over `HistoryRewrite` cases  | append-only mints; `RewriteDemand` gates `Write`/`Rebase`           |

## [03]-[CRDT_ALGEBRA]

- Owner: `CrdtField` `[Union]` the convergent op-based/delta-state field family carrying the six replicated data types; `CrdtOp` the delta payload a changefeed entry carries; `RgaCell` the one growable-array element; `Crdt` the merge-fold surface whose `Merge` is commutative, associative, and idempotent over the op multiset, plus the version-vector-gated tombstone compaction.
- Cases: `LwwRegister`, `MvRegister`, `OrSet`, `PnCounter`, `RgaSequence`, `EphemeralMap` on `CrdtField`; `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain | Beat | Leave` on `CrdtOp`.
- Entry: `public static CrdtField Merge(CrdtField left, CrdtField right)` is the join-semilattice least-upper-bound, total over the six cases and idempotent; `public static CrdtField Apply(CrdtField state, CrdtOp op)` folds one delta carrying its HLC cell; `public static CrdtField Seed(CrdtOp op)` is the total generated `Switch` materializing a fresh cell's matching empty arm from its first op (the cell-type-stability genesis — a new op case breaks the build here); `public static CrdtField Compact(CrdtField state, VersionVector quiescent, Instant liveness)` reclaims `RgaSequence` tombstones the quiescence horizon proves unreferenceable and evicts `EphemeralMap` entries past the physical liveness deadline.
- Auto: a CRDT mutation rides one `OpLogEntry` carrying the `CrdtOp` delta as `Payload` so the convergent merge rides the changefeed projected off Marten, and a peer's `SyncMerge.Apply` dispatches the `column-family=crdt` row into `Crdt.Apply` rather than the LWW `Adjudicate` scalar; the OR-set merge is per-element tag-set union minus the union of observed-remove tombstones so add and concurrent remove resolve add-wins; the RGA element id IS the order key (the weave groups by causal predecessor, orders same-predecessor siblings descending by `ElementId`, depth-first-linearizes from the sentinel) so two concurrent inserts converge to one deterministic order on every peer; the PN-counter folds per-origin increments; the MV-register keeps every value its `VersionVector` context dominates none of; the `EphemeralMap` keeps one entry per origin where an incoming `Beat` supersedes only on a strictly-dominating `Hlc`, a `Leave` evicts add-wins-loses against a strictly-later `Beat`, and `Compact` evicts every entry whose last-beat physical instant precedes the liveness deadline — presence expiry reads the physical instant, never the op-count quiescence horizon.
- Receipt: a converged merge rides `SyncApplyReceipt`; a tombstone, live-element, compacted-tombstone, and live-presence count fold into the `store.crdt.merge` fact.
- Packages: NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox — no hasher: every CRDT identity is the `ElementId` order key or the wire owner's `ContentKey`, so a `System.IO.Hashing` row here is the stale admission the algebra never composes.
- Growth: a new replicated type is one `CrdtField` case plus one `CrdtOp` arm plus one `Merge`/`Apply` arm plus one `Seed` arm the generated total `Switch` forces; zero new surface — a per-type merge service, a second convergence engine, or an op-transform rebase is the deleted form because the join-semilattice subsumes idempotency, commutativity, and reorder tolerance.
- Boundary: `Merge` is a join-semilattice least-upper-bound so any partition of any permutation of the op multiset applied any number of times converges to identical state — the strict superset of the `Version/ledger#MERGE_LAW` LWW `Adjudicate`, which survives only as the `LwwRegister` arm; the `RgaSequence` carries tombstones so a deleted slot stays stable for later concurrent inserts and `Compact` reclaims only when the quiescence horizon dominates the cell's lamport stamp; the `OrSet` `Merge` is the per-element live-tag union minus the union of both tombstone sets through `Set.Except` (a `Set.Remove(Set)` is the rejected spelling); the `MvRegister` is a causal anti-chain keeping a value iff no other value's context strictly dominates it; the `PnCounter` is one per-origin map of `(Sequence, Positive, Negative)` running totals monotone under sequence-max merge, so a replayed or reordered `Increment` is absorbed idempotently (a delta-adding fold is the deleted non-idempotent form); the RGA element id is `(Guid Origin, ulong Logical)` and IS the convergent order key, never positional, so the `RgaCell` carries no redundant `Hlc`; the `EphemeralMap` is per-origin-LWW-by-HLC under add-wins liveness — `Compact` is the durable-presence distinction (an entry whose last-beat physical instant precedes the liveness deadline is a peer that stopped beating, so eviction is convergence-correct and idempotent), presence liveness a physical-time horizon distinct from the RGA op-count tombstone-GC horizon, and the `Maintain` op carries BOTH (`Quiescent` for sequence GC, `Liveness` for presence expiry); `Crdt.Merge` reads no wall clock — the `Hlc` cell from the Marten event stamp is the only ordering input, so convergence is deterministic; the `(left, right)` and `(state, op)` tuple `switch`es are total on the diagonal (one arm per CRDT type) and the off-diagonal `_` arm is unreachable by the cell-type-stability invariant — a `(NodeId, Field)` cell is one fixed `CrdtField` arm for its whole lifetime (`Crdt.Seed` — the total generated `CrdtOp.Switch` — materializes a fresh cell's matching empty arm from its FIRST op, and a decoded op whose type disagrees with its cell is contract drift the `CrdtWire.Decode` `CommitFault.DecodeDrift` rail already rejects before this fold), so a cross-type merge is structurally impossible rather than a silent identity-return, and the `_ => left`/`_ => state` arm is the unreachable totality floor, never a soft fallback that hides type drift.

```csharp signature
public readonly record struct ElementId(Guid Origin, ulong Logical) : IComparable<ElementId> {
    public static readonly ElementId Head = new(Guid.Empty, 0UL);
    public int CompareTo(ElementId other) {
        int byLogical = Logical.CompareTo(other.Logical);
        return byLogical != 0 ? byLogical : Origin.CompareTo(other.Origin);
    }
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
    // Per-origin RUNNING TOTALS, not a bare delta: Sequence is the origin's monotone op counter and
    // Positive/Negative its cumulative sums, so Apply is a max-merge — a replayed or reordered Increment
    // converges identically (the idempotent join-semilattice law a delta-adding fold cannot satisfy).
    public sealed record Increment(string Field, Guid Origin, long Sequence, long Positive, long Negative) : CrdtOp;
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
    public sealed record PnCounter(HashMap<Guid, (long Sequence, long Positive, long Negative)> Origins) : CrdtField;
    public sealed record RgaSequence(Seq<RgaCell> Cells) : CrdtField;
    public sealed record EphemeralMap(HashMap<Guid, (ReadOnlyMemory<byte> State, Hlc Cell)> Live) : CrdtField;
}

public static class Crdt {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.crdt.merge"), StoreSlot.Create("store.crdt.decode"));

    // The cell-type-stability genesis: a fresh (NodeId, Field) cell materializes its CrdtField arm from its
    // FIRST op through the generated total Switch — a new op case breaks the build here, and every later op
    // for that cell hits the fixed diagonal arm in Apply.
    public static CrdtField Seed(CrdtOp op) => op.Switch<CrdtField>(
        set: static _ => new CrdtField.LwwRegister(ReadOnlyMemory<byte>.Empty, Hlc.Zero, Guid.Empty),
        write: static _ => new CrdtField.MvRegister(Seq<(ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell)>()),
        add: static _ => new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), Set<ElementId>()),
        remove: static _ => new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), Set<ElementId>()),
        increment: static _ => new CrdtField.PnCounter(HashMap<Guid, (long Sequence, long Positive, long Negative)>()),
        insertAfter: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
        delete: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
        maintain: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
        beat: static _ => new CrdtField.EphemeralMap(HashMap<Guid, (ReadOnlyMemory<byte> State, Hlc Cell)>()),
        leave: static _ => new CrdtField.EphemeralMap(HashMap<Guid, (ReadOnlyMemory<byte> State, Hlc Cell)>()));

    public static CrdtField Merge(CrdtField left, CrdtField right) => (left, right) switch {
        (CrdtField.LwwRegister l, CrdtField.LwwRegister r) => (r.Cell, r.Origin).CompareTo((l.Cell, l.Origin)) > 0 ? r : l,
        (CrdtField.MvRegister l, CrdtField.MvRegister r) => new CrdtField.MvRegister(AntiChain(l.Values + r.Values)),
        (CrdtField.OrSet l, CrdtField.OrSet r) when l.Tombstoned.Union(r.Tombstoned) is var graves =>
            new CrdtField.OrSet(r.Live.Fold(l.Live, static (acc, s) => acc.AddOrUpdate(s.Key, e => e.Union(s.Value), s.Value)).Map((_, tags) => tags.Except(graves)).Filter(static t => t.Count > 0), graves),
        (CrdtField.PnCounter l, CrdtField.PnCounter r) => new CrdtField.PnCounter(r.Origins.Fold(l.Origins, static (acc, s) => acc.AddOrUpdate(s.Key, held => held.Sequence >= s.Value.Sequence ? held : s.Value, s.Value))),
        (CrdtField.RgaSequence l, CrdtField.RgaSequence r) => new CrdtField.RgaSequence(Weave(l.Cells, r.Cells)),
        (CrdtField.EphemeralMap l, CrdtField.EphemeralMap r) => new CrdtField.EphemeralMap(r.Live.Fold(l.Live, static (acc, s) => acc.AddOrUpdate(s.Key, held => held.Cell.CompareTo(s.Value.Cell) >= 0 ? held : s.Value, s.Value))),
        _ => left,
    };

    public static CrdtField Apply(CrdtField state, CrdtOp op) => (state, op) switch {
        (CrdtField.LwwRegister reg, CrdtOp.Set set) => (set.Cell, set.Origin).CompareTo((reg.Cell, reg.Origin)) > 0 ? new CrdtField.LwwRegister(set.Value, set.Cell, set.Origin) : reg,
        (CrdtField.MvRegister mv, CrdtOp.Write w) => new CrdtField.MvRegister(AntiChain(mv.Values.Filter(h => !w.Context.Dominates(h.Context)).Add((w.Value, w.Context, w.Cell)))),
        (CrdtField.OrSet s, CrdtOp.Add add) => new CrdtField.OrSet(s.Live.AddOrUpdate(add.Element, e => e.Add(add.Tag), Set(add.Tag)), s.Tombstoned),
        (CrdtField.OrSet s, CrdtOp.Remove rem) when toSet(rem.ObservedTags) is var observed => new CrdtField.OrSet(s.Live.AddOrUpdate(rem.Element, e => e.Except(observed), Set<ElementId>()).Filter(static t => t.Count > 0), s.Tombstoned.Union(observed)),
        (CrdtField.PnCounter c, CrdtOp.Increment inc) => new CrdtField.PnCounter(
            c.Origins.AddOrUpdate(inc.Origin, held => held.Sequence >= inc.Sequence ? held : (inc.Sequence, inc.Positive, inc.Negative), (inc.Sequence, inc.Positive, inc.Negative))),
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

    public static long Value(CrdtField.PnCounter counter) => counter.Origins.Values.Sum(static origin => origin.Positive - origin.Negative);
    public static Seq<ReadOnlyMemory<byte>> Materialize(CrdtField.RgaSequence seq) => seq.Cells.Filter(static c => !c.Tombstone).Map(static c => c.Value);
    public static Seq<(Guid Origin, ReadOnlyMemory<byte> State)> Live(CrdtField.EphemeralMap map) => toSeq(map.Live.Map(static (o, s) => (o, s.State)));

    static Seq<(ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell)> AntiChain(Seq<(ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell)> values) =>
        toSeq(values.Distinct()).Filter(c => !values.Exists(o => !o.Context.Equals(c.Context) && o.Context.Dominates(c.Context)));

    static Seq<RgaCell> Weave(Seq<RgaCell> left, Seq<RgaCell> right) {
        IEnumerable<RgaCell> merged = (left + right).GroupBy(static c => c.Id).Select(static g => g.Aggregate(static (a, b) => a with { Tombstone = a.Tombstone || b.Tombstone }));
        HashMap<ElementId, Seq<RgaCell>> children = toHashMap(merged.GroupBy(static c => c.After).Select(static g => (g.Key, toSeq(g.OrderByDescending(static c => c.Id)))));
        return Linearize(children, RgaCell.Origin, Seq<RgaCell>());
    }

    static Seq<RgaCell> Linearize(HashMap<ElementId, Seq<RgaCell>> children, ElementId after, Seq<RgaCell> woven) =>
        children.Find(after).IfNone(Seq<RgaCell>()).Fold(woven, (acc, cell) => Linearize(children, cell.Id, acc.Add(cell)));
}
```

| [INDEX] | [TYPE]       | [CRDT_CLASS]                          | [CONVERGENCE]                                             |
| :-----: | :----------- | :------------------------------------ | :-------------------------------------------------------- |
|  [01]   | LwwRegister  | last-write-wins by (HLC, origin)      | total order on the stamp tuple; superset of `Adjudicate`  |
|  [02]   | MvRegister   | multi-value concurrent-keep           | causal anti-chain; dominated writes collapse              |
|  [03]   | OrSet        | add-wins observed-remove set          | per-element tag-set union minus observed removes          |
|  [04]   | PnCounter    | per-origin running totals + sequence  | per-origin sequence-max of monotone totals                |
|  [05]   | RgaSequence  | replicated growable array             | predecessor-keyed weave; `Compact` reclaims at quiescence |
|  [06]   | EphemeralMap | add-wins observed-remove presence map | per-origin LWW-by-HLC; `Compact` self-expires at liveness |

## [04]-[CRDT_WIRE]

- Owner: `Hlc` the hybrid-logical-clock stamp the Marten event `Timestamp`, the changefeed projection, the CRDT merge, the commit cell, and the wire all read; `CrdtOpWire` the `[MessagePack.Union]` op encoding the `OpLogEntry.Payload` carries for `column-family=crdt` rows; `CommitFault` the closed `[Union]` fault family deriving from the KERNEL `Rasm.Domain.Expected` in the 8260 band (`DecodeDrift` 8261 · `RewriteAbsent` 8262 · `ParityDrift` 8263 · `OwnerMinted` 8264 — the exact sibling template `SyncFault` 8250 and `RecoveryFault` 8290 realize; a bare `Error.New` integer is the deleted form); `CrdtWire` the static codec owning the byte-canonical content key, the `Encode`/`Decode` pair through the package `PersistenceResolver`, and the `UntrustedData` restore-lane decode; `ParitySlot` the `[SmartEnum<string>]` corpus-leg axis carrying its producer-owner label; `ParityVector` the one fixture carrier — canonical bytes plus the digest ALWAYS derived through the kernel `ContentHash.Of` at mint (never an unstamped `Option`); fixture FREEZE status is the kernel registry's (`Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` rows [04]/[08] stay DESIGN-PIN until the one-time harness proof pins concrete inputs and golden bytes — this producer derives vectors, the registry alone declares them frozen); `ContentParityCorpus` the static surface minting the Persistence leg of the `ONE_WIRE_FIXTURE_CORPUS` and reconciling a local corpus against the golden one.
- Cases: 10 op rows — `set | write | add | remove | increment | insertAfter | delete | maintain | beat | leave`; the `[Key]` sequence IS the wire schema, dense and append-only, a retired key never reassigned; the `beat`/`leave` arms carry the `EphemeralMap` presence delta; the 4 parity slots — `hlc-cell | commit-key | crdt-op | elementset` — name their producer owner, the first three minted from this owner's own writers and the fourth flowing in one-directionally through `ContentParityCorpus.Contribute(ParitySlot.ElementSet, set.Preimage)` — the `Query/lane#ELEMENT_SET_ALGEBRA` owner calls it, handing the `ElementSet.Preimage` distinct-sorted length-framed `NodeId`-packed bytes (the same preimage `ElementSetAlgebra.Receipt` hashes), so the Version owner freezes the foreign byte shape but never reaches back into Query to re-derive it.
- Entry: `public static UInt128 ContentKey(CrdtOp op)` is the byte-canonical content key over the `None`-compression companion encoding; `public static ReadOnlyMemory<byte> Encode(CrdtOp op)` writes the durable delta under `Lz4BlockArray`; `public static ReadOnlyMemory<byte> EncodeCompanion(CrdtOp op)` writes the same delta under `None` for the Python/TS consumers; `public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload)` reads under `UntrustedData` with the depth and decompressed-size ceilings, a contract rejection failing the typed `CommitFault.DecodeDrift` (8261). `public static HashMap<ParitySlot, ParityVector> Mint(Hlc cell, Seq<UInt128> parents, Seq<UInt128> opKeys, string branch, VersionVector vector, string actor, CommitMessage message, CrdtOp op, params ReadOnlySpan<ParityVector> contributed)` mints the three owner-local parity vectors over this page's own writers (the commit-key leg framing the FULL preimage — branch, vector, actor, message — through the one `CommitGraph.Preimage`) and folds in the `Contribute`d foreign vectors; `public static Fin<ParityVector> Contribute(ParitySlot slot, ReadOnlyMemory<byte> canonical)` is the contribution seam a foreign producer (the `elementset` owner) calls, failing `CommitFault.OwnerMinted` (8264) on an owner-minted slot so the Version owner never re-derives a Query byte shape; `public static Validation<Error, Unit> Reconcile(HashMap<ParitySlot, ParityVector> local, HashMap<ParitySlot, ParityVector> golden)` accumulates every `CommitFault.ParityDrift` (8263) the cross-runtime harness finds against the golden corpus.
- Auto: `Hlc.Observe` swaps the local cell forward past both the wall clock and the observed remote cell so a received op never rewinds the local logical counter; `CrdtWire.Encode` rides the codec profile so a `CrdtOp` delta crosses as `OpLogEntry.Payload` bytes the snapshot codec already verifies; the wire union and the `CrdtOp` union share one case vocabulary so a new op arm is one wire row plus one `CrdtOp` arm plus one map case; `ContentParityCorpus.Mint` seals each owner-local fixture from the SAME writer the live path runs — the HLC cell from `Hlc.WriteTo`, the commit-key preimage from the ONE `CommitGraph.Preimage` writer (never a re-implemented layout), the CRDT-op companion from `CrdtWire.EncodeCompanion` — so a parity fixture is byte-identical to what the live encode produces and every `ParityVector.Of` mint derives its digest through the kernel `ContentHash.Of` (the one seed-zero discipline; a corpus-local seed constant is the deleted form).
- Receipt: an encoded delta carries no receipt (the `OpLogEntry` carries the lane codec, content key, and HLC cell); a decode failure folds into `store.crdt.decode` as the typed `CommitFault.DecodeDrift`; a parity drift folds into the `Reconcile` `Validation` as the accumulated `CommitFault.ParityDrift` cross-runtime mismatch set, never a first-mismatch abort.
- Packages: MessagePack, Thinktecture.Runtime.Extensions.MessagePack, Rasm (`Rasm.Domain` `ContentHash.Of` + `Rasm.Domain.Expected` — the fault-band base), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new op is one `CrdtOpWire` `[MessagePack.Union]` tag plus one `[Key]` member plus one `Map`/`Lift` arm; `Lift` over the owned `CrdtOp` `[Union]` is the generated total `Switch` so a new case breaks the build, while `Map` over the foreign wire union stays a language `switch` whose `_ => throw` is the contract-drift guard; a new parity leg is one `ParitySlot` row carrying its producer label plus one `Mint` or `Contribute` vector, never a second corpus store or a per-fixture golden-bytes constant family; zero new surface.
- Boundary: this is the flagship `CrdtOpWire` amendment to the one-wire-vocabulary law — `OpLogEntry.Payload` carries a `CrdtOpWire` union for `column-family=crdt` rows, LWW `Adjudicate` survives only as the `set` arm reconstructing `LwwRegister`, and the breaking descriptor change is owned at `AppHost/runtime-ports#WIRE_LAW` with the TS-web and Python companions decoding the amended payload; the `Hlc` is one packed `(Instant Physical, ulong Logical)` whose ordering is `Physical` then `Logical` so two peers compare causality without a wall clock and `WriteTo` emits the canonical 16-byte cell the commit content key and the op content key both hash; the wire `[Key]` sequence obeys the retirement law so contract drift is a build diagnostic through `MessagePackAnalyzer`; the restore lane reads under `UntrustedData` plus the object-graph depth ceiling AND a bounded decompressed-size cap because a synced delta admits a decompression bomb the depth cap alone never catches, the contract rejection surfacing as the typed `CommitFault.DecodeDrift` on the `Fin` rail (the `CommitFault` band derives from the KERNEL `Rasm.Domain.Expected` exactly as `SyncFault`/`RecoveryFault` do — parameterless `: base()`, per-case `Code`/`Message`/`Category` `Switch`, no `[GenerateUnionOps]` — so a recovery reads `error.HasCode(8261)`/`error.IsType<CommitFault.ParityDrift>()`, never a message substring); `ContentKey` hashes the `None`-companion canonical bytes (never the LZ4 at-rest framing) through the kernel `ContentHash.Of` so the op content key is byte-reproducible across the C#, Python, and TS runtimes and is the same seed-zero identity the structural diff and the federation keys consume; `ContentParityCorpus` freezes the `Hlc.WriteTo` 16-byte cell, the canonical commit-key preimage (the one `CommitGraph.Preimage` writer, never a re-implemented layout), and the `CrdtOpWire` `None`-companion encoding, each as one `ParityVector` whose digest `ParityVector.Of` ALWAYS derives through `ContentHash.Of` at mint — the fixture set lands byte-identical into the kernel golden corpus once the registry pins it (`Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` rows [04]/[08], DESIGN-PIN until the one-time harness proof freezes inputs and golden bytes; the registry is the ONE status authority), the C# shared-corpus harness then asserts this producer emits its fixture byte-for-byte, and `python:runtime/evidence/identity` + `typescript:core/value/contentKey` read the frozen corpus, never a Version-local pin; the `elementset` leg is the `Query/lane#ELEMENT_SET_ALGEBRA` `ElementSetAlgebra.Receipt` distinct-sorted `NodeId`-packed preimage `Contribute`d by THAT owner, and `Contribute` refuses an owner-minted slot (`ParitySlot.MintedHere`) with `CommitFault.OwnerMinted` so the Version owner accepts the foreign byte shape but never reaches back into Query to re-derive it — the dependency stays one-directional; `Reconcile` is the cross-runtime gate the C#-host golden corpus and the Python/TS replicas both fold a local corpus against, accumulating every `CommitFault.ParityDrift` through `Validation` rather than aborting on the first.

```csharp signature
public readonly record struct Hlc(Instant Physical, ulong Logical) : IComparable<Hlc> {
    public static readonly Hlc Zero = new(Instant.MinValue, 0UL);
    public int CompareTo(Hlc other) {
        int byPhysical = Physical.CompareTo(other.Physical);
        return byPhysical != 0 ? byPhysical : Logical.CompareTo(other.Logical);
    }
    public Hlc Advance(Instant wall) => wall > Physical ? new Hlc(wall, 0UL) : new Hlc(Physical, Logical + 1UL);
    public Hlc Observe(Hlc remote, Instant wall) {
        Instant lead = Instant.Max(Instant.Max(Physical, remote.Physical), wall);
        return new Hlc(lead, (lead == Physical, lead == remote.Physical) switch {
            (true, true) => ulong.Max(Logical, remote.Logical) + 1UL,
            (true, false) => Logical + 1UL,
            (false, true) => remote.Logical + 1UL,
            _ => 0UL,
        });
    }
    public void WriteTo(IBufferWriter<byte> sink) {
        Span<byte> span = sink.GetSpan(16);
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
    [MessagePackObject] public sealed record Increment([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] long Sequence, [property: Key(3)] long Positive, [property: Key(4)] long Negative) : CrdtOpWire;
    [MessagePackObject] public sealed record InsertAfter([property: Key(0)] string Field, [property: Key(1)] Guid PredOrigin, [property: Key(2)] ulong PredLogical, [property: Key(3)] Guid IdOrigin, [property: Key(4)] ulong IdLogical, [property: Key(5)] ReadOnlyMemory<byte> Value) : CrdtOpWire;
    [MessagePackObject] public sealed record Delete([property: Key(0)] string Field, [property: Key(1)] Guid IdOrigin, [property: Key(2)] ulong IdLogical) : CrdtOpWire;
    [MessagePackObject] public sealed record Maintain([property: Key(0)] string Field, [property: Key(1)] (Guid Origin, long Seq)[] Quiescent, [property: Key(2)] long LivenessTicks) : CrdtOpWire;
    [MessagePackObject] public sealed record Beat([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] ReadOnlyMemory<byte> State, [property: Key(3)] long PhysicalTicks, [property: Key(4)] ulong Logical) : CrdtOpWire;
    [MessagePackObject] public sealed record Leave([property: Key(0)] string Field, [property: Key(1)] Guid Origin, [property: Key(2)] long PhysicalTicks, [property: Key(3)] ulong Logical) : CrdtOpWire;
}

// --- [ERRORS] --------------------------------------------------------------------------

// The commit/wire fault band (8260): a closed [Union] over the KERNEL `Rasm.Domain.Expected` — the SAME
// parameterless-base-ctor + per-case Code/Message/Category Switch template SyncFault (8250) and RecoveryFault
// (8290) realize; no [GenerateUnionOps]. The Expected derivation makes a bare case an Error directly so it
// lifts onto Fin<T>/Validation with no .ToError() hop; band membership derives `Code => FaultBand.Commit + n`
// through the registry row (`Element/graph#FAULT_TABLES`) — a bare Error.New integer OR a bare literal in the
// Switch is the deleted form.
[Union]
public abstract partial record CommitFault : Rasm.Domain.Expected, IValidationError<CommitFault> {
    private CommitFault() : base() { }
    public sealed record DecodeDrift(string Cause) : CommitFault;
    public sealed record RewriteAbsent(UInt128 Source) : CommitFault;
    public sealed record ParityDrift(string Slot, string Producer) : CommitFault;
    public sealed record OwnerMinted(string Slot) : CommitFault;

    public override int Code => FaultBand.Commit + Switch(
        decodeDrift:   static _ => 1,
        rewriteAbsent: static _ => 2,
        parityDrift:   static _ => 3,
        ownerMinted:   static _ => 4);

    public override string Message => Switch(
        decodeDrift:   static c => $"<crdt-decode-drift:{c.Cause}>",
        rewriteAbsent: static c => $"<rewrite-source-absent:{c.Source:x32}>",
        parityDrift:   static c => $"<parity-drift:{c.Slot}@{c.Producer}>",
        ownerMinted:   static c => $"<parity-owner-mints:{c.Slot}>");

    public override string Category => Switch(
        decodeDrift:   static _ => "Wire",
        rewriteAbsent: static _ => "Rewrite",
        parityDrift:   static _ => "Parity",
        ownerMinted:   static _ => "Parity");

    public static CommitFault Create(string message) => new DecodeDrift(message);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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
        increment: static i => new CrdtOpWire.Increment(i.Field, i.Origin, i.Sequence, i.Positive, i.Negative),
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
        CrdtOpWire.Increment i => new CrdtOp.Increment(i.Field, i.Origin, i.Sequence, i.Positive, i.Negative),
        CrdtOpWire.InsertAfter ins => new CrdtOp.InsertAfter(ins.Field, new ElementId(ins.PredOrigin, ins.PredLogical), new ElementId(ins.IdOrigin, ins.IdLogical), ins.Value),
        CrdtOpWire.Delete d => new CrdtOp.Delete(d.Field, new ElementId(d.IdOrigin, d.IdLogical)),
        CrdtOpWire.Maintain m => new CrdtOp.Maintain(m.Field, new VersionVector(toHashMap(m.Quiescent)), Instant.FromUnixTimeTicks(m.LivenessTicks)),
        CrdtOpWire.Beat b => new CrdtOp.Beat(b.Field, b.Origin, b.State, new Hlc(Instant.FromUnixTimeTicks(b.PhysicalTicks), b.Logical)),
        CrdtOpWire.Leave l => new CrdtOp.Leave(l.Field, l.Origin, new Hlc(Instant.FromUnixTimeTicks(l.PhysicalTicks), l.Logical)),
        _ => throw new MessagePack.MessagePackSerializationException("<crdt-wire-unmapped>"),
    };

    public static ReadOnlyMemory<byte> Encode(CrdtOp op) => MessagePackSerializer.Serialize(Lift(op), Write);
    public static ReadOnlyMemory<byte> EncodeCompanion(CrdtOp op) => MessagePackSerializer.Serialize(Lift(op), Companion);
    public static UInt128 ContentKey(CrdtOp op) => ContentHash.Of(EncodeCompanion(op).Span);

    public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload) =>
        Try.lift(() => Map(MessagePackSerializer.Deserialize<CrdtOpWire>(payload, Restore))).Run()
            .MapFail(static error => new CommitFault.DecodeDrift(error.Message));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParitySlot {
    public static readonly ParitySlot HlcCell = new("hlc-cell", "csharp:Version/commits#CRDT_WIRE", mintedHere: true);
    public static readonly ParitySlot CommitKey = new("commit-key", "csharp:Version/commits#COMMIT_DAG", mintedHere: true);
    public static readonly ParitySlot CrdtOp = new("crdt-op", "csharp:Version/commits#CRDT_WIRE", mintedHere: true);
    public static readonly ParitySlot CrdtOpSet = new("crdt-op-set", "csharp:Version/commits#CRDT_ALGEBRA", mintedHere: true);
    public static readonly ParitySlot ElementSet = new("elementset", "csharp:Query/lane#ELEMENT_SET_ALGEBRA", mintedHere: false);
    public string Producer { get; }
    public bool MintedHere { get; }
    private ParitySlot(string key, string producer, bool mintedHere) : this(key) => (Producer, MintedHere) = (producer, mintedHere);
}

// The parity fixture carrier: canonical bytes plus the digest ALWAYS derived through the kernel
// `ContentHash.Of` at mint; the kernel ONE_WIRE_FIXTURE_CORPUS registry alone declares a fixture frozen
// (rows [04]/[08] stay DESIGN-PIN until the harness proof) — an unstamped-Option carrier and a corpus-local
// seed constant are the deleted forms.
public readonly record struct ParityVector(ParitySlot Slot, ReadOnlyMemory<byte> Canonical, UInt128 Digest) {
    public static ParityVector Of(ParitySlot slot, ReadOnlyMemory<byte> canonical) => new(slot, canonical, ContentHash.Of(canonical.Span));
    public bool Holds(ParityVector pinned) => Slot == pinned.Slot && Digest == pinned.Digest;
}

public static class ContentParityCorpus {
    public static ParityVector Cell(Hlc cell) {
        ArrayBufferWriter<byte> buffer = new(16);
        cell.WriteTo(buffer);
        return ParityVector.Of(ParitySlot.HlcCell, buffer.WrittenMemory.ToArray());
    }

    public static ParityVector CommitPreimage(Seq<UInt128> parents, Seq<UInt128> opKeys, string branch, VersionVector vector, string actor, Hlc cell, CommitMessage message) {
        ArrayBufferWriter<byte> buffer = new();
        CommitGraph.Preimage(buffer, toSeq(parents.Distinct().OrderBy(static k => k)), toSeq(opKeys.OrderBy(static k => k)), branch, vector, actor, cell, message);
        return ParityVector.Of(ParitySlot.CommitKey, buffer.WrittenMemory.ToArray());
    }

    public static ParityVector Op(CrdtOp op) => ParityVector.Of(ParitySlot.CrdtOp, CrdtWire.EncodeCompanion(op).ToArray());

    // CRDT_OP_SET producer (kernel corpus row [04]): EVERY delivery permutation of the op set folds to one
    // converged state, and the vector's canonical bytes are the converged MvRegister anti-chain in Hlc-cell
    // order — a permutation-dependent fold refutes the algebra and fails the mint instead of pinning a lie.
    public static Fin<ParityVector> OpSet(Seq<CrdtOp> ops) {
        if (ops.IsEmpty) { return Fin.Fail<ParityVector>(new CommitFault.ParityDrift(ParitySlot.CrdtOpSet.Key, "<empty-op-set>")); }
        Seq<byte[]> folds = Permutations(ops).Map(order => Canonical(order.Fold(Crdt.Seed(ops[0]), Crdt.Apply)));
        return folds.Map(static bytes => ContentHash.Of(bytes)).Distinct().Count() == 1
            ? Fin.Succ(ParityVector.Of(ParitySlot.CrdtOpSet, folds[0]))
            : Fin.Fail<ParityVector>(new CommitFault.ParityDrift(ParitySlot.CrdtOpSet.Key, "<divergent-delivery-fold>"));
    }

    static Seq<Seq<CrdtOp>> Permutations(Seq<CrdtOp> ops) =>
        ops.Count <= 1
            ? Seq(ops)
            : toSeq(Enumerable.Range(0, ops.Count)).Bind(pick => Permutations(ops.RemoveAt(pick)).Map(rest => ops[pick].Cons(rest)));

    static byte[] Canonical(CrdtField state) {
        ArrayBufferWriter<byte> buffer = new();
        if (state is CrdtField.MvRegister mv) {
            foreach ((ReadOnlyMemory<byte> value, VersionVector _, Hlc cell) in mv.Values.OrderBy(static held => held.Cell).ToSeq()) {
                cell.WriteTo(buffer);
                buffer.Write(value.Span);
            }
        }
        return buffer.WrittenSpan.ToArray();
    }

    public static Fin<ParityVector> Contribute(ParitySlot slot, ReadOnlyMemory<byte> canonical) =>
        slot.MintedHere
            ? Fin.Fail<ParityVector>(new CommitFault.OwnerMinted(slot.Key))
            : Fin.Succ(ParityVector.Of(slot, canonical));

    public static HashMap<ParitySlot, ParityVector> Mint(Hlc cell, Seq<UInt128> parents, Seq<UInt128> opKeys, string branch, VersionVector vector, string actor, CommitMessage message, CrdtOp op, params ReadOnlySpan<ParityVector> contributed) =>
        LanguageExt.Iterable<ParityVector>.FromSpan(contributed).Fold(
            HashMap((ParitySlot.HlcCell, Cell(cell)), (ParitySlot.CommitKey, CommitPreimage(parents, opKeys, branch, vector, actor, cell, message)), (ParitySlot.CrdtOp, Op(op))),
            static (corpus, vector) => corpus.AddOrUpdate(vector.Slot, vector));

    public static Validation<Error, Unit> Reconcile(HashMap<ParitySlot, ParityVector> local, HashMap<ParitySlot, ParityVector> golden) =>
        toSeq(golden).Traverse(slot => local.Find(slot.Key) is { IsSome: true, Case: ParityVector held } && held.Holds(slot.Value)
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new CommitFault.ParityDrift(slot.Key.Key, slot.Value.Slot.Producer))).As().Map(static _ => unit);
}
```

| [INDEX] | [POLICY]         | [VALUE]                                | [BINDING]                                            |
| :-----: | :--------------- | :------------------------------------- | :--------------------------------------------------- |
|  [01]   | HLC stamp source | Marten event `Timestamp` cell          | one `Hlc` for op-log, CRDT merge, commit cell, wire  |
|  [02]   | wire schema      | `[Key]` sequence, append-only          | retired key never reassigned; analyzer gate          |
|  [03]   | content key      | `None`-companion canonical bytes       | byte-reproducible across C#/Python/TS; no at-rest LZ4 |
|  [04]   | restore guard    | `UntrustedData` + depth + size ceiling | decompression bomb stops beyond the depth cap        |
|  [05]   | parity corpus    | kernel `ContentHash.Of` at every mint  | `VERSION_PARITY`; producer-emits gate                |
|  [06]   | contribution    | `Contribute` refuses `MintedHere`     | Query supplies `elementset`; no reverse derivation   |
|  [07]   | fault band      | `CommitFault : Expected` 8260         | closed codes 8261-8264                               |
