# 1. Delete the unused re-anchor model

**From**

`libs/dotnet/Rasm/.planning/Processing/session.md:88`

```csharp
public (Set<int> Vertices, Set<int> Faces) Affected =>
    Switch(
        degenerate:    static d => (Set<int>(), d.CollapsedFaces),
        gap:           static g => (g.StitchedVertices, g.BridgedFaces),
        weld:          static w => (w.MergedVertices, Set<int>()),
        manifold:      static m => (m.ForkedVertices, m.ForkedFaces),
        selfIntersect: static s => (s.MintedVertices, s.RetiledFaces),
        orient:        static o => (Set<int>(), o.FlippedFaces),
        merge:         static m => (m.SelectedVertices, m.SelectedFaces));
```

`libs/dotnet/Rasm/.planning/Processing/session.md:99`

```csharp
public sealed record RebuildLog(Set<int> Vertices, Set<int> Faces, Seq<HealStage> Ops) {
    public static readonly RebuildLog Empty = new(Set<int>(), Set<int>(), Seq<HealStage>());

    public bool ReanchorsLineage => !Vertices.IsEmpty || !Faces.IsEmpty;
}
```

`libs/dotnet/Rasm/.planning/Processing/session.md:112`

```csharp
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
```

**To**

```csharp
// HealStep.Affected DELETED
```

```csharp
// RebuildLog DELETED
```

```csharp
// HealSession.ToLog DELETED
```

**Why**

No project fence consumes `Affected`, `RebuildLog`, or `ToLog`. `Spatial/naming.md` re-anchors from the prior `NameTable` and rebuilt `CanonicalTopology`; it has no heal-index input. This model therefore publishes a false boundary and duplicates the canonical-topology owner without providing naming capability.

**Change**

Delete the projection, aggregate, and fold. Keep changed vertex and face sets directly on the heal evidence rebuilt in task 2. Remove `HealStage.RebuildsTopology`, whose only consumer is the deleted fold.

**Delta**

Target code-fence LOC: -25. Repair ripple code-fence LOC: -1. Module surface: -1 type and -8 members (`Affected`, `Vertices`, `Faces`, `Ops`, `Empty`, `ReanchorsLineage`, `ToLog`, `RebuildsTopology`); +0 types and +0 members; net -1 type, -8 members, and -26 code-fence LOC.

**Ripples**

Remove `RebuildsTopology` from every `HealStage` item and delete its property in `libs/dotnet/Rasm/.planning/Processing/repair.md`. Remove the obsolete bool-pair ruling from `libs/dotnet/Rasm/RULINGS.md`. Remove the naming-re-anchor claims from `session.md`, `repair.md`, `libs/dotnet/Rasm/README.md`, and `libs/dotnet/Rasm/ARCHITECTURE.md`; `libs/dotnet/Rasm/.planning/Spatial/naming.md` requires no code change.

# 2. Collapse duplicate topology and per-operation step types

**From**

`libs/dotnet/Rasm/.planning/Processing/session.md:24`

```csharp
using LanguageExt;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;
```

`libs/dotnet/Rasm/.planning/Processing/session.md:34`

```csharp
public readonly record struct ManifoldStatus(
    int EulerCharacteristic, int BoundaryComponents, bool IsManifold, bool IsOriented,
    int NonManifoldEdges, Option<int> Genus) {
    public static ManifoldStatus Of((int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus) projection) =>
        new(projection.Euler, projection.BoundaryComponents, projection.IsManifold, projection.IsOriented, projection.NonManifoldEdges, projection.Genus);

    public bool GenusClosed => Genus.Exists(genus => IsManifold && BoundaryComponents == 0 && NonManifoldEdges == 0
        && EulerCharacteristic == 2 - (2 * genus));
}
```

`libs/dotnet/Rasm/.planning/Processing/session.md:44`

```csharp
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
```

`libs/dotnet/Rasm/.planning/Processing/session.md:59`

```csharp
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
}
```

`libs/dotnet/Rasm/.planning/Processing/session.md:105`

```csharp
public sealed record HealSession(MeshSpace Input, MeshSpace Healed, Seq<HealStep> Steps) : IValidityEvidence {
    public Option<ManifoldStatus> FinalStatus => Steps.Last.Map(static step => step.After);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Steps.Count, floor: 1),
        Steps.ForAll(static step => step.IsValid));
```

**To**

```csharp
using LanguageExt;
using Rasm.Domain;
using Rasm.Meshing;
```

