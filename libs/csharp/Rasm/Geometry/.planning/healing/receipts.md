# [RASM_HEALING_RECEIPTS]

The typed per-op rebuild evidence the heal rail emits and the naming `Track` re-anchor consumes. The page owns `ManifoldStatus` (the before/after topological snapshot projected from the composed `Vectors` `TopologyReceipt`), the `RebuildReceipt` `[Union]` — one typed case per `HealOp` recording op kind, tolerance, before/after status, and the affected entity refs — the `HealSession` carrier the naming `Track` reads, and the `RebuildLog` fold that flattens a session into the per-`EntityKind` re-anchor seed. The receipt records the op tolerance and the gate state but mints NO hash and asserts NO content identity — the healed mesh's content hash is the `topology/reconciliation#NAMING_HASH` `Encode` job; the receipt only names which entities changed so the reference identity (`TopoName`) re-binds.

The `RebuildReceipt` chain on the `HealSession` crosses only the in-process seam to the naming `Track` fold; the receipt records are interior types that never sit between wire and rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                                              |
| :-----: | :---------------- | :------------------------------------------------------------------------------------------------- |
|   [1]   | REBUILD_RECEIPTS  | `RebuildReceipt` `[Union]` typed per-op evidence; `ManifoldStatus` projection; `HealSession` carrier; the `Fold` that threads receipts across a heal session for the naming `Track` re-anchor |

## [2]-[REBUILD_RECEIPTS]

