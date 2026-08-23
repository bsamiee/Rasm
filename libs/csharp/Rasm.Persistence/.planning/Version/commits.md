# [PERSISTENCE_VERSION_COMMITS]

`CommitGraph` owns content-addressed history, ref policy, vector order, merge bases, anti-entropy ranges, and append-only rewrites. `Crdt` owns the convergent field algebra; `CrdtWire` projects it through the generated `rasm.contracts.crdt.v1.CrdtOpWire` oneof; `Hlc` supplies their shared causal cell. `ContentParityCorpus` derives every local fixture from the live writer and accepts foreign fixtures only through `Contribute`. Marten supplies the append substrate, `OpLogEntry` supplies the changefeed's positional message envelope, `GrantSet` supplies branch authorization, and `ContentHash.Of` supplies payload-byte integrity without replacing operation identity.

## [01]-[INDEX]

- [02]-[COMMIT_DAG]: content-addressed commit-DAG with commit messages, named branches, annotated tags, maximal-antichain merge-base, and version vectors.
- [03]-[CRDT_ALGEBRA]: RGA, OR-set, MV-register, PN-counter, LWW, and ephemeral-presence convergent CRDT.
- [04]-[CRDT_WIRE]: HLC stamp, generated `crdt.v1` op payload, positional op-log embedding, and the cross-runtime parity corpus.

## [02]-[COMMIT_DAG]

- Owner: `CommitNode` the content-addressed commit record carrying its `CommitMessage`; `RefCapability`/`RefPolicy` the ref-class vocabulary and its three legal corners; `RefKind` the ref-class axis; `BranchRef` the named-ref pointer with a per-branch `Element/authority#GRANT_ALGEBRA` `GrantSet` ACL (the branch-lane narrowing of the one object-authorization vocabulary, never the disjoint AppHost `Capability`), upstream tracking, and annotated-tag payload; `VersionVector` the per-origin sequence map owning the ONE canonical slot order and the ONE `CanonicalBytes` field stream every byte-deriving reader takes; `MerkleRange` the reconciliation node; `HistoryRewrite` the append-only rewrite request family with `RewriteSeam` its delegate frame; `CommitGraph` the static surface owning hash, parent links, the maximal-antichain merge base, vector compare, the Merkle range fold, the recursive anti-entropy descent, and the one polymorphic `Rewrite` entry.
- Cases: `RefKind` is `Branch | LightweightTag | AnnotatedTag | RemoteTracking`, each holding its `RefCapability` corner — `{Mutable}`, `{}`, `{Annotated}` — so an annotated-yet-mutable ref is unrepresentable rather than merely unwritten; `CommitGraph.Order` compares two `VersionVector` values into `Before | After | Concurrent | Equal`; `MerkleRange` is `Empty | Bounded`, so an empty digest carries no fabricated low/high key, and `CommitGraph.Reconcile` recursively bisects only divergent bounded subranges; `HistoryRewrite` closes at `Revert | CherryPick | Rebase`, every case an append-only mint through the one `Commit` writer.
- Entry: `CommitGraph.Commit(parents, inherited, opKeys, branch, origin, actor, cell, message)` is a pure value whose content key is the kernel `ContentHash.Of` over the `CommitGraph.Fields` stream — `(SortedDistinctParents, SortedOpKeys, Branch, VersionVector, Actor, Hlc, CommitMessage)` named in order on the kernel `CanonicalWriter`, count-framed and length-framed by the writer alone — the vector advanced on the COMMITTING origin's slot — the writer's store id off the session, never `branch.Origin`, which names the ref's minting peer and collapses every writer on one branch into one causal slot; `MergeBase(resolve, left, right)` returns the maximal common-ancestor antichain nearest-first; `AdvanceDemand(commit, head)` derives the branch-advance authorization as a total `VectorOrder` dispatch; `Reconcile(children, local, remote)` returns the divergent leaf ranges; `Rewrite(...)` is the one polymorphic history-rewrite entry and `RewriteDemand(rewrite)` its gate.

- Auto: `inherited` is the parent-vector join (`VersionVector.Join` is the per-slot max) advanced by the committed op count on the COMMITTING origin's slot (the `origin` parameter — two writers on one branch occupy two distinct slots, so `Order` reads their concurrency truthfully), so a merge commit's vector dominates both parents; `MerkleRange.Of` folds a sorted content-key window into a digest so anti-entropy compares top-down and transfers only the divergent subtree.
- Receipt: a commit rides `ReceiptSinkPort` under `store.commit`; a branch mutation rides `store.branch`; the range-reconciliation transfer count rides `SyncApplyReceipt`.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` + `CanonicalWriter` — the one federation hasher and the ONE preimage alphabet; `EpsilonPolicy.ZeroTolerance` the grid-free lane), System.IO.Hashing (`XxHash128` — the accumulator `MerkleRange.Of` hands `CanonicalWriter.Streaming`, the peer anti-entropy digest that is never a content-key mint), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new ref kind is one `RefKind` row; a new ACL grant is one `Element/authority#GRANT_ALGEBRA` `Grant` row the `GrantSet` admits (never a local flag); a richer commit header is one field on `CommitMessage`/`CommitNode`; a new rewrite verb is one `HistoryRewrite` case with one `Rewrite` arm and its `RewriteDemand` grant row; zero new surface — a parallel commit store, a second DAG walker, or a git-shaped object database is the deleted form. This owner claims no op-log projection without a real application binding.
- Boundary: `VersionVector.Ordered` and `VersionVector.CanonicalBytes` are the vector's own canonical form, so the commit key, the `Version/ledger#CHANGEFEED` `OperationId` key, and the `CrdtOpWire` context arrays all read ONE order — a caller enumerating `Slots` directly writes hash-bucket order, which mints a different digest per runtime and per insertion history for one causal position. Commit keys derive from `ContentHash.Of` over the `Fields` stream — the kernel writer is the only alphabet, so no page-local length prefix or LE word exists beside it — and `Commit` distinct-sorts parents so a duplicate-parent or reordered-parent merge converges on one node; a wall-clock or random commit id is the deleted form. Member `CommitMessage` IS in the preimage, so re-wording mints a fresh node exactly as an amend does in any content-addressed DAG — a preimage omitting an identity member is the split-brain the identity/preimage agreement law forbids.
- Boundary: `MergeBase` is the true merge-base set — reachability intersect minus dominated — computed NEAR-LINEAR: two `Rank` passes for the common set and the nearest-first ordering, then ONE `Reach` pass seeded with every common candidate's parents, whose reached-intersect-common set is exactly the dominated candidates. Per-candidate `Rank` re-walks are the deleted `O(candidates x graph)` form. Clean history yields one base, a criss-cross the two-or-more bases the three-way merge virtualizes, and disjoint histories the empty `Seq`; the `Rasm.Bim` three-way merge is the named consumer.
- Boundary: `VersionVector` is the ONE concurrency primitive — `Order` returns `Concurrent` exactly when neither side dominates — and `AdvanceDemand` is its consumer, deriving the demand off `(IsMerge, Order)` as a TOTAL dispatch so the gate reads a derived `GrantSet` and never a caller-guessed lane. `BranchRef` grants ride the `Element/authority#GRANT_ALGEBRA` `GrantSet` narrowed to the branch lane, NOT the disjoint AppHost effect-gating `Capability` whose name the authority owner forbids re-using across strata. Gating conjoins the mutability precondition with BOTH `GrantSet` sides, and `Admits` is `Admin`-superuser-aware, so a flat `Write`-only gate that lets a read-plus-write actor force-push, and a parallel branch-only enum, are both deleted forms.
- Boundary: every `HistoryRewrite` is APPEND-ONLY — a revert is the inverse delta as a NEW commit, a cherry-pick one commit's ops replayed onto another head, a rebase a sequential replay minting a fresh linear lineage. History never mutates and the source commits stay reachable. This DAG stores op KEYS, so the `RewriteSeam` returns keys and a delta inversion or replay conflict faults on ITS owner's rail BEFORE any commit mints — a half-applied rewrite cannot exist. Mutating rewrites, a ref force-moved without its `RewriteDemand` gate, or a manual counter-edit standing in for a revert is the deleted form.
- Boundary: the durable commit-DAG is where the `csharp:Rasm.Bim` `BimCommit` federates and durably stores; the Bim three-way merge bases against this owner's `MergeBase` antichain. No current binding projects a `CommitNode` into the op-log, so this owner claims no `commit`-lane producer until that application handoff exists.

