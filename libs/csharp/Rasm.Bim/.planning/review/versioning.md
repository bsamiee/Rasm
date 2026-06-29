# [BIM_VERSIONING]

The content-addressed model-history owner over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`: one `BimCommit` commit object whose identity IS the `Review/diff#MODEL_DIFF` `ElementFingerprint` set it carries, the `BimBranch` working ref, the in-memory `BimRepository` commit-DAG threading commits by `ParentKeys`, and the `Version` three-way merge algebra reconciling two divergent revisions against their merge-base into a merged fingerprint graph plus a closed `MergeConflict` `[Union]` the `Review/coordination#SIGN_OFF` `SignOff` resolves. A commit is the fingerprint set, a diff is two commits, and a merge folds two fingerprint streams against a virtualized merge-base — so the commit-DAG is the branching counterpart to the linear `Review/diff#AUDIT` `AuditTrail` Merkle chain, the two sharing the one content-key idiom and neither re-derived from the other.

Identity is the `Review/diff#MODEL_DIFF` `ElementFingerprint.GlobalId` reused verbatim — the Bim-stored `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` (the IFC `GlobalId` [H6]) where present, the neutral kernel `NodeId` string the `ModelDiff.Fingerprint` fold falls back to off the federation surface, so a commit over the WORKING graph captures an authored element carrying no IFC `GlobalId` yet (keyed by its `NodeId`) instead of dropping it, and a parallel commit-local fingerprint or a second identity scheme is the deleted form (the `ElementFingerprint` is diff's). The `CommitKey` composes the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `CanonicalWriter` + `ContentAddress` over the lineage-plus-fingerprint preimage — the ONE canonical codec and the ONE seed-zero `XxHash128` hasher the node/edge/graph addresses ride — so a hand-rolled UTF-8 string-join keyed through a second `XxHash128` is the deleted form `address#SINGLE_HASHER` closes, and the key EXCLUDES the author/message/`Instant` so a re-commit of the identical model and lineage is genuinely idempotent while a retroactive content edit mints a divergent key.

The durable commit-DAG storage, the governed branch ACL/tag refs, and the maximal-antichain merge-base are the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner's — this owner produces the host-neutral content-addressed commit objects and the merge algebra Persistence stores by the same `CommitKey` and bases against the durable `CommitGraph.MergeBase` antichain, the in-memory `BimRepository` being the transient working DAG (the commit-DAG counterpart of the seam `Rasm.Element/Graph/delta#GRAPH_DELTA` `WorkingGraph`), never a durable store. The page composes the `Review/diff#MODEL_DIFF` `ElementFingerprint`/`ModelDiff.Fingerprint`, the seam `ElementGraph`/`Node`/`NodeId`/`ContentAddress`, the shared `QuikGraph` commit-DAG walks, and the `Review/coordination#SIGN_OFF` `SignOff` as settled vocabulary. The page is HOST-LOCAL — conflict resolution is the coordination `SignOff`'s concern and never an auto-resolve, and a versioning rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE.

## [01]-[INDEX]

- [01]-[VERSION_GRAPH]: `BimCommit` the content-addressed commit, `BimBranch` the working ref, the in-memory `BimRepository` commit-DAG with the `QuikGraph`-folded `Commit`/`History`/`CommonAncestor`/`Merge`/`CommitMerge`, the `MergeConflict` `[Union]`, the `MergeOutcome` receipt, and the `Version` three-way merge algebra (merge-base virtualization + content-and-placement divergence) over the `Review/diff#MODEL_DIFF` `ElementFingerprint`.

## [02]-[VERSION_GRAPH]

- Owner: `BimCommit` the immutable commit object carrying the `UInt128 CommitKey` content identity, the `Seq<UInt128> ParentKeys` lineage (empty for a root, one for a linear commit, two-or-more for a merge), the `Map<string, ElementFingerprint> Fingerprints` the `Review/diff#MODEL_DIFF` fingerprint of every element keyed by its `ElementFingerprint.GlobalId` (the IFC `ExternalId`, or the neutral `NodeId` string `ModelDiff.Fingerprint` falls back to for an authored element off the federation surface [H6]), and the author/message/capture `Instant` carried metadata — the commit identity derives from its content (lineage plus fingerprint set) through the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` over the `CanonicalWriter` preimage, the SAME codec and seed-zero `XxHash128` the `Review/diff#AUDIT` chain and the seam node/edge/graph addresses key on; `BimBranch` the thin in-memory working ref pointing one branch name at its head `CommitKey` (the durable governed ref — `RefKind`/ACL/upstream/annotated-tag — is the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` `BranchRef`'s); `BimRepository` the in-memory working commit-DAG carrying the `Map<UInt128, BimCommit>` commit set and the `Map<string, BimBranch>` branch refs, owning `Commit`/`History`/`CommonAncestor`/`Merge`/`CommitMerge` over the `QuikGraph` lineage; `MergeConflict` the closed `[Union]` of the four irreconcilable divergences the three-way merge surfaces for sign-off; `MergeOutcome` the merge receipt carrying the auto-merged `Map<string, ElementFingerprint>` and the `Seq<MergeConflict>` the `SignOff` resolves; `Version` the static three-way merge algebra over the content-keyed fingerprint maps.
- Cases: `MergeConflict` arms `BothModified` (one `GlobalId` whose `ContentKey` diverged from the merge-base on BOTH branches to different `ContentAddress`es — `BaseKey`, `OursKey`, `TheirsKey`) · `PlacementDiverged` (one `GlobalId` both branches RELOCATED divergently while the content converged — `BasePlacement`, `OursPlacement`, `TheirsPlacement`; the placement conflict a `ContentKey`-only merge silently auto-merged, dropping a branch's relocation) · `ModifiedAndRemoved` (one `GlobalId` one branch removed while the other modified it past the base — `BaseKey`, `SurvivingKey`, `bool RemovedByOurs`) · `AddedTwiceDivergent` (one `GlobalId` both branches added with no common ancestor and divergent content — `OursKey`, `TheirsKey`) (4) — a content divergence, a placement divergence, a delete/edit race, and a no-base divergent add are the four irreducible element-granularity merge shapes, each carrying its `GlobalId` through the base accessor (the `Review/diff#MODEL_DIFF` `ElementChange.GlobalId` idiom) and never a per-kind conflict class; a `BimCommit` carries its lineage arity in `ParentKeys` rather than a per-kind record — `IsRoot` reads the empty parent set, `IsMerge` reads the multi-parent set — so a root, a linear, and an octopus-merge commit are one record discriminated by the parent count, never a `RootCommit`/`MergeCommit` class family.
- Entry: `BimRepository.Commit(ElementGraph graph, string branch, string author, string message, Instant at)` seals the seam graph as a `BimCommit` child of the branch's current head (a fresh branch roots with no parent), advancing the in-memory branch ref — total, no rail, a commit is one expression over the graph's fingerprint set; `BimRepository.Merge(UInt128 ours, UInt128 theirs, Op key)` resolves both heads, bases on the single-nearest `CommonAncestor`, and folds the three-way `MergeOutcome`, `Fin<T>` railing `Model/faults#FAULT_BAND` `BimFault.DanglingReference` (`merge-commit-absent`) BARE when either head names a commit the working set never declares; `BimRepository.CommitMerge(MergeOutcome resolved, Seq<UInt128> parents, string branch, string author, string message, Instant at, Op key)` seals a CLEAN (or sign-off-resolved) outcome as a merge commit whose parent arity rides the `Seq`, `Fin<T>` railing `BimFault.ModelRejected` (`merge-unresolved-conflicts`) on an outcome still carrying unresolved `Conflicts` so a conflicted merge never auto-commits; `Version.Merge(BimCommit ours, BimCommit theirs, Seq<BimCommit> bases)` is the host-neutral three-way merge — it virtualizes the merge-base antichain into one virtual base then folds the `MergeOutcome`, the algebra the durable owner bases against its `CommitGraph.MergeBase` antichain.
- Auto: `Commit` reads the branch head as the parent (a `Seq<UInt128>` of zero or one), folds `graph.ObjectNodes` into the `Map<string, ElementFingerprint>` through the `Review/diff#MODEL_DIFF` `ModelDiff.Fingerprint(graph, objectNode)` (no second fingerprint), derives the `CommitKey` over the parents-plus-fingerprint-set preimage through `ContentAddress.Of` (excluding the author/message/`Instant` for idempotency), and advances the branch ref in one `with` projection; `History(head)` folds the ancestor-ward child→parent lineage into a `QuikGraph` `BidirectionalGraph<UInt128, SEdge<UInt128>>` and walks it with `BreadthFirstSearchAlgorithm` so a merge commit's two lineages converge without re-visiting (the hand-rolled visited-set BFS retired per `libs/csharp/.api/api-quikgraph`); `CommonAncestor(ours, theirs)` answers the merge-base through `QuikGraph` Tarjan `OfflineLeastCommonAncestor`, rooting at the initial commit and descending the parent→child lineage so the LCA of the two heads IS the deepest common ancestor — the single-nearest in-memory base, the durable maximal-antichain superset being `CommitGraph.MergeBase`; `Version.Merge` virtualizes the base antichain (empty → `BimCommit.Empty` for unrelated histories, one → that base, many → a deterministic per-`GlobalId` fold over the criss-cross/octopus bases) then folds the union of the three commits' `GlobalId` keys, resolving each id by comparing its per-side `ElementFingerprint` to the base — an id changed on only one side takes that side, an id changed convergently on both takes the value, a content divergence surfaces `BothModified`, a content-converged placement divergence surfaces `PlacementDiverged`, a removal honored against an unchanged side drops, a removal racing a modification surfaces `ModifiedAndRemoved` — so the merge weighs BOTH the `ContentKey` and the `PlacementKey`, the auto-merge landing every non-conflicting element and the `MergeConflict` set carrying only the genuine divergences for `Review/coordination#SIGN_OFF`.
- Receipt: the `BimCommit` is the content-addressed history object the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner stores by `CommitKey` (the same content-key the `Exchange/wire#WIRE_PROJECTION` `IfcWire` face and the `Review/diff#MODEL_DIFF` carry), the `BimRepository` the in-memory working graph a federation branches and reconciles before Persistence durably stores the commits, and the `MergeOutcome` the typed three-way merge evidence — the auto-merged fingerprint graph plus the `MergeConflict` set the coordination `SignOff` advances through `Open → InProgress → Resolved → Closed` as each conflict is settled; the commit-DAG, the linear `Review/diff#AUDIT` `AuditTrail`, and the `Exchange/wire#WIRE_PROJECTION` `IfcWire` face read the one content-key space, never a parallel version identity.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, QuikGraph, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a wider commit lineage arity (an octopus merge of three branches) is one longer `ParentKeys` set on the same record and one longer base antichain `Version.Merge` already virtualizes, never a new commit type; a new conflict kind (a both-retyped classification conflict) is one `MergeConflict` union arm the three-way fold surfaces; a new branch operation (a rebase, a cherry-pick) is one fold over the same `ParentKeys` DAG reusing `Version.Merge`; the durable DAG storage, the governed `BranchRef`, and the antichain merge-base ride the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` ripple; never a per-commit-kind record, never a second identity scheme, and never an auto-resolved conflict.
- Boundary: the commit and merge key on the `Review/diff#MODEL_DIFF` `ElementFingerprint.GlobalId` (the IFC `Node.Object.ExternalId`, or the neutral `NodeId` string `ModelDiff.Fingerprint` falls back to off the federation surface so an authored element carrying no IFC `GlobalId` yet is captured, never dropped [H6]) and a parallel commit-local fingerprint or a second identity scheme is the deleted form — the `ElementFingerprint` is diff's, reused verbatim; the `CommitKey` composes the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `CanonicalWriter` + `ContentAddress` (the ONE codec, the ONE seed-zero `XxHash128`) so a hand-rolled `Encoding.UTF8.GetBytes(string.Join(...))` preimage keyed through a second `XxHash128.HashToUInt128` — a delimiter-forgeable encoding the `;`/`|`/`,`/`=` separators collide on — is the named defect `address#SINGLE_HASHER` closes, and the preimage excludes the author/message/`Instant` so a re-commit of the identical model and lineage is idempotent (the prior instant-bearing key broke that invariant); the commit fingerprints the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph.ObjectNodes` and a `BimModel.Elements`/`BimElement` fold over the retired parallel element record is the deleted form (`Model/elements#IFC_CLASS`); `History`/`CommonAncestor` fold a transient `QuikGraph` graph (`BreadthFirstSearchAlgorithm` / Tarjan `OfflineLeastCommonAncestor`) per the shared `libs/csharp/.api/api-quikgraph` substrate and a hand-rolled BFS-intersection over a `Map<>`/`Seq<>` adjacency is the rejected form; the in-memory `BimRepository` is the TRANSIENT working DAG (the commit-DAG counterpart of the seam `WorkingGraph`) and the durable commit-DAG store, the governed `BranchRef` (`RefKind`/ACL/tag/upstream), and the maximal-antichain merge-base are the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` owner's — a durable store minted here is the named seam violation, this owner producing the host-neutral commit objects and the merge algebra joined at the `Review/versioning → csharp:Rasm.Persistence/Version/commits # [CONTENT_KEY]: BimCommit content-addressed commit-DAG` seam by the `CommitKey` (Persistence stores the `BimCommit` as a generic `CommitNode` under the wire-carried key and never re-derives it) and at the `Review/versioning → csharp:Rasm.Persistence/Version/commits # [SHAPE]: BimCommit DAG common-ancestor merge substrate` seam where `Version.Merge` bases against the durable `CommitGraph.MergeBase` antichain; the three-way merge reuses the `Review/diff#MODEL_DIFF` `ElementFingerprint` verbatim and weighs BOTH the `ContentKey` AND the `PlacementKey` so a divergent relocation surfaces `PlacementDiverged` rather than being silently auto-merged (a `ContentKey`-only comparison dropping a branch's placement is the deleted defect), and a field-by-field element comparison beside the fingerprint is the deleted form; this owner is the FINGERPRINT-altitude federation/IFC three-way merge (offline multi-writer reconciliation over the element-identity content keys), distinct from the `csharp:Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` `StructuralMerge.ThreeWay` FOREST-altitude merge (member-level `Generator.Equals` `Inequalities` patches over the full `ElementGraph` topology — `Move`/`Reorder`/`Retype`/`TopologyBreak`), neither re-cased as the other; conflict resolution is the `Review/coordination#SIGN_OFF` `SignOff` lifecycle's concern — `Version.Merge` surfaces the typed `MergeConflict` set and never auto-resolves, so `CommitMerge` rejecting an unresolved outcome is the law and a silent last-write-wins merge is the deleted form; the `MergeConflict` is a closed `[Union]` and a per-kind conflict class is the deleted form; the commit-DAG and the linear `Review/diff#AUDIT` `AuditTrail` are distinct owners over the one content-key — the audit trail the per-element who/when/what linear Merkle chain, the commit-DAG the branching revision graph, neither re-cased as the other; the page is HOST-LOCAL — a `BimCommit` carries the host-free `ElementFingerprint` content keys, never a RhinoCommon type or host-bound geometry, and takes a neutral NodaTime `Instant` rather than the app-platform `csharp:Rasm.AppHost` `ClockPolicy` an AEC-domain owner never references; a versioning rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE (the `Op`-keyed `Expected`-derived case IS the `Error`, the `new BimFault.X("string").ToError()` lowering hop GONE).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Linq;
using LanguageExt;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Element;
using Thinktecture;
using Op = Rasm.Domain.Op;
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

    // Mint a commit from a seam ElementGraph: every Object node's Review/diff#MODEL_DIFF ElementFingerprint folded
    // into the content-addressed commit, keyed by the fingerprint's GlobalId — the IFC ExternalId where present, the
    // neutral NodeId string ModelDiff.Fingerprint falls back to off the federation surface, so an authored element
    // carrying no IFC GlobalId yet is captured [H6]. ModelDiff.Fingerprint is reused verbatim — the commit IS the
    // fingerprint set, never a second element signature.
    public static BimCommit Of(ElementGraph graph, Seq<UInt128> parents, string author, string message, Instant at) =>
        Sealed(parents, graph.ObjectNodes.Map(o => ModelDiff.Fingerprint(graph, o)).Map(static fp => (fp.GlobalId, fp)).ToMap(), author, message, at);

    // The commit identity is content-addressed over the lineage + the diff fingerprint set ONLY — author/message/at
    // are carried metadata EXCLUDED from the key, so a re-commit of the identical model and lineage is genuinely
    // idempotent (an instant-bearing key broke that) and a retroactive content edit mints a divergent key. The
    // preimage rides the seam CanonicalWriter + kernel seed-zero ContentAddress (the ONE codec and the ONE hasher
    // the node/edge/graph addresses use) — a delimiter-forgeable UTF-8 string-join keyed through a second XxHash128
    // is the defect address#SINGLE_HASHER closes.
    public static BimCommit Sealed(Seq<UInt128> parents, Map<string, ElementFingerprint> fingerprints, string author, string message, Instant at) =>
        new(KeyOf(parents, fingerprints), parents, fingerprints, author, message, at);

    // No measure is written, so the tolerance is inert; the fingerprint ContentAddresses are already content-keyed.
    // Parents sort ascending and fingerprints by GlobalId so a reordered parent set or insertion order
    // addresses identically — the order-independence the durable CommitNode shares.
    static UInt128 KeyOf(Seq<UInt128> parents, Map<string, ElementFingerprint> fingerprints) {
        CanonicalWriter w = new(0.0);
        w.Ordinal(parents.Count);
        foreach (UInt128 p in parents.OrderBy(static k => k)) { w.U128(p); }
        w.Ordinal(fingerprints.Count);
        foreach (var (id, fp) in fingerprints.OrderBy(static e => e.Key, StringComparer.Ordinal)) {
            w.String(id).U128(fp.ContentKey.Value).U128(fp.PlacementKey.Value);
        }
        return ContentAddress.Of(w.ToBytes().Span).Value;
    }
}

// The thin in-memory working ref (name -> head CommitKey); the durable governed ref (RefKind/ACL/upstream/
// annotated-tag, the two-sided Movable gate) is the csharp:Rasm.Persistence/Version/commits BranchRef's.
public sealed record BimBranch(string Name, UInt128 Head);

// The closed element-granularity merge-conflict family the three-way fold surfaces for Review/coordination#SIGN_OFF.
// Each arm carries its GlobalId through the base accessor (the Review/diff#MODEL_DIFF ElementChange idiom) — the IFC
// ExternalId or the NodeId fallback the fingerprint key carries; a consumer raising a BCF topic anchors on the
// GlobalId directly. A per-kind conflict class is the deleted form. The divergent ContentAddresses are diff's keys.
[Union]
public abstract partial record MergeConflict {
    private MergeConflict(string globalId) => GlobalId = globalId;

    public string GlobalId { get; }

    public sealed record BothModified(string GlobalId, ContentAddress BaseKey, ContentAddress OursKey, ContentAddress TheirsKey) : MergeConflict(GlobalId);
    public sealed record PlacementDiverged(string GlobalId, ContentAddress BasePlacement, ContentAddress OursPlacement, ContentAddress TheirsPlacement) : MergeConflict(GlobalId);
    public sealed record ModifiedAndRemoved(string GlobalId, ContentAddress BaseKey, ContentAddress SurvivingKey, bool RemovedByOurs) : MergeConflict(GlobalId);
    public sealed record AddedTwiceDivergent(string GlobalId, ContentAddress OursKey, ContentAddress TheirsKey) : MergeConflict(GlobalId);
}

public sealed record MergeOutcome(Map<string, ElementFingerprint> Merged, Seq<MergeConflict> Conflicts) {
    public bool IsClean => Conflicts.IsEmpty;
}

// The in-memory working commit-DAG (the commit-DAG counterpart of the seam WorkingGraph) — transient, never the
// durable store. Persistence stores each BimCommit as a generic CommitNode under the carried CommitKey and owns
// the governed BranchRef + the maximal-antichain MergeBase; this working set folds a QuikGraph lineage per walk.
public sealed record BimRepository(Map<UInt128, BimCommit> Commits, Map<string, BimBranch> Branches) {
    public static readonly BimRepository Empty = new(Map<UInt128, BimCommit>(), Map<string, BimBranch>());

    public Option<BimCommit> Find(UInt128 key) => Commits.Find(key);
    public Option<BimBranch> Branch(string name) => Branches.Find(name);

    // Seal a seam ElementGraph as a child of the branch head (a fresh branch roots with no parent) and advance the
    // in-memory branch ref. The durable store + the ACL-gated branch advance are Persistence's; this is the working
    // advance. Takes a neutral NodaTime Instant — never the app-platform ClockPolicy an AEC owner cannot reference.
    public (BimRepository Repository, BimCommit Commit) Commit(ElementGraph graph, string branch, string author, string message, Instant at) {
        Seq<UInt128> parents = Branches.Find(branch).Map(static b => Seq(b.Head)).IfNone(Seq<UInt128>());
        BimCommit commit = BimCommit.Of(graph, parents, author, message, at);
        return (Advance(branch, commit), commit);
    }

    // The ancestor sequence head -> ... -> root: the ancestor-ward child->parent lineage folded into a QuikGraph
    // BidirectionalGraph and walked breadth-first (api-quikgraph commit-DAG walk; the hand-rolled visited-set BFS
    // retired), discovery order so a merge commit's two lineages converge without re-visiting. The DiscoverVertex
    // fold is the sanctioned QuikGraph kernel boundary — the imperative walk collapses INTO the package, the domain
    // algebra stays pure.
    public Seq<BimCommit> History(UInt128 head) {
        BidirectionalGraph<UInt128, SEdge<UInt128>> lineage = Lineage(ancestorward: true);
        if (!lineage.ContainsVertex(head)) { return Seq<BimCommit>(); }
        System.Collections.Generic.List<UInt128> order = [];
        BreadthFirstSearchAlgorithm<UInt128, SEdge<UInt128>> bfs = new(lineage);
        bfs.DiscoverVertex += order.Add;
        bfs.Compute(head);
        return toSeq(order).Choose(Commits.Find);
    }

    // The merge-base via QuikGraph Tarjan OfflineLeastCommonAncestor (api-quikgraph commit-DAG merge-base; the
    // hand-rolled BFS-intersection LCA is the rejected form). The Tarjan offline form roots at the PARENTLESS root
    // of ours's lineage (the IsRoot ancestor — the one commit from which BOTH heads descend when histories are
    // shared; a merge diamond's BFS-farthest ancestor is NOT the root, so rooting there would fail to reach theirs
    // and mis-report shared history as disjoint) and descends the parent->child lineage, so the LCA of the two heads
    // IS the deepest common ancestor — the single-nearest in-memory base, the durable maximal-antichain superset
    // being csharp:Rasm.Persistence CommitGraph.MergeBase. Disjoint histories (the LCA query returning false, or no
    // shared root) fold the merge against BimCommit.Empty.
    public Option<UInt128> CommonAncestor(UInt128 ours, UInt128 theirs) =>
        History(ours).Filter(static c => c.IsRoot).HeadOrNone().Map(static c => c.CommitKey).Bind(root => {
            SEquatableEdge<UInt128> pair = new(ours, theirs);
            TryFunc<SEquatableEdge<UInt128>, UInt128> lca = Lineage(ancestorward: false).OfflineLeastCommonAncestor(root, [pair]);
            return lca(pair, out UInt128 mergeBase) ? Some(mergeBase) : Option<UInt128>.None;
        });

    // Resolve both heads, base on the single-nearest CommonAncestor, fold the three-way over the fingerprint maps;
    // an absent head rails BimFault.DanglingReference BARE. The durable path supplies the CommitGraph.MergeBase
    // antichain to Version.Merge directly, which virtualizes 0/1/N bases — one algebra, two base sources.
    public Fin<MergeOutcome> Merge(UInt128 ours, UInt128 theirs, Op key) =>
        from o in Commits.Find(ours).ToFin(new BimFault.DanglingReference(key, $"merge-commit-absent:{ours:X32}"))
        from t in Commits.Find(theirs).ToFin(new BimFault.DanglingReference(key, $"merge-commit-absent:{theirs:X32}"))
        let bases = CommonAncestor(ours, theirs).Bind(Commits.Find).ToSeq()
        select Version.Merge(o, t, bases);

    // Seal a CLEAN (or sign-off-resolved) outcome as a merge commit + advance the branch; an outcome still carrying
    // unresolved Conflicts rails ModelRejected BARE so a conflicted merge NEVER auto-commits — conflict resolution
    // is the Review/coordination#SIGN_OFF lifecycle's concern. The parent arity rides the Seq, so a pairwise merge
    // passes the two heads and an octopus merge the N heads — one entry, the lineage arity in the value.
    public Fin<(BimRepository Repository, BimCommit Commit)> CommitMerge(
        MergeOutcome resolved, Seq<UInt128> parents, string branch, string author, string message, Instant at, Op key) =>
        resolved.IsClean
            ? Fin.Succ(Seal(BimCommit.Sealed(parents, resolved.Merged, author, message, at), branch))
            : Fin.Fail<(BimRepository, BimCommit)>(new BimFault.ModelRejected(key, $"merge-unresolved-conflicts:{resolved.Conflicts.Count}"));

    (BimRepository Repository, BimCommit Commit) Seal(BimCommit commit, string branch) => (Advance(branch, commit), commit);

    BimRepository Advance(string branch, BimCommit commit) => this with {
        Commits = Commits.AddOrUpdate(commit.CommitKey, commit),
        Branches = Branches.AddOrUpdate(branch, new BimBranch(branch, commit.CommitKey)),
    };

    // The commit lineage as a transient QuikGraph BidirectionalGraph (the vertex IS the UInt128 CommitKey) — a
    // per-walk fold, never a stored domain field. `ancestorward` true gives child->parent out-edges (the History
    // ancestor BFS reaches a head's parents); false gives parent->child out-edges (the CommonAncestor Tarjan LCA
    // descends from the root initial commit to both heads) — one fold, the edge orientation the only policy.
    BidirectionalGraph<UInt128, SEdge<UInt128>> Lineage(bool ancestorward) {
        BidirectionalGraph<UInt128, SEdge<UInt128>> dag = new(allowParallelEdges: false);
        foreach (BimCommit commit in Commits.Values) {
            dag.AddVertex(commit.CommitKey);
            foreach (UInt128 parent in commit.ParentKeys) {
                dag.AddVerticesAndEdge(ancestorward ? new SEdge<UInt128>(commit.CommitKey, parent) : new SEdge<UInt128>(parent, commit.CommitKey));
            }
        }
        return dag;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Version {
    // The host-neutral three-way merge: virtualize the merge-base antichain (the durable CommitGraph.MergeBase
    // superset, or the in-memory single-nearest) into one base, then fold the union of the three commits' GlobalId
    // keys. Reuses the Review/diff#MODEL_DIFF ElementFingerprint verbatim — no second identity. The key universe
    // sorts by GlobalId so the merged map and the conflict sequence are deterministic.
    public static MergeOutcome Merge(BimCommit ours, BimCommit theirs, Seq<BimCommit> bases) =>
        ThreeWay(ours, theirs, VirtualBase(bases));

    // The criss-cross/octopus virtual base: the durable CommitGraph.MergeBase yields the maximal common-ancestor
    // ANTICHAIN, so a clean history gives one base, a criss-cross two-or-more. Empty -> BimCommit.Empty (unrelated
    // histories), one -> that base, many -> the union of every base's fingerprints keeping per GlobalId the
    // deterministically-minimal ContentKey — a STABLE virtual ancestor that reduces the antichain by a deterministic
    // min-key heuristic, NOT a recursive 3-way merge of the bases: CommitGraph.MergeBase owns the antichain SET
    // computation, this fold its deterministic per-GlobalId reduction on BOTH the in-memory and durable paths, so a
    // criss-cross resolves against this one virtual base and an ours/theirs divergence relative to it surfaces as a
    // typed conflict.
    static BimCommit VirtualBase(Seq<BimCommit> bases) =>
        bases.Fold(BimCommit.Empty, static (acc, b) => acc with {
            Fingerprints = b.Fingerprints.Fold(acc.Fingerprints, static (map, id, fp) =>
                map.AddOrUpdate(id, prior => fp.ContentKey.Value < prior.ContentKey.Value ? fp : prior, fp)),
        });

    static MergeOutcome ThreeWay(BimCommit ours, BimCommit theirs, BimCommit mergeBase) =>
        toSeq(ours.Fingerprints.Keys.Concat(theirs.Fingerprints.Keys).Concat(mergeBase.Fingerprints.Keys).Distinct().OrderBy(static id => id, StringComparer.Ordinal))
            .Fold(new MergeOutcome(Map<string, ElementFingerprint>(), Seq<MergeConflict>()), (acc, id) => {
                var (conflict, keep) = Resolve(id, ours.Fingerprints.Find(id), theirs.Fingerprints.Find(id), mergeBase.Fingerprints.Find(id));
                return new MergeOutcome(
                    keep.Match(Some: fp => acc.Merged.AddOrUpdate(id, fp), None: () => acc.Merged),
                    conflict.Match(Some: c => acc.Conflicts.Add(c), None: () => acc.Conflicts));
            });

    // Resolve one GlobalId across the three sides into (conflict?, keep?): keep=Some lands the merged fingerprint,
    // keep=None is a converged removal, conflict=Some routes the divergence to SignOff. The eight-case three-way
    // truth table threaded through Option.Match (no unsafe value access); the both-sides-absent rows fold to a
    // converged removal, and the all-absent row never enumerates (the id would not be in the key universe).
    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) Resolve(
        string id, Option<ElementFingerprint> ours, Option<ElementFingerprint> theirs, Option<ElementFingerprint> mergeBase) =>
        ours.Match(
            Some: o => theirs.Match(
                Some: t => BothSides(id, o, t, mergeBase),
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
        string id, ElementFingerprint ours, ElementFingerprint theirs, Option<ElementFingerprint> mergeBase) {
        bool oursChanged = mergeBase.Match(Some: b => !Same(b, ours), None: static () => true);
        bool theirsChanged = mergeBase.Match(Some: b => !Same(b, theirs), None: static () => true);
        return (oursChanged, theirsChanged) switch {
            (false, _)                => (Option<MergeConflict>.None, Some(theirs)),
            (_, false)                => (Option<MergeConflict>.None, Some(ours)),
            _ when Same(ours, theirs) => (Option<MergeConflict>.None, Some(ours)),
            _                         => (Some(Divergence(id, ours, theirs, mergeBase)), Option<ElementFingerprint>.None),
        };
    }

    // The divergent-edit conflict, axis-typed: a content divergence is BothModified (or AddedTwiceDivergent with no
    // base), a content-CONVERGENT placement-only divergence is PlacementDiverged — so a pure divergent relocation
    // surfaces its own conflict the prior ContentKey-only merge silently auto-merged, dropping a branch's placement.
    static MergeConflict Divergence(string id, ElementFingerprint ours, ElementFingerprint theirs, Option<ElementFingerprint> mergeBase) =>
        mergeBase.Match(
            Some: b => ours.ContentKey != theirs.ContentKey
                ? (MergeConflict)new MergeConflict.BothModified(id, b.ContentKey, ours.ContentKey, theirs.ContentKey)
                : new MergeConflict.PlacementDiverged(id, b.PlacementKey, ours.PlacementKey, theirs.PlacementKey),
            None: () => new MergeConflict.AddedTwiceDivergent(id, ours.ContentKey, theirs.ContentKey));

    // One side removed, the other survives: an UNCHANGED surviving side (content AND placement match the base)
    // honors the removal (drop); any change races the removal into ModifiedAndRemoved — the full-signature check
    // so a relocation racing a removal is never silently dropped.
    static (Option<MergeConflict> Conflict, Option<ElementFingerprint> Keep) RemovedVsModified(
        string id, ElementFingerprint surviving, ElementFingerprint mergeBase, bool removedByOurs) =>
        Same(surviving, mergeBase)
            ? (Option<MergeConflict>.None, Option<ElementFingerprint>.None)
            : (Some<MergeConflict>(new MergeConflict.ModifiedAndRemoved(id, mergeBase.ContentKey, surviving.ContentKey, removedByOurs)), Option<ElementFingerprint>.None);

    // The full element signature — content AND placement — so the merge weighs a relocation, never the ContentKey alone.
    static bool Same(ElementFingerprint a, ElementFingerprint b) => a.ContentKey == b.ContentKey && a.PlacementKey == b.PlacementKey;
}
```

## [03]-[RESEARCH]

- [COMMIT_DAG]: the `BimCommit` content-addressed commit object reuses the `Review/diff#MODEL_DIFF` `ElementFingerprint` verbatim (the settled `(string GlobalId, ContentAddress ContentKey, ContentAddress PlacementKey)` carrier diff owns and explicitly names "the SAME `ElementFingerprint` the `Review/versioning#VERSION_GRAPH` `BimCommit` keys its `Map<string, ElementFingerprint>` on"), the `GlobalId` the Bim-stored `Node.Object.ExternalId` where present and the neutral `NodeId` string the `ModelDiff.Fingerprint(graph, node)` fold falls back to off the federation surface [H6] — so a commit over the working graph captures an authored element with no IFC `GlobalId` yet, never dropping it, and the commit is the per-element fingerprint set (a commit is the fingerprint map, a diff is two commits via `ModelDiff.Between`, a merge folds two fingerprint streams against a virtual base); the `CommitKey` derives through the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.Of` over a `CanonicalWriter` preimage (the ONE seed-zero `XxHash128` and the ONE canonical codec the node/edge/graph addresses and the `Review/diff#AUDIT` `AuditEntry.EntryKey` ride), excluding the author/message/`Instant` for re-commit idempotency — a hand-rolled `Encoding.UTF8.GetBytes(string.Join("|", ...))` preimage hashed through a second `System.IO.Hashing` `XxHash128.HashToUInt128` is the named drift defect `address#SINGLE_HASHER` closes; the `BimRepository` `History`/`CommonAncestor` are transient `QuikGraph` folds over the `ParentKeys` lineage as a `BidirectionalGraph<UInt128, SEdge<UInt128>>` (the `libs/csharp/.api/api-quikgraph` commit-DAG entry the README roster registers) — `History` a `BreadthFirstSearchAlgorithm` discovery walk over the ancestor-ward child→parent orientation, and `CommonAncestor` a Tarjan `OfflineLeastCommonAncestor` rooting at the initial commit over the parent→child orientation (the only orientation whose out-edge descent reaches both heads, so the LCA is the deepest common ancestor), the hand-rolled visited-set BFS and BFS-intersection LCA retired.
- [THREE_WAY_MERGE]: the `Version.Merge` fold is the standard three-way reconciliation over the content-keyed fingerprint maps — it virtualizes the merge-base ANTICHAIN (`VirtualBase` folding a criss-cross/octopus base set into one virtual base by the deterministic per-`GlobalId` minimal `ContentKey`, the deterministic virtual-base discipline `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` "a criss-cross yields the two-or-more bases the three-way merge virtualizes" prescribes) then resolves each `GlobalId` by comparing its per-side `ElementFingerprint` against the base, weighing BOTH the `ContentKey` and the `PlacementKey` (an id changed on one side takes that side, a convergent change on both takes the value, a content divergence surfaces `MergeConflict.BothModified`, a content-converged placement divergence surfaces `MergeConflict.PlacementDiverged`, a remove racing a modification surfaces `MergeConflict.ModifiedAndRemoved`, a no-common-ancestor divergent add surfaces `MergeConflict.AddedTwiceDivergent`); the `PlacementDiverged` arm closes the correctness gap a `ContentKey`-only comparison left — a divergent relocation on both branches was silently auto-merged, dropping a branch's placement — and the `MergeConflict` set is resolved by the `Review/coordination#SIGN_OFF` `SignOff` lifecycle, `Version.Merge` never auto-resolving and `BimRepository.CommitMerge` rejecting an outcome still carrying `Conflicts` per the IDEAS `[VERSIONED]` "conflict resolution is the coordination sign-off's concern, not an auto-resolve" law; the `Resolve` eight-case three-way truth table threads through `Option.Match` rather than unsafe value access, the both-sides-absent rows folding to a converged removal and the all-absent row never enumerated.
- [PERSISTENCE_SEAM]: the durable commit-DAG storage, the governed `BranchRef` (`RefKind`/ACL/upstream/annotated-tag, the two-sided `Movable` gate), and the maximal-antichain merge-base are the `csharp:Rasm.Persistence/Version/commits#COMMIT_DAG` Version owner's concern joined at the `Review/versioning → csharp:Rasm.Persistence/Version/commits # [CONTENT_KEY]: BimCommit content-addressed commit-DAG` seam by the `CommitKey` — Persistence stores the `BimCommit` as a generic `CommitNode` under the wire-carried key without re-deriving it (the `commit` column-family `OpLogEntry` riding the Marten changefeed), this owner producing the content-addressed commit objects and the merge algebra; the `Review/versioning → csharp:Rasm.Persistence/Version/commits # [SHAPE]: BimCommit DAG common-ancestor merge substrate` seam bases `Version.Merge` on the durable `CommitGraph.MergeBase` maximal common-ancestor antichain (the durable superset of this owner's single-nearest in-memory base, the `VirtualBase` fold consuming the antichain directly); this owner is the FINGERPRINT-altitude federation/IFC three-way merge (the offline multi-writer reconciliation §4-RT H11 keeps first-class), distinct from the `csharp:Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` `StructuralMerge.ThreeWay` FOREST-altitude merge over the full `ElementGraph` topology (the `Generator.Equals` `Inequalities` member-level patches, `Move`/`Reorder`/`Retype`/`TopologyBreak`/`ContainmentCycle`), the two altitude-split and neither re-cased as the other; the Marten event substrate sits BENEATH the preserved op-log/CRDT/time-travel engine [H11], so the commit-DAG, the linear `Review/diff#AUDIT` `AuditTrail`, and the `Exchange/wire#WIRE_PROJECTION` `IfcWire` face all read the one content-key space, Bim minting no second identity scheme, no durable store, and no auto-resolve.
