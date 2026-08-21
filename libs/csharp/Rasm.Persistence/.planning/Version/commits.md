# [PERSISTENCE_VERSION_COMMITS]

`CommitGraph` owns content-addressed history, ref policy, vector order, merge bases, anti-entropy ranges, and append-only rewrites. `Crdt` owns the convergent field algebra; `CrdtWire` owns its bounded MessagePack encoding; `Hlc` supplies their shared causal cell. `ContentParityCorpus` derives every local fixture from the live writer and accepts foreign fixtures only through `Contribute`. Marten supplies the append substrate, `OpLogEntry` supplies the changefeed's message envelope, `GrantSet` supplies branch authorization, and `ContentHash.Of` supplies cross-runtime identity.

## [01]-[INDEX]

- [02]-[COMMIT_DAG]: content-addressed commit-DAG with commit messages, named branches, annotated tags, maximal-antichain merge-base, and version vectors.
- [03]-[CRDT_ALGEBRA]: RGA, OR-set, MV-register, PN-counter, LWW, and ephemeral-presence convergent CRDT.
- [04]-[CRDT_WIRE]: HLC stamp, `CrdtOp` codec, `CrdtOpWire` op-log payload amendment, and the cross-runtime parity corpus.

## [02]-[COMMIT_DAG]

- Owner: `CommitNode` the content-addressed commit record carrying its `CommitMessage`; `RefCapability`/`RefPolicy` the ref-class vocabulary and its three legal corners; `RefKind` the ref-class axis; `BranchRef` the named-ref pointer with a per-branch `Element/authority#GRANT_ALGEBRA` `GrantSet` ACL (the branch-lane narrowing of the one object-authorization vocabulary, never the disjoint AppHost `Capability`), upstream tracking, and annotated-tag payload; `VersionVector` the per-origin sequence map owning the ONE canonical slot order and the ONE canonical vector writer every byte-deriving reader takes; `MerkleRange` the reconciliation node; `HistoryRewrite` the append-only rewrite request family with `RewriteSeam` its delegate frame; `CommitGraph` the static surface owning hash, parent links, the maximal-antichain merge base, vector compare, the Merkle range fold, the recursive anti-entropy descent, and the one polymorphic `Rewrite` entry.
- Cases: `RefKind` is `Branch | LightweightTag | AnnotatedTag | RemoteTracking`, each holding its `RefCapability` corner — `{Mutable}`, `{}`, `{Annotated}` — so an annotated-yet-mutable ref is unrepresentable rather than merely unwritten; `CommitGraph.Order` compares two `VersionVector` values into `Before | After | Concurrent | Equal`; `MerkleRange` is `Empty | Bounded`, so an empty digest carries no fabricated low/high key, and `CommitGraph.Reconcile` recursively bisects only divergent bounded subranges; `HistoryRewrite` closes at `Revert | CherryPick | Rebase`, every case an append-only mint through the one `Commit` writer.
- Entry: `CommitGraph.Commit(parents, inherited, opKeys, branch, origin, actor, cell, message)` is a pure value whose content key is the kernel `ContentHash.Of` over the canonical `(parent-count, SortedDistinctParents, op-count, SortedOpKeys, Branch, VersionVector, Actor, Hlc, CommitMessage)` preimage, the vector advanced on the COMMITTING origin's slot — the writer's store id off the session, never `branch.Origin`, which names the ref's minting peer and collapses every writer on one branch into one causal slot; `MergeBase(resolve, left, right)` returns the maximal common-ancestor antichain nearest-first; `AdvanceDemand(commit, head)` derives the branch-advance authorization as a total `VectorOrder` dispatch; `Reconcile(children, local, remote)` returns the divergent leaf ranges; `Rewrite(...)` is the one polymorphic history-rewrite entry and `RewriteDemand(rewrite)` its gate.

