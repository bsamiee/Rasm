# [RASM_HEALING_RECEIPTS]

This page mints the typed heal evidence — `ManifoldStatus` snapshots, the `RebuildReceipt` per-op union, the `HealSession` carrier, and the `RebuildLog` re-anchor fold — that a `Heal.Repair` session emits and the naming `Track` re-anchor consumes. Every record is an interior type crossing only the in-process seam to the naming fold, never sitting between wire and rail.

`ManifoldStatus` is the composed `Rasm.Meshing` `TopologyReceipt` projected through the Genus-tolerant six-field row, un-gated so the non-manifold, boundaried, or odd-Euler meshes the heal exists for snapshot instead of failing. Each `RebuildReceipt` case registers its convergence witness as `IValidityEvidence.IsValid`, `HealSession.IsValid` folds `ValidityClaim.All` over the chain, and the boolean case carries the arrangement `BooleanReceipt` as payload.

## [01]-[INDEX]

- [02]-[REBUILD_RECEIPTS]: `ManifoldStatus` Genus-tolerant projection + `GenusClosed` witness; `RebuildReceipt` `[Union]` typed per-op evidence registering `IValidityEvidence`; `HealSession` carrier folding `ValidityClaim.All` over the chain; `RebuildLog` re-anchor seed.

## [02]-[REBUILD_RECEIPTS]

