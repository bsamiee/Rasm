# [RASM_TOPOLOGY_RECONCILIATION]

The one naming↔hash reconciliation fence that maps stable entity references onto the Persistence `version-control#STRUCTURAL_DIFF` `GeometryHash` content-address. The page owns `CanonicalTopology` — the immutable hash-friendly adjacency record the Persistence `GeometryHash` content-addresses, built once from native `Mesh` half-edge adjacency through a fixed canonical traversal order; `NamingHash` — the `Reconcile` fold projecting `TopoName` stable refs onto the content hash; and `Encode`, the canonical-adjacency byte emitter. The geometry domain computes NO hash and mints NO second identity: `Encode` emits the EXACT canonical bytes (brep face-edge-vertex adjacency or mesh half-edge adjacency) the Persistence `XxHash128` reads, and `Reconcile` is the sole bridge. It composes `Vectors` `MeshSpace` and the native `Mesh` topology surface as settled vocabulary — read, never re-mint — and feeds the `naming.md` `Track` fold the per-entity `CanonicalTopology` it re-anchors against.

The canonical-adjacency bytes cross only the in-process seam to the Persistence `GeometryHash`; `CanonicalTopology` and the `NameAddress` records are interior types that never sit between wire and rail. `Reconcile` keeps the reference axis (`TopoName`, lineage-stable) and the content axis (`GeometryHash`, change-sensitive) orthogonal — two identities reconciled by one fence, never collapsed.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                                                          |
| :-----: | :---------- | :--------------------------------------------------------------------------------------------- |
|   [1]   | NAMING_HASH | `Reconcile` (TopoName→GeometryHash projection); `Encode` canonical-adjacency bytes; immutable `CanonicalTopology` records |

## [2]-[NAMING_HASH]

