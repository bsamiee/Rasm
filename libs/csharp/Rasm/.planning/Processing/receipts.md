# [RASM_HEALING_RECEIPTS]

The typed per-op rebuild evidence the heal rail emits and the naming `Track` re-anchor consumes. The page owns `ManifoldStatus` — the before/after topological snapshot projected from the composed `Vectors` `TopologyReceipt` through the Genus-tolerant six-field row `(Euler, BoundaryComponents, IsManifold, IsOriented, NonManifoldEdges, Option<int> Genus)`, un-gated, so the defective meshes the heal exists for (non-manifold, boundaried, odd-Euler) snapshot instead of failing — the `RebuildReceipt` `[Union]` (one typed case per `HealOp`, each carrying its policy tolerance, before/after status, and the affected entity refs seeded from the arena's dirty bitsets), the `HealSession` carrier the naming `Track` reads, and the `RebuildLog` fold that flattens a session into the per-`EntityKind` re-anchor seed. Convergence is not a standalone fold: every receipt case REGISTERS its per-op success witness as `IValidityEvidence.IsValid` — `NonManifoldEdges` for the manifold split, `BoundaryComponents` for the gap close, `IsOriented` for the orientation walk, the genus-consistent closed target `Euler = 2 − 2·Genus ∧ BoundaryComponents = 0` for the boolean — and `HealSession.IsValid` is the `ValidityClaim.All` fold over the chain.

The boolean case carries the `Meshing/arrangement#ARRANGEMENT` `BooleanReceipt` as payload beside `ManifoldStatus` — ONE receipt type corpus-wide, never a renamed sibling — and its convergence witness is the boolean's OWN topological success, never a gate flag: a scale-gated boolean fails the arrangement rail with `NativeAssetMissing` 2423 and never reaches a mint, so every minted `MergeReceipt` is un-gated by construction. The receipt records op tolerances and affected refs but mints NO hash and asserts NO content identity — the healed mesh's content hash is the `Spatial/reconciliation#NAMING_HASH` `Encode` job; the receipt only names which entities changed so the reference identity (`TopoName`) re-binds. The `RebuildReceipt` chain crosses only the in-process seam to the naming `Track` fold; the records are interior types that never sit between wire and rail.

## [01]-[INDEX]

- [01]-[REBUILD_RECEIPTS]: `ManifoldStatus` six-field Genus-tolerant projection + `GenusClosed` witness; `RebuildReceipt` `[Union]` typed per-op evidence registering `IValidityEvidence`; `RebuildLog` re-anchor seed; `HealSession` carrier whose validity is the `ValidityClaim.All` fold over the chain.

## [02]-[REBUILD_RECEIPTS]

- Owner: `ManifoldStatus` the before/after topological snapshot — the SIX fields the public `VectorIntent.Topology(space, key).Project<(int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus)>` seam yields from the composed `Vectors` `TopologyReceipt` via the Genus-tolerant `ProjectionRow` (`Meshing/mesh.md`), never re-counted: `NonManifoldEdges` is the actionable defect count the manifold kernel targets, `BoundaryComponents` the gap kernel's witness, `IsOriented` the orientation kernel's, and `Genus` stays `Option<int>` because a non-manifold or non-oriented snapshot has no validated genus — exactly the input class the heal admits; `GenusClosed` the derived closed-target witness (`Euler = 2 − 2·Genus` with zero boundary on a manifold — the boolean's success predicate, computed never stored); `RebuildReceipt` `[Union]` the typed per-op evidence — one case per `HealOp` carrying the op's policy tolerance, before/after `ManifoldStatus`, and the affected index sets seeded from the arena dirty bitsets, each case registering its own convergence witness as `IsValid`; `HealSession` the session carrier (input mesh, healed mesh, ordered receipt chain) whose `IsValid` is the `ValidityClaim.All` fold the corpus validity oracle reads; `RebuildLog` the fold projection that flattens a session into the `(EntityKind, affected-index-set)` re-anchor input the `Spatial/naming#TOPO_NAMING` `Track` reads.
- Cases: `RebuildReceipt` cases `DegenerateReceipt` · `GapReceipt` · `WeldReceipt` · `ManifoldReceipt` · `SelfIntersectReceipt` · `OrientReceipt` · `MergeReceipt` (7, one per `HealOp`; the boolean case is `MergeReceipt` because its payload IS arrangement's `BooleanReceipt` — a local record named `BooleanReceipt` beside the composed type is the deleted duplicate); `ManifoldStatus` is one record (not a union) carrying the six projected scalars plus the derived `GenusClosed` witness.
- Entry: `public static RebuildReceipt Of(HealOp op, RepairPolicy policy, ManifoldStatus before, ManifoldStatus after, MeshEdit result, Option<BooleanReceipt> merge)` mints the typed receipt for an applied op — the before/after status arrives ALREADY PROJECTED through the Genus-tolerant seam (the heal session binds the projection on the `Fin` rail before minting, never a swallowed default), the policy travels beside the stateless op so each case records ITS tolerance (`policy.Arena.WeldTolerance` on the weld, `policy.SliverAreaFloor` on the collapse, `policy.GapMaxSpan` on the gap, `policy.MaxManifoldPasses` on the split), the affected seeds read the arena dirty bitsets (`result.DirtyFaces()`/`DirtyVertices()` — monotone within an arena, so a seed over-approximates but never misses an entity; a boolean's fresh arena admits every slot dirty), and the boolean arm carries `merge` — the arrangement's own receipt the `Heal.Merge` step forwarded; `public RebuildLog ToLog()` on `HealSession` folds the chain into the per-`EntityKind` affected-ref set the naming `Track` re-anchors against, filtering by `HealStage.RebuildsTopology` so an `OrientNormals` op contributes nothing (winding leaves adjacency — and hence the `TopoSignature` — unchanged).
- Auto: each `RebuildReceipt` case derives its convergence witness from the six-field delta it already carries — `WeldReceipt`/`DegenerateReceipt`/`SelfIntersectReceipt` assert no new non-manifold edges (a weld additionally never opens boundary), `GapReceipt` asserts `BoundaryComponents` strictly dropped when a bridge landed (an edge-split elsewhere INCREASES boundary components, so a global "boundary improved" heuristic is wrong — the witness is per-op), `ManifoldReceipt` asserts `NonManifoldEdges` reached zero (boundary regression is ADMITTED: the vertex-copy split deliberately opens boundary the later gap pass may close), `OrientReceipt` asserts `IsOriented` with the Euler characteristic unchanged, and `MergeReceipt` asserts `After.GenusClosed` — the boolean's own topological success witness, NOT a gate flag (the `AssetGated`-tested convergence this rebuild killed inverted the fold: a successful boolean never converged); `HealSession.IsValid` folds `ValidityClaim.All` over a non-empty chain of per-receipt witnesses — the ONE registered convergence surface, no standalone `Converged`/`Improved` hand-rolls beside it.
- Receipt: this cluster IS the receipt owner — the `RebuildReceipt` chain on the `HealSession` is the heal evidence the naming `Track` consumes; no parallel heal-tracking ledger, the `ManifoldStatus` is the composed `TopologyReceipt` projection (never a second manifold computation), and the boolean payload is the composed arrangement `BooleanReceipt` (never a second census).
- Packages: `Rasm`/Vectors (`TopologyReceipt` via the Genus-tolerant `ProjectionRow` — composed), `Rasm.Geometry.Meshing` (`MeshEdit` dirty-bitset seed), `Rasm.Geometry.Arrangement` (`BooleanOp`/`BooleanReceipt`/`BooleanRoute` — the composed payload), Rasm.Domain (`IValidityEvidence`/`ValidityClaim` — the registered validity fold), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new heal op is one `RebuildReceipt` case carrying its typed evidence and its own `IsValid` witness arm (mirroring its `HealOp` case — the generated `Switch` breaks every dispatch site until the arm lands); a new topological status field is one column on `ManifoldStatus` projected from the existing `TopologyReceipt` carrier plus one `ProjectionRow` widening at the mesh.md seam; zero new surface — never a generic receipt abstraction collapsing the typed cases.
- Boundary: `RebuildReceipt` is the ONE typed receipt union and a generic `IReceipt`/`HealLedger`/reported-value abstraction erasing the per-kind evidence is the deleted form — a `WeldReceipt`'s merged-vertex set and a `ManifoldReceipt`'s forked-face set are different shapes and stay typed; the before/after status is the composed `Vectors` `TopologyReceipt` projected into `ManifoldStatus` through the un-gated six-field row and a domain-local manifold/genus recomputation is the deleted form; the boolean payload is arrangement's `BooleanReceipt` and a renamed sibling record is the deleted duplicate this rebuild removed; convergence REGISTERS as `IValidityEvidence` and a standalone bool fold beside the registered witness is the deleted hand-roll; the `RebuildLog` feeds the `Spatial/naming#TOPO_NAMING` `Track` re-anchor and the receipt's affected-ref set IS the re-anchor seed — a heal that rebuilds topology without emitting its affected entities is the named defect (the naming fold re-anchors blind); the receipt records op tolerance and payload evidence but mints NO hash and asserts NO content identity — the healed mesh's content hash is the `Spatial/reconciliation#NAMING_HASH` `Encode` job, the receipt only names which entities changed so the reference identity (`TopoName`) re-binds.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using LanguageExt;
using Rasm.Domain;
using Rasm.Geometry.Arrangement;
using Rasm.Geometry.Meshing;
using Rasm.Vectors;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Geometry.Healing;

// --- [MODELS] -----------------------------------------------------------------------------
// The Genus-tolerant six-field snapshot: total over non-manifold/boundaried/odd-Euler inputs —
// Genus stays Option because an unvalidated genus is absence, never a sentinel.
public readonly record struct ManifoldStatus(
    int EulerCharacteristic, int BoundaryComponents, bool IsManifold, bool IsOriented,
    int NonManifoldEdges, Option<int> Genus) {
    public static ManifoldStatus Of((int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus) projection) =>
        new(projection.Euler, projection.BoundaryComponents, projection.IsManifold, projection.IsOriented, projection.NonManifoldEdges, projection.Genus);

    // The genus-consistent closed target: the boolean success witness, derived never stored.
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

    // Per-op convergence witness, REGISTERED into the validity fold: each case tests exactly the
    // six-field delta its kernel moves — a global boundary heuristic is wrong (edge-split opens boundary).
    public bool IsValid =>
        Switch(
            degenerateReceipt:    static d => d.After.NonManifoldEdges <= d.Before.NonManifoldEdges,
            gapReceipt:           static g => g.BridgedFaces.IsEmpty
                ? g.After.BoundaryComponents <= g.Before.BoundaryComponents
                : g.After.BoundaryComponents < g.Before.BoundaryComponents,
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
                // Unreachable None by construction: a gated boolean failed the arrangement rail before any mint.
                merge.IfNone(static () => new BooleanReceipt(Classified: 0, Kept: 0, Welded: 0, Route: BooleanRoute.Managed)),
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

public sealed record RebuildLog(Set<int> Vertices, Set<int> Edges, Set<int> Faces, Seq<string> Ops) {
    public static readonly RebuildLog Empty = new(Set<int>.Empty, Set<int>.Empty, Set<int>.Empty, Seq<string>());

    public bool ReanchorsLineage => !Vertices.IsEmpty || !Edges.IsEmpty || !Faces.IsEmpty;
}

public sealed record HealSession(MeshSpace Input, MeshSpace Healed, Seq<RebuildReceipt> Receipts) : IValidityEvidence {
    // THE registered convergence surface: ValidityClaim.All over the per-receipt witnesses —
    // the standalone Converged/Improved bool folds are dead into this registration.
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Receipts.Count, floor: 1),
        Receipts.ForAll(static receipt => receipt.IsValid));

    // Re-anchor seed: only topology-rebuilding stages contribute — the RebuildsTopology column decides,
    // never a per-case special list.
    public RebuildLog ToLog() =>
        Receipts.Filter(static receipt => receipt.Stage.RebuildsTopology)
            .Fold(RebuildLog.Empty, static (log, receipt) => {
                (Set<int> v, Set<int> e, Set<int> f) = receipt.Affected;
                return log with {
                    Vertices = log.Vertices.TryAddRange(v),
                    Edges = log.Edges.TryAddRange(e),
                    Faces = log.Faces.TryAddRange(f),
                    Ops = log.Ops.Add(receipt.Stage.Key),
                };
            });
}
```

## [03]-[DENSITY_BAR]

One owner per axis; capability is a case or column, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes — pure carriers, the receipts are returned in the `Heal.Repair` rail (`repair.md`).

| [INDEX] | [AXIS/CONCERN]           | [OWNER]                    | [KIND]                                                                                                                          | [RAIL]                                      | [CASES] |
| :-----: | :----------------------- | :------------------------- | :------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------------ | :-----: |
|  [4c]   | Rebuild receipt          | `RebuildReceipt`           | `[Union]` 7 typed per-op cases + `Of` mint (policy-tolerances + dirty-bitset seeds) + `Affected` seed + per-case `IsValid` witness | carrier (returned in `Heal.Repair` rail)    |    7    |
|  [4d]   | Topological status       | `ManifoldStatus`           | record projected from `Vectors` `TopologyReceipt` via the Genus-tolerant six-field row + derived `GenusClosed` witness           | `ManifoldStatus.Of → ManifoldStatus` (pure) |    —    |
|  [4e]   | Heal session + re-anchor | `HealSession`/`RebuildLog` | `IValidityEvidence` session carrier (`ValidityClaim.All` over the chain) + `ToLog` fold gated on `HealStage.RebuildsTopology`    | `HealSession.ToLog → RebuildLog` (pure)     |    —    |

The typed `RebuildReceipt` family, the `ManifoldStatus` projection, and the `HealSession`/`RebuildLog` fold are transcription-complete pure-managed fences composing the `Vectors` `TopologyReceipt` projection seam, the arrangement `BooleanReceipt` payload, and the arena dirty bitsets — none depending on a live-host member spelling beyond the stable native `Mesh` surface the topology sibling pins.

## [04]-[CROSS_PAGE_SEAMS]

Three seams reach sibling owners this page composes but does not write — noted for ALIGN, never edited here.

- `Meshing/mesh.md` `TopologyReceipt.Project` Genus-tolerant row: `ManifoldStatus.Of` binds the un-gated `(int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus)` `ProjectionRow` landed beside the Genus-gated triple — every field already rides the `TopologyReceipt` carrier, so the projection is a re-read, never a recomputation; the Genus-gated triple row stays for consumers whose contract requires a validated genus.
- `Spatial/reconciliation#NAMING_HASH` `Encode` content-address: `HealSession.Healed` is the canonical hash-friendly `MeshSpace` the `CanonicalTopology.OfMesh`/`Encode` content-addresses through the Persistence `GeometryHash`. The heal computes NO hash — it emits the healed mesh and the receipt chain; the naming/hash fence reads the result. The post-heal re-hash (a manifold-repaired mesh re-hashes distinctly because adjacency changed, a welded-but-shape-stable mesh may re-hash identically at the topology level) is the morph-vs-break law the golden fixture asserts — flagged so the heal-then-rehash round-trip is in the cross-package fixture scope. `MeshEdit.Of` triangulates quads by the exact `Predicate.Orient2D` `QuadDiagonal` choice (not a fixed `(A,C)` fan) so a healed n-gon re-hashes diagonal-stable against a shape-identical input — the seam owner confirms `CanonicalTopology.OfMesh` admits an already-triangulated working mesh without re-triangulating on a different diagonal.
- `Spatial/naming#TOPO_NAMING` edge-keying: `RebuildReceipt.Affected` seeds `Edges` as empty for every op — welds re-anchor `Vertices`, splits/collapses re-anchor `Faces`, and `ManifoldRepair`'s edge-fork re-anchors the forked `Faces`/`Vertices` rather than a distinct `Edge` set, because the working arena keys topology by face triples, not half-edge handles. The naming `Track` resolves edges via `VertexNames` (as `Spatial/naming#TOPO_NAMING` `Track(NameTable, CanonicalTopology, Generation)` does), so the empty `Edge` seed is correct; a `Track` that re-anchored edge `TopoName`s from a seeded `Edge` set would under-seed, and the seam owner confirms the `VertexNames` resolution before any edge-key change.