```csharp signature

// Ref capabilities close as a vocabulary. `{Mutable, Annotated}` is the corner two bool columns could hold while no
// ref shape answers it — an annotated tag is immutable by definition — so the law bars it at type init.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefCapability : ICapability<RefCapability> {
    public static readonly RefCapability Mutable = new("mutable");
    public static readonly RefCapability Annotated = new("annotated");
}

public static class RefPolicy {
    public static readonly CapabilitySet<RefCapability> Movable = CapabilitySet<RefCapability>.Of(RefCapability.Mutable);
    public static readonly CapabilitySet<RefCapability> Frozen = CapabilitySet<RefCapability>.None;
    public static readonly CapabilitySet<RefCapability> Signed = CapabilitySet<RefCapability>.Of(RefCapability.Annotated);
    public static readonly CapabilityLaw<RefCapability> Law = new(Seq(Movable, Frozen, Signed));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefKind {
    public static readonly RefKind Branch = new("branch", RefPolicy.Movable);
    public static readonly RefKind LightweightTag = new("tag", RefPolicy.Frozen);
    public static readonly RefKind AnnotatedTag = new("tag-annotated", RefPolicy.Signed);
    public static readonly RefKind RemoteTracking = new("remote-tracking", RefPolicy.Movable);
    public CapabilitySet<RefCapability> Refs { get; }
    private RefKind(string key, CapabilitySet<RefCapability> refs) : this(key) => Refs = refs;

    public static readonly Fin<Unit> Lawful =
        toSeq(Items).Traverse(static row => RefPolicy.Law.Admit(row.Refs)).As().Map(static _ => unit);
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
    public VersionVector Advance(Guid origin, long count) => new(Slots.AddOrUpdate(origin, e => e + count, count));
    public VersionVector Join(VersionVector other) => new(other.Slots.Fold(Slots, static (acc, s) => acc.AddOrUpdate(s.Key, e => long.Max(e, s.Value), s.Value)));
    public bool Dominates(VersionVector other) => other.Slots.ForAll(s => Slots.Find(s.Key).IfNone(0L) >= s.Value);
    public long At(Guid origin) => Slots.Find(origin).IfNone(0L);
    // ONE canonical slot order for every byte-deriving reader. `HashMap` enumerates in hash-bucket order, so a commit
    // preimage, an `OperationId` key, and a `CrdtOpWire` context each hashing their own enumeration mint three byte
    // strings for one causal position — the exact fork that keeps `CRDT_OP_SET` unfreezable. Ordinal over the
    // lowercase-N GUID text is the order Python `bytes` and TS `Uint8Array` reproduce; `Guid.CompareTo` sorts by .NET
    // field layout no peer runtime holds.
    public Seq<(Guid Origin, long Seq)> Ordered =>
        toSeq(Slots.AsIterable().OrderBy(static slot => slot.Key.ToString("N"), StringComparer.Ordinal).Select(static slot => (slot.Key, slot.Value)));

    // Canonical vector cell on the kernel writer: `Rows` count-frames the slots, `String` length-frames the 32-char
    // lowercase-N GUID text, `I64` fixes the counter. The commit key, the `OperationId` key, and the parity corpus
    // all call THIS member, so a slot-order edit cannot land on one key and not the other.
    public void CanonicalBytes(CanonicalWriter writer) =>
        writer.Rows(Ordered, static (slot, w) => { w.String(slot.Origin.ToString("N")).I64(slot.Seq); });
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

// `BranchRef.Acl` narrows the `Element/authority#GRANT_ALGEBRA` `GrantSet` to the branch lane (`AclScope.Branch`),
// carrying the ONE object-authorization vocabulary, NOT the disjoint AppHost effect-gating `Capability` (a
// cross-stratum name the authority owner forbids sharing). `Movable` gates on a `GrantSet` demand the caller selects
// per operation (`GrantSet.Of(Grant.Write)` for a fast-forward, `Grant.Merge`/`Grant.Rebase`/`Grant.ForcePush`
// for the wider rewrites) so one polymorphic gate discriminates by the demanded value; `GrantSet.Admits` is
// `Admin`-superuser-aware.
public sealed record BranchRef(string Name, RefKind Kind, UInt128 Head, GrantSet Acl, Guid Origin, Instant At, Option<string> Upstream, Option<UInt128> Target, CommitMessage Annotation, string Tagger) {
    public bool Movable(GrantSet actor, GrantSet demand) =>
        Kind.Refs.Admits(RefCapability.Mutable) && actor.Admits(demand) && Acl.Admits(demand);
}

public readonly record struct CommitNode(UInt128 ContentKey, Seq<UInt128> Parents, Seq<UInt128> OpKeys, string Branch, VersionVector Vector, string Actor, Hlc Cell, CommitMessage Message) {
    public bool IsMerge => Parents.Count > 1;
    public bool IsRoot => Parents.IsEmpty;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record MerkleRange {
    private MerkleRange() { }
    public sealed record Empty(UInt128 DigestValue) : MerkleRange;
    public sealed record Bounded(UInt128 Low, UInt128 High, UInt128 DigestValue, int Count) : MerkleRange;

    public UInt128 Digest => Map(empty: static range => range.DigestValue, bounded: static range => range.DigestValue);
    public bool Leaf => Map(empty: static _ => true, bounded: static range => range.Count <= CommitGraph.Fanout);
}

// `HistoryRewrite` closes the append-only rewrite request family: every rewrite MINTS new commits, history never
// mutates.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record HistoryRewrite {
    private HistoryRewrite() { }
    public sealed record Revert(UInt128 Target) : HistoryRewrite;
    public sealed record CherryPick(UInt128 Pick) : HistoryRewrite;
    public sealed record Rebase(Seq<UInt128> Chain, UInt128 NewBase) : HistoryRewrite;
}

// `RewriteSeam` frames the rewrite delegates: `Resolve` the commit reader; `Invert` the inverse op-key set of one
// commit's delta (the `GraphDelta` inversion behind the ledger/merge owners — added↔removed, revised pairs flipped);
// `Transplant` one commit's ops replayed onto a new head (three-way against the commit's parent, the `Version/merge`
// owner behind the delegate); `Stamp` the one HLC atom (`OpLog.Stamp`'s cell, never a second clock). Keys in, keys
// out — payload replay is the delegate owners' concern, and their typed conflict faults surface BEFORE any commit
// mints.
public sealed record RewriteSeam(
    Func<UInt128, Option<CommitNode>> Resolve,
    Func<CommitNode, IO<Seq<UInt128>>> Invert,
    Func<CommitNode, UInt128, IO<Seq<UInt128>>> Transplant,
    IO<Hlc> Stamp);

public static class CommitGraph {
    public const int Fanout = 16;

    // `Advance` steps the COMMITTING origin's slot — the writer's store id, never branch.Origin (the ref's minting
    // peer), so two writers on one branch occupy two causal slots and Order reads Concurrent. The key is the kernel
    // `ContentHash.Of` over `Fields`: one field-naming site the live mint and the parity corpus both call.
    public static CommitNode Commit(Seq<UInt128> parents, VersionVector inherited, Seq<UInt128> opKeys, BranchRef branch, Guid origin, string actor, Hlc cell, CommitMessage message) {
        CommitNode unkeyed = Unkeyed(parents, opKeys, branch.Name, inherited.Advance(origin, opKeys.Count), actor, cell, message);
        return unkeyed with { ContentKey = ContentHash.Of(unkeyed, Fields) };
    }

    // The node before its key: distinct-sorted parents and sorted op keys beside the vector the caller settled.
    // `ContentParityCorpus.CommitPreimage` retains the same node's `Fields` bytes, so the fixture and the live key
    // read one field stream.
    internal static CommitNode Unkeyed(Seq<UInt128> parents, Seq<UInt128> opKeys, string branch, VersionVector vector, string actor, Hlc cell, CommitMessage message) =>
        new(UInt128.Zero, toSeq(parents.Distinct().OrderBy(static k => k)), toSeq(opKeys.OrderBy(static k => k)), branch, vector, actor, cell, message);

    // ONE commit-key field stream over the kernel writer: `Rows` count-frames parents and op keys, `String`
    // length-frames branch, actor, and message, the vector and cell stream their own `CanonicalBytes`. Every
    // identity member is named here and `ContentKey` alone is excluded, so the key and the preimage cannot split.
    internal static void Fields(CommitNode node, CanonicalWriter w) {
        w.Rows(node.Parents, static (parent, x) => { x.U128(parent); })
         .Rows(node.OpKeys, static (key, x) => { x.U128(key); })
         .String(node.Branch);
        node.Vector.CanonicalBytes(w);
        w.String(node.Actor);
        node.Cell.CanonicalBytes(w);
        w.String(node.Message.Summary).String(node.Message.Body);
    }

    public static VectorOrder Order(VersionVector left, VersionVector right) =>
        (left.Slots.Equals(right.Slots), left.Dominates(right), right.Dominates(left)) switch {
            (true, _, _) => VectorOrder.Equal,
            (_, true, false) => VectorOrder.After,
            (_, false, true) => VectorOrder.Before,
            _ => VectorOrder.Concurrent,
        };

    // `VectorOrder` reaches its ONE consumer here, dispatched TOTALLY rather than through an equality ladder, so a
    // fifth order case breaks the build here. A merge demands `Merge`, a fast-forward or equal advance `Write`, and a
    // non-dominating reset `ForcePush`; the caller gates `branch.Movable(actor, AdvanceDemand(...))`.
    public static GrantSet AdvanceDemand(CommitNode commit, VersionVector head) =>
        commit.IsMerge
            ? GrantSet.Of(Grant.Merge)
            : Order(commit.Vector, head).Switch(
                before:     static _ => GrantSet.Of(Grant.ForcePush),
                concurrent: static _ => GrantSet.Of(Grant.ForcePush),
                after:      static _ => GrantSet.Of(Grant.Write),
                equal:      static _ => GrantSet.Of(Grant.Write));

    // `RewriteDemand` is AdvanceDemand's sibling over the rewrite family: Revert/CherryPick are forward commits
    // (Grant.Write); Rebase is the history rewrite the Grant.Rebase row exists for. The caller gates
    // `branch.Movable(actor, RewriteDemand(rewrite))` BEFORE Rewrite runs.
    public static GrantSet RewriteDemand(HistoryRewrite rewrite) => rewrite.Switch(
        revert: static _ => GrantSet.Of(Grant.Write),
        cherryPick: static _ => GrantSet.Of(Grant.Write),
        rebase: static _ => GrantSet.Of(Grant.Rebase));

    // ONE polymorphic rewrite entry — the request case discriminates, never three sibling verbs. Every arm mints NEW
    // CommitNodes through the one Commit writer: Revert commits the target's inverse op keys onto `onto`; CherryPick
    // transplants one commit's ops onto `onto`; Rebase FoldM-threads the chain OLDEST-FIRST onto NewBase, each
    // transplant landing on the previously minted head so the result is a fresh linear lineage. `head` is the vector
    // at `onto`; an unresolvable key faults CommitFault.RewriteAbsent (8261) typed.
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

    // Near-linear merge-base: two Rank passes (common set + nearest-first metric), then ONE Reach pass seeded with
    // every common candidate's parents — reached ∩ common IS the dominated set (a common node strictly reachable from
    // another common node via parent edges). The per-candidate Rank re-walk is the deleted O(candidates × graph)
    // form; Rasm.Bim MergeBase is the named consumer.
    public static Seq<UInt128> MergeBase(Func<UInt128, Option<CommitNode>> resolve, UInt128 left, UInt128 right) {
        HashMap<UInt128, int> leftRanked = Rank(resolve, left);
        HashMap<UInt128, int> rightRanked = Rank(resolve, right);
        Set<UInt128> common = toSet(toSeq(rightRanked.Keys).Filter(leftRanked.ContainsKey));
        Set<UInt128> dominated = Reach(resolve, toSeq(common).Bind(c => resolve(c).Map(static n => n.Parents).IfNone(Seq<UInt128>()))).Intersect(common);
        return toSeq(common.Filter(c => !dominated.Contains(c)).OrderBy(c => (leftRanked[c] + rightRanked[c], c)));
    }