```csharp
// ManifoldStatus DELETED
```

```csharp
// StepSeed DELETED
```

```csharp
public sealed record HealStep(
    HealStage Stage, Topology Before, Topology After, Set<int> Vertices, Set<int> Faces,
    Option<int> Residual, Option<(BooleanOp Op, BooleanCensus Census)> Boolean) : IValidityEvidence {
    public bool IsValid => Stage.Map(
        state: this,
        weld:         static step => step.After.NonManifoldEdges <= step.Before.NonManifoldEdges
            && step.After.BoundaryComponents <= step.Before.BoundaryComponents,
        degenerate:   static step => step.After.NonManifoldEdges <= step.Before.NonManifoldEdges,
        gap:          static step => step.Faces.IsEmpty
            ? step.After.BoundaryComponents <= step.Before.BoundaryComponents
            : step.After.NonManifoldEdges <= step.Before.NonManifoldEdges,
        manifold:     static step => step.Residual.Exists(static count => count == 0),
        orient:       static step => step.After.Traits.Admits(MeshTrait.Oriented)
            && step.After.EulerCharacteristic == step.Before.EulerCharacteristic,
        selfIntersect: static step => step.After.NonManifoldEdges <= step.Before.NonManifoldEdges,
        boolean:      static step => step.Boolean.IsSome
            && step.After.BoundaryComponents == 0
            && step.After.NonManifoldEdges == 0
            && step.After.Genus is { IsSome: true, Case: int genus }
            && step.After.EulerCharacteristic == 2 - (2 * genus));
}
```

```csharp
public sealed record HealSession(
    MeshSpace Input, MeshSpace Healed, RepairPolicy Policy, Seq<HealStep> Steps) : IValidityEvidence {
    public Option<Topology> FinalStatus => Steps.Last.Map(static step => step.After);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Steps.Count, floor: 1),
        Steps.ForAll(static step => step.IsValid));
}
```

**Why**

`ManifoldStatus` duplicates fields already owned by `Rasm.Meshing.Topology`, and its requested projection tuple does not exist: the exact projection carries `CapabilitySet<MeshTrait>`, while `Topology` itself is already a supported self-projection. `HealStep` then repeats the same stage, status pair, and changed-index columns across seven nested types solely to run one validity projection. `StepSeed` and `HealStage.Step` add a second dispatch after `HealOp` has already selected the operation. A new `TopologyChange` wrapper would preserve that indirection instead of removing it.

**Change**

Use `Topology` directly. Replace the seven-case result union with one `HealStep` carrying the common change evidence, the manifold residual, and the optional boolean evidence. Store `RepairPolicy` once on `HealSession` so tolerance lanes and the pass budget remain reconstructible without repeating them per step. Derive validity with the generated exhaustive `HealStage.Map`, the result-producing generated operation; do not use `Switch` as a value projection. In `Heal.Repair`, fold the already-selected `HealOp` directly into the stage, changed sets, residual, and boolean payload, then construct `HealStep` inline.

**Delta**

Target code-fence LOC: -42. Target surface: 9 types and 70 members removed; 0 types and 14 members added; net -9 types and -56 members. Repair ripple surface: 5 members removed (`Collects`, `Mint`, `Step`, `HealOp.Stage`, `Heal.Minted`) and none added. Whole-move net surface: -9 types and -61 members.

**Ripples**

In `libs/dotnet/Rasm/.planning/Processing/repair.md`, reduce `HealStage` to seven keyed items; preserve the genuine second weld-and-degenerate cleanup by spelling the eight-entry `Heal.Standard` sequence directly; delete `Collects`, `Mint`, `Step`, `HealOp.Stage`, and `Heal.Minted`; rename `HealEdit.Merge` to `Boolean`; make `Heal.Status` return `Fin<Topology>` through the existing self-projection; and have the repair fold use generated `HealOp.Map` once to produce stage-specific sets, residual, and boolean evidence before the single `HealStep` construction. In `libs/dotnet/Rasm.Fabrication/.planning/Ingress/solid.md:696`, retain the real final-status consumer and read `Topology.Watertight`, `Topology.Traits.Admits(MeshTrait.Oriented)`, and `BoundaryComponents`. Replace `ManifoldStatus`, per-case `HealStep`, stage-delegate, and `Merge` claims in `session.md`, `repair.md`, `libs/dotnet/Rasm/README.md`, `libs/dotnet/Rasm/ARCHITECTURE.md`, and `libs/dotnet/Rasm/.api/api-manifold.md`.
