# [RASM_TOPOLOGY_RECONCILIATION]

The one naming↔hash reconciliation fence that maps stable entity references onto the `Persistence/version-control#STRUCTURAL_DIFF` `GeometryHash` content-address. The page owns `CanonicalTopology` — the immutable hash-friendly adjacency record the Persistence `GeometryHash` content-addresses, built once from native `Mesh` half-edge adjacency through a fixed canonical traversal order; `NamingHash` — the `Reconcile` fold projecting `TopoName` stable refs onto the content hash; and `Encode`, the canonical-adjacency byte emitter. The geometry domain mints NO second identity: `Encode` hashes the EXACT canonical bytes (brep face-edge-vertex adjacency or mesh half-edge adjacency) through the kernel `Domain/ContentHash` seed-zero entry the Persistence `GeometryHash` also content-addresses, and `Reconcile` is the sole bridge. It composes `Vectors` `MeshSpace` and the native `Mesh` topology surface as settled vocabulary — read, never re-mint — and feeds the `naming.md` `Track` fold the per-entity `CanonicalTopology` it re-anchors against.

The canonical-adjacency bytes cross only the in-process seam to the Persistence `GeometryHash`; `CanonicalTopology` and the `NameAddress` records are interior types that never sit between wire and rail. `Reconcile` keeps the reference axis (`TopoName`, lineage-stable) and the content axis (`GeometryHash`, change-sensitive) orthogonal — two identities reconciled by one fence, never collapsed. The page also hosts the `ONE_WIRE_FIXTURE_CORPUS` index — the content-addressed golden-fixture corpus every cross-language parity harness reads — because the geometry domain owns its sole host-derived REAL byte fixture (the `CANONICAL_BYTE_IDENTITY` stream and digest); each other corpus fixture names its producer owner and stays a design-pin gap until that producer pins its byte-deriving input.

## [01]-[INDEX]

- [01]-[NAMING_HASH]: `Reconcile` (`TopoName`→`GeometryHash` projection); `Encode` canonical-adjacency bytes; immutable `CanonicalTopology` records.
- [02]-[ONE_WIRE_FIXTURE_CORPUS]: The content-addressed golden-fixture index every cross-language parity harness reads; the `CANONICAL_BYTE_IDENTITY` stream+digest is its sole host-derived REAL byte fixture.

## [02]-[NAMING_HASH]

