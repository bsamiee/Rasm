# [RASM_HEALING_SESSION]

This page mints the heal session — `ManifoldStatus` snapshots, the `HealStep` per-op union, the `HealSession` chain, and the `RebuildLog` re-anchor fold — that `Heal.Repair` returns and the naming `Track` re-anchor consumes. Every value is an interior type crossing only the in-process boundary to the naming fold, never sitting between wire and result.

`ManifoldStatus` is the composed `Rasm.Meshing` `Topology` projected through the Genus-tolerant six-field row, un-gated so the non-manifold, boundaried, or odd-Euler meshes the heal exists for snapshot instead of failing. Each `HealStep` case registers its convergence witness as `IValidityEvidence.IsValid`, `HealSession.IsValid` folds `ValidityClaim.All` over the chain, and the boolean case carries the arrangement `BooleanCensus` as payload. Every band a case records is the derived `Tolerance` carrier — the arena's own bound `Context` resolved through the lane `RepairPolicy` names — never a bare double no lane can trace.

## [01]-[INDEX]

- [02]-[HEAL_SESSION]: `ManifoldStatus` Genus-tolerant projection + `GenusClosed` witness; `HealStep` `[Union]` one settled step per op registering `IValidityEvidence`; `HealSession` chain folding `ValidityClaim.All`; `RebuildLog` re-anchor seed.

## [02]-[HEAL_SESSION]

