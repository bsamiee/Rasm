# [BIM_VERSIONING]

The content-addressed model-history owner: one `BimCommit` commit object whose identity IS the `Review/diff#MODEL_DIFF` `ElementFingerprint` set it carries, a `BimBranch` named ref, the `BimRepository` commit-DAG threading commits by `ParentKeys`, and the `Version.Merge` three-way fold reconciling two divergent revisions against their common ancestor into a merged fingerprint graph plus a closed `MergeConflict` `[Union]` (`BothModified`/`ModifiedAndRemoved`/`AddedTwiceDivergent`) the `Review/coordination#SIGN_OFF` `SignOff` resolves. A commit is the fingerprint set, a diff is two commits, and a merge folds two `ElementChange` streams against a common ancestor — so the commit-DAG is the branching counterpart to the linear `Review/diff#AUDIT` `AuditTrail` Merkle chain, the two sharing the one content-key idiom and neither re-derived from the other. The page composes the `Review/diff#MODEL_DIFF` `ElementFingerprint`/`ModelDiff` algebra, the `Model/elements#ELEMENT_MODEL` `BimModel`, and the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` `XxHash128.HashToUInt128` content-key as settled vocabulary; the durable DAG storage is the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner's, this owner producing the content-addressed commit objects and the merge algebra it stores by the same content-key the wire and diff carry. The page is HOST-LOCAL — conflict resolution is the coordination `SignOff`'s concern, never an auto-resolve, and the merge reuses the `Review/diff#MODEL_DIFF` content-key, never a second identity scheme.

## [01]-[INDEX]

- [01]-[VERSION_GRAPH]: `BimCommit` content-addressed commit record, `BimBranch` ref, the `BimRepository` commit-DAG with `Commit`/`History`/`CommonAncestor` folds, the `MergeConflict` `[Union]`, the `MergeOutcome` receipt, and the `Version.Merge` three-way fold over `ElementFingerprint`.

## [02]-[VERSION_GRAPH]

