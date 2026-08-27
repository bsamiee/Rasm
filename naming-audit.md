# 1. Derive lineage outcome from the entry itself

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:126-134`, anchor `NameEntry.Outcome(Generation generation)`

### From

```csharp
public TrackOutcome Outcome(Generation generation) =>
    Born.Value < generation.Value
        ? new TrackOutcome.Survived(Name)
        : Parent.Match(
            Some: parent => (TrackOutcome)new TrackOutcome.Migrated(Name, parent),
            None: () => new TrackOutcome.Born(Name));
```

### To

```csharp
public TrackOutcome Outcome => Born.Value < LastSeen.Value
    ? new TrackOutcome.Survived(Name)
    : Parent.Match(
        Some: parent => (TrackOutcome)new TrackOutcome.Migrated(Name, parent),
        None: () => new TrackOutcome.Born(Name));
```

### Why

`Born`, `LastSeen`, and `Parent` already carry the complete classification evidence. Accepting an unrelated generation lets a caller classify a stale row as `Survived`; deriving from `LastSeen` removes that invalid degree of freedom while preserving the genuine typed outcome capability and its closed `TrackOutcome` family.

# 2. Delete the raw policy forwarding factory

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:119-124`, anchor `NamingPolicy.Of`

### From

```csharp
public static Fin<NamingPolicy> Of(double migrationOverlap, Op? key = null) =>
    key.OrDefault().AcceptValidated<UnitInterval>(candidate: migrationOverlap)
        .Map(static fraction => new NamingPolicy(MigrationOverlap: fraction));
```

### To

```csharp
// NamingPolicy.Of DELETED
```

### Why

The factory only renames the existing `AcceptValidated<UnitInterval>` admission and immediately forwards its result into the public record constructor. It has no consumer under `libs/dotnet/` and adds no policy invariant or behavior. Keep `NamingPolicy` and its canonical row because the policy is a genuine growth owner; delete only the unused raw-value convenience wrapper.

# 3. Collapse entity description into one generated operation

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:36-56`, anchors `EntityKind.Vertex`, `Edge`, `Face`, `BoundaryOf`, `FingerprintOf`, and `Star`
- `libs/dotnet/Rasm/.planning/Spatial/naming.md:202-203`, anchors the paired generated calls in `Naming.Step`

### From

```csharp
[UseDelegateFromConstructor] internal partial Seq<TopoName> BoundaryOf(
    NameTable table, RebuiltEntity entity);
[UseDelegateFromConstructor] internal partial Seq<int> FingerprintOf(
    RebuiltEntity entity, HashMap<int, Arr<int>> stars);
```

```csharp
private static Seq<int> Star(RebuiltEntity entity, HashMap<int, Arr<int>> stars) =>
    toSeq(entity.KindHistogram.AsIterable())
    + toSeq(toSeq(entity.IncidentVertices.AsIterable()).Map(stars.Find).Somes()
        .OrderBy(static histogram => histogram, HistogramOrder)
        .SelectMany(static histogram => histogram.AsIterable()));
```

### To

```csharp
[UseDelegateFromConstructor]
internal partial (Seq<TopoName> Boundary, Seq<int> Fingerprint) Describe(
    HashMap<int, TopoName> vertices, RebuiltEntity entity,
    HashMap<int, Arr<int>> stars);
```

```csharp
public static readonly EntityKind Vertex = new(key: 0, describe: static (_, entity, stars) =>
    (Seq<TopoName>(), toSeq(entity.KindHistogram.AsIterable())
        + toSeq(toSeq(entity.IncidentVertices.AsIterable()).Map(stars.Find).Somes()
            .OrderBy(static histogram => histogram, HistogramOrder)
            .SelectMany(static histogram => histogram.AsIterable()))));
```

```csharp
public static readonly EntityKind Edge = new(key: 1, describe: static (vertices, entity, _) =>
    (toSeq(entity.IncidentVertices.AsIterable()).Map(vertices.Find).Somes(),
        toSeq(entity.KindHistogram.AsIterable())));
public static readonly EntityKind Face = new(key: 2, describe: static (vertices, entity, _) =>
    (toSeq(entity.IncidentVertices.AsIterable()).Map(vertices.Find).Somes(),
        toSeq(entity.KindHistogram.AsIterable())));