- Owner: `ManifoldStatus` the before/after topological snapshot (`EulerCharacteristic`, `Genus`, `BoundaryComponents` — the triple the public `VectorIntent.Topology(...).Project<(int Euler, int Genus, int BoundaryComponents)>` seam yields from the composed `Vectors` `TopologyReceipt`, never re-counted, and never the non-projectable `IsManifold`/`NonManifoldEdges`); `RebuildReceipt` `[Union]` the typed per-op evidence — one case per `HealOp` recording op kind, tolerance, before/after `ManifoldStatus`, and the affected entity refs (vertex/face index sets), so a `WeldReceipt` carries the weld tolerance and the merged-vertex set, a `ManifoldReceipt` carries the split-edge count, a `BooleanReceipt` carries the `BooleanOp` and the native-asset gate state — a typed receipt per heal kind, never a generic `IReceipt`/ledger; `HealSession` the session carrier (input mesh, healed mesh, the ordered `RebuildReceipt` chain) the naming `Track` re-anchor consumes; `RebuildLog` the fold projection that flattens a `HealSession` into the `(EntityKind, affected-index-set)` re-anchor input the `topology/naming#TOPO_NAMING` `Track` reads.
- Cases: `RebuildReceipt` cases `DegenerateReceipt` · `GapReceipt` · `WeldReceipt` · `ManifoldReceipt` · `SelfIntersectReceipt` · `OrientReceipt` · `BooleanReceipt` (7, one per `HealOp`); `ManifoldStatus` is one record (not a union) carrying the topological scalars.
- Entry: `public static RebuildReceipt Of(HealOp op, ManifoldStatus before, ManifoldStatus after, MeshEdit result)` mints the typed receipt for an applied op — the before/after status arrives ALREADY PROJECTED through the public `VectorIntent.Topology(space).Project<(int Euler, int Genus, int BoundaryComponents)>` seam (the heal session binds the projection on the `Fin` rail before minting, never a swallowed default), discriminating on the `HealOp` case to build the matching `RebuildReceipt` case with the op's evidence and the affected index set from the `MeshEdit`; `public RebuildLog ToLog()` on `HealSession` folds the chain into the per-`EntityKind` affected-ref set the naming `Track` re-anchors against (a `RebuildsTopology` op contributes its affected faces/vertices; an `OrientNormals` op contributes nothing to re-anchoring because winding leaves adjacency — and hence the `TopoSignature` — unchanged).
- Auto: `Of` reads the `HealOp.Kind` and the `MeshEdit.AffectedFaces`/`AffectedVertices` to construct the case-matched receipt — the before/after `ManifoldStatus` carries the `(EulerCharacteristic, Genus, BoundaryComponents)` triple the public seam projects (the only public `TopologyReceipt` projection; `IsManifold`/`NonManifoldEdges` are not projectable and `MeshKernel.TopologyDetailed` is internal to `Rasm.Vectors`) so the receipt records the exact topological delta the op produced (a `GapClose` shows `BoundaryComponents` dropping toward 0, a `ManifoldRepair`/`SelfIntersectResolve` shows `EulerCharacteristic` moving toward its genus-consistent closed target `2 − 2·Genus`); `HealSession.ToLog` folds the chain, unioning the affected-index sets of the `RebuildsTopology` ops into the per-`EntityKind` re-anchor seed (face ops touch `EntityKind.Face`, vertex welds touch `EntityKind.Vertex`, edge splits touch `EntityKind.Edge`) so the naming `Track` knows exactly which entities changed lineage and which survived — a moved-but-untouched face keeps its `TopoName`, a split face's children `Migrate` from the affected-face seed.
- Receipt: this cluster IS the receipt owner — the `RebuildReceipt` chain on the `HealSession` is the heal evidence the naming `Track` consumes; no parallel heal-tracking ledger, and the `ManifoldStatus` is the composed `TopologyReceipt` projection, never a second manifold computation.
- Packages: `Rasm`/Vectors (`TopologyReceipt` — composed), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new heal op is one `RebuildReceipt` case carrying its typed evidence (mirroring its `HealOp` case); a new topological status field is one column on `ManifoldStatus` projected from the existing `TopologyReceipt`; zero new surface — never a generic receipt abstraction collapsing the typed cases.
- Boundary: `RebuildReceipt` is the ONE typed receipt union and a generic `IReceipt`/`HealLedger`/reported-value abstraction erasing the per-kind evidence is the deleted form — a `WeldReceipt`'s merged-vertex set and a `ManifoldReceipt`'s split-edge count are different shapes and stay typed; the before/after status is the composed `Vectors` `TopologyReceipt` projected into `ManifoldStatus` and a domain-local manifold/genus recomputation is the deleted form (the topology owner already computes it); the `RebuildLog` feeds the `topology/naming#TOPO_NAMING` `Track` re-anchor and the receipt's affected-ref set IS the re-anchor seed — a heal that rebuilds topology without emitting its affected entities is the named defect (the naming fold would re-anchor blind); the receipt records the op tolerance and the gate state but mints NO hash and asserts NO content identity — the healed mesh's content hash is the `topology/reconciliation#NAMING_HASH` `Encode` job, the receipt only names which entities changed so the reference identity (`TopoName`) re-binds; the `BooleanReceipt` carries the `BooleanOp` and the native-asset gate state so a gated boolean produces an honest receipt (op attempted, asset missing) rather than a silent success — the receipt is the audit that the tier-3 gate held.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
// The before/after topological snapshot each receipt records. SEAM REALITY: the ONLY public projection of the composed
// Rasm/Vectors TopologyReceipt is VectorIntent.Topology(space).Project<(int Euler, int Genus, int BoundaryComponents)>
// (the receipt exposes exactly this tuple as a ProjectionRow; Project<TopologyReceipt> is UNSUPPORTED,
// MeshKernel.TopologyDetailed is internal to Rasm.Vectors). So ManifoldStatus carries the projectable triple, never the
// non-projectable IsManifold/NonManifoldEdges fields. Improvement is a closed-surface delta: a watertight 2-manifold has
// BoundaryComponents 0 and Euler = 2 - 2·Genus, so a heal improves iff boundary count drops (cracks/gaps closed) or, at
// equal boundary, the Euler characteristic moves toward its genus-consistent target — never a re-derived manifold flag.
public readonly record struct ManifoldStatus(int EulerCharacteristic, int Genus, int BoundaryComponents) {
    public static ManifoldStatus Of((int Euler, int Genus, int BoundaryComponents) projection) =>
        new(projection.Euler, projection.Genus, projection.BoundaryComponents);

    // Closed 2-manifold target: BoundaryComponents 0 and Euler == 2 - 2·Genus. A heal improves iff it closes boundary
    // (fewer cracks/holes) or, at equal boundary, brings Euler closer to its genus-consistent closed value.
    public bool Improved(ManifoldStatus before) =>
        BoundaryComponents < before.BoundaryComponents
        || (BoundaryComponents == before.BoundaryComponents
            && Math.Abs(EulerCharacteristic - (2 - (2 * Genus))) < Math.Abs(before.EulerCharacteristic - (2 - (2 * before.Genus))));
}