- Owner: `BimCommit` the immutable commit object carrying the `UInt128 CommitKey` content identity, the `Seq<UInt128> ParentKeys` lineage (empty for a root, one for a normal commit, two for a merge), the `Map<string, ElementFingerprint> Fingerprints` the `Review/diff#MODEL_DIFF` fingerprint of every element keyed by `GlobalId`, the author/message, and the capture `Instant` — the commit identity derives from its content (fingerprints plus parents plus author/message/instant) through the same `XxHash128.HashToUInt128` idiom the `Review/diff#AUDIT` chain keys on, so a retroactive content edit mints a divergent `CommitKey` and a re-commit of the identical model and lineage is idempotent; `BimBranch` the named ref pointing one branch name at its head `CommitKey`; `BimRepository` the commit-DAG owner carrying the `Map<UInt128, BimCommit>` commit store and the `Map<string, BimBranch>` branch refs, owning `Commit` (seal a `BimModel` as a child of the branch head), `History` (the ancestor walk), and `CommonAncestor` (the merge-base over the DAG); `MergeConflict` the closed `[Union]` of the three irreconcilable divergences the three-way merge surfaces for sign-off; `MergeOutcome` the merge receipt carrying the auto-merged `Map<string, ElementFingerprint>` and the `Seq<MergeConflict>` the `SignOff` resolves; `Version` the static three-way merge fold over the content-keyed fingerprint maps.
- Cases: `MergeConflict` arms `BothModified` (one `GlobalId` whose `ContentKey` diverged from the merge-base on BOTH branches to different keys — `BaseKey`, `OursKey`, `TheirsKey`) · `ModifiedAndRemoved` (one `GlobalId` one branch removed while the other modified it past the base — `BaseKey`, `SurvivingKey`, `bool RemovedByOurs`) · `AddedTwiceDivergent` (one `GlobalId` both branches added with no common ancestor and divergent `ContentKey` — `OursKey`, `TheirsKey`) (3); a `BimCommit` carries its lineage arity in `ParentKeys` rather than a per-kind record — `IsRoot` reads the empty parent set, `IsMerge` reads the two-parent set — so a root, a linear, and a merge commit are one record discriminated by the parent count, never a `RootCommit`/`MergeCommit` class family.
- Entry: `BimRepository.Commit(BimModel model, string branch, string author, string message, ClockPolicy clocks)` seals the model as a `BimCommit` child of the branch's current head (a fresh branch roots with no parent), advancing the branch ref — total, no rail, a commit is one expression over the model's fingerprint set; `BimRepository.Merge(UInt128 ours, UInt128 theirs)` resolves the merge-base and folds the three-way `MergeOutcome`, `Fin<T>` aborting when either head names a commit the store never declares (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`) lowered with `.ToError()`; `BimRepository.CommitMerge(MergeOutcome resolved, UInt128 ours, UInt128 theirs, string branch, string author, string message, ClockPolicy clocks)` seals a CLEAN (or sign-off-resolved) outcome as a two-parent merge commit, `Fin<T>` rejecting an outcome still carrying unresolved `Conflicts` (`BimFault.ModelRejected`) so a conflicted merge never auto-commits; `Version.Merge(BimCommit ours, BimCommit theirs, BimCommit mergeBase)` is the total three-way fold producing the `MergeOutcome`.
- Auto: `Commit` reads the branch head as the parent (a `Seq<UInt128>` of zero or one), folds `model.Elements` into the `Map<string, ElementFingerprint>` through the `Review/diff#MODEL_DIFF` `ModelDiff.Fingerprint(element)` (no second fingerprint), derives the `CommitKey` over the parents-plus-fingerprints-plus-author/message/instant content, and advances the branch ref to the new commit in one `with` projection; `History(head)` is the breadth-first ancestor walk over `ParentKeys` threading an immutable visited-set fold so a merge commit's two lineages converge without re-visiting; `CommonAncestor(ours, theirs)` collects every ancestor of `ours` once through `History`, then walks `theirs`'s ancestry breadth-first returning the first commit in the `ours` ancestor set — the lowest common ancestor the three-way merge diffs each side against; `Version.Merge` folds the union of the three commits' `GlobalId` keys, resolving each id by comparing its per-side `ElementFingerprint.ContentKey` to the merge-base — an id changed on only one side takes that side, an id changed convergently (equal `ContentKey`) on both takes the convergent value, an id changed divergently surfaces a typed `MergeConflict`, a removal honored against an unchanged side drops, and a removal racing a modification surfaces `ModifiedAndRemoved` — so the merge reads the `Review/diff#MODEL_DIFF` content-key the same way the pairwise diff does, the auto-merge landing every non-conflicting element and the `MergeConflict` set carrying only the genuine divergences for `Review/coordination#SIGN_OFF`.
- Receipt: the `BimCommit` is the content-addressed history object the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner stores by `CommitKey` (the same content-key the `Exchange/wire#WIRE_PROJECTION` `OpLog` face and the `Review/diff#MODEL_DIFF` carry), the `BimRepository` the branch/merge graph a federation branches and reconciles, and the `MergeOutcome` the typed three-way merge evidence — the auto-merged fingerprint graph plus the `MergeConflict` set the coordination `SignOff` advances through `Open → Resolved → Closed` as each conflict is settled; the commit-DAG, the linear `Review/diff#AUDIT` `AuditTrail`, and the `Exchange/wire#WIRE_PROJECTION` `OpLog` face read the one content-key space, never a parallel version identity.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm
- Growth: a new commit lineage arity (an octopus merge of three branches) is one longer `ParentKeys` set on the same record, never a new commit type; a new conflict kind (a both-moved-divergent placement conflict over the `PlacementKey`) is one `MergeConflict` union arm the three-way fold surfaces; a new branch operation (a rebase, a cherry-pick) is one fold over the same `ParentKeys` DAG reusing `Version.Merge`; the durable DAG storage rides the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` ripple; never a per-commit-kind record, never a second identity scheme, and never an auto-resolved conflict.
- Boundary: the commit identity is the `Review/diff#MODEL_DIFF` `ElementFingerprint` set keyed through the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` `XxHash128.HashToUInt128` idiom and a second commit-hash or a `Guid`-keyed DAG is the named drift defect — a commit is the fingerprint set, a diff is two commits, a merge folds two `ElementChange` streams against a common ancestor; the three-way merge reuses the `Review/diff#MODEL_DIFF` content-key verbatim and a field-by-field element comparison beside the fingerprint is the deleted form; conflict resolution is the `Review/coordination#SIGN_OFF` `SignOff` lifecycle's concern — the `Version.Merge` fold surfaces the typed `MergeConflict` set and never auto-resolves a divergence, so `CommitMerge` rejecting an unresolved outcome is the law and a silent last-write-wins merge is the deleted form; the `MergeConflict` is a closed `[Union]` and a per-kind conflict class is the deleted form; the durable commit-DAG storage is the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner's concern joined at the `Review/versioning → csharp:Rasm.Persistence/Version/commits # [CONTENT_KEY]: BimCommit content-addressed commit-DAG` seam by the `CommitKey` — Persistence stores the `BimCommit` as a generic `CommitNode` under the wire-carried `CommitKey` and never re-derives the key — and a durable store minted here is the named seam violation: this owner produces the content-addressed commit objects and the merge algebra, the durable DAG riding the Persistence ripple; the commit-DAG and the linear `Review/diff#AUDIT` `AuditTrail` are distinct owners over the one content-key — the audit trail is the per-element who/when/what linear Merkle chain, the commit-DAG the branching revision graph, neither re-cased as the other; the `Exchange/wire#WIRE_PROJECTION` `OpLog` face carries the `ElementChange` op rows the `csharp:Rasm.Persistence/Sync` op-stream CRDT convergence reconciles, while the `BimCommit` DAG common-ancestor merge substrate is the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` `CommitGraph.MergeBase` the durable owner bases three-way merges against at the `Review/versioning → csharp:Rasm.Persistence/Version/commits # [SHAPE]: BimCommit DAG common-ancestor merge substrate` seam, this owner producing the host-neutral commit objects and the three-way merge the durable owner stores and bases; the page is HOST-LOCAL — a `BimCommit` carries the host-free `ElementFingerprint` content-keys, never a RhinoCommon type or a host-bound geometry; a versioning rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO.Hashing;
using System.Text;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NodaTime;
using Rasm;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record BimCommit(
    UInt128 CommitKey,
    Seq<UInt128> ParentKeys,
    Map<string, ElementFingerprint> Fingerprints,
    string Author,
    string Message,
    Instant At) {
    public static readonly BimCommit Empty = new(UInt128.Zero, Seq<UInt128>(), Map<string, ElementFingerprint>(), "", "", Instant.MinValue);

    public bool IsRoot => ParentKeys.IsEmpty;
    public bool IsMerge => ParentKeys.Count > 1;

    public static BimCommit Of(BimModel model, Seq<UInt128> parents, string author, string message, Instant at) =>
        Sealed(parents, model.Elements.ToMap(static e => e.GlobalId, static e => ModelDiff.Fingerprint(e)), author, message, at);

    // The commit identity is content-addressed over the lineage + the diff fingerprint set + the
    // authorship — the same XxHash128 idiom the Review/diff#AUDIT chain keys on, so a re-commit of the
    // identical model and lineage is idempotent and a retroactive content edit mints a divergent key.
    public static BimCommit Sealed(Seq<UInt128> parents, Map<string, ElementFingerprint> fingerprints, string author, string message, Instant at) =>
        new(KeyOf(parents, fingerprints, author, message, at), parents, fingerprints, author, message, at);

    static UInt128 KeyOf(Seq<UInt128> parents, Map<string, ElementFingerprint> fingerprints, string author, string message, Instant at) =>
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join("|",
            string.Join(",", parents.Map(static p => p.ToString())),
            string.Join(";", fingerprints.OrderBy(static p => p.Key).Map(static p => $"{p.Key}={p.Value.ContentKey}:{p.Value.PlacementKey}")),
            author, message, at.ToUnixTimeTicks().ToString())));
}

