# [RASM_TOPOLOGY]

Rasm geometry-domain topology owner: persistent topological naming that survives rebuilds, and the one naming↔hash reconciliation fence that maps stable entity references onto the Persistence `version-control#STRUCTURAL_DIFF` `GeometryHash` content-address ([R2]). The page owns `TopoName` — a `[ValueObject]` lineage reference (one naming algebra over every entity modality, never a per-entity-type parallel namer), the `NameTable` registry that carries the generation/lineage record, and the `Track` re-anchoring fold that re-binds names across a rebuild by topological signature; and `NamingHash` — the `Reconcile` fold projecting `TopoName` stable refs onto `GeometryHash`, the `Encode` canonical-adjacency byte emitter the Persistence `GeometryHash` content-addresses, and the `CanonicalTopology` immutable record family that feeds the content-address. The geometry domain computes NO hash and mints NO second identity: `Encode` emits the EXACT canonical bytes (brep face-edge-vertex adjacency or mesh half-edge adjacency) the Persistence `XxHash128` reads, and `Reconcile` is the sole bridge. It composes `Rasm`/Vectors `MeshSpace` and the native `Mesh` topology surface (`TopologyVertices`/`TopologyEdges`/`Faces`, `GetConnectedFaces`, `GetTopologyVertices`, `GetConnectedTopologyVertices`) as SETTLED vocabulary — read, never re-mint.

Wire posture: HOST-LOCAL, no TS_PROJECTION cluster. `TopoName` is a reference identity that never crosses a transport; the canonical-adjacency bytes cross only the in-process seam to the Persistence `GeometryHash`. The `EntityKind` discriminant, `TopoName` value object, and `CanonicalTopology` records are interior types that never sit between wire and rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                                                          |
| :-----: | :---------- | :--------------------------------------------------------------------------------------------- |
|   [1]   | TOPO_NAMING | One `TopoName` lineage algebra; `NameTable` generation registry; `Track` re-anchor-by-signature fold |
|   [2]   | NAMING_HASH | `Reconcile` (TopoName→GeometryHash projection); `Encode` canonical-adjacency bytes; immutable `CanonicalTopology` records |

## [2]-[TOPO_NAMING]