- Owner: `CanonicalTopology` the immutable record family the geometry domain emits — the hash-friendly canonical adjacency the Persistence `GeometryHash` content-addresses, built ONCE from `Vectors` mesh half-edge adjacency or brep face-edge-vertex adjacency through a fixed canonical traversal order (lowest-index-vertex rotation per cycle, sorted boundary, little-endian) so two byte-identical topologies emit byte-identical bytes; `NamingHash` the static reconciliation surface owning `Encode` (canonical bytes) and `Reconcile` (TopoName→GeometryHash projection); `NameAddress` the per-name content-address binding (a `TopoName` reference identity paired with the `UInt128` content hash of its current canonical entity bytes).
- Cases: `CanonicalTopology` is built by ONE `OfMesh` factory over the native `Mesh` topology (vertices in topology-vertex order, edges as sorted endpoint-name pairs, faces as lowest-vertex-rotated boundary cycles) — a brep overload reads face-edge-vertex adjacency through the identical canonical-order law, so the encoder is one byte-order owner across both modalities, never two serializers. `BuildEntities` derives the per-entity `RebuiltEntity` re-anchor input from the real adjacency (a vertex's incidence is its star, an edge's its two endpoints, a face's its boundary cycle; the `[vertex, edge, face]` kind histogram counts true incident degree, never a hand-literal) so distinct neighborhoods yield distinct `TopoSignature`s.
- Entry: `public static UInt128 Encode(CanonicalTopology topology)` emits the canonical adjacency digest the `Persistence/version-control#STRUCTURAL_DIFF` `GeometryHash` reads — byte-identical to the bytes Persistence `StructuralMerge` content-addresses (the FROZEN golden-bytes fixture both packages assert against); `public static Fin<NamingHash> Reconcile(NameTable names, CanonicalTopology topology)` projects every `TopoName` onto the `GeometryHash` of its current canonical entity bytes — `Fin<T>` routes `GeometryFault.HashMismatch` (band 2400) when a name's entity bytes are absent from the topology (a dangling reference is a defect, never a silent skip), returning the `NameAddress` map where each lineage-stable name binds to its change-sensitive content hash.
- Auto: `Encode` writes the WHOLE-topology digest as `XxHash128` over the canonical byte stream — topology-vertex count, then each edge as its sorted (min,max) topology-vertex-index pair, then each face as its lowest-index-rotated boundary topology-vertex cycle — the EXACT adjacency encoding the Persistence `GeometryHash` reads, so a morph (moved control points, same adjacency) re-hashes identically at the topology level while a topology break (changed adjacency) re-hashes distinctly; `Reconcile` folds the `NameTable` entries, hashing each entry's `CanonicalBytes` via `XxHash128` into the per-name content address, so a moved-but-unchanged face keeps its `TopoName` (reference identity from `Track`) AND re-derives the same per-entity `GeometryHash` (content identity), which is exactly the reconciliation the bridge owns.
- Receipt: `NamingHash` carries the whole-topology `GeometryHash` plus the `NameAddress` map (TopoName→content-hash); this IS the reconciliation evidence the Persistence `StructuralMerge` consumes as `GraphNode.GeometryHash` per node — no parallel reconciliation ledger.
- Packages: `Rasm`/Vectors (native `Mesh` topology surface — composed), `Rasm` (the kernel `Domain.ContentHash` seed-zero entry — composed, no second hasher), `System.Buffers.Binary` (`BinaryPrimitives`), LanguageExt.Core, BCL inbox.
- Growth: a new geometry modality (brep beside mesh) is one `CanonicalTopology.Of*` factory sharing the identical canonical-order law; a new reconciliation projection is one column on `NameAddress`; zero new surface — never a second hash function.
- Boundary: `Encode` emits the canonical bytes the Persistence `GeometryHash` content-addresses and the geometry domain computes NO hash beyond this shared encoder — a domain-local content address or a parallel serializer is the deleted form; this page is the SOLE OWNER of the frozen canonical-adjacency byte layout — the FROZEN field order is `int32-LE VertexCount` · `int32-LE EdgeCount` · `(int32-LE Min, int32-LE Max)` per sorted edge endpoint pair in `Edges` order · `int32-LE FaceCount` · per lowest-vertex-rotated face cycle `(int32-LE CycleLength, int32-LE Vertex…)` — every integer little-endian, no padding, no separators, hashed by `XxHash128.HashToUInt128` over the contiguous stream, and `Persistence/version-control#STRUCTURAL_DIFF` reads this IDENTICAL layout before its `XxHash128` rather than re-deriving a second encoding; the golden-bytes fixture is the cross-package byte-identity proof both packages assert against, so a drifted byte order in either package is a caught defect, not a silent merge corruption; `Reconcile` keeps the reference axis (`TopoName`, lineage-stable) and the content axis (`GeometryHash`, change-sensitive) ORTHOGONAL — they are two identities reconciled by one fence, and a name bound directly as a content hash (or a hash treated as a name) is the collapsed defect; a dangling reference routes `GeometryFault.HashMismatch(...).ToError()` over the `Fin` rail (the band-2400 union from `Numerics/faults.md`, lowered into the `Error` rail through `ToError()`), never a silent skip; the `CanonicalTopology` records are immutable so the emitted bytes are referentially transparent — a mutable adjacency buffer feeding the hash is the named non-determinism defect; the per-name content hash reuses the SAME kernel `Domain/ContentHash` seed-zero entry the Persistence `GeometryHash` composes (one hash function across the federation), never a domain-chosen digest.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------------
public sealed record CanonicalTopology(
    int VertexCount,
    Arr<(int Min, int Max)> Edges,
    Arr<int[]> Faces,
    Seq<RebuiltEntity> Entities) {

    public static CanonicalTopology OfMesh(MeshSpace space) {
        var mesh = space.DuplicateNative();
        int vertices = mesh.TopologyVertices.Count;
        var edges = toSeq(Enumerable.Range(0, mesh.TopologyEdges.Count)).Map(e => {
            IndexPair p = mesh.TopologyEdges.GetTopologyVertices(e);
            return p.I <= p.J ? (Min: p.I, Max: p.J) : (Min: p.J, Max: p.I);
        }).ToArr();
        var faces = toSeq(Enumerable.Range(0, mesh.Faces.Count)).Map(f => {
            int[] cycle = mesh.TopologyVertices.IndicesFromFace(f);
            return RotateToLowest(cycle);
        }).ToArr();
        var entities = BuildEntities(vertices, edges, faces);
        return new CanonicalTopology(vertices, edges, faces, entities);
    }

    static int[] RotateToLowest(int[] cycle) {
        int pivot = 0;
        for (int i = 1; i < cycle.Length; i++) if (cycle[i] < cycle[pivot]) pivot = i;
        var rotated = new int[cycle.Length];
        for (int i = 0; i < cycle.Length; i++) rotated[i] = cycle[(pivot + i) % cycle.Length];
        return rotated;
    }

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

public readonly record struct NameAddress(TopoName Name, EntityKind Kind, UInt128 ContentHash);

public sealed record NamingHash(UInt128 GeometryHash, HashMap<TopoName, NameAddress> Addresses);

// --- [OPERATIONS] --------------------------------------------------------------------------------
public static class NamingHashOps {
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
        return ContentHash.Of(canonical.WrittenSpan);
    }

    public static Fin<NamingHash> Reconcile(NameTable names, CanonicalTopology topology) =>
        names.Entries.Values.Fold(Fin.Succ(HashMap<TopoName, NameAddress>.Empty),
            (acc, entry) => acc.Bind(map => entry.CanonicalBytes.Length == 0
                ? Fin.Fail<HashMap<TopoName, NameAddress>>(GeometryFault.HashMismatch(entry.Name.Value, entry.Kind.Key).ToError())
                : Fin.Succ(map.AddOrUpdate(entry.Name,
                    new NameAddress(entry.Name, entry.Kind, ContentHash.Of(entry.CanonicalBytes))))))
            .Map(addresses => new NamingHash(Encode(topology), addresses));
}
```

## [03]-[ONE_WIRE_FIXTURE_CORPUS]

The single content-addressed golden-fixture corpus every runtime parity harness reads — the missing parent of the `CONTENT_IDENTITY_PARITY`, `FAULT_WIRE_ROUNDTRIP`, and `TRI_LANGUAGE_WIRE_PARITY` cross-language obligations. Every fixture is keyed by the one `XxHash128` seed (seed zero, two-64-bit-half order) the federation mints C#-side, frozen once so a parity drift surfaces as a single corpus mismatch instead of three divergent reproductions. This cluster is the corpus INDEX; the `CANONICAL_BYTE_IDENTITY` stream and digest above are the sole host-derived REAL byte fixture the geometry domain owns directly, and each other fixture names its producer owner and its `[SOURCE]` state — REAL when the bytes are host-derived or deterministically derivable from a settled design, DESIGN-PIN when the design input is not yet frozen. A DESIGN-PIN fixture carries no fabricated byte set; its bytes are derived only after the producer page pins the missing input.

- Owner: the corpus is a federation index, not a second store — each fixture lives with its producer page and this cluster binds them under the one seed. No fixture byte set is invented here; a DESIGN-PIN row is a blocking gap on the named producer, never a placeholder byte literal.
- Consumers: `python:runtime/evidence/identity#IDENTITY` reproduces the seed through `xxhash.xxh3_128_intdigest` and reads the corpus to assert byte-identical reproduction; `typescript:wire/frame#CONTENT_HASHING` reproduces it through the 128-bit wasm hash and reads the `tests/contracts` corpus (TS readers in `tests/typescript/_testkit`); the C# shared test corpus (the `[BRANCH_TEST_NODE_PROVISIONING]` shared-corpus task) asserts every producer emits its fixture byte-for-byte. Every consumer reads the one frozen corpus, never re-deriving its own fixtures.
- Boundary: the seed is the single key across all fixtures — a per-fixture digest function or a per-runtime fixture mint is the named drift defect; the corpus is read-only for every consumer and every producer asserts its own fixture against the frozen bytes; a fabricated byte set for an unpinned fixture is the rejected form, replaced by an explicit DESIGN-PIN gap on the producer.

The fixture set the corpus carries, each keyed by the one seed:

| [INDEX] | [FIXTURE]               | [PRODUCER_OWNER]                                                              | [SOURCE]   | [SHAPE]                                                                                                                                                         |
| :-----: | :---------------------- | :---------------------------------------------------------------------------- | :--------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | CANONICAL_BYTE_IDENTITY | `Rasm/Geometry/Spatial/reconciliation#CANONICAL_BYTE_IDENTITY` (this page)    | REAL       | 52-byte int32-LE adjacency stream + `0x9462A71A5DD13DCFA3B1D6D225FCBE70` digest                                                                                 |
|  [02]   | CLASH_GOLDEN            | `Rasm/Geometry/Spatial/index#CLASH_GOLDEN`                                    | REAL       | 8-primitive `BoundingBox(min,max)` set → 160-byte `Bounds`/`Nodes` LE stream; `NodeCount == 3`, clash-pair set `{(0,1),(2,3),(4,5),(6,7)}` (4 pairs) all PINNED |
|  [03]   | FAULT_TRIPLES           | `Rasm.Compute/Runtime/channels#FAULT_PROJECTION`                              | DESIGN-PIN | `FaultDetail` `(package, code, case)` triples spanning the disjoint bands (ComputeFault 2200, HopFault 4500, WireFault 4520-4532, store/config app roots)       |
|  [04]   | CRDT_OP_SET             | `Rasm.Persistence/Version/commits#CRDT_ALGEBRA`                               | DESIGN-PIN | the `MvRegister`/`opMerge` op-set whose divergent-delivery folds converge byte-identically                                                                      |
|  [05]   | GLB_BY_KEY              | `Rasm.Compute/Runtime/codecs#TILE_PARTITION` over the GLB tessellation result | DESIGN-PIN | one content-keyed GLB sample keyed by the `ContentIdentity` seed                                                                                                |
|  [06]   | HLC_TWO_HALF            | `Rasm.AppHost/Runtime/ports#HLC_FANIN`                                        | DESIGN-PIN | the two-64-bit-half HLC stamps whose half order an off-by-one-half would corrupt                                                                                |
|  [07]   | MATERIAL_LAYER_GOLDEN   | `Rasm.Element/Projection/address#CONTENT_ADDRESS`                             | DESIGN-PIN | the float-bearing `IfcMaterialLayer`-shaped node (layer thicknesses + properties) whose `CanonicalWriter` IEEE-754-LE bytes the C#/Python/TypeScript `ContentAddress` agree on byte-for-byte                       |

Fixtures [1] and [2] are REAL and frozen against their producer pages — [1] CANONICAL_BYTE_IDENTITY on this page (the 52-byte stream, the `XxHash128.HashToUInt128` digest, the morph and topology-break discriminators), [2] CLASH_GOLDEN in `Rasm/Geometry/Spatial/index#CLASH_GOLDEN` now that the 8 `BoundingBox(min,max)` coordinate tuples, the SAH split (axis X, primitives `{0,1,2,3}` left / `{4,5,6,7}` right), the identity `Order` permutation, `NodeCount == 3`, the 160-byte `Bounds`/`Nodes` LE stream, and the 4-pair clash set `{(0,1),(2,3),(4,5),(6,7)}` are PINNED and the `NodeLinkProjection` bytes are deterministically derivable from those coordinates. Fixtures [3]–[7] remain DESIGN-PIN: the bytes are derivable only after each producer page (in the `Rasm.Compute`/`Rasm.Persistence`/`Rasm.AppHost`/`Rasm.Element` siblings, outside this folder's write-scope) pins its missing input, and no fabricated byte set stands in for an unpinned fixture. No design-pin gap inside the geometry kernel's write-scope blocks corpus completion; the remaining gaps are owned by the named cross-folder producers and are the synthesis tier's arbitration.

## [04]-[DENSITY_BAR]

One owner per axis; capability is a factory or fold arm, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes.

| [INDEX] | [AXIS/CONCERN]             | [OWNER]                      | [KIND]                                                            | [RAIL]                                                | [CASES] |
| :-----: | :------------------------- | :--------------------------- | :---------------------------------------------------------------- | :---------------------------------------------------- | :-----: |
|  [06]   | Canonical adjacency        | `CanonicalTopology`          | immutable hash-friendly record + `OfMesh` canonical-order encoder | `CanonicalTopology.OfMesh → CanonicalTopology` (pure) |    —    |
|  [07]   | Naming↔hash reconciliation | `NamingHash`/`NamingHashOps` | static surface + `Encode` + `Reconcile` fold                      | `NamingHashOps.Reconcile → Fin<NamingHash>`           |    2    |

## [05]-[RESEARCH]

- [CANONICAL_BYTE_IDENTITY] — `NamingHashOps.Encode` emits the canonical adjacency bytes (`int32-LE VertexCount` · `int32-LE EdgeCount` · `(int32-LE Min, int32-LE Max)` per sorted edge endpoint pair · `int32-LE FaceCount` · per lowest-vertex-rotated face cycle `(int32-LE CycleLength, int32-LE Vertex…)` · every integer little-endian, contiguous, no padding) the `Persistence/version-control#STRUCTURAL_DIFF` `GeometryHash` reads; this page is the SOLE OWNER of that frozen field order and Persistence reads the identical layout, never a second encoding. The byte-identity contract is the FROZEN golden-bytes fixture both the geometry domain and Persistence `StructuralMerge` assert against — the shared reference is the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) whose 52-byte canonical stream is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00` and whose `XxHash128.HashToUInt128` digest is `0x9462A71A5DD13DCFA3B1D6D225FCBE70` (16-byte LE `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94`); a tier-2 cross-package byte-equality harness feeds this reference through both `NamingHashOps.Encode` and the Persistence `GeometryHash` path and asserts the stream bytes AND the digest, with the morph case (moved control points, same adjacency → equal hash) and the topology-break case (changed adjacency → distinct hash) as the two discriminating laws. The live-host spellings are HOST-VALIDATED against the Rhino 9 WIP `Mesh` surface: `Mesh.TopologyVertices.IndicesFromFace(int)` returns the boundary cycle in CCW winding `[0,1,2]` for the single-triangle reference, `Mesh.TopologyEdges.GetTopologyVertices(int) : IndexPair` yields the sorted endpoint pairs `(0,1),(0,2),(1,2)`, the `IndexPair.I`/`.J` member surface carries the pair endpoints, and `RotateToLowest([0,1,2]) = [0,1,2]` is the identity rotation — the rotation law is SETTLED, the stream and digest are the FROZEN reference fixture, and this fact is never re-asserted from a fresh host probe.