    // The range digest is a PEER anti-entropy address, never a content-key mint: the accumulator is this owner's and
    // the framing is the kernel writer's `Streaming` leg, so the window keys count-framed `U128` rows under the one
    // alphabet and no second LE word layout exists beside `ContentHash.Of`.
    public static MerkleRange Of(Seq<UInt128> sortedKeys) {
        XxHash128 accumulator = new(seed: 0L);
        UInt128 address = CanonicalWriter.Streaming(EpsilonPolicy.ZeroTolerance, accumulator)
            .Rows(sortedKeys, static (key, w) => { w.U128(key); })
            .Digest();
        return sortedKeys.IsEmpty
            ? new MerkleRange.Empty(address)
            : new MerkleRange.Bounded(sortedKeys[0], sortedKeys[sortedKeys.Count - 1], address, sortedKeys.Count);
    }

    public static Seq<MerkleRange> Reconcile(Func<MerkleRange, Seq<MerkleRange>> children, MerkleRange local, MerkleRange remote) =>
        local.Digest == remote.Digest ? Seq<MerkleRange>()
        : remote.Leaf ? Seq(remote)
        : children(remote).Bind(child => Sibling(children(local), child) is { IsSome: true, Case: MerkleRange peer } ? Reconcile(children, peer, child) : Seq(child));

    static Option<MerkleRange> Sibling(Seq<MerkleRange> locals, MerkleRange remote) =>
        locals.Find(candidate => (candidate, remote) is (MerkleRange.Bounded local, MerkleRange.Bounded sought)
            && local.Low <= sought.High && sought.Low <= local.High);

    // `Rank` takes the EXPRESSION_SPINE named-kernel exemption: a longest-path BFS over the commit DAG — the
    // work-queue re-enqueues a node on finding a deeper path so the rank is the MAX generation (the nearest-first
    // merge-base ordering metric), a memoized traversal a monadic fold cannot express without re-walking, so the
    // mutable work-list is the kernel.
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

    // `Reach` runs the ONE reverse-reachability generation-mark pass (EXPRESSION_SPINE named-kernel exemption — a
    // visited-set BFS work-list): every key reachable via one-or-more parent edges from the seed frontier, O(V+E)
    // once regardless of candidate count.
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

| [INDEX] | [POLICY]              | [VALUE]                                   | [BINDING]                                                  |
| :-----: | :-------------------- | :---------------------------------------- | :--------------------------------------------------------- |
|  [01]   | merge-base resolution | maximal common-ancestor antichain         | near-linear: two `Rank` + ONE `Reach` pass; git multi-base |
|  [02]   | branch-advance demand | `AdvanceDemand` off `(IsMerge, Order)`    | derived `GrantSet`; never a caller-guessed lane            |
|  [03]   | content-key preimage  | `CommitGraph.Fields` on `CanonicalWriter` | identity/preimage agree; re-word is a fresh node           |
|  [04]   | branch grant          | `Element/authority#GRANT_ALGEBRA`         | `Movable` gates `Grant.Write`/`Merge`/`Rebase`/`ForcePush` |
|  [05]   | history rewrite       | `Rewrite` over `HistoryRewrite` cases     | append-only mints; `RewriteDemand` gates `Write`/`Rebase`  |

## [03]-[CRDT_ALGEBRA]

- Owner: `CrdtField` `[Union]` the convergent op-based/delta-state field family carrying the six replicated data types; `CrdtOp` the delta payload a changefeed entry carries; `RgaCell` the one growable-array element; `Crdt` the merge-fold surface whose `Merge` is commutative, associative, and idempotent over the op multiset, with the version-vector-gated tombstone compaction.
- Cases: `LwwRegister`, `MvRegister`, `OrSet`, `PnCounter`, `RgaSequence`, `EphemeralMap` on `CrdtField`; `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain | Beat | Leave` on `CrdtOp`.
- Entry: `Crdt.Merge(left, right)` joins matching family states and refuses a family mismatch; `Apply(state, id, op)` keeps the outer operation dot beside the generated delta through the fold; `Seed(op)` materializes a fresh cell only for a family-identifying op and refuses an unseated `Maintain`; `Law(op)` returns the arm's commutation row consumed by `Version/ledger#MERGE_LAW`.
- Auto: `SyncMerge.Apply` carries the admitted entry's `OperationId` into `Crdt.Apply`; this owner claims no op-log producer. OR-set live tags and tombstones remain keyed by the same element. RGA compaction clears retired value bytes while retaining identity, predecessor, value identity, and child routing. PN buckets accept only monotone cumulative halves and refuse equal-sequence forks. MV-register entries retain the outer dot separately from the register-specific causal context. Presence retains stamped live/left cells plus a monotone physical maintenance horizon.
- Receipt: a converged merge rides `SyncApplyReceipt`; a tombstone, live-element, compacted-tombstone, and live-presence count fold into the `store.crdt.merge` fact.
- Packages: NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, Generator.Equals, BCL inbox. Retired RGA values keep the kernel `ContentHash.Of` identity already owned by the suite; no CRDT-local hasher or package is admitted.
- Growth: a new replicated type is one `CrdtField` case, one `CrdtOp` arm, one `Merge`/`Apply` arm, and one `Seed`/`Law` pair the generated total `Switch` forces; zero new surface — a per-type merge service, a second convergence engine, or an op-transform rebase is the deleted form because the join-semilattice subsumes idempotency, commutativity, and reorder tolerance.
- Boundary: any partition of every causally eligible ordering of one admitted op multiset, applied any number of times, converges to identical state. RGA compaction is a value-retirement filter that preserves topology; OR-set removes spend one element and its observed tags; MV survival compares each candidate context with held outer dots; PN sequence ties must repeat the same totals. A presence maintenance horizon remains in state after cells compact, so a late pre-horizon beat cannot resurrect on one delivery order only.
- Boundary: a `(NodeId, Field)` cell has one family for its lifetime. Off-diagonal merge/apply and unseated maintenance refuse as convergence drift rather than returning the left operand or guessing RGA. `Crdt.Merge` reads no wall clock: retained HLC cells and declared horizons are its only ordering inputs. Payload-bearing cases use `CrdtBytes` while retaining `ReadOnlyMemory<byte>` at the zero-copy decode seam.
- Acceptance: `ContentParityCorpus.OpSet` folds every topological dot/op permutation twice and refuses any digest disagreement; fixtures include concurrent MV writes, delete-before-insert RGA replay, element-scoped OR removes, PN fork refusal, and maintain-before-beat presence replay.

```csharp signature
public readonly record struct ElementId(Guid Origin, ulong Logical) : IComparable<ElementId> {
    public static readonly ElementId Head = new(Guid.Empty, 0UL);
    public int CompareTo(ElementId other) {
        Span<byte> left = stackalloc byte[16];
        Span<byte> right = stackalloc byte[16];
        _ = Origin.TryWriteBytes(left, bigEndian: true, out _);
        _ = other.Origin.TryWriteBytes(right, bigEndian: true, out _);
        int byOrigin = left.SequenceCompareTo(right);
        return byOrigin != 0 ? byOrigin : Logical.CompareTo(other.Logical);
    }
}

// CrdtBytes is the ONE payload comparer: convergence proves by state equality, and a ReadOnlyMemory member compares
// by buffer coordinates, so a re-decoded replica of identical bytes reads unequal without it.
public sealed class CrdtBytes : IEqualityComparer<ReadOnlyMemory<byte>> {
    public static readonly CrdtBytes Default = new();
    public bool Equals(ReadOnlyMemory<byte> left, ReadOnlyMemory<byte> right) => left.Span.SequenceEqual(right.Span);
    public int GetHashCode(ReadOnlyMemory<byte> value) { HashCode hash = new(); hash.AddBytes(value.Span); return hash.ToHashCode(); }
}

[Equatable]
public readonly partial record struct RgaCell(
    ElementId Id,
    ElementId After,
    [property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> Value,
    UInt128 ValueKey,
    bool Tombstone,
    bool Routing) {
    public static readonly ElementId Origin = ElementId.Head;
    public static RgaCell Live(ElementId id, ElementId after, ReadOnlyMemory<byte> value) =>
        new(id, after, value, ContentHash.Of(value.Span), Tombstone: false, Routing: false);
}

[Equatable]
public sealed partial record MvEntry(
    [property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> Value,
    OperationId Version,
    VersionVector Context,
    Hlc Cell,
    Guid Origin);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PresenceValue {
    private PresenceValue() { }
    [Equatable]
    public sealed partial record Live([property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> State) : PresenceValue;
    public sealed record Left : PresenceValue;
}

public sealed record PresenceCell(PresenceValue Value, Hlc Cell);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record CrdtOp {
    private CrdtOp() { }
    [Equatable] public sealed partial record Set(string Field, [property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> Value, Hlc Cell, Guid Origin) : CrdtOp;
    [Equatable] public sealed partial record Write(string Field, [property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell, Guid Origin) : CrdtOp;
    public sealed record Add(string Field, UInt128 Element, ElementId Tag) : CrdtOp;
    public sealed record Remove(string Field, UInt128 Element, Seq<ElementId> ObservedTags) : CrdtOp;
    // Per-origin RUNNING TOTALS, not a bare delta: Sequence is the origin's monotone op counter and Positive/Negative
    // its cumulative sums, so Apply is a max-merge — a replayed or reordered Increment converges identically (the
    // idempotent join-semilattice law a delta-adding fold cannot satisfy).
    public sealed record Increment(string Field, Guid Origin, long Sequence, long Positive, long Negative) : CrdtOp;
    [Equatable] public sealed partial record InsertAfter(string Field, ElementId Predecessor, ElementId Id, [property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> Value) : CrdtOp;
    public sealed record Delete(string Field, ElementId Id) : CrdtOp;
    public sealed record Maintain(string Field, VersionVector Quiescent, Instant Liveness) : CrdtOp;
    [Equatable] public sealed partial record Beat(string Field, Guid Origin, [property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> State, Hlc Cell) : CrdtOp;
    public sealed record Leave(string Field, Guid Origin, Hlc Cell) : CrdtOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CrdtField {
    private CrdtField() { }
    [Equatable] public sealed partial record LwwRegister([property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> Value, Hlc Cell, Guid Origin) : CrdtField;
    public sealed record MvRegister(Seq<MvEntry> Values) : CrdtField;
    public sealed record OrSet(
        HashMap<UInt128, Set<ElementId>> Live,
        HashMap<UInt128, Set<ElementId>> Tombstoned) : CrdtField;
    public sealed record PnCounter(HashMap<Guid, (long Sequence, long Positive, long Negative)> Origins) : CrdtField;
    public sealed record RgaSequence(Seq<RgaCell> Cells, Set<ElementId> Deleted) : CrdtField;
    public sealed record EphemeralMap(HashMap<Guid, PresenceCell> Cells, Instant Horizon) : CrdtField;
}

public static class Crdt {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.crdt.merge"), StoreSlot.Create("store.crdt.decode"));

    public static Fin<CrdtField> Seed(CrdtOp op) => op.Switch<Fin<CrdtField>>(
        set: static _ => Fin.Succ<CrdtField>(new CrdtField.LwwRegister(ReadOnlyMemory<byte>.Empty, Hlc.Zero, Guid.Empty)),
        write: static _ => Fin.Succ<CrdtField>(new CrdtField.MvRegister(Seq<MvEntry>())),
        add: static _ => Fin.Succ<CrdtField>(new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), HashMap<UInt128, Set<ElementId>>())),
        remove: static _ => Fin.Succ<CrdtField>(new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), HashMap<UInt128, Set<ElementId>>())),
        increment: static _ => Fin.Succ<CrdtField>(new CrdtField.PnCounter(HashMap<Guid, (long Sequence, long Positive, long Negative)>())),
        insertAfter: static _ => Fin.Succ<CrdtField>(new CrdtField.RgaSequence(Seq<RgaCell>(), Set<ElementId>())),
        delete: static _ => Fin.Succ<CrdtField>(new CrdtField.RgaSequence(Seq<RgaCell>(), Set<ElementId>())),
        maintain: static m => Drift($"maintain-unseated:{m.Field}"),
        beat: static _ => Fin.Succ<CrdtField>(new CrdtField.EphemeralMap(HashMap<Guid, PresenceCell>(), Instant.MinValue)),
        leave: static _ => Fin.Succ<CrdtField>(new CrdtField.EphemeralMap(HashMap<Guid, PresenceCell>(), Instant.MinValue)));

    // Per-mutation-kind commutation, over the SAME `Version/ledger#CHANGEFEED` `OpLaw` triple the lane stance and the
    // `typescript:core/state/merge` `Merge.Law` spell — one algebra, three transcriptions. `set` is the lone
    // `Ordered` arm: two concurrent writers of one cell are a genuine conflict a total order resolves and one value
    // loses, so `Version/ledger#MERGE_LAW` counts an `Ordered` arm that leaves state unchanged as `Conflicted` rather
    // than as idempotent replay. `maintain` is `Semilattice` because compaction composes pure filters (`filter(p) ∘
    // filter(q)` is `filter(p ∧ q)` in either order, idempotent), yet it is a MEET on the tombstone set where every
    // other arm is a JOIN on state — which is why its admission gate lives on the entry's causal context and not in
    // this fold.
    public static OpLaw Law(CrdtOp op) => op.Switch(
        set: static _ => OpLaw.Ordered,
        write: static _ => OpLaw.Semilattice,
        add: static _ => OpLaw.Semilattice,
        remove: static _ => OpLaw.Semilattice,
        // Disjoint per-origin buckets absorbed by sequence-max: commutative and idempotent, yet not a semilattice
        // over the observable total, which the `Value` projection sums rather than joins.
        increment: static _ => OpLaw.Commutative,
        insertAfter: static _ => OpLaw.Semilattice,
        delete: static _ => OpLaw.Semilattice,
        maintain: static _ => OpLaw.Semilattice,
        beat: static _ => OpLaw.Semilattice,
        leave: static _ => OpLaw.Semilattice);

