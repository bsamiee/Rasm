# [RASM_TOPOLOGY_NAMING]

Persistent topological naming survives every rebuild behind one `Naming.Apply(NamingOp, Op? key)` entry: the closed `NamingOp` `[Union]` folds `Track` re-anchoring names across a rebuild and `Resolve` minting the generation-zero table for a first build, over one `TopoName` lineage algebra spanning every `EntityKind`. `TopoName` is a content-address-derived `UInt128` reference identity — which entity, lineage-stable across generations — orthogonal to the content identity the reconciliation sibling bridges. Its fold accumulates every non-injective re-anchor into `Validation<Error, NameTable>` before exiting the one `Fin` result, so one verdict carries a defective rebuild's complete collision set, each row a `GeometryFault.NameCollision` (`Numerics/faults.md`).

`Naming` reads `Rasm.Meshing` `MeshSpace` and the native `Mesh` topology as settled vocabulary — never re-minted — and consumes the `CanonicalTopology` the reconciliation sibling emits in this same `Rasm.Spatial` namespace; `TopoName`, `EntityKind`, `NamingOp`, and the lineage records are interior types that never cross a transport. Migration rides one `NamingPolicy.MigrationOverlap` fraction, and the emitted `NameTable` gates its own `IValidityEvidence` fold before every `Apply` returns.

## [01]-[INDEX]

- [02]-[TOPO_NAMING]: one `Naming.Apply(NamingOp, Op?)` entry over the `TopoName` lineage algebra; the `NameTable` generation registry of fingerprint buckets, boundary postings, and `Self`-keyed vertex rows; overlap-fraction migration under `NamingPolicy`; the accumulating injectivity audit.

## [02]-[TOPO_NAMING]