- Auto: a commit appends one `Version/ledger#CHANGEFEED` `OpLogEntry` of `SyncOpKind.Upsert` on the `commit` column family carrying the `CommitNode` payload, so the commit-DAG rides the one changefeed projected off Marten and never a second store; `inherited` is the parent-vector join (`VersionVector.Join` is the per-slot max) advanced by the committed op count on the COMMITTING origin's slot (the `origin` parameter — two writers on one branch occupy two distinct slots, so `Order` reads their concurrency truthfully), so a merge commit's vector dominates both parents; `MerkleRange.Of` folds a sorted content-key window into a digest so anti-entropy compares top-down and transfers only the divergent subtree.
- Receipt: a commit rides `ReceiptSinkPort` under `store.commit`; a branch mutation rides `store.branch`; the range-reconciliation transfer count rides `SyncApplyReceipt`.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` — the one federation hasher over the commit preimage), System.IO.Hashing (`XxHash128.Append`/`GetCurrentHashAsUInt128` — the incremental `MerkleRange` peer digest only, never a content-key mint), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new ref kind is one `RefKind` row; a new ACL grant is one `Element/authority#GRANT_ALGEBRA` `Grant` row the `GrantSet` admits (never a local flag); a richer commit header is one field on `CommitMessage`/`CommitNode`; a new rewrite verb is one `HistoryRewrite` case with one `Rewrite` arm and its `RewriteDemand` grant row; zero new surface — a parallel commit store, a second DAG walker, or a git-shaped object database is the deleted form because the commit rides the changefeed and the content address rides `ContentAddress`; a domain-minted commit (the `csharp:Rasm.Bim` `BimCommit`) is one wire-sourced `CommitNode` stored under its carried content key, never re-minted.
- Boundary: `VersionVector.Ordered` and `VersionVector.WriteTo` are the vector's own canonical form, hoisted out of `Preimage` so the commit key, the `Version/ledger#CHANGEFEED` `OperationId` key, and the `CrdtOpWire` context arrays all read ONE order — a caller enumerating `Slots` directly writes hash-bucket order, which mints a different digest per runtime and per insertion history for one causal position. Commit keys derive from `ContentHash.Of` over the canonical preimage, and `Commit` distinct-sorts parents so a duplicate-parent or reordered-parent merge converges on one node; a wall-clock or random commit id is the deleted form. Member `CommitMessage` IS in the preimage, so re-wording mints a fresh node exactly as an amend does in any content-addressed DAG — a preimage omitting an identity member is the split-brain the identity/preimage agreement law forbids.
- Boundary: `MergeBase` is the true merge-base set — reachability intersect minus dominated — computed NEAR-LINEAR: two `Rank` passes for the common set and the nearest-first ordering, then ONE `Reach` pass seeded with every common candidate's parents, whose reached-intersect-common set is exactly the dominated candidates. Per-candidate `Rank` re-walks are the deleted `O(candidates x graph)` form. Clean history yields one base, a criss-cross the two-or-more bases the three-way merge virtualizes, and disjoint histories the empty `Seq`; the `Rasm.Bim` three-way merge is the named consumer.
- Boundary: `VersionVector` is the ONE concurrency primitive — `Order` returns `Concurrent` exactly when neither side dominates — and `AdvanceDemand` is its consumer, deriving the demand off `(IsMerge, Order)` as a TOTAL dispatch so the gate reads a derived `GrantSet` and never a caller-guessed lane. `BranchRef` grants ride the `Element/authority#GRANT_ALGEBRA` `GrantSet` narrowed to the branch lane, NOT the disjoint AppHost effect-gating `Capability` whose name the authority owner forbids re-using across strata. Gating conjoins the mutability precondition with BOTH `GrantSet` sides, and `Admits` is `Admin`-superuser-aware, so a flat `Write`-only gate that lets a read-plus-write actor force-push, and a parallel branch-only enum, are both deleted forms.
- Boundary: every `HistoryRewrite` is APPEND-ONLY — a revert is the inverse delta as a NEW commit, a cherry-pick one commit's ops replayed onto another head, a rebase a sequential replay minting a fresh linear lineage. History never mutates and the source commits stay reachable. This DAG stores op KEYS, so the `RewriteSeam` returns keys and a delta inversion or replay conflict faults on ITS owner's rail BEFORE any commit mints — a half-applied rewrite cannot exist. Mutating rewrites, a ref force-moved without its `RewriteDemand` gate, or a manual counter-edit standing in for a revert is the deleted form.
- Boundary: the durable commit-DAG is where the `csharp:Rasm.Bim` `BimCommit` federates and durably stores — a domain commit crosses at the wire as one `commit`-family `OpLogEntry` and lands as a generic `CommitNode` stored UNDER its wire-carried content key, never re-derived through `Commit`'s native preimage (the Bim key deliberately excludes message for federation-sync idempotency, so the two identity laws never collide), and the Bim three-way merge bases against this owner's `MergeBase` antichain.

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

    // Canonical vector cell: little-endian slot count, then per slot the length-framed 32-char lowercase-N GUID text
    // and its little-endian counter. `CommitGraph.Preimage` inlined this layout; `OperationId.WriteTo` now shares
    // this one writer, so a preimage edit cannot desync the identity key from the commit key.
    public void WriteTo(IBufferWriter<byte> sink) {
        Seq<(Guid Origin, long Seq)> slots = Ordered;
        BinaryPrimitives.WriteInt32LittleEndian(sink.GetSpan(4), slots.Count);
        sink.Advance(4);
        foreach ((Guid origin, long counter) in slots) {
            CommitGraph.Framed(sink, origin.ToString("N"));
            BinaryPrimitives.WriteInt64LittleEndian(sink.GetSpan(8), counter);
            sink.Advance(8);
        }
    }
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

    public static CommitNode Commit(Seq<UInt128> parents, VersionVector inherited, Seq<UInt128> opKeys, BranchRef branch, Guid origin, string actor, Hlc cell, CommitMessage message) {
        Seq<UInt128> parentSet = toSeq(parents.Distinct().OrderBy(static k => k));
        Seq<UInt128> sortedKeys = toSeq(opKeys.OrderBy(static k => k));
        VersionVector vector = inherited.Advance(origin, opKeys.Count);
        ArrayBufferWriter<byte> canonical = new();
        Preimage(canonical, parentSet, sortedKeys, branch.Name, vector, actor, cell, message);
        // `Advance` steps the COMMITTING origin's slot — the writer's store id, never branch.Origin (the ref's
        // minting peer), so two writers on one branch occupy two causal slots and Order reads Concurrent.
        return new CommitNode(ContentHash.Of(canonical.WrittenSpan), parentSet, sortedKeys, branch.Name, vector, actor, cell, message);
    }

    // `Preimage` is the ONE commit-key writer (the parity `commit-key` slot mints through it, never a re-implemented
    // layout): count-framed parents and op keys, branch, lowercase-N GUID vector slots, actor, cell, and message.
    public static void Preimage(IBufferWriter<byte> sink, Seq<UInt128> sortedDistinctParents, Seq<UInt128> sortedOpKeys, string branch, VersionVector vector, string actor, Hlc cell, CommitMessage message) {
        BinaryPrimitives.WriteInt32LittleEndian(sink.GetSpan(4), sortedDistinctParents.Count);
        sink.Advance(4);
        foreach (UInt128 parent in sortedDistinctParents) { BinaryPrimitives.WriteUInt128LittleEndian(sink.GetSpan(16), parent); sink.Advance(16); }
        BinaryPrimitives.WriteInt32LittleEndian(sink.GetSpan(4), sortedOpKeys.Count);
        sink.Advance(4);
        foreach (UInt128 key in sortedOpKeys) { BinaryPrimitives.WriteUInt128LittleEndian(sink.GetSpan(16), key); sink.Advance(16); }
        Framed(sink, branch);
        vector.WriteTo(sink);
        Framed(sink, actor);
        cell.WriteTo(sink);
        Framed(sink, message.Summary);
        Framed(sink, message.Body);
    }

    // Internal, not private: `VersionVector.WriteTo` frames its GUID slot text through this one writer, so the
    // length-prefix convention has a single spelling across the preimage and the identity key.
    internal static void Framed(IBufferWriter<byte> sink, string text) {
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

    public static MerkleRange Of(Seq<UInt128> sortedKeys) {
        using XxHash128 digest = new();
        Span<byte> word = stackalloc byte[16];
        foreach (UInt128 key in sortedKeys) { BinaryPrimitives.WriteUInt128LittleEndian(word, key); digest.Append(word); }
        UInt128 address = digest.GetCurrentHashAsUInt128();
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

- Owner: `CrdtField` `[Union]` the convergent op-based/delta-state field family carrying the six replicated data types; `CrdtOp` the delta payload a changefeed entry carries; `RgaCell` the one growable-array element; `Crdt` the merge-fold surface whose `Merge` is commutative, associative, and idempotent over the op multiset, with the version-vector-gated tombstone compaction.
- Cases: `LwwRegister`, `MvRegister`, `OrSet`, `PnCounter`, `RgaSequence`, `EphemeralMap` on `CrdtField`; `Set | Write | Add | Remove | Increment | InsertAfter | Delete | Maintain | Beat | Leave` on `CrdtOp`.
- Entry: `Crdt.Merge(left, right)` is the join-semilattice least-upper-bound, total over the six cases and idempotent; `Apply(state, op)` folds one delta carrying its HLC cell; `Seed(op)` is the total generated `Switch` materializing a fresh cell's matching empty arm from its FIRST op, so a new op case breaks the build at the genesis; `Compact(state, quiescent, liveness)` reclaims `RgaSequence` tombstones the quiescence horizon proves unreferenceable and evicts `EphemeralMap` entries past the physical liveness deadline; `Law(op)` returns the arm's commutation row, the one input `Version/ledger#MERGE_LAW` reads to tell a lost total order from an idempotent replay.
- Auto: a CRDT mutation rides one `OpLogEntry` carrying the delta as `Payload`, so convergent merge rides the changefeed projected off Marten and a peer's `SyncMerge.Apply` dispatches the `crdt` row into `Crdt.Apply` rather than the LWW scalar. Merging an OR-set takes the per-element tag-set union minus the union of observed-remove tombstones, so add and concurrent remove resolve add-wins. Every RGA element id IS its own order key — the weave groups by causal predecessor, orders same-predecessor siblings descending, and depth-first-linearizes from the sentinel — so two concurrent inserts converge to one order on every peer. PN-counters fold per-origin running totals; the MV-register keeps every value no other value's context dominates; the `EphemeralMap` keeps one entry per origin under strict-HLC supersession, and `Compact` evicts by PHYSICAL instant, never the op-count quiescence horizon.
- Receipt: a converged merge rides `SyncApplyReceipt`; a tombstone, live-element, compacted-tombstone, and live-presence count fold into the `store.crdt.merge` fact.
- Packages: NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, Generator.Equals (`[Equatable]`/`[CustomEquality]` — payload-true state equality), BCL inbox — no hasher: every CRDT identity is the `ElementId` order key or the wire owner's `ContentKey`, so a `System.IO.Hashing` row here is the stale admission the algebra never composes.
- Growth: a new replicated type is one `CrdtField` case, one `CrdtOp` arm, one `Merge`/`Apply` arm, and one `Seed`/`Law` pair the generated total `Switch` forces; zero new surface — a per-type merge service, a second convergence engine, or an op-transform rebase is the deleted form because the join-semilattice subsumes idempotency, commutativity, and reorder tolerance.
- Boundary: `Merge` is a join-semilattice least-upper-bound, so any partition of any permutation of the op multiset applied any number of times converges to identical state — the strict superset of the `Version/ledger#MERGE_LAW` LWW `Adjudicate`, which survives only as the `LwwRegister` arm. Case `RgaSequence` carries tombstones so a deleted slot stays stable for later concurrent inserts, and `Compact` reclaims only when the quiescence horizon dominates the cell's lamport stamp. Merging `OrSet` takes the per-element live-tag union minus both tombstone sets through `Set.Except`; the `MvRegister` is a causal anti-chain; the `PnCounter` is per-origin running totals monotone under sequence-max merge, so a replayed `Increment` is absorbed where a delta-adding fold is the deleted non-idempotent form.
- Boundary: the `EphemeralMap` is per-origin-LWW-by-HLC under add-wins liveness, and `Compact` is the durable-presence distinction — an entry whose last-beat PHYSICAL instant precedes the deadline is a peer that stopped beating, so eviction is convergence-correct and idempotent. Presence liveness is a physical-time horizon distinct from the RGA op-count tombstone-GC horizon, which is why the `Maintain` op carries BOTH. `Crdt.Merge` reads no wall clock: the `Hlc` cell from the Marten stamp is the only ordering input, so convergence is deterministic.
- Boundary: the `(left, right)` and `(state, op)` tuple switches are total on the DIAGONAL and the off-diagonal arm is unreachable by cell-type stability — a `(NodeId, Field)` cell is one fixed `CrdtField` arm for its whole lifetime, since `Seed` materializes the matching empty arm from the first op and a decoded op disagreeing with its cell is contract drift the decode rail already rejected. Arms `_ => left` and `_ => state` are therefore the unreachable totality floor, never a soft fallback hiding type drift. Convergence is OBSERVED as state equality, so every payload-bearing case is `[Equatable]` with `CrdtBytes` on each `ReadOnlyMemory<byte>` member; the carrier stays `ReadOnlyMemory<byte>` because the decode lands zero-copy slices and an `ImmutableArray<byte>` swap re-types the wire-decode seam.

```csharp signature
public readonly record struct ElementId(Guid Origin, ulong Logical) : IComparable<ElementId> {
    public static readonly ElementId Head = new(Guid.Empty, 0UL);
    public int CompareTo(ElementId other) {
        int byLogical = Logical.CompareTo(other.Logical);
        return byLogical != 0 ? byLogical : Origin.CompareTo(other.Origin);
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
public readonly partial record struct RgaCell(ElementId Id, ElementId After, [property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> Value, bool Tombstone) {
    public static readonly ElementId Origin = ElementId.Head;
    // `Tombstone` is the RGA's own monotone state bit, so the LIVE mint is a named factory rather than a bare
    // positional `false` a reader must count arguments to interpret.
    public static RgaCell Live(ElementId id, ElementId after, ReadOnlyMemory<byte> value) => new(id, after, value, Tombstone: false);
}

[Equatable]
public sealed partial record MvEntry([property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> Value, VersionVector Context, Hlc Cell);

[Equatable]
public sealed partial record PresenceCell([property: CustomEquality(typeof(CrdtBytes))] ReadOnlyMemory<byte> State, Hlc Cell);

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
    public sealed record OrSet(HashMap<UInt128, Set<ElementId>> Live, Set<ElementId> Tombstoned) : CrdtField;
    public sealed record PnCounter(HashMap<Guid, (long Sequence, long Positive, long Negative)> Origins) : CrdtField;
    public sealed record RgaSequence(Seq<RgaCell> Cells) : CrdtField;
    public sealed record EphemeralMap(HashMap<Guid, PresenceCell> Live) : CrdtField;
}

public static class Crdt {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.crdt.merge"), StoreSlot.Create("store.crdt.decode"));

    // `Seed` fixes cell-type stability: a fresh (NodeId, Field) cell materializes its CrdtField arm from its FIRST op
    // through the generated total Switch — a new op case breaks the build here, and every later op for that cell
    // hits the fixed diagonal arm in Apply.
    public static CrdtField Seed(CrdtOp op) => op.Switch<CrdtField>(
        set: static _ => new CrdtField.LwwRegister(ReadOnlyMemory<byte>.Empty, Hlc.Zero, Guid.Empty),
        write: static _ => new CrdtField.MvRegister(Seq<MvEntry>()),
        add: static _ => new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), Set<ElementId>()),
        remove: static _ => new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), Set<ElementId>()),
        increment: static _ => new CrdtField.PnCounter(HashMap<Guid, (long Sequence, long Positive, long Negative)>()),
        insertAfter: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
        delete: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
        maintain: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
        beat: static _ => new CrdtField.EphemeralMap(HashMap<Guid, PresenceCell>()),
        leave: static _ => new CrdtField.EphemeralMap(HashMap<Guid, PresenceCell>()));

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
        (CrdtField.MvRegister mv, CrdtOp.Write w) => new CrdtField.MvRegister(AntiChain(mv.Values.Filter(h => !w.Context.Dominates(h.Context)).Add(new MvEntry(w.Value, w.Context, w.Cell)))),
        (CrdtField.OrSet s, CrdtOp.Add add) => new CrdtField.OrSet(s.Live.AddOrUpdate(add.Element, e => e.Add(add.Tag), Set(add.Tag)), s.Tombstoned),
        (CrdtField.OrSet s, CrdtOp.Remove rem) when toSet(rem.ObservedTags) is var observed => new CrdtField.OrSet(s.Live.AddOrUpdate(rem.Element, e => e.Except(observed), Set<ElementId>()).Filter(static t => t.Count > 0), s.Tombstoned.Union(observed)),
        (CrdtField.PnCounter c, CrdtOp.Increment inc) => new CrdtField.PnCounter(
            c.Origins.AddOrUpdate(inc.Origin, held => held.Sequence >= inc.Sequence ? held : (inc.Sequence, inc.Positive, inc.Negative), (inc.Sequence, inc.Positive, inc.Negative))),
        (CrdtField.RgaSequence seq, CrdtOp.InsertAfter ins) => new CrdtField.RgaSequence(Weave(seq.Cells, Seq(RgaCell.Live(ins.Id, ins.After, ins.Value)))),
        (CrdtField.RgaSequence seq, CrdtOp.Delete del) => new CrdtField.RgaSequence(seq.Cells.Map(c => c.Id == del.Id ? c with { Tombstone = true } : c)),
        (CrdtField.RgaSequence seq, CrdtOp.Maintain m) => Compact(seq, m.Quiescent, m.Liveness),
        (CrdtField.EphemeralMap map, CrdtOp.Beat b) => new CrdtField.EphemeralMap(map.Live.AddOrUpdate(b.Origin, h => h.Cell.CompareTo(b.Cell) >= 0 ? h : new PresenceCell(b.State, b.Cell), new PresenceCell(b.State, b.Cell))),
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
    public static Seq<(Guid Origin, ReadOnlyMemory<byte> State)> Live(CrdtField.EphemeralMap map) => toSeq(map.Live.Map(static (o, s) => (o, s.State)).Values);

    // Distinct rides MvEntry's generated equality — CrdtBytes on Value — so a re-decoded duplicate write collapses.
    static Seq<MvEntry> AntiChain(Seq<MvEntry> values) =>
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

Merge policy per mutation kind — `Crdt.Law` returns column three, and `Version/ledger#MERGE_LAW` reads it to separate a lost total order from an idempotent replay:

| [INDEX] | [OP]          | [LAW]       | [CONCURRENT_SIBLING]                     | [CONFLICT_RESOLUTION]                            |
| :-----: | :------------ | :---------- | :--------------------------------------- | :----------------------------------------------- |
|  [01]   | `set`         | Ordered     | genuine conflict — one value loses       | total order on `(Hlc, Origin)`; loser receipted  |
|  [02]   | `write`       | Semilattice | both survive on the causal anti-chain    | none — the reader resolves the multi-value cell  |
|  [03]   | `add`         | Semilattice | tag-set union; tags are unique per add   | add-wins over a `remove` that observed no tag    |
|  [04]   | `remove`      | Semilattice | observed-tag tombstone union             | loses every tag it never observed                |
|  [05]   | `increment`   | Commutative | disjoint per-origin buckets              | unreachable — two origins never share a slot     |
|  [06]   | `insertAfter` | Semilattice | siblings order by descending `ElementId` | deterministic weave; absent predecessor refuses  |
|  [07]   | `delete`      | Semilattice | monotone tombstone flag                  | tombstone wins; the slot stays for later inserts |
|  [08]   | `maintain`    | Semilattice | filter composition — a MEET, not a join  | divergent horizons are one history, two views    |
|  [09]   | `beat`        | Semilattice | per-origin cell; origins are disjoint    | strictly-later `Hlc` supersedes within one slot  |
|  [10]   | `leave`       | Semilattice | per-origin eviction                      | loses to a strictly-later `beat`; equal keeps it |

## [04]-[CRDT_WIRE]

- Owner: `Hlc` the hybrid-logical-clock stamp the Marten event `Timestamp`, the changefeed projection, the CRDT merge, the commit cell, and the wire all read; `CrdtOpWire` the `[MessagePack.Union]` op encoding the `OpLogEntry.Payload` carries for `crdt` rows; `CrdtOpMapper` the ONE generated seam between the two op vocabularies; `CommitFault` the closed `[Union]` fault family over the KERNEL `Rasm.Domain.Fault` in the 8260 band; `CrdtWire` the static codec owning the byte-canonical content key, the `Encode`/`Decode` pair through the package resolver, and the `UntrustedData` restore lane; `ParitySlot` the corpus-leg axis carrying its producer-owner label; `ParityVector` the one fixture carrier whose digest ALWAYS derives through the kernel `ContentHash.Of` at mint; `ContentParityCorpus` the surface minting this package's parity legs and reconciling a local corpus against the golden one.
- Cases: ten op rows — `set | write | add | remove | increment | insertAfter | delete | maintain | beat | leave` — whose `[Key]` sequence IS the wire schema, dense and append-only, a retired key never reassigned; the `beat`/`leave` arms carry the presence delta. Every `ParitySlot` row names its producer owner beside a `MintedHere` stance the `Mint`/`Contribute` split reads: a minted-here row derives from this owner's own writers, a contributed row flows in one-directionally, and the roster alone fixes membership so a new leg falsifies no sentence. Row `elementset` has its `Query/lane#ELEMENT_SET_ALGEBRA` owner calls `Contribute` with its own framed preimage, so the Version owner freezes the foreign byte shape but never reaches back into Query to re-derive it.
- Entry: `CrdtWire.ContentKey(op)` is the byte-canonical content key over the `None`-compression companion encoding; `Encode(op)` writes the durable delta under `Lz4BlockArray` and `EncodeCompanion(op)` the same delta under `None` for the Python and TS consumers; `Decode(payload)` reads under `UntrustedData` with the depth and decompressed-size ceilings, a contract rejection failing the typed `CommitFault.DecodeDrift`; `ContentParityCorpus.Mint(...)` mints every minted-here vector over this page's own writers and folds in the contributed ones; `Contribute(slot, canonical)` is the contribution seam a foreign producer calls, failing `OwnerMinted` on an owner-minted slot; `Reconcile(local, golden)` accumulates every `ParityDrift` the cross-runtime harness finds.
- Auto: `Hlc.Observe` swaps the local cell forward past both the wall clock and the observed remote cell so a received op never rewinds the local logical counter; `CrdtWire.Encode` rides the codec profile so a `CrdtOp` delta crosses as `OpLogEntry.Payload` bytes the snapshot codec already verifies; the wire union and the `CrdtOp` union share one case vocabulary so a new op arm is one wire row, one `CrdtOp` arm, and one map case; `ContentParityCorpus.Mint` seals each owner-local fixture from the SAME writer the live path runs — the HLC cell from `Hlc.WriteTo`, the commit-key preimage from the ONE `CommitGraph.Preimage` writer (never a re-implemented layout), the CRDT-op companion from `CrdtWire.EncodeCompanion` — so a parity fixture is byte-identical to what the live encode produces and every `ParityVector.Of` mint derives its digest through the kernel `ContentHash.Of` (the one seed-zero discipline; a corpus-local seed constant is the deleted form).
- Receipt: an encoded delta carries no receipt (the `OpLogEntry` carries the lane codec, content key, and HLC cell); a decode failure folds into `store.crdt.decode` as the typed `CommitFault.DecodeDrift`; a parity drift folds into the `Reconcile` `Validation` as the accumulated `CommitFault.ParityDrift` cross-runtime mismatch set, never a first-mismatch abort.
- Packages: MessagePack, Thinktecture.Runtime.Extensions.MessagePack, Rasm (`Rasm.Domain` `ContentHash.Of` + `Rasm.Domain.Fault` — the fault-band base), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new op is one `CrdtOpWire` `[MessagePack.Union]` tag, one `[Key]` member, one `CrdtOp` arm, and the mapper's own pair of rows — the union's generated `Switch` breaks the build on the domain side and the `[MapDerivedType]` roster names the wire side; a new parity leg is one `ParitySlot` row with one `Mint` or `Contribute` vector, never a second corpus store or a per-fixture golden-bytes constant family; zero new surface.
- Boundary: this is the flagship `CrdtOpWire` amendment to the one-wire-vocabulary law — `OpLogEntry.Payload` carries a `CrdtOpWire` union for `crdt` rows, LWW `Adjudicate` survives only as the `set` arm reconstructing `LwwRegister`, and the breaking descriptor change is owned at `Rasm.AppHost/Runtime/ports#WIRE_LAW` with the TS-web and Python companions decoding the amended payload. Every `[Key]` sequence obeys the retirement law, so contract drift is a build diagnostic through `MessagePackAnalyzer`.
- Boundary: the `Hlc` layout is the KERNEL's byte-for-byte (`Rasm/Domain/frame#RECEIPT_PORT`) — physical half first as the Unix-tick `long` at one tick per hundred nanoseconds, logical half second as the monotone `ulong`, both little endian — so `WriteTo` emits the canonical sixteen-byte cell the commit key and the op key both hash, and ordering compares causality without a wall clock. `Hlc.Zero` is an in-memory absence value alone, because its physical half is outside the I63 domain the packed slot admits.
- Boundary: the restore lane reads under `UntrustedData` with the object-graph depth ceiling AND a bounded decompressed-size cap, because a synced delta admits a decompression bomb the depth cap alone never catches; the rejection surfaces as the typed `CommitFault.DecodeDrift` on the `Fin` rail. `ContentKey` hashes the `None`-companion canonical bytes, never the at-rest LZ4 framing, so the op key is byte-reproducible across the three runtimes and is the same seed-zero identity the structural diff and the federation keys consume.
- Boundary: `ContentParityCorpus` freezes the cell, the canonical commit-key preimage through the ONE `CommitGraph.Preimage` writer, and the companion op encoding, each as one `ParityVector` whose digest derives through `ContentHash.Of` at mint. Freeze authority rests with the manifest alone — `HLC_TWO_HALF` and `CRDT_OP_SET` stay DESIGN-PIN until the harness proof pins concrete inputs and golden bytes — so this producer derives vectors and never declares them frozen. `Contribute` refuses an owner-minted slot, keeping the dependency one-directional, and `Reconcile` accumulates every drift through `Validation` rather than aborting on the first.

```csharp signature
// Cell layout is the kernel's, byte-for-byte (`Rasm/Domain/frame#RECEIPT_PORT`): physical half FIRST as the Unix-tick
// `long` at one tick per hundred nanoseconds, logical half second as the monotone `ulong`, both little endian. `Zero`
// is an IN-MEMORY absence value alone — `Instant.MinValue`'s ticks are negative, outside the I63 domain the packed
// slot admits, so a `Zero` reaching `WriteTo` writes a cell no peer's mint may produce.
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

    public override string Message => Switch(
        decodeDrift:   static c => $"<crdt-decode-drift:{c.Cause.Message}>",
        rewriteAbsent: static c => $"<rewrite-source-absent:{c.Source:x32}>",
        parityDrift:   static c => $"<parity-drift:{c.Slot}@{c.Producer}>",
        ownerMinted:   static c => $"<parity-owner-mints:{c.Slot}>");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CrdtWire {
    static readonly MessagePackSerializerOptions Write = MessagePackSerializerOptions.Standard
        .WithResolver(CompositeResolver.Create(PersistenceResolver.Instance, StandardResolver.Instance))
        .WithCompression(MessagePackCompression.Lz4BlockArray);
    static readonly MessagePackSerializerOptions Restore = Write.WithSecurity(MessagePackSecurity.UntrustedData.WithMaximumObjectGraphDepth(64).WithMaximumDecompressedSize(1 << 20));
    static readonly MessagePackSerializerOptions Companion = Write.WithCompression(MessagePackCompression.None);

    public static ReadOnlyMemory<byte> Encode(CrdtOp op) => MessagePackSerializer.Serialize(CrdtOpMapper.Wire(op), Write);
    public static ReadOnlyMemory<byte> EncodeCompanion(CrdtOp op) => MessagePackSerializer.Serialize(CrdtOpMapper.Wire(op), Companion);
    public static UInt128 ContentKey(CrdtOp op) => ContentHash.Of(EncodeCompanion(op).Span);

    public static Fin<CrdtOp> Decode(ReadOnlyMemory<byte> payload) =>
        Op.Of().Catch(() => Fin.Succ(CrdtOpMapper.Op(MessagePackSerializer.Deserialize<CrdtOpWire>(payload, Restore))))
            .MapFail(static error => error.Exception.Case is MessagePackSerializationException or FormatterNotRegisteredException
                ? (Error)new CommitFault.DecodeDrift(error)
                : error);
}

// --- [COMPOSITION] -----------------------------------------------------------------------
// ONE mapper owns the whole op seam and zero assignment statements survive. Domain-to-wire crosses through the
// union's GENERATED TOTAL `Switch`, so a new `CrdtOp` case breaks the build; wire-to-domain takes
// `[MapDerivedType]`, the form reserved for an OPEN hierarchy, because `CrdtOpWire` is a MessagePack union with no generated dispatch to
// break. Every shape divergence is one attribute row: the `Hlc` cell and the `ElementId` flatten to their wire halves
// and the vector to its canonical slot array.
[Mapper]
[MapperRequiredMapping(RequiredMappingStrategy.Both)]
public static partial class CrdtOpMapper {
    public static CrdtOpWire Wire(CrdtOp op) => op.Switch<CrdtOpWire>(
        set: static o => Wire(o), write: static o => Wire(o), add: static o => Wire(o), remove: static o => Wire(o),
        increment: static o => Wire(o), insertAfter: static o => Wire(o), delete: static o => Wire(o),
        maintain: static o => Wire(o), beat: static o => Wire(o), leave: static o => Wire(o));

    [MapProperty([nameof(CrdtOp.Set.Cell), nameof(Hlc.Physical)], [nameof(CrdtOpWire.Set.PhysicalTicks)])]
    [MapProperty([nameof(CrdtOp.Set.Cell), nameof(Hlc.Logical)], [nameof(CrdtOpWire.Set.Logical)])]
    private static partial CrdtOpWire.Set Wire(CrdtOp.Set op);

    [MapProperty([nameof(CrdtOp.Write.Cell), nameof(Hlc.Physical)], [nameof(CrdtOpWire.Write.PhysicalTicks)])]
    [MapProperty([nameof(CrdtOp.Write.Cell), nameof(Hlc.Logical)], [nameof(CrdtOpWire.Write.Logical)])]
    private static partial CrdtOpWire.Write Wire(CrdtOp.Write op);

    [MapProperty([nameof(CrdtOp.Add.Tag), nameof(ElementId.Origin)], [nameof(CrdtOpWire.Add.TagOrigin)])]
    [MapProperty([nameof(CrdtOp.Add.Tag), nameof(ElementId.Logical)], [nameof(CrdtOpWire.Add.TagLogical)])]
    private static partial CrdtOpWire.Add Wire(CrdtOp.Add op);

    private static partial CrdtOpWire.Remove Wire(CrdtOp.Remove op);
    private static partial CrdtOpWire.Increment Wire(CrdtOp.Increment op);

    [MapProperty([nameof(CrdtOp.InsertAfter.Predecessor), nameof(ElementId.Origin)], [nameof(CrdtOpWire.InsertAfter.PredOrigin)])]
    [MapProperty([nameof(CrdtOp.InsertAfter.Predecessor), nameof(ElementId.Logical)], [nameof(CrdtOpWire.InsertAfter.PredLogical)])]
    [MapProperty([nameof(CrdtOp.InsertAfter.Id), nameof(ElementId.Origin)], [nameof(CrdtOpWire.InsertAfter.IdOrigin)])]
    [MapProperty([nameof(CrdtOp.InsertAfter.Id), nameof(ElementId.Logical)], [nameof(CrdtOpWire.InsertAfter.IdLogical)])]
    private static partial CrdtOpWire.InsertAfter Wire(CrdtOp.InsertAfter op);

    [MapProperty([nameof(CrdtOp.Delete.Id), nameof(ElementId.Origin)], [nameof(CrdtOpWire.Delete.IdOrigin)])]
    [MapProperty([nameof(CrdtOp.Delete.Id), nameof(ElementId.Logical)], [nameof(CrdtOpWire.Delete.IdLogical)])]
    private static partial CrdtOpWire.Delete Wire(CrdtOp.Delete op);

    [MapProperty(nameof(CrdtOp.Maintain.Liveness), nameof(CrdtOpWire.Maintain.LivenessTicks))]
    private static partial CrdtOpWire.Maintain Wire(CrdtOp.Maintain op);

    [MapProperty([nameof(CrdtOp.Beat.Cell), nameof(Hlc.Physical)], [nameof(CrdtOpWire.Beat.PhysicalTicks)])]
    [MapProperty([nameof(CrdtOp.Beat.Cell), nameof(Hlc.Logical)], [nameof(CrdtOpWire.Beat.Logical)])]
    private static partial CrdtOpWire.Beat Wire(CrdtOp.Beat op);

    [MapProperty([nameof(CrdtOp.Leave.Cell), nameof(Hlc.Physical)], [nameof(CrdtOpWire.Leave.PhysicalTicks)])]
    [MapProperty([nameof(CrdtOp.Leave.Cell), nameof(Hlc.Logical)], [nameof(CrdtOpWire.Leave.Logical)])]
    private static partial CrdtOpWire.Leave Wire(CrdtOp.Leave op);

    [MapDerivedType<CrdtOpWire.Set, CrdtOp.Set>]
    [MapDerivedType<CrdtOpWire.Write, CrdtOp.Write>]
    [MapDerivedType<CrdtOpWire.Add, CrdtOp.Add>]
    [MapDerivedType<CrdtOpWire.Remove, CrdtOp.Remove>]
    [MapDerivedType<CrdtOpWire.Increment, CrdtOp.Increment>]
    [MapDerivedType<CrdtOpWire.InsertAfter, CrdtOp.InsertAfter>]
    [MapDerivedType<CrdtOpWire.Delete, CrdtOp.Delete>]
    [MapDerivedType<CrdtOpWire.Maintain, CrdtOp.Maintain>]
    [MapDerivedType<CrdtOpWire.Beat, CrdtOp.Beat>]
    [MapDerivedType<CrdtOpWire.Leave, CrdtOp.Leave>]
    public static partial CrdtOp Op(CrdtOpWire wire);

    // Reverse mapping UN-flattens into positional record constructors, which no member path expresses, so each
    // composite target member takes one whole-source reader and the RMG020 cost is declared here rather than absorbed
    // silently.
    [MapPropertyFromSource(nameof(CrdtOp.Set.Cell), Use = nameof(Cell))]
    private static partial CrdtOp.Set Op(CrdtOpWire.Set wire);
    [MapPropertyFromSource(nameof(CrdtOp.Write.Cell), Use = nameof(Cell))]
    private static partial CrdtOp.Write Op(CrdtOpWire.Write wire);
    [MapPropertyFromSource(nameof(CrdtOp.Add.Tag), Use = nameof(Tag))]
    private static partial CrdtOp.Add Op(CrdtOpWire.Add wire);
    private static partial CrdtOp.Remove Op(CrdtOpWire.Remove wire);
    private static partial CrdtOp.Increment Op(CrdtOpWire.Increment wire);
    [MapPropertyFromSource(nameof(CrdtOp.InsertAfter.Predecessor), Use = nameof(Predecessor))]
    [MapPropertyFromSource(nameof(CrdtOp.InsertAfter.Id), Use = nameof(Element))]
    private static partial CrdtOp.InsertAfter Op(CrdtOpWire.InsertAfter wire);
    [MapPropertyFromSource(nameof(CrdtOp.Delete.Id), Use = nameof(Element))]
    private static partial CrdtOp.Delete Op(CrdtOpWire.Delete wire);
    [MapProperty(nameof(CrdtOpWire.Maintain.LivenessTicks), nameof(CrdtOp.Maintain.Liveness))]
    private static partial CrdtOp.Maintain Op(CrdtOpWire.Maintain wire);
    [MapPropertyFromSource(nameof(CrdtOp.Beat.Cell), Use = nameof(Cell))]
    private static partial CrdtOp.Beat Op(CrdtOpWire.Beat wire);
    [MapPropertyFromSource(nameof(CrdtOp.Leave.Cell), Use = nameof(Cell))]
    private static partial CrdtOp.Leave Op(CrdtOpWire.Leave wire);

    // `Ordered`, never `Slots`: the companion encoding IS the content-key preimage, so a hash-bucket enumeration
    // gives one causal context two digests and strands the parity fixtures unfreezable.
    [UserMapping] private static (Guid Origin, long Seq)[] Slots(VersionVector vector) => [.. vector.Ordered];
    [UserMapping] private static VersionVector Vector((Guid Origin, long Seq)[] slots) => new(toHashMap(slots));
    [UserMapping] private static (Guid Origin, ulong Logical)[] Tags(Seq<ElementId> tags) => [.. tags.Map(static t => (t.Origin, t.Logical))];
    [UserMapping] private static Seq<ElementId> Tags((Guid Origin, ulong Logical)[] tags) => toSeq(tags).Map(static t => new ElementId(t.Origin, t.Logical));
    [UserMapping] private static long Ticks(Instant at) => at.ToUnixTimeTicks();
    [UserMapping] private static Instant At(long ticks) => Instant.FromUnixTimeTicks(ticks);

    private static Hlc Cell(CrdtOpWire.Set wire) => new(Instant.FromUnixTimeTicks(wire.PhysicalTicks), wire.Logical);
    private static Hlc Cell(CrdtOpWire.Write wire) => new(Instant.FromUnixTimeTicks(wire.PhysicalTicks), wire.Logical);
    private static Hlc Cell(CrdtOpWire.Beat wire) => new(Instant.FromUnixTimeTicks(wire.PhysicalTicks), wire.Logical);
    private static Hlc Cell(CrdtOpWire.Leave wire) => new(Instant.FromUnixTimeTicks(wire.PhysicalTicks), wire.Logical);
    private static ElementId Tag(CrdtOpWire.Add wire) => new(wire.TagOrigin, wire.TagLogical);
    private static ElementId Predecessor(CrdtOpWire.InsertAfter wire) => new(wire.PredOrigin, wire.PredLogical);
    private static ElementId Element(CrdtOpWire.InsertAfter wire) => new(wire.IdOrigin, wire.IdLogical);
    private static ElementId Element(CrdtOpWire.Delete wire) => new(wire.IdOrigin, wire.IdLogical);
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
// `ContentHash.Of` at mint; tests/contracts/MANIFEST.md alone declares a fixture frozen (HLC_TWO_HALF and CRDT_OP_SET
// stay DESIGN-PIN until the harness proof) — an unstamped-Option carrier and a corpus-local seed constant are the
// deleted forms.
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

    // CRDT_OP_SET producer (kernel corpus row [04]): EVERY delivery permutation of the op set folds to one converged
    // state, and the vector's canonical bytes are the converged MvRegister anti-chain in Hlc-cell order — a
    // permutation-dependent fold refutes the algebra and fails the mint instead of pinning a lie.
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
            foreach ((ReadOnlyMemory<byte> value, VersionVector _, Hlc cell) in toSeq(mv.Values.OrderBy(static held => held.Cell))) {
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
        toSeq(golden.AsIterable()).Traverse(slot => local.Find(slot.Key) is { IsSome: true, Case: ParityVector held } && held.Holds(slot.Value)
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new CommitFault.ParityDrift(slot.Key.Key, slot.Value.Slot.Producer))).As().Map(static _ => unit);
}
```

| [INDEX] | [POLICY]         | [VALUE]                                | [BINDING]                                             |
| :-----: | :--------------- | :------------------------------------- | :---------------------------------------------------- |
|  [01]   | HLC stamp source | Marten event `Timestamp` cell          | one `Hlc` for op-log, CRDT merge, commit cell, wire   |
|  [02]   | wire schema      | `[Key]` sequence, append-only          | retired key never reassigned; analyzer gate           |
|  [03]   | content key      | `None`-companion canonical bytes       | byte-reproducible across C#/Python/TS; no at-rest LZ4 |
|  [04]   | restore guard    | `UntrustedData` + depth + size ceiling | decompression bomb stops beyond the depth cap         |
|  [05]   | parity corpus    | kernel `ContentHash.Of` at every mint  | `VERSION_PARITY`; producer-emits gate                 |
|  [06]   | contribution     | `Contribute` refuses `MintedHere`      | Query supplies `elementset`; no reverse derivation    |
|  [07]   | fault band       | `[FaultCase]` roster, band 8260        | codes 8260-8263 seal off `Fault`                      |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