public sealed record BimBranch(string Name, UInt128 Head);

[Union]
public partial record MergeConflict {
    partial record BothModified(string GlobalId, UInt128 BaseKey, UInt128 OursKey, UInt128 TheirsKey);
    partial record ModifiedAndRemoved(string GlobalId, UInt128 BaseKey, UInt128 SurvivingKey, bool RemovedByOurs);
    partial record AddedTwiceDivergent(string GlobalId, UInt128 OursKey, UInt128 TheirsKey);

    public string GlobalId => Switch(
        bothModified:        static c => c.GlobalId,
        modifiedAndRemoved:  static c => c.GlobalId,
        addedTwiceDivergent: static c => c.GlobalId);
}

public sealed record MergeOutcome(Map<string, ElementFingerprint> Merged, Seq<MergeConflict> Conflicts) {
    public bool IsClean => Conflicts.IsEmpty;
}

public sealed record BimRepository(Map<UInt128, BimCommit> Commits, Map<string, BimBranch> Branches) {
    public static readonly BimRepository Empty = new(Map<UInt128, BimCommit>(), Map<string, BimBranch>());

    public Option<BimCommit> Find(UInt128 key) => Commits.Find(key);
    public Option<BimBranch> Branch(string name) => Branches.Find(name);

    public (BimRepository Repository, BimCommit Commit) Commit(BimModel model, string branch, string author, string message, ClockPolicy clocks) {
        var parents = Branches.Find(branch).Map(static b => Seq(b.Head)).IfNone(Seq<UInt128>());
        var commit = BimCommit.Of(model, parents, author, message, clocks.Now);
        return (Advance(branch, commit), commit);
    }

