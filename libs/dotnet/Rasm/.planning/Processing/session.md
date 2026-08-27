# [RASM_HEALING_SESSION]

This page mints the heal session — the `HealStep` evidence record over the `Rasm.Meshing` `Topology` witness and the `HealSession` chain — that `Heal.Repair` returns. Every value is an interior type crossing only the in-process boundary, never sitting between wire and result.

`Topology` is the composed `Rasm.Meshing` witness read whole, un-gated so the non-manifold, boundaried, or odd-Euler meshes the heal exists for snapshot instead of failing. One `HealStep` record carries the stage, the before/after `Topology` pair, the changed vertex and face sets, the manifold residual, and the boolean arrangement evidence; `HealStep.IsValid` registers the stage's convergence witness as `IValidityEvidence` through one generated `HealStage.Switch`, and `HealSession.IsValid` folds `ValidityClaim.All` over the chain. Every band a step's kernel read is a `ToleranceLane` the session's `RepairPolicy` names off the arena's own bound `Context` — the policy rides the session once, never a per-step scalar.

## [01]-[INDEX]

- [02]-[HEAL_SESSION]: `HealStep` one evidence record per applied op registering `IValidityEvidence` through `HealStage.Switch`; `HealSession` chain carrying the policy and folding `ValidityClaim.All`.

## [02]-[HEAL_SESSION]

- Owner: `HealStep` the one settled step — the `HealStage` that ran, the before/after `Rasm.Meshing` `Topology` pair read whole and never re-counted (`NonManifoldEdges` is the projected defect count every delta witness reads, `BoundaryComponents` the gap bridge's coherence evidence — it moves ±1 per bridge, so the count is evidence, never law — and `Genus` stays `Option<int>` because a non-manifold or non-oriented snapshot has no validated genus, exactly the input class the heal admits), the changed `Vertices` and `Faces` seeded from the arena dirty bitsets, the `Option<int>` manifold `Residual` the split's carried `Incidence` fold answers (the single authority a position-keyed topology-vertex re-merge cannot erase), and the `Option<(BooleanOp, BooleanCensus)>` `Boolean` arrangement evidence; `HealSession` carries the input mesh, healed mesh, the `RepairPolicy` the fold ran under, and the ordered step chain, its `IsValid` the `ValidityClaim.All` fold the corpus validity oracle reads.
- Cases: `HealStage` rows `Weld` · `Degenerate` · `Gap` · `Manifold` · `Orient` · `SelfIntersect` · `Boolean`, one per `HealOp`, each an arm of `HealStep.IsValid`; the record carries no per-stage case — a stage's absent axis reads `None` or the empty set.
- Entry: `Heal.Repair` constructs each `HealStep` inline after the op's `RepairEdit` publishes and the after-`Topology` lands — before/after arrive as the whole `Topology` witness (`before[n] = after[n-1]`), the changed sets read the arena dirty bitsets (`DirtyFaces()`/`DirtyVertices()` — admission clears both, so a set bit names a kernel edit; marks still accumulate across the ops sharing one arena, so a seed over-approximates forward but never misses an entity), the manifold residual reads the `Incidence` the split carried, and the boolean arm reads the whole arena extent instead because its arena is wholly new material no bit distinguishes and takes the `(BooleanOp, BooleanCensus)` pair the `Heal.Boolean` edit forwarded; a missing carry or census leaves the axis `None` and the stage's witness reads it as unconverged.
- Law: `HealStep` carries no `Context` or band column — the arena the fold ran holds the bound context every band derives from (`MeshEdit.Tolerance`) and `HealSession.Policy` names the lanes, so a step cannot record a band from a context the mutation never ran under and no tolerance repeats per step.
- Law: `Stage`, `Before`, and `After` are columns on the one record, so no per-arm derivation re-answers them; `Stage` is the keyless `HealStage` row the applied `HealOp` case selected at construction, so a stage other than the op's own is unrepresentable.
- Auto: `HealStep.IsValid` derives the stage's convergence witness from the `Topology` delta through one generated `HealStage.Switch` over the record — `Weld`/`Degenerate`/`SelfIntersect` assert no new non-manifold edges (a weld also never opens boundary), `Gap` asserts a landed bridge minted no non-manifold edge (a mis-paired strip trebles an edge and fires the witness) while `BoundaryComponents` movement stays evidence (a cross-gap bridge merges two loops −1, a slit bridge splits its loop +1, a hole closure retires one, so a count law in either direction is the trap the global boundary heuristic already breaks), `Manifold` asserts its `Residual` reached zero (the projected `NonManifoldEdges` re-merges the split's coincident copies and can never witness it; boundary regression is admitted — the vertex-copy split opens boundary a later gap pass may close), `Orient` asserts `MeshTrait.Oriented` with the Euler characteristic unchanged, and `Boolean` asserts the census landed on a closed shell whose Euler characteristic matches its validated genus — the boolean's own topological success witness rather than a gate flag; `HealSession.IsValid` folds `ValidityClaim.All` over the non-empty chain of per-step witnesses — the one registered convergence surface.
- Output: the `HealStep` chain on the `HealSession` is the heal evidence returned in the `Heal.Repair` result, `FinalStatus` the last step's after-`Topology`; the topology witness and the boolean `BooleanCensus` are composed, neither re-computed here.
- Packages: `Rasm.Meshing` (`Topology` the un-gated witness; `MeshEdit` dirty-bitset seed and bound `Context`; `BooleanOp`/`BooleanCensus` — the composed payload; `MeshTrait` the trait vocabulary), Rasm.Domain (`IValidityEvidence`/`ValidityClaim` the registered validity fold), LanguageExt.Core, BCL inbox.
- Growth: a new heal op lands THREE coupled rows in one pass — a keyless `HealStage` row, a `HealOp` case selecting it, and its arm in the `Heal.Repair` step fold and in `HealStep.IsValid` — each generated `Switch` breaking every dispatch site until its own arm lands, so presence is compiler-enforced. A new topological status fact is one column on `Topology` at the mesh.md owner, read here with no projection to widen.
- Boundary: `HealStep` is ONE record — a weld's merged vertices and a split's forked faces are the same `Vertices`/`Faces` axes, the per-stage meaning riding `Stage`; the before/after status is the composed `Rasm.Meshing` `Topology` witness whole; the boolean payload is the arrangement `BooleanCensus`; convergence registers as `IValidityEvidence`. The changed sets carry VERTICES and FACES alone because the arena keys topology by face triples, so an edge column would publish an empty set every op; `Orient` fills `Faces` with the flipped faces as pure evidence a session consumer audits for the winding change. Payload evidence rides the step, which mints no hash and asserts no content identity — the healed mesh's content hash is the reconciliation `Encode` job, the step only naming which entities changed.

