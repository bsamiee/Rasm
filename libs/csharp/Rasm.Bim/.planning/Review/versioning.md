# [BIM_VERSIONING]

The content-addressed model-history owner over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`: one `BimCommit` commit object whose identity IS the `Review/diff#MODEL_DIFF` `ElementFingerprint` set it carries, the `BimBranch` working ref, the in-memory `BimRepository` commit-DAG threading commits by `ParentKeys`, and the `ModelHistory` three-way merge algebra reconciling two divergent revisions against their merge-base into a merged fingerprint graph plus a closed `MergeConflict` `[Union]` the `Review/coordination#SIGN_OFF` `SignOff` resolves. A commit is the fingerprint set, a diff is two commits, and a merge folds two fingerprint streams against a per-element reduction of the merge-base antichain — so the commit-DAG is the branching counterpart to the linear `Review/diff#AUDIT` `AuditTrail` Merkle chain, the two sharing the one content-key idiom and neither re-derived from the other.

Identity is the `Review/diff#MODEL_DIFF` `ElementFingerprint.GlobalId` reused verbatim — the Bim-stored `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` (the IFC `GlobalId` [H6]) where present, the neutral kernel `NodeId` string the `ModelDiff.Fingerprint` fold falls back to off the federation surface, so a commit over the WORKING graph captures an authored element carrying no IFC `GlobalId` yet (keyed by its `NodeId`) instead of dropping it, two federation surfaces carrying one `GlobalId` fault the commit typed rather than silently collapsing to one fingerprint, and a parallel commit-local fingerprint or a second identity scheme is the deleted form (the `ElementFingerprint` is diff's). The `CommitKey` composes the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter` + `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` over the lineage-plus-fingerprint preimage — the ONE canonical codec and the ONE seed-zero `XxHash128` hasher the node/edge/graph addresses ride — so a hand-rolled UTF-8 string-join keyed through a second `XxHash128` is the deleted form `Projection/address#CONTENT_ADDRESS` closes, and the key EXCLUDES the author/message/`Instant` so a re-commit of the identical model and lineage is genuinely idempotent while a retroactive content edit mints a divergent key.

The durable commit-DAG storage, the governed branch ACL/tag refs, and the maximal-antichain merge-base are the `Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner's — this owner produces the host-neutral content-addressed commit objects and the merge algebra Persistence stores by the same `CommitKey` and bases against the durable `CommitGraph.MergeBase` antichain, the in-memory `BimRepository` being the transient working DAG (the commit-DAG counterpart of the seam `Rasm.Element/Graph/delta#GRAPH_DELTA` `WorkingGraph`), never a durable store. The page composes the `Review/diff#MODEL_DIFF` `ElementFingerprint`/`ModelDiff.Fingerprint`, the seam `ElementGraph`/`Node`/`NodeId`/`ContentAddress`, the shared `QuikGraph` commit-DAG walks, and the `Review/coordination#SIGN_OFF` `SignOff` as settled vocabulary. The page is HOST-LOCAL — conflict resolution is the coordination `SignOff`'s concern and never an auto-resolve, and a versioning rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE.

## [01]-[INDEX]

- [02]-[VERSION_GRAPH]: `BimCommit` the content-addressed commit, `BimBranch` the working ref, the in-memory `BimRepository` commit-DAG with the `QuikGraph`-folded `Commit`/`History`/`MergeBases`/`Merge`/`CommitMerge`, the `MergeConflict` `[Union]`, the `MergeOutcome` receipt, and the `ModelHistory` three-way merge algebra (antichain base reduction + content-and-placement divergence) over the `Review/diff#MODEL_DIFF` `ElementFingerprint`.

## [02]-[VERSION_GRAPH]

- Owner: `BimCommit` the immutable commit object carrying the seam `ContentAddress CommitKey` content identity, the `Seq<ContentAddress> ParentKeys` lineage (empty for a root, one for a linear commit, two-or-more for a merge), the `Map<string, ElementFingerprint> Fingerprints` the `Review/diff#MODEL_DIFF` fingerprint of every element keyed by its `ElementFingerprint.GlobalId` (the IFC `ExternalId`, or the neutral `NodeId` string `ModelDiff.Fingerprint` falls back to for an authored element off the federation surface [H6]), and the author/message/capture `Instant`/`Option<FidelityReceipt>` carried metadata — the ingest exchange's `Projection/semantic#SEMANTIC_PROJECTOR` drop ledger seals beside the version it produced, evidence never identity — the commit identity derives from its content (lineage plus fingerprint set) through the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` over the `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter` preimage, the SAME codec and seed-zero `XxHash128` the `Review/diff#AUDIT` chain and the seam node/edge/graph addresses key on; `BimBranch` the thin in-memory working ref pointing one branch name at its head `CommitKey` (the durable governed ref — `RefKind`/ACL/upstream/annotated-tag — is the `Rasm.Persistence/Version/commits#COMMIT_DAG` `BranchRef`'s); `BimRepository` the in-memory working commit-DAG carrying the `Map<ContentAddress, BimCommit>` commit set and the `Map<string, BimBranch>` branch refs, owning `Commit`/`History`/`MergeBases`/`Merge`/`CommitMerge` over the `QuikGraph` lineage; `MergeConflict` the closed `[Union]` of the five irreconcilable divergences the three-way merge surfaces for sign-off; `MergeOutcome` the merge receipt carrying the auto-merged `Map<string, ElementFingerprint>` and the `Seq<MergeConflict>` the `SignOff` resolves; `ModelHistory` the static three-way merge algebra over the content-keyed fingerprint maps.
- Cases: `MergeConflict` arms `BothModified` (one `GlobalId` whose `ContentKey` diverged from the merge-base on BOTH branches — `Base`/`Ours`/`Theirs` full `ElementFingerprint`s, so a mixed content+placement divergence keeps its placement evidence, plus the `Review/diff#MODEL_DIFF` `AspectDelta` set naming WHICH members moved, the axis-typed payload a reviewer resolves on) · `PlacementDiverged` (one `GlobalId` both branches RELOCATED divergently while the content converged — the same three-fingerprint payload; the placement conflict a `ContentKey`-only merge silently auto-merged, dropping a branch's relocation) · `ModifiedAndRemoved` (one `GlobalId` one branch removed while the other modified it past the base — `Base`, `Surviving`, `bool RemovedByOurs`; equal `ContentKey`s with divergent `PlacementKey`s read as a relocation racing the removal) · `AddedTwiceDivergent` (one `GlobalId` both branches added with no common ancestor and divergent signatures — `Ours`, `Theirs`) · `CrissCross` (one `GlobalId` whose antichain bases DISAGREE, so the element has no single ancestor to diff against — `Seq<ElementFingerprint> Bases`, `Ours`, `Theirs`) (5) — a content divergence, a placement divergence, a delete/edit race, a no-base divergent add, and an ambiguous-ancestry element are the five irreducible element-granularity merge shapes, each declaring its `GlobalId` through an ABSTRACT base accessor its leaf overrides (the `Review/diff#MODEL_DIFF` `ElementChange.GlobalId` idiom — a base auto-property beside same-named leaf records SHADOWS instead of overriding, so one conflict answers two GlobalIds depending on the static type the caller holds) and the full per-side `ElementFingerprint`s (diff's currency verbatim — a decomposed `ContentAddress` subset dropping the second axis is the deleted form), never a per-kind conflict class; a `BimCommit` carries its lineage arity in `ParentKeys` rather than a per-kind record — `IsRoot` reads the empty parent set, `IsMerge` reads the multi-parent set — so a root, a linear, and an octopus-merge commit are one record discriminated by the parent count, never a `RootCommit`/`MergeCommit` class family.
- Entry: `BimRepository.Commit(ElementGraph graph, string branch, string author, string message, Instant at, Op key, Option<FidelityReceipt> fidelity = default)` seals the seam graph as a `BimCommit` child of the branch's current head (a fresh branch roots with no parent), advancing the in-memory branch ref — an UNCHANGED model no-ops (a head whose fingerprint map equals the minted one returns the head with the repository untouched, the entry-level idempotency that stops a federation sync loop appending no-change commits every tick) — `Fin<T>` railing `Model/faults#FAULT_BAND` `BimFault.ModelRejected` (`commit-duplicate-globalid`) BARE when two federation surfaces carry one IFC `GlobalId`: the seam keys nodes by `NodeId` and imposes no `ExternalId` uniqueness, so the `GlobalId`-keyed commit admits that federation-identity invariant EXACTLY ONCE at its own ingress (the LanguageExt `ToMap` duplicate-key throw dressed as a total commit is the deleted illusion); `BimRepository.Of(Seq<BimCommit> commits, Seq<BimBranch> branches, Op key)` is the bulk working-set admission — wire-received or store-loaded sets dedup commits by content key and rail a duplicated branch name (`repository-branch-duplicate`) or a head the set never declares (`repository-branch-absent`) typed, while out-of-set PARENTS stay legal so a shallow working set truncates conservatively; `BimRepository.Merge(ContentAddress ours, ContentAddress theirs, Func<string, ImmutableArray<AspectDelta>> deltas, Op key)` guards the lineage acyclic through `IsDirectedAcyclicGraph` (`merge-lineage-cyclic` — content-addressing makes an honest cycle unrepresentable, so a cycle proves forged `CommitKey`s), resolves both heads, bases on the whole `MergeBases` ANTICHAIN (so `CrissCross` is reachable from the in-memory entry, one law with the durable `CommitGraph.MergeBase`), threads the `Review/diff#MODEL_DIFF` member-delta lookup a `BothModified` conflict carries, and folds the three-way `MergeOutcome`, railing `BimFault.DanglingReference` (`merge-commit-absent`) BARE when either head names a commit the working set never declares; `BimRepository.CommitMerge(MergeOutcome resolved, Seq<ContentAddress> parents, string branch, string author, string message, Instant at, Op key)` seals a CLEAN (or sign-off-resolved) outcome as a merge commit whose parent arity rides the `Seq`, railing `BimFault.ModelRejected` (`merge-parent-arity`) on fewer than two distinct parents (a "merge" sealing a root or linear shape), `BimFault.DanglingReference` (`merge-parent-absent`) on a parent the working set never declares, and `BimFault.ModelRejected` (`merge-unresolved-conflicts`) on an outcome still carrying unresolved `Conflicts` so a conflicted merge never auto-commits; `ModelHistory.Merge(BimCommit ours, BimCommit theirs, Seq<BimCommit> bases, Func<string, ImmutableArray<AspectDelta>> deltas)` is the host-neutral three-way merge — it reduces the merge-base antichain per `GlobalId` then folds the `MergeOutcome`, the algebra the durable owner bases against its `CommitGraph.MergeBase` antichain.
- Auto: `Commit` reads the branch head as the parent (a `Seq<ContentAddress>` of zero or one), folds `graph.ObjectNodes` into the `Map<string, ElementFingerprint>` through the `Review/diff#MODEL_DIFF` `ModelDiff.Fingerprint(graph, objectNode)` (no second fingerprint), derives the `CommitKey` over the parents-plus-fingerprint-set preimage through `ContentAddress.Of` (excluding the author/message/`Instant`/`Fidelity` carried metadata for idempotency), and advances the branch ref in one `with` projection; `Sealed` normalizes `ParentKeys` sorted-distinct — the durable `CommitNode` `SortedDistinctParents` canon mirrored — so a reordered or duplicated parent seq seals one commit and `IsMerge` reads true arity; `History(head)` and `MergeBases(ours, theirs)` share ONE `Ancestry` kernel folding the ancestor-ward child→parent lineage into a `QuikGraph` `BidirectionalGraph<ContentAddress, SEdge<ContentAddress>>` walked by `BreadthFirstSearchAlgorithm` under the shipped `VertexDistanceRecorderObserver` scoped by its own `Attach` `IDisposable` (the algorithm IS an `ITreeBuilderAlgorithm`, that observer's own seam), the unit-relaxer distance giving generation depth so the closure reads level-ordered and deterministic; `MergeBases` answers the MINIMAL common-ancestor antichain — every common ancestor that no other common ancestor descends from — so a clean history yields one base and a criss-cross two or more, the SAME shape the durable `CommitGraph.MergeBase` publishes, sound over the multi-parent DAG where the retired Tarjan `OfflineLeastCommonAncestor` was not (its input contract is a rooted TREE: the DFS tree of a merge diamond keeps one in-edge per merge commit, hides the other lineage, and mis-bases a post-merge query at the root); `ModelHistory.Merge` reduces the base antichain PER ELEMENT — the bases that carry an id contribute their distinct signatures, and where they AGREE the single signature IS that element's base while a DISAGREEMENT means the element has no single ancestor and surfaces `CrissCross` carrying every candidate beside the two sides (only a convergent ours/theirs still lands, because there the ambiguous ancestry decides nothing) — then folds the union of the three commits' `GlobalId` keys, resolving each id by comparing its per-side `ElementFingerprint` to the base — an id changed on only one side takes that side, an id changed convergently on both takes the value, a content divergence surfaces `BothModified`, a content-converged placement divergence surfaces `PlacementDiverged`, a removal honored against an unchanged side drops, a removal racing a modification surfaces `ModifiedAndRemoved` — so the merge weighs BOTH the `ContentKey` and the `PlacementKey`, the auto-merge landing every non-conflicting element and the `MergeConflict` set carrying only the genuine divergences for `Review/coordination#SIGN_OFF`.
- Receipt: the `BimCommit` is the content-addressed history object the `Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner stores by `CommitKey` (the same content-key the `Exchange/wire#WIRE_PROJECTION` `IfcWire` face and the `Review/diff#MODEL_DIFF` carry) carrying the ingest exchange's `FidelityReceipt` beside its fingerprints — a federation audit reads which counted semantic drops produced the version it pulls — the `BimRepository` the in-memory working graph a federation branches and reconciles before Persistence durably stores the commits, and the `MergeOutcome` the typed three-way merge evidence — the auto-merged fingerprint graph plus the `MergeConflict` set the coordination `SignOff` advances through `Open → InProgress → Resolved → Closed` as each conflict is settled; the commit-DAG, the linear `Review/diff#AUDIT` `AuditTrail`, and the `Exchange/wire#WIRE_PROJECTION` `IfcWire` face read the one content-key space, never a parallel version identity.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, QuikGraph, LanguageExt.Core, Rasm.Element, Rasm, BCL `System.Collections.Immutable`
- Growth: a wider commit lineage arity (an octopus merge of three branches) is one longer `ParentKeys` set on the same record and one longer base antichain `ModelHistory.Merge` already reduces, never a new commit type; a new conflict kind (a both-retyped classification conflict) is one `MergeConflict` union arm the three-way fold surfaces; a richer conflict payload is one column on its arm fed from evidence the composing rail already holds, never a second diff re-derived here; history REWRITE is not this owner's — `Rasm.Persistence/Version/commits#COMMIT_DAG` publishes the append-only `HistoryRewrite` family (`Revert`/`CherryPick`/`Rebase`) through its one `Rewrite` entry, and a new rewrite verb is a case there; this owner grows only the merge fold those verbs consume; the durable DAG storage, the governed `BranchRef`, and the antichain merge-base ride the `Rasm.Persistence/Version/commits#COMMIT_DAG` ripple; never a per-commit-kind record, never a second identity scheme, and never an auto-resolved conflict.
- Boundary: `BimRepository.Seal` is the ONE point both `Commit` and `CommitMerge` funnel through, so it fires the `Model/observability#HOOK_RAIL` `rasm.bim.review.committed` point at that edge with `BimFact.Committed` — `CommitKey`, the sorted-distinct parent keys, the advanced branch name, and the fingerprint count off the sealed result — the unchanged-model no-op returning the head without reaching `Seal` and therefore firing nothing, the CloudEvents announcement being `Exchange/events#EVENT_PROJECTION`'s observe subscription over that point and a direct message-envelope mint here the deleted form; the commit and merge key on the `Review/diff#MODEL_DIFF` `ElementFingerprint.GlobalId` (the IFC `Node.Object.ExternalId`, or the neutral `NodeId` string `ModelDiff.Fingerprint` falls back to off the federation surface so an authored element carrying no IFC `GlobalId` yet is captured, never dropped [H6]) and a parallel commit-local fingerprint or a second identity scheme is the deleted form — the `ElementFingerprint` is diff's, reused verbatim; the `CommitKey` composes the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter` + `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` (the ONE codec, the ONE seed-zero `XxHash128`) so a hand-rolled `Encoding.UTF8.GetBytes(string.Join(...))` preimage keyed through a second `XxHash128.HashToUInt128` — a delimiter-forgeable encoding the `;`/`|`/`,`/`=` separators collide on — is the named defect `Projection/address#CONTENT_ADDRESS` closes, and the preimage excludes the author/message/`Instant`/`Fidelity` carried metadata so a re-commit of the identical model and lineage is idempotent (the prior instant-bearing key broke that invariant); the commit fingerprints the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph.ObjectNodes` and a `BimModel.Elements`/`BimElement` fold over the retired parallel element record is the deleted form (`Model/elements#IFC_CLASS`); the commit admits the `GlobalId`-uniqueness invariant the seam does not carry (nodes key by `NodeId`, `ExternalId` has no uniqueness law), faulting a federation duplicate typed instead of throwing through `ToMap` or last-wins-dropping a real element; `History`/`MergeBases` fold a transient `QuikGraph` graph through ONE `BreadthFirstSearchAlgorithm` `Ancestry` kernel per the shared `libs/csharp/.api/api-quikgraph` substrate, ordered by the shipped `VertexDistanceRecorderObserver` under its own `Attach` scope — a raw `DiscoverVertex` `+=`/`-=` pair over a mutable `List` is the deleted form (a detach spelled as a statement outlives the walk on any early exit, where the scoped composition cannot); the merge-base is the minimal-common-ancestor ANTICHAIN over two such closures and a single-nearest election is the deleted form that silently reduces ambiguous ancestry to one arbitrary base — the exact election `CrissCross` exists to refuse; `Merge` pre-gates the lineage with `IsDirectedAcyclicGraph`, and BOTH prior walk forms are rejected: the hand-rolled visited-set walk over a `Map<>`/`Seq<>` adjacency (the WALK is QuikGraph's), and Tarjan `OfflineLeastCommonAncestor` over the multi-parent DAG (the substrate's contract is a rooted TREE — over a merge diamond its DFS tree hides one lineage and mis-reports a post-merge base as the root); the in-memory `BimRepository` is the TRANSIENT working DAG (the commit-DAG counterpart of the seam `WorkingGraph`) and the durable commit-DAG store, the governed `BranchRef` (`RefKind`/ACL/tag/upstream), and the maximal-antichain merge-base are the `Rasm.Persistence/Version/commits#COMMIT_DAG` owner's — a durable store minted here is the named seam violation, this owner producing the host-neutral commit objects and the merge algebra joined at the `Review/versioning → Rasm.Persistence/Version/commits # [CONTENT_KEY]: BimCommit content-addressed commit-DAG` seam by the `CommitKey` (Persistence stores the `BimCommit` as a generic `CommitNode` under the wire-carried key and never re-derives it) and at the `Review/versioning → Rasm.Persistence/Version/commits # [SHAPE]: BimCommit DAG common-ancestor merge substrate` seam where `ModelHistory.Merge` bases against the durable `CommitGraph.MergeBase` antichain; the three-way merge reuses the `Review/diff#MODEL_DIFF` `ElementFingerprint` verbatim and weighs BOTH the `ContentKey` AND the `PlacementKey` so a divergent relocation surfaces `PlacementDiverged` rather than being silently auto-merged (a `ContentKey`-only comparison dropping a branch's placement is the deleted defect), and a field-by-field element comparison beside the fingerprint is the deleted form; an element whose antichain bases DISAGREE has no single ancestor and surfaces `CrissCross` carrying every candidate — electing one base by hash magnitude is deterministic but semantically arbitrary and silently converts a real `BothModified` into an auto-merge whenever a side happens to match the elected base, so that election is the deleted form, while the strictly better recursive pairwise base merge needs ancestor commits this host-neutral algebra does not hold and belongs to the durable owner if it ever lands; the commit and parent keys are typed seam `ContentAddress` values end to end — the `.Value` unwrap that stored a raw `UInt128` erased the one content-key type `Review/diff#AUDIT` states its keys in, and a hex rendering is a formatting read off `.Value`, never the stored shape; this owner is the FINGERPRINT-altitude federation/IFC three-way merge (offline multi-writer reconciliation over the element-identity content keys), distinct from the `Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` `StructuralMerge.ThreeWay` FOREST-altitude merge (member-level `Generator.Equals` `Inequalities` patches over the full `ElementGraph` topology — `Move`/`Reorder`/`Retype`/`TopologyBreak`), neither re-cased as the other; conflict resolution is the `Review/coordination#SIGN_OFF` `SignOff` lifecycle's concern — `ModelHistory.Merge` surfaces the typed `MergeConflict` set and never auto-resolves, so `CommitMerge` rejecting an unresolved outcome is the law and a silent last-write-wins merge is the deleted form; history REWRITE (revert, cherry-pick, rebase) is the `Rasm.Persistence/Version/commits#COMMIT_DAG` `HistoryRewrite`/`Rewrite` owner's append-only surface and re-deriving it here is the deleted form — this owner publishes the merge fold those verbs consume; the `MergeConflict` is a closed `[Union]` and a per-kind conflict class is the deleted form; the commit-DAG and the linear `Review/diff#AUDIT` `AuditTrail` are distinct owners over the one content-key — the audit trail the per-element who/when/what linear Merkle chain, the commit-DAG the branching revision graph, neither re-cased as the other; the page is HOST-LOCAL — a `BimCommit` carries the host-free `ElementFingerprint` content keys, never a RhinoCommon type or host-bound geometry, and takes a neutral NodaTime `Instant` rather than the app-platform `Rasm.AppHost` `ClockPolicy` an AEC-domain owner never references; a versioning rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE (the `Op`-keyed `Expected`-derived case IS the `Error`, the `new BimFault.X("string").ToError()` lowering hop GONE).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Immutable;
using System.Linq;
using LanguageExt;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record BimCommit(
    ContentAddress CommitKey,
    Seq<ContentAddress> ParentKeys,
    Map<string, ElementFingerprint> Fingerprints,
    string Author,
    string Message,
    Instant At,
    Option<FidelityReceipt> Fidelity) {
    public bool IsRoot => ParentKeys.IsEmpty;
    public bool IsMerge => ParentKeys.Count > 1;

    // Mint a commit from a seam ElementGraph: every Object node's Review/diff#MODEL_DIFF ElementFingerprint folded
    // into the content-addressed commit, keyed by the fingerprint's GlobalId — the IFC ExternalId where present, the
    // neutral NodeId string ModelDiff.Fingerprint falls back to off the federation surface, so an authored element
    // carrying no IFC GlobalId yet is captured [H6]. The seam keys nodes by NodeId with NO ExternalId uniqueness law,
    // so the GlobalId-keyed commit admits that invariant EXACTLY ONCE here: a federation duplicate rails ModelRejected
    // BARE (the raw ToMap duplicate-key throw dressed as a total commit, and a last-wins fold silently dropping a real
    // element, are the deleted forms). ModelDiff.Fingerprint is reused verbatim — the commit IS the fingerprint set.
    public static Fin<BimCommit> Of(ElementGraph graph, Seq<ContentAddress> parents, string author, string message, Instant at, Op key, Option<FidelityReceipt> fidelity = default) {
        Seq<ElementFingerprint> prints = graph.ObjectNodes.Map(o => ModelDiff.Fingerprint(graph, o));
        Seq<string> collided = toSeq(prints.GroupBy(static fp => fp.GlobalId, StringComparer.Ordinal).Where(static g => g.Skip(1).Any()).Select(static g => g.Key));
        return collided.IsEmpty
            ? Fin.Succ(Sealed(parents, prints.Map(static fp => (fp.GlobalId, fp)).ToMap(), author, message, at, fidelity))
            : Fin.Fail<BimCommit>(new BimFault.ModelRejected(key, $"commit-duplicate-globalid:{string.Join(',', collided)}"));
    }

    // The commit identity is content-addressed over the lineage + the diff fingerprint set ONLY — author/message/at
    // and the exchange's Projection/semantic#SEMANTIC_PROJECTOR FidelityReceipt are carried metadata EXCLUDED from
    // the key (evidence, never identity), so a re-commit of the identical model and lineage is genuinely
    // idempotent (an instant-bearing key broke that) and a retroactive content edit mints a divergent key. ParentKeys
    // normalize sorted-distinct HERE, the one construction path — the durable CommitNode SortedDistinctParents canon
    // mirrored — so a reordered or duplicated parent seq seals one commit and IsMerge reads true arity. The
    // preimage rides the seam CanonicalWriter (Projection/address#CANONICAL_WRITER) + kernel seed-zero ContentAddress
    // (Projection/address#CONTENT_ADDRESS) — the ONE codec and the ONE hasher the node/edge/graph addresses use — so a
    // delimiter-forgeable UTF-8 string-join keyed through a second XxHash128 is the defect #CONTENT_ADDRESS closes.
    public static BimCommit Sealed(Seq<ContentAddress> parents, Map<string, ElementFingerprint> fingerprints, string author, string message, Instant at, Option<FidelityReceipt> fidelity = default) {
        Seq<ContentAddress> lineage = toSeq(parents.Distinct().OrderBy(static k => k.Value));
        return new(KeyOf(lineage, fingerprints), lineage, fingerprints, author, message, at, fidelity);
    }

    // No measure is written, so the tolerance is inert; the fingerprint ContentAddresses are already content-keyed.
    // Parents arrive canonical from Sealed; fingerprints sort by GlobalId so insertion order addresses identically —
    // the order-independence the durable CommitNode shares.
    static ContentAddress KeyOf(Seq<ContentAddress> parents, Map<string, ElementFingerprint> fingerprints) {
        CanonicalWriter w = new(0.0);
        w.Ordinal(parents.Count);
        foreach (ContentAddress p in parents) { w.U128(p.Value); }
        w.Ordinal(fingerprints.Count);
        foreach (var (id, fp) in fingerprints.OrderBy(static e => e.Key, StringComparer.Ordinal)) {
            w.String(id).U128(fp.ContentKey.Value).U128(fp.PlacementKey.Value);
        }
        return ContentAddress.Of(w.ToBytes().Span);
    }
}

// The thin in-memory working ref (name -> head CommitKey); the durable governed ref (RefKind/ACL/upstream/
// annotated-tag, the two-sided Movable gate) is the Rasm.Persistence/Version/commits BranchRef's.
public sealed record BimBranch(string Name, ContentAddress Head);

// The closed element-granularity merge-conflict family the three-way fold surfaces for Review/coordination#SIGN_OFF.
// Each arm carries its GlobalId through the base accessor (the Review/diff#MODEL_DIFF ElementChange idiom — a BCF
// topic anchors on it directly) and the FULL per-side ElementFingerprints — diff's currency verbatim, never a
// decomposed ContentAddress subset, so BOTH divergence axes stay evidence (a placement-only race inside
// ModifiedAndRemoved reads as equal ContentKeys + divergent PlacementKeys, where a ContentKey-only payload would
// render Base == Surviving). The case name is the diagnosis; a per-kind conflict class is the deleted form.
// GlobalId is an ABSTRACT base member each leaf overrides through its own positional property — the working form
// Review/diff#MODEL_DIFF's ElementChange already uses. A base auto-property paired with same-named leaf records
// shadows rather than overrides (CS0108), so a caller holding the base reads the constructor-captured value while
// a caller holding the leaf reads the record's own: one conflict, two answers.
[Union]
public abstract partial record MergeConflict {
    private MergeConflict() { }

    public abstract string GlobalId { get; }

    // BothModified alone carries the AXIS-TYPED payload: the two content keys say THAT the element diverged and
    // nothing about WHERE, so the Review/diff#MODEL_DIFF AspectDelta rows the composing rail already computed ride
    // the conflict — a sign-off reviewer reads which Pset value, which quantity, which material binding each side
    // moved, and resolves on the aspect rather than on two opaque hashes. The placement and delete/edit arms carry
    // no delta set: their divergence axis is already named by the case.
    public sealed record BothModified(string GlobalId, ElementFingerprint Base, ElementFingerprint Ours, ElementFingerprint Theirs, ImmutableArray<AspectDelta> Deltas) : MergeConflict;
    public sealed record PlacementDiverged(string GlobalId, ElementFingerprint Base, ElementFingerprint Ours, ElementFingerprint Theirs) : MergeConflict;
    public sealed record ModifiedAndRemoved(string GlobalId, ElementFingerprint Base, ElementFingerprint Surviving, bool RemovedByOurs) : MergeConflict;
    public sealed record AddedTwiceDivergent(string GlobalId, ElementFingerprint Ours, ElementFingerprint Theirs) : MergeConflict;
    // CrissCross is the ONE arm whose sides are Options, because ambiguous ancestry does not imply both sides
    // still carry the element: one branch may have removed it under bases that disagree. A required-fingerprint
    // payload has no spelling for that, so the absent side gets filled with a COPY of the present one — and the
    // reviewer then reads Ours == Theirs, the exact signature of two branches that agree, on the one row where a
    // branch deleted the element. The absence is evidence; it stays typed.
    public sealed record CrissCross(string GlobalId, Seq<ElementFingerprint> Bases, Option<ElementFingerprint> Ours, Option<ElementFingerprint> Theirs) : MergeConflict;
}

public sealed record MergeOutcome(Map<string, ElementFingerprint> Merged, Seq<MergeConflict> Conflicts) {
    public bool IsClean => Conflicts.IsEmpty;
}

// The in-memory working commit-DAG (the commit-DAG counterpart of the seam WorkingGraph) — transient, never the
// durable store. Persistence stores each BimCommit as a generic CommitNode under the carried CommitKey and owns
// the governed BranchRef + the maximal-antichain MergeBase; this working set folds a QuikGraph lineage per walk.
public sealed record BimRepository(Map<ContentAddress, BimCommit> Commits, Map<string, BimBranch> Branches) {
    public static readonly BimRepository Empty = new(Map<ContentAddress, BimCommit>(), Map<string, BimBranch>());

    // The bulk working-set admission — a wire-received or store-loaded commit set enters ONCE here: commits dedup
    // by content key (two equal keys ARE one commit — AddOrUpdate, never the throwing ToMap), a branch naming two
    // divergent heads or a head the set never declares rails typed. Commit PARENTS may lie outside the set — a
    // shallow working set is legal: History truncates at the boundary and a beyond-the-boundary merge-base
    // resolves as no-base AddedTwiceDivergent conflicts, conservative, never a silent wrong merge.
    public static Fin<BimRepository> Of(Seq<BimCommit> commits, Seq<BimBranch> branches, Op key) {
        Map<ContentAddress, BimCommit> declared = commits.Fold(Map<ContentAddress, BimCommit>(), static (map, c) => map.AddOrUpdate(c.CommitKey, c));
        Seq<string> duplicated = toSeq(branches.GroupBy(static b => b.Name, StringComparer.Ordinal).Where(static g => g.Select(static b => b.Head).Distinct().Skip(1).Any()).Select(static g => g.Key));
        Seq<string> dangling = branches.Filter(b => !declared.ContainsKey(b.Head)).Map(static b => b.Name).Distinct();
        return (duplicated.IsEmpty, dangling.IsEmpty) switch {
            (false, _) => Fin.Fail<BimRepository>(new BimFault.ModelRejected(key, $"repository-branch-duplicate:{string.Join(',', duplicated)}")),
            (_, false) => Fin.Fail<BimRepository>(new BimFault.DanglingReference(key, $"repository-branch-absent:{string.Join(',', dangling)}")),
            _          => Fin.Succ(new BimRepository(declared, branches.Fold(Map<string, BimBranch>(), static (map, b) => map.AddOrUpdate(b.Name, b)))),
        };
    }

    public Option<BimCommit> Find(ContentAddress key) => Commits.Find(key);
    public Option<BimBranch> Branch(string name) => Branches.Find(name);

    // Seal a seam ElementGraph as a child of the branch head (a fresh branch roots with no parent) and advance the
    // in-memory branch ref; the one federation-identity fault (commit-duplicate-globalid) rails BARE from
    // BimCommit.Of. An UNCHANGED model no-ops: a head whose fingerprint map equals the minted one returns the head
    // itself with the repository untouched — the entry-level idempotency the key-level law alone cannot give
    // (the child's lineage embeds the head, so re-sealing would mint a fresh key per sync tick, commit spam).
    // The durable store + the ACL-gated branch advance are Persistence's; this is the working advance. Takes a
    // neutral NodaTime Instant — never the app-platform ClockPolicy an AEC owner cannot reference. An ingest-sealed
    // commit carries the exchange's FidelityReceipt beside the fingerprints; the unchanged no-op returns the head
    // and drops the fresh receipt — no new version, no new evidence seat.
    public Fin<(BimRepository Repository, BimCommit Commit)> Commit(
        ElementGraph graph, string branch, string author, string message, Instant at, Op key,
        Option<FidelityReceipt> fidelity = default, Option<BimHooks> hooks = default) =>
        BimCommit.Of(graph, Branches.Find(branch).Map(static b => Seq(b.Head)).IfNone(Seq<ContentAddress>()), author, message, at, key, fidelity)
            .Map(commit => Branches.Find(branch).Bind(b => Commits.Find(b.Head)).Filter(head => head.Fingerprints == commit.Fingerprints).Match(
                Some: head => (this, head),
                None: () => Seal(commit, branch, key, hooks)));

    // The ancestor sequence head -> ... -> root in breadth-first discovery order — the one Ancestry kernel projected
    // onto commits, so a merge commit's two lineages converge without re-visiting.
    public Seq<BimCommit> History(ContentAddress head) => Ancestry(Lineage(), head).Choose(Commits.Find);

    // The in-memory merge-base ANTICHAIN: every MINIMAL common ancestor, not the single nearest. A common
    // ancestor that is itself a proper ancestor of another common ancestor is shadowed and drops, so a clean
    // history yields exactly one base and a criss-cross yields two or more — the SAME shape the durable
    // CommitGraph.MergeBase publishes, so ModelHistory.Merge runs ONE law over both sources and the CrissCross
    // arm is reachable from this entry rather than existing only for a caller who happened to hold the durable
    // antichain. The single-nearest form is the deleted shape: it silently reduced an ambiguous ancestry to one
    // arbitrary base, which is exactly the election the CrissCross arm exists to refuse. Level-order discovery
    // makes the closure sound over the multi-parent DAG where Tarjan OfflineLeastCommonAncestor is NOT (its
    // contract is a rooted TREE: the DFS tree of a merge diamond keeps one in-edge per merge commit, hides the
    // other lineage, and mis-bases a post-merge query at the root — the api-quikgraph warning; a first-parent tree
    // projection loses the same lineage, so the retired Tarjan call has no lossless rooted repair). Disjoint
    // histories fold empty, which ThreeWay resolves as a no-base divergence.
    public Seq<ContentAddress> MergeBases(ContentAddress ours, ContentAddress theirs) => MergeBases(Lineage(), ours, theirs);

    // LanguageExt.HashSet spelled qualified: `using LanguageExt` and the BCL generic collections both publish a
    // HashSet<T>, and the bare name binds whichever import the file's using order favours — the same
    // disambiguation convention Review/coordination#COORDINATION holds at its own set-typed locals.
    static Seq<ContentAddress> MergeBases(BidirectionalGraph<ContentAddress, SEdge<ContentAddress>> lineage, ContentAddress ours, ContentAddress theirs) {
        LanguageExt.HashSet<ContentAddress> theirAncestry = toHashSet(Ancestry(lineage, theirs));
        Seq<ContentAddress> common = Ancestry(lineage, ours).Filter(theirAncestry.Contains);
        LanguageExt.HashSet<ContentAddress> shadowed = toHashSet(
            common.Bind(candidate => Ancestry(lineage, candidate).Filter(reached => reached != candidate)));
        return common.Filter(candidate => !shadowed.Contains(candidate));
    }

    // Resolve both heads, base on the single-nearest CommonAncestor, fold the three-way over the fingerprint maps;
    // the acyclicity guard is the forged-key integrity gate (an honest content-addressed lineage cannot cycle — a
    // child's key derives from its parents' keys), an absent head rails DanglingReference BARE, and ONE Lineage
    // fold serves the gate and both ancestry closures. The durable path supplies the CommitGraph.MergeBase
    // antichain to ModelHistory.Merge directly, which virtualizes 0/1/N bases — one algebra, two base sources.
    public Fin<MergeOutcome> Merge(ContentAddress ours, ContentAddress theirs, Func<string, ImmutableArray<AspectDelta>> deltas, Op key) =>
        from lineage in Fin.Succ(Lineage())
        from _ in guard(lineage.IsDirectedAcyclicGraph(), () => (Error)new BimFault.ModelRejected(key, "merge-lineage-cyclic"))
        from o in Commits.Find(ours).ToFin(new BimFault.DanglingReference(key, $"merge-commit-absent:{ours.Value:X32}"))
        from t in Commits.Find(theirs).ToFin(new BimFault.DanglingReference(key, $"merge-commit-absent:{theirs.Value:X32}"))
        let bases = MergeBases(lineage, ours, theirs).Choose(Commits.Find)
        select ModelHistory.Merge(o, t, bases, deltas);

    // Seal a CLEAN (or sign-off-resolved) outcome as a merge commit + advance the branch: fewer than two DISTINCT
    // parents rails ModelRejected (a "merge" sealing a root or a linear commit is a forged lineage shape — the
    // arity gate reads the Sealed-canonical distinct set, so a duplicated head cannot smuggle past it), a parent
    // the working set never declares rails DanglingReference (a sealed merge never dangles its lineage), and an
    // outcome still carrying unresolved Conflicts rails ModelRejected BARE so a conflicted merge NEVER
    // auto-commits — conflict resolution is the Review/coordination#SIGN_OFF lifecycle's concern. The parent
    // arity rides the Seq, so a pairwise merge passes the two heads and an octopus merge the N heads — one entry,
    // the lineage arity in the value.
    public Fin<(BimRepository Repository, BimCommit Commit)> CommitMerge(
        MergeOutcome resolved, Seq<ContentAddress> parents, string branch, string author, string message, Instant at, Op key,
        Option<BimHooks> hooks = default) =>
        (Absent: parents.Filter(p => !Commits.ContainsKey(p)), resolved.IsClean) switch {
            _ when parents.Distinct().Count < 2 => Fin.Fail<(BimRepository, BimCommit)>(new BimFault.ModelRejected(key, $"merge-parent-arity:{parents.Distinct().Count}")),
            ({ IsEmpty: false } absent, _)      => Fin.Fail<(BimRepository, BimCommit)>(new BimFault.DanglingReference(key, $"merge-parent-absent:{string.Join(',', absent.Map(static p => p.Value.ToString("X32")))}")),
            (_, false)                          => Fin.Fail<(BimRepository, BimCommit)>(new BimFault.ModelRejected(key, $"merge-unresolved-conflicts:{resolved.Conflicts.Count}")),
            _                                   => Fin.Succ(Seal(BimCommit.Sealed(parents, resolved.Merged, author, message, at), branch, key, hooks)),
        };

    // Both Commit and CommitMerge funnel through this ONE seal, which is therefore the only fire site for the
    // rasm.bim.review.committed point; the unchanged-model no-op returns the head without reaching here, which is
    // what keeps the announced stream free of no-change rows. Firing here rather than announcing here keeps the
    // envelope custody at Exchange/events#EVENT_PROJECTION, which observes this point like any other subscriber.
    (BimRepository Repository, BimCommit Commit) Seal(BimCommit commit, string branch, Op key, Option<BimHooks> hooks) {
        BimRepository advanced = Advance(branch, commit);
        ignore(hooks.Map(live => live.Committed.Fire(new BimFact.Committed(
            Key: key,
            CommitKey: commit.CommitKey.Value,
            Parents: ContentKeySet.Of(commit.Parents.Map(static parent => parent.Value)),
            Branch: branch,
            Elements: commit.Fingerprints.Count))));
        return (advanced, commit);
    }

    BimRepository Advance(string branch, BimCommit commit) => this with {
        Commits = Commits.AddOrUpdate(commit.CommitKey, commit),
        Branches = Branches.AddOrUpdate(branch, new BimBranch(branch, commit.CommitKey)),
    };

    // The breadth-first ancestor closure over a built lineage — the sanctioned QuikGraph kernel boundary: the
    // imperative walk collapses INTO the package, the domain algebra stays pure. The order rides the SHIPPED
    // VertexDistanceRecorderObserver scoped by its own Attach IDisposable — BreadthFirstSearchAlgorithm IS an
    // ITreeBuilderAlgorithm, which is exactly that observer's Attach seam, so the raw += / -= event pair over a
    // mutable List is the deleted form: a subscription whose detach is a statement outlives the walk on any early
    // exit, where the scoped composition cannot. Under a unit relaxer the recorded distance IS the generation
    // depth, so ordering by it reproduces level order; the tie-break on the key makes two walks of one lineage
    // byte-identical where dictionary enumeration order alone does not. The head leads the sequence explicitly and
    // is FILTERED out of the recorded distances: the observer seeds each tree edge's SOURCE at the relaxer's initial
    // distance, so a head carrying any parent lands in Distances at depth zero and an unfiltered projection would
    // emit it twice, while a parentless head records nothing at all — the explicit lead covers both. An unknown head
    // folds empty.
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

    // The commit lineage as a transient QuikGraph BidirectionalGraph (the vertex IS the ContentAddress CommitKey) — a
    // per-walk fold, never a stored domain field. Child->parent out-edges: every walk is ancestor-ward (the
    // parent->child orientation existed only for the retired Tarjan descent).
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

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ModelHistory {
    // The host-neutral three-way merge: reduce the merge-base antichain (the durable CommitGraph.MergeBase
    // superset, or the in-memory single-nearest) per GlobalId, then fold the union of the three commits' GlobalId
    // keys. Reuses the Review/diff#MODEL_DIFF ElementFingerprint verbatim — no second identity. The key universe
    // sorts by GlobalId so the merged map and the conflict sequence are deterministic.
    // `deltas` is the Review/diff#MODEL_DIFF member-level evidence keyed by GlobalId — the composing rail already
    // holds the ModelDiff between the two revisions, so a BothModified conflict carries WHICH aspects diverged
    // rather than two opaque content keys a reviewer must re-diff by hand to act on.
    public static MergeOutcome Merge(BimCommit ours, BimCommit theirs, Seq<BimCommit> bases, Func<string, ImmutableArray<AspectDelta>> deltas) =>
        ThreeWay(ours, theirs, bases, deltas);

    // CRISS-CROSS LAW: BOTH base sources — the in-memory BimRepository.MergeBases and the durable
    // CommitGraph.MergeBase — yield the minimal common-ancestor ANTICHAIN, so a clean
    // history gives one base and a criss-cross two-or-more. Where the antichain's bases AGREE on an element (one
    // distinct signature) the reduction is total and that signature IS the base. Where they DISAGREE the element
    // has no single ancestor, and electing one is a decision the merge is not entitled to make: the prior
    // hash-magnitude election picked the numerically smaller ContentKey — deterministic, semantically arbitrary,
    // and silently converting a real BothModified into an auto-merge whenever ours or theirs happened to match the
    // elected base. The disagreement surfaces instead as a typed CrissCross conflict carrying EVERY candidate base
    // beside the two sides, so a sign-off reviewer sees exactly the ambiguity the history contains. Recursive
    // pairwise base merging is the strictly better algorithm and is deliberately NOT taken here: it demands
    // ancestor commits this host-neutral algebra does not hold (the durable owner does), so it belongs to
    // Rasm.Persistence/Version/commits#COMMIT_DAG if it ever lands, and the conservative typed conflict is
    // the honest floor until then. Empty antichain -> unrelated histories, resolved as a no-base divergence.
    static Seq<ElementFingerprint> BaseOf(Seq<BimCommit> bases, string id) =>
        bases.Choose(commit => commit.Fingerprints.Find(id)).Distinct();

    static MergeOutcome ThreeWay(BimCommit ours, BimCommit theirs, Seq<BimCommit> bases, Func<string, ImmutableArray<AspectDelta>> deltas) =>
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

    // An element whose antichain bases disagree: where the two sides CONVERGE the ambiguous ancestry decides
    // nothing and the converged value lands — Option equality states that in one predicate, because a converged
    // presence and a converged removal are the same fact about the sides and both keep exactly what they agree
    // on. Every divergence is the typed CrissCross a reviewer resolves, carrying each side AS IT STANDS: a
    // present-versus-removed divergence is the case ambiguous ancestry makes most likely and the one a
    // fingerprint-shaped payload can only report as agreement.
    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) Ambiguous(
        string id, Option<ElementFingerprint> ours, Option<ElementFingerprint> theirs, Seq<ElementFingerprint> bases) =>
        ours.Equals(theirs)
            ? (Option<MergeConflict>.None, ours)
            : (Some<MergeConflict>(new MergeConflict.CrissCross(id, bases, ours, theirs)), Option<ElementFingerprint>.None);

    // Resolve one GlobalId across the three sides into (conflict?, keep?): keep=Some lands the merged fingerprint,
    // keep=None is a converged removal, conflict=Some routes the divergence to SignOff. The eight-case three-way
    // truth table threaded through Option.Match (no unsafe value access); the both-sides-absent rows fold to a
    // converged removal, and the all-absent row never enumerates (the id would not be in the key universe).
    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) Resolve(
        string id, Option<ElementFingerprint> ours, Option<ElementFingerprint> theirs, Option<ElementFingerprint> mergeBase,
        Func<string, ImmutableArray<AspectDelta>> deltas) =>
        ours.Match(
            Some: o => theirs.Match(
                Some: t => BothSides(id, o, t, mergeBase, deltas),
                None: () => mergeBase.Match(
                    Some: b => RemovedVsModified(id, o, b, removedByOurs: false),
                    None: () => (Option<MergeConflict>.None, Some(o)))),
            None: () => theirs.Match(
                Some: t => mergeBase.Match(
                    Some: b => RemovedVsModified(id, t, b, removedByOurs: true),
                    None: () => (Option<MergeConflict>.None, Some(t))),
                None: () => (Option<MergeConflict>.None, Option<ElementFingerprint>.None)));

    // Both sides present: an element is changed when EITHER its ContentKey or its PlacementKey moved off the base
    // (a no-base both-present case treats both as changed -> an added-twice divergence). Only one side changed ->
    // take it; both converged to one signature -> take it; both diverged -> a typed conflict.
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

    // The divergent-edit conflict, axis-typed: a content divergence is BothModified (or AddedTwiceDivergent with no
    // base), a content-CONVERGENT placement-only divergence is PlacementDiverged — so a pure divergent relocation
    // surfaces its own conflict the prior ContentKey-only merge silently auto-merged, dropping a branch's placement.
    // Every arm carries the full per-side fingerprints, so the mixed content+placement divergence keeps its
    // placement evidence inside BothModified rather than dropping the second axis.
    static MergeConflict Divergence(string id, ElementFingerprint ours, ElementFingerprint theirs, Option<ElementFingerprint> mergeBase,
        Func<string, ImmutableArray<AspectDelta>> deltas) =>
        mergeBase.Match(
            Some: b => ours.ContentKey != theirs.ContentKey
                ? (MergeConflict)new MergeConflict.BothModified(id, b, ours, theirs, deltas(id))
                : new MergeConflict.PlacementDiverged(id, b, ours, theirs),
            None: () => new MergeConflict.AddedTwiceDivergent(id, ours, theirs));

    // One side removed, the other survives: an UNCHANGED surviving side (content AND placement match the base)
    // honors the removal (drop); any change races the removal into ModifiedAndRemoved — the full-signature check
    // so a relocation racing a removal is never silently dropped, and the full fingerprints keep the raced axis
    // readable (a placement-only race shows equal ContentKeys and divergent PlacementKeys).
    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) RemovedVsModified(
        string id, ElementFingerprint surviving, ElementFingerprint mergeBase, bool removedByOurs) =>
        Same(surviving, mergeBase)
            ? (Option<MergeConflict>.None, Option<ElementFingerprint>.None)
            : (Some<MergeConflict>(new MergeConflict.ModifiedAndRemoved(id, mergeBase, surviving, removedByOurs)), Option<ElementFingerprint>.None);

    // The full element signature — content AND placement — so the merge weighs a relocation, never the ContentKey alone.
    static bool Same(ElementFingerprint a, ElementFingerprint b) => a.ContentKey == b.ContentKey && a.PlacementKey == b.PlacementKey;
}
```

## [03]-[RESEARCH]

(none)
