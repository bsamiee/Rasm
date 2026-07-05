# [RASM_TOPOLOGY_NAMING]

Persistent topological naming that survives rebuilds behind ONE entry: `Fin<NameTable> Naming.Apply(NamingOp, Op? key = null)` folds the closed `NamingOp` `[Union]` — `Track(prior, rebuilt)` re-anchoring names across a rebuild at `prior.Generation.Next()`, `Resolve(boundary)` admitting the generation-zero table for a first build — over one `TopoName` lineage algebra spanning every `EntityKind`. The name is a content-address-derived `UInt128` but it is a REFERENCE identity (which entity, lineage-stable across generations), orthogonal to the content identity the reconciliation sibling bridges to the Persistence `GeometryHash`. The page composes `Rasm.Meshing` `MeshSpace` and the native `Mesh` topology surface as settled vocabulary — read, never re-mint — and consumes the `CanonicalTopology` the reconciliation sibling emits inside this same `Rasm.Spatial` namespace. Fault routing is the `naming` cluster 2404-2407: a non-injective re-anchor routes `GeometryFault.NameCollision` 2404, and the internal fold ACCUMULATES every collision as `Validation<Error, NameTable>` before exiting the one `Fin` rail, so a defective rebuild reports its whole collision set in one verdict.

`TopoName` is a reference identity that never crosses a transport; `EntityKind`, the `TopoName` value object, `NamingOp`, and the lineage records are interior types that never sit between wire and rail.

## [01]-[INDEX]

- [01]-[TOPO_NAMING]: ONE `Naming.Apply(NamingOp, Op?)` entry; `TopoName` lineage algebra; `NameTable` generation registry (fingerprint buckets + boundary postings + `Self`-keyed vertex rows) with `IValidityEvidence` registration; overlap-fraction migration under `NamingPolicy` with the vertex star-refine pass; accumulating injectivity audit.

## [02]-[TOPO_NAMING]