    public static Fin<CrdtField> Merge(CrdtField left, CrdtField right) => (left, right) switch {
        (CrdtField.LwwRegister l, CrdtField.LwwRegister r) =>
            Register(l, r).Map<CrdtField>(static held => held),
        (CrdtField.MvRegister l, CrdtField.MvRegister r) =>
            AntiChain(l.Values + r.Values).Map<CrdtField>(static values => new CrdtField.MvRegister(values)),
        (CrdtField.OrSet l, CrdtField.OrSet r) => Fin.Succ<CrdtField>(Observed(l, r)),
        (CrdtField.PnCounter l, CrdtField.PnCounter r) =>
            r.Origins.Fold(Fin.Succ(l), static (rail, row) => rail.Bind(held => Counter(held, row.Key, row.Value)))
                .Map<CrdtField>(static held => held),
        (CrdtField.RgaSequence l, CrdtField.RgaSequence r) =>
            Weave(l.Cells, r.Cells).Map<CrdtField>(cells => {
                var deleted = l.Deleted.Union(r.Deleted);
                return new CrdtField.RgaSequence(
                    cells.Map(cell => deleted.Contains(cell.Id) ? cell with { Tombstone = true } : cell), deleted);
            }),
        (CrdtField.EphemeralMap l, CrdtField.EphemeralMap r) => Fin.Succ<CrdtField>(Presence(l, r)),
        _ => Drift($"family-merge:{left.GetType().Name}:{right.GetType().Name}"),
    };

    public static Fin<CrdtField> Apply(CrdtField state, OperationId identity, CrdtOp op) => (state, op) switch {
        (CrdtField.LwwRegister register, CrdtOp.Set set) =>
            Register(register, new CrdtField.LwwRegister(set.Value, set.Cell, set.Origin))
                .Map<CrdtField>(static held => held),
        (CrdtField.MvRegister mv, CrdtOp.Write write) =>
            AntiChain(mv.Values.Add(new MvEntry(write.Value, identity, write.Context, write.Cell, write.Origin)))
                .Map<CrdtField>(static values => new CrdtField.MvRegister(values)),
        (CrdtField.OrSet set, CrdtOp.Add add) => Fin.Succ<CrdtField>(new CrdtField.OrSet(
            set.Live.AddOrUpdate(add.Element, held => held.Add(add.Tag), Set(add.Tag)), set.Tombstoned)),
        (CrdtField.OrSet set, CrdtOp.Remove remove) when toSet(remove.ObservedTags) is var observed =>
            Fin.Succ<CrdtField>(new CrdtField.OrSet(
                set.Live,
                set.Tombstoned.AddOrUpdate(remove.Element, held => held.Union(observed), observed))),
        (CrdtField.PnCounter counter, CrdtOp.Increment increment) =>
            Counter(counter, increment.Origin, (increment.Sequence, increment.Positive, increment.Negative))
                .Map<CrdtField>(static held => held),
        (CrdtField.RgaSequence sequence, CrdtOp.InsertAfter insert) =>
            Inserted(sequence, insert).Map<CrdtField>(static held => held),
        (CrdtField.RgaSequence _, CrdtOp.Delete delete) when delete.Id == RgaCell.Origin =>
            Drift("rga-delete-root"),
        (CrdtField.RgaSequence sequence, CrdtOp.Delete delete) => Fin.Succ<CrdtField>(
            new CrdtField.RgaSequence(
                sequence.Cells.Map(cell => cell.Id == delete.Id ? cell with { Tombstone = true } : cell),
                sequence.Deleted.Add(delete.Id))),
        (CrdtField.RgaSequence sequence, CrdtOp.Maintain maintain) => Fin.Succ<CrdtField>(Compact(sequence, maintain.Quiescent)),
        (CrdtField.EphemeralMap map, CrdtOp.Beat beat) => Fin.Succ<CrdtField>(Presence(map, beat)),
        (CrdtField.EphemeralMap map, CrdtOp.Leave leave) => Fin.Succ<CrdtField>(Presence(map, leave)),
        (CrdtField.EphemeralMap map, CrdtOp.Maintain maintain) => Fin.Succ<CrdtField>(Compact(map, maintain.Liveness)),
        _ => Drift($"family-apply:{state.GetType().Name}:{op.GetType().Name}"),
    };

    public static long Value(CrdtField.PnCounter counter) => counter.Origins.Values.Sum(static origin => origin.Positive - origin.Negative);
    public static Set<UInt128> Members(CrdtField.OrSet set) => toSet(set.Live
        .Filter((element, tags) => tags.Exists(tag => !set.Tombstoned.Find(element).IfNone(Set<ElementId>()).Contains(tag)))
        .Keys);
    public static Seq<ReadOnlyMemory<byte>> Materialize(CrdtField.RgaSequence sequence) =>
        sequence.Cells.Filter(static cell => !cell.Tombstone && !cell.Routing).Map(static cell => cell.Value);
    public static Seq<(Guid Origin, ReadOnlyMemory<byte> State)> Live(CrdtField.EphemeralMap map) =>
        map.Cells.Choose(static (origin, cell) => cell.Value is PresenceValue.Live live
            ? Some((origin, live.State))
            : None).Values.ToSeq();

    static int RegisterOrder(Hlc leftCell, Guid leftOrigin, Hlc rightCell, Guid rightOrigin) {
        int byCell = leftCell.CompareTo(rightCell);
        if (byCell != 0) { return byCell; }
        Span<byte> left = stackalloc byte[16];
        Span<byte> right = stackalloc byte[16];
        _ = leftOrigin.TryWriteBytes(left, bigEndian: true, out _);
        _ = rightOrigin.TryWriteBytes(right, bigEndian: true, out _);
        return left.SequenceCompareTo(right);
    }

    static Fin<CrdtField.LwwRegister> Register(
        CrdtField.LwwRegister held,
        CrdtField.LwwRegister candidate) =>
        RegisterOrder(candidate.Cell, candidate.Origin, held.Cell, held.Origin) switch {
            > 0 => Fin.Succ(candidate),
            < 0 => Fin.Succ(held),
            _ when CrdtBytes.Default.Equals(candidate.Value, held.Value) => Fin.Succ(held),
            _ => Drift<CrdtField.LwwRegister>("lww-cell-fork"),
        };

