# [RASM_TOPOLOGY_NAMING]

Persistent topological naming survives every rebuild behind one `Naming.Apply(NamingOp, Op? key)` entry: the closed `NamingOp` `[Union]` folds `Track` re-anchoring names across a rebuild and `Resolve` minting the generation-zero table for a first build, over one `TopoName` lineage algebra spanning every `EntityKind`. A `TopoName` is a content-address-derived `UInt128` reference identity — which entity, lineage-stable across generations — orthogonal to the content identity the reconciliation sibling bridges. Its fold accumulates every non-injective re-anchor into `Validation<Error, NameTable>` before exiting the one `Fin` rail, so one verdict carries a defective rebuild's complete collision set, each row a `GeometryFault.NameCollision` (`Numerics/faults.md`).

`Naming` reads `Rasm.Meshing` `MeshSpace` and the native `Mesh` topology as settled vocabulary — never re-minted — and consumes the `CanonicalTopology` the reconciliation sibling emits in this same `Rasm.Spatial` namespace; `TopoName`, `EntityKind`, `NamingOp`, and the lineage records are interior types that never cross a transport. Migration rides one `NamingPolicy.MigrationOverlap` fraction, and the emitted `NameTable` gates its own `IValidityEvidence` fold before every `Apply` returns.

## [01]-[INDEX]

- [02]-[TOPO_NAMING]: one `Naming.Apply(NamingOp, Op?)` entry over the `TopoName` lineage algebra; the `NameTable` generation registry of fingerprint buckets, boundary postings, and `Self`-keyed vertex rows; overlap-fraction migration under `NamingPolicy`; the accumulating injectivity audit.

## [02]-[TOPO_NAMING]

