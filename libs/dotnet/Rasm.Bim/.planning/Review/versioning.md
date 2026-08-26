# [BIM_VERSIONING]

The content-addressed model-history owner over the shared `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`: one `BimCommit` commit object whose identity IS the `Review/diff#MODEL_DIFF` `ElementFingerprint` set it carries, the `BimBranch` working ref, the in-memory `BimRepository` commit-DAG threading commits by `ParentKeys`, and the `ModelHistory` three-way merge algebra reconciling two divergent revisions against their merge-base into a merged fingerprint graph plus a closed `MergeConflict` `[Union]` the `Review/coordination#SIGN_OFF` `SignOff` resolves. A commit is the fingerprint set, a diff is two commits, and a merge folds two fingerprint streams against a per-element reduction of the merge-base antichain — so the commit-DAG is the branching counterpart to the linear `Review/diff#AUDIT` `AuditTrail` Merkle chain, the two sharing the one content-key idiom and neither re-derived from the other.

Identity is the `Review/diff#MODEL_DIFF` `ElementFingerprint.GlobalId` reused verbatim — the Bim-stored `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` (the IFC `GlobalId` [H6]) where present, the neutral kernel `NodeId` string the `ModelDiff.Fingerprint` fold falls back to off the federation surface, so a commit over the WORKING graph captures an authored element carrying no IFC `GlobalId` yet instead of dropping it, two federation surfaces carrying one `GlobalId` fault the commit typed rather than silently collapsing to one fingerprint, and a parallel commit-local fingerprint or a second identity scheme is the deleted form. The `CommitKey` composes the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` + the shared `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` over the lineage-plus-fingerprint preimage — the ONE canonical codec and the ONE seed-zero `XxHash128` hasher the node/edge/graph addresses ride — so a hand-rolled UTF-8 string-join keyed through a second `XxHash128` is the deleted form, and the key EXCLUDES the author/message/`Instant` so a re-commit of the identical model and lineage is genuinely idempotent while a retroactive content edit mints a divergent key.

The durable commit-DAG storage, the governed branch ACL/tag refs, and the maximal-antichain merge-base are the `Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner's — this owner produces the host-neutral content-addressed commit objects and the merge algebra Persistence stores by the same `CommitKey` and bases against the durable `CommitGraph.MergeBase` antichain, the in-memory `BimRepository` being the transient working DAG (the commit-DAG counterpart of the shared `Rasm.Element/Graph/delta#GRAPH_DELTA` `WorkingGraph`), never a durable store. The page composes the `Review/diff#MODEL_DIFF` `ElementFingerprint`/`ModelDiff.Fingerprint`, the shared `ElementGraph`/`Node`/`NodeId`/`ContentAddress`, the shared `QuikGraph` commit-DAG walks, and the `Review/coordination#SIGN_OFF` `SignOff` as settled vocabulary. The page is HOST-LOCAL — conflict resolution is the coordination `SignOff`'s concern and never an auto-resolve, and a versioning rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE.

## [01]-[INDEX]

- [02]-[VERSION_GRAPH]: `BimCommit` the content-addressed commit, `BimBranch` the working ref, `MergeSide` the two-row branch-side vocabulary, the in-memory `BimRepository` commit-DAG with the `QuikGraph`-folded `Commit`/`History`/`MergeBases`/`Merge`/`CommitMerge`, the `MergeConflict` `[Union]`, the `MergeOutcome`, and the `ModelHistory` three-way merge algebra (antichain base reduction + content-and-placement divergence) over the `Review/diff#MODEL_DIFF` `ElementFingerprint`.

## [02]-[VERSION_GRAPH]