- Owner: `EntityKind` `[SmartEnum<int>]` the entity-modality discriminant (`Vertex`/`Edge`/`Face`) carrying the per-kind signature-arity column; `TopoSignature` the rebuild-invariant topological fingerprint a name re-anchors against (boundary-vertex multiset + incident-kind histogram, position-free so a rigid move keeps the signature); `TopoName` `[ValueObject<UInt128>]` the stable lineage reference — one naming algebra over every `EntityKind`, the modality lives in the `Kind` column of the `NameEntry` row, never a `VertexName`/`EdgeName`/`FaceName` parallel triple; `Generation` the monotone rebuild counter; `NameEntry` the lineage record (name + kind + birth generation + last-seen generation + provenance parent name + current signature + stored resolved boundary-name set + canonical bytes); `NameTable` the immutable registry keyed by `TopoName` with the signature index AND a topology-vertex-index→name row (`VertexNames`) that resolves intrinsic incidence to prior names; `Track` the re-anchoring fold that matches names across a rebuild.
- Cases: `EntityKind` rows `Vertex` (arity 0, signature is its own position bucket) · `Edge` (arity 2, signature is its endpoint-name multiset) · `Face` (arity n, signature is its boundary-edge-name cycle) (3); `Track` outcomes per rebuilt entity are `Survived` (signature match → keep name, bump last-seen) · `Migrated` (parent-signature subset match → new name, parent provenance) · `Born` (no match → fresh name) folded into the next-generation `NameTable`.
- Entry: `public static Fin<NameTable> Track(NameTable prior, CanonicalTopology rebuilt, Generation next)` — `Fin<T>` routes a `GeometryFault.NameCollision` (band 2400) when two rebuilt entities of one kind resolve to the same prior name (a non-injective re-anchor is a defect, never a silent overwrite); the fold walks every rebuilt entity, resolves its `TopoSignature` against the prior signature index, and emits the next-generation table where each entity carries either its surviving prior name, a migrated child name with parent provenance, or a fresh name minted from the entity's canonical bytes. `public static TopoName Mint(EntityKind kind, ReadOnlySpan<byte> canonicalBytes, Generation born)` mints the lineage-root name as `XxHash128`-derived `UInt128` over `(kind ordinal · canonical entity bytes · born generation)` so a fresh name is content-stable but lineage-distinct across generations.
- Auto: `Track` folds entities in `EntityKind` order (vertices first so `VertexNames` is populated before edges/faces resolve incidence) and reads the prior `NameTable.SignatureIndex` (`HashMap<EntityKind, HashMap<TopoSignature, TopoName>>`) so the exact-match probe is one hash-map lookup per rebuilt entity, never a quadratic cross-product; each entity's intrinsic incident topology-vertex indices resolve to prior `TopoName`s through `NameTable.ResolveBoundary` (the `VertexNames` row), and that resolved set IS the position-free signature input. An exact `TopoSignature` hit is `Survived` (the entity kept its topological neighborhood), a parent-boundary subset hit (the prior entry's stored `Boundary` names are a proper subset of the rebuilt entity's) is `Migrated` with the prior name threaded as `Parent` provenance under a deterministic smallest-`TopoName` tiebreak (a face split inherits the parent face's lineage; the tiebreak makes a split-into-n choose one stable parent), and a miss is `Born`. The single `EntityKind` discriminant means one fold body serves vertex, edge, and face re-anchoring — the signature arity is a row column, not three fold copies.
- Receipt: `Track` returns the next-generation `NameTable` directly (the registry IS the receipt — birth/last-seen generations and parent provenance are the per-name lineage evidence); no parallel tracking ledger.
- Packages: `Rasm`/Vectors (`MeshSpace`, native `Mesh` topology — composed), Thinktecture.Runtime.Extensions, LanguageExt.Core, `System.IO.Hashing` (`XxHash128`), BCL inbox.
- Growth: a new entity modality is one `EntityKind` row with its signature-arity column and one signature-builder arm; a new lineage outcome (e.g. `Merged` for a face-merge re-bind) is one `TrackOutcome` case on the existing fold; zero new surface — never a fourth `*Name` value object.
- Boundary: `TopoName` is the ONE naming value object over all entity kinds and a `VertexName`/`EdgeName`/`FaceName` triple is the deleted form — the modality is the `Kind` column; `TopoSignature` is position-free (built from incident NAMES and kind histograms, never coordinates) so a rigid transform preserves every name and only an adjacency change re-anchors, which is exactly the `GeometryHash` morph-vs-topology-break distinction read from the same canonical adjacency; the re-anchor reads the prior `SignatureIndex` and a per-rebuild O(n·m) signature cross-scan is the named defect; `Track` is total over the `Fin` rail and a thrown collision is forbidden — a non-injective name resolution routes `GeometryFault.NameCollision`, where the sibling-owned `GeometryFault` (`faults.md`, band 2400) is an `Error`-derived union so `GeometryFault.NameCollision(...).ToError()` is the `Fin<T>` failure channel and no separate error type sits in the rail; the name is a content-address-derived `UInt128` but it is a REFERENCE identity (which entity, lineage-stable across generations), orthogonal to the `GeometryHash` CONTENT identity (what shape, change-sensitive) — minting a second content hash here is the deleted form ([R2]), and `TopoName.Value` is NEVER equality-tested against a `NameAddress.ContentHash` even though both are raw `UInt128` (the reference and content axes never compare cross-axis); the `Migrated` parent is resolved by the stored `NameEntry.Boundary` column (never re-derived) with a deterministic smallest-`TopoName` tiebreak so a face split into n children selects one stable lineage parent; the `NameTable` is immutable and `Track` returns the next generation, never an in-place mutation of the prior table.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class EntityKind {
    public static readonly EntityKind Vertex = new(key: 0, signatureArity: 0);
    public static readonly EntityKind Edge   = new(key: 1, signatureArity: 2);
    public static readonly EntityKind Face   = new(key: 2, signatureArity: -1); // -1 = variadic boundary cycle

    public int SignatureArity { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TrackOutcome {
    private TrackOutcome() { }

    public sealed record Survived(TopoName Name) : TrackOutcome;
    public sealed record Migrated(TopoName Name, TopoName Parent) : TrackOutcome;
    public sealed record Born(TopoName Name) : TrackOutcome;
}

// --- [MODELS] ------------------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct Generation {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError("Generation must be >= 0.");

    public Generation Next() => From(Value + 1);
}

// Position-free rebuild-invariant fingerprint: the entity's incident-NAME multiset (sorted, canonical)
// plus an incident-kind histogram. Built from names + kinds only — never coordinates — so a rigid move
// preserves the signature and only an adjacency change re-anchors, matching the GeometryHash morph/break law.
[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoSignature {
    public static TopoSignature Of(EntityKind kind, ReadOnlySpan<TopoName> incidentNames, ReadOnlySpan<int> kindHistogram) {
        var buffer = new ArrayBufferWriter<byte>((incidentNames.Length + kindHistogram.Length + 1) * 16);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], kind.Key); buffer.Advance(4);
        // canonical order: sort the incident names so neighbor permutation never perturbs the signature
        var sorted = incidentNames.ToArray(); Array.Sort(sorted, static (a, b) => a.Value.CompareTo(b.Value));
        foreach (var name in sorted) { BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], name.Value); buffer.Advance(16); }
        foreach (int count in kindHistogram) { BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], count); buffer.Advance(4); }
        return From(XxHash128.HashToUInt128(buffer.WrittenSpan));
    }

    // Parent-subset test for the Migrated arm: the prior boundary names are a proper sub-neighborhood of the
    // rebuilt entity (a face split inherits its parent's boundary). Pure set predicate over two sorted name
    // sets — no signature state participates, so it is static, not an instance method.
    public static bool Subsumes(ReadOnlySpan<TopoName> priorBoundary, ReadOnlySpan<TopoName> rebuiltBoundary) {
        var rebuilt = rebuiltBoundary.ToArray(); Array.Sort(rebuilt, static (a, b) => a.Value.CompareTo(b.Value));
        foreach (var name in priorBoundary)
            if (Array.BinarySearch(rebuilt, name, Comparer<TopoName>.Create(static (a, b) => a.Value.CompareTo(b.Value))) < 0) return false;
        return priorBoundary.Length > 0 && priorBoundary.Length < rebuiltBoundary.Length;
    }
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoName {
    // Lineage-root mint: content-stable over the entity's canonical bytes, lineage-distinct across generations.
    public static TopoName Mint(EntityKind kind, ReadOnlySpan<byte> canonicalBytes, Generation born) {
        var buffer = new ArrayBufferWriter<byte>(canonicalBytes.Length + 8);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], kind.Key); buffer.Advance(4);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], born.Value); buffer.Advance(4);
        buffer.Write(canonicalBytes);
        return From(XxHash128.HashToUInt128(buffer.WrittenSpan));
    }
}