- Owner: `ManifoldStatus` the before/after topological snapshot — six scalars the `VectorIntent.Topology` projection entry yields from the composed `Rasm.Meshing` `Topology` via the Genus-tolerant `ProjectionRow`, never re-counted: `NonManifoldEdges` is the projected defect count every delta witness reads and the gap bridge's coherence witness (`BoundaryComponents` moves ±1 per bridge, so the count is evidence, never law) while the manifold split's own convergence rides `HealStep.Manifold.ArenaResidual`, the arena fold being the single authority a position-keyed topology-vertex re-merge cannot erase, and `Genus` stays `Option<int>` because a non-manifold or non-oriented snapshot has no validated genus — exactly the input class the heal admits; `GenusClosed` derives the closed-target witness on a manifold, computed never stored; `HealStep` `[Union]` mints one typed case per `HealOp` carrying the op's band as a `Tolerance`, before/after `ManifoldStatus`, and the affected index sets seeded from the arena dirty bitsets, each case registering its convergence witness as `IsValid`; `HealSession` carries the input mesh, healed mesh, and ordered step chain, its `IsValid` the `ValidityClaim.All` fold the corpus validity oracle reads; `RebuildLog` flattens a session into the `(EntityKind, affected-index-set)` re-anchor input the naming `Track` reads.
- Cases: `HealStep` cases `Degenerate` · `Gap` · `Weld` · `Manifold` · `SelfIntersect` · `Orient` · `Merge`, one per `HealOp`; `Merge` carries the composed arrangement `BooleanCensus`. `ManifoldStatus` is one record carrying the six projected scalars and the derived `GenusClosed` witness.
- Entry: `StepSeed.Of(RepairPolicy, ManifoldStatus before, ManifoldStatus after, MeshEdit, HealEdit, Op)` packs one mint payload and `stage.Step(seed)` mints the typed step on the `Fin` result — before/after status arrives already projected through the Genus-tolerant entry (the session binds the projection before minting), the policy travels beside the stateless op so each case records its band (`seed.Context.For(policy.Arena.Weld)` on the weld, `.For(policy.Sliver)` on the collapse, `.For(policy.Gap)` on the bridge, `policy.MaxManifoldPasses` on the split), the affected seeds read the arena dirty bitsets (`result.DirtyFaces()`/`DirtyVertices()` — admission clears both, so a set bit names a kernel edit; marks still accumulate across the ops sharing one arena, so a seed over-approximates forward but never misses an entity, and the session-level union `ToLog` folds is identical either way), the boolean arm reads the whole arena extent instead because its arena is wholly new material no bit distinguishes, and it takes the `(BooleanOp, BooleanCensus)` pair the `Heal.Merge` edit forwarded — absent, that arm lowers typed rather than fabricating an empty arrangement; `public RebuildLog ToLog()` on `HealSession` folds the chain into the per-`EntityKind` affected-ref set, filtering by `HealStage.RebuildsTopology` so an `OrientNormals` op contributes nothing (winding leaves adjacency and the `TopoSignature` unchanged).
- Law: `StepSeed` takes no `Context` parameter — the arena it carries holds the bound context every band derives from (`MeshEdit.Tolerance`), so a step cannot record a band from a context the mutation never ran under.
- Law: `Stage`, `Before`, and `After` are columns on the union ROOT, not `Switch` tables — every case carries all three, so the derivation that used to re-answer them per arm collapses into three abstract get/init declarations the case synthesis fills.
- Auto: each `HealStep` case derives its convergence witness from the six-field delta it carries — `Weld`/`Degenerate`/`SelfIntersect` assert no new non-manifold edges (a weld also never opens boundary), `Gap` asserts a landed bridge minted no non-manifold edge (a mis-paired strip trebles an edge and fires the witness) while `BoundaryComponents` movement stays evidence (a cross-gap bridge merges two loops −1, a slit bridge splits its loop +1, a hole closure retires one, so a count law in either direction is the trap the global boundary heuristic already breaks), `Manifold` asserts its `ArenaResidual` reached zero (the projected `NonManifoldEdges` re-merges the split's coincident copies and can never witness it; boundary regression is admitted — the vertex-copy split opens boundary a later gap pass may close), `Orient` asserts `IsOriented` with the Euler characteristic unchanged, and `Merge` asserts `After.GenusClosed`, the boolean's own topological success witness rather than a gate flag; `HealSession.IsValid` folds `ValidityClaim.All` over the non-empty chain of per-step witnesses — the one registered convergence surface.
- Output: the `HealStep` chain on the `HealSession` is the heal evidence the naming `Track` consumes, returned in the `Heal.Repair` result; `ManifoldStatus` is the composed `Topology` projection and the boolean payload the composed arrangement `BooleanCensus`, neither re-computed here.
- Packages: `Rasm.Meshing` (`Topology` via the Genus-tolerant `Rasm.Numerics` `ProjectionRow`; `MeshEdit` dirty-bitset seed and bound `Context`; `BooleanOp`/`BooleanCensus` — the composed payload), `Rasm.Numerics` (`Dimension` the pass-budget column), Rasm.Domain (`IValidityEvidence`/`ValidityClaim` the registered validity fold, `Tolerance`/`ToleranceLane` the band carrier), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new heal op lands THREE coupled rows in one pass — a `HealStage` row carrying both its `Mint` and `Step` delegates, a `HealOp` case, and a `HealStep` case with its typed evidence and `IsValid` arm — each generated `Switch` breaking every dispatch site until its own row lands, so presence is compiler-enforced. PAIRING is structural: `HealStage.Step` is the ONE mint and stamps its own row onto `HealStep.Stage`, so a step cannot carry a stage other than the one that minted it and no inverse table re-answers the question. New topological status fields are one column on `ManifoldStatus` projected from the existing `Topology` carrier and one `ProjectionRow` widening at the mesh.md boundary.
- Boundary: `HealStep` stays the typed per-kind union — a `Weld`'s merged-vertex set and a `Manifold`'s forked-face set are different shapes carried by different cases; the before/after status is the composed `Rasm.Meshing` `Topology` projected through the un-gated six-field row; the boolean payload is the arrangement `BooleanCensus`; convergence registers as `IValidityEvidence`. `RebuildLog` feeds the naming `Track` re-anchor and the step's affected-ref set is the re-anchor seed, so a topology-rebuilding op that emits no affected entities re-anchors the naming fold blind; the fold carries VERTICES and FACES alone because the arena keys topology by face triples and the `Track` resolves edges through `VertexNames`, so an edge column publishes an empty set every op and makes `ReanchorsLineage` read a slot no arm can set. `Orient.FlippedFaces` rides `Affected` as pure evidence — the `RebuildsTopology` filter excludes the orient stage, so the naming fold still sees nothing while a session consumer can audit the winding change. Op band and payload evidence ride the step, which mints no hash and asserts no content identity — the healed mesh's content hash is the reconciliation `Encode` job, the step only naming which entities changed so the reference identity (`TopoName`) re-binds.

```csharp
using LanguageExt;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Processing;

public readonly record struct ManifoldStatus(
    int EulerCharacteristic, int BoundaryComponents, bool IsManifold, bool IsOriented,
    int NonManifoldEdges, Option<int> Genus) {
    public static ManifoldStatus Of((int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus) projection) =>
        new(projection.Euler, projection.BoundaryComponents, projection.IsManifold, projection.IsOriented, projection.NonManifoldEdges, projection.Genus);

    public bool GenusClosed => Genus.Exists(genus => IsManifold && BoundaryComponents == 0 && NonManifoldEdges == 0
        && EulerCharacteristic == 2 - (2 * genus));
}

internal readonly record struct StepSeed(
    RepairPolicy Policy, MeshEdit Result, ManifoldStatus Before, ManifoldStatus After,
    Set<int> Faces, Set<int> Vertices, Option<Incidence> Carry,
    Option<(BooleanOp Op, BooleanCensus Census)> Merge, Op Key) {
    internal Context Context => Result.Tolerance;

    internal Set<int> ExtentFaces => toSet(Range(0, Result.FaceCount));
    internal Set<int> ExtentVertices => toSet(Range(0, Result.VertexCount));

    internal static StepSeed Of(RepairPolicy policy, ManifoldStatus before, ManifoldStatus after, MeshEdit result, HealEdit edit, Op key) =>
        new(Policy: policy, Result: result, Before: before, After: after,
            Faces: toSet(result.DirtyFaces()), Vertices: toSet(result.DirtyVertices()),
            Carry: edit.Carry, Merge: edit.Merge, Key: key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HealStep : IValidityEvidence {
    private HealStep() { }

    public sealed record Degenerate(HealStage Stage, Tolerance Sliver, ManifoldStatus Before, ManifoldStatus After, Set<int> CollapsedFaces) : HealStep;
    public sealed record Gap(HealStage Stage, Tolerance Band, ManifoldStatus Before, ManifoldStatus After, Set<int> BridgedFaces, Set<int> StitchedVertices) : HealStep;
    public sealed record Weld(HealStage Stage, Tolerance Band, ManifoldStatus Before, ManifoldStatus After, Set<int> MergedVertices) : HealStep;
    public sealed record Manifold(HealStage Stage, Dimension PassBudget, int ArenaResidual, ManifoldStatus Before, ManifoldStatus After, Set<int> ForkedFaces, Set<int> ForkedVertices) : HealStep;
    public sealed record SelfIntersect(HealStage Stage, ManifoldStatus Before, ManifoldStatus After, Set<int> RetiledFaces, Set<int> MintedVertices) : HealStep;
    public sealed record Orient(HealStage Stage, ManifoldStatus Before, ManifoldStatus After, Set<int> FlippedFaces) : HealStep;
    public sealed record Merge(HealStage Stage, BooleanOp Op, BooleanCensus Census, ManifoldStatus Before, ManifoldStatus After, Set<int> SelectedFaces, Set<int> SelectedVertices) : HealStep;

    public abstract HealStage Stage { get; init; }
    public abstract ManifoldStatus Before { get; init; }
    public abstract ManifoldStatus After { get; init; }

    public bool IsValid =>
        Switch(
            degenerate:    static d => d.After.NonManifoldEdges <= d.Before.NonManifoldEdges,
            gap:           static g => g.BridgedFaces.IsEmpty
                ? g.After.BoundaryComponents <= g.Before.BoundaryComponents
                : g.After.NonManifoldEdges <= g.Before.NonManifoldEdges,
            weld:          static w => w.After.NonManifoldEdges <= w.Before.NonManifoldEdges
                && w.After.BoundaryComponents <= w.Before.BoundaryComponents,
            manifold:      static m => m.ArenaResidual == 0,
            selfIntersect: static s => s.After.NonManifoldEdges <= s.Before.NonManifoldEdges,
            orient:        static o => o.After.IsOriented && o.After.EulerCharacteristic == o.Before.EulerCharacteristic,
            merge:         static m => m.After.GenusClosed);

    public (Set<int> Vertices, Set<int> Faces) Affected =>
        Switch(
            degenerate:    static d => (Set<int>.Empty, d.CollapsedFaces),
            gap:           static g => (g.StitchedVertices, g.BridgedFaces),
            weld:          static w => (w.MergedVertices, Set<int>.Empty),
            manifold:      static m => (m.ForkedVertices, m.ForkedFaces),
            selfIntersect: static s => (s.MintedVertices, s.RetiledFaces),
            orient:        static o => (Set<int>.Empty, o.FlippedFaces),
            merge:         static m => (m.SelectedVertices, m.SelectedFaces));
}

public sealed record RebuildLog(Set<int> Vertices, Set<int> Faces, Seq<HealStage> Ops) {
    public static readonly RebuildLog Empty = new(Set<int>.Empty, Set<int>.Empty, Seq<HealStage>());

    public bool ReanchorsLineage => !Vertices.IsEmpty || !Faces.IsEmpty;
}

public sealed record HealSession(MeshSpace Input, MeshSpace Healed, Seq<HealStep> Steps) : IValidityEvidence {
    public Option<ManifoldStatus> FinalStatus => Steps.Last.Map(static step => step.After);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Steps.Count, floor: 1),
        Steps.ForAll(static step => step.IsValid));

    public RebuildLog ToLog() =>
        Steps.Filter(static step => step.Stage.RebuildsTopology)
            .Fold(RebuildLog.Empty, static (log, step) => {
                (Set<int> v, Set<int> f) = step.Affected;
                return log with {
                    Vertices = log.Vertices.TryAddRange(v),
                    Faces = log.Faces.TryAddRange(f),
                    Ops = log.Ops.Add(step.Stage),
                };
            });
}
```

## [03]-[DENSITY_BAR]

Each `[RESULT]` cell names the one return type its owner exposes; the steps are pure carriers returned in the `Heal.Repair` result.

| [INDEX] | [AXIS_CONCERN]           | [OWNER]                    | [RESULT]                                    | [CASES] |
| :-----: | :----------------------- | :------------------------- | :------------------------------------------ | :-----: |
|  [01]   | Heal step                | `HealStep`                 | carrier (returned in `Heal.Repair` result)  |    7    |
|  [02]   | Topological status       | `ManifoldStatus`           | `ManifoldStatus.Of → ManifoldStatus` (pure) |    —    |
|  [03]   | Heal session + re-anchor | `HealSession`/`RebuildLog` | `HealSession.ToLog → RebuildLog` (pure)     |    —    |

`HealStep`, the `ManifoldStatus` projection, and the `HealSession`/`RebuildLog` fold are transcription-complete pure-managed fences composing the `Topology` projection entry, the arrangement `BooleanCensus` payload, the arena dirty bitsets and bound context, and the `Incidence` the split already folded and carried forward — none depending on a live-host member spelling beyond the stable native `Mesh` surface the topology sibling pins.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