    static Fin<Seq<MvEntry>> AntiChain(Seq<MvEntry> values) {
        var versions = values.GroupBy(static entry => (entry.Version.Origin, entry.Version.Counter.Signed));
        if (versions.Any(static group => group.Distinct().Count() != 1)) { return Drift<Seq<MvEntry>>("mv-dot-fork"); }
        Seq<MvEntry> unique = toSeq(versions.Select(static group => group.First()));
        return Fin.Succ(toSeq(unique.Filter(candidate => !unique.Exists(other =>
                other.Version != candidate.Version
                && other.Context.At(candidate.Version.Origin) >= candidate.Version.Counter.Signed))
            .OrderBy(static entry => entry.Cell)
            .ThenBy(static entry => entry.Origin.ToString("N"), StringComparer.Ordinal)
            .ThenBy(static entry => entry.Version.Origin.ToString("N"), StringComparer.Ordinal)
            .ThenBy(static entry => entry.Version.Counter.Signed)));
    }

    static CrdtField.OrSet Observed(CrdtField.OrSet left, CrdtField.OrSet right) {
        var graves = right.Tombstoned.Fold(left.Tombstoned, static (map, row) =>
            map.AddOrUpdate(row.Key, held => held.Union(row.Value), row.Value));
        var live = right.Live.Fold(left.Live, static (map, row) =>
            map.AddOrUpdate(row.Key, held => held.Union(row.Value), row.Value));
        return new CrdtField.OrSet(live, graves);
    }

    static Fin<CrdtField.PnCounter> Counter(
        CrdtField.PnCounter counter,
        Guid origin,
        (long Sequence, long Positive, long Negative) candidate) =>
        counter.Origins.Find(origin).Match(
            Some: held => candidate == held
                ? Fin.Succ(counter)
                : candidate.Sequence < held.Sequence
                    && candidate.Positive <= held.Positive
                    && candidate.Negative <= held.Negative
                    ? Fin.Succ(counter)
                : candidate.Sequence == held.Sequence || candidate.Positive < held.Positive || candidate.Negative < held.Negative
                    ? Drift<CrdtField.PnCounter>($"counter-fork:{origin:n}:{candidate.Sequence}")
                    : candidate.Sequence < held.Sequence
                    ? Drift<CrdtField.PnCounter>($"counter-fork:{origin:n}:{candidate.Sequence}")
                    : Fin.Succ(new CrdtField.PnCounter(counter.Origins.Add(origin, candidate))),
            None: () => Fin.Succ(new CrdtField.PnCounter(counter.Origins.Add(origin, candidate))));

    static Fin<CrdtField.RgaSequence> Inserted(CrdtField.RgaSequence sequence, CrdtOp.InsertAfter insert) {
        if (insert.Id == RgaCell.Origin || insert.Id == insert.Predecessor
            || (insert.Predecessor != RgaCell.Origin && !sequence.Cells.Exists(cell => cell.Id == insert.Predecessor))) {
            return Drift<CrdtField.RgaSequence>($"rga-identity:{insert.Id}");
        }
        return sequence.Cells.Find(cell => cell.Id == insert.Id).Match(
            Some: held => held.After == insert.Predecessor && held.ValueKey == ContentHash.Of(insert.Value.Span)
                ? Fin.Succ(sequence)
                : Drift<CrdtField.RgaSequence>($"rga-reuse:{insert.Id}"),
            None: () => Weave(
                    sequence.Cells,
                    Seq(sequence.Deleted.Contains(insert.Id)
                        ? (RgaCell.Live(insert.Id, insert.Predecessor, insert.Value) with { Tombstone = true })
                        : RgaCell.Live(insert.Id, insert.Predecessor, insert.Value)))
                .Map(cells => new CrdtField.RgaSequence(cells, sequence.Deleted)));
    }

    static CrdtField.RgaSequence Compact(CrdtField.RgaSequence sequence, VersionVector quiescent) {
        var retired = toSet(sequence.Cells
            .Filter(cell => cell.Tombstone && quiescent.At(cell.Id.Origin) >= (long)cell.Id.Logical)
            .Map(static cell => cell.Id));
        return new CrdtField.RgaSequence(
            sequence.Cells.Map(cell => retired.Contains(cell.Id)
                ? cell with { Value = ReadOnlyMemory<byte>.Empty, Routing = true }
                : cell),
            sequence.Deleted.Filter(id => !retired.Contains(id)));
    }

    static CrdtField.EphemeralMap Presence(CrdtField.EphemeralMap left, CrdtField.EphemeralMap right) {
        var horizon = Instant.Max(left.Horizon, right.Horizon);
        var cells = right.Cells.Fold(left.Cells, static (map, row) =>
            map.AddOrUpdate(row.Key, held => Presence(held, row.Value), row.Value));
        return new CrdtField.EphemeralMap(cells.Filter((_, cell) => cell.Cell.Physical >= horizon), horizon);
    }

    static CrdtField.EphemeralMap Presence(CrdtField.EphemeralMap map, CrdtOp.Beat beat) =>
        beat.Cell.Physical < map.Horizon
            ? map
            : new CrdtField.EphemeralMap(
                map.Cells.AddOrUpdate(
                    beat.Origin,
                    held => Presence(held, new PresenceCell(new PresenceValue.Live(beat.State), beat.Cell)),
                    new PresenceCell(new PresenceValue.Live(beat.State), beat.Cell)),
                map.Horizon);

    static CrdtField.EphemeralMap Presence(CrdtField.EphemeralMap map, CrdtOp.Leave leave) =>
        leave.Cell.Physical < map.Horizon
            ? map
            : new CrdtField.EphemeralMap(
                map.Cells.AddOrUpdate(
                    leave.Origin,
                    held => Presence(held, new PresenceCell(new PresenceValue.Left(), leave.Cell)),
                    new PresenceCell(new PresenceValue.Left(), leave.Cell)),
                map.Horizon);

    static PresenceCell Presence(PresenceCell held, PresenceCell candidate) => held.Cell.CompareTo(candidate.Cell) switch {
        < 0 => candidate,
        > 0 => held,
        _ when held.Value is PresenceValue.Live && candidate.Value is PresenceValue.Left => held,
        _ when candidate.Value is PresenceValue.Live && held.Value is PresenceValue.Left => candidate,
        _ when held.Value is PresenceValue.Live left && candidate.Value is PresenceValue.Live right
            => left.State.Span.SequenceCompareTo(right.State.Span) >= 0 ? held : candidate,
        _ => held,
    };

    static CrdtField.EphemeralMap Compact(CrdtField.EphemeralMap map, Instant liveness) {
        var horizon = Instant.Max(map.Horizon, liveness);
        return new CrdtField.EphemeralMap(map.Cells.Filter((_, cell) => cell.Cell.Physical >= horizon), horizon);
    }

    static Fin<Seq<RgaCell>> Weave(Seq<RgaCell> left, Seq<RgaCell> right) {
        var groups = (left + right).GroupBy(static cell => cell.Id);
        if (groups.Any(static group => group.Select(static cell => (cell.After, cell.ValueKey)).Distinct().Count() != 1)) {
            return Drift<Seq<RgaCell>>("rga-id-fork");
        }
        IEnumerable<RgaCell> merged = groups.Select(static group => group.Aggregate(static (left, right) => {
            bool routing = left.Routing || right.Routing;
            return left with {
                Value = routing ? ReadOnlyMemory<byte>.Empty : left.Value,
                Tombstone = left.Tombstone || right.Tombstone,
                Routing = routing,
            };
        }));
        HashMap<ElementId, Seq<RgaCell>> children = toHashMap(merged.GroupBy(static cell => cell.After)
            .Select(static group => (group.Key, toSeq(group.OrderBy(static cell => cell.Id)))));
        return Fin.Succ(Linearize(children, RgaCell.Origin, Seq<RgaCell>()));
    }

    static Seq<RgaCell> Linearize(HashMap<ElementId, Seq<RgaCell>> children, ElementId after, Seq<RgaCell> woven) =>
        children.Find(after).IfNone(Seq<RgaCell>()).Fold(woven, (held, cell) => Linearize(children, cell.Id, held.Add(cell)));

    static Fin<CrdtField> Drift(string axis) =>
        Fin.Fail<CrdtField>(new KernelFault.InvalidValue($"crdt:{axis}", "convergence invariant"));
    static Fin<T> Drift<T>(string axis) =>
        Fin.Fail<T>(new KernelFault.InvalidValue($"crdt:{axis}", "convergence invariant"));
}
```

| [INDEX] | [TYPE]       | [CRDT_CLASS]                         | [CONVERGENCE]                                             |
| :-----: | :----------- | :----------------------------------- | :-------------------------------------------------------- |
|  [01]   | LwwRegister  | last-write-wins by (HLC, origin)     | total order on the stamp tuple; superset of `Adjudicate`  |
|  [02]   | MvRegister   | multi-value concurrent-keep          | causal anti-chain; dominated writes collapse              |
|  [03]   | OrSet        | add-wins observed-remove set         | per-element tag-set union minus observed removes          |
|  [04]   | PnCounter    | per-origin running totals + sequence | per-origin sequence-max of monotone totals                |
|  [05]   | RgaSequence  | replicated growable array            | predecessor weave; compacted routes retain no value bytes |
|  [06]   | EphemeralMap | stamped live/left presence register  | per-origin HLC join under a retained liveness horizon     |

Merge policy per mutation kind — `Crdt.Law` returns column three, and `Version/ledger#MERGE_LAW` reads it to separate a lost total order from an idempotent replay:

