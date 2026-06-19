# [RASM_TOPOLOGY_NAMING]

Persistent topological naming that survives rebuilds: one `TopoName` lineage reference over every entity modality, the `NameTable` registry carrying the generation/lineage record, and the `Track` re-anchoring fold that re-binds names across a rebuild by topological signature. The name is a content-address-derived `UInt128` but it is a REFERENCE identity (which entity, lineage-stable across generations), orthogonal to the content identity the reconciliation page bridges to the Persistence `GeometryHash`. The page composes `Vectors` `MeshSpace` and the native `Mesh` topology surface as settled vocabulary — read, never re-mint — and consumes the `CanonicalTopology` the reconciliation sibling emits.

`TopoName` is a reference identity that never crosses a transport; `EntityKind`, the `TopoName` value object, and the lineage records are interior types that never sit between wire and rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                                                               |
| :-----: | :---------- | :--------------------------------------------------------------------------------------------------- |
|   [1]   | TOPO_NAMING | One `TopoName` lineage algebra; `NameTable` generation registry; `Track` re-anchor-by-signature fold |

## [2]-[TOPO_NAMING]

- Owner: `EntityKind` `[SmartEnum<int>]` the entity-modality discriminant (`Vertex`/`Edge`/`Face`) carrying the per-kind signature-arity column; `TopoSignature` the rebuild-invariant topological fingerprint a name re-anchors against (boundary-vertex multiset + incident-kind histogram, position-free so a rigid move keeps the signature); `TopoName` `[ValueObject<UInt128>]` the stable lineage reference — one naming algebra over every `EntityKind`, the modality lives in the `Kind` column of the `NameEntry` row, never a `VertexName`/`EdgeName`/`FaceName` parallel triple; `Generation` the monotone rebuild counter; `NameEntry` the lineage record (name + kind + birth generation + last-seen generation + provenance parent name + current signature + stored resolved boundary-name set + canonical bytes); `NameTable` the immutable registry keyed by `TopoName` with the signature index AND a topology-vertex-index→name row (`VertexNames`) that resolves intrinsic incidence to prior names; `Track` the re-anchoring fold that matches names across a rebuild.
- Cases: `EntityKind` rows `Vertex` (arity 0, signature is its own position bucket) · `Edge` (arity 2, signature is its endpoint-name multiset) · `Face` (`SignatureArity` `-1` sentinel for the variadic boundary cycle) (3); `Track` outcomes per rebuilt entity are `Survived` (signature match → keep name, bump last-seen) · `Migrated` (parent-signature subset match → new name, parent provenance) · `Born` (no match → fresh name) folded into the next-generation `NameTable`.
- Entry: `public static Fin<NameTable> Track(NameTable prior, CanonicalTopology rebuilt, Generation next)` — `Fin<T>` routes a `GeometryFault.NameCollision` (band 2400) when two rebuilt entities of one kind resolve to the same prior name (a non-injective re-anchor is a defect, never a silent overwrite); the fold walks every rebuilt entity, resolves its `TopoSignature` against the prior signature index, and emits the next-generation table where each entity carries either its surviving prior name, a migrated child name with parent provenance, or a fresh name minted from the entity's canonical bytes. `public static TopoName Mint(EntityKind kind, ReadOnlySpan<byte> canonicalBytes, Generation born)` mints the lineage-root name as `XxHash128`-derived `UInt128` over `(kind ordinal · canonical entity bytes · born generation)` so a fresh name is content-stable but lineage-distinct across generations.
- Auto: `Track` folds entities in `EntityKind` order (vertices first so `VertexNames` is populated before edges/faces resolve incidence) and reads the prior `NameTable.SignatureIndex` (`HashMap<EntityKind, HashMap<TopoSignature, TopoName>>`) so the exact-match probe is one hash-map lookup per rebuilt entity, never a quadratic cross-product; each entity's intrinsic incident topology-vertex indices resolve to prior `TopoName`s through `NameTable.ResolveBoundary` (the `VertexNames` row), and that resolved set IS the position-free signature input. An exact `TopoSignature` hit is `Survived` (the entity kept its topological neighborhood), a parent-boundary subset hit (the prior entry's stored `Boundary` names are a proper subset of the rebuilt entity's) is `Migrated` with the prior name threaded as `Parent` provenance under a deterministic smallest-`TopoName` tiebreak (a face split inherits the parent face's lineage; the tiebreak makes a split-into-n choose one stable parent), and a miss is `Born`. The single `EntityKind` discriminant means one fold body serves vertex, edge, and face re-anchoring — the signature arity is a row column, not three fold copies.
- Receipt: `Track` returns the next-generation `NameTable` directly (the registry IS the receipt — birth/last-seen generations and parent provenance are the per-name lineage evidence); no parallel tracking ledger.
- Packages: `Rasm`/Vectors (`MeshSpace`, native `Mesh` topology — composed), Thinktecture.Runtime.Extensions, LanguageExt.Core, `System.IO.Hashing` (`XxHash128`), BCL inbox.
- Growth: a new entity modality is one `EntityKind` row with its signature-arity column and one signature-builder arm; a new lineage outcome (e.g. `Merged` for a face-merge re-bind) is one `TrackOutcome` case on the existing fold; zero new surface — never a fourth `*Name` value object.
- Boundary: `TopoName` is the ONE naming value object over all entity kinds and a `VertexName`/`EdgeName`/`FaceName` triple is the deleted form — the modality is the `Kind` column; `TopoSignature` is position-free (built from incident NAMES and kind histograms, never coordinates) so a rigid transform preserves every name and only an adjacency change re-anchors, which is exactly the `GeometryHash` morph-vs-topology-break distinction read from the same canonical adjacency; the re-anchor reads the prior `SignatureIndex` and a per-rebuild O(n·m) signature cross-scan is the named defect; `Track` is total over the `Fin` rail and a thrown collision is forbidden — a non-injective name resolution routes `GeometryFault.NameCollision`, where the sibling-owned `GeometryFault` (`faults/faults.md`, band 2400) lowers into the `Error` rail through `ToError()` so `GeometryFault.NameCollision(...).ToError()` is the `Fin<T>` failure channel and no separate error type sits in the rail; the name is a content-address-derived `UInt128` but it is a REFERENCE identity (which entity, lineage-stable across generations), orthogonal to the `GeometryHash` CONTENT identity (what shape, change-sensitive) — minting a second content hash here is the deleted form, and `TopoName.Value` is NEVER equality-tested against a `NameAddress.ContentHash` even though both are raw `UInt128` (the reference and content axes never compare cross-axis); the `Migrated` parent is resolved by the stored `NameEntry.Boundary` column (never re-derived) with a deterministic smallest-`TopoName` tiebreak so a face split into n children selects one stable lineage parent; the `NameTable` is immutable and `Track` returns the next generation, never an in-place mutation of the prior table.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class EntityKind {
    public static readonly EntityKind Vertex = new(key: 0, signatureArity: 0);
    public static readonly EntityKind Edge   = new(key: 1, signatureArity: 2);
    public static readonly EntityKind Face   = new(key: 2, signatureArity: -1);

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

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoSignature {
    public static TopoSignature Of(EntityKind kind, ReadOnlySpan<TopoName> incidentNames, ReadOnlySpan<int> kindHistogram) {
        var buffer = new ArrayBufferWriter<byte>((incidentNames.Length + kindHistogram.Length + 1) * 16);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], kind.Key); buffer.Advance(4);
        var sorted = incidentNames.ToArray(); Array.Sort(sorted, static (a, b) => a.Value.CompareTo(b.Value));
        foreach (var name in sorted) { BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], name.Value); buffer.Advance(16); }
        foreach (int count in kindHistogram) { BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], count); buffer.Advance(4); }
        return From(XxHash128.HashToUInt128(buffer.WrittenSpan));
    }

    public static bool Subsumes(ReadOnlySpan<TopoName> priorBoundary, ReadOnlySpan<TopoName> rebuiltBoundary) {
        var rebuilt = rebuiltBoundary.ToArray(); Array.Sort(rebuilt, static (a, b) => a.Value.CompareTo(b.Value));
        foreach (var name in priorBoundary)
            if (Array.BinarySearch(rebuilt, name, Comparer<TopoName>.Create(static (a, b) => a.Value.CompareTo(b.Value))) < 0) return false;
        return priorBoundary.Length > 0 && priorBoundary.Length < rebuiltBoundary.Length;
    }
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoName {
    public static TopoName Mint(EntityKind kind, ReadOnlySpan<byte> canonicalBytes, Generation born) {
        var buffer = new ArrayBufferWriter<byte>(canonicalBytes.Length + 8);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], kind.Key); buffer.Advance(4);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], born.Value); buffer.Advance(4);
        buffer.Write(canonicalBytes);
        return From(XxHash128.HashToUInt128(buffer.WrittenSpan));
    }
}

