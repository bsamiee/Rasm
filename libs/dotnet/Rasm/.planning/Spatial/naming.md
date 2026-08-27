# [RASM_TOPOLOGY_NAMING]

Persistent topological naming survives every rebuild behind one `Naming.Apply(NamingOp)` entry: the closed `NamingOp` `[Union]` folds `Track` re-anchoring names across a rebuild and `Resolve` minting the generation-zero table for a first build, over one `TopoName` lineage algebra spanning every `EntityKind`. `TopoName` is a content-address-derived `UInt128` reference identity — which entity, lineage-stable across generations — orthogonal to the content identity the reconciliation sibling bridges. Its fold accumulates every non-injective re-anchor into `Validation<Error, NameTable>` before exiting the one `Fin` result, so one verdict carries a defective rebuild's complete collision set, each row a `GeometryFault.NameCollision` (`Numerics/faults.md`).

`Naming` reads `Rasm.Meshing` `MeshSpace` and the native `Mesh` topology as settled vocabulary — never re-minted — and consumes the `CanonicalTopology` the reconciliation sibling emits in this same `Rasm.Spatial` namespace; `TopoName`, `EntityKind`, `NamingOp`, and the lineage records are interior types that never cross a transport. Migration rides one `NamingPolicy.MigrationOverlap` fraction, and the emitted `NameTable` gates its own `IValidityEvidence` fold before every `Apply` returns.

## [01]-[INDEX]

- [02]-[TOPO_NAMING]: one `Naming.Apply(NamingOp, Op?)` entry over the `TopoName` lineage algebra; the `NameTable` generation registry of `TopoName`-keyed lineage entries; overlap-fraction migration under `NamingPolicy`; the accumulating injectivity audit.

## [02]-[TOPO_NAMING]