```

```csharp
// EntityKind.BoundaryOf DELETED
// EntityKind.FingerprintOf DELETED
// EntityKind.Star DELETED
```

### Why

Boundary names and the fingerprint are consumed only as the two inputs to one signature mint. One constructor-backed delegate keeps behavior row-owned for every `EntityKind` while deleting one generated method, one delegate slot per row, and the one-call `Star` member; it also prevents the two halves from observing different vertex state.

# 4. Keep only authoritative state in the name table

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:138-173`, anchors `NameTable.SignatureIndex`, `BoundaryIndex`, `VertexNames`, `Empty`, `IsValid`, `Resolve`, `ResolveBoundary`, and `With`
- `libs/dotnet/Rasm/.planning/Spatial/naming.md:187-192`, anchors `Naming.Anchor` index reads and fold seed
- `libs/dotnet/Rasm/.planning/Spatial/naming.md:229-240`, anchor `Naming.OverlapParent` posting reads

### From

```csharp
public sealed record NameTable(
    HashMap<TopoName, NameEntry> Entries,
    HashMap<EntityKind, HashMap<TopoSignature, Set<TopoName>>> SignatureIndex,
    HashMap<TopoName, Seq<TopoName>> BoundaryIndex,
    HashMap<int, TopoName> VertexNames,
    Generation Generation) : IValidityEvidence {
```

```csharp
public Seq<TopoName> Resolve(EntityKind kind, TopoSignature signature) =>
    SignatureIndex.Find(kind).Bind(index => index.Find(signature))
        .Match(Some: static bucket => toSeq(bucket), None: static () => Seq<TopoName>());

public Seq<TopoName> ResolveBoundary(Arr<int> incidentVertices) =>
    toSeq(incidentVertices.AsIterable()).Map(VertexNames.Find).Somes();
```

### To

```csharp
public sealed record NameTable(
    HashMap<TopoName, NameEntry> Entries,
    Generation Generation) : IValidityEvidence {
    public bool IsValid => Entries.AsIterable().ForAll(pair =>
        pair.Key == pair.Value.Name
        && pair.Value.Born.Value <= pair.Value.LastSeen.Value
        && pair.Value.LastSeen.Value <= Generation.Value);
```

```csharp
// NameTable.SignatureIndex DELETED
// NameTable.BoundaryIndex DELETED
// NameTable.VertexNames DELETED
// NameTable.Resolve DELETED
// NameTable.ResolveBoundary DELETED
// NameTable.With DELETED
// NameTable.Empty DELETED
```

Build both prior-only indexes once at the start of `Anchor`:

```csharp
(HashMap<EntityKind, HashMap<TopoSignature, Set<TopoName>>> Exact,
    HashMap<TopoName, Set<TopoName>> Migration) indexes = prior.Entries.Values.Fold(
    (Exact: HashMap<EntityKind, HashMap<TopoSignature, Set<TopoName>>>.Empty,
        Migration: HashMap<TopoName, Set<TopoName>>.Empty), static (all, entry) => {
```

```csharp
HashMap<TopoSignature, Set<TopoName>> rows = all.Exact.Find(entry.Kind)
    .IfNone(HashMap<TopoSignature, Set<TopoName>>.Empty);
return (all.Exact.AddOrUpdate(entry.Kind, rows.AddOrUpdate(entry.Signature,
        names => names.TryAdd(entry.Name), Set(entry.Name))),
    entry.Boundary.Distinct().Fold(all.Migration, (posted, name) => posted.AddOrUpdate(
        name, names => names.TryAdd(entry.Name), Set(entry.Name))));
});
```

Keep current-generation vertex ordinals beside the fold, not in its result:

```csharp
(NameTable Table, Seq<Error> Collisions, HashMap<int, TopoName> Vertices) folded =
    toSeq(rebuilt.Entities.OrderBy(static entity => entity.Kind.Key)).Fold(
        (Table: new NameTable(HashMap<TopoName, NameEntry>.Empty, next),
            Collisions: Seq<Error>(), Vertices: HashMap<int, TopoName>.Empty),
```

### Why

`Entries` and `Generation` are the table's truth. Exact buckets and migration postings are total derivations read only while the next rebuild probes the prior generation, and vertex ordinals exist only during the vertices-first construction of the current generation. Deriving the two prior indexes together preserves indexed lookup complexity; keeping vertices in fold state removes three public fields, their constructor and update paths, three forwarding members, and validity clauses whose only job was to detect stale mirrors. Exact buckets remain ordered `Set<TopoName>` values, so deterministic smallest-name selection needs no per-lookup sort.