```csharp
using LanguageExt;
using Rasm.Domain;
using Rasm.Meshing;

namespace Rasm.Processing;

public sealed record HealStep(
    HealStage Stage, Topology Before, Topology After, Set<int> Vertices, Set<int> Faces,
    Option<int> Residual, Option<(BooleanOp Op, BooleanCensus Census)> Boolean) : IValidityEvidence {
    public bool IsValid => Stage.Switch(
        state: this,
        weld:          static step => step.After.NonManifoldEdges <= step.Before.NonManifoldEdges
            && step.After.BoundaryComponents <= step.Before.BoundaryComponents,
        degenerate:    static step => step.After.NonManifoldEdges <= step.Before.NonManifoldEdges,
        gap:           static step => step.Faces.IsEmpty
            ? step.After.BoundaryComponents <= step.Before.BoundaryComponents
            : step.After.NonManifoldEdges <= step.Before.NonManifoldEdges,
        manifold:      static step => step.Residual.Exists(static count => count == 0),
        orient:        static step => step.After.Traits.Admits(MeshTrait.Oriented)
            && step.After.EulerCharacteristic == step.Before.EulerCharacteristic,
        selfIntersect: static step => step.After.NonManifoldEdges <= step.Before.NonManifoldEdges,
        boolean:       static step => step.Boolean.IsSome
            && step.After.BoundaryComponents == 0
            && step.After.NonManifoldEdges == 0
            && step.After.Genus is { IsSome: true, Case: int genus }
            && step.After.EulerCharacteristic == 2 - (2 * genus));
}

public sealed record HealSession(
    MeshSpace Input, MeshSpace Healed, RepairPolicy Policy, Seq<HealStep> Steps) : IValidityEvidence {
    public Option<Topology> FinalStatus => Steps.Last.Map(static step => step.After);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Steps.Count, floor: 1),
        Steps.ForAll(static step => step.IsValid));
}
```

## [03]-[DENSITY_BAR]

Each `[RESULT]` cell names the one return type its owner exposes; the steps are pure carriers returned in the `Heal.Repair` result.

| [INDEX] | [AXIS_CONCERN] | [OWNER]       | [RESULT]                                   | [CASES] |
| :-----: | :------------- | :------------ | :----------------------------------------- | :-----: |
|  [01]   | Heal step      | `HealStep`    | carrier (returned in `Heal.Repair` result) |    —    |
|  [02]   | Heal session   | `HealSession` | carrier (returned in `Heal.Repair` result) |    —    |

`HealStep` and the `HealSession` chain are transcription-complete pure-managed fences composing the `Topology` witness, the arrangement `BooleanCensus` payload, the arena dirty bitsets and bound context, and the `Incidence` the split already folded and carried forward — none depending on a live-host member spelling beyond the stable native `Mesh` surface the topology sibling pins.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