// One typed receipt per heal op — the modality is the case, never a generic ledger. Each carries op kind, tolerance,
// before/after ManifoldStatus, and the affected entity refs (the re-anchor seed the naming Track consumes).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RebuildReceipt {
    private RebuildReceipt() { }

    public sealed record DegenerateReceipt(double SliverFloor, ManifoldStatus Before, ManifoldStatus After, Set<int> CollapsedFaces) : RebuildReceipt;
    public sealed record GapReceipt(double MaxSpan, ManifoldStatus Before, ManifoldStatus After, Set<int> BridgedFaces, Set<int> StitchedVertices) : RebuildReceipt;
    public sealed record WeldReceipt(double Tolerance, ManifoldStatus Before, ManifoldStatus After, Set<int> MergedVertices) : RebuildReceipt;
    public sealed record ManifoldReceipt(int SplitEdges, ManifoldStatus Before, ManifoldStatus After, Set<int> ForkedFaces, Set<int> ForkedVertices) : RebuildReceipt;
    public sealed record SelfIntersectReceipt(double Tolerance, ManifoldStatus Before, ManifoldStatus After, Set<int> SplitFaces) : RebuildReceipt;
    public sealed record OrientReceipt(ManifoldStatus Before, ManifoldStatus After, Set<int> FlippedFaces) : RebuildReceipt;
    public sealed record BooleanReceipt(BooleanOp Op, bool AssetGated, ManifoldStatus Before, ManifoldStatus After, Set<int> SelectedFaces) : RebuildReceipt;

    public HealKind Kind =>
        Switch(
            degenerateReceipt:     static _ => HealKind.Degenerate,
            gapReceipt:            static _ => HealKind.Gap,
            weldReceipt:           static _ => HealKind.Weld,
            manifoldReceipt:       static _ => HealKind.Manifold,
            selfIntersectReceipt:  static _ => HealKind.SelfIntersect,
            orientReceipt:         static _ => HealKind.Orient,
            booleanReceipt:        static _ => HealKind.Boolean);

    // Mint the case-matched receipt for an applied op: read the HealOp case, the MeshEdit affected sets, and the
    // before/after status, building the typed evidence. The affected-ref set IS the naming Track re-anchor seed. The
    // before/after status arrives ALREADY PROJECTED (the (Euler, Genus, BoundaryComponents) triple the public seam yields).
    public static RebuildReceipt Of(HealOp op, ManifoldStatus before, ManifoldStatus after, MeshEdit result) {
        (ManifoldStatus b, ManifoldStatus a) = (before, after);
        return op.Switch<RebuildReceipt>(
            degenerateCollapse:   d => new DegenerateReceipt(d.Policy.SliverAreaFloor, b, a, result.AffectedFaces),
            gapClose:             g => new GapReceipt(g.Policy.GapMaxSpan, b, a, result.AffectedFaces, result.AffectedVertices),
            duplicateWeld:        w => new WeldReceipt(w.Policy.WeldTolerance, b, a, result.AffectedVertices),
            manifoldRepair:       _ => new ManifoldReceipt(result.AffectedFaces.Count, b, a, result.AffectedFaces, result.AffectedVertices),
            selfIntersectResolve: s => new SelfIntersectReceipt(s.Policy.IntersectTolerance, b, a, result.AffectedFaces),
            orientNormals:        _ => new OrientReceipt(b, a, result.AffectedFaces),
            boolean:              bo => new BooleanReceipt(bo.Op, AssetGated: false, b, a, result.AffectedFaces));
    }

    // The per-EntityKind affected-ref contribution to the naming Track re-anchor seed. An Orient receipt contributes
    // nothing — winding leaves adjacency (and hence the position-free TopoSignature) unchanged, so no name re-anchors.
    public (Set<int> Vertices, Set<int> Edges, Set<int> Faces) Affected =>
        Switch(
            degenerateReceipt:     static d => (Set<int>.Empty, Set<int>.Empty, d.CollapsedFaces),
            gapReceipt:            static g => (g.StitchedVertices, Set<int>.Empty, g.BridgedFaces),
            weldReceipt:           static w => (w.MergedVertices, Set<int>.Empty, Set<int>.Empty),
            manifoldReceipt:       static m => (m.ForkedVertices, Set<int>.Empty, m.ForkedFaces),
            selfIntersectReceipt:  static s => (Set<int>.Empty, Set<int>.Empty, s.SplitFaces),
            orientReceipt:         static _ => (Set<int>.Empty, Set<int>.Empty, Set<int>.Empty),
            booleanReceipt:        static b => (Set<int>.Empty, Set<int>.Empty, b.SelectedFaces));
}

// The session carrier: input mesh, healed mesh, the ordered receipt chain. ToLog folds the chain into the
// per-EntityKind affected-ref seed the topology/naming#TOPO_NAMING Track re-anchor consumes across the rebuild.
public sealed record HealSession(MeshSpace Input, MeshSpace Healed, Seq<RebuildReceipt> Receipts) {
    public RebuildLog ToLog() =>
        Receipts.Fold(RebuildLog.Empty, static (log, receipt) => {
            var (v, e, f) = receipt.Affected;
            return log with {
                Vertices = log.Vertices.TryAddRange(v),
                Edges = log.Edges.TryAddRange(e),
                Faces = log.Faces.TryAddRange(f),
                Ops = log.Ops.Add(receipt.Kind.Key),
            };
        });