- Owner: `BimCommit` the immutable commit object carrying the shared `ContentAddress CommitKey` content identity, the `Seq<ContentAddress> ParentKeys` lineage (empty for a root, one for a linear commit, two-or-more for a merge), the `Map<string, ElementFingerprint> Fingerprints` keyed by each `ElementFingerprint.GlobalId` [H6], and the author/message/capture `Instant`/`Option<FidelityLog>` carried metadata — the ingest exchange's `Projection/fidelity#FIDELITY_LEDGER` drop ledger seals beside the version it produced, evidence never identity; `BimBranch` the thin in-memory working ref pointing one branch name at its head `CommitKey` (the durable governed ref — `RefKind`/ACL/upstream/annotated-tag — is the `Rasm.Persistence/Version/commits#COMMIT_DAG` `BranchRef`'s); `MergeSide` the `[SmartEnum<string>]` two-row branch-side vocabulary every side-bearing conflict and fold reads, so no boolean carries a polarity only its parameter name explains; `BimRepository` the in-memory working commit-DAG carrying the `Map<ContentAddress, BimCommit>` commit set and the `Map<string, BimBranch>` branch refs, owning `Commit`/`History`/`MergeBases`/`Merge`/`CommitMerge` over the `QuikGraph` lineage; `MergeConflict` the closed `[Union]` of the five irreconcilable divergences the three-way merge surfaces for sign-off; `MergeOutcome` the merge result carrying the auto-merged `Map<string, ElementFingerprint>` and the `Seq<MergeConflict>` the `SignOff` resolves; `ModelHistory` the static three-way merge algebra over the content-keyed fingerprint maps.
- Cases: `MergeConflict` arms `BothModified` (one `GlobalId` whose `ContentKey` diverged from the merge-base on BOTH branches — `Base`/`Ours`/`Theirs` full `ElementFingerprint`s plus the `Review/diff#MODEL_DIFF` `AspectDelta` set naming WHICH members moved) · `PlacementDiverged` (one `GlobalId` both branches RELOCATED divergently while the content converged — the same three-fingerprint payload; the placement conflict a `ContentKey`-only merge silently auto-merged) · `ModifiedAndRemoved` (one `GlobalId` one branch removed while the other modified it past the base — `Base`, `Surviving`, `MergeSide RemovedBy`) · `AddedTwiceDivergent` (one `GlobalId` both branches added with no common ancestor and divergent signatures) · `CrissCross` (one `GlobalId` whose antichain bases DISAGREE, so the element has no single ancestor — `Seq<ElementFingerprint> Bases` beside the two `Option`-carried sides) (5) — a content divergence, a placement divergence, a delete/edit race, a no-base divergent add, and an ambiguous-ancestry element are the five irreducible element-granularity merge shapes, each declaring its `GlobalId` through an ABSTRACT base accessor its leaf overrides and the full per-side `ElementFingerprint`s (diff's currency verbatim — a decomposed `ContentAddress` subset dropping the second axis is the deleted form), never a per-kind conflict class; a `BimCommit` carries its lineage arity in `ParentKeys` rather than a per-kind record — `IsRoot` reads the empty parent set, `IsMerge` the multi-parent set — so a root, a linear, and an octopus-merge commit are one record discriminated by the parent count.
- Entry: `BimRepository.Commit(ElementGraph graph, string branch, string author, string message, Instant at, Op key, Option<FidelityLog> fidelity = default, Option<BimHooks> hooks = default)` seals the element graph as a `BimCommit` child of the branch's current head (a fresh branch roots with no parent), advancing the in-memory branch ref — an UNCHANGED model no-ops, the entry-level idempotency that stops a federation sync loop appending no-change commits every tick — `Fin<T>` refusing `BimFault.Refused` with `BimReason.Rejected` (`commit-duplicate-globalid`) BARE when two federation surfaces carry one IFC `GlobalId`: the shared keys nodes by `NodeId` and imposes no `ExternalId` uniqueness, so the `GlobalId`-keyed commit admits that federation-identity invariant EXACTLY ONCE at its own ingress; `BimRepository.Of(Seq<BimCommit> commits, Seq<BimBranch> branches, Op key)` is the bulk working-set admission — wire-received or store-loaded sets dedup commits by content key and refuse a duplicated branch name (`repository-branch-duplicate`) or a head the set never declares (`repository-branch-absent`) typed, while out-of-set PARENTS stay legal so a shallow working set truncates conservatively; `BimRepository.Merge(ContentAddress ours, ContentAddress theirs, Func<string, ImmutableArray<AspectDelta>> deltas, Op key)` guards the lineage acyclic through `IsDirectedAcyclicGraph` (`merge-lineage-cyclic` — content-addressing makes an honest cycle unrepresentable, so a cycle proves forged `CommitKey`s), resolves both heads, bases on the whole `MergeBases` ANTICHAIN so `CrissCross` is reachable from the in-memory entry under one law with the durable `CommitGraph.MergeBase`, threads the member-delta lookup a `BothModified` conflict carries, and folds the three-way `MergeOutcome`, refusing `BimFault.Refused` with `BimReason.DanglingReference` (`merge-commit-absent`) BARE when either head names a commit the working set never declares; `BimRepository.CommitMerge(MergeOutcome resolved, Seq<ContentAddress> parents, string branch, string author, string message, Instant at, Op key, Option<BimHooks> hooks = default)` seals a CLEAN (or sign-off-resolved) outcome as a merge commit whose parent arity rides the `Seq`, refusing `merge-parent-arity` on fewer than two distinct parents, `merge-commit-absent` on a parent the working set never declares, and `merge-unresolved-conflicts` on an outcome still carrying unresolved `Conflicts` so a conflicted merge never auto-commits; `ModelHistory.Merge(BimCommit ours, BimCommit theirs, Seq<BimCommit> bases, Func<string, ImmutableArray<AspectDelta>> deltas)` is the host-neutral three-way merge — it reduces the merge-base antichain per `GlobalId` then folds the `MergeOutcome`.
- Auto: `Commit` reads the branch head as the parent, folds `graph.ObjectNodes` into the `Map<string, ElementFingerprint>` through `ModelDiff.Fingerprint` (no second fingerprint), derives the `CommitKey` over the parents-plus-fingerprint-set preimage through `ContentAddress.Of` (excluding the carried metadata for idempotency), and advances the branch ref in one `with` projection; `Sealed` normalizes `ParentKeys` sorted-distinct — the durable `CommitNode` `SortedDistinctParents` canon mirrored — so a reordered or duplicated parent seq seals one commit and `IsMerge` reads true arity; `History(head)` and `MergeBases(ours, theirs)` share ONE `Ancestry` kernel folding the ancestor-ward child→parent lineage into a `QuikGraph` `BidirectionalGraph` walked by `BreadthFirstSearchAlgorithm` under the shipped `VertexDistanceRecorderObserver` scoped by its own `Attach` `IDisposable`, the unit-relaxer distance giving generation depth so the closure reads level-ordered and deterministic; `MergeBases` answers the MINIMAL common-ancestor antichain — every common ancestor that no other common ancestor descends from — so a clean history yields one base and a criss-cross two or more, the SAME shape the durable `CommitGraph.MergeBase` publishes, sound over the multi-parent DAG where the retired Tarjan `OfflineLeastCommonAncestor` was not (its input contract is a rooted TREE: the DFS tree of a merge diamond keeps one in-edge per merge commit, hides the other lineage, and mis-bases a post-merge query at the root); `ModelHistory.Merge` reduces the base antichain PER ELEMENT — where the bases AGREE the single signature IS that element's base, a DISAGREEMENT surfacing `CrissCross` carrying every candidate beside the two sides — then folds the union of the three commits' `GlobalId` keys, resolving each id against its base so an id changed on only one side takes that side, an id changed convergently takes the value, a content divergence surfaces `BothModified`, a content-converged placement divergence surfaces `PlacementDiverged`, a removal honored against an unchanged side drops, and a removal racing a modification surfaces `ModifiedAndRemoved` — the merge weighing BOTH the `ContentKey` and the `PlacementKey`.
- Output: the `BimCommit` is the content-addressed history object the `Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner stores by `CommitKey` (the same content-key the `Exchange/wire#WIRE_PROJECTION` `IfcWire` face and the `Review/diff#MODEL_DIFF` carry) carrying the ingest exchange's `FidelityLog` beside its fingerprints — a federation audit reads which counted semantic drops produced the version it pulls — the `BimRepository` the in-memory working graph a federation branches and reconciles before Persistence durably stores the commits, and the `MergeOutcome` the typed three-way merge evidence the coordination `SignOff` advances through `Open → InProgress → Resolved → Closed` as each conflict is settled; the commit-DAG, the linear `Review/diff#AUDIT` `AuditTrail`, and the `IfcWire` face read the one content-key space, never a parallel version identity.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, QuikGraph, LanguageExt.Core, Rasm.Element, Rasm, BCL `System.Collections.Immutable`
- Growth: a wider commit lineage arity (an octopus merge of three branches) is one longer `ParentKeys` set on the same record and one longer base antichain `ModelHistory.Merge` already reduces, never a new commit type; a new conflict kind is one `MergeConflict` union arm the three-way fold surfaces; a richer conflict payload is one column on its arm fed from evidence the composing path already holds, never a second diff re-derived here; history REWRITE is not this owner's — `Rasm.Persistence/Version/commits#COMMIT_DAG` publishes the append-only `HistoryRewrite` family (`Revert`/`CherryPick`/`Rebase`) through its one `Rewrite` entry and this owner grows only the merge fold those verbs consume; the durable DAG storage, the governed `BranchRef`, and the antichain merge-base ride the Persistence ripple; never a per-commit-kind record, never a second identity scheme, and never an auto-resolved conflict.
- Boundary: `BimRepository.Seal` is the ONE point both `Commit` and `CommitMerge` funnel through, so it is the sole fire site for the `Model/observability#HOOKS` `rasm.bim.review.committed` point with `BimFact.Committed` — `CommitKey`, the sorted-distinct parent keys as a `ContentKeySet`, the advanced branch name, and the fingerprint count off the sealed result — the unchanged-model no-op returning the head without reaching `Seal` and therefore firing nothing, the CloudEvents announcement being `Exchange/events#EVENT_PROJECTION`'s observe subscription over that point and a direct message-envelope mint here the deleted form. The commit and merge key on the `Review/diff#MODEL_DIFF` `ElementFingerprint.GlobalId` and a parallel commit-local fingerprint or a second identity scheme is the deleted form — the `ElementFingerprint` is diff's, reused verbatim. The `CommitKey` composes the kernel `CanonicalWriter` + the shared `ContentAddress` (the ONE codec, the ONE seed-zero `XxHash128`) so a delimiter-forgeable `Encoding.UTF8.GetBytes(string.Join(...))` preimage keyed through a second hasher is the named defect `Projection/address#CONTENT_ADDRESS` closes, and the preimage excludes the author/message/`Instant`/`Fidelity` carried metadata so a re-commit of the identical model and lineage is idempotent. The commit fingerprints the shared `ElementGraph.ObjectNodes` and a `BimModel.Elements` fold over the retired parallel element record is the deleted form; it admits the `GlobalId`-uniqueness invariant the contract does not carry, faulting a federation duplicate typed instead of throwing through `ToMap` or last-wins-dropping a real element. `History`/`MergeBases` fold a transient `QuikGraph` graph through ONE `BreadthFirstSearchAlgorithm` `Ancestry` kernel per the shared `libs/dotnet/.api/api-quikgraph.md` substrate, ordered by the shipped `VertexDistanceRecorderObserver` under its own `Attach` scope — a raw `DiscoverVertex` `+=`/`-=` pair over a mutable `List` is the deleted form, because a detach spelled as a statement outlives the walk on any early exit. The merge-base is the minimal-common-ancestor ANTICHAIN over two such closures and a single-nearest election is the deleted form that silently reduces ambiguous ancestry to one arbitrary base — the exact election `CrissCross` exists to refuse — and BOTH prior walk forms are rejected: the hand-rolled visited-set walk over a `Map<>` adjacency, and Tarjan `OfflineLeastCommonAncestor` over the multi-parent DAG. The in-memory `BimRepository` is the TRANSIENT working DAG and the durable commit-DAG store, the governed `BranchRef`, and the maximal-antichain merge-base are the Persistence owner's — a durable store minted here is the named contract violation, this owner joining at the `Review/versioning → Rasm.Persistence/Version/commits # [CONTENT_KEY]: BimCommit content-addressed commit-DAG` contract by the `CommitKey` and at the `# [SHAPE]: BimCommit DAG common-ancestor merge substrate` contract where `ModelHistory.Merge` bases against `CommitGraph.MergeBase`. The three-way merge reuses the `ElementFingerprint` verbatim and weighs BOTH axes so a divergent relocation surfaces `PlacementDiverged` rather than being silently auto-merged, and a field-by-field element comparison beside the fingerprint is the deleted form; an element whose antichain bases DISAGREE has no single ancestor and surfaces `CrissCross` carrying every candidate, while the strictly better recursive pairwise base merge needs ancestor commits this host-neutral algebra does not hold and belongs to the durable owner if it ever lands. The commit and parent keys are typed shared `ContentAddress` values end to end — the `.Value` unwrap that stored a raw `UInt128` erased the one content-key type `Review/diff#AUDIT` states its keys in. This owner is the FINGERPRINT-altitude federation/IFC three-way merge, distinct from the `Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` `StructuralMerge.ThreeWay` FOREST-altitude merge over the full topology, neither re-cased as the other; conflict resolution is the `SignOff` lifecycle's concern, so `CommitMerge` rejecting an unresolved outcome is the law and a silent last-write-wins merge is the deleted form. The page is HOST-LOCAL — a `BimCommit` carries host-free content keys and takes a neutral NodaTime `Instant` off the threaded `IClock` rather than the app-platform `ClockPolicy` an AEC-domain owner never references — and a versioning rejection raises its `BimFault.Refused` value carrying its closed scope and reason and lifts `BimFault` BARE.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using LanguageExt;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Bim.Model;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using Op = Rasm.Domain.Op;
using BimHooks = Rasm.Domain.HookSet<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class MergeSide {
    public static readonly MergeSide Ours = new("ours");
    public static readonly MergeSide Theirs = new("theirs");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BimCommit(
    ContentAddress CommitKey,
    Seq<ContentAddress> ParentKeys,
    Map<string, ElementFingerprint> Fingerprints,
    string Author,
    string Message,
    Instant At,
    Option<FidelityLog> Fidelity) {
    public bool IsRoot => ParentKeys.IsEmpty;
    public bool IsMerge => ParentKeys.Count > 1;

    public static Fin<BimCommit> Of(ElementGraph graph, Seq<ContentAddress> parents, string author, string message, Instant at, Op key, Option<FidelityLog> fidelity = default) {
        Seq<ElementFingerprint> prints = graph.ObjectNodes.Map(o => ModelDiff.Fingerprint(graph, o));
        Seq<string> collided = toSeq(prints.GroupBy(static fp => fp.GlobalId, StringComparer.Ordinal).Where(static g => g.Skip(1).Any()).Select(static g => g.Key));
        return collided.IsEmpty
            ? Fin.Succ(Sealed(parents, prints.Map(static fp => (fp.GlobalId, fp)).ToMap(), author, message, at, fidelity))
            : Fin.Fail<BimCommit>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "duplicate-globalid", "commit", string.Join(',', collided) })));
    }

    public static BimCommit Sealed(Seq<ContentAddress> parents, Map<string, ElementFingerprint> fingerprints, string author, string message, Instant at, Option<FidelityLog> fidelity = default) {
        Seq<ContentAddress> lineage = toSeq(parents.Distinct().OrderBy(static k => k.Value));
        return new(KeyOf(lineage, fingerprints), lineage, fingerprints, author, message, at, fidelity);
    }

    static ContentAddress KeyOf(Seq<ContentAddress> parents, Map<string, ElementFingerprint> fingerprints) =>
        ContentAddress.Of(ContentHash.Of((Parents: parents, Fingerprints: fingerprints), static (state, w) => {
            w.Ordinal(state.Parents.Count);
            foreach (ContentAddress p in state.Parents) { w.U128(p.Value); }
            w.Ordinal(state.Fingerprints.Count);
            foreach (var (id, fp) in state.Fingerprints.OrderBy(static e => e.Key, StringComparer.Ordinal)) {
                w.String(id).U128(fp.ContentKey.Value).U128(fp.PlacementKey.Value);
            }
        }));
}