- Owner: `EntityKind` `[SmartEnum<int>]` the entity-modality discriminant (`Vertex`/`Edge`/`Face`) carrying the per-kind signature-arity column — the discriminant `solver.md` re-anchors against and the `IndexMismatch` fault payload names; `NamingOp` `[Union]` the request algebra (`Track` · `Resolve`) the ONE `Naming.Apply` entry folds; `NamingPolicy` the migration policy row (`MigrationOverlap` — the shared-name overlap fraction a re-anchor must reach to inherit lineage); `TopoSignature` the rebuild-invariant topological fingerprint a name re-anchors against (boundary-name multiset + incident-kind histogram, WL-1-refined with lexicographically sorted star-neighbor kind histograms on the arity-0 vertex row, position-free so a rigid move keeps the signature) carrying `Of` and the `Overlap` shared-name-fraction predicate; `TopoName` `[ValueObject<UInt128>]` the stable lineage reference — one naming algebra over every `EntityKind`, the modality lives in the `Kind` column of the `NameEntry` row, never a `VertexName`/`EdgeName`/`FaceName` parallel triple; `Generation` the monotone rebuild counter; `NameEntry` the lineage record (name + kind + birth/last-seen generations + parent provenance + signature + resolved boundary-name set + canonical bytes) with the derived `Outcome` lineage classification; `RebuiltEntity` the per-entity re-anchor input carrying its OWN index as the explicit `Self` column beside its incidence; `NameTable` the immutable registry keyed by `TopoName` with the smallest-name-sorted fingerprint BUCKET index (`SignatureIndex` — indistinguishable entities share fingerprints legitimately), the inverted boundary-name POSTING index (`BoundaryIndex` — the migration candidate gather), and the topology-vertex-index→name row (`VertexNames`), registered `IValidityEvidence`; `Naming` the static entry surface.
- Cases: `EntityKind` rows `Vertex` (arity 0) · `Edge` (arity 2) · `Face` (`SignatureArity` `-1` sentinel for the variadic boundary cycle) (3); `NamingOp` cases `Track(prior, rebuilt)` · `Resolve(boundary)` (2); `TrackOutcome` cases `Survived` · `Migrated` · `Born` (3) — DERIVED from the `NameEntry` lineage columns through `Outcome(generation)`, never a parallel carrier threaded beside the fold (the retired build-then-discard outcome threading is the deleted dead weight).
- Entry: `public static Fin<NameTable> Naming.Apply(NamingOp op, Op? key = null)` — the ONE entry over both modalities. `Track(prior, rebuilt)` walks every rebuilt entity, resolves its `TopoSignature` against the prior signature index, and emits the next-generation table at `prior.Generation.Next()` — the explicit `Generation` parameter of the retired `TopoNaming.Track` triple is the deleted knob: monotonicity holds by construction, never by caller discipline. `Resolve(boundary)` is the genesis modality — the SAME fold against `NameTable.Empty` at generation zero, every entity `Born` with a content-stable minted name — so a first build and a re-anchor are one body discriminated by the op case, never two entrypoints. Collisions ACCUMULATE: the fold collects every non-injective re-anchor as a `GeometryFault.NameCollision(name, kind)` row into `Validation<Error, NameTable>` and exits `.ToFin()`, so the public rail stays `Fin<NameTable>` while a defective rebuild reports its COMPLETE collision set in one verdict (the fail-fast first-collision fold is the deleted form); the emitted table additionally gates its own `IValidityEvidence` fold before emission. `TopoName.Mint(kind, canonicalBytes, born)` mints the lineage-root name as a kernel `ContentHash`-derived `UInt128` (seed-zero `XxHash128`) over `(kind ordinal · born generation · canonical entity bytes)` so a fresh name is content-stable but lineage-distinct across generations.
- Auto: the fold walks entities in `EntityKind` order — vertices first, and LOAD-BEARINGLY so: an edge/face resolves its incident topology-vertex indices through the IN-PROGRESS next-generation `NameTable.ResolveBoundary` (the `VertexNames` row its endpoints just anchored), so a genesis signature carries real names and a generation-g signature compares like-with-like against the generation-(g−1) index (resolving rebuilt indices against the PRIOR table's `VertexNames` is the deleted cross-generation index confusion — prior `Self` keys never bind rebuilt indices, and it left every genesis signature empty-boundary degenerate). The `SignatureArity` column gates the boundary feed: the arity-0 vertex row takes NO boundary names — its star feeds the WL-1 FINGERPRINT instead (`Fingerprint` appends the lexicographically sorted star-neighbor kind histograms to the entity's own histogram, separating same-valence vertices by neighborhood without names or coordinates, identically computed at both generations — the sort is full-array, so a new `EntityKind` row widens every histogram and the refinement absorbs it) — while edges (arity 2) and faces (variadic −1) feed their resolved name multisets. The exact-match probe reads the prior `NameTable.SignatureIndex` bucket (`HashMap<EntityKind, HashMap<TopoSignature, Seq<TopoName>>>`, smallest-name-sorted) claimed-filtered — one hash-map lookup per rebuilt entity, never a quadratic cross-scan; topologically indistinguishable entities legitimately share a fingerprint, so a symmetric mesh assigns its bucket deterministically (canonical walk order × smallest unclaimed name) and an EXHAUSTED bucket falls through to migration/birth as growth, never a false collision. The registry keys `VertexNames` by the entity's OWN `RebuiltEntity.Self` index — the retired `IncidentVertices[0]` keying was the star-minimum MIS-KEY (a vertex's incidence array holds its NEIGHBORS, so element zero named the smallest star neighbor, silently corrupting every downstream boundary resolution) and is dead. An exact `TopoSignature` hit is `Survived` (keep the name, bump `LastSeen`); a miss searches migration parents by SHARED-NAME OVERLAP FRACTION — `TopoSignature.Overlap` = `|prior ∩ rebuilt| / min(|prior|, |rebuilt|)` over the sorted boundary-name multisets, thresholded by `NamingPolicy.MigrationOverlap`, candidates gathered from the prior `BoundaryIndex` posting lists (only an entity sharing a boundary name can clear a >0 floor) — taking the maximum-overlap candidate under a deterministic smallest-`TopoName` tiebreak (a face split's children each overlap the parent at fraction 1.0, a merge's parent overlaps the merged child at 1.0, and a partial re-anchor clears the policy floor or is `Born`); a CLAIMED SURVIVOR REMAINS ELIGIBLE as a migration parent — provenance is lineage, not a name claim — and injectivity holds because a `Migrated`/`Born` entry carries a FRESH mint whose `born` generation is folded into the hash (a fresh name can never equal a prior-generation name, and a duplicate fresh mint — two identical-byte entities in one rebuild — is caught by the claimed-set guard and accumulated as a collision). The `RefineVertices` post-pass closes the fold: with every vertex anchored, each vertex's star resolves through the COMPLETED `VertexNames` and lands as its stored `Boundary` (the next generation's migration material, posted into `BoundaryIndex`), and a generation-fresh orphan vertex gains star-overlap `Parent` provenance — an edge collapse's absorbed-into vertex migrates from the prior vertex it subsumed instead of orphaning. The single `EntityKind` discriminant means one fold body serves vertex, edge, and face re-anchoring — the signature arity is a row column, not three fold copies.
- Receipt: `Apply` returns the next-generation `NameTable` directly — the registry IS the receipt: birth/last-seen generations and parent provenance are the per-name lineage evidence, `NameEntry.Outcome(table.Generation)` derives the `Survived`/`Migrated`/`Born` classification from those columns on demand, and `NameTable : IValidityEvidence` declares one `ValidityClaim.All` fold (fingerprint-bucket membership totals the entry count exactly — the standing injectivity witness, every registered name indexed once; every vertex row resolves to a registered entry; lineage generations ordered `Born ≤ LastSeen ≤ Generation`) the entry gates before emission; no parallel tracking ledger.
- Packages: `Rasm.Meshing` (`MeshSpace`, native `Mesh` topology — composed), `Rasm.Domain` (`ContentHash.Of` seed-zero — the ONE hasher, no second; `Op` key rail; `ValidityClaim`/`IValidityEvidence`), `Rasm.Numerics` (`GeometryFault` band 2400), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Validation` accumulation/`HashMap`/`Seq`/`Option`), BCL inbox.
- Growth: a new entity modality is one `EntityKind` row with its signature-arity column and one signature-builder arm; a new lineage outcome (e.g. `Merged` for a face-merge re-bind) is one `TrackOutcome` case plus one `Outcome` projection arm; a new migration disambiguator is one `NamingPolicy` column read by the parent search; a new op modality is one `NamingOp` case on the same `Apply` fold; zero new surface — never a fourth `*Name` value object.
- Boundary: `TopoName` is the ONE naming value object over all entity kinds and a `VertexName`/`EdgeName`/`FaceName` triple is the deleted form — the modality is the `Kind` column; `TopoSignature` is position-free (built from incident NAMES and kind histograms, never coordinates) so a rigid transform preserves every name and only an adjacency change re-anchors — exactly the `GeometryHash` morph-vs-topology-break distinction read from the same canonical adjacency; the migration predicate is the `Overlap` shared-name fraction under the `NamingPolicy.MigrationOverlap` threshold and the retired strict-subset `Subsumes` test is the deleted form TWICE OVER — its subset direction was inverted for splits (it demanded parent ⊂ child where a split child's resolved boundary is a subset of the PARENT's), and a strict-subset predicate cannot rank partial re-anchors at all; `VertexNames` keys by `RebuiltEntity.Self` and the `IncidentVertices[0]` star-minimum key is the named mis-key defect; boundary names resolve through the NEXT-generation table under the vertices-first walk and a prior-table resolution of rebuilt indices is the named cross-generation confusion defect; the exact re-anchor reads the prior `SignatureIndex` buckets and the migration search gathers candidates from the prior `BoundaryIndex` posting lists — a per-rebuild O(n·m) cross-scan on EITHER lane is the named defect; a fingerprint bucket resolves claimed-filtered smallest-name-first and a single-slot signature map that overwrites same-fingerprint siblings is the deleted form; `Apply` is total over the `Fin` rail and a thrown collision is forbidden — non-injective resolutions route `GeometryFault.NameCollision` (band 2400, `Numerics/faults.md`, lowered through `ToError()`), accumulated internally as `Validation<Error, NameTable>` with `Fin` the public entry rail (the accumulating carrier is the internal injectivity-proof shape, never a second public rail); the name is a content-address-derived `UInt128` REFERENCE identity, orthogonal to the `GeometryHash` CONTENT identity — minting a second content hash here is the deleted form, and `TopoName.Value` is NEVER equality-tested against a `NameAddress.ContentHash` even though both are raw `UInt128` (the reference and content axes never compare cross-axis); the `Migrated` parent is resolved from the stored `NameEntry.Boundary` column (never re-derived) under maximum overlap with the deterministic smallest-`TopoName` tiebreak so a split-into-n selects one stable lineage parent independent of `HashMap` enumeration order; the `NameTable` is immutable and `Apply` returns the next generation, never an in-place mutation of the prior table.

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

    // Shared-name overlap fraction |a ∩ b| / min(|a|, |b|) over the sorted boundary multisets:
    // 1.0 for a subset in EITHER direction (split child ⊂ parent, parent ⊂ merged child), the
    // policy-thresholded migration predicate — the strict-subset Subsumes test is the deleted form.
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
    // The lineage classification is DERIVED from the columns — never a parallel carrier.
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

    // A fingerprint BUCKET, smallest name first: topologically indistinguishable entities share a
    // fingerprint legitimately (a symmetric mesh), so exact-match resolution is claimed-filtered
    // bucket selection — the single-slot map that silently dropped same-fingerprint entries is dead.
    public Seq<TopoName> Resolve(EntityKind kind, TopoSignature signature) =>
        SignatureIndex.Find(kind).Bind(index => index.Find(signature)).IfNone(Seq<TopoName>());

    public Seq<TopoName> ResolveBoundary(ReadOnlySpan<int> incidentVertices) =>
        toSeq(incidentVertices.ToArray()).Map(VertexNames.Find).Somes();

    // VertexNames keys by the entity's OWN Self index — IncidentVertices[0] was the star-minimum mis-key.
    // Bucket and posting inserts are idempotent per name, so the vertex refine re-With never double-registers.
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

    // The accumulating audit: every collision joins ONE Validation failure set — the fold never
    // aborts early, so one verdict reports the COMPLETE non-injective re-anchor set.
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

    // Boundary names resolve through the IN-PROGRESS next-generation table: vertices walk first,
    // so an edge/face reads the names its endpoints JUST anchored — a genesis signature is real
    // and a gen-g signature compares like-with-like against the gen-(g-1) index. The arity-0
    // vertex row takes no boundary (its star feeds the fingerprint, never the name multiset).
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
                // An EXHAUSTED bucket is growth, never a collision: the extra same-fingerprint
                // entity migrates or is born fresh — the false positive on symmetric growth dies.
                None: () => MigrateOrBirth(prior, entity, signature, boundary, next, policy))
            .Bind(entry => state.Claimed.Contains(entry.Name)
                ? Fin.Fail<NameEntry>(new GeometryFault.NameCollision(entry.Name.Value, entity.Kind.Key).ToError())
                : Fin.Succ(entry));
        return bound.Match(
            Succ: entry => (state.Table.With(entry, entity.Self), state.Claimed.Add(entry.Name), state.Collisions),
            Fail: error => (state.Table, state.Claimed, state.Collisions.Add(error)));
    }

    // Lexicographic full-array order: a new EntityKind row widens the histogram and the sort
    // absorbs it — a fixed (h[0], h[1], h[2]) tuple key would silently ignore the new column.
    static readonly IComparer<int[]> HistogramOrder =
        Comparer<int[]>.Create(static (a, b) => a.AsSpan().SequenceCompareTo(b));

    // WL-1 refinement: the vertex fingerprint appends the SORTED star-neighbor kind histograms, so
    // same-valence vertices with different neighborhoods separate without names or coordinates —
    // both generations compute the same refinement, so exact-match compares like-with-like.
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

    // A claimed survivor stays ELIGIBLE as a migration parent — provenance is lineage, not a
    // claim; the child's fresh born-generation mint carries injectivity.
    static Fin<NameEntry> MigrateOrBirth(NameTable prior, RebuiltEntity entity, TopoSignature signature, Seq<TopoName> boundary, Generation next, NamingPolicy policy) {
        Option<TopoName> parent = OverlapParent(prior, entity.Kind, boundary, policy);
        TopoName name = TopoName.Mint(entity.Kind, entity.CanonicalBytes, next);
        return Fin.Succ(new NameEntry(name, entity.Kind, next, next, parent, signature, boundary, entity.CanonicalBytes));
    }

    // Posting-list candidate gather: only a prior entity SHARING a boundary name can clear a >0
    // overlap floor, so the full-registry cross-scan per miss is the deleted form.
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

    // Vertex refine post-pass: with EVERY vertex anchored, each vertex's star resolves through the
    // COMPLETED VertexNames — the star becomes the stored Boundary (the next generation's migration
    // material), and a generation-fresh orphan gains star-overlap Parent provenance (an edge
    // collapse's absorbed-into vertex migrates from the prior vertex it subsumed). Names never change here.
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

## [03]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes.

| [INDEX] | [AXIS/CONCERN]              | [OWNER]                | [KIND]                                                                                          | [RAIL]                                       | [CASES] |
| :-----: | :-------------------------- | :--------------------- | :----------------------------------------------------------------------------------------------- | :--------------------------------------------- | :-----: |
|  [01]   | Entry                       | `Naming`/`NamingOp`    | `[Union]` (`Track`/`Resolve`) folded by ONE `Apply` with `Op? key` threading                      | `Naming.Apply → Fin<NameTable>`                |    2    |
|  [02]   | Entity modality             | `EntityKind`           | `[SmartEnum<int>]` Vertex/Edge/Face + signature-arity column                                      | discriminant (pure)                            |    3    |
|  [03]   | Re-anchor outcome           | `TrackOutcome`         | `[Union]` Survived/Migrated/Born — derived via `NameEntry.Outcome(generation)`                    | projection (pure)                              |    3    |
|  [04]   | Migration policy            | `NamingPolicy`         | policy row (`MigrationOverlap` threshold), `Canonical` default                                    | value (threaded by the op case)                |    —    |
|  [05]   | Topological fingerprint     | `TopoSignature`        | `[ValueObject<UInt128>]` position-free name/kind digest (WL-1 vertex refinement) + `Overlap`      | `TopoSignature.Of → TopoSignature` (pure)      |    —    |
|  [06]   | Stable lineage reference    | `TopoName`             | `[ValueObject<UInt128>]` one naming algebra over all kinds + generation-salted `Mint`             | `TopoName.Mint → TopoName` (pure)              |    —    |
|  [07]   | Naming registry             | `NameTable`            | immutable registry + fingerprint buckets + boundary postings + `Self`-keyed `VertexNames` row, `IValidityEvidence` | value (returned in the `Apply` rail)           |    —    |

## [04]-[RESEARCH]

- [REANCHOR_INJECTIVITY] — `Naming.Apply` is total over the `Fin` rail and reports EVERY non-injective re-anchor in one verdict: the fold accumulates each `GeometryFault.NameCollision` into `Validation<Error, NameTable>` and exits `.ToFin()`, so the harness reads the complete collision set, never the first. Injectivity holds structurally on every lane: Survive×Survive cannot double-claim because bucket resolution is claimed-filtered (an exhausted fingerprint bucket falls through to migration/birth as GROWTH — the false collision on a symmetric mesh gaining one more indistinguishable entity is the deleted verdict); Survive×Migrate is resolved by lineage — a claimed survivor still seeds sibling `Parent` provenance because provenance is lineage rather than a name claim, while the migrated child's name is a FRESH `TopoName.Mint` whose `born` generation is folded into the digest, making a fresh-versus-prior collision unrepresentable. The two remaining collision routes are REAL defects: a duplicate fresh mint (two byte-identical entities in one rebuild, caught by the claimed-set guard on the minted name) and a bucket hit whose registry entry is absent (a corrupt prior table). The tier-2 property harness drives the rebuild-matching validation: generate a mesh, apply a labelled topological operation (rigid move → all names `Survived` — vertex fingerprints are position-free and the WL-1 star-histogram refinement plus canonical walk order keeps every bucket assignment stable; face split → each child `Migrated` with the parent's provenance under overlap fraction 1.0; vertex insert → `Born`; edge collapse → the collapsed edge and consumed vertex absent, each rewired edge `Migrated` at overlap 0.5, and the absorbed-into vertex `Migrated` from the prior vertex it subsumed via the `RefineVertices` star-overlap pass — never an orphaned `Born`), classify each entry through `NameEntry.Outcome(table.Generation)`, and assert the next-generation `NameTable` is injective per `EntityKind` — the registered `NameTable.IsValid` claim set (fingerprint-bucket membership totalling the entry count, ordered lineage generations, resolvable vertex rows) is the standing machine-checked witness the entry gates on every `Apply`. The migration disambiguator under test is `TopoSignature.Overlap` — the shared-name overlap fraction over stored `NameEntry.Boundary` columns (read directly, never re-derived; vertex stars stored by the refine pass so the vertex tier carries migration material too), candidates gathered from the `BoundaryIndex` posting lists, max-overlap selection, `NamingPolicy.MigrationOverlap` floor, and the deterministic smallest-`TopoName` tiebreak so a split-into-n parent selection is stable across runs regardless of `HashMap` enumeration order.