# 5. Inline the one-use transition and delete the claimed-name mirror

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:190-192`, anchors the `Claimed` fold state and `Step` call
- `libs/dotnet/Rasm/.planning/Spatial/naming.md:199-227`, anchors `Naming.Step`, `Survive`, and `MigrateOrBirth`

### From

```csharp
.Fold((Table: NameTable.Empty with { Generation = next },
    Claimed: Set<TopoName>.Empty, Collisions: Seq<Error>()),
    (state, entity) => Step(prior, entity, stars, next, policy, key, state));
```

```csharp
static Fin<NameEntry> Survive(NameTable prior, TopoName name, TopoSignature signature,
    Seq<TopoName> boundary, Generation next, RebuiltEntity entity, Op key) =>
    prior.Entries.Find(name).ToFin(key.InvalidResult())
        .Map(prev => prev with { LastSeen = next, Signature = signature,
            Boundary = boundary, Canonical = entity.Canonical });
```

```csharp
static Fin<NameEntry> MigrateOrBirth(NameTable prior, RebuiltEntity entity,
    TopoSignature signature, Seq<TopoName> boundary, Generation next, NamingPolicy policy) {
    Option<TopoName> parent = OverlapParent(prior, entity.Kind, boundary, policy);
    TopoName name = TopoName.Mint(entity.Kind, entity.Canonical, next);
    return Fin.Succ(new NameEntry(name, entity.Kind, next, next,
        parent, signature, boundary, entity.Canonical));
}
```

### To

Inline the transition as the `Fold` body:

```csharp
(state, entity) => {
    (Seq<TopoName> Boundary, Seq<int> Fingerprint) shape =
        entity.Kind.Describe(state.Vertices, entity, stars);
    TopoSignature signature = TopoSignature.Of(
        entity.Kind, shape.Boundary, shape.Fingerprint);
```

```csharp
Option<NameEntry> exact = indexes.Exact.Find(entity.Kind)
    .Bind(rows => rows.Find(signature))
    .Bind(names => toSeq(names).Filter(name => !state.Table.Entries.ContainsKey(name)).Head)
    .Bind(prior.Entries.Find);
```

```csharp
Fin<NameEntry> bound = exact.Match(
    Some: entry => Fin.Succ(entry with { LastSeen = next, Signature = signature,
        Boundary = shape.Boundary, Canonical = entity.Canonical }),
    None: () => {
        TopoName name = TopoName.Mint(entity.Kind, entity.Canonical, next);
        return state.Table.Entries.ContainsKey(name) || prior.Entries.ContainsKey(name)
            ? Fin.Fail<NameEntry>(new GeometryFault.NameCollision(name, entity.Kind))
            : Fin.Succ(new NameEntry(name, entity.Kind, next, next,
                Parent(entity.Kind, shape.Boundary), signature, shape.Boundary, entity.Canonical));
    });
```

```csharp
return bound.Match(
    Succ: entry => (state.Table with { Entries = state.Table.Entries.AddOrUpdate(entry.Name, entry) },
        state.Collisions, entity.Kind == EntityKind.Vertex
            ? state.Vertices.AddOrUpdate(entity.Self, entry.Name) : state.Vertices),
    Fail: error => (state.Table, state.Collisions.Add(error), state.Vertices));
});
```

```csharp
// Naming.Step DELETED
// Naming.Survive DELETED
// Naming.MigrateOrBirth DELETED
// Claimed DELETED
```

### Why

The transition and both branch helpers each have one call site. Inlining them exposes one exact-or-fresh decision without private forwarding surface. `Table.Entries` already is the current generation's claimed-name set, so `Claimed` duplicates state. A fresh mint must refuse collision with both current assignments and every prior identity; the current code checks only already-claimed names, allowing a hash collision with an unclaimed prior name to masquerade as a new lineage.

# 6. Localize overlap scoring inside parent selection

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:96-105`, anchor `TopoSignature.Overlap`
- `libs/dotnet/Rasm/.planning/Spatial/naming.md:229-244`, anchors `Naming.OverlapParent` and `Naming.Beats`
- `libs/dotnet/Rasm/.planning/Spatial/naming.md:223-240` and `246-257`, anchors the two parent-selection call sites

### From