- Owner: `Naming.Apply` folds the `NamingOp` request algebra over one `TopoName` lineage; `TopoName` `[ValueObject<UInt128>]` is the single naming reference across every `EntityKind`, its modality carried in the `Kind` column of `NameEntry`. `NameTable` is the immutable per-generation registry keyed by `TopoName`, carrying the fingerprint-bucket `SignatureIndex`, the inverted `BoundaryIndex` postings, and the `Self`-keyed `VertexNames` rows, registered `IValidityEvidence`.
- Cases: `EntityKind` rows `Vertex` (arity 0), `Edge` (arity 2), and `Face` (variadic `-1`); `NamingOp` cases `Track` and `Resolve`; `TrackOutcome` cases `Survived`, `Migrated`, and `Born`, derived from the `NameEntry` lineage columns through `Outcome(generation)`.
- Entry: `Naming.Apply(NamingOp, Op? key)` is the one entry over both modalities. `Track(prior, rebuilt)` walks every rebuilt entity, resolves its `TopoSignature` against the prior signature index, and emits the table at `prior.Generation.Next()`, so monotonicity holds by construction; `Resolve(boundary)` runs the same fold against `NameTable.Empty` at generation zero with every entity `Born`, so a first build and a re-anchor are one body discriminated by the op case. Collisions accumulate into `Validation<Error, NameTable>` and exit `.ToFin()`, keeping the public rail `Fin<NameTable>`. `TopoName.Mint(kind, canonicalBytes, born)` derives the lineage-root name as a seed-zero `XxHash128` `UInt128` over kind ordinal, born generation, and canonical bytes — content-stable yet lineage-distinct across generations.
- Auto: `Naming.Apply` walks entities in `EntityKind` order so vertices anchor first and every edge or face resolves its incident vertices through the in-progress next-generation `VertexNames` — a genesis signature carries real names and generation g compares like-with-like against generation g−1. `SignatureArity` gates the boundary feed: the vertex row takes no boundary names and separates by its WL-1 star fingerprint, while edges and faces feed resolved name multisets. Exact re-anchor is a claimed-filtered `SignatureIndex` bucket lookup — indistinguishable entities share a bucket, so an exhausted bucket falls through to migration or birth as growth. A miss migrates by maximum `TopoSignature.Overlap` over `BoundaryIndex` candidates under the `NamingPolicy.MigrationOverlap` floor and a smallest-`TopoName` tiebreak, and a `Migrated` or `Born` entry mints a fresh generation-salted name that cannot collide with a prior one. `RefineVertices` stores each vertex's completed star as the next generation's migration material and gives a generation-fresh orphan star-overlap provenance. One `EntityKind` discriminant drives one fold body across vertex, edge, and face.
- Receipt: `Apply` returns the next-generation `NameTable` directly — the registry is the receipt, its birth and last-seen generations and parent provenance the per-name lineage evidence, and `NameEntry.Outcome(Generation)` derives the `Survived`/`Migrated`/`Born` classification on demand. `NameTable : IValidityEvidence` declares one `ValidityClaim.All` fold — fingerprint-bucket membership totalling the entry count is the standing injectivity witness, every vertex row resolves to a registered entry, and lineage generations stay ordered `Born ≤ LastSeen ≤ Generation` — gated before emission.
- Packages: `Rasm.Meshing` (`MeshSpace`, native `Mesh`), `Rasm.Domain` (`ContentHash.Of`, the `Op` key rail, `ValidityClaim`/`IValidityEvidence`), `Rasm.Numerics` (`GeometryFault`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Validation`/`HashMap`/`Seq`/`Option`), BCL inbox.
- Growth: a new entity modality is one `EntityKind` row with its signature-arity column and one signature-builder arm; a new lineage outcome is one `TrackOutcome` case and one `Outcome` projection arm; a new migration disambiguator is one `NamingPolicy` column the parent search reads; a new op modality is one `NamingOp` case on the same `Apply` fold.
- Boundary: `TopoName` is the one naming value object over every `EntityKind`, the modality carried in the `Kind` column. `TopoSignature` is position-free — built from incident names and kind histograms, never coordinates — so a rigid transform preserves every name and only an adjacency change re-anchors, matching the morph-versus-topology-break distinction `GeometryHash` reads from the same canonical adjacency. Migration is the `Overlap` shared-name fraction under the `NamingPolicy.MigrationOverlap` floor; `VertexNames` keys by `RebuiltEntity.Self`; boundary names resolve through the next-generation table under the vertices-first walk; exact re-anchor reads the prior `SignatureIndex` buckets while migration gathers from the `BoundaryIndex` postings through hash-index lookups. `Apply` is total over the `Fin` rail, a name collision routing `GeometryFault.NameCollision` (`Numerics/faults.md`) accumulated internally as `Validation<Error, NameTable>`. `TopoName` is a `UInt128` reference identity orthogonal to the `GeometryHash` content identity the reconciliation sibling bridges, and the `NameTable` is immutable — `Apply` returns the next generation.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Spatial;

// --- [TYPES] -----------------------------------------------------------------------------------
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NamingOp {
    private NamingOp() { }

    public sealed record Track(NameTable Prior, CanonicalTopology Rebuilt, Option<NamingPolicy> Policy = default) : NamingOp;
    public sealed record Resolve(CanonicalTopology Boundary) : NamingOp;
}

// --- [CONSTANTS] ---------------------------------------------------------------------------------
public sealed record NamingPolicy(double MigrationOverlap) {
    public static readonly NamingPolicy Canonical = new(MigrationOverlap: 0.5);
}

// --- [MODELS] ------------------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct Generation {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError("Generation must be >= 0.");

    public Generation Next() => Create(Value + 1);
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoSignature {
    public static TopoSignature Of(EntityKind kind, ReadOnlySpan<TopoName> incidentNames, ReadOnlySpan<int> kindHistogram) {
        ArrayBufferWriter<byte> buffer = new((incidentNames.Length + kindHistogram.Length + 1) * 16);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], kind.Key); buffer.Advance(4);
        TopoName[] sorted = incidentNames.ToArray(); Array.Sort(sorted, static (a, b) => a.Value.CompareTo(b.Value));
        foreach (TopoName name in sorted) { BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], name.Value); buffer.Advance(16); }
        foreach (int count in kindHistogram) { BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], count); buffer.Advance(4); }
        return Create(ContentHash.Of(buffer.WrittenSpan));
    }

    // Shared-name overlap |a ∩ b| / min(|a|,|b|) over the sorted boundary multisets: 1.0 for a subset in
    // either direction (split child ⊂ parent, parent ⊂ merged child), the policy-thresholded migration predicate.
    public static double Overlap(ReadOnlySpan<TopoName> prior, ReadOnlySpan<TopoName> rebuilt) {
        if (prior.IsEmpty || rebuilt.IsEmpty) return 0.0;
        TopoName[] a = prior.ToArray(); TopoName[] b = rebuilt.ToArray();
        Array.Sort(a, static (x, y) => x.Value.CompareTo(y.Value));
        Array.Sort(b, static (x, y) => x.Value.CompareTo(y.Value));
        int shared = 0, i = 0, j = 0;
        while (i < a.Length && j < b.Length) {
            int order = a[i].Value.CompareTo(b[j].Value);
            if (order == 0) { shared++; i++; j++; }
            else if (order < 0) i++;
            else j++;
        }
        return (double)shared / Math.Min(a.Length, b.Length);
    }
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoName {
    // born folds into the digest: a fresh mint can never collide with a prior-generation name.
    public static TopoName Mint(EntityKind kind, ReadOnlySpan<byte> canonicalBytes, Generation born) {
        ArrayBufferWriter<byte> buffer = new(canonicalBytes.Length + 8);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], kind.Key); buffer.Advance(4);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4)[..4], born.Value); buffer.Advance(4);
        buffer.Write(canonicalBytes);
        return Create(ContentHash.Of(buffer.WrittenSpan));
    }
}

public readonly record struct NameEntry(
    TopoName Name, EntityKind Kind, Generation Born, Generation LastSeen, Option<TopoName> Parent, TopoSignature Signature, Seq<TopoName> Boundary, byte[] CanonicalBytes) {
    // Outcome derives the lineage classification from the columns, never a parallel carrier.
    public TrackOutcome Outcome(Generation generation) =>
        Born.Value < generation.Value
            ? new TrackOutcome.Survived(Name)
            : Parent.Match(
                Some: parent => (TrackOutcome)new TrackOutcome.Migrated(Name, parent),
                None: () => new TrackOutcome.Born(Name));
}

public readonly record struct RebuiltEntity(EntityKind Kind, int Self, byte[] CanonicalBytes, int[] IncidentVertices, int[] KindHistogram);

public sealed record NameTable(
    HashMap<TopoName, NameEntry> Entries,
    HashMap<EntityKind, HashMap<TopoSignature, Seq<TopoName>>> SignatureIndex,
    HashMap<TopoName, Seq<TopoName>> BoundaryIndex,
    HashMap<int, TopoName> VertexNames,
    Generation Generation) : IValidityEvidence {
    public static readonly NameTable Empty = new(
        HashMap<TopoName, NameEntry>.Empty, HashMap<EntityKind, HashMap<TopoSignature, Seq<TopoName>>>.Empty,
        HashMap<TopoName, Seq<TopoName>>.Empty, HashMap<int, TopoName>.Empty, Generation.Create(0));

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Generation.Value >= 0),
        ValidityClaim.CountExactly(
            count: SignatureIndex.Values.Fold(0, static (n, perKind) => perKind.Values.Fold(n, static (m, bucket) => m + bucket.Count)),
            expected: Entries.Count),
        ValidityClaim.Of(Entries.Values.ForAll(entry => entry.Born.Value <= entry.LastSeen.Value && entry.LastSeen.Value <= Generation.Value)),
        ValidityClaim.Of(VertexNames.Values.ForAll(Entries.ContainsKey)));

    // A fingerprint bucket, smallest name first: topologically indistinguishable entities share a fingerprint
    // legitimately (a symmetric mesh), so exact-match resolution is claimed-filtered bucket selection.
    public Seq<TopoName> Resolve(EntityKind kind, TopoSignature signature) =>
        SignatureIndex.Find(kind).Bind(index => index.Find(signature)).IfNone(Seq<TopoName>());

    public Seq<TopoName> ResolveBoundary(ReadOnlySpan<int> incidentVertices) =>
        toSeq(incidentVertices.ToArray()).Map(VertexNames.Find).Somes();

    // VertexNames keys by the entity's OWN Self index; bucket and posting inserts are idempotent per name, so the vertex refine re-With never double-registers.
    public NameTable With(NameEntry entry, int self) {
        HashMap<TopoSignature, Seq<TopoName>> perKind = SignatureIndex.Find(entry.Kind).IfNone(HashMap<TopoSignature, Seq<TopoName>>.Empty);
        Seq<TopoName> bucket = perKind.Find(entry.Signature).IfNone(Seq<TopoName>());
        HashMap<TopoSignature, Seq<TopoName>> index = perKind.AddOrUpdate(entry.Signature,
            bucket.Contains(entry.Name) ? bucket : toSeq(bucket.Add(entry.Name).OrderBy(static n => n.Value)));
        HashMap<TopoName, Seq<TopoName>> postings = toSeq(entry.Boundary.Distinct()).Fold(BoundaryIndex, (posted, name) => posted.AddOrUpdate(
            name, owners => owners.Contains(entry.Name) ? owners : owners.Add(entry.Name), Seq(entry.Name)));
        HashMap<int, TopoName> vertices = entry.Kind == EntityKind.Vertex ? VertexNames.AddOrUpdate(self, entry.Name) : VertexNames;
        return this with {
            Entries = Entries.AddOrUpdate(entry.Name, entry), SignatureIndex = SignatureIndex.AddOrUpdate(entry.Kind, index),
            BoundaryIndex = postings, VertexNames = vertices,
        };
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------------
public static class Naming {
    public static Fin<NameTable> Apply(NamingOp op, Op? key = null) {
        Op minted = key.OrDefault();
        return op.Switch(
            state: minted,
            track: static (k, t) => Anchor(t.Prior, t.Rebuilt, t.Prior.Generation.Next(), t.Policy.IfNone(NamingPolicy.Canonical), k),
            resolve: static (k, r) => Anchor(NameTable.Empty, r.Boundary, Generation.Create(0), NamingPolicy.Canonical, k));
    }

    // Every collision joins one Validation failure set; the fold never aborts early, so one verdict reports the complete non-injective re-anchor set.
    static Fin<NameTable> Anchor(NameTable prior, CanonicalTopology rebuilt, Generation next, NamingPolicy policy, Op key) {
        HashMap<int, int[]> stars = toHashMap(rebuilt.Entities
            .Filter(static e => e.Kind == EntityKind.Vertex).Map(static e => (e.Self, e.KindHistogram)));
        (NameTable Table, Set<TopoName> Claimed, Seq<Error> Collisions) folded = toSeq(rebuilt.Entities.OrderBy(static e => e.Kind.Key))
            .Fold((Table: NameTable.Empty with { Generation = next }, Claimed: Set<TopoName>.Empty, Collisions: Seq<Error>()),
                (state, entity) => Step(prior, entity, stars, next, policy, state));
        return folded.Collisions.IsEmpty
            ? Fin.Succ(RefineVertices(prior, folded.Table, rebuilt, policy, next))
                .Bind(refined => guard(refined.IsValid, key.InvalidResult()).ToFin().Map(_ => refined))
            : Validation.Fail<Error, NameTable>(folded.Collisions).ToFin();
    }

    // Boundary names resolve through the in-progress next-generation table — vertices walk first, so an edge/face reads the names its endpoints
    // just anchored, gen-g comparing like-with-like against gen-(g-1); the arity-0 vertex takes no boundary, its star feeding the fingerprint.
    static (NameTable Table, Set<TopoName> Claimed, Seq<Error> Collisions) Step(
        NameTable prior, RebuiltEntity entity, HashMap<int, int[]> stars, Generation next, NamingPolicy policy,
        (NameTable Table, Set<TopoName> Claimed, Seq<Error> Collisions) state) {
        Seq<TopoName> boundary = entity.Kind.SignatureArity == 0
            ? Seq<TopoName>()
            : state.Table.ResolveBoundary(entity.IncidentVertices);
        TopoSignature signature = TopoSignature.Of(entity.Kind, boundary.ToArray(), Fingerprint(entity, stars));
        Fin<NameEntry> bound = prior.Resolve(entity.Kind, signature)
            .Filter(name => !state.Claimed.Contains(name))
            .Head
            .Match(
                Some: name => Survive(prior, name, signature, boundary, next, entity),
                // An exhausted bucket is growth, never a collision: the extra same-fingerprint entity migrates or is born fresh.
                None: () => MigrateOrBirth(prior, entity, signature, boundary, next, policy))
            .Bind(entry => state.Claimed.Contains(entry.Name)
                ? Fin.Fail<NameEntry>(new GeometryFault.NameCollision(entry.Name.Value, entity.Kind.Key).ToError())
                : Fin.Succ(entry));
        return bound.Match(
            Succ: entry => (state.Table.With(entry, entity.Self), state.Claimed.Add(entry.Name), state.Collisions),
            Fail: error => (state.Table, state.Claimed, state.Collisions.Add(error)));
    }

    // Lexicographic full-array order: a new EntityKind row widens the histogram and the sort absorbs it.
    static readonly IComparer<int[]> HistogramOrder =
        Comparer<int[]>.Create(static (a, b) => a.AsSpan().SequenceCompareTo(b));

    // WL-1 refinement: the vertex fingerprint appends the sorted star-neighbor kind histograms, so same-valence
    // vertices with different neighborhoods separate without names or coordinates, identically at both generations.
    static int[] Fingerprint(RebuiltEntity entity, HashMap<int, int[]> stars) =>
        entity.Kind.SignatureArity != 0 ? entity.KindHistogram : [
            .. entity.KindHistogram,
            .. toSeq(entity.IncidentVertices).Map(v => stars.Find(v)).Somes()
                .OrderBy(static h => h, HistogramOrder)
                .SelectMany(static h => h)];

    static Fin<NameEntry> Survive(NameTable prior, TopoName name, TopoSignature signature, Seq<TopoName> boundary, Generation next, RebuiltEntity entity) =>
        prior.Entries.Find(name).Match(
            Some: prev => Fin.Succ(prev with { LastSeen = next, Signature = signature, Boundary = boundary, CanonicalBytes = entity.CanonicalBytes }),
            // A signature-index hit whose registry entry is absent is a corrupt prior table.
            None: () => Fin.Fail<NameEntry>(new GeometryFault.NameCollision(name.Value, entity.Kind.Key).ToError()));

    // A claimed survivor stays eligible as a migration parent — provenance is lineage, not a claim; the child's fresh born-generation mint carries injectivity.
    static Fin<NameEntry> MigrateOrBirth(NameTable prior, RebuiltEntity entity, TopoSignature signature, Seq<TopoName> boundary, Generation next, NamingPolicy policy) {
        Option<TopoName> parent = OverlapParent(prior, entity.Kind, boundary, policy);
        TopoName name = TopoName.Mint(entity.Kind, entity.CanonicalBytes, next);
        return Fin.Succ(new NameEntry(name, entity.Kind, next, next, parent, signature, boundary, entity.CanonicalBytes));
    }

    // Posting-list candidate gather: only a prior entity sharing a boundary name can clear a >0 overlap floor.
    static Option<TopoName> OverlapParent(NameTable prior, EntityKind kind, Seq<TopoName> boundary, NamingPolicy policy) {
        TopoName[] rebuilt = boundary.ToArray();
        return toSeq(
                toSeq(toSeq(boundary.Distinct())
                    .Map(name => prior.BoundaryIndex.Find(name).IfNone(Seq<TopoName>()))
                    .Fold(Seq<TopoName>(), static (all, owners) => all + owners).Distinct())
                .Map(prior.Entries.Find).Somes()
                .Filter(prev => prev.Kind == kind)
                .Map(prev => (Entry: prev, Score: TopoSignature.Overlap(prev.Boundary.ToArray(), rebuilt)))
                .Filter(candidate => candidate.Score >= policy.MigrationOverlap)
                .OrderBy(static candidate => (-candidate.Score, candidate.Entry.Name.Value)))
            .Head.Map(static candidate => candidate.Entry.Name);
    }

    // Vertex refine post-pass: with every vertex anchored, each vertex's star resolves through the completed VertexNames and becomes the stored
    // Boundary (the next generation's migration material); a generation-fresh orphan gains star-overlap Parent provenance. Names never change here.
    static NameTable RefineVertices(NameTable prior, NameTable table, CanonicalTopology rebuilt, NamingPolicy policy, Generation next) =>
        rebuilt.Entities.Filter(static e => e.Kind == EntityKind.Vertex)
            .Fold(table, (acc, entity) => acc.VertexNames.Find(entity.Self).Bind(acc.Entries.Find)
                .Map(entry => (Entry: entry, Star: acc.ResolveBoundary(entity.IncidentVertices)))
                .Match(
                    Some: bound => acc.With(bound.Entry with {
                        Boundary = bound.Star,
                        Parent = bound.Entry.Born == next && bound.Entry.Parent.IsNone
                            ? OverlapParent(prior, entity.Kind, bound.Star, policy)
                            : bound.Entry.Parent,
                    }, entity.Self),
                    None: () => acc));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
