# [RASM_HEALING_RECEIPTS]

This page mints the typed heal evidence — `ManifoldStatus` snapshots, the `RebuildReceipt` per-op union, the `HealSession` carrier, and the `RebuildLog` re-anchor fold — that a `Heal.Repair` session emits and the naming `Track` re-anchor consumes. Every record is an interior type crossing only the in-process seam to the naming fold, never sitting between wire and rail.

`ManifoldStatus` is the composed `Rasm.Meshing` `TopologyReceipt` projected through the Genus-tolerant six-field row, un-gated so the non-manifold, boundaried, or odd-Euler meshes the heal exists for snapshot instead of failing. Each `RebuildReceipt` case registers its convergence witness as `IValidityEvidence.IsValid`, `HealSession.IsValid` folds `ValidityClaim.All` over the chain, and the boolean case carries the arrangement `BooleanReceipt` as payload. Every band a case records is the derived `Tolerance` carrier — the arena's own bound `Context` resolved through the lane `RepairPolicy` names — never a bare double the receipt cannot trace to a lane.

## [01]-[INDEX]

- [02]-[REBUILD_RECEIPTS]: `ManifoldStatus` Genus-tolerant projection + `GenusClosed` witness; `RebuildReceipt` `[Union]` typed per-op evidence registering `IValidityEvidence`; `HealSession` carrier folding `ValidityClaim.All` over the chain; `RebuildLog` re-anchor seed.

## [02]-[REBUILD_RECEIPTS]