- Owner: `CanonicalTopology` the immutable record family the geometry domain emits — the hash-friendly canonical adjacency the Persistence `GeometryHash` content-addresses, built ONCE from `Vectors` mesh half-edge adjacency or brep face-edge-vertex adjacency through a fixed canonical traversal order (lowest-index-vertex rotation per cycle, sorted boundary, little-endian) so two byte-identical topologies emit byte-identical bytes; `NamingHash` the static reconciliation surface owning `Encode` (canonical bytes) and `Reconcile` (TopoName→GeometryHash projection); `NameAddress` the per-name content-address binding (a `TopoName` reference identity paired with the `UInt128` content hash of its current canonical entity bytes).
- Cases: `CanonicalTopology` is built by ONE `OfMesh` factory over the native `Mesh` topology (vertices in topology-vertex order, edges as sorted endpoint-name pairs, faces as lowest-vertex-rotated boundary cycles) — a brep overload reads face-edge-vertex adjacency through the identical canonical-order law, so the encoder is one byte-order owner across both modalities, never two serializers.
- Entry: `public static UInt128 Encode(CanonicalTopology topology)` emits the canonical adjacency digest the Persistence `version-control#STRUCTURAL_DIFF` `GeometryHash` reads — byte-identical to the bytes Persistence `StructuralMerge` content-addresses (the FROZEN golden-bytes fixture both packages assert against); `public static Fin<NamingHash> Reconcile(NameTable names, CanonicalTopology topology)` projects every `TopoName` onto the `GeometryHash` of its current canonical entity bytes — `Fin<T>` routes `GeometryFault.HashMismatch` (band 2400) when a name's entity bytes are absent from the topology (a dangling reference is a defect, never a silent skip), returning the `NameAddress` map where each lineage-stable name binds to its change-sensitive content hash.
- Auto: `Encode` writes the WHOLE-topology digest as `XxHash128` over the canonical byte stream — topology-vertex count, then each edge as its sorted (min,max) topology-vertex-index pair, then each face as its lowest-index-rotated boundary topology-vertex cycle — the EXACT adjacency encoding the Persistence `GeometryHash` reads, so a morph (moved control points, same adjacency) re-hashes identically at the topology level while a topology break (changed adjacency) re-hashes distinctly; `Reconcile` folds the `NameTable` entries, hashing each entry's `CanonicalBytes` via `XxHash128` into the per-name content address, so a moved-but-unchanged face keeps its `TopoName` (reference identity from `Track`) AND re-derives the same per-entity `GeometryHash` (content identity), which is exactly the reconciliation the bridge owns.
- Receipt: `NamingHash` carries the whole-topology `GeometryHash` plus the `NameAddress` map (TopoName→content-hash); this IS the reconciliation evidence the Persistence `StructuralMerge` consumes as `GraphNode.GeometryHash` per node — no parallel reconciliation ledger.
- Packages: `Rasm`/Vectors (native `Mesh` topology surface — composed), `System.IO.Hashing` (`XxHash128`), `System.Buffers.Binary` (`BinaryPrimitives`), LanguageExt.Core, BCL inbox.
- Growth: a new geometry modality (brep beside mesh) is one `CanonicalTopology.Of*` factory sharing the identical canonical-order law; a new reconciliation projection is one column on `NameAddress`; zero new surface — never a second hash function.
- Boundary: `Encode` emits the canonical bytes the Persistence `GeometryHash` content-addresses and the geometry domain computes NO hash beyond this shared encoder — a domain-local content address or a parallel serializer is the deleted form; the canonical order is FROZEN (sorted edge endpoints, lowest-vertex-rotated face cycles, little-endian) and the golden-bytes fixture is the cross-package byte-identity proof both packages assert against, so a drifted byte order in either package is a caught defect, not a silent merge corruption; `Reconcile` keeps the reference axis (`TopoName`, lineage-stable) and the content axis (`GeometryHash`, change-sensitive) ORTHOGONAL — they are two identities reconciled by one fence, and a name bound directly as a content hash (or a hash treated as a name) is the collapsed defect; a dangling reference routes `GeometryFault.HashMismatch(...).ToError()` over the `Fin` rail (the band-2400 `Error`-derived union from `faults/faults.md`), never a silent skip; the `CanonicalTopology` records are immutable so the emitted bytes are referentially transparent — a mutable adjacency buffer feeding the hash is the named non-determinism defect; the per-name content hash reuses the SAME `XxHash128` the Persistence `GeometryHash` uses (one hash function across the federation), never a domain-chosen digest.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------------
// The hash-friendly immutable adjacency the geometry domain emits and Persistence GeometryHash content-addresses.
// Built ONCE from Rasm/Vectors native Mesh topology through the FROZEN canonical-order law; immutable so the
// emitted bytes are referentially transparent. Faces are stored as lowest-vertex-rotated boundary cycles;
// edges as sorted endpoint-index pairs — the same canonical bytes Persistence StructuralMerge addresses.
public sealed record CanonicalTopology(
    int VertexCount,
    Arr<(int Min, int Max)> Edges,        // sorted topology-vertex endpoint pairs, edge-index order
    Arr<int[]> Faces,                     // lowest-index-rotated boundary topology-vertex cycles, face-index order
    Seq<RebuiltEntity> Entities) {        // the per-entity canonical-bytes view the naming Track fold consumes

    // ONE encoder over native Mesh topology: read TopologyVertices/TopologyEdges/Faces, canonicalize, never re-mint.
    public static CanonicalTopology OfMesh(MeshSpace space) {
        var mesh = space.DuplicateNative();
        int vertices = mesh.TopologyVertices.Count;
        var edges = toSeq(Enumerable.Range(0, mesh.TopologyEdges.Count)).Map(e => {
            IndexPair p = mesh.TopologyEdges.GetTopologyVertices(e);
            return p.I <= p.J ? (Min: p.I, Max: p.J) : (Min: p.J, Max: p.I);
        }).ToArr();
        var faces = toSeq(Enumerable.Range(0, mesh.Faces.Count)).Map(f => {
            int[] cycle = mesh.TopologyVertices.IndicesFromFace(f); // boundary topology-vertex cycle
            return RotateToLowest(cycle);
        }).ToArr();
        var entities = BuildEntities(vertices, edges, faces);
        return new CanonicalTopology(vertices, edges, faces, entities);
    }

    // Canonical cycle rotation: start the boundary cycle at its lowest topology-vertex index so a cyclic
    // permutation of the same face emits identical bytes; orientation is preserved (no reversal).
    static int[] RotateToLowest(int[] cycle) {
        int pivot = 0;
        for (int i = 1; i < cycle.Length; i++) if (cycle[i] < cycle[pivot]) pivot = i;
        var rotated = new int[cycle.Length];
        for (int i = 0; i < cycle.Length; i++) rotated[i] = cycle[(pivot + i) % cycle.Length];
        return rotated;
    }

    // Per-entity canonical-bytes + INTRINSIC incident topology-vertex indices the Track fold resolves to prior
    // names. A vertex's incidence is its own index plus its star (the vertices it shares an edge with); an
    // edge's is its two endpoints; a face's is its boundary cycle. The kind histogram counts incident
    // [vertex, edge, face] degree from the real adjacency, never a hand-literal — so distinct neighborhoods
    // produce distinct signatures and the re-anchor input is the true topology.
    static Seq<RebuiltEntity> BuildEntities(int vertices, Arr<(int Min, int Max)> edges, Arr<int[]> faces) {
        var star = Enumerable.Range(0, vertices).ToDictionary(v => v, _ => new SortedSet<int>());
        foreach (var (min, max) in edges) { star[min].Add(min); star[min].Add(max); star[max].Add(min); star[max].Add(max); }
        var vertexEdgeDegree = Enumerable.Range(0, vertices).ToDictionary(v => v, _ => 0);
        foreach (var (min, max) in edges) { vertexEdgeDegree[min]++; vertexEdgeDegree[max]++; }
        var vertexFaceDegree = Enumerable.Range(0, vertices).ToDictionary(v => v, _ => 0);
        foreach (int[] c in faces) foreach (int v in c.Distinct()) vertexFaceDegree[v]++;
        return toSeq(Enumerable.Range(0, vertices))
            .Map(v => new RebuiltEntity(EntityKind.Vertex, Bytes(v), star[v].ToArray(), new[] { star[v].Count, vertexEdgeDegree[v], vertexFaceDegree[v] }))
            .Append(toSeq(edges).Map(e => new RebuiltEntity(EntityKind.Edge, Bytes(e.Min, e.Max), new[] { e.Min, e.Max }, new[] { 2, 1, faces.Count(c => Adjacent(c, e.Min, e.Max)) })))
            .Append(toSeq(faces).Map(c => new RebuiltEntity(EntityKind.Face, Bytes(c), c, new[] { c.Distinct().Count(), c.Length, 1 })));
    }

    // An edge (min,max) lies on a face boundary when both endpoints appear consecutively in the cycle.
    static bool Adjacent(int[] cycle, int min, int max) {
        for (int i = 0; i < cycle.Length; i++) {
            int a = cycle[i], b = cycle[(i + 1) % cycle.Length];
            if ((a == min && b == max) || (a == max && b == min)) return true;
        }
        return false;
    }

    static byte[] Bytes(params int[] values) {
        var buffer = new ArrayBufferWriter<byte>(values.Length * 4);
        foreach (int value in values) { BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], value); buffer.Advance(4); }
        return buffer.WrittenSpan.ToArray();
    }
}