    public Seq<BimCommit> History(UInt128 head) => Walk(Seq(head), toHashSet<UInt128>(), Seq<BimCommit>());

    Seq<BimCommit> Walk(Seq<UInt128> frontier, HashSet<UInt128> seen, Seq<BimCommit> chain) =>
        frontier.HeadOrNone().Match(
            None: () => chain,
            Some: key => seen.Contains(key)
                ? Walk(frontier.Tail, seen, chain)
                : Commits.Find(key).Match(
                    Some: commit => Walk(frontier.Tail.Concat(commit.ParentKeys), seen.Add(key), chain.Add(commit)),
                    None: () => Walk(frontier.Tail, seen.Add(key), chain)));

    // The merge-base: collect every ancestor of `ours` once, then walk `theirs`'s ancestry breadth-first
    // returning the first commit in the `ours` ancestor set — the lowest common ancestor the three-way
    // merge diffs each side against. No common ancestor (unrelated histories) folds against BimCommit.Empty.
    public Option<UInt128> CommonAncestor(UInt128 ours, UInt128 theirs) =>
        Nearest(Seq(theirs), toHashSet<UInt128>(), toHashSet(History(ours).Map(static c => c.CommitKey)));

    Option<UInt128> Nearest(Seq<UInt128> frontier, HashSet<UInt128> seen, HashSet<UInt128> target) =>
        frontier.HeadOrNone().Match(
            None: () => Option<UInt128>.None,
            Some: key => target.Contains(key) ? Some(key)
                : seen.Contains(key) ? Nearest(frontier.Tail, seen, target)
                : Commits.Find(key).Match(
                    Some: commit => Nearest(frontier.Tail.Concat(commit.ParentKeys), seen.Add(key), target),
                    None: () => Nearest(frontier.Tail, seen.Add(key), target)));

    public Fin<MergeOutcome> Merge(UInt128 ours, UInt128 theirs) =>
        from o in Commits.Find(ours).ToFin(new BimFault.DanglingReference($"merge-commit-absent:{ours}").ToError())
        from t in Commits.Find(theirs).ToFin(new BimFault.DanglingReference($"merge-commit-absent:{theirs}").ToError())
        let mergeBase = CommonAncestor(ours, theirs).Bind(Commits.Find).IfNone(BimCommit.Empty)
        select Version.Merge(o, t, mergeBase);

    public Fin<(BimRepository Repository, BimCommit Commit)> CommitMerge(
        MergeOutcome resolved, UInt128 ours, UInt128 theirs, string branch, string author, string message, ClockPolicy clocks) =>
        resolved.IsClean
            ? FinSucc(Seal(BimCommit.Sealed(Seq(ours, theirs), resolved.Merged, author, message, clocks.Now), branch))
            : FinFail<(BimRepository, BimCommit)>(new BimFault.ModelRejected($"merge-unresolved-conflicts:{resolved.Conflicts.Count}").ToError());