- Owner: `ManifoldStatus` the before/after topological snapshot — six scalars the `VectorIntent.Topology` projection seam yields from the composed `Rasm.Meshing` `TopologyReceipt` via the Genus-tolerant `ProjectionRow`, never re-counted: `NonManifoldEdges` is the projected defect count every delta witness reads and the gap bridge's coherence witness (`BoundaryComponents` moves ±1 per bridge, so the count is evidence, never law) while the manifold split's own convergence rides `ManifoldReceipt.ArenaResidual`, the arena fold being the single authority a position-keyed topology-vertex re-merge cannot erase, and `Genus` stays `Option<int>` because a non-manifold or non-oriented snapshot has no validated genus — exactly the input class the heal admits; `GenusClosed` derives the closed-target witness on a manifold, computed never stored; `RebuildReceipt` `[Union]` mints one typed case per `HealOp` carrying the op's band as a `Tolerance`, before/after `ManifoldStatus`, and the affected index sets seeded from the arena dirty bitsets, each case registering its convergence witness as `IsValid`; `HealSession` carries the input mesh, healed mesh, and ordered receipt chain, its `IsValid` the `ValidityClaim.All` fold the corpus validity oracle reads; `RebuildLog` flattens a session into the `(EntityKind, affected-index-set)` re-anchor input the naming `Track` reads.
- Cases: `RebuildReceipt` cases `DegenerateReceipt` · `GapReceipt` · `WeldReceipt` · `ManifoldReceipt` · `SelfIntersectReceipt` · `OrientReceipt` · `MergeReceipt`, one per `HealOp`; the boolean case is `MergeReceipt` carrying the composed arrangement `BooleanReceipt`. `ManifoldStatus` is one record carrying the six projected scalars and the derived `GenusClosed` witness.
- Entry: `ReceiptSeed.Of(HealOp, RepairPolicy, ManifoldStatus before, ManifoldStatus after, MeshEdit, HealStep, Op)` packs one mint payload and `stage.Receipt(seed)` mints the typed receipt on the `Fin` rail — before/after status arrives already projected through the Genus-tolerant seam (the heal session binds the projection before minting), the policy travels beside the stateless op so each case records its band (`seed.Context.For(policy.Arena.Weld)` on the weld, `.For(policy.Sliver)` on the collapse, `.For(policy.Gap)` on the bridge, `policy.MaxManifoldPasses` on the split), the affected seeds read the arena dirty bitsets (`result.DirtyFaces()`/`DirtyVertices()` — admission clears both, so a set bit names a kernel edit; marks still accumulate across the ops sharing one arena, so a seed over-approximates forward but never misses an entity, and the session-level union `ToLog` folds is identical either way), the boolean arm reads the whole arena extent instead because its arena is wholly new material no bit distinguishes, and it takes the `(BooleanOp, BooleanReceipt)` pair the `Heal.Merge` step forwarded — absent, that arm lowers typed rather than fabricating an empty arrangement; `public RebuildLog ToLog()` on `HealSession` folds the chain into the per-`EntityKind` affected-ref set, filtering by `HealStage.RebuildsTopology` so an `OrientNormals` op contributes nothing (winding leaves adjacency and the `TopoSignature` unchanged).
- Law: `ReceiptSeed` takes no `Context` parameter — the arena it carries holds the bound context every band derives from (`MeshEdit.Tolerance`), so a receipt cannot record a band from a context the mutation never ran under.
- Law: `Stage`, `Before`, and `After` are columns on the union ROOT, not `Switch` tables — every case carries all three, so the derivation that used to re-answer them per arm collapses into three abstract get/init declarations the case synthesis fills.
- Auto: each `RebuildReceipt` case derives its convergence witness from the six-field delta it carries — `WeldReceipt`/`DegenerateReceipt`/`SelfIntersectReceipt` assert no new non-manifold edges (a weld also never opens boundary), `GapReceipt` asserts a landed bridge minted no non-manifold edge (a mis-paired strip trebles an edge and fires the witness) while `BoundaryComponents` movement stays evidence (a cross-gap bridge merges two loops −1, a slit bridge splits its loop +1, a hole closure retires one, so a count law in either direction is the trap the global boundary heuristic already breaks), `ManifoldReceipt` asserts its `ArenaResidual` reached zero (the projected `NonManifoldEdges` re-merges the split's coincident copies and can never witness it; boundary regression is admitted — the vertex-copy split opens boundary a later gap pass may close), `OrientReceipt` asserts `IsOriented` with the Euler characteristic unchanged, and `MergeReceipt` asserts `After.GenusClosed`, the boolean's own topological success witness rather than a gate flag; `HealSession.IsValid` folds `ValidityClaim.All` over the non-empty chain of per-receipt witnesses — the one registered convergence surface.
- Receipt: this cluster is the receipt owner — the `RebuildReceipt` chain on the `HealSession` is the heal evidence the naming `Track` consumes, returned in the `Heal.Repair` rail; `ManifoldStatus` is the composed `TopologyReceipt` projection and the boolean payload the composed arrangement `BooleanReceipt`, neither re-computed here.
- Packages: `Rasm.Meshing` (`TopologyReceipt` via the Genus-tolerant `Rasm.Numerics` `ProjectionRow`; `MeshEdit` dirty-bitset seed and bound `Context`; `BooleanOp`/`BooleanReceipt` — the composed payload, `BooleanRoute` inside the receipt), `Rasm.Numerics` (`Dimension` the pass-budget column), Rasm.Domain (`IValidityEvidence`/`ValidityClaim` the registered validity fold, `Tolerance`/`ToleranceLane` the band carrier), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new heal op lands THREE coupled rows in one pass — a `HealStage` row carrying both its `Mint` and `Receipt` delegates, a `HealOp` case, and a `RebuildReceipt` case with its typed evidence and `IsValid` arm — each generated `Switch` breaking every dispatch site until its own row lands, so presence is compiler-enforced. PAIRING is now structural: `HealStage.Receipt` is the ONE mint and stamps its own row onto `RebuildReceipt.Stage`, so a receipt cannot carry a stage other than the one that minted it and the inverse table that used to re-answer the question is gone. New topological status fields are one column on `ManifoldStatus` projected from the existing `TopologyReceipt` carrier and one `ProjectionRow` widening at the mesh.md seam.
- Boundary: `RebuildReceipt` stays the typed per-kind union — a `WeldReceipt`'s merged-vertex set and a `ManifoldReceipt`'s forked-face set are different shapes carried by different cases; the before/after status is the composed `Rasm.Meshing` `TopologyReceipt` projected through the un-gated six-field row; the boolean payload is the arrangement `BooleanReceipt`; convergence registers as `IValidityEvidence`. `RebuildLog` feeds the naming `Track` re-anchor and the receipt's affected-ref set is the re-anchor seed, so a topology-rebuilding op that emits no affected entities re-anchors the naming fold blind; the fold carries VERTICES and FACES alone because the arena keys topology by face triples and the `Track` resolves edges through `VertexNames`, so an edge column would publish an empty set every op and make `ReanchorsLineage` read a slot no arm can set. `OrientReceipt.FlippedFaces` rides `Affected` as pure evidence — the `RebuildsTopology` filter excludes the orient stage, so the naming fold still sees nothing while a session consumer can audit the winding change. Op band and payload evidence ride the receipt, which mints no hash and asserts no content identity — the healed mesh's content hash is the reconciliation `Encode` job, the receipt only naming which entities changed so the reference identity (`TopoName`) re-binds.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Processing;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ManifoldStatus(
    int EulerCharacteristic, int BoundaryComponents, bool IsManifold, bool IsOriented,
    int NonManifoldEdges, Option<int> Genus) {
    public static ManifoldStatus Of((int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus) projection) =>
        new(projection.Euler, projection.BoundaryComponents, projection.IsManifold, projection.IsOriented, projection.NonManifoldEdges, projection.Genus);

    public bool GenusClosed =>
        Genus.Match(
            Some: genus => IsManifold && BoundaryComponents == 0 && NonManifoldEdges == 0
                && EulerCharacteristic == 2 - (2 * genus),
            None: static () => false);
}

internal readonly record struct ReceiptSeed(
    RepairPolicy Policy, MeshEdit Result, ManifoldStatus Before, ManifoldStatus After,
    Set<int> Faces, Set<int> Vertices, Option<Incidence> Carry,
    Option<(BooleanOp Op, BooleanReceipt Receipt)> Merge, Op Key) {
    internal Context Context => Result.Tolerance;

    internal Set<int> ExtentFaces => toSet(Range(0, Result.FaceCount));
    internal Set<int> ExtentVertices => toSet(Range(0, Result.VertexCount));

    internal static ReceiptSeed Of(RepairPolicy policy, ManifoldStatus before, ManifoldStatus after, MeshEdit result, HealStep step, Op key) =>
        new(Policy: policy, Result: result, Before: before, After: after,
            Faces: toSet(result.DirtyFaces()), Vertices: toSet(result.DirtyVertices()),
            Carry: step.Carry, Merge: step.Merge, Key: key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RebuildReceipt : IValidityEvidence {
    private RebuildReceipt() { }

    public sealed record DegenerateReceipt(HealStage Stage, Tolerance Sliver, ManifoldStatus Before, ManifoldStatus After, Set<int> CollapsedFaces) : RebuildReceipt;
    public sealed record GapReceipt(HealStage Stage, Tolerance Gap, ManifoldStatus Before, ManifoldStatus After, Set<int> BridgedFaces, Set<int> StitchedVertices) : RebuildReceipt;
    public sealed record WeldReceipt(HealStage Stage, Tolerance Weld, ManifoldStatus Before, ManifoldStatus After, Set<int> MergedVertices) : RebuildReceipt;
    public sealed record ManifoldReceipt(HealStage Stage, Dimension PassBudget, int ArenaResidual, ManifoldStatus Before, ManifoldStatus After, Set<int> ForkedFaces, Set<int> ForkedVertices) : RebuildReceipt;
    public sealed record SelfIntersectReceipt(HealStage Stage, ManifoldStatus Before, ManifoldStatus After, Set<int> RetiledFaces, Set<int> MintedVertices) : RebuildReceipt;
    public sealed record OrientReceipt(HealStage Stage, ManifoldStatus Before, ManifoldStatus After, Set<int> FlippedFaces) : RebuildReceipt;
    public sealed record MergeReceipt(HealStage Stage, BooleanOp Op, BooleanReceipt Merge, ManifoldStatus Before, ManifoldStatus After, Set<int> SelectedFaces, Set<int> SelectedVertices) : RebuildReceipt;

    public abstract HealStage Stage { get; init; }
    public abstract ManifoldStatus Before { get; init; }
    public abstract ManifoldStatus After { get; init; }

    public bool IsValid =>
        Switch(
            degenerateReceipt:    static d => d.After.NonManifoldEdges <= d.Before.NonManifoldEdges,
            gapReceipt:           static g => g.BridgedFaces.IsEmpty
                ? g.After.BoundaryComponents <= g.Before.BoundaryComponents
                : g.After.NonManifoldEdges <= g.Before.NonManifoldEdges,
            weldReceipt:          static w => w.After.NonManifoldEdges <= w.Before.NonManifoldEdges
                && w.After.BoundaryComponents <= w.Before.BoundaryComponents,
            manifoldReceipt:      static m => m.ArenaResidual == 0,
            selfIntersectReceipt: static s => s.After.NonManifoldEdges <= s.Before.NonManifoldEdges,
            orientReceipt:        static o => o.After.IsOriented && o.After.EulerCharacteristic == o.Before.EulerCharacteristic,
            mergeReceipt:         static m => m.After.GenusClosed);

    public (Set<int> Vertices, Set<int> Faces) Affected =>
        Switch(
            degenerateReceipt:    static d => (Set<int>.Empty, d.CollapsedFaces),
            gapReceipt:           static g => (g.StitchedVertices, g.BridgedFaces),
            weldReceipt:          static w => (w.MergedVertices, Set<int>.Empty),
            manifoldReceipt:      static m => (m.ForkedVertices, m.ForkedFaces),
            selfIntersectReceipt: static s => (s.MintedVertices, s.RetiledFaces),
            orientReceipt:        static o => (Set<int>.Empty, o.FlippedFaces),
            mergeReceipt:         static m => (m.SelectedVertices, m.SelectedFaces));
}

public sealed record RebuildLog(Set<int> Vertices, Set<int> Faces, Seq<HealStage> Ops) {
    public static readonly RebuildLog Empty = new(Set<int>.Empty, Set<int>.Empty, Seq<HealStage>());

    public bool ReanchorsLineage => !Vertices.IsEmpty || !Faces.IsEmpty;
}

public sealed record HealSession(MeshSpace Input, MeshSpace Healed, Seq<RebuildReceipt> Receipts) : IValidityEvidence {
    public Option<ManifoldStatus> FinalStatus => Receipts.Last.Map(static receipt => receipt.After);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Receipts.Count, floor: 1),
        Receipts.ForAll(static receipt => receipt.IsValid));

    public RebuildLog ToLog() =>
        Receipts.Filter(static receipt => receipt.Stage.RebuildsTopology)
            .Fold(RebuildLog.Empty, static (log, receipt) => {
                (Set<int> v, Set<int> f) = receipt.Affected;
                return log with {
                    Vertices = log.Vertices.TryAddRange(v),
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

`RebuildReceipt`, the `ManifoldStatus` projection, and the `HealSession`/`RebuildLog` fold are transcription-complete pure-managed fences composing the `TopologyReceipt` projection seam, the arrangement `BooleanReceipt` payload, the arena dirty bitsets and bound context, and the `Incidence` the split already folded and carried forward — none depending on a live-host member spelling beyond the stable native `Mesh` surface the topology sibling pins.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