// One lineage record over every EntityKind — the modality is the Kind column, never a parallel *Entry type.
// Boundary is the resolved incident-name set stored as a column (not re-derived) so the Migrated re-anchor's
// parent-subset test reads the prior generation's boundary directly via BoundaryOf.
public readonly record struct NameEntry(
    TopoName Name, EntityKind Kind, Generation Born, Generation LastSeen, Option<TopoName> Parent, TopoSignature Signature, Seq<TopoName> Boundary, byte[] CanonicalBytes);

// The rebuilt entity the fold consumes: its kind, its canonical bytes, the INTRINSIC incident topology-vertex
// indices of its neighborhood (resolved to prior TopoNames inside Track via the prior generation's VertexNames
// row — adjacency is intrinsic to the mesh, names are extrinsic to the generation), and the kind histogram.
public readonly record struct RebuiltEntity(EntityKind Kind, byte[] CanonicalBytes, int[] IncidentVertices, int[] KindHistogram);

public sealed record NameTable(
    HashMap<TopoName, NameEntry> Entries,
    HashMap<EntityKind, HashMap<TopoSignature, TopoName>> SignatureIndex,
    HashMap<int, TopoName> VertexNames,   // topology-vertex index → its TopoName, this generation; resolves intrinsic incidence to prior names
    Generation Generation) {
    public static readonly NameTable Empty =
        new(HashMap<TopoName, NameEntry>.Empty, HashMap<EntityKind, HashMap<TopoSignature, TopoName>>.Empty, HashMap<int, TopoName>.Empty, Generation.From(0));

    public Option<TopoName> Resolve(EntityKind kind, TopoSignature signature) =>
        SignatureIndex.Find(kind).Bind(index => index.Find(signature));

    // Resolve intrinsic incident topology-vertex indices to the PRIOR generation's TopoNames; an index with no
    // prior name (a freshly inserted vertex) drops out, so the boundary set is exactly the surviving neighborhood.
    public Seq<TopoName> ResolveBoundary(ReadOnlySpan<int> incidentVertices) =>
        toSeq(incidentVertices.ToArray()).Map(VertexNames.Find).Somes();

    public NameTable With(NameEntry entry, int vertexIndex) {
        var index = SignatureIndex.Find(entry.Kind).IfNone(HashMap<TopoSignature, TopoName>.Empty).AddOrUpdate(entry.Signature, entry.Name);
        var vertices = entry.Kind == EntityKind.Vertex ? VertexNames.AddOrUpdate(vertexIndex, entry.Name) : VertexNames;
        return this with { Entries = Entries.AddOrUpdate(entry.Name, entry), SignatureIndex = SignatureIndex.AddOrUpdate(entry.Kind, index), VertexNames = vertices };
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------------
public static class TopoNaming {
    // Re-anchor every rebuilt entity against the prior table by topological signature; one fold body serves
    // vertex/edge/face because EntityKind is a discriminant column, not three fold copies. Vertices fold first
    // (EntityKind.Key order) so the next-generation VertexNames row is populated before edges/faces resolve.
    public static Fin<NameTable> Track(NameTable prior, CanonicalTopology rebuilt, Generation next) =>
        rebuilt.Entities.OrderBy(static e => e.Kind.Key)
            .Fold(Fin.Succ((Table: NameTable.Empty with { Generation = next }, Claimed: HashSet<TopoName>.Empty)),
                (acc, entity) => acc.Bind(state => Anchor(prior, entity, next, state.Claimed).Map(bound =>
                    (Table: state.Table.With(bound.Entry, entity.IncidentVertices[0]), Claimed: state.Claimed.Add(bound.Entry.Name)))))
            .Map(static state => state.Table);

    static Fin<(NameEntry Entry, TrackOutcome Outcome)> Anchor(
        NameTable prior, RebuiltEntity entity, Generation next, HashSet<TopoName> claimed) {
        // Resolve intrinsic incident topology-vertex indices to the PRIOR generation's names — that resolved set
        // IS the position-free signature input AND the parent-subset boundary the Migrated arm tests against.
        var boundary = prior.ResolveBoundary(entity.IncidentVertices);
        var signature = TopoSignature.Of(entity.Kind, boundary.ToArray(), entity.KindHistogram);
        return prior.Resolve(entity.Kind, signature).Match(
            Some: name => claimed.Contains(name)
                ? Fin.Fail<(NameEntry, TrackOutcome)>(GeometryFault.NameCollision(name.Value, entity.Kind.Key).ToError())
                : Survive(prior, name, signature, boundary, next, entity),
            None: () => MigrateOrBirth(prior, entity, signature, boundary, next, claimed));
    }

    static Fin<(NameEntry, TrackOutcome)> Survive(NameTable prior, TopoName name, TopoSignature signature, Seq<TopoName> boundary, Generation next, RebuiltEntity entity) =>
        prior.Entries.Find(name).Match(
            Some: prev => Fin.Succ((prev with { LastSeen = next, Signature = signature, Boundary = boundary, CanonicalBytes = entity.CanonicalBytes }, (TrackOutcome)new TrackOutcome.Survived(name))),
            None: () => Fin.Fail<(NameEntry, TrackOutcome)>(GeometryFault.NameCollision(name.Value, entity.Kind.Key).ToError()));

    static Fin<(NameEntry, TrackOutcome)> MigrateOrBirth(NameTable prior, RebuiltEntity entity, TopoSignature signature, Seq<TopoName> boundary, Generation next, HashSet<TopoName> claimed) {
        var rebuiltBoundary = boundary.ToArray();
        var parent = prior.Entries.Values
            .Filter(prev => prev.Kind == entity.Kind && !claimed.Contains(prev.Name) && TopoSignature.Subsumes(prev.Boundary.ToArray(), rebuiltBoundary))
            .OrderBy(static prev => prev.Name.Value)   // deterministic parent: smallest TopoName under multi-subsume (face split into n)
            .HeadOrNone();
        var name = TopoName.Mint(entity.Kind, entity.CanonicalBytes, next);
        var entry = new NameEntry(name, entity.Kind, next, next, parent.Map(static p => p.Name), signature, boundary, entity.CanonicalBytes);
        return Fin.Succ((entry, parent.Match(
            Some: p => (TrackOutcome)new TrackOutcome.Migrated(name, p.Name),
            None: () => new TrackOutcome.Born(name))));
    }
}
```

## [3]-[NAMING_HASH]

- Owner: `CanonicalTopology` the immutable record family the geometry domain emits — the hash-friendly canonical adjacency the Persistence `GeometryHash` content-addresses, built ONCE from `Rasm`/Vectors mesh half-edge adjacency or brep face-edge-vertex adjacency through a fixed canonical traversal order (lowest-index-vertex rotation per cycle, sorted boundary, little-endian) so two byte-identical topologies emit byte-identical bytes; `NamingHash` the static reconciliation surface owning `Encode` (canonical bytes) and `Reconcile` (TopoName→GeometryHash projection); `NameAddress` the per-name content-address binding (a `TopoName` reference identity paired with the `UInt128` content hash of its current canonical entity bytes).
- Cases: `CanonicalTopology` is built by ONE `OfMesh` factory over the native `Mesh` topology (vertices in topology-vertex order, edges as sorted endpoint-name pairs, faces as lowest-vertex-rotated boundary cycles) — a brep overload reads face-edge-vertex adjacency through the identical canonical-order law, so the encoder is one byte-order owner across both modalities, never two serializers.
- Entry: `public static UInt128 Encode(CanonicalTopology topology)` emits the canonical adjacency digest the Persistence `version-control#STRUCTURAL_DIFF` `GeometryHash` reads — byte-identical to the bytes Persistence `StructuralMerge` content-addresses (the FROZEN golden-bytes fixture both packages assert against per `[G4]`); `public static Fin<NamingHash> Reconcile(NameTable names, CanonicalTopology topology)` projects every `TopoName` onto the `GeometryHash` of its current canonical entity bytes — `Fin<T>` routes `GeometryFault.HashMismatch` (band 2400) when a name's entity bytes are absent from the topology (a dangling reference is a defect, never a silent skip), returning the `NameAddress` map where each lineage-stable name binds to its change-sensitive content hash.
- Auto: `Encode` writes the WHOLE-topology digest as `XxHash128` over the canonical byte stream — topology-vertex count, then each edge as its sorted (min,max) topology-vertex-index pair, then each face as its lowest-index-rotated boundary topology-vertex cycle — the EXACT adjacency encoding the Persistence `GeometryHash` reads, so a morph (moved control points, same adjacency) re-hashes identically at the topology level while a topology break (changed adjacency) re-hashes distinctly; `Reconcile` folds the `NameTable` entries, hashing each entry's `CanonicalBytes` via `XxHash128` into the per-name content address, so a moved-but-unchanged face keeps its `TopoName` (reference identity from `Track`) AND re-derives the same per-entity `GeometryHash` (content identity), which is exactly the [R2] reconciliation.
- Receipt: `NamingHash` carries the whole-topology `GeometryHash` plus the `NameAddress` map (TopoName→content-hash); this IS the reconciliation evidence the Persistence `StructuralMerge` consumes as `GraphNode.GeometryHash` per node — no parallel reconciliation ledger.
- Packages: `Rasm`/Vectors (native `Mesh` topology surface — composed), `System.IO.Hashing` (`XxHash128`), `System.Buffers.Binary` (`BinaryPrimitives`), LanguageExt.Core, BCL inbox.
- Growth: a new geometry modality (brep beside mesh) is one `CanonicalTopology.Of*` factory sharing the identical canonical-order law; a new reconciliation projection is one column on `NameAddress`; zero new surface — never a second hash function.
- Boundary: `Encode` emits the canonical bytes the Persistence `GeometryHash` content-addresses and the geometry domain computes NO hash beyond this shared encoder — a domain-local content address or a parallel serializer is the deleted form ([R2]); the canonical order is FROZEN (sorted edge endpoints, lowest-vertex-rotated face cycles, little-endian) and the `[G4]` golden-bytes fixture is the cross-package byte-identity proof both packages assert against, so a drifted byte order in either package is a caught defect, not a silent merge corruption; `Reconcile` keeps the reference axis (`TopoName`, lineage-stable) and the content axis (`GeometryHash`, change-sensitive) ORTHOGONAL — they are two identities reconciled by one fence, and a name bound directly as a content hash (or a hash treated as a name) is the collapsed defect; a dangling reference routes `GeometryFault.HashMismatch(...).ToError()` over the `Fin` rail (the band-2400 `Error`-derived union from `faults.md`), never a silent skip; the `CanonicalTopology` records are immutable so the emitted bytes are referentially transparent — a mutable adjacency buffer feeding the hash is the named non-determinism defect; the per-name content hash reuses the SAME `XxHash128` the Persistence `GeometryHash` uses (one hash function across the federation), never a domain-chosen digest.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------------
// The hash-friendly immutable adjacency the geometry domain emits and Persistence GeometryHash content-addresses.
// Built ONCE from Rasm/Vectors native Mesh topology through the FROZEN canonical-order law; immutable so the
// emitted bytes are referentially transparent. Faces are stored as lowest-vertex-rotated boundary cycles;
// edges as sorted endpoint-index pairs — the same canonical bytes Persistence StructuralMerge addresses ([G4]).
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
    // byte-identical to the bytes Persistence StructuralMerge content-addresses (the [G4] golden fixture).
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

    // Project every TopoName onto the GeometryHash of its current canonical entity bytes: the [R2] bridge.
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

## [4]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]            | [OWNER]            | [KIND]                                                                | [RAIL]                                          | [CASES] |  [STATE]  |
| :-----: | :------------------------ | :---------------- | :------------------------------------------------------------------- | :--------------------------------------------- | :-----: | :-------: |
|   [1]   | Entity modality           | `EntityKind`      | `[SmartEnum<int>]` Vertex/Edge/Face + signature-arity column          | discriminant (pure)                            |    3    | FINALIZED |
|   [2]   | Re-anchor outcome         | `TrackOutcome`    | `[Union]` Survived/Migrated/Born                                      | carrier (returned in `Track` rail)             |    3    | FINALIZED |
|   [3]   | Topological fingerprint   | `TopoSignature`   | `[ValueObject<UInt128>]` position-free incident-name/kind digest + `Subsumes` | `TopoSignature.Of → TopoSignature` (pure)      |    —    | FINALIZED |
|   [4]   | Stable lineage reference  | `TopoName`        | `[ValueObject<UInt128>]` one naming algebra over all kinds + `Mint`   | `TopoName.Mint → TopoName` (pure)              |    —    | FINALIZED |
|   [5]   | Naming registry + re-anchor | `NameTable`/`TopoNaming` | immutable registry + signature index + `VertexNames` row + `Track` fold | `TopoNaming.Track → Fin<NameTable>`            |    3    |   SPIKE   |
|   [6]   | Canonical adjacency       | `CanonicalTopology` | immutable hash-friendly record + `OfMesh` canonical-order encoder   | `CanonicalTopology.OfMesh → CanonicalTopology` (pure) |    —    |   SPIKE   |
|   [7]   | Naming↔hash reconciliation | `NamingHash`/`NamingHashOps` | static surface + `Encode` + `Reconcile` fold                 | `NamingHashOps.Reconcile → Fin<NamingHash>`   |    2    |   SPIKE   |

`[STATE]`: rows [1]-[4] FINALIZED (the `EntityKind` discriminant, `TrackOutcome` union, `TopoSignature` fingerprint, and `TopoName` lineage value object are pure, closed, and proof-free). Rows [5]-[7] are SPIKE — fence-complete bodies with two residuals named in RESEARCH: row [5]/`Track` and the `BoundaryOf`-replacing stored-boundary re-anchor hold until `[REANCHOR_INJECTIVITY]` pins the boundary-storage shape (now `NameEntry.Boundary` + `NameTable.VertexNames`) against the golden rebuild fixture and confirms Survive×Migrate cross-case injectivity; rows [6]/`OfMesh` and [7]/`Encode` hold until `[CANONICAL_BYTE_IDENTITY]` verifies the live-host `Mesh.TopologyVertices.IndicesFromFace` boundary-cycle winding and the cross-package `XxHash128` byte-equality against the `[G4]` golden fixture. No open shape — both residuals are verification spikes, not undecided structure.

## [5]-[RESEARCH]

- [CANONICAL_BYTE_IDENTITY] SPIKE: `NamingHashOps.Encode` emits the canonical adjacency bytes (vertex count · sorted edge endpoint pairs · lowest-vertex-rotated face cycles · little-endian) the Persistence `version-control#STRUCTURAL_DIFF` `GeometryHash` reads. The byte-identity contract is the `[G4]` FROZEN golden-bytes fixture both the geometry domain and Persistence `StructuralMerge` assert against — a tier-2 cross-package byte-equality harness feeds one reference mesh through both `NamingHashOps.Encode` and the Persistence `GeometryHash` path and asserts `XxHash128` equality, the morph case (moved control points, same adjacency → equal hash) and the topology-break case (changed adjacency → distinct hash) as the two discriminating laws; the still-unverified live-host spellings are the RhinoCommon `Mesh.TopologyVertices.IndicesFromFace(int)`, `Mesh.TopologyEdges.GetTopologyVertices(int) : IndexPair`, and `IndexPair.I`/`.J` member surface (composed from `Rasm`/Vectors, confirmed against the Vectors `Mesh.cs` usage at `TopologyEdges.GetTopologyVertices`/`GetConnectedFaces`) — confirm `IndicesFromFace` returns the boundary cycle in consistent winding before the rotation law is FINALIZED.
- [REANCHOR_INJECTIVITY] SPIKE: `TopoNaming.Track` is total over the `Fin` rail and routes `GeometryFault.NameCollision` on a non-injective re-anchor (two rebuilt entities of one kind resolving to one prior name). The tier-2 property harness drives the rebuild-matching validation: generate a mesh, apply a labelled topological operation (rigid move → all names Survive; face split → child Migrates with parent provenance; vertex insert → Born; edge collapse → the collapsed-into name Survives, the collapsed name absent), and assert the `TrackOutcome` matches the operation's expected lineage class and that the next-generation `NameTable` is injective per `EntityKind`; the static `TopoSignature.Subsumes` parent-subset predicate is the migration disambiguator under test (a face split's children each subsume the parent boundary), with a deterministic smallest-`TopoName` tiebreak so a split-into-n parent selection is stable across runs (the golden rebuild fixture would otherwise flake on `HashMap` enumeration order). The boundary-storage shape is now pinned — `NameEntry.Boundary` stores the resolved incident-name set as a column (read directly in `MigrateOrBirth`, never re-derived) and `NameTable.VertexNames` resolves intrinsic incident topology-vertex indices to prior names via `ResolveBoundary`; the OPEN residual is the injectivity proof itself, specifically the Survive×Migrate cross-case (a `Survived` name followed by a `Migrated` that mints a child of the same prior parent), which the per-`EntityKind` `claimed` set must prove non-colliding alongside the already-guarded Survive×Survive case — holding the cluster at SPIKE until the property harness asserts injectivity across both cross-cases against the golden rebuild fixture.