```csharp
public static double Overlap(Seq<TopoName> prior, Seq<TopoName> rebuilt) {
    if (prior.IsEmpty || rebuilt.IsEmpty) return 0.0;
    HashMap<TopoName, int> tally = prior.Fold(HashMap<TopoName, int>.Empty,
        static (held, name) => held.AddOrUpdate(name, static count => count + 1, 1));
```

```csharp
static bool Beats((NameEntry Entry, double Score) challenger,
    (NameEntry Entry, double Score) held) =>
    challenger.Score > held.Score
    || (challenger.Score == held.Score && challenger.Entry.Name.Value < held.Entry.Name.Value);
```

### To

Move the shared parent search into `Anchor` as a local function capturing `prior`, `indexes.Migration`, and `policy`:

```csharp
Option<TopoName> Parent(EntityKind kind, Seq<TopoName> boundary) =>
    toSeq(boundary.Distinct())
        .Bind(name => indexes.Migration.Find(name).Match(
            Some: static names => toSeq(names), None: static () => Seq<TopoName>()))
        .Distinct().Map(prior.Entries.Find).Somes()
        .Filter(entry => entry.Kind == kind)
```

```csharp
.Map(entry => {
    if (entry.Boundary.IsEmpty || boundary.IsEmpty) return (Entry: entry, Score: 0.0);
    HashMap<TopoName, int> tally = entry.Boundary.Fold(HashMap<TopoName, int>.Empty,
        static (held, name) => held.AddOrUpdate(name, static count => count + 1, 1));
    int shared = boundary.Fold((Tally: tally, Hits: 0), static (state, name) =>
        state.Tally.Find(name).Filter(static count => count > 0).Match(
            Some: count => (state.Tally.SetItem(name, count - 1), state.Hits + 1),
            None: () => state)).Hits;
```

```csharp
return (Entry: entry, Score: (double)shared
    / Math.Min(entry.Boundary.Count, boundary.Count));
}).Filter(candidate => candidate.Score >= policy.MigrationOverlap.Value)
.Fold(Option<(NameEntry Entry, double Score)>.None, static (best, next) => best.Match(
    Some: held => next.Score > held.Score
        || (next.Score == held.Score && next.Entry.Name.Value < held.Entry.Name.Value)
            ? Some(next) : best,
    None: () => Some(next))).Map(static candidate => candidate.Entry.Name);
```

```csharp
// TopoSignature.Overlap DELETED
// Naming.OverlapParent DELETED
// Naming.Beats DELETED
```

### Why

Multiset overlap is behavior of migration parent selection, not of the exact-signature value, and its deterministic tiebreak has the same single owner. The parent search is genuinely shared by fresh-entry creation and vertex refinement, so a local function preserves that reuse while deleting three module-level members, keeping the operation-scoped indexes and threshold in lexical scope, and avoiding an unnecessary sort.