public readonly record struct NameEntry(
    TopoName Name, EntityKind Kind, Generation Born, Generation LastSeen, Option<TopoName> Parent, TopoSignature Signature, Seq<TopoName> Boundary, byte[] CanonicalBytes);

public readonly record struct RebuiltEntity(EntityKind Kind, byte[] CanonicalBytes, int[] IncidentVertices, int[] KindHistogram);

public sealed record NameTable(
    HashMap<TopoName, NameEntry> Entries,
    HashMap<EntityKind, HashMap<TopoSignature, TopoName>> SignatureIndex,
    HashMap<int, TopoName> VertexNames,
    Generation Generation) {
    public static readonly NameTable Empty =
        new(HashMap<TopoName, NameEntry>.Empty, HashMap<EntityKind, HashMap<TopoSignature, TopoName>>.Empty, HashMap<int, TopoName>.Empty, Generation.From(0));

    public Option<TopoName> Resolve(EntityKind kind, TopoSignature signature) =>
        SignatureIndex.Find(kind).Bind(index => index.Find(signature));

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
    public static Fin<NameTable> Track(NameTable prior, CanonicalTopology rebuilt, Generation next) =>
        rebuilt.Entities.OrderBy(static e => e.Kind.Key)
            .Fold(Fin.Succ((Table: NameTable.Empty with { Generation = next }, Claimed: HashSet<TopoName>.Empty)),
                (acc, entity) => acc.Bind(state => Anchor(prior, entity, next, state.Claimed).Map(bound =>
                    (Table: state.Table.With(bound.Entry, entity.IncidentVertices[0]), Claimed: state.Claimed.Add(bound.Entry.Name)))))
            .Map(static state => state.Table);

    static Fin<(NameEntry Entry, TrackOutcome Outcome)> Anchor(
        NameTable prior, RebuiltEntity entity, Generation next, HashSet<TopoName> claimed) {
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
            .OrderBy(static prev => prev.Name.Value)
            .HeadOrNone();
        var name = TopoName.Mint(entity.Kind, entity.CanonicalBytes, next);
        var entry = new NameEntry(name, entity.Kind, next, next, parent.Map(static p => p.Name), signature, boundary, entity.CanonicalBytes);
        return Fin.Succ((entry, parent.Match(
            Some: p => (TrackOutcome)new TrackOutcome.Migrated(name, p.Name),
            None: () => new TrackOutcome.Born(name))));
    }
}
```

## [3]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes.

| [INDEX] | [AXIS/CONCERN]              | [OWNER]                  | [KIND]                                                                        | [RAIL]                                    | [CASES] |
| :-----: | :-------------------------- | :----------------------- | :---------------------------------------------------------------------------- | :---------------------------------------- | :-----: |
|   [1]   | Entity modality             | `EntityKind`             | `[SmartEnum<int>]` Vertex/Edge/Face + signature-arity column                  | discriminant (pure)                       |    3    |
|   [2]   | Re-anchor outcome           | `TrackOutcome`           | `[Union]` Survived/Migrated/Born                                              | carrier (returned in `Track` rail)        |    3    |
|   [3]   | Topological fingerprint     | `TopoSignature`          | `[ValueObject<UInt128>]` position-free incident-name/kind digest + `Subsumes` | `TopoSignature.Of → TopoSignature` (pure) |    —    |
|   [4]   | Stable lineage reference    | `TopoName`               | `[ValueObject<UInt128>]` one naming algebra over all kinds + `Mint`           | `TopoName.Mint → TopoName` (pure)         |    —    |
|   [5]   | Naming registry + re-anchor | `NameTable`/`TopoNaming` | immutable registry + signature index + `VertexNames` row + `Track` fold       | `TopoNaming.Track → Fin<NameTable>`       |    3    |

## [4]-[RESEARCH]

- [REANCHOR_INJECTIVITY] — `TopoNaming.Track` is total over the `Fin` rail and routes `GeometryFault.NameCollision` on a non-injective re-anchor (two rebuilt entities of one kind resolving to one prior name). The tier-2 property harness drives the rebuild-matching validation: generate a mesh, apply a labelled topological operation (rigid move → all names Survive; face split → child Migrates with parent provenance; vertex insert → Born; edge collapse → the collapsed-into name Survives, the collapsed name absent), and assert the `TrackOutcome` matches the operation's expected lineage class and that the next-generation `NameTable` is injective per `EntityKind`; the static `TopoSignature.Subsumes` parent-subset predicate is the migration disambiguator under test (a face split's children each subsume the parent boundary), with a deterministic smallest-`TopoName` tiebreak so a split-into-n parent selection is stable across runs (an enumeration-order-dependent tiebreak flakes the golden rebuild fixture on `HashMap` order — the smallest-name rule removes that dependence). The boundary-storage shape is pinned — `NameEntry.Boundary` stores the resolved incident-name set as a column (read directly in `MigrateOrBirth`, never re-derived) and `NameTable.VertexNames` resolves intrinsic incident topology-vertex indices to prior names via `ResolveBoundary`; the OPEN residual is the injectivity proof itself, specifically the Survive×Migrate cross-case (a `Survived` name followed by a `Migrated` that mints a child of the same prior parent), which the per-`EntityKind` `claimed` set must prove non-colliding alongside the already-guarded Survive×Survive case — held until the property harness asserts injectivity across both cross-cases against the golden rebuild fixture.