- Owner: `Naming.Apply` folds the `NamingOp` request algebra over one `TopoName` lineage; `TopoName` `[ValueObject<UInt128>]` is the single naming reference across every `EntityKind`, its modality carried in the `Kind` column of `NameEntry`. `NameTable` is the immutable per-generation registry of `Entries` keyed by `TopoName` at one `Generation`, registered `IValidityEvidence`; exact buckets and migration postings derive from the prior table inside `Anchor`, and vertex ordinals live only in fold state.
- Cases: `EntityKind` rows `Vertex`, `Edge`, and `Face`, each carrying its one `Describe` delegate column that emits boundary names and fingerprint together; `NamingOp` cases `Track` and `Resolve`; `TrackOutcome` cases `Survived`, `Migrated`, and `Born`, derived from the `NameEntry` lineage columns through `Outcome`.
- Entry: `Naming.Apply(NamingOp)` is the one entry over both modalities. `Track(prior, rebuilt)` admits both inputs through `Op.AcceptInput` as one accumulating `Validation` before the generation advances, then walks every rebuilt entity, resolves its `TopoSignature` against exact buckets derived from the prior entries, and emits the table at `prior.Generation.Next()`, so monotonicity holds by construction and a computed successor routes its refusal on the same result rather than throwing; `Resolve(boundary)` runs the same fold against an empty generation-zero `NameTable` with every entity `Born`, so a first build and a re-anchor are one body discriminated by the op case. Collisions accumulate into `Validation<Error, NameTable>` and exit `.ToFin()`, keeping the public type `Fin<NameTable>`. `TopoName.Mint(kind, canonical, born)` derives the lineage-root name as a seed-zero `UInt128` over kind ordinal, born generation, and the entity's canonical word run — content-stable yet lineage-distinct across generations. Every preimage emits through the kernel `CanonicalWriter`: `Mint` and `TopoSignature.Of` both take the count frames `Rows`/`Sorted` write, a digest change no wire sees, because `TopoName` and `TopoSignature` are interior types no transport carries, and the frames close the collision an unframed word run, name list, and histogram spell together.
- Auto: `Naming.Apply` walks entities in `EntityKind` order so vertices anchor first and every edge or face resolves its incident vertices through the in-progress vertex ordinals of the generation being built — a genesis signature carries real names and generation g compares like-with-like against generation g−1. The `EntityKind` row owns the boundary feed: the vertex row takes no boundary names and separates by its WL-1 star fingerprint, while edges and faces feed resolved name multisets. Exact re-anchor is a bucket lookup over an ordered `Set<TopoName>` derived once from the prior entries and filtered against the current `Entries` — indistinguishable entities share a bucket, so an exhausted bucket falls through to migration or birth as growth. Misses migrate by maximum multiset overlap over migration-posting candidates, scored inside the `Parent` local search under the `NamingPolicy.MigrationOverlap` floor and a smallest-`TopoName` tiebreak, taken as one linear argmax fold. A `Migrated` or `Born` entry mints a fresh generation-salted name, refused as `NameCollision` against both current and prior identities. The vertex refinement pass stores each vertex's completed star as the following generation's migration material and gives a generation-fresh orphan star-overlap provenance. One `EntityKind` discriminant drives one fold body across vertex, edge, and face.
- Output: `Apply` returns the `NameTable` of the generation it builds directly — the registry IS the result, its birth and last-seen generations and parent provenance the per-name lineage evidence, and `NameEntry.Outcome` derives the `Survived`/`Migrated`/`Born` classification from `Born`, `LastSeen`, and `Parent` alone. `NameTable : IValidityEvidence` declares one `ForAll` over `Entries` — every key equals its entry's `Name` and lineage generations stay ordered `Born ≤ LastSeen ≤ Generation` — gated through `Op.AcceptValue` before emission. Non-negativity is NOT among them: `Generation`'s own factory validation refuses a negative, so a claim re-asserting it never fails.
- Packages: `Rasm.Meshing` (`MeshSpace`, native `Mesh`), `Rasm.Domain` (`ContentHash.Of`, the `Op` key type, `IValidityEvidence`), `Rasm.Numerics` (`GeometryFault`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Validation`/`HashMap`/`Seq`/`Option`), BCL inbox.
- Growth: a new entity modality is one `EntityKind` row stating its boundary feed and fingerprint; a new lineage outcome is one `TrackOutcome` case and one `Outcome` projection arm; a new migration disambiguator is one `NamingPolicy` column the parent search reads; a new op modality is one `NamingOp` case on the same `Apply` fold.
- Boundary: `TopoName` is the one naming value object over every `EntityKind`, the modality carried in the `Kind` column. `TopoSignature` is position-free — built from incident names and kind histograms, never coordinates — so a rigid transform preserves every name and only an adjacency change re-anchors, matching the morph-versus-topology-break distinction `GeometryHash` reads from the same canonical adjacency. Migration is the shared-name multiset fraction under the `NamingPolicy.MigrationOverlap` floor, scored inside the `Parent` search; vertex ordinals key by `RebuiltEntity.Self` in fold state; boundary names resolve through those ordinals on the vertices-first walk; exact buckets and migration postings both derive once from the prior `Entries` as hash indexes. `Apply` is total over `Fin`, a name collision routing `GeometryFault.NameCollision` (`Numerics/faults.md`) accumulated internally as `Validation<Error, NameTable>`. `TopoName` is a `UInt128` reference identity orthogonal to the `GeometryHash` content identity the reconciliation sibling bridges, and the `NameTable` is immutable — `Apply` returns the next generation.

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
    public static readonly EntityKind Vertex = new(key: 0, describe: static (_, entity, stars) =>
        (Seq<TopoName>(), toSeq(entity.KindHistogram.AsIterable())
            + toSeq(toSeq(entity.IncidentVertices.AsIterable()).Map(stars.Find).Somes()
                .OrderBy(static histogram => histogram, HistogramOrder)
                .SelectMany(static histogram => histogram.AsIterable()))));
    public static readonly EntityKind Edge = new(key: 1, describe: static (vertices, entity, _) =>
        (toSeq(entity.IncidentVertices.AsIterable()).Map(vertices.Find).Somes(),
            toSeq(entity.KindHistogram.AsIterable())));
    public static readonly EntityKind Face = new(key: 2, describe: static (vertices, entity, _) =>
        (toSeq(entity.IncidentVertices.AsIterable()).Map(vertices.Find).Somes(),
            toSeq(entity.KindHistogram.AsIterable())));

    [UseDelegateFromConstructor]
    internal partial (Seq<TopoName> Boundary, Seq<int> Fingerprint) Describe(
        HashMap<int, TopoName> vertices, RebuiltEntity entity, HashMap<int, Arr<int>> stars);

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

    public Fin<Generation> Next() => Value == int.MaxValue
        ? Fin.Fail<Generation>(new KernelFault.InvalidResult())
        : FactoryBridge.Accept<Generation>(candidate: Value + 1);
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoSignature {
    internal static TopoSignature Of(EntityKind kind, Seq<TopoName> incidentNames, Seq<int> kindHistogram) =>
        Create(ContentHash.Of(
            state: (Kind: kind, Names: incidentNames, Histogram: kindHistogram),
            chunks: static (row, sink) => sink
                .Ordinal(value: row.Kind.Key)
                .Sorted(rows: row.Names, key: static name => name.Value, order: Comparer<UInt128>.Default,
                    field: static (name, field) => field.U128(value: name.Value))
                .Rows(rows: row.Histogram, field: static (count, field) => field.Ordinal(value: count))));
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TopoName {
    internal static TopoName Mint(EntityKind kind, Arr<int> canonical, Generation born) =>
        Create(ContentHash.Of(
            state: (Kind: kind, Born: born, Canonical: canonical),
            chunks: static (row, sink) => sink
                .Ordinal(value: row.Kind.Key)
                .Ordinal(value: row.Born.Value)
                .Rows(rows: toSeq(row.Canonical.AsIterable()), field: static (word, field) => field.Ordinal(value: word))));
}

public sealed record NamingPolicy(UnitInterval MigrationOverlap) {
    public static readonly NamingPolicy Canonical = new(MigrationOverlap: UnitInterval.Create(value: 0.5));
}

public readonly record struct NameEntry(
    TopoName Name, EntityKind Kind, Generation Born, Generation LastSeen, Option<TopoName> Parent, TopoSignature Signature, Seq<TopoName> Boundary, Arr<int> Canonical) {
    public TrackOutcome Outcome => Born.Value < LastSeen.Value
        ? new TrackOutcome.Survived(Name)
        : Parent is { IsSome: true, Case: TopoName parent }
            ? new TrackOutcome.Migrated(Name, parent)
            : new TrackOutcome.Born(Name);
}

public readonly record struct RebuiltEntity(EntityKind Kind, int Self, Arr<int> Canonical, Arr<int> IncidentVertices, Arr<int> KindHistogram);

public sealed record NameTable(HashMap<TopoName, NameEntry> Entries, Generation Generation) : IValidityEvidence {
    public bool IsValid => Entries.AsIterable().ForAll(pair =>
        pair.Key == pair.Value.Name
        && pair.Value.Born.Value <= pair.Value.LastSeen.Value
        && pair.Value.LastSeen.Value <= Generation.Value);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Naming {
    public static Fin<NameTable> Apply(NamingOp op) {
        return op.Switch(
            state: minted,
            track: static (k, t) =>
                (Acceptance.Input(t.Prior).ToValidation(), Acceptance.Input(t.Rebuilt).ToValidation())
                    .Apply(static (prior, rebuilt) => (Prior: prior, Rebuilt: rebuilt)).As().ToFin()
                    >> (input => input.Prior.Generation.Next(k)
                        >> (next => Anchor(input.Prior, input.Rebuilt, next, t.Policy.IfNone(NamingPolicy.Canonical), k))),
            resolve: static (k, r) => Acceptance.Input(r.Boundary)
                >> (boundary => Anchor(
                    new NameTable(HashMap<TopoName, NameEntry>(), Generation.Create(0)),
                    boundary, Generation.Create(0), NamingPolicy.Canonical, k)));
    }

    static Fin<NameTable> Anchor(NameTable prior, CanonicalTopology rebuilt, Generation next, NamingPolicy policy) {
        (HashMap<EntityKind, HashMap<TopoSignature, Set<TopoName>>> Exact, HashMap<TopoName, Set<TopoName>> Migration) indexes =
            prior.Entries.Values.Fold(
                (Exact: HashMap<EntityKind, HashMap<TopoSignature, Set<TopoName>>>(), Migration: HashMap<TopoName, Set<TopoName>>()),
                static (all, entry) => {
                    HashMap<TopoSignature, Set<TopoName>> rows = all.Exact.Find(entry.Kind).IfNone(HashMap<TopoSignature, Set<TopoName>>());
                    return (all.Exact.AddOrUpdate(entry.Kind, rows.AddOrUpdate(entry.Signature, names => names.TryAdd(entry.Name), Set(entry.Name))),
                        entry.Boundary.Distinct().Fold(all.Migration, (posted, name) => posted.AddOrUpdate(
                            name, names => names.TryAdd(entry.Name), Set(entry.Name))));
                });
        HashMap<int, Arr<int>> stars = toHashMap(rebuilt.Entities
            .Filter(static e => e.Kind == EntityKind.Vertex).Map(static e => (e.Self, e.KindHistogram)));

        Option<TopoName> Parent(EntityKind kind, Seq<TopoName> boundary) =>
            toSeq(boundary.Distinct())
                .Bind(name => indexes.Migration.Find(name).Match(Some: static names => toSeq(names), None: static () => Seq<TopoName>()))
                .Distinct().Map(prior.Entries.Find).Somes()
                .Filter(entry => entry.Kind == kind)
                .Map(entry => {
                    if (entry.Boundary.IsEmpty || boundary.IsEmpty) return (Entry: entry, Score: 0.0);
                    HashMap<TopoName, int> tally = entry.Boundary.Fold(HashMap<TopoName, int>(),
                        static (held, name) => held.AddOrUpdate(name, static count => count + 1, 1));
                    int shared = boundary.Fold((Tally: tally, Hits: 0), static (state, name) =>
                        state.Tally.Find(name).Filter(static count => count > 0).Match(
                            Some: count => (state.Tally.SetItem(name, count - 1), state.Hits + 1),
                            None: () => state)).Hits;
                    return (Entry: entry, Score: (double)shared / Math.Min(entry.Boundary.Count, boundary.Count));
                })
                .Filter(candidate => candidate.Score >= policy.MigrationOverlap.Value)
                .Fold(Option<(NameEntry Entry, double Score)>.None, static (best, next) => best.Match(
                    Some: held => next.Score > held.Score || (next.Score == held.Score && next.Entry.Name.Value < held.Entry.Name.Value)
                        ? Some(next) : best,
                    None: () => Some(next)))
                .Map(static candidate => candidate.Entry.Name);

        (NameTable Table, Seq<Error> Collisions, HashMap<int, TopoName> Vertices) folded =
            toSeq(rebuilt.Entities.OrderBy(static entity => entity.Kind.Key)).Fold(
                (Table: new NameTable(HashMap<TopoName, NameEntry>(), next), Collisions: Seq<Error>(), Vertices: HashMap<int, TopoName>()),
                (state, entity) => {
                    (Seq<TopoName> Boundary, Seq<int> Fingerprint) shape = entity.Kind.Describe(state.Vertices, entity, stars);
                    TopoSignature signature = TopoSignature.Of(entity.Kind, shape.Boundary, shape.Fingerprint);
                    Option<NameEntry> exact = indexes.Exact.Find(entity.Kind)
                        .Bind(rows => rows.Find(signature))
                        .Bind(names => toSeq(names).Filter(name => !state.Table.Entries.ContainsKey(name)).Head)
                        .Bind(prior.Entries.Find);
                    Fin<NameEntry> bound = exact.Match(
                        Some: entry => Fin.Succ(entry with { LastSeen = next, Signature = signature, Boundary = shape.Boundary, Canonical = entity.Canonical }),
                        None: () => {
                            TopoName name = TopoName.Mint(entity.Kind, entity.Canonical, next);
                            return state.Table.Entries.ContainsKey(name) || prior.Entries.ContainsKey(name)
                                ? Fin.Fail<NameEntry>(new GeometryFault.NameCollision(name, entity.Kind))
                                : Fin.Succ(new NameEntry(name, entity.Kind, next, next,
                                    Parent(entity.Kind, shape.Boundary), signature, shape.Boundary, entity.Canonical));
                        });
                    return bound.Match(
                        Succ: entry => (state.Table with { Entries = state.Table.Entries.AddOrUpdate(entry.Name, entry) },
                            state.Collisions, entity.Kind == EntityKind.Vertex ? state.Vertices.AddOrUpdate(entity.Self, entry.Name) : state.Vertices),
                        Fail: error => (state.Table, state.Collisions.Add(error), state.Vertices));
                });

        NameTable refined = rebuilt.Entities.Filter(static entity => entity.Kind == EntityKind.Vertex)
            .Fold(folded.Table, (table, entity) => folded.Vertices.Find(entity.Self)
                .Bind(table.Entries.Find)
                .Map(entry => (Entry: entry, Star: toSeq(entity.IncidentVertices.AsIterable()).Map(folded.Vertices.Find).Somes()))
                .Match(Some: bound => table with { Entries = table.Entries.SetItem(bound.Entry.Name,
                    bound.Entry with { Boundary = bound.Star,
                        Parent = bound.Entry.Born == next && bound.Entry.Parent.IsNone
                            ? Parent(entity.Kind, bound.Star) : bound.Entry.Parent }) },
                    None: () => table));

        return folded.Collisions.IsEmpty
            ? Acceptance.Value(refined)
            : Validation.Fail<Error, NameTable>(Error.Many(folded.Collisions)).ToFin();
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