| [INDEX] | [OP]          | [LAW]       | [CONCURRENT_SIBLING]                    | [CONFLICT_RESOLUTION]                            |
| :-----: | :------------ | :---------- | :-------------------------------------- | :----------------------------------------------- |
|  [01]   | `set`         | Ordered     | genuine conflict — one value loses      | total order on `(Hlc, Origin)`; loser receipted  |
|  [02]   | `write`       | Semilattice | both survive on the causal anti-chain   | none — the reader resolves the multi-value cell  |
|  [03]   | `add`         | Semilattice | tag-set union; tags are unique per add  | add-wins over a `remove` that observed no tag    |
|  [04]   | `remove`      | Semilattice | observed-tag tombstone union            | loses every tag it never observed                |
|  [05]   | `increment`   | Commutative | disjoint per-origin buckets             | unreachable — two origins never share a slot     |
|  [06]   | `insertAfter` | Semilattice | siblings order by ascending `ElementId` | deterministic weave; absent predecessor refuses  |
|  [07]   | `delete`      | Semilattice | monotone tombstone flag                 | tombstone wins; the slot stays for later inserts |
|  [08]   | `maintain`    | Semilattice | filter composition — a MEET, not a join | divergent horizons are one history, two views    |
|  [09]   | `beat`        | Semilattice | per-origin cell; origins are disjoint   | strictly-later `Hlc` supersedes within one slot  |
|  [10]   | `leave`       | Semilattice | per-origin stamped tombstone            | later wins; an equal-cell beat wins              |

## [04]-[CRDT_WIRE]

- Owner: `Hlc` the hybrid-logical-clock stamp the Marten event `Timestamp`, the changefeed projection, the CRDT merge, the commit cell, and the generated wire all read; corpus `CrdtOpWire` the ONE ten-arm wire vocabulary; `CrdtOpMapper` the projection between that generated oneof and the domain union; `CommitFault` the closed `[Union]` fault family over the KERNEL `Rasm.Domain.Fault` in the 8260 band; `CrdtWire` the static bounded proto-binary codec; `ParitySlot` the corpus-leg axis carrying its producer-owner label; `ParityVector` the one fixture carrier whose digest ALWAYS derives through the kernel `ContentHash.Of` at mint; `ContentParityCorpus` the surface minting this package's parity legs and reconciling a local corpus against the golden one.
- Cases: the generated required oneof closes at `set | write | add | remove | increment | insertAfter | delete | maintain | beat | leave`; `field` sits once on the root, the generated arm owns every variant-only member, and `beat`/`leave` carry the presence delta. Every `ParitySlot` row names its producer owner beside a `MintedHere` stance the `Mint`/`Contribute` split reads: a minted-here row derives from this owner's own writers, a contributed row flows in one-directionally, and the roster alone fixes membership so a new leg falsifies no sentence. Row `elementset` has its `Query/lane#ELEMENT_SET_ALGEBRA` owner call `Contribute` with its own framed preimage, so the Version owner freezes the foreign byte shape but never reaches back into Query to re-derive it.
- Entry: `CrdtWire.Encode(op)` maps onto the generated message, validates the descriptor rules plus strict causal-row order, and emits proto-binary bytes; `ContentKey(payload)` hashes THOSE held bytes without re-encoding; `CrdtWire.Decode(payload)` bounds extent, parses, validates, and maps one generated arm, failing `CommitFault.DecodeDrift` on malformed, unset, unknown, or non-canonical input. `ContentParityCorpus.Mint(...)` mints every minted-here vector over this page's own writers and folds in the contributed ones; `Contribute(slot, canonical)` is the contribution seam a foreign producer calls, failing `OwnerMinted` on an owner-minted slot; `Reconcile(local, golden)` accumulates every `ParityDrift` the cross-runtime harness finds.
- Auto: `Hlc.Observe` swaps the local cell forward past both the wall clock and the observed remote cell so a received op never rewinds the local logical counter; `CrdtWire.Encode` supplies the raw `OpLogEntry.Payload` only for the `crdt` family, while every other family retains its existing payload codec and the positional envelope stays unchanged. The generated oneof is the case authority and each adapter dispatch reads it directly; no `[MessagePack.Union]`, msgspec arm hierarchy, or TypeScript positional arm schema survives. `ContentParityCorpus.Mint` retains the actual proto payload bytes the live key consumed; semantic parity compares decoded generated values because protobuf serialization is not the cross-runtime canonical preimage.
- Receipt: an encoded delta carries no receipt (the `OpLogEntry` carries the lane codec, content key, and HLC cell); an invalid domain-to-wire projection refuses as `CommitFault.EncodeDrift`, a decode failure folds into `store.crdt.decode` as `CommitFault.DecodeDrift`, and a parity drift folds into the `Reconcile` `Validation` as the accumulated `CommitFault.ParityDrift` cross-runtime mismatch set, never a first-mismatch abort.
- Packages: Rasm.Contracts (`rasm.contracts.crdt.v1.CrdtOpWire` plus `clock.v1.Hlc`), Google.Protobuf, Celly.Protovalidate, Rasm (`Rasm.Domain` `ContentHash.Of` + `CanonicalWriter.Retaining`/`ToBytes` — parity mint; `Rasm.Domain.Fault` — fault-band base), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new op is one corpus oneof arm plus its typed message, one `CrdtOp` arm, and the generated-arm/domain adapter pair; every peer regenerates from the same descriptor and no peer authors a wire case. A new parity leg is one `ParitySlot` row with one `Mint` or `Contribute` vector, never a second corpus store or a per-fixture golden-bytes constant family; zero new surface.
- Boundary: `CrdtOpWire` is generated proto-binary under ONE family-derived discriminant: only `ColumnFamily.Crdt` decodes `OpLogEntry.Payload` as that message. The thirteen-slot MessagePack `OpLogEntry` envelope remains positional and its payload remains raw, so scalar, geometry, presence, commit, branch, and attest entries cross byte-identically without an `Any` or a fabricated CRDT arm. LWW `Adjudicate` survives only as the generated `set` arm reconstructing `LwwRegister`; an unset or unknown generated arm refuses typed.
- Boundary: the `Hlc` layout is the KERNEL's byte-for-byte (`Rasm/Domain/frame#RECEIPT_PORT`) — physical half first as the Unix-tick `long` at one tick per hundred nanoseconds, logical half second as the monotone `ulong`, two `CanonicalWriter.I64` words — so `CanonicalBytes` streams the canonical sixteen-byte cell the commit key and the op key both hash, and ordering compares causality without a wall clock. `Hlc.Zero` is an in-memory absence value alone, because its physical half is outside the I63 domain the packed slot admits.
- Boundary: the restore lane rejects a payload past the declared byte ceiling before parsing and validates every known field and required arm before domain admission. Generated parsing may preserve unknown fields on the transient message, but the domain projection claims no opaque forward-transit path; a relay that owes byte preservation forwards the held payload instead of parse-reserializing it. Protobuf bytes are NOT semantic identity across runtimes: `ContentKey` hashes the exact payload octets stored and announced, a receiver verifies those held octets, and no decoder parse-reserializes before that check. `OperationId` remains the causal identity, so equal payload bytes from two peers never collapse two operations.
- Boundary: `ContentParityCorpus` freezes the cell, the commit-key `Fields` stream, and an actual generated op payload, each as one `ParityVector` whose digest derives through `ContentHash.Of` at mint. The CRDT contract's manifest proof is value parity over the generated oneof, not cross-runtime protobuf byte identity; `Contribute` refuses an owner-minted slot, keeping the dependency one-directional, and `Reconcile` accumulates every drift through `Validation` rather than aborting on the first.