# 7. Inline the one-use vertex refinement

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:193-195`, anchor `RefineVertices(prior, folded.Table, rebuilt, policy, next)`
- `libs/dotnet/Rasm/.planning/Spatial/naming.md:246-257`, anchor private `Naming.RefineVertices`

### From

```csharp
static NameTable RefineVertices(NameTable prior, NameTable table,
    CanonicalTopology rebuilt, NamingPolicy policy, Generation next) =>
    rebuilt.Entities.Filter(static e => e.Kind == EntityKind.Vertex)
        .Fold(table, (acc, entity) => acc.VertexNames.Find(entity.Self).Bind(acc.Entries.Find)
```

### To

```csharp
NameTable refined = rebuilt.Entities.Filter(static entity => entity.Kind == EntityKind.Vertex)
    .Fold(folded.Table, (table, entity) => folded.Vertices.Find(entity.Self)
        .Bind(table.Entries.Find)
        .Map(entry => (Entry: entry, Star: toSeq(entity.IncidentVertices.AsIterable())
            .Map(folded.Vertices.Find).Somes()))
```

```csharp
.Match(Some: bound => table with { Entries = table.Entries.SetItem(bound.Entry.Name,
    bound.Entry with { Boundary = bound.Star,
        Parent = bound.Entry.Born == next && bound.Entry.Parent.IsNone
            ? Parent(entity.Kind, bound.Star) : bound.Entry.Parent }) },
    None: () => table));
```

```csharp
// Naming.RefineVertices DELETED
```

### Why

Refinement is the immediate second phase of the same build and has one call site. Inlining deletes the private member, reuses the transient vertex map explicitly, and updates only the authoritative entry map instead of routing through the deleted index-maintenance wrapper.

# 8. Admit independent track inputs and make generation advance total

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:77-83`, anchor `Generation.Next`
- `libs/dotnet/Rasm/.planning/Spatial/naming.md:178-185`, anchors `Naming.Apply` and `op.Switch`

### From

```csharp
public Fin<Generation> Next(Op key) =>
    key.AcceptValidated<Generation>(candidate: Value + 1);
```

```csharp
track: static (k, t) => t.Prior.Generation.Next(key: k)
    .Bind(next => Anchor(t.Prior, t.Rebuilt, next,
        t.Policy.IfNone(NamingPolicy.Canonical), k)),
resolve: static (k, r) => Anchor(NameTable.Empty, r.Boundary,
    Generation.Create(0), NamingPolicy.Canonical, k));
```

### To

```csharp
public Fin<Generation> Next(Op key) => Value == int.MaxValue
    ? Fin.Fail<Generation>(key.InvalidResult())
    : key.AcceptValidated<Generation>(candidate: Value + 1);
```

```csharp
track: static (k, t) =>
    (k.AcceptInput(t.Prior).ToValidation(), k.AcceptInput(t.Rebuilt).ToValidation())
        .Apply(static (prior, rebuilt) => (Prior: prior, Rebuilt: rebuilt)).As().ToFin()
        >> (input => input.Prior.Generation.Next(k)
            >> (next => Anchor(input.Prior, input.Rebuilt, next,
                t.Policy.IfNone(NamingPolicy.Canonical), k))),
```

```csharp
resolve: static (k, r) => k.AcceptInput(r.Boundary)
    >> (boundary => Anchor(
        new NameTable(HashMap<TopoName, NameEntry>.Empty, Generation.Create(0)),
        boundary, Generation.Create(0), NamingPolicy.Canonical, k)));
```

### Why

The prior table and rebuilt topology are independent evidence, so `Validation<Error,T>` plus tuple `Apply` accumulates both admission failures before the one `.ToFin()` exit. Advancing the generation depends on admitted prior state, so `>>` owns that sequence without a nested `Bind` ladder. `Next` remains on the genuine `Generation` owner, but explicitly refuses `int.MaxValue` before addition so overflow cannot escape the result carrier as an exception.

# 9. Finish through the acceptance oracle and LanguageExt error aggregate

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:193-196`, anchor `return folded.Collisions.IsEmpty`

### From

```csharp
return folded.Collisions.IsEmpty
    ? Fin.Succ(RefineVertices(prior, folded.Table, rebuilt, policy, next))
        .Bind(refined => guard(refined.IsValid, key.InvalidResult()).ToFin().Map(_ => refined))
    : Validation.Fail<Error, NameTable>(folded.Collisions).ToFin();
```

### To

```csharp
return folded.Collisions.IsEmpty
    ? key.AcceptValue(refined)
    : Validation.Fail<Error, NameTable>(Error.Many(folded.Collisions)).ToFin();
```

### Why

`Op.AcceptValue` is the branch oracle for `IValidityEvidence`, so the local `guard` duplicates its gate. `Error.Many(Seq<Error>)` is LanguageExt's native aggregate for the collisions the stateful fold already collected; wrapping that one aggregate in `Validation<Error,NameTable>` preserves the declared accumulating-to-short-circuit carrier boundary and fixes the current attempt to pass `Seq<Error>` where the failure type is `Error`.

# 10. Close construction-only identity factories

### Location

- `libs/dotnet/Rasm/.planning/Spatial/naming.md:87-94`, anchor `TopoSignature.Of`
- `libs/dotnet/Rasm/.planning/Spatial/naming.md:110-116`, anchor `TopoName.Mint`

### From

```csharp
public static TopoSignature Of(EntityKind kind, Seq<TopoName> incidentNames,
    Seq<int> kindHistogram) =>
```

```csharp
public static TopoName Mint(EntityKind kind, Arr<int> canonical, Generation born) =>
```

### To

```csharp
internal static TopoSignature Of(EntityKind kind, Seq<TopoName> incidentNames,
    Seq<int> kindHistogram) =>
```

```csharp
internal static TopoName Mint(EntityKind kind, Arr<int> canonical, Generation born) =>
```

### Why

Both generated value objects are genuine identities used outside their minting body, but only `Naming` constructs them under the canonical preimage law. Closing the two factories removes unsupported callable surface without deleting either identity, moving it behind a wrapper, or weakening its type distinction.