    (BimRepository Repository, BimCommit Commit) Seal(BimCommit commit, string branch) => (Advance(branch, commit), commit);

    BimRepository Advance(string branch, BimCommit commit) => this with {
        Commits = Commits.AddOrUpdate(commit.CommitKey, commit),
        Branches = Branches.AddOrUpdate(branch, new BimBranch(branch, commit.CommitKey)),
    };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Version {
    // The three-way fold over the content-keyed fingerprint maps: each GlobalId across ours/theirs/base
    // resolves by comparing its per-side ContentKey to the base. Only one side changed → take that side;
    // both changed convergently → take it; both changed divergently → a typed MergeConflict the coordination
    // SignOff resolves. Reuses Review/diff#MODEL_DIFF ElementFingerprint verbatim — no second identity.
    public static MergeOutcome Merge(BimCommit ours, BimCommit theirs, BimCommit mergeBase) =>
        ours.Fingerprints.Keys.Concat(theirs.Fingerprints.Keys).Concat(mergeBase.Fingerprints.Keys).Distinct().ToSeq()
            .Fold(new MergeOutcome(Map<string, ElementFingerprint>(), Seq<MergeConflict>()), (acc, id) => {
                var (conflict, keep) = Resolve(id, ours.Fingerprints.Find(id), theirs.Fingerprints.Find(id), mergeBase.Fingerprints.Find(id));
                return new MergeOutcome(
                    keep.Match(Some: fp => acc.Merged.AddOrUpdate(id, fp), None: () => acc.Merged),
                    conflict.Match(Some: c => acc.Conflicts.Add(c), None: () => acc.Conflicts));
            });

    // Resolve one GlobalId across the three sides into (conflict?, keep?): keep=Some lands the merged
    // fingerprint, keep=None is a converged removal, conflict=Some routes the divergence to SignOff. The
    // key comparison is the Review/diff content-key so a relocation and a property edit read identically.
    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) Resolve(
        string id, Option<ElementFingerprint> ours, Option<ElementFingerprint> theirs, Option<ElementFingerprint> mergeBase) =>
        (ours.IsSome, theirs.IsSome, mergeBase.IsSome) switch {
            (true,  true,  _)     => BothSides(id, ours.ValueUnsafe(), theirs.ValueUnsafe(), mergeBase),
            (false, true,  true)  => RemovedVsModified(id, theirs.ValueUnsafe(), mergeBase.ValueUnsafe(), removedByOurs: true),
            (true,  false, true)  => RemovedVsModified(id, ours.ValueUnsafe(), mergeBase.ValueUnsafe(), removedByOurs: false),
            (true,  false, false) => (Option<MergeConflict>.None, ours),
            (false, true,  false) => (Option<MergeConflict>.None, theirs),
            _                     => (Option<MergeConflict>.None, Option<ElementFingerprint>.None),
        };

    static (Option<MergeConflict>, Option<ElementFingerprint>) BothSides(
        string id, ElementFingerprint ours, ElementFingerprint theirs, Option<ElementFingerprint> mergeBase) {
        var baseKey = mergeBase.Map(static f => f.ContentKey);
        bool oursChanged = baseKey.Map(k => k != ours.ContentKey).IfNone(true);
        bool theirsChanged = baseKey.Map(k => k != theirs.ContentKey).IfNone(true);
        return (oursChanged, theirsChanged) switch {
            (false, _)                                  => (Option<MergeConflict>.None, Some(theirs)),
            (_, false)                                  => (Option<MergeConflict>.None, Some(ours)),
            _ when ours.ContentKey == theirs.ContentKey => (Option<MergeConflict>.None, Some(ours)),
            _ => mergeBase.Match(
                Some: b  => (Some((MergeConflict)new MergeConflict.BothModified(id, b.ContentKey, ours.ContentKey, theirs.ContentKey)), Option<ElementFingerprint>.None),
                None: () => (Some((MergeConflict)new MergeConflict.AddedTwiceDivergent(id, ours.ContentKey, theirs.ContentKey)), Option<ElementFingerprint>.None)),
        };
    }