// The reconciliation evidence: the whole-topology content hash plus per-name content-address bindings.
// TopoName (reference, lineage-stable) ↔ GeometryHash (content, change-sensitive) — two orthogonal axes, one fence.
public readonly record struct NameAddress(TopoName Name, EntityKind Kind, UInt128 ContentHash);

public sealed record NamingHash(UInt128 GeometryHash, HashMap<TopoName, NameAddress> Addresses);

// --- [OPERATIONS] --------------------------------------------------------------------------------
public static class NamingHashOps {
    // The canonical-adjacency digest the Persistence version-control#STRUCTURAL_DIFF GeometryHash reads —
    // byte-identical to the bytes Persistence StructuralMerge content-addresses (the golden fixture).
    // Vertex count, then sorted edge endpoint pairs, then lowest-rotated face cycles, little-endian throughout.
    public static UInt128 Encode(CanonicalTopology topology) {
        var canonical = new ArrayBufferWriter<byte>();
        BinaryPrimitives.WriteInt32LittleEndian(canonical.GetSpan(4)[..4], topology.VertexCount); canonical.Advance(4);
        BinaryPrimitives.WriteInt32LittleEndian(canonical.GetSpan(4)[..4], topology.Edges.Count); canonical.Advance(4);
        foreach (var (min, max) in topology.Edges) {
            BinaryPrimitives.WriteInt32LittleEndian(canonical.GetSpan(4)[..4], min); canonical.Advance(4);
            BinaryPrimitives.WriteInt32LittleEndian(canonical.GetSpan(4)[..4], max); canonical.Advance(4);
        }
        BinaryPrimitives.WriteInt32LittleEndian(canonical.GetSpan(4)[..4], topology.Faces.Count); canonical.Advance(4);
        foreach (int[] cycle in topology.Faces) {
            BinaryPrimitives.WriteInt32LittleEndian(canonical.GetSpan(4)[..4], cycle.Length); canonical.Advance(4);
            foreach (int vertex in cycle) { BinaryPrimitives.WriteInt32LittleEndian(canonical.GetSpan(4)[..4], vertex); canonical.Advance(4); }
        }
        return XxHash128.HashToUInt128(canonical.WrittenSpan);
    }