public sealed record BimBranch(string Name, ContentAddress Head);

[Union]
public abstract partial record MergeConflict {
    private MergeConflict() { }

    public abstract string GlobalId { get; }

    public sealed record BothModified(string GlobalId, ElementFingerprint Base, ElementFingerprint Ours, ElementFingerprint Theirs, ImmutableArray<AspectDelta> Deltas) : MergeConflict;
    public sealed record PlacementDiverged(string GlobalId, ElementFingerprint Base, ElementFingerprint Ours, ElementFingerprint Theirs) : MergeConflict;
    public sealed record ModifiedAndRemoved(string GlobalId, ElementFingerprint Base, ElementFingerprint Surviving, MergeSide RemovedBy) : MergeConflict;
    public sealed record AddedTwiceDivergent(string GlobalId, ElementFingerprint Ours, ElementFingerprint Theirs) : MergeConflict;
    public sealed record CrissCross(string GlobalId, Seq<ElementFingerprint> Bases, Option<ElementFingerprint> Ours, Option<ElementFingerprint> Theirs) : MergeConflict;
}

public sealed record MergeOutcome(Map<string, ElementFingerprint> Merged, Seq<MergeConflict> Conflicts) {
    public bool IsClean => Conflicts.IsEmpty;
}

public sealed record BimRepository(Map<ContentAddress, BimCommit> Commits, Map<string, BimBranch> Branches) {
    public static readonly BimRepository Empty = new(Map<ContentAddress, BimCommit>(), Map<string, BimBranch>());