    // True iff every RebuildsTopology op left the mesh manifold-improving-or-stable — the heal session's net verdict.
    public bool Converged =>
        Receipts.ForAll(static r => r.Switch(
            degenerateReceipt:     static d => true,
            gapReceipt:            static g => true,
            weldReceipt:           static w => true,
            manifoldReceipt:       static m => m.After.Improved(m.Before),
            selfIntersectReceipt:  static _ => true,
            orientReceipt:         static _ => true,
            booleanReceipt:        static b => b.AssetGated));
}

// The re-anchor seed the naming Track reads: the union of every topology-changing op's affected entity refs, keyed
// by EntityKind, plus the applied-op key sequence (the lineage-provenance trail). This is exactly the topology/
// naming#TOPO_NAMING Track input — the heal names which entities changed; the naming fold re-binds TopoName lineage.
public sealed record RebuildLog(Set<int> Vertices, Set<int> Edges, Set<int> Faces, Seq<string> Ops) {
    public static readonly RebuildLog Empty = new(Set<int>.Empty, Set<int>.Empty, Set<int>.Empty, Seq<string>());

    public bool ReanchorsLineage => !Vertices.IsEmpty || !Edges.IsEmpty || !Faces.IsEmpty;
}
```

## [3]-[DENSITY_BAR]

One owner per axis; capability is a case or column, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes — pure carriers, the receipts are returned in the `Heal.Repair` rail (`repair.md`).

| [INDEX] | [AXIS/CONCERN]            | [OWNER]            | [KIND]                                                                          | [RAIL]                                          | [CASES] |
| :-----: | :------------------------ | :---------------- | :----------------------------------------------------------------------------- | :--------------------------------------------- | :-----: |
|  [4c]   | Rebuild receipt           | `RebuildReceipt`  | `[Union]` 7 typed per-op evidence cases + `Of` mint + `Affected` re-anchor seed | carrier (returned in `Heal.Repair` rail)       |    7    |
|  [4d]   | Topological status        | `ManifoldStatus`  | record projected from `Vectors` `TopologyReceipt` via the public `(Euler, Genus, BoundaryComponents)` seam | `ManifoldStatus.Of → ManifoldStatus` (pure)    |    —    |
|  [4e]   | Heal session + re-anchor  | `HealSession`/`RebuildLog` | session carrier + `ToLog` fold into the naming `Track` re-anchor seed   | `HealSession.ToLog → RebuildLog` (pure)         |    —    |

The typed `RebuildReceipt` family, the `ManifoldStatus` projection, and the `HealSession`/`RebuildLog` fold are transcription-complete pure-managed fences composing the `Vectors` `TopologyReceipt` projection seam, none depending on a live-host member spelling beyond the stable native `Mesh` surface the topology sibling pins.

## [4]-[CROSS_PAGE_SEAMS]

Two seams reach sibling owners this page composes but does not write — noted for ALIGN, never edited here.

- `topology/reconciliation#NAMING_HASH` `Encode` content-address: `HealSession.Healed` is the canonical hash-friendly `MeshSpace` the `CanonicalTopology.OfMesh`/`Encode` content-addresses through the Persistence `GeometryHash`. The heal computes NO hash — it emits the healed mesh and the receipt chain; the naming/hash fence reads the result. The post-heal re-hash (a manifold-repaired mesh re-hashes distinctly because adjacency changed, a welded-but-shape-stable mesh may re-hash identically at the topology level) is the morph-vs-break law the golden fixture asserts — flagged so the heal-then-rehash round-trip is in the cross-package fixture scope. `MeshEdit.OfMesh` triangulates quads by the exact `Predicate.Orient3D` `QuadDiagonal` choice (not a fixed `(A,C)` fan) so a healed n-gon re-hashes diagonal-stable against a shape-identical input — the seam owner confirms `CanonicalTopology.OfMesh` admits an already-triangulated working mesh without re-triangulating on a different diagonal.
- `topology/naming#TOPO_NAMING` edge-keying: `RebuildReceipt.Affected` seeds `Edges` as empty for every op — welds re-anchor `Vertices`, splits/collapses re-anchor `Faces`, and `ManifoldRepair`'s edge-fork re-anchors the forked `Faces`/`Vertices` rather than a distinct `Edge` set, because the working `MeshEdit` keys topology by face triples, not half-edge handles. If the naming `Track` re-anchors edge `TopoName`s from a seeded `Edge` set rather than re-deriving edges from `VertexNames`, healing under-seeds edge re-anchoring; the seam owner confirms `Track` resolves edges via `VertexNames` (as `topology/naming#TOPO_NAMING` `Track(NameTable, CanonicalTopology, Generation)` currently does) so the empty `Edge` seed is correct, or healing emits forked-edge keys.