- Owner: `Naming.Apply` folds the `NamingOp` request algebra over one `TopoName` lineage; `TopoName` `[ValueObject<UInt128>]` is the single naming reference across every `EntityKind`, its modality carried in the `Kind` column of `NameEntry`. `NameTable` is the immutable per-generation registry keyed by `TopoName`, carrying the fingerprint-bucket `SignatureIndex`, the inverted `BoundaryIndex` postings, and the `Self`-keyed `VertexNames` rows, registered `IValidityEvidence`.
- Cases: `EntityKind` rows `Vertex`, `Edge`, and `Face`, each carrying its own `BoundaryOf` feed and `FingerprintOf` separator as delegate columns; `NamingOp` cases `Track` and `Resolve`; `TrackOutcome` cases `Survived`, `Migrated`, and `Born`, derived from the `NameEntry` lineage columns through `Outcome(generation)`.
- Entry: `Naming.Apply(NamingOp, Op? key)` is the one entry over both modalities. `Track(prior, rebuilt)` walks every rebuilt entity, resolves its `TopoSignature` against the prior signature index, and emits the table at `prior.Generation.Next(key)`, so monotonicity holds by construction and a computed successor routes its refusal on the same result rather than throwing; `Resolve(boundary)` runs the same fold against `NameTable.Empty` at generation zero with every entity `Born`, so a first build and a re-anchor are one body discriminated by the op case. Collisions accumulate into `Validation<Error, NameTable>` and exit `.ToFin()`, keeping the public type `Fin<NameTable>`. `TopoName.Mint(kind, canonical, born)` derives the lineage-root name as a seed-zero `UInt128` over kind ordinal, born generation, and the entity's canonical word run — content-stable yet lineage-distinct across generations. Every preimage emits through the kernel `CanonicalWriter`: `Mint` and `TopoSignature.Of` both take the count frames `Rows`/`Sorted` write, a digest change no wire sees, because `TopoName` and `TopoSignature` are interior types no transport carries, and the frames close the collision an unframed word run, name list, and histogram spell together.
- Auto: `Naming.Apply` walks entities in `EntityKind` order so vertices anchor first and every edge or face resolves its incident vertices through the in-progress `VertexNames` of the generation being built — a genesis signature carries real names and generation g compares like-with-like against generation g−1. The `EntityKind` row owns the boundary feed: the vertex row takes no boundary names and separates by its WL-1 star fingerprint, while edges and faces feed resolved name multisets. Exact re-anchor is a claimed-filtered `SignatureIndex` bucket lookup over an ordered `Set<TopoName>` — indistinguishable entities share a bucket, so an exhausted bucket falls through to migration or birth as growth. Misses migrate by maximum `TopoSignature.Overlap` over `BoundaryIndex` candidates under the `NamingPolicy.MigrationOverlap` floor and a smallest-`TopoName` tiebreak, taken as one linear argmax fold. A `Migrated` or `Born` entry mints a fresh generation-salted name that cannot collide with a prior one. `RefineVertices` stores each vertex's completed star as the following generation's migration material and gives a generation-fresh orphan star-overlap provenance. One `EntityKind` discriminant drives one fold body across vertex, edge, and face.
- Output: `Apply` returns the `NameTable` of the generation it builds directly — the registry IS the result, its birth and last-seen generations and parent provenance the per-name lineage evidence, and `NameEntry.Outcome(Generation)` derives the `Survived`/`Migrated`/`Born` classification on demand. `NameTable : IValidityEvidence` declares one `ValidityClaim.All` fold — fingerprint-bucket membership totalling the entry count is the standing injectivity witness, every vertex row resolves to a registered entry, and lineage generations stay ordered `Born ≤ LastSeen ≤ Generation` — gated before emission. Non-negativity is NOT among them: `Generation`'s own factory validation refuses a negative, so a claim re-asserting it never fails.
- Packages: `Rasm.Meshing` (`MeshSpace`, native `Mesh`), `Rasm.Domain` (`ContentHash.Of`, the `Op` key type, `ValidityClaim`/`IValidityEvidence`), `Rasm.Numerics` (`GeometryFault`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Validation`/`HashMap`/`Seq`/`Option`), BCL inbox.
- Growth: a new entity modality is one `EntityKind` row stating its boundary feed and fingerprint; a new lineage outcome is one `TrackOutcome` case and one `Outcome` projection arm; a new migration disambiguator is one `NamingPolicy` column the parent search reads; a new op modality is one `NamingOp` case on the same `Apply` fold.
- Boundary: `TopoName` is the one naming value object over every `EntityKind`, the modality carried in the `Kind` column. `TopoSignature` is position-free — built from incident names and kind histograms, never coordinates — so a rigid transform preserves every name and only an adjacency change re-anchors, matching the morph-versus-topology-break distinction `GeometryHash` reads from the same canonical adjacency. Migration is the `Overlap` shared-name fraction under the `NamingPolicy.MigrationOverlap` floor; `VertexNames` keys by `RebuiltEntity.Self`; boundary names resolve through the table under construction on the vertices-first walk; exact re-anchor reads the prior `SignatureIndex` buckets while migration gathers from the `BoundaryIndex` postings through hash-index lookups. `Apply` is total over `Fin`, a name collision routing `GeometryFault.NameCollision` (`Numerics/faults.md`) accumulated internally as `Validation<Error, NameTable>`. `TopoName` is a `UInt128` reference identity orthogonal to the `GeometryHash` content identity the reconciliation sibling bridges, and the `NameTable` is immutable — `Apply` returns the next generation.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Spatial;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class EntityKind {
    public static readonly EntityKind Vertex = new(key: 0,
        boundaryOf: static (_, _) => Seq<TopoName>(),
        fingerprintOf: static (entity, stars) => Star(entity: entity, stars: stars));
    public static readonly EntityKind Edge = new(key: 1,
        boundaryOf: static (table, entity) => table.ResolveBoundary(entity.IncidentVertices),
        fingerprintOf: static (entity, _) => toSeq(entity.KindHistogram.AsIterable()));
    public static readonly EntityKind Face = new(key: 2,
        boundaryOf: static (table, entity) => table.ResolveBoundary(entity.IncidentVertices),
        fingerprintOf: static (entity, _) => toSeq(entity.KindHistogram.AsIterable()));

    [UseDelegateFromConstructor] internal partial Seq<TopoName> BoundaryOf(NameTable table, RebuiltEntity entity);
    [UseDelegateFromConstructor] internal partial Seq<int> FingerprintOf(RebuiltEntity entity, HashMap<int, Arr<int>> stars);

    private static Seq<int> Star(RebuiltEntity entity, HashMap<int, Arr<int>> stars) =>
        toSeq(entity.KindHistogram.AsIterable())
        + toSeq(toSeq(entity.IncidentVertices.AsIterable()).Map(stars.Find).Somes()
            .OrderBy(static histogram => histogram, HistogramOrder)
            .SelectMany(static histogram => histogram.AsIterable()));

    private static readonly IComparer<Arr<int>> HistogramOrder = Comparer<Arr<int>>.Create(
        static (a, b) => toSeq(a.AsIterable()).AsSpan().SequenceCompareTo(toSeq(b.AsIterable()).AsSpan()));
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

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct Generation {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError("Generation must be >= 0.");

    public Fin<Generation> Next(Op key) => key.AcceptValidated<Generation>(candidate: Value + 1);
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoSignature {
    public static TopoSignature Of(EntityKind kind, Seq<TopoName> incidentNames, Seq<int> kindHistogram) =>
        Create(ContentHash.Of(
            state: (Kind: kind, Names: incidentNames, Histogram: kindHistogram),
            chunks: static (row, sink) => sink
                .Ordinal(value: row.Kind.Key)
                .Sorted(rows: row.Names, key: static name => name.Value, order: Comparer<UInt128>.Default,
                    field: static (name, field) => field.U128(value: name.Value))
                .Rows(rows: row.Histogram, field: static (count, field) => field.Ordinal(value: count))));

    public static double Overlap(Seq<TopoName> prior, Seq<TopoName> rebuilt) {
        if (prior.IsEmpty || rebuilt.IsEmpty) return 0.0;
        HashMap<TopoName, int> tally = prior.Fold(HashMap<TopoName, int>.Empty,
            static (held, name) => held.AddOrUpdate(name, static count => count + 1, 1));
        int shared = rebuilt.Fold((Tally: tally, Hits: 0), static (state, name) =>
            state.Tally.Find(name).Filter(static count => count > 0).Match(
                Some: count => (state.Tally.SetItem(name, count - 1), state.Hits + 1),
                None: () => state)).Hits;
        return (double)shared / Math.Min(prior.Count, rebuilt.Count);
    }
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoName {
    public static TopoName Mint(EntityKind kind, Arr<int> canonical, Generation born) =>
        Create(ContentHash.Of(
            state: (Kind: kind, Born: born, Canonical: canonical),
            chunks: static (row, sink) => sink
                .Ordinal(value: row.Kind.Key)
                .Ordinal(value: row.Born.Value)
                .Rows(rows: toSeq(row.Canonical.AsIterable()), field: static (word, field) => field.Ordinal(value: word))));
}

public sealed record NamingPolicy(UnitInterval MigrationOverlap) {
    public static readonly NamingPolicy Canonical = new(MigrationOverlap: UnitInterval.Create(value: 0.5));
    public static Fin<NamingPolicy> Of(double migrationOverlap, Op? key = null) =>
        key.OrDefault().AcceptValidated<UnitInterval>(candidate: migrationOverlap)
            .Map(static fraction => new NamingPolicy(MigrationOverlap: fraction));
}

public readonly record struct NameEntry(
    TopoName Name, EntityKind Kind, Generation Born, Generation LastSeen, Option<TopoName> Parent, TopoSignature Signature, Seq<TopoName> Boundary, Arr<int> Canonical) {
    public TrackOutcome Outcome(Generation generation) =>
        Born.Value < generation.Value
            ? new TrackOutcome.Survived(Name)
            : Parent.Match(
                Some: parent => (TrackOutcome)new TrackOutcome.Migrated(Name, parent),
                None: () => new TrackOutcome.Born(Name));
}

public readonly record struct RebuiltEntity(EntityKind Kind, int Self, Arr<int> Canonical, Arr<int> IncidentVertices, Arr<int> KindHistogram);

public sealed record NameTable(
    HashMap<TopoName, NameEntry> Entries,
    HashMap<EntityKind, HashMap<TopoSignature, Set<TopoName>>> SignatureIndex,
    HashMap<TopoName, Seq<TopoName>> BoundaryIndex,
    HashMap<int, TopoName> VertexNames,
    Generation Generation) : IValidityEvidence {
    public static readonly NameTable Empty = new(
        HashMap<TopoName, NameEntry>.Empty, HashMap<EntityKind, HashMap<TopoSignature, Set<TopoName>>>.Empty,
        HashMap<TopoName, Seq<TopoName>>.Empty, HashMap<int, TopoName>.Empty, Generation.Create(0));

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(
            count: SignatureIndex.Values.Fold(0, static (n, perKind) => perKind.Values.Fold(n, static (m, bucket) => m + bucket.Count)),
            expected: Entries.Count),
        Entries.Values.ForAll(entry => entry.Born.Value <= entry.LastSeen.Value && entry.LastSeen.Value <= Generation.Value),
        VertexNames.Values.ForAll(Entries.ContainsKey));

    public Seq<TopoName> Resolve(EntityKind kind, TopoSignature signature) =>
        SignatureIndex.Find(kind).Bind(index => index.Find(signature))
            .Match(Some: static bucket => toSeq(bucket), None: static () => Seq<TopoName>());

    public Seq<TopoName> ResolveBoundary(Arr<int> incidentVertices) =>
        toSeq(incidentVertices.AsIterable()).Map(VertexNames.Find).Somes();

    public NameTable With(NameEntry entry, int self) {
        HashMap<TopoSignature, Set<TopoName>> perKind = SignatureIndex.Find(entry.Kind).IfNone(HashMap<TopoSignature, Set<TopoName>>.Empty);
        HashMap<TopoSignature, Set<TopoName>> index = perKind.AddOrUpdate(entry.Signature,
            bucket => bucket.TryAdd(entry.Name), Set(entry.Name));
        HashMap<TopoName, Seq<TopoName>> postings = toSeq(entry.Boundary.Distinct()).Fold(BoundaryIndex, (posted, name) => posted.AddOrUpdate(
            name, owners => owners.Contains(entry.Name) ? owners : owners.Add(entry.Name), Seq(entry.Name)));
        HashMap<int, TopoName> vertices = entry.Kind == EntityKind.Vertex ? VertexNames.AddOrUpdate(self, entry.Name) : VertexNames;
        return this with {
            Entries = Entries.AddOrUpdate(entry.Name, entry), SignatureIndex = SignatureIndex.AddOrUpdate(entry.Kind, index),
            BoundaryIndex = postings, VertexNames = vertices,
        };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Naming {
    public static Fin<NameTable> Apply(NamingOp op, Op? key = null) {
        Op minted = key.OrDefault();
        return op.Switch(
            state: minted,
            track: static (k, t) => t.Prior.Generation.Next(key: k)
                .Bind(next => Anchor(t.Prior, t.Rebuilt, next, t.Policy.IfNone(NamingPolicy.Canonical), k)),
            resolve: static (k, r) => Anchor(NameTable.Empty, r.Boundary, Generation.Create(0), NamingPolicy.Canonical, k));
    }

    static Fin<NameTable> Anchor(NameTable prior, CanonicalTopology rebuilt, Generation next, NamingPolicy policy, Op key) {
        HashMap<int, Arr<int>> stars = toHashMap(rebuilt.Entities
            .Filter(static e => e.Kind == EntityKind.Vertex).Map(static e => (e.Self, e.KindHistogram)));
        (NameTable Table, Set<TopoName> Claimed, Seq<Error> Collisions) folded = toSeq(rebuilt.Entities.OrderBy(static e => e.Kind.Key))
            .Fold((Table: NameTable.Empty with { Generation = next }, Claimed: Set<TopoName>.Empty, Collisions: Seq<Error>()),
                (state, entity) => Step(prior, entity, stars, next, policy, key, state));
        return folded.Collisions.IsEmpty
            ? Fin.Succ(RefineVertices(prior, folded.Table, rebuilt, policy, next))
                .Bind(refined => guard(refined.IsValid, key.InvalidResult()).ToFin().Map(_ => refined))
            : Validation.Fail<Error, NameTable>(folded.Collisions).ToFin();
    }

    static (NameTable Table, Set<TopoName> Claimed, Seq<Error> Collisions) Step(
        NameTable prior, RebuiltEntity entity, HashMap<int, Arr<int>> stars, Generation next, NamingPolicy policy, Op key,
        (NameTable Table, Set<TopoName> Claimed, Seq<Error> Collisions) state) {
        Seq<TopoName> boundary = entity.Kind.BoundaryOf(table: state.Table, entity: entity);
        TopoSignature signature = TopoSignature.Of(entity.Kind, boundary, entity.Kind.FingerprintOf(entity: entity, stars: stars));
        Fin<NameEntry> bound = prior.Resolve(entity.Kind, signature)
            .Filter(name => !state.Claimed.Contains(name))
            .Head
            .Match(
                Some: name => Survive(prior, name, signature, boundary, next, entity, key),
                None: () => MigrateOrBirth(prior, entity, signature, boundary, next, policy))
            .Bind(entry => state.Claimed.Contains(entry.Name)
                ? Fin.Fail<NameEntry>(new GeometryFault.NameCollision(entry.Name, entity.Kind))
                : Fin.Succ(entry));
        return bound.Match(
            Succ: entry => (state.Table.With(entry, entity.Self), state.Claimed.Add(entry.Name), state.Collisions),
            Fail: error => (state.Table, state.Claimed, state.Collisions.Add(error)));
    }

    static Fin<NameEntry> Survive(NameTable prior, TopoName name, TopoSignature signature, Seq<TopoName> boundary, Generation next, RebuiltEntity entity, Op key) =>
        prior.Entries.Find(name)
            .ToFin(key.InvalidResult())
            .Map(prev => prev with { LastSeen = next, Signature = signature, Boundary = boundary, Canonical = entity.Canonical });

    static Fin<NameEntry> MigrateOrBirth(NameTable prior, RebuiltEntity entity, TopoSignature signature, Seq<TopoName> boundary, Generation next, NamingPolicy policy) {
        Option<TopoName> parent = OverlapParent(prior, entity.Kind, boundary, policy);
        TopoName name = TopoName.Mint(entity.Kind, entity.Canonical, next);
        return Fin.Succ(new NameEntry(name, entity.Kind, next, next, parent, signature, boundary, entity.Canonical));
    }

    static Option<TopoName> OverlapParent(NameTable prior, EntityKind kind, Seq<TopoName> boundary, NamingPolicy policy) =>
        toSeq(toSeq(boundary.Distinct())
                .Bind(name => prior.BoundaryIndex.Find(name).IfNone(Seq<TopoName>()))
                .Distinct())
            .Map(prior.Entries.Find).Somes()
            .Filter(prev => prev.Kind == kind)
            .Map(prev => (Entry: prev, Score: TopoSignature.Overlap(prior: prev.Boundary, rebuilt: boundary)))
            .Filter(candidate => candidate.Score >= policy.MigrationOverlap.Value)
            .Fold(Option<(NameEntry Entry, double Score)>.None, static (best, next) => best.Match(
                Some: held => Beats(challenger: next, held: held) ? Some(next) : best,
                None: () => Some(next)))
            .Map(static candidate => candidate.Entry.Name);

    static bool Beats((NameEntry Entry, double Score) challenger, (NameEntry Entry, double Score) held) =>
        challenger.Score > held.Score
        || (challenger.Score == held.Score && challenger.Entry.Name.Value < held.Entry.Name.Value);

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
-->

(none)