```csharp signature
using System.Buffers.Binary;
using Celly.Protovalidate;
using Google.Protobuf;
using Rasm.Contracts.Crdt.V1;

// Cell layout remains the kernel's canonical-frame owner for commit and HLC parity. The generated CRDT message
// composes the corpus Hlc message instead of re-spelling these halves on every operation arm.
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
    // Two fixed-width `I64` words on the kernel writer — the physical ticks, then the logical half reinterpreted
    // `unchecked` so the monotone `ulong` rides the one signed word the alphabet declares.
    public void CanonicalBytes(CanonicalWriter writer) =>
        writer.I64(Physical.ToUnixTimeTicks()).I64(unchecked((long)Logical));
}

// --- [ERRORS] --------------------------------------------------------------------------

// `[FaultCase]` seats this family on the kernel `[FaultCase]` floor over `Rasm.Domain.Fault` — the SAME
// template `SyncFault` and `RecoveryFault` realize — so a bare case is an `Error` that lifts onto
// `Fin`/`Validation` with no `.ToError()` hop. `Code` SEAL to the seated row, so a case past the
// row's span breaks where the roster proves itself; a bare `Error.New` integer or a literal offset in a `Switch`
// is the deleted form.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommitFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Commit;
    private CommitFault() { }
    [FaultCase(0)]
    public sealed partial record DecodeDrift(Error Cause) : CommitFault(), ICausedFault;
    [FaultCase(1)]
    public sealed partial record RewriteAbsent(UInt128 Source) : CommitFault();
    [FaultCase(2)]
    public sealed partial record ParityDrift(string Slot, string Producer) : CommitFault();
    [FaultCase(3)]
    public sealed partial record OwnerMinted(string Slot) : CommitFault();
    [FaultCase(4)]
    public sealed partial record EncodeDrift(Error Cause) : CommitFault(), ICausedFault;

    public override string Message => Switch(
        decodeDrift:   static c => $"<crdt-decode-drift:{c.Cause.Message}>",
        rewriteAbsent: static c => $"<rewrite-source-absent:{c.Source:x32}>",
        parityDrift:   static c => $"<parity-drift:{c.Slot}@{c.Producer}>",
        ownerMinted:   static c => $"<parity-owner-mints:{c.Slot}>",
        encodeDrift:   static c => $"<crdt-encode-drift:{c.Cause.Message}>");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CrdtWire {
    const int PayloadLimit = 1 << 20;
    static readonly Validator Rules = new(CrdtReflection.Descriptor);

    // The key hashes the exact held payload bytes. Parsing and re-encoding before this call is forbidden because
    // protobuf serialization is not the semantic canonical form across runtimes.
    public static UInt128 ContentKey(ReadOnlyMemory<byte> payload) => ContentHash.Of(payload.Span);

    public static Fin<ReadOnlyMemory<byte>> Encode(CrdtOp op) =>
        Op.Of().Catch(() => CrdtOpMapper.Wire(op))
            .MapFail(static error => new CommitFault.EncodeDrift(error))
            .Bind(static wire => Lawful(wire)
                ? Fin.Succ(wire)
                : Fin.Fail<CrdtOpWire>(new CommitFault.EncodeDrift(Error.New("<crdt-contract-drift>"))))
            .Bind(static wire => wire.CalculateSize() <= PayloadLimit
                ? Fin.Succ((ReadOnlyMemory<byte>)wire.ToByteArray())
                : Fin.Fail<ReadOnlyMemory<byte>>(new CommitFault.EncodeDrift(
                    Error.New($"<crdt-payload-overrun:{wire.CalculateSize()}:{PayloadLimit}>"))));

    public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload) =>
        payload.Length > PayloadLimit
            ? Fin.Fail<CrdtOp>(new CommitFault.DecodeDrift(Error.New($"<crdt-payload-overrun:{payload.Length}:{PayloadLimit}>")))
            : Op.Of().Catch(() => Fin.Succ(CrdtOpWire.Parser.ParseFrom(payload.Span)))
                .MapFail(static error => error.Exception.Case is InvalidProtocolBufferException
                    ? (Error)new CommitFault.DecodeDrift(error)
                    : error)
                .Bind(Admit)
                .Bind(CrdtOpMapper.Op);

    static Fin<CrdtOpWire> Admit(CrdtOpWire wire) =>
        Lawful(wire)
            ? Fin.Succ(wire)
            : Fin.Fail<CrdtOpWire>(new CommitFault.DecodeDrift(Error.New("<crdt-contract-drift>")));

    static bool Lawful(CrdtOpWire wire) => Rules.Validate(wire).Count == 0 && CrdtOpMapper.Ordered(wire);
}

// --- [COMPOSITION] -----------------------------------------------------------------------
// The generated oneof is the sole wire case owner. This adapter performs only irreducible domain transforms: UUID
// and UInt128 network-order bytes, HLC construction, and ordered causal rows. No peer-authored DTO or arm hierarchy
// sits beside the descriptor.
public static class CrdtOpMapper {
    public static CrdtOpWire Wire(CrdtOp op) => op.Switch<CrdtOpWire>(
        set: static o => new() { Field = o.Field, Set = new SetOp { Value = Octets(o.Value), Stamp = Stamp(o.Cell), Origin = Uuid(o.Origin) } },
        write: static o => new() { Field = o.Field, Write = new WriteOp { Value = Octets(o.Value), Context = { Slots(o.Context) }, Stamp = Stamp(o.Cell), Origin = Uuid(o.Origin) } },
        add: static o => new() { Field = o.Field, Add = new AddOp { Element = Wide(o.Element), Tag = Id(o.Tag) } },
        remove: static o => new() { Field = o.Field, Remove = new RemoveOp { Element = Wide(o.Element), ObservedTags = { Ids(o.ObservedTags) } } },
        increment: static o => new() { Field = o.Field, Increment = new IncrementOp { Origin = Uuid(o.Origin), Sequence = checked((ulong)o.Sequence), Positive = checked((ulong)o.Positive), Negative = checked((ulong)o.Negative) } },
        insertAfter: static o => new() { Field = o.Field, InsertAfter = new InsertAfterOp { Predecessor = Id(o.Predecessor), Id = Id(o.Id), Value = Octets(o.Value) } },
        delete: static o => new() { Field = o.Field, Delete = new DeleteOp { Id = Id(o.Id) } },
        maintain: static o => new() { Field = o.Field, Maintain = new MaintainOp { Quiescent = { Slots(o.Quiescent) }, LivenessTicks = o.Liveness.ToUnixTimeTicks() } },
        beat: static o => new() { Field = o.Field, Beat = new BeatOp { Origin = Uuid(o.Origin), State = Octets(o.State), Stamp = Stamp(o.Cell) } },
        leave: static o => new() { Field = o.Field, Leave = new LeaveOp { Origin = Uuid(o.Origin), Stamp = Stamp(o.Cell) } });

    public static Fin<CrdtOp> Op(CrdtOpWire wire) => wire.ArmCase switch {
        CrdtOpWire.ArmOneofCase.Set => Mapped(() => new CrdtOp.Set(wire.Field, wire.Set.Value.Memory, Cell(wire.Set.Stamp), Uuid(wire.Set.Origin))),
        CrdtOpWire.ArmOneofCase.Write => Mapped(() => new CrdtOp.Write(wire.Field, wire.Write.Value.Memory, Vector(wire.Write.Context), Cell(wire.Write.Stamp), Uuid(wire.Write.Origin))),
        CrdtOpWire.ArmOneofCase.Add => Mapped(() => new CrdtOp.Add(wire.Field, Wide(wire.Add.Element), Id(wire.Add.Tag))),
        CrdtOpWire.ArmOneofCase.Remove => Mapped(() => new CrdtOp.Remove(wire.Field, Wide(wire.Remove.Element), toSeq(wire.Remove.ObservedTags.Select(Id)))),
        CrdtOpWire.ArmOneofCase.Increment => Mapped(() => new CrdtOp.Increment(wire.Field, Uuid(wire.Increment.Origin), checked((long)wire.Increment.Sequence), checked((long)wire.Increment.Positive), checked((long)wire.Increment.Negative))),
        CrdtOpWire.ArmOneofCase.InsertAfter => Mapped(() => new CrdtOp.InsertAfter(wire.Field, Id(wire.InsertAfter.Predecessor), Id(wire.InsertAfter.Id), wire.InsertAfter.Value.Memory)),
        CrdtOpWire.ArmOneofCase.Delete => Mapped(() => new CrdtOp.Delete(wire.Field, Id(wire.Delete.Id))),
        CrdtOpWire.ArmOneofCase.Maintain => Mapped(() => new CrdtOp.Maintain(wire.Field, Vector(wire.Maintain.Quiescent), Instant.FromUnixTimeTicks(wire.Maintain.LivenessTicks))),
        CrdtOpWire.ArmOneofCase.Beat => Mapped(() => new CrdtOp.Beat(wire.Field, Uuid(wire.Beat.Origin), wire.Beat.State.Memory, Cell(wire.Beat.Stamp))),
        CrdtOpWire.ArmOneofCase.Leave => Mapped(() => new CrdtOp.Leave(wire.Field, Uuid(wire.Leave.Origin), Cell(wire.Leave.Stamp))),
        _ => Fin.Fail<CrdtOp>(new CommitFault.DecodeDrift(Error.New("<crdt-arm-unset-or-unknown>"))),
    };

    static Fin<CrdtOp> Mapped(Func<CrdtOp> map) =>
        Op.Of().Catch(map).MapFail(static error => new CommitFault.DecodeDrift(error));

    public static bool Ordered(CrdtOpWire wire) => wire.ArmCase switch {
        CrdtOpWire.ArmOneofCase.Write => Ordered(wire.Write.Context),
        CrdtOpWire.ArmOneofCase.Remove => Ordered(wire.Remove.ObservedTags),
        CrdtOpWire.ArmOneofCase.Maintain => Ordered(wire.Maintain.Quiescent),
        _ => true,
    };

    static IEnumerable<VectorSlot> Slots(VersionVector vector) =>
        vector.Ordered.Map(static slot => new VectorSlot { Origin = Uuid(slot.Origin), Sequence = checked((ulong)slot.Seq) });
    static VersionVector Vector(IEnumerable<VectorSlot> slots) =>
        new(toHashMap(slots.Select(static slot => (Uuid(slot.Origin), checked((long)slot.Sequence)))));
    static IEnumerable<Rasm.Contracts.Crdt.V1.ElementId> Ids(Seq<ElementId> ids) =>
        ids.OrderBy(static id => id.Origin.ToString("N"), StringComparer.Ordinal).ThenBy(static id => id.Logical).Select(Id);
    static bool Ordered(IEnumerable<VectorSlot> rows) =>
        rows.Zip(rows.Skip(1), static (left, right) => left.Origin.Span.SequenceCompareTo(right.Origin.Span) < 0).All(static ordered => ordered);
    static bool Ordered(IEnumerable<Rasm.Contracts.Crdt.V1.ElementId> rows) =>
        rows.Zip(rows.Skip(1), static (left, right) =>
            (left.Origin.Span.SequenceCompareTo(right.Origin.Span), left.Logical.CompareTo(right.Logical)) is (< 0, _) or (0, < 0)).All(static ordered => ordered);

    static Rasm.Contracts.Clock.V1.Hlc Stamp(Hlc cell) => new() { Physical = cell.Physical.ToUnixTimeTicks(), Logical = cell.Logical };
    static Hlc Cell(Rasm.Contracts.Clock.V1.Hlc cell) => new(Instant.FromUnixTimeTicks(cell.Physical), cell.Logical);
    static Rasm.Contracts.Crdt.V1.ElementId Id(ElementId id) => new() { Origin = Uuid(id.Origin), Logical = id.Logical };
    static ElementId Id(Rasm.Contracts.Crdt.V1.ElementId id) => new(Uuid(id.Origin), id.Logical);
    static ByteString Octets(ReadOnlyMemory<byte> value) => ByteString.CopyFrom(value.Span);

    static ByteString Uuid(Guid value) {
        Span<byte> bytes = stackalloc byte[16];
        _ = value.TryWriteBytes(bytes, bigEndian: true, out _);
        return ByteString.CopyFrom(bytes);
    }
    static Guid Uuid(ByteString value) => new(value.Span, bigEndian: true);
    static ByteString Wide(UInt128 value) {
        Span<byte> bytes = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(bytes, value);
        return ByteString.CopyFrom(bytes);
    }
    static UInt128 Wide(ByteString value) => BinaryPrimitives.ReadUInt128BigEndian(value.Span);
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

// `ParityVector` carries one parity fixture: canonical bytes with the digest ALWAYS derived through the kernel
// `ContentHash.Of` at mint; tests/contracts/manifest.json alone declares a fixture frozen (HLC_TWO_HALF and CRDT_OP_SET
// stay DESIGN-PIN until the harness proof) — an unstamped-Option carrier and a corpus-local seed constant are the
// deleted forms.
public readonly record struct ParityVector(ParitySlot Slot, ReadOnlyMemory<byte> Canonical, UInt128 Digest) {
    public static ParityVector Of(ParitySlot slot, ReadOnlyMemory<byte> canonical) => new(slot, canonical, ContentHash.Of(canonical.Span));
    public bool Holds(ParityVector pinned) => Slot == pinned.Slot && Digest == pinned.Digest;
}

public static class ContentParityCorpus {
    // Every minted-here vector RETAINS the bytes its live digest consumed: `CanonicalWriter.Retaining` is the one
    // mint whose `ToBytes` close is legal, so a fixture is the exact byte string a peer reproduces and a streaming
    // writer reaching this seat refuses typed rather than pinning an empty preimage.
    static Fin<ParityVector> Retained<TState>(ParitySlot slot, TState state, Action<TState, CanonicalWriter> fields) {
        CanonicalWriter writer = CanonicalWriter.Retaining(EpsilonPolicy.ZeroTolerance);
        fields(state, writer);
        return writer.ToBytes(Op.Of(name: slot.Key)).Map(bytes => ParityVector.Of(slot, bytes));
    }

    public static Fin<ParityVector> Cell(Hlc cell) =>
        Retained(ParitySlot.HlcCell, cell, static (held, w) => held.CanonicalBytes(w));

    // The SAME `CommitGraph.Fields` stream the live key hashes, over the same unkeyed node `Commit` builds.
    public static Fin<ParityVector> CommitPreimage(Seq<UInt128> parents, Seq<UInt128> opKeys, string branch, VersionVector vector, string actor, Hlc cell, CommitMessage message) =>
        Retained(ParitySlot.CommitKey, CommitGraph.Unkeyed(parents, opKeys, branch, vector, actor, cell, message), CommitGraph.Fields);

    public static Fin<ParityVector> Op(CrdtOp op) =>
        CrdtWire.Encode(op).Map(static bytes => ParityVector.Of(ParitySlot.CrdtOp, bytes));

    // CRDT_OP_SET producer (kernel corpus row [04]): every topological delivery permutation folds twice to prove
    // replay idempotence, and the vector retains the converged state. A permutation-dependent fold refuses.
    public static Fin<ParityVector> OpSet(Seq<(OperationId Id, CrdtOp Op)> ops) =>
        ops.IsEmpty
            ? Fin.Fail<ParityVector>(new CommitFault.ParityDrift(ParitySlot.CrdtOpSet.Key, "<empty-op-set>"))
            : Permutations(ops)
                .Map(order => Crdt.Seed(ops[0].Op)
                    .Bind(seed => order.Fold(
                        Fin.Succ(seed),
                        (rail, row) => rail.Bind(state => Crdt.Apply(state, row.Id, row.Op)))
                        .Bind(state => order.Fold(
                            Fin.Succ(state),
                            (rail, row) => rail.Bind(held => Crdt.Apply(held, row.Id, row.Op)))))
                    .Bind(state => Retained(ParitySlot.CrdtOpSet, state, Canonical)))
                .TraverseM(identity).As()
                .Bind(folds => folds.Map(static vector => vector.Digest).Distinct().Count() == 1
                    ? Fin.Succ(folds[0])
                    : Fin.Fail<ParityVector>(new CommitFault.ParityDrift(ParitySlot.CrdtOpSet.Key, "<divergent-delivery-fold>")));

    static Seq<Seq<(OperationId Id, CrdtOp Op)>> Permutations(Seq<(OperationId Id, CrdtOp Op)> ops) =>
        ops.Count <= 1
            ? Seq(ops)
            : toSeq(Enumerable.Range(0, ops.Count))
                .Filter(pick => CausallyEligible(ops, pick))
                .Bind(pick => Permutations(ops.RemoveAt(pick)).Map(rest => ops[pick].Cons(rest)));

    static bool CausallyEligible(Seq<(OperationId Id, CrdtOp Op)> remaining, int pick) {
        OperationId candidate = remaining[pick].Id;
        return !Enumerable.Range(0, remaining.Count).Any(index => index != pick
            && candidate.Context.At(remaining[index].Id.Origin) >= remaining[index].Id.Counter.Signed);
    }

    static void Canonical(CrdtField state, CanonicalWriter w) {
        switch (state) {
            case CrdtField.LwwRegister register:
                w.Ordinal(0);
                register.Cell.CanonicalBytes(w);
                w.String(register.Origin.ToString("N")).Ordinal(register.Value.Length).Raw(register.Value.Span);
                break;
            case CrdtField.MvRegister multi:
                w.Ordinal(1).Rows(toSeq(multi.Values
                    .OrderBy(static held => held.Cell)
                    .ThenBy(static held => held.Origin.ToString("N"), StringComparer.Ordinal)
                    .ThenBy(static held => held.Version.Origin.ToString("N"), StringComparer.Ordinal)
                    .ThenBy(static held => held.Version.Counter.Signed)), static (held, x) => {
                    x.String(held.Origin.ToString("N"));
                    x.String(held.Version.Origin.ToString("N")).I64(held.Version.Counter.Signed);
                    held.Context.CanonicalBytes(x);
                    held.Cell.CanonicalBytes(x);
                    x.Ordinal(held.Value.Length).Raw(held.Value.Span);
                });
                break;
            case CrdtField.OrSet observed:
                w.Ordinal(2);
                var elements = toSeq(observed.Live.Keys.Concat(observed.Tombstoned.Keys).Distinct().Order());
                w.Rows(elements, (element, x) => {
                    x.U128(element);
                    Elements(observed.Live.Find(element).IfNone(Set<ElementId>()), x);
                    Elements(observed.Tombstoned.Find(element).IfNone(Set<ElementId>()), x);
                });
                break;
            case CrdtField.PnCounter counter:
                w.Ordinal(3).Rows(toSeq(counter.Origins.AsIterable()
                    .OrderBy(static row => row.Key.ToString("N"), StringComparer.Ordinal)), static (row, x) =>
                        x.String(row.Key.ToString("N")).I64(row.Value.Sequence)
                            .I64(row.Value.Positive).I64(row.Value.Negative));
                break;
            case CrdtField.RgaSequence sequence:
                w.Ordinal(4).Rows(sequence.Cells, static (cell, x) => {
                    Element(cell.Id, x);
                    Element(cell.After, x);
                    x.U128(cell.ValueKey).Bool(cell.Tombstone).Bool(cell.Routing)
                        .Ordinal(cell.Value.Length).Raw(cell.Value.Span);
                });
                Elements(sequence.Deleted, w);
                break;
            case CrdtField.EphemeralMap presence:
                w.Ordinal(5).I64(presence.Horizon.ToUnixTimeTicks())
                    .Rows(toSeq(presence.Cells.AsIterable()
                        .OrderBy(static row => row.Key.ToString("N"), StringComparer.Ordinal)), static (row, x) => {
                            x.String(row.Key.ToString("N"));
                            row.Value.Cell.CanonicalBytes(x);
                            if (row.Value.Value is PresenceValue.Live live) {
                                x.Bool(true).Ordinal(live.State.Length).Raw(live.State.Span);
                            }
                            else { x.Bool(false); }
                        });
                break;
        }
    }

    static void Elements(Set<ElementId> elements, CanonicalWriter writer) =>
        writer.Rows(toSeq(elements.Order()), static (element, held) => Element(element, held));

    static void Element(ElementId element, CanonicalWriter writer) =>
        writer.String(element.Origin.ToString("N")).I64(unchecked((long)element.Logical));

    public static Fin<ParityVector> Contribute(ParitySlot slot, ReadOnlyMemory<byte> canonical) =>
        slot.MintedHere
            ? Fin.Fail<ParityVector>(new CommitFault.OwnerMinted(slot.Key))
            : Fin.Succ(ParityVector.Of(slot, canonical));

    public static Fin<HashMap<ParitySlot, ParityVector>> Mint(Hlc cell, Seq<UInt128> parents, Seq<UInt128> opKeys, string branch, VersionVector vector, string actor, CommitMessage message, CrdtOp op, params ReadOnlySpan<ParityVector> contributed) {
        HashMap<ParitySlot, ParityVector> foreign = LanguageExt.Iterable<ParityVector>.FromSpan(contributed)
            .Fold(HashMap<ParitySlot, ParityVector>(), static (corpus, vector) => corpus.AddOrUpdate(vector.Slot, vector));
        return (Cell(cell), CommitPreimage(parents, opKeys, branch, vector, actor, cell, message), Op(op))
            .Apply((hlc, commit, encoded) => foreign.Fold(
                HashMap((ParitySlot.HlcCell, hlc), (ParitySlot.CommitKey, commit), (ParitySlot.CrdtOp, encoded)),
                static (corpus, contributedVector) => corpus.AddOrUpdate(contributedVector.Key, contributedVector.Value)))
            .As();
    }

    public static Validation<Error, Unit> Reconcile(HashMap<ParitySlot, ParityVector> local, HashMap<ParitySlot, ParityVector> golden) =>
        toSeq(golden.AsIterable()).Traverse(slot => local.Find(slot.Key) is { IsSome: true, Case: ParityVector held } && held.Holds(slot.Value)
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new CommitFault.ParityDrift(slot.Key.Key, slot.Value.Slot.Producer))).As().Map(static _ => unit);
}
```

| [INDEX] | [POLICY]         | [VALUE]                                | [BINDING]                                              |
| :-----: | :--------------- | :------------------------------------- | :----------------------------------------------------- |
|  [01]   | HLC stamp source | Marten event `Timestamp` cell          | one `Hlc` for op-log, CRDT merge, commit cell, wire    |
|  [02]   | wire schema      | generated required protobuf oneof      | corpus descriptor is the sole ten-arm authority        |
|  [03]   | content key      | exact held proto-binary payload bytes  | verify before parse; never parse-reserialize           |
|  [04]   | restore guard    | byte ceiling + parse + protovalidate   | malformed, unset, unknown arm, and drift refuse typed  |
|  [05]   | parity corpus    | generated value parity + held-byte key | semantic parity never asserts canonical protobuf bytes |
|  [06]   | contribution     | `Contribute` refuses `MintedHere`      | Query supplies `elementset`; no reverse derivation     |
|  [07]   | fault band       | `[FaultCase]` roster, band 8260        | codes 8260-8264 seal off `Fault`                       |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