    public static Fin<BimRepository> Of(Seq<BimCommit> commits, Seq<BimBranch> branches, Op key) {
        Map<ContentAddress, BimCommit> declared = commits.Fold(Map<ContentAddress, BimCommit>(), static (map, c) => map.AddOrUpdate(c.CommitKey, c));
        Seq<string> duplicated = toSeq(branches.GroupBy(static b => b.Name, StringComparer.Ordinal).Where(static g => g.Select(static b => b.Head).Distinct().Skip(1).Any()).Select(static g => g.Key));
        Seq<string> dangling = branches.Filter(b => !declared.ContainsKey(b.Head)).Map(static b => b.Name).Distinct();
        return (duplicated.IsEmpty, dangling.IsEmpty) switch {
            (false, _) => Fin.Fail<BimRepository>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "repository-branch-duplicate", string.Join(',', duplicated) }))),
            (_, false) => Fin.Fail<BimRepository>(new BimFault.Refused(key, BimScope.Review, BimReason.DanglingReference, string.Join(':', new object?[] { "repository-branch-absent", string.Join(',', dangling) }))),
            _          => Fin.Succ(new BimRepository(declared, branches.Fold(Map<string, BimBranch>(), static (map, b) => map.AddOrUpdate(b.Name, b)))),
        };
    }

    public Option<BimCommit> Find(ContentAddress key) => Commits.Find(key);
    public Option<BimBranch> Branch(string name) => Branches.Find(name);

    public Fin<(BimRepository Repository, BimCommit Commit)> Commit(
        ElementGraph graph, string branch, string author, string message, Instant at, Op key,
        Option<FidelityLog> fidelity = default, Option<BimHooks> hooks = default) =>
        BimCommit.Of(graph, Branches.Find(branch).Map(static b => Seq(b.Head)).IfNone(Seq<ContentAddress>()), author, message, at, key, fidelity)
            .Map(commit => Branches.Find(branch).Bind(b => Commits.Find(b.Head)).Filter(head => head.Fingerprints == commit.Fingerprints).Match(
                Some: head => (this, head),
                None: () => Seal(commit, branch, key, hooks)));

    public Seq<BimCommit> History(ContentAddress head) => Ancestry(Lineage(), head).Choose(Commits.Find);

    public Seq<ContentAddress> MergeBases(ContentAddress ours, ContentAddress theirs) => MergeBases(Lineage(), ours, theirs);

    static Seq<ContentAddress> MergeBases(BidirectionalGraph<ContentAddress, SEdge<ContentAddress>> lineage, ContentAddress ours, ContentAddress theirs) {
        LanguageExt.HashSet<ContentAddress> theirAncestry = toHashSet(Ancestry(lineage, theirs));
        Seq<ContentAddress> common = Ancestry(lineage, ours).Filter(theirAncestry.Contains);
        LanguageExt.HashSet<ContentAddress> shadowed = toHashSet(
            common.Bind(candidate => Ancestry(lineage, candidate).Filter(reached => reached != candidate)));
        return common.Filter(candidate => !shadowed.Contains(candidate));
    }

    public Fin<MergeOutcome> Merge(ContentAddress ours, ContentAddress theirs, Func<string, ImmutableArray<AspectDelta>> deltas, Op key) =>
        from lineage in Fin.Succ(Lineage())
        from _ in guard(lineage.IsDirectedAcyclicGraph(), () => (Error)new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "merge-lineage-cyclic" })))
        from o in Commits.Find(ours).ToFin(new BimFault.Refused(key, BimScope.Review, BimReason.DanglingReference, string.Join(':', new object?[] { "merge-commit-absent", "ours", ours.Value.ToString("X32", CultureInfo.InvariantCulture) })))
        from t in Commits.Find(theirs).ToFin(new BimFault.Refused(key, BimScope.Review, BimReason.DanglingReference, string.Join(':', new object?[] { "merge-commit-absent", "theirs", theirs.Value.ToString("X32", CultureInfo.InvariantCulture) })))
        let bases = MergeBases(lineage, ours, theirs).Choose(Commits.Find)
        select ModelHistory.Merge(o, t, bases, deltas);

    public Fin<(BimRepository Repository, BimCommit Commit)> CommitMerge(
        MergeOutcome resolved, Seq<ContentAddress> parents, string branch, string author, string message, Instant at, Op key,
        Option<BimHooks> hooks = default) =>
        (Absent: parents.Filter(p => !Commits.ContainsKey(p)), resolved.IsClean) switch {
            _ when parents.Distinct().Count < 2 => Fin.Fail<(BimRepository, BimCommit)>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "merge-parent-arity", parents.Distinct().Count.ToString(CultureInfo.InvariantCulture) }))),
            ({ IsEmpty: false } absent, _)      => Fin.Fail<(BimRepository, BimCommit)>(new BimFault.Refused(key, BimScope.Review, BimReason.DanglingReference, string.Join(':', new object?[] { "merge-commit-absent", "parent", string.Join(',', absent.Map(static p => p.Value.ToString("X32", CultureInfo.InvariantCulture))) }))),
            (_, false)                          => Fin.Fail<(BimRepository, BimCommit)>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "merge-unresolved-conflicts", resolved.Conflicts.Count.ToString(CultureInfo.InvariantCulture) }))),
            _                                   => Fin.Succ(Seal(BimCommit.Sealed(parents, resolved.Merged, author, message, at), branch, key, hooks)),
        };

    (BimRepository Repository, BimCommit Commit) Seal(BimCommit commit, string branch, Op key, Option<BimHooks> hooks) {
        BimRepository advanced = Advance(branch, commit);
        ignore(hooks.Map(live => live.Fire(BimPoint.Committed, new BimFact.Committed(
            Key: key,
            CommitKey: commit.CommitKey.Value,
            Parents: ContentKeySet.Of(commit.ParentKeys.Map(static parent => parent.Value)),
            Branch: branch,
            Elements: commit.Fingerprints.Count), key)));
        return (advanced, commit);
    }

    BimRepository Advance(string branch, BimCommit commit) => this with {
        Commits = Commits.AddOrUpdate(commit.CommitKey, commit),
        Branches = Branches.AddOrUpdate(branch, new BimBranch(branch, commit.CommitKey)),
    };

    static Seq<ContentAddress> Ancestry(BidirectionalGraph<ContentAddress, SEdge<ContentAddress>> lineage, ContentAddress head) {
        if (!lineage.ContainsVertex(head)) { return Seq<ContentAddress>(); }
        BreadthFirstSearchAlgorithm<ContentAddress, SEdge<ContentAddress>> bfs = new(lineage);
        VertexDistanceRecorderObserver<ContentAddress, SEdge<ContentAddress>> depths = new(static _ => 1.0);
        using (depths.Attach(bfs)) { bfs.Compute(head); }
        return Seq(head) + toSeq(depths.Distances
            .Where(entry => entry.Key != head)
            .OrderBy(static entry => entry.Value)
            .ThenBy(static entry => entry.Key.Value)
            .Select(static entry => entry.Key));
    }

    BidirectionalGraph<ContentAddress, SEdge<ContentAddress>> Lineage() {
        BidirectionalGraph<ContentAddress, SEdge<ContentAddress>> dag = new(allowParallelEdges: false);
        foreach (BimCommit commit in Commits.Values) {
            dag.AddVertex(commit.CommitKey);
            foreach (ContentAddress parent in commit.ParentKeys) {
                dag.AddVerticesAndEdge(new SEdge<ContentAddress>(commit.CommitKey, parent));
            }
        }
        return dag;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ModelHistory {
    public static MergeOutcome Merge(BimCommit ours, BimCommit theirs, Seq<BimCommit> bases, Func<string, ImmutableArray<AspectDelta>> deltas) =>
        toSeq(ours.Fingerprints.Keys.Concat(theirs.Fingerprints.Keys).Concat(bases.Bind(static b => b.Fingerprints.Keys.ToSeq()))
                .Distinct().OrderBy(static id => id, StringComparer.Ordinal))
            .Fold(new MergeOutcome(Map<string, ElementFingerprint>(), Seq<MergeConflict>()), (acc, id) => {
                Seq<ElementFingerprint> candidates = BaseOf(bases, id);
                var (conflict, keep) = candidates.Count > 1
                    ? Ambiguous(id, ours.Fingerprints.Find(id), theirs.Fingerprints.Find(id), candidates)
                    : Resolve(id, ours.Fingerprints.Find(id), theirs.Fingerprints.Find(id), candidates.Head, deltas);
                return new MergeOutcome(
                    keep.Match(Some: fp => acc.Merged.AddOrUpdate(id, fp), None: () => acc.Merged),
                    conflict.Match(Some: c => acc.Conflicts.Add(c), None: () => acc.Conflicts));
            });

    static Seq<ElementFingerprint> BaseOf(Seq<BimCommit> bases, string id) =>
        bases.Choose(commit => commit.Fingerprints.Find(id)).Distinct();

    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) Ambiguous(
        string id, Option<ElementFingerprint> ours, Option<ElementFingerprint> theirs, Seq<ElementFingerprint> bases) =>
        ours.Equals(theirs)
            ? (Option<MergeConflict>.None, ours)
            : (Some<MergeConflict>(new MergeConflict.CrissCross(id, bases, ours, theirs)), Option<ElementFingerprint>.None);

    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) Resolve(
        string id, Option<ElementFingerprint> ours, Option<ElementFingerprint> theirs, Option<ElementFingerprint> mergeBase,
        Func<string, ImmutableArray<AspectDelta>> deltas) =>
        ours.Match(
            Some: o => theirs.Match(
                Some: t => BothSides(id, o, t, mergeBase, deltas),
                None: () => mergeBase.Match(
                    Some: b => RemovedVsModified(id, o, b, MergeSide.Theirs),
                    None: () => (Option<MergeConflict>.None, Some(o)))),
            None: () => theirs.Match(
                Some: t => mergeBase.Match(
                    Some: b => RemovedVsModified(id, t, b, MergeSide.Ours),
                    None: () => (Option<MergeConflict>.None, Some(t))),
                None: () => (Option<MergeConflict>.None, Option<ElementFingerprint>.None)));

    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) BothSides(
        string id, ElementFingerprint ours, ElementFingerprint theirs, Option<ElementFingerprint> mergeBase,
        Func<string, ImmutableArray<AspectDelta>> deltas) {
        bool oursChanged = mergeBase.Match(Some: b => !Same(b, ours), None: static () => true);
        bool theirsChanged = mergeBase.Match(Some: b => !Same(b, theirs), None: static () => true);
        return (oursChanged, theirsChanged) switch {
            (false, _)                => (Option<MergeConflict>.None, Some(theirs)),
            (_, false)                => (Option<MergeConflict>.None, Some(ours)),
            _ when Same(ours, theirs) => (Option<MergeConflict>.None, Some(ours)),
            _                         => (Some(Divergence(id, ours, theirs, mergeBase, deltas)), Option<ElementFingerprint>.None),
        };
    }

    static MergeConflict Divergence(string id, ElementFingerprint ours, ElementFingerprint theirs, Option<ElementFingerprint> mergeBase,
        Func<string, ImmutableArray<AspectDelta>> deltas) =>
        mergeBase.Match(
            Some: b => ours.ContentKey != theirs.ContentKey
                ? (MergeConflict)new MergeConflict.BothModified(id, b, ours, theirs, deltas(id))
                : new MergeConflict.PlacementDiverged(id, b, ours, theirs),
            None: () => new MergeConflict.AddedTwiceDivergent(id, ours, theirs));

    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) RemovedVsModified(
        string id, ElementFingerprint surviving, ElementFingerprint mergeBase, MergeSide removedBy) =>
        Same(surviving, mergeBase)
            ? (Option<MergeConflict>.None, Option<ElementFingerprint>.None)
            : (Some<MergeConflict>(new MergeConflict.ModifiedAndRemoved(id, mergeBase, surviving, removedBy)), Option<ElementFingerprint>.None);

    static bool Same(ElementFingerprint a, ElementFingerprint b) => a.ContentKey == b.ContentKey && a.PlacementKey == b.PlacementKey;
}
```

## [03]-[RESEARCH]

(none)