- Owner: `ManifoldStatus` the before/after topological snapshot — six scalars the `VectorIntent.Topology` projection seam yields from the composed `Rasm.Meshing` `TopologyReceipt` via the Genus-tolerant `ProjectionRow`, never re-counted: `NonManifoldEdges` is the actionable defect count the manifold kernel targets and the gap bridge's coherence witness (`BoundaryComponents` moves ±1 per bridge, so the count is evidence, never law), and `Genus` stays `Option<int>` because a non-manifold or non-oriented snapshot has no validated genus — exactly the input class the heal admits; `GenusClosed` derives the closed-target witness on a manifold, computed never stored; `RebuildReceipt` `[Union]` mints one typed case per `HealOp` carrying the op's policy tolerance, before/after `ManifoldStatus`, and the affected index sets seeded from the arena dirty bitsets, each case registering its convergence witness as `IsValid`; `HealSession` carries the input mesh, healed mesh, and ordered receipt chain, its `IsValid` the `ValidityClaim.All` fold the corpus validity oracle reads; `RebuildLog` flattens a session into the `(EntityKind, affected-index-set)` re-anchor input the naming `Track` reads.
- Cases: `RebuildReceipt` cases `DegenerateReceipt` · `GapReceipt` · `WeldReceipt` · `ManifoldReceipt` · `SelfIntersectReceipt` · `OrientReceipt` · `MergeReceipt`, one per `HealOp`; the boolean case is `MergeReceipt` carrying the composed arrangement `BooleanReceipt`. `ManifoldStatus` is one record carrying the six projected scalars and the derived `GenusClosed` witness.
- Entry: `public static RebuildReceipt Of(HealOp op, RepairPolicy policy, ManifoldStatus before, ManifoldStatus after, MeshEdit result, Option<BooleanReceipt> merge)` mints the typed receipt for an applied op — before/after status arrives already projected through the Genus-tolerant seam (the heal session binds the projection on the `Fin` rail before minting), the policy travels beside the stateless op so each case records its tolerance (`policy.Arena.WeldTolerance` on the weld, `policy.SliverAreaFloor` on the collapse, `policy.GapMaxSpan` on the gap, `policy.MaxManifoldPasses` on the split), the affected seeds read the arena dirty bitsets (`result.DirtyFaces()`/`DirtyVertices()` — monotone within an arena, so a seed over-approximates but never misses an entity, and a boolean's fresh arena admits every slot dirty), and the boolean arm carries the `merge` the `Heal.Merge` step forwarded; `public RebuildLog ToLog()` on `HealSession` folds the chain into the per-`EntityKind` affected-ref set, filtering by `HealStage.RebuildsTopology` so an `OrientNormals` op contributes nothing (winding leaves adjacency and the `TopoSignature` unchanged).
- Auto: each `RebuildReceipt` case derives its convergence witness from the six-field delta it carries — `WeldReceipt`/`DegenerateReceipt`/`SelfIntersectReceipt` assert no new non-manifold edges (a weld also never opens boundary), `GapReceipt` asserts a landed bridge minted no non-manifold edge (a mis-paired strip trebles an edge and fires the witness) while `BoundaryComponents` movement stays evidence (a cross-gap bridge merges two loops −1, a slit bridge splits its loop +1, a hole closure retires one, so a count law in either direction is the trap the global boundary heuristic already breaks), `ManifoldReceipt` asserts `NonManifoldEdges` reached zero (boundary regression is admitted — the vertex-copy split opens boundary a later gap pass may close), `OrientReceipt` asserts `IsOriented` with the Euler characteristic unchanged, and `MergeReceipt` asserts `After.GenusClosed`, the boolean's own topological success witness rather than a gate flag; `HealSession.IsValid` folds `ValidityClaim.All` over the non-empty chain of per-receipt witnesses — the one registered convergence surface.
- Receipt: this cluster is the receipt owner — the `RebuildReceipt` chain on the `HealSession` is the heal evidence the naming `Track` consumes, returned in the `Heal.Repair` rail; `ManifoldStatus` is the composed `TopologyReceipt` projection and the boolean payload the composed arrangement `BooleanReceipt`, neither re-computed here.
- Packages: `Rasm.Meshing` (`TopologyReceipt` via the Genus-tolerant `Rasm.Numerics` `ProjectionRow`; `MeshEdit` dirty-bitset seed; `BooleanOp`/`BooleanReceipt` + `Empty` — the composed payload, `BooleanRoute` inside the receipt), Rasm.Domain (`IValidityEvidence`/`ValidityClaim` — the registered validity fold), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new heal op is one `RebuildReceipt` case carrying its typed evidence and its own `IsValid` witness arm (mirroring its `HealOp` case — the generated `Switch` breaks every dispatch site until the arm lands); a new topological status field is one column on `ManifoldStatus` projected from the existing `TopologyReceipt` carrier and one `ProjectionRow` widening at the mesh.md seam.
- Boundary: `RebuildReceipt` stays the typed per-kind union — a `WeldReceipt`'s merged-vertex set and a `ManifoldReceipt`'s forked-face set are different shapes carried by different cases; the before/after status is the composed `Rasm.Meshing` `TopologyReceipt` projected through the un-gated six-field row; the boolean payload is the arrangement `BooleanReceipt`; convergence registers as `IValidityEvidence`. `RebuildLog` feeds the naming `Track` re-anchor and the receipt's affected-ref set is the re-anchor seed, so a topology-rebuilding op that emits no affected entities re-anchors the naming fold blind; `Affected` seeds `Edges` empty for every op because the arena keys topology by face triples and the `Track` resolves edges through `VertexNames`. Op tolerance and payload evidence ride the receipt, which mints no hash and asserts no content identity — the healed mesh's content hash is the reconciliation `Encode` job, the receipt only naming which entities changed so the reference identity (`TopoName`) re-binds.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Rasm.Meshing;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Processing;

// --- [MODELS] -----------------------------------------------------------------------------
// Genus-tolerant six-field snapshot, total over non-manifold/boundaried/odd-Euler meshes; Option marks unvalidated genus absent, never a sentinel.
public readonly record struct ManifoldStatus(
    int EulerCharacteristic, int BoundaryComponents, bool IsManifold, bool IsOriented,
    int NonManifoldEdges, Option<int> Genus) {
    public static ManifoldStatus Of((int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus) projection) =>
        new(projection.Euler, projection.BoundaryComponents, projection.IsManifold, projection.IsOriented, projection.NonManifoldEdges, projection.Genus);

    // Genus-consistent closed target — the boolean's success predicate.
    public bool GenusClosed =>
        Genus.Match(
            Some: genus => IsManifold && BoundaryComponents == 0 && NonManifoldEdges == 0
                && EulerCharacteristic == 2 - (2 * genus),
            None: static () => false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RebuildReceipt : IValidityEvidence {
    private RebuildReceipt() { }

    public sealed record DegenerateReceipt(double SliverFloor, ManifoldStatus Before, ManifoldStatus After, Set<int> CollapsedFaces) : RebuildReceipt;
    public sealed record GapReceipt(double MaxSpan, ManifoldStatus Before, ManifoldStatus After, Set<int> BridgedFaces, Set<int> StitchedVertices) : RebuildReceipt;
    public sealed record WeldReceipt(double Tolerance, ManifoldStatus Before, ManifoldStatus After, Set<int> MergedVertices) : RebuildReceipt;
    public sealed record ManifoldReceipt(int PassBudget, ManifoldStatus Before, ManifoldStatus After, Set<int> ForkedFaces, Set<int> ForkedVertices) : RebuildReceipt;
    public sealed record SelfIntersectReceipt(ManifoldStatus Before, ManifoldStatus After, Set<int> RetiledFaces, Set<int> MintedVertices) : RebuildReceipt;
    public sealed record OrientReceipt(ManifoldStatus Before, ManifoldStatus After, Set<int> FlippedFaces) : RebuildReceipt;
    public sealed record MergeReceipt(BooleanOp Op, BooleanReceipt Merge, ManifoldStatus Before, ManifoldStatus After, Set<int> SelectedFaces, Set<int> SelectedVertices) : RebuildReceipt;

    public HealStage Stage =>
        Switch(
            degenerateReceipt:    static _ => HealStage.Degenerate,
            gapReceipt:           static _ => HealStage.Gap,
            weldReceipt:          static _ => HealStage.Weld,
            manifoldReceipt:      static _ => HealStage.Manifold,
            selfIntersectReceipt: static _ => HealStage.SelfIntersect,
            orientReceipt:        static _ => HealStage.Orient,
            mergeReceipt:         static _ => HealStage.Boolean);

    public ManifoldStatus StatusBefore =>
        Switch(
            degenerateReceipt:    static d => d.Before, gapReceipt:    static g => g.Before,
            weldReceipt:          static w => w.Before, manifoldReceipt: static m => m.Before,
            selfIntersectReceipt: static s => s.Before, orientReceipt: static o => o.Before,
            mergeReceipt:         static m => m.Before);

    public ManifoldStatus StatusAfter =>
        Switch(
            degenerateReceipt:    static d => d.After, gapReceipt:    static g => g.After,
            weldReceipt:          static w => w.After, manifoldReceipt: static m => m.After,
            selfIntersectReceipt: static s => s.After, orientReceipt: static o => o.After,
            mergeReceipt:         static m => m.After);

    // Per-op witness registered into the validity fold: each case tests only the six-field delta its kernel moves. Boundary
    // COUNT is never law — a cross-gap bridge merges loops (−1), a slit splits one (+1) — so the gap witness is manifold-coherence.
    public bool IsValid =>
        Switch(
            degenerateReceipt:    static d => d.After.NonManifoldEdges <= d.Before.NonManifoldEdges,
            gapReceipt:           static g => g.BridgedFaces.IsEmpty
                ? g.After.BoundaryComponents <= g.Before.BoundaryComponents
                : g.After.NonManifoldEdges <= g.Before.NonManifoldEdges,
            weldReceipt:          static w => w.After.NonManifoldEdges <= w.Before.NonManifoldEdges
                && w.After.BoundaryComponents <= w.Before.BoundaryComponents,
            manifoldReceipt:      static m => m.After.NonManifoldEdges == 0,
            selfIntersectReceipt: static s => s.After.NonManifoldEdges <= s.Before.NonManifoldEdges,
            orientReceipt:        static o => o.After.IsOriented && o.After.EulerCharacteristic == o.Before.EulerCharacteristic,
            mergeReceipt:         static m => m.After.GenusClosed);

    public static RebuildReceipt Of(HealOp op, RepairPolicy policy, ManifoldStatus before, ManifoldStatus after, MeshEdit result, Option<BooleanReceipt> merge) {
        Set<int> faces = toSet(result.DirtyFaces());
        Set<int> vertices = toSet(result.DirtyVertices());
        return op.Switch<RebuildReceipt>(
            duplicateWeld:        _ => new WeldReceipt(policy.Arena.WeldTolerance, before, after, vertices),
            degenerateCollapse:   _ => new DegenerateReceipt(policy.SliverAreaFloor, before, after, faces),
            gapClose:             _ => new GapReceipt(policy.GapMaxSpan.Value, before, after, faces, vertices),
            manifoldRepair:       _ => new ManifoldReceipt(policy.MaxManifoldPasses.Value, before, after, faces, vertices),
            selfIntersectResolve: _ => new SelfIntersectReceipt(before, after, faces, vertices),
            orientNormals:        _ => new OrientReceipt(before, after, faces),
            boolean:              b => new MergeReceipt(
                b.Op,
                // Unreachable None: a gated boolean fails the arrangement rail before any mint.
                merge.IfNone(BooleanReceipt.Empty),
                before, after, faces, vertices));
    }

    public (Set<int> Vertices, Set<int> Edges, Set<int> Faces) Affected =>
        Switch(
            degenerateReceipt:    static d => (Set<int>.Empty, Set<int>.Empty, d.CollapsedFaces),
            gapReceipt:           static g => (g.StitchedVertices, Set<int>.Empty, g.BridgedFaces),
            weldReceipt:          static w => (w.MergedVertices, Set<int>.Empty, Set<int>.Empty),
            manifoldReceipt:      static m => (m.ForkedVertices, Set<int>.Empty, m.ForkedFaces),
            selfIntersectReceipt: static s => (s.MintedVertices, Set<int>.Empty, s.RetiledFaces),
            orientReceipt:        static _ => (Set<int>.Empty, Set<int>.Empty, Set<int>.Empty),
            mergeReceipt:         static m => (m.SelectedVertices, Set<int>.Empty, m.SelectedFaces));
}

// Ops carries the HealStage vocabulary itself — consumers read the typed row or its Key, never a re-parsed string.
public sealed record RebuildLog(Set<int> Vertices, Set<int> Edges, Set<int> Faces, Seq<HealStage> Ops) {
    public static readonly RebuildLog Empty = new(Set<int>.Empty, Set<int>.Empty, Set<int>.Empty, Seq<HealStage>());

    public bool ReanchorsLineage => !Vertices.IsEmpty || !Edges.IsEmpty || !Faces.IsEmpty;
}

public sealed record HealSession(MeshSpace Input, MeshSpace Healed, Seq<RebuildReceipt> Receipts) : IValidityEvidence {
    public Option<ManifoldStatus> FinalStatus => Receipts.Last.Map(static receipt => receipt.StatusAfter);

    // THE registered convergence surface: ValidityClaim.All over the per-receipt witnesses.
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Receipts.Count, floor: 1),
        ValidityClaim.Of(Receipts.ForAll(static receipt => receipt.IsValid)));

    // Re-anchor seed: the RebuildsTopology column selects the contributing stages.
    public RebuildLog ToLog() =>
        Receipts.Filter(static receipt => receipt.Stage.RebuildsTopology)
            .Fold(RebuildLog.Empty, static (log, receipt) => {
                (Set<int> v, Set<int> e, Set<int> f) = receipt.Affected;
                return log with {
                    Vertices = log.Vertices.TryAddRange(v),
                    Edges = log.Edges.TryAddRange(e),
                    Faces = log.Faces.TryAddRange(f),
                    Ops = log.Ops.Add(receipt.Stage),
                };
            });
}
```

## [03]-[DENSITY_BAR]

Each `[RAIL]` cell names the one return rail its owner exposes; the receipts are pure carriers returned in the `Heal.Repair` rail.

| [INDEX] | [AXIS_CONCERN]           | [OWNER]                    | [RAIL]                                      | [CASES] |
| :-----: | :----------------------- | :------------------------- | :------------------------------------------ | :-----: |
|  [01]   | Rebuild receipt          | `RebuildReceipt`           | carrier (returned in `Heal.Repair` rail)    |    7    |
|  [02]   | Topological status       | `ManifoldStatus`           | `ManifoldStatus.Of → ManifoldStatus` (pure) |    —    |
|  [03]   | Heal session + re-anchor | `HealSession`/`RebuildLog` | `HealSession.ToLog → RebuildLog` (pure)     |    —    |

`RebuildReceipt`, the `ManifoldStatus` projection, and the `HealSession`/`RebuildLog` fold are transcription-complete pure-managed fences composing the `TopologyReceipt` projection seam, the arrangement `BooleanReceipt` payload, and the arena dirty bitsets — none depending on a live-host member spelling beyond the stable native `Mesh` surface the topology sibling pins.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