    // Project every TopoName onto the GeometryHash of its current canonical entity bytes: the reconciliation bridge.
    // A moved-but-unchanged entity keeps its name (Track) AND re-derives the same content hash here.
    public static Fin<NamingHash> Reconcile(NameTable names, CanonicalTopology topology) =>
        names.Entries.Values.Fold(Fin.Succ(HashMap<TopoName, NameAddress>.Empty),
            (acc, entry) => acc.Bind(map => entry.CanonicalBytes.Length == 0
                ? Fin.Fail<HashMap<TopoName, NameAddress>>(GeometryFault.HashMismatch(entry.Name.Value, entry.Kind.Key).ToError())
                : Fin.Succ(map.AddOrUpdate(entry.Name,
                    new NameAddress(entry.Name, entry.Kind, XxHash128.HashToUInt128(entry.CanonicalBytes))))))
            .Map(addresses => new NamingHash(Encode(topology), addresses));
}
```

## [3]-[DENSITY_BAR]

One owner per axis; capability is a factory or fold arm, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes.

| [INDEX] | [AXIS/CONCERN]            | [OWNER]            | [KIND]                                                                | [RAIL]                                          | [CASES] |
| :-----: | :------------------------ | :---------------- | :------------------------------------------------------------------- | :--------------------------------------------- | :-----: |
|   [6]   | Canonical adjacency       | `CanonicalTopology` | immutable hash-friendly record + `OfMesh` canonical-order encoder   | `CanonicalTopology.OfMesh → CanonicalTopology` (pure) |    —    |
|   [7]   | Naming↔hash reconciliation | `NamingHash`/`NamingHashOps` | static surface + `Encode` + `Reconcile` fold                 | `NamingHashOps.Reconcile → Fin<NamingHash>`   |    2    |

## [4]-[RESEARCH]

- [CANONICAL_BYTE_IDENTITY] — `NamingHashOps.Encode` emits the canonical adjacency bytes (vertex count · sorted edge endpoint pairs · lowest-vertex-rotated face cycles · little-endian) the Persistence `version-control#STRUCTURAL_DIFF` `GeometryHash` reads. The byte-identity contract is the FROZEN golden-bytes fixture both the geometry domain and Persistence `StructuralMerge` assert against — a tier-2 cross-package byte-equality harness feeds one reference mesh through both `NamingHashOps.Encode` and the Persistence `GeometryHash` path and asserts `XxHash128` equality, the morph case (moved control points, same adjacency → equal hash) and the topology-break case (changed adjacency → distinct hash) as the two discriminating laws; the still-unverified live-host spellings are the RhinoCommon `Mesh.TopologyVertices.IndicesFromFace(int)`, `Mesh.TopologyEdges.GetTopologyVertices(int) : IndexPair`, and `IndexPair.I`/`.J` member surface (composed from `Vectors`, confirmed against the Vectors `Mesh.cs` usage at `TopologyEdges.GetTopologyVertices`/`GetConnectedFaces`) — confirm `IndicesFromFace` returns the boundary cycle in consistent winding before the rotation law is settled.