    static (Option<MergeConflict>, Option<ElementFingerprint>) RemovedVsModified(
        string id, ElementFingerprint surviving, ElementFingerprint mergeBase, bool removedByOurs) =>
        surviving.ContentKey == mergeBase.ContentKey
            ? (Option<MergeConflict>.None, Option<ElementFingerprint>.None)
            : (Some((MergeConflict)new MergeConflict.ModifiedAndRemoved(id, mergeBase.ContentKey, surviving.ContentKey, removedByOurs)), Option<ElementFingerprint>.None);
}
```

## [03]-[RESEARCH]

- [COMMIT_DAG]: the `BimCommit` content-addressed commit object reuses the `Review/diff#MODEL_DIFF` `ElementFingerprint(GlobalId, ContentKey, PlacementKey)` verbatim — the commit is the per-element fingerprint set, so a commit is the fingerprint map, a diff is two commits (`ModelDiff.Between` over the two snapshots the commits fingerprint), and a merge folds two `ElementChange` streams against a common ancestor; the `CommitKey` derives through the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` `XxHash128.HashToUInt128` idiom over the parents-plus-fingerprints-plus-authorship content (BCL `System.IO.Hashing` inbox, settled at `.api/api-hashing`) the same way the `Review/diff#AUDIT` `AuditTrail.EntryKey` chains, so the commit-DAG and the linear audit chain share the one content-key space and a second commit-hash scheme is the named drift defect; the `BimRepository` `History`/`CommonAncestor` are immutable breadth-first folds over the `ParentKeys` DAG threading a visited set, never a mutable graph walk, and a merge commit's two parents converge through the visited-set fold.
- [THREE_WAY_MERGE]: the `Version.Merge` fold is the standard three-way reconciliation over the content-keyed fingerprint maps — `CommonAncestor` yields the merge-base, and each `GlobalId` resolves by comparing its per-side `ElementFingerprint.ContentKey` against the base (an id changed on one side takes that side, a convergent change on both takes the value, a divergent change surfaces `MergeConflict.BothModified`, a remove racing a modification surfaces `MergeConflict.ModifiedAndRemoved`, and a no-common-ancestor divergent add surfaces `MergeConflict.AddedTwiceDivergent`); the merge reuses the `Review/diff#MODEL_DIFF` content-key so a relocation (`PlacementKey` delta) and a property edit (`ContentKey` delta) read identically to the pairwise diff, and the `MergeConflict` set is resolved by the `Review/coordination#SIGN_OFF` `SignOff` lifecycle — `Version.Merge` never auto-resolves, `BimRepository.CommitMerge` rejecting an outcome still carrying `Conflicts` per the IDEAS `[VERSIONED]` "conflict resolution is the coordination sign-off's concern, not an auto-resolve" law.
- [PERSISTENCE_SEAM]: the durable commit-DAG storage is the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner's concern joined at the `Review/versioning → csharp:Rasm.Persistence/Version/commits # [CONTENT_KEY]: BimCommit content-addressed commit-DAG` seam by the `CommitKey` — Persistence stores the `BimCommit` as a generic `CommitNode` under the wire-carried `CommitKey` without re-deriving it, this owner producing the content-addressed commit objects and the merge algebra, the durable DAG riding the Persistence ripple; the `Review/versioning → csharp:Rasm.Persistence/Version/commits # [SHAPE]: BimCommit DAG common-ancestor merge substrate` seam bases this owner's `CommonAncestor` on the durable `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` `CommitGraph.MergeBase` maximal common-ancestor antichain (the durable superset of this owner's single-nearest in-memory base), while the `Exchange/wire#WIRE_PROJECTION` `OpLog` face's `ElementChange` op rows ride the `csharp:Rasm.Persistence/Sync` op-stream CRDT convergence so the commit-DAG, the linear `Review/diff#AUDIT` `AuditTrail`, and the `OpLog` wire face all read the one content-key space; the `Review/diff#MODEL_DIFF` `ElementFingerprint`/`ModelDiff` algebra, the `Model/elements#ELEMENT_MODEL` `BimModel`, and the `Review/coordination#SIGN_OFF` `SignOff` are composed as settled vocabulary, Bim minting no second identity scheme, no durable store, and no auto-resolve.
