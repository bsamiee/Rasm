# [PERSISTENCE_VERSION_RETENTION]

`ArtifactKind` binds the asset class every durable artifact admits under and DERIVES its `RetentionClass`, which in turn binds storage lane, schedule, classification ceiling, loss policy, and identity scheme. `RetentionCatalog` owns admission. `RetentionSweep` derives one conserved verdict partition, marks reachability from reconstructed cuts or Marten event tags, and executes every eviction or cold-tier demotion through one receipt rail. Full-history reachability shields referenced content; `OrphanAge` gates never-referenced debris.

## [01]-[INDEX]

- [02]-[RETENTION_CLASSES]: the asset-class axis every catalog derives retention from, the closed class axis, the five-decision class row, the seam-local classification-ceiling rank table, identity-scheme behavioral families, and the budget/loss policies.
- [03]-[SWEEP_AND_GC]: the pure state-threaded verdict fold, its row-or-partition grain, first-class holds, the full-history reachability GC, and the one receipted deletion executor every lane routes through.

## [02]-[RETENTION_CLASSES]

- Owner: `ArtifactKind` the `[SmartEnum<string>]` ASSET-CLASS axis every durable artifact admits under, carrying its derived `RetentionClass` and its `CacheTier` recency lane and owning the two provenance selectors (`Texture`, `Representation`) that answer both the row and the origin key one producer value projects; `CacheTier` the two-row recency-lane vocabulary the app platform reads settled; `RepresentationSlot` the bounded `RepresentationContentHash` slot roster whose deferred column names each slot's kind; `RetentionClass` the `[SmartEnum<string>]` lifecycle axis carrying its five decisions (storage lane, retention schedule, classification ceiling, loss policy, identity scheme); `StorageLane` the `[SmartEnum<string>]` durable-home axis; `LossPolicy` the receipted-evict/declared-expiry/never-evict vocabulary whose BOTH columns the sweep dispatch consumes; `IdentityScheme` the content-keyed-versus-name-plus-epoch vocabulary carrying the `Identity` mint (the name-plus-epoch key derives through the kernel `ContentHash.Of` seed-zero entry); `RetentionCeiling` the static frozen sensitivity-rank table that supplies the ordering `DataClassification` does not carry; `RetentionFault` the closed admission fault; `RetentionCatalog` the static surface owning the one-fold admission (classify-check, identity-derive, race-admit, lane-write).
- Cases: each artifact family is one `ArtifactKind` row carrying its retention class and recency lane, and a family whose retention derives from PROVENANCE is two rows behind one selector reading the discriminant off a value the producer already holds — `Texture(planKey)` answers `TextureSet` (`Cache`, rebuildable from its recorded graph/plan/seed triple) or `TextureAcquired` (`Blob`, durable because a retired model card and a drifted execution provider make the bytes unreproducible), and `Representation(slot, bodyKey)` answers the lossless `RepresentationBox` (`Blob`, the authority no fold reproduces) against the derived `RepresentationBody`/`Axis`/`Footprint` (`Cache`, re-tessellated from that body) and the measured `CoverageRaster` (`Blob`, an observation no fold reproduces), each beside the origin key the admission records; the canonical retention set closes at six rows — `snapshot` (sealed AS-OF snapshot, `SnapshotArchive` lane, content-keyed, newest-N), `stream` (Marten event stream, `EventStream` lane, append-only, never evicted), `blob` (durable artifact bytes, `ObjectStore` lane, content-keyed, never-evict cold-tiering, full-history-reachable), `evidence` (incident bundle, `SnapshotArchive` lane, name-plus-epoch, declared-expiry), `cache` (transient content blob, `ObjectStore` lane, content-keyed, receipted-evict), `ephemeral` (presence/awareness, `Transient` lane, name-plus-epoch, declared-expiry, never durable); a class fitting no row is an admission rejection, never a default; class membership is immutable, reclassification is export-then-readmit so every lived lifecycle stays receipted.
- Entry: `public static Fin<RetentionFact> Admit(RetentionClass cls, ContentAddress contentKey, string name, ulong epoch, DataClassification stamp, long bytes, StorageTier tier, Func<ContentAddress, bool> resident, ProjectionContext frame)` is the one admission fold (the artifact's current `StorageTier` rides into the fact so the sweep cold-tiering verdict reads it; the catalog key is minted IN the fold through `cls.Scheme.Identity(contentKey, name, epoch)` — the content-keyed scheme passes the content address through and reads neither `name` nor `epoch`, the name-plus-epoch scheme derives `ContentHash.Of(name ++ epoch)` — so a caller never pre-mints identity); `public RetentionSchedule Schedule { get; }` projects the class's sweep cadence and budget; `public static bool RetentionCeiling.Ranked(DataClassification stamp)` is the fail-closed mapped-ness gate and `public static bool RetentionCeiling.Admits(DataClassification stamp, DataClassification ceiling)` the seam-local sensitivity comparison the admission fold reads directly (no per-class forwarder, no `Exceeds`).
- Auto: admission is one fold — classify-check (an UNRANKED stamp — a newer upstream `DataClassification` tier this seam rank table has not yet ordered — rejects `Unstamped` BEFORE the compare because absence of a seam rank is not clearance; a ranked stamp exceeding the ceiling rejects `CeilingBreach`), identity-derive (`cls.Scheme.Identity` — the content-keyed scheme IS its content address, the name-plus-epoch scheme mints its catalog key from the stable name plus admission epoch through the ONE kernel `ContentHash.Of` seed-zero entry, never a second hasher), race-admit (content-keyed classes get dedup and race-loser disposal free, name-plus-epoch classes get versioned replacement free, zero conditional code), lane-write; the sensitivity rank is a frozen `RetentionCeiling` table keyed by `DataClassification` because the AppHost taxonomy carries only a `RedactorKind` column and no ordinal — the ordering the "escalating sensitivity" doctrine asserts lives HERE as a policy value, never re-derived per call; byte counts record from the artifact's own sealed length fields (`SnapshotCatalogRow.StoredLength`, `ChunkManifest.Length`, `BlobResidence.Length`), never a later filesystem stat.
- Receipt: an admission rides `store.retention.admit` carrying the class and bytes; a ceiling breach rides `store.retention.reject` carrying the stamp and ceiling; an unranked stamp rides `store.retention.reject` carrying the key.
- Packages: Marten (`EventTagQuery`/`QueryByTagsAsync` adapter), Rasm (`Rasm.Domain` `ContentHash.Of` — the name-plus-epoch identity mint, [B]), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` rows + `Switch` + `Items`), LanguageExt.Core (`Fin`/`Option`), NodaTime (`Duration`/`Instant`), BCL inbox.
- Growth: a new asset family is one `ArtifactKind` row carrying its retention class and recency lane, and a family whose retention derives from provenance is two rows behind one selector returning the row beside the origin key its discriminating value also is; a new representation slot is one `RepresentationSlot` row with its deferred kind column; a new lifecycle class is one `RetentionClass` row carrying its five decisions; a new durable home is one `StorageLane` row; a new loss policy is one `LossPolicy` row; a new sensitivity tier is one `RetentionCeiling` rank entry keyed by the AppHost `DataClassification` row; a new cold-tier rung is one `RetentionCeiling.Colder` ladder entry over the blobstore `StorageTier` vocabulary; zero new surface — a per-artifact retention table, a second classification taxonomy, a STORED retention column beside the kind that derives it, an origin flag beside the value that already discriminates, an ordinal added to `DataClassification` upstream (the rank is a Persistence policy, not an AppHost concern), a `StorageTier`-ordering owner duplicated from blobstore, or a default class for an unfit artifact is the deleted form because the class set is closed and admission is one fold over the five decisions.
- Boundary: the asset class is the DISCRIMINANT and retention its derived column, so every catalog that stores bytes stores the kind and reads `Kind.Retention` — a second catalog storing the class beside a kind is the drift this seating deletes, and the taxonomy seats HERE rather than at the read index because the object-plane catalog and the artifact index are two strata peers and a concept two peers reach seats at the lowest stratum either reaches; every stored thing belongs to exactly one class row carrying five decisions, the storage lane naming its durable home (`SnapshotArchive` for sealed artifacts, `EventStream` for the Marten system of record, `ObjectStore` for content-keyed blobs and cache, `Transient` for awareness) so the sweep budgets and the deletion executor route by lane row, never a `cls.Key == "blob"` string compare; the identity scheme alone yields two complete behavioral families (content-keyed classes get dedup and race-loser disposal free, name-plus-epoch classes get versioned replacement free); a budget breach truncates with an embedded receipt (capture must succeed degraded) while a ceiling breach rejects outright (security never degrades), the two overflow responses never interchangeable; the sensitivity comparison is seam-local because `DataClassification` is the AppHost redaction taxonomy whose only column is its `RedactorKind` — `RetentionCeiling.Admits` supplies the rank rather than calling a non-existent `Exceeds`, and an UNRANKED classification (a newer upstream tier this seam table has not ordered) is rejected as a distinct `Unstamped` fault, fail-closed, rather than silently collapsed to `int.MaxValue` and reported as a `CeilingBreach` that never compared — absence of a seam rank is not clearance; import re-verifies stamps so an export round-trip cannot launder a ceiling; the `stream` class is append-only and never evicted because the Marten event stream is the system of record (only the AS-OF snapshot density and the blob reachability are reclaimable); the `blob` class is content-keyed, `NeverEvict`, and full-history-reachable so an artifact blob a historical version references is never collected — an aged one cold-tiers down the `StorageTier` ladder and only never-referenced crash debris collects through the orphan pass (`#SWEEP_AND_GC`) — and the `Store/blobstore#BLOB_GC` lane registers its `BlobCatalogRow` in this class so the one GC governs both the snapshot spine and the object plane.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StorageLane {
    public static readonly StorageLane SnapshotArchive = new("snapshot-archive", durable: true);
    public static readonly StorageLane EventStream = new("event-stream", durable: true);
    public static readonly StorageLane ObjectStore = new("object-store", durable: true);
    public static readonly StorageLane Transient = new("transient", durable: false);
    public bool Durable { get; }
    private StorageLane(string key, bool durable) : this(key) => Durable = durable;
}

[SmartEnum]
public sealed partial class LossPolicy {
    public static readonly LossPolicy ReceiptedEvict = new(evicts: true, expires: false);
    public static readonly LossPolicy DeclaredExpiry = new(evicts: false, expires: true);
    public static readonly LossPolicy NeverEvict = new(evicts: false, expires: false);
    public bool Evicts { get; }
    public bool Expires { get; }
    // The dispatch shield column: reachability protects only a class that can collect; the never-evict
    // arm consumes the mark itself as its orphan/cool discriminant.
    public bool Collects => Evicts || Expires;
    private LossPolicy(bool evicts, bool expires) => (Evicts, Expires) = (evicts, expires);
}

[SmartEnum]
public sealed partial class IdentityScheme {
    public static readonly IdentityScheme ContentKeyed = new(dedups: true);
    public static readonly IdentityScheme NamePlusEpoch = new(dedups: false);
    public bool Dedups { get; }
    // The identity-derive stage made real: a content-keyed artifact IS its content address; a name-plus-epoch
    // artifact (evidence bundle, awareness row) mints its catalog key from the stable name + admission epoch
    // through the ONE kernel seed-zero entry — versioned replacement rides the epoch, never a second hasher.
    public ContentAddress Identity(ContentAddress contentKey, string name, ulong epoch) =>
        Dedups ? contentKey : ContentAddress.Of(ContentHash.Of(Encoding.UTF8.GetBytes($"{name}#{epoch:x16}")));
    private IdentityScheme(bool dedups) => Dedups = dedups;
}

// The retention fault band (828x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless protected ctor;
// `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the Persistence-sibling
// `Element/codec#SNAPSHOT_SPINE` `CodecFault` (83xx) and `Element/identity#SCHEMA_VERDICT` `IdentityFault` (834x) realize
// — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)` ctor (no `Category` to
// override) is the deleted form. Band membership derives `Code => FaultBand.Retention + n` through the registry row
// (`Element/graph#FAULT_TABLES` — a bare integer literal is the deleted form), `Message`/`Category` projecting
// through the generated `Switch`, so the typed case lifts BARE onto `Fin<T>` with no `.ToError()` hop and a recovery
// reads `error.IsType<RetentionFault.Unstamped>()` / `error.HasCode(8283)` / `error.Category`, never a message
// substring. No `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in, so the band carries no
// generated per-case `SelfOp`. `Create` is the IValidationError admission the
// generated converter bridge calls on a deserialization reject — `Unclassed` (an admitted artifact never mints it
// directly); `CeilingBreach` is a RANKED stamp whose seam rank exceeds the class ceiling (the genuine sensitivity
// comparison); `Unstamped` is an UNRANKED stamp the seam table does not order (fail-closed — a `CeilingBreach` reporting
// a comparison that never happened is the deleted form).
[Union]
public abstract partial record RetentionFault : Rasm.Domain.Expected, IValidationError<RetentionFault> {
    private RetentionFault() : base() { }
    public sealed record Unclassed(string Artifact) : RetentionFault;
    public sealed record CeilingBreach(DataClassification Stamp, DataClassification Ceiling) : RetentionFault;
    public sealed record Unstamped(ContentAddress Key) : RetentionFault;

    public override int Code => FaultBand.Retention + Switch(
        unclassed:     static _ => 1,
        ceilingBreach: static _ => 2,
        unstamped:     static _ => 3);

    public override string Message => Switch(
        unclassed:     static c => $"<retention-unclassed:{c.Artifact}>",
        ceilingBreach: static c => $"<retention-ceiling:{c.Stamp}>{c.Ceiling}>",
        unstamped:     static c => $"<retention-unstamped:{c.Key.Value:x32}>");

    public override string Category => Switch(
        unclassed:     static _ => "Unclassed",
        ceilingBreach: static _ => "Ceiling",
        unstamped:     static _ => "Unstamped");

    public static RetentionFault Create(string message) => new Unclassed(message);
}

// `OrphanAge` is the DECLARED orphan-collection age gate the never-evict arm reads — an unreachable artifact
// younger than it is Kept (a write mid-flight, a checkpoint not yet referenced), never an age policy smuggled
// into the key-only eligibility predicate; `Duration.MaxValue` closes the arm structurally (the `stream` SoR).
public readonly record struct RetentionSchedule(Duration Cadence, long ByteBudget, int CountBound, Duration AgeBound, Duration OrphanAge);
// The admitted artifact's measured fact. `Tier` is its CURRENT durable storage tier (the `Store/blobstore#OBJECT_STORE`
// `StorageTier` row the blob/snapshot lane sealed it at), read by the cold-tiering verdict so a `NeverEvict`-class artifact
// past its age demotes one rung instead of evicting and an artifact already at the coldest tier is `Kept` idempotently —
// a `Transient`/`EventStream`-lane fact rides `StorageTier.Standard`, and the `Lane.Durable` gate keeps the demotion
// ladder off a non-durable lane structurally. A fact with no tier field would foreclose the dedup-plus-cold-tiering
// alternative `H10` admits, the deleted thin slice.
// `PartitionSpan` is the PARTITION-grained inventory slot: a family carrying a `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow`
// row sweeps its partitions rather than its rows, so its fact names the partition and the row count one drop
// retires while `Bytes` and `At` keep their fact meaning at that grain (the partition's stored size, its window
// start). A per-row fact carries `None` and every arm below reads exactly as it did.
public readonly record struct PartitionSpan(string Name, int Rows);
public readonly record struct RetentionFact(RetentionClass Class, ContentAddress Key, long Bytes, StorageTier Tier, Instant At, Option<PartitionSpan> Partition = default);

public static class RetentionCeiling {
    static readonly FrozenDictionary<DataClassification, int> Rank = new[] {
        DataClassification.None, DataClassification.Operational, DataClassification.Internal, DataClassification.HostIdentity,
        DataClassification.UserContent, DataClassification.Personal, DataClassification.Confidential, DataClassification.Credential, DataClassification.Secret,
    }.Select(static (row, ordinal) => (row, ordinal)).ToFrozenDictionary(static t => t.row, static t => t.ordinal);

    // The seam-local sensitivity rank `DataClassification` does not carry. `Ranked` is the FAIL-CLOSED admission gate the
    // one fold reads: a stamp absent from the rank (a NEWER upstream `DataClassification` tier this Persistence rank table
    // has not yet ordered) is unmapped — `Admit` rejects it as `Unstamped` (a distinct, diagnosable "this seam does not
    // know this classification" fault), NEVER a silent `int.MaxValue` collapsed into a `CeilingBreach` that reports a rank
    // comparison that never happened. `Admits` compares two MAPPED ranks; the unmapped case is `Ranked`'s own arm.
    static int Of(DataClassification row) => Rank[row];
    public static bool Ranked(DataClassification row) => Rank.ContainsKey(row);
    public static bool Admits(DataClassification stamp, DataClassification ceiling) => Ranked(stamp) && (Of(stamp) <= Of(ceiling));

    // The cold-tiering demotion ladder — a retention POLICY (the lifecycle cadence lives here) over the blobstore-owned
    // `StorageTier` vocabulary: `Standard -> Infrequent -> Cold -> Archive`, `Archive` the floor returning `None`. The
    // `NeverEvict`-class cold-tiering alternative (`H10`: geometry-GC-forbidden = dedup-plus-cold-tiering) demotes one rung
    // per age threshold, idempotent at the floor, so an aged-but-reachable artifact blob colds-tiers rather than collects.
    static readonly FrozenDictionary<StorageTier, StorageTier> Colder = new[] {
        (StorageTier.Standard, StorageTier.Infrequent), (StorageTier.Infrequent, StorageTier.Cold), (StorageTier.Cold, StorageTier.Archive),
    }.ToFrozenDictionary(static t => t.Item1, static t => t.Item2);
    public static Option<StorageTier> Demote(StorageTier tier) => Colder.TryGetValue(tier, out StorageTier next) ? Some(next) : None;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetentionClass {
    public static readonly RetentionClass Snapshot = new("snapshot", StorageLane.SnapshotArchive, LossPolicy.ReceiptedEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, new RetentionSchedule(Duration.FromHours(6), 64L * 1024 * 1024 * 1024, 32, Duration.FromDays(365), Duration.FromDays(7)));
    public static readonly RetentionClass Stream = new("stream", StorageLane.EventStream, LossPolicy.NeverEvict, IdentityScheme.ContentKeyed, DataClassification.Confidential, new RetentionSchedule(Duration.MaxValue, long.MaxValue, int.MaxValue, Duration.MaxValue, Duration.MaxValue));
    public static readonly RetentionClass Blob = new("blob", StorageLane.ObjectStore, LossPolicy.NeverEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, new RetentionSchedule(Duration.FromHours(12), 512L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromDays(90), Duration.FromDays(7)));
    public static readonly RetentionClass Evidence = new("evidence", StorageLane.SnapshotArchive, LossPolicy.DeclaredExpiry, IdentityScheme.NamePlusEpoch, DataClassification.Confidential, new RetentionSchedule(Duration.FromDays(1), 8L * 1024 * 1024 * 1024, 256, Duration.FromDays(90), Duration.FromDays(7)));
    public static readonly RetentionClass Cache = new("cache", StorageLane.ObjectStore, LossPolicy.ReceiptedEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, new RetentionSchedule(Duration.FromHours(1), 16L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromDays(7), Duration.FromHours(24)));
    public static readonly RetentionClass Ephemeral = new("ephemeral", StorageLane.Transient, LossPolicy.DeclaredExpiry, IdentityScheme.NamePlusEpoch, DataClassification.Internal, new RetentionSchedule(Duration.FromMinutes(1), 1L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromMinutes(5), Duration.FromMinutes(10)));

    public StorageLane Lane { get; }
    public LossPolicy Loss { get; }
    public IdentityScheme Scheme { get; }
    public DataClassification Ceiling { get; }
    public RetentionSchedule Schedule { get; }
    private RetentionClass(string key, StorageLane lane, LossPolicy loss, IdentityScheme scheme, DataClassification ceiling, RetentionSchedule schedule) : this(key) =>
        (Lane, Loss, Scheme, Ceiling, Schedule) = (lane, loss, scheme, ceiling, schedule);
}

// The recency lane an admitted artifact's bytes occupy in the app-platform two-tier cache. Two rows because the
// only decision is whether a payload may pin the process-local tier: a small receipt may, a plane pyramid or a
// lossless representation may not. The AppHost projects the settled row and never re-derives a size heuristic.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CacheTier {
    public static readonly CacheTier ModelResult = new("model-result");
    public static readonly CacheTier ArtifactBlob = new("artifact-blob");
}

// The ASSET-CLASS axis every durable artifact admits under, seated BESIDE `RetentionClass` because retention is
// its derived column and both `Store/blobstore#BLOB_GC`'s catalog and `Query/cache#ARTIFACT_BLOB_INDEX`'s index
// read it — two strata peers reaching one concept seat it at the lowest stratum either reaches, so the taxonomy
// lives here and the S3 index composes it downward rather than the S2 store reaching up for it.
// Retention is DERIVED, never stored: a row's `Retention` IS its class, so a catalogued class contradicting its
// kind is unrepresentable at both catalogs and the two can no longer drift. A family whose retention derives from
// PROVENANCE is two rows behind one selector reading the discriminant off a value the producer already holds.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArtifactKind {
    public static readonly ArtifactKind Interchange = new("interchange", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind EpContext = new("ep-context", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind OnnxProfile = new("onnx-profile", RetentionClass.Cache, CacheTier.ModelResult);
    // A floor-provider parity residual re-derives by re-running the canary, so it takes the cache class exactly
    // as the EP-context blob does; the app root binds the Compute `ParityPort` against this row, and Compute
    // names the kind nowhere — the port hides it.
    public static readonly ArtifactKind ParityVerdict = new("parity-verdict", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind IfcSemantic = new("ifc-semantic", RetentionClass.Blob, CacheTier.ArtifactBlob);
    // Reality-capture bytes are an OBSERVATION no fold reproduces — the instrument moved, the site changed, and a
    // re-scan yields different bytes — so the class is Blob exactly as the measured coverage grid's is, and every
    // durable scan row (`Ingest/pointcloud#SCAN_RESIDENCE` header, registration, region) is DERIVED from these bytes.
    public static readonly ArtifactKind Scan = new("scan", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind ChunkContent = new("chunk-content", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind CloudRun = new("cloud-run", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind Assessment = new("assessment", RetentionClass.Cache, CacheTier.ModelResult);
    // Representation families — the object plane's own primary payload, whose five slots carry genuinely different
    // retention economics and therefore five rows rather than one "geometry" lump. The LOSSLESS body is the
    // authority no fold reproduces, so it is `Blob`; the display mesh, the structural line, and the boundary ring
    // are DERIVED projections of it, so they are `Cache` and re-tessellate at compute cost; a measured coverage
    // grid is an observation no fold reproduces, so it is `Blob`. This is the same provenance argument the texture
    // rows make, applied to the family that made the plane exist. Every row takes `ArtifactBlob` — a mesh, a BREP,
    // or a raster pyramid is exactly the oversized payload that must never pin the process-local tier.
    public static readonly ArtifactKind RepresentationBox = new("representation-box", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind RepresentationBody = new("representation-body", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind RepresentationAxis = new("representation-axis", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind RepresentationFootprint = new("representation-footprint", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind CoverageRaster = new("coverage-raster", RetentionClass.Blob, CacheTier.ArtifactBlob);
    // Texture-plane families: retention derives from PROVENANCE, so the concept is two rows and the producer
    // never hand-picks one. A PRESS-BAKED set rebuilds exactly from the (graph key, plan key, seed) triple its
    // own press receipt records, so it is Cache — re-derivable at compute cost. A NEURAL-ACQUIRED set cannot:
    // its model card retires, its execution provider drifts, and re-running the stage reproduces different
    // bytes, so cache-classing it is evidence loss and the row is Blob.
    public static readonly ArtifactKind TextureSet = new("texture-set", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind TextureAcquired = new("texture-acquired", RetentionClass.Blob, CacheTier.ArtifactBlob);
    // HDF5 archive families off the Compute Runtime/codecs archive owner, admitted content-key-only through
    // ArtifactIndexRow like every artifact here. Provenance decides the class: an INGESTED corpus (weather grid,
    // reference bands, Joe-Kuo class resources, external sparse exchange) is bytes no fold reproduces — Blob;
    // a SOLVER-EMITTED history, modal basis, mesh container, ensemble, or checkpoint re-derives from its
    // content-keyed inputs at compute cost — Cache; the graduation envelope and the initializer pack are
    // acquired evidence a retired producer cannot re-mint — Blob.
    public static readonly ArtifactKind ArchiveCorpus = new("archive-corpus", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind ArchiveSolve = new("archive-solve", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind GraduationEnvelope = new("graduation-envelope", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind InitializerPack = new("initializer-pack", RetentionClass.Blob, CacheTier.ModelResult);
    // Fabrication egress families: keys mirror the Rasm.Fabrication EgressKind rows verbatim; federation is
    // content-key-only — no Fabrication type crosses this page.
    public static readonly ArtifactKind CutProgram = new("cutprogram", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind Placement = new("placement", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind Remnant = new("remnant", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind Cli = new("cli", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind ThreeMf = new("threemf", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind Nc1 = new("nc1", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind StockSnapshot = new("stock-snapshot", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind Traveler = new("traveler", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind DigitalProductPassport = new("digital-product-passport", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind FlatPattern = new("flat-pattern", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind BendProgram = new("bend-program", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind WeldPlan = new("weld-plan", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind ScanVectors = new("scan-vectors", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind Plan = new("plan", RetentionClass.Cache, CacheTier.ModelResult);

    public RetentionClass Retention { get; }
    public CacheTier Tier { get; }
    private ArtifactKind(string key, RetentionClass retention, CacheTier tier) : this(key) => (Retention, Tier) = (retention, tier);

    // Provenance-derived texture-set admission: the press PLAN key is present exactly when a press baked the
    // planes and absent on an acquired set, so the discriminant is recoverable from the value the producer
    // already holds and no origin flag rides beside it. A caller naming the row directly can pair acquired bytes
    // with a rebuildable class, which the retention sweep then evicts — the one selector forecloses it.
    // One plan key projects BOTH the class discriminant AND the origin the index records, so the selector hands
    // back that SOURCE KEY beside the row: a `TextureSet` row can never claim rebuildability while its own
    // index carries no origin, and a press family can never strand under per-set content keys.
    // Reading the key twice at a call site is exactly the divergence one return forecloses.
    public static (ArtifactKind Kind, Option<UInt128> Source) Texture(Option<UInt128> planKey) =>
        (planKey.IsSome ? TextureSet : TextureAcquired, planKey);

    // Representation-slot admission: the `Object` node carries one `Option<UInt128>` per slot, so the slot a
    // producer is writing IS the discriminant and the selector returns the row beside the LOSSLESS body key the
    // derived slots regenerate from — a `RepresentationBody`/`Axis`/`Footprint` row admitting with `None` origin
    // would claim re-derivability while its own index names nothing to re-derive from, exactly the split the
    // texture selector forecloses. The `Box` arm returns `None` because the lossless body IS the origin.
    public static (ArtifactKind Kind, Option<UInt128> Source) Representation(RepresentationSlot slot, Option<UInt128> bodyKey) =>
        (slot.Kind(), slot == RepresentationSlot.Box ? None : bodyKey);
}

// The `Graph/element#NODE_MODEL` `RepresentationContentHash` slot roster as a bounded vocabulary, so a producer
// names the slot it holds and the kind derives — a caller passing `ArtifactKind` directly could pair a display
// mesh with the lossless class the sweep then never evicts, which the selector forecloses. The correspondence is
// a DEFERRED column: two smart-enum rosters referencing each other's fields eagerly capture null before either
// materializes, so the row answers behind `static () => Row` and the read supplies the materialization edge.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RepresentationSlot {
    public static readonly RepresentationSlot Box = new("box", static () => ArtifactKind.RepresentationBox);
    public static readonly RepresentationSlot Body = new("body", static () => ArtifactKind.RepresentationBody);
    public static readonly RepresentationSlot Axis = new("axis", static () => ArtifactKind.RepresentationAxis);
    public static readonly RepresentationSlot FootPrint = new("footprint", static () => ArtifactKind.RepresentationFootprint);
    public static readonly RepresentationSlot Coverage = new("coverage", static () => ArtifactKind.CoverageRaster);

    [UseDelegateFromConstructor] public partial ArtifactKind Kind();
}

// The storage lane's own conditional-write verdict: `Stored` fresh bytes, `Replaced` a prior name+epoch version
// (the loser's disposal is the lane receipt's, committed in the same conditional write), `Deduped` a resident
// content key (no bytes moved) — the admission fact reads the COMMITTED outcome, never a pre-write prediction.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LaneOutcome {
    private LaneOutcome() { }
    public sealed record Stored(long Bytes) : LaneOutcome;
    public sealed record Replaced(long Bytes, ulong PriorEpoch) : LaneOutcome;
    public sealed record Deduped : LaneOutcome;

    public long Committed => this.Map(stored: static s => s.Bytes, replaced: static r => r.Bytes, deduped: static _ => 0L);
}

public static class RetentionCatalog {
    // The one admission fold, all four stages IN the fold: classify-check (an UNRANKED stamp fails `Unstamped` fail-closed
    // BEFORE the ceiling compare — absence of a seam rank is not clearance; a ranked-but-exceeding stamp fails `CeilingBreach`),
    // identity-derive (the `cls.Scheme.Identity` mint — the scheme row consumes the ingredients it needs: content-keyed passes
    // the content address through, name-plus-epoch mints off `name`+`epoch`; a caller-preminted key beside a prose-only derive
    // stage is the deleted split-brain), race-admit (a content-keyed class dedups a resident key to a Deduped fact with no
    // write; a name-plus-epoch class drives the lane's conditional write whose receipt names replace-or-fresh and disposes
    // the race loser store-side), lane-write (the injected `write` leg IS the declared StorageLane's conditional write —
    // an Admit that only predicts is the deleted form). The artifact's CURRENT `StorageTier` rides the fact.
    public static IO<Fin<RetentionFact>> Admit(RetentionClass cls, ContentAddress contentKey, string name, ulong epoch, DataClassification stamp, StorageTier tier, Func<ContentAddress, bool> resident, Func<ContentAddress, IO<Fin<LaneOutcome>>> write, ProjectionContext frame) {
        if (!RetentionCeiling.Ranked(stamp)) { return IO.pure(Fin<RetentionFact>.Fail(new RetentionFault.Unstamped(contentKey))); }
        if (!RetentionCeiling.Admits(stamp, cls.Ceiling)) { return IO.pure(Fin<RetentionFact>.Fail(new RetentionFault.CeilingBreach(stamp, cls.Ceiling))); }
        ContentAddress key = cls.Scheme.Identity(contentKey, name, epoch);
        return cls.Scheme.Dedups && resident(key)
            ? IO.pure(Fin<RetentionFact>.Succ(new RetentionFact(cls, key, 0L, tier, frame.Now())))
            : write(key).Map(outcome => outcome.Map(committed => new RetentionFact(cls, key, committed.Committed, tier, frame.Now())));
    }
}
```

| [INDEX] | [POLICY]          | [VALUE]                                             | [BINDING]                                                |
| :-----: | :---------------- | :-------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | class membership  | one of six closed `RetentionClass` rows             | unfit artifact rejects, never defaults                   |
|  [02]   | storage lane      | `StorageLane` row per class                         | sweep/delete route by lane, not a key string compare     |
|  [03]   | sensitivity rank  | seam-local `RetentionCeiling` frozen table          | `DataClassification` carries no ordinal; rank lives here |
|  [04]   | identity scheme   | content-keyed vs name-plus-epoch, `Scheme.Identity` | dedup/race-disposal vs versioned-replacement             |
|  [05]   | ceiling vs budget | ceiling rejects, budget truncates                   | security never degrades; capture succeeds degraded       |
|  [06]   | stream class      | append-only, never evicted                          | the event SoR; only snapshots and blobs reclaim          |

## [03]-[SWEEP_AND_GC]

- Owner: `SweepVerdict` the closed per-artifact verdict carrying the artifact's own byte figure; `PartitionSpan` the partition-grained inventory slot a rolling-window family's fact carries; `Hold` the first-class hold row (whole-class, identity-set, stamp-range selectors composing by union); `SweepReceipt` the run summary proving `inventory = kept + held + cooled + evicted`; `Reachability` the full-history mark surface; `RetentionSweep` the static surface owning the pure state-threaded verdict fold and the one receipted mutation executor (delete plus cold-tier demote).
- Cases: `SweepVerdict` is `Kept | Held | HeldOverBudget | EvictAge | EvictCount | EvictSize | EvictAdministrative | EvictOrphan | DropPartition | Cool`; `DropPartition` is the PARTITION-grained age eviction — a rolling-window family's aged trailing partition leaves whole through the database's own constant-time drop, one verdict carrying the partition name, the rows it retired, and its bytes, its `Key` the ordinary `IdentityScheme.NamePlusEpoch` mint over that name because a partition IS the epoch-grained name-plus-epoch unit; held bytes count against the budget but cannot evict so preservation pressure surfaces as `HeldOverBudget` rather than displacing onto unheld artifacts; `EvictAdministrative` is the operator-deletion verdict so a manual purge rides the same single executor and receipt stream rather than a side channel; `EvictOrphan` is the never-evict class's ONLY collection — an unreachable artifact no AS-OF cut ever referenced (crash debris, a race-loser row) past the class's DECLARED `RetentionSchedule.OrphanAge`, so a `NeverEvict` artifact younger than its orphan threshold stays non-evictable, `H10` holds for every referenced blob, and the crash-window loop still closes; `Cool` is the never-evict cold-tiering verdict (`H10`'s geometry-GC-forbidden alternative made real) carrying the `From`/`To` `StorageTier` so an aged never-evict artifact demotes one rung rather than collects or keeps-forever; every evict verdict and `HeldOverBudget` carries both the `Key` and the artifact `Bytes` so the executor emits a truthful per-eviction byte figure and the run summary sums real reclaimed bytes; the reachability mark is `Reachable | Orphan` over the full-history cut set.
- Entry: `public static (Seq<SweepVerdict> Verdicts, SweepReceipt Receipt) Run(RetentionClass cls, Seq<RetentionFact> inventory, Seq<Hold> holds, Reachability live, Func<ContentAddress, bool> eligible, Instant now, Guid correlation)` is the pure verdict fold; `public static IO<LanguageExt.HashSet<ContentAddress>> Mark(ReachabilitySource source)` dispatches one full-history reachability entry over reconstructed `Cuts` or a Marten `EventTagQuery` `Tags` adapter; `public static IO<SweepReceipt> Execute(RetentionClass cls, Seq<SweepVerdict> verdicts, Func<ContentAddress, IO<Unit>> evict, Func<ContentAddress, StorageTier, IO<Unit>> demote, ProjectionContext frame)` is the one receipted mutation executor — every eviction routing through `evict`, every cold-tier demotion through `demote` — the returned receipt carrying the sweep's real `RetentionClass`, the real reclaimed bytes, and the frame correlation.
- Auto: the sweep is one pure state-threaded `Fold` walking the inventory newest-first (`OrderByDescending` on the admission `Instant`) — holds and fenced keys exit first (yielding `Held`, or `HeldOverBudget` once their running bytes clear the budget), the reachability mark shielding ONLY a collecting class (`LossPolicy.Collects`); a DECLARED-EXPIRY class then expires at its declared bound (an artifact past `AgeBound` yields the age verdict — the declared expiry IS the age eviction, and budget pressure never expires a declared-expiry artifact early because capture-side truncation owns the budget response); a NEVER-EVICT class consumes the mark itself — an unreachable, fence-cleared artifact past its declared `OrphanAge` yields `EvictOrphan` (a younger orphan is `Kept` — the age condition is the schedule's own column, never a policy hidden in the key-only eligibility predicate) and a reachable one runs the cold-tiering arm GATED on `Lane.Durable` (a durable-lane artifact past `AgeBound` whose `StorageTier` can still demote yields `Cool`; a non-durable lane has no colder home so the arm never fires there; an already-coldest or still-young one is `Kept`); a RECEIPTED-EVICT class takes the first deciding verdict in the declared order (age past `AgeBound`, then count past `CountBound`, then size past `ByteBudget`); BOTH arms mint their age verdict through the one `Aged` projection so the fact's own GRAIN decides its shape — a row-grained fact yields `EvictAge` and a partition-grained one a whole-partition `DropPartition` — while the count and size stages stay row-grained economics a still-young partition is `Kept` ahead of, so a rolling-window family pays one verdict per aged partition where it once paid one per row; the newest-first walk threads `(LiveCount, RunningBytes)` over the retained-newest so `EvictCount` fires once `CountBound` newer survivors are already kept and `EvictSize` once running bytes clears `ByteBudget`, which keeps newest-N and evicts the OLDEST beyond budget in one pass (the size and count stages demand opposite walk directions under an ascending walk — newest-first reconciles both), every evict verdict carrying the artifact's own bytes and the prepend-built ledger reading oldest-first; a `Cool` is RETAINED (the bytes stay resident, demoted) so it threads `Live`/`Bytes` exactly like `Kept`; verdicts are a pure function of the inventory snapshot, the policy snapshot, the hold rows, and the eligibility predicate under one clock instant, so the verdict list is a testable value and a partial sweep resumes by re-folding with no journal; the reachability mark runs over EVERY AS-OF cut, not head — a content key referenced by any historical `TimeCut`'s reconstructed graph is `Reachable` and never collected, so a blob a prior version still references survives even after head drops it; blob bytes delete after the catalog row commit (the crash window produces collectible orphans, never dangling rows) and the age-gated orphan pass closes the loop.
- Receipt: every removed artifact emits `(class, identity, deciding rule, bytes)`, every dropped partition `(class, identity, "partition", bytes)` beside the partition name and the row count it retired, and every demotion `(class, identity, "cool", from-tier, to-tier)`; the run summary proves `inventory = kept + held + cooled + evicted`; unreceipted deletion OR demotion anywhere is a rail rejection, and the receipt stream is itself a count-and-age-bounded class closing meta-retention at depth one; evict verdicts cross the `rasm.persistence.retention.sweep` veto point (`Store/observability#HOOK_RAIL` `PersistenceHooks.Swept`) BEFORE `Execute` — a subscriber refusal downgrades the verdict to `Held`, never an aborted sweep.
- Packages: Marten (`store.Advanced.DropAgedRollingPartitionsAsync` — the rolling family's bound `evict` arrow), LanguageExt.Core (`Seq`/`Fold`/`IO`/`TraverseM`/`HashSet`/`Option`), Thinktecture.Runtime.Extensions (`[Union]` + `Switch`), NodaTime, BCL inbox.
- Growth: a new sweep rule is one stage in the declared verdict order; a new hold selector is one `Hold` case; a new deletion provenance is one `SweepVerdict` evict case (as `EvictAdministrative`, `EvictOrphan`, and `DropPartition` are); a new partition-retired family is one `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` row with the partition-grained inventory its sweep reads, never a second verdict case; a new preservation-side transition is one retaining `SweepVerdict` case (as `Cool` is) plus one executor delegate; zero new surface — a second sweeper, a head-only GC, an unreceipted cleanup, a tier-transition side channel beside the one executor, or an export-to-preserve workaround is the deleted form because the sweep is the single mutation executor and the GC marks over the full history.
- Boundary: the reachability GC runs over the FULL event history, not head (`H10`) — `Mark` folds the referenced content keys of every AS-OF cut's reconstructed graph so an artifact blob or snapshot a historical version references is `Reachable` and survives, and a head-only GC that collects a blob a prior version still cites is the deleted form; the alternative permitted by `H10` is geometry-GC-forbidden (dedup-plus-cold-tiering with no collection), expressed as a `blob`-class schedule whose `LossPolicy.NeverEvict` makes the age-threshold a `Cool` cold-tier demotion (the `RetentionCeiling.Demote` ladder over the blobstore `StorageTier`, re-PUT through the `Execute` `demote` delegate) rather than an eviction — the landed `Blob` row IS that schedule, and a `NeverEvict` class that merely keeps-forever, or a prose-only "tiering" with no verdict, is the deleted thin slice; reachability shields ONLY a collecting class — the never-evict arm consumes the mark as its orphan/cool discriminant, so `EvictOrphan` collects ONLY an artifact no cut ever referenced and never a referenced blob (`H10` holds structurally), while the `stream` class never reaches that arm because its cadence never schedules a sweep and the SoR fence rides the injected eligibility predicate; the sweep dispatch consumes BOTH `LossPolicy` columns, the derived `Collects` shield, and the lane's `Durable` flag — `Expires` selects the declared-expiry arm (the artifact evicts AT its declared `AgeBound`, so an aged evidence bundle or awareness row expires instead of nonsensically cold-tiering), and the `Cool` arm demands `Lane.Durable` because a tier re-PUT is a durable-home operation a `Transient`-lane artifact cannot take — a captured-but-never-read policy column is the deleted illusory form; holds are first-class rows bound late at sweep time so a hold placed today protects artifacts admitted tomorrow, release deletes the row with no eviction side effect, and every run emits an active-hold inventory because forgotten holds are the dominant retention failure; the executor is the one mutation surface every lane routes through (a snapshot sweep, a blob GC, a cache eviction, an operator purge through `EvictAdministrative`, a cold-tier demotion through `Cool`) so the receipt stream is the complete lifecycle ledger; eligibility predicates inject (sync fences, projection floors, export pins, the `Store/blobstore#BLOB_GC` WORM/object-lock fence holding a blob under an active retention-until) so the sweep owns zero domain-safety rules and every refusal names the predicate that held it — the orphan AGE condition is NOT one of them: age is a fact the sweep already holds (`fact.At` against `Schedule.OrphanAge`), and a key-only predicate cannot see it, so smuggling the age policy into `eligible` is the deleted form; the injected `evict` arrow is itself a lane-owned effect that can fail with a lane-specific typed fault (the blob lane's `WormEvict` surfaces `Store/blobstore#OBJECT_STORE` `RemoteStoreFault.Locked` when a compliance-window blob is targeted, the defense-in-depth second gate behind the eligibility fence) — `Execute` lifts that fault through the run rail rather than swallowing it, so a WORM violation is a typed refusal on the receipt stream, never a silent skip or a generic provider 403; a family carrying a `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` row sweeps at PARTITION grain — its class's own declared `AgeBound` still decides and the roster's aged edge sits one period beyond it, so a drop never outruns the verdict; its inventory enumerates partitions, so `Inventory == Kept + Held + Cooled + Evicted` closes over exactly the units the run walked and the receipt stream still accounts every deletion, one `DropPartition` naming the rows a drop retired rather than a silent bulk removal, while holds and the eligibility fence keep their meaning at that grain (a hold over the class holds the partition, and a held partition never drops); the content-keyed classes keep the per-row receipted sweep and full-history reachability untouched, because a partition drop cannot consult a reachability mark and the roster admits only families whose whole table shares one declared bound; the drop still executes through the ONE `Execute` mutation surface — for a rolling family the injected `evict` arrow IS `store.Advanced.DropAgedRollingPartitionsAsync`, so an unreceipted deletion stays a rail rejection here exactly as it is for a blob delete — and the single-writer boot pass's `ApplyRollingPartitionsAsync` (`Element/identity#SCHEMA_VERDICT`) is the same trailing drop composed with the leading `RollPartitionsForwardAsync` provision, both idempotent and multi-node safe, so boot and cadence performing the same drop is one act and a cron rotation job beside them is the deleted form.

```csharp signature
// `Key`/`Bytes` are ABSTRACT on the root so each case's synthesized positional property OVERRIDES them — a concrete
// computed base property beside a same-named positional parameter is the deleted form (the parameter goes unread and
// the base switch recurses into itself); `Kept` carries no byte figure, so its override is the explicit zero.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepVerdict {
    private SweepVerdict() { }
    public sealed record Kept(ContentAddress Key) : SweepVerdict { public override long Bytes => 0L; }
    public sealed record Held(ContentAddress Key, long Bytes, string By) : SweepVerdict;
    public sealed record HeldOverBudget(ContentAddress Key, long Bytes, long OverBy) : SweepVerdict;
    public sealed record EvictAge(ContentAddress Key, long Bytes, Duration Age) : SweepVerdict;
    public sealed record EvictCount(ContentAddress Key, long Bytes, int Rank) : SweepVerdict;
    public sealed record EvictSize(ContentAddress Key, long Bytes, long OverBy) : SweepVerdict;
    public sealed record EvictAdministrative(ContentAddress Key, long Bytes, string By) : SweepVerdict;
    // The never-evict class's ONLY collection: an unreachable artifact no AS-OF cut ever referenced (crash debris,
    // a race-loser row) past its declared Schedule.OrphanAge — the age-gated orphan pass closing the write-crash
    // loop while H10 keeps every referenced blob collection-free and a younger orphan stays Kept.
    public sealed record EvictOrphan(ContentAddress Key, long Bytes, Duration Age) : SweepVerdict;
    // `DropPartition` is the PARTITION-grained age eviction: a rolling-window family's aged partition leaves
    // through its database's own constant-time drop, so one verdict accounts every row it carried rather than
    // one verdict per row. `Key` is the ordinary `IdentityScheme.NamePlusEpoch` mint over the partition name,
    // because a partition IS an epoch-grained name-plus-epoch unit, so the abstract `Key` holds with no shape
    // break and the receipt stream reads one identity space end to end.
    public sealed record DropPartition(ContentAddress Key, string Partition, int Rows, long Bytes) : SweepVerdict;
    // The cold-tiering verdict (`H10`: geometry-GC-forbidden = dedup-plus-cold-tiering): a `NeverEvict`-class artifact past
    // its `AgeBound` whose `StorageTier` can still demote rides `Cool` carrying the next-colder tier, so eviction is REPLACED
    // by a tier transition the `Execute` `demote` delegate re-PUTs at — preservation pressure on a never-evict class flows
    // to colder storage, never to collection or to displacing onto unheld artifacts.
    public sealed record Cool(ContentAddress Key, long Bytes, StorageTier From, StorageTier To) : SweepVerdict;

    public abstract ContentAddress Key { get; }
    public abstract long Bytes { get; }
    public bool Evicts => this is EvictAge or EvictCount or EvictSize or EvictAdministrative or EvictOrphan or DropPartition;
    public bool Retains => this is Held or HeldOverBudget;
    public bool Cools => this is Cool;
    public string Rule => Switch(
        kept:                static _ => "kept",
        held:                static _ => "hold",
        heldOverBudget:      static _ => "hold",
        evictAge:            static _ => "age",
        evictCount:          static _ => "count",
        evictSize:           static _ => "size",
        evictAdministrative: static _ => "administrative",
        evictOrphan:         static _ => "orphan",
        dropPartition:       static _ => "partition",
        cool:                static _ => "cool");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Hold {
    private Hold() { }
    public sealed record WholeClass(RetentionClass Class) : Hold;
    public sealed record IdentitySet(Set<ContentAddress> Keys) : Hold;
    public sealed record StampRange(Instant From, Instant Until) : Hold;

    public bool Holds(RetentionFact fact) => Switch(
        state: fact,
        wholeClass:  static (f, c) => c.Class == f.Class,
        identitySet: static (f, s) => s.Keys.Contains(f.Key),
        stampRange:  static (f, r) => (f.At >= r.From) && (f.At < r.Until));
}

public readonly record struct Reachability(LanguageExt.HashSet<ContentAddress> Live) {
    public static readonly Reachability None = new(LanguageExt.HashSet<ContentAddress>.Empty);
    public bool Reachable(ContentAddress key) => Live.Contains(key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record ReachabilitySource {
    private ReachabilitySource() { }
    public sealed record Cuts(Seq<TimeCut> EveryCut, Func<TimeCut, Seq<ContentAddress>> ReferencedAt) : ReachabilitySource;
    public sealed record Tags(EventTagQuery Query, Func<EventTagQuery, IO<Seq<ContentAddress>>> QueryByTags) : ReachabilitySource;
}

public readonly record struct SweepReceipt(RetentionClass Class, int Inventory, int Kept, int Held, int Cooled, int Evicted, long EvictedBytes, Instant At, CorrelationId Correlation) {
    // A cooled artifact is RETAINED (demoted, not collected), so it partitions with kept/held — the conservation identity
    // closes over all four retention-side counts plus the evicted count, never silently dropping the cold-tiering rung.
    public bool Conserves => Inventory == (Kept + Held + Cooled + Evicted);
}

public static class RetentionSweep {
    // The retention lane's own receipt slots, mounted through the `Store/observability#SLOT_REGISTRY` census spread
    // like every other emitting owner's roster — the admission and refusal slots the `#RETENTION_CLASSES` receipt
    // names, plus the sweep slot the run summary rides, so the verdict stream is projectable rather than a receipt
    // no registry knows and no arm can key on.
    public static readonly StoreSlot AdmitSlot = StoreSlot.Create("store.retention.admit");
    public static readonly StoreSlot RejectSlot = StoreSlot.Create("store.retention.reject");
    public static readonly StoreSlot SweepSlot = StoreSlot.Create("store.retention.sweep");
    public static readonly Seq<StoreSlot> Slots = Seq(AdmitSlot, RejectSlot, SweepSlot);

    // Both source cases settle to the same idempotent `LanguageExt.HashSet.Add` mark: reconstructed cuts fold every
    // historical graph, while the tag adapter projects Marten `QueryByTagsAsync(EventTagQuery)` results to content keys.
    public static IO<LanguageExt.HashSet<ContentAddress>> Mark(ReachabilitySource source) => source.Switch(
        cuts: static row => IO.pure(row.EveryCut.Fold(LanguageExt.HashSet<ContentAddress>.Empty, (live, cut) => row.ReferencedAt(cut).Fold(live, static (set, key) => set.Add(key)))),
        tags: static row => row.QueryByTags(row.Query).Map(static keys => keys.Fold(LanguageExt.HashSet<ContentAddress>.Empty, static (set, key) => set.Add(key))));

    public static (Seq<SweepVerdict> Verdicts, SweepReceipt Receipt) Run(
        RetentionClass cls, Seq<RetentionFact> inventory, Seq<Hold> holds, Reachability live, Func<ContentAddress, bool> eligible, Instant now, Guid correlation) {
        (Seq<SweepVerdict> Ledger, int Live, long Bytes) scan = toSeq(inventory.OrderByDescending(static f => f.At))
            .Fold((Ledger: Seq<SweepVerdict>(), Live: 0, Bytes: 0L),
                (state, fact) => Advance(state, fact, Decide(cls, state, fact, holds, live, eligible, now)));
        Seq<SweepVerdict> verdicts = scan.Ledger;
        SweepReceipt receipt = verdicts.Fold(new SweepReceipt(cls, inventory.Count, 0, 0, 0, 0, 0L, now, correlation), static (sum, v) =>
            v.Evicts ? sum with { Evicted = sum.Evicted + 1, EvictedBytes = sum.EvictedBytes + v.Bytes }
            : v.Cools ? sum with { Cooled = sum.Cooled + 1 }
            : v.Retains ? sum with { Held = sum.Held + 1 }
            : sum with { Kept = sum.Kept + 1 });
        return (verdicts, receipt);
    }

    // The loss-policy dispatch consumes BOTH columns, the derived Collects shield, and the lane's Durable flag:
    // holds and the eligibility fence exit first; the reachability mark shields ONLY a collecting class (a
    // DECLARED-EXPIRY or RECEIPTED-EVICT artifact a cut references is Held); a NEVER-EVICT class consumes the mark
    // itself — an unreachable, fence-cleared artifact past its declared Schedule.OrphanAge is EvictOrphan (a
    // younger orphan is Kept — age never hides in the key-only predicate) and a reachable one past its age
    // cold-tiers one rung per pass (idempotent at `Archive`)
    // ONLY on a durable lane — a Transient artifact has no colder home, so the arm is structurally closed there,
    // never a silent `Kept`-forever OR a nonsense awareness-row demotion; a DECLARED-EXPIRY class evicts AT its
    // declared AgeBound (budget pressure never expires it early — capture-side truncation owns the budget
    // response); only a RECEIPTED-EVICT class runs the age/count/size eviction ladder. BOTH arms mint their age
    // verdict through `Aged`, so the fact's own grain — row or partition — decides whether an aged fact leaves as
    // EvictAge or as one whole-partition DropPartition.
    static SweepVerdict Decide(RetentionClass cls, (Seq<SweepVerdict> Ledger, int Live, long Bytes) state, RetentionFact fact, Seq<Hold> holds, Reachability live, Func<ContentAddress, bool> eligible, Instant now) =>
        holds.Exists(h => h.Holds(fact)) || !eligible(fact.Key) || (cls.Loss.Collects && live.Reachable(fact.Key))
            ? (state.Bytes + fact.Bytes) > cls.Schedule.ByteBudget
                ? new SweepVerdict.HeldOverBudget(fact.Key, fact.Bytes, (state.Bytes + fact.Bytes) - cls.Schedule.ByteBudget)
                : new SweepVerdict.Held(fact.Key, fact.Bytes, "hold-or-reachable")
            : cls.Loss.Expires
                ? (now - fact.At) >= cls.Schedule.AgeBound
                    ? Aged(fact, now - fact.At)
                    : new SweepVerdict.Kept(fact.Key)
                : !cls.Loss.Evicts
                    ? !live.Reachable(fact.Key)
                        ? (now - fact.At) >= cls.Schedule.OrphanAge
                            ? new SweepVerdict.EvictOrphan(fact.Key, fact.Bytes, now - fact.At)
                            : new SweepVerdict.Kept(fact.Key)
                        : cls.Lane.Durable && ((now - fact.At) >= cls.Schedule.AgeBound) && RetentionCeiling.Demote(fact.Tier) is { IsSome: true, Case: StorageTier colder }
                            ? new SweepVerdict.Cool(fact.Key, fact.Bytes, fact.Tier, colder)
                            : new SweepVerdict.Kept(fact.Key)
                    : (now - fact.At) >= cls.Schedule.AgeBound
                        ? Aged(fact, now - fact.At)
                        : fact.Partition.IsSome
                            ? new SweepVerdict.Kept(fact.Key)
                            : (state.Live + 1) > cls.Schedule.CountBound
                                ? new SweepVerdict.EvictCount(fact.Key, fact.Bytes, state.Live + 1)
                                : (state.Bytes + fact.Bytes) > cls.Schedule.ByteBudget
                                    ? new SweepVerdict.EvictSize(fact.Key, fact.Bytes, (state.Bytes + fact.Bytes) - cls.Schedule.ByteBudget)
                                    : new SweepVerdict.Kept(fact.Key);

    // `Aged` mints the age verdict at the fact's own GRAIN, the ONE mint both loss-policy arms reach: a
    // partition-grained fact leaves whole as `DropPartition` (the database's constant-time drop, one verdict for
    // every row it carried) and a row-grained one as `EvictAge`, so a rolling-window family retires by partition
    // under either policy and the two arms can never diverge on what an aged fact costs. The count and size
    // stages stay ROW-grained economics — newest-N rank and a running byte budget are questions a partition does
    // not answer — so a still-young partition-grained fact is `Kept` ahead of them rather than mis-minting an
    // `EvictCount`/`EvictSize` over a whole window.
    static SweepVerdict Aged(RetentionFact fact, Duration age) =>
        fact.Partition.Case is PartitionSpan span
            ? new SweepVerdict.DropPartition(fact.Key, span.Name, span.Rows, fact.Bytes)
            : new SweepVerdict.EvictAge(fact.Key, fact.Bytes, age);

    // A Cool is RETAINED (the bytes stay resident, demoted) so it threads `Live`/`Bytes` exactly like `Kept`/`Held` —
    // only an evict releases the running figures.
    static (Seq<SweepVerdict> Ledger, int Live, long Bytes) Advance((Seq<SweepVerdict> Ledger, int Live, long Bytes) state, RetentionFact fact, SweepVerdict verdict) =>
        state with {
            Ledger = verdict.Cons(state.Ledger),
            Live = state.Live + (verdict.Evicts ? 0 : 1),
            Bytes = state.Bytes + (verdict.Evicts ? 0L : fact.Bytes),
        };

    // The ONE receipted executor every lane routes through: an evict verdict deletes through `evict`, a `Cool` re-PUTs the
    // blob at its colder tier through `demote` (both effectful, both receipted in the one fact stream) — so a manual purge,
    // a snapshot/blob GC, a cache eviction, AND a cold-tier demotion all flow through this single surface, never a side
    // channel. `Cool` bytes count as retained (`Cooled`), evict bytes as reclaimed, so the conservation partition closes.
    public static IO<SweepReceipt> Execute(RetentionClass cls, Seq<SweepVerdict> verdicts, Func<ContentAddress, IO<Unit>> evict, Func<ContentAddress, StorageTier, IO<Unit>> demote, ProjectionContext frame) =>
        from freed in verdicts.Filter(static v => v.Evicts).TraverseM(v => evict(v.Key).Map(_ => v.Bytes)).As()
        from _ in verdicts.Choose(static v => v is SweepVerdict.Cool c ? Some(c) : None).TraverseM(c => demote(c.Key, c.To)).As()
        select new SweepReceipt(cls, verdicts.Count, verdicts.Count(static v => v is SweepVerdict.Kept), verdicts.Count(static v => v.Retains), verdicts.Count(static v => v.Cools), freed.Count, freed.Fold(0L, static (acc, bytes) => acc + bytes), frame.Now(), frame.Correlation);
}
```

| [INDEX] | [POLICY]        | [VALUE]                                                   | [BINDING]                                                 |
| :-----: | :-------------- | :-------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | sweep fold      | state-threaded newest-first fold                          | pure verdict list; re-fold resumes, no journal            |
|  [02]   | declared order  | holds-first → expires → never-evict → age/count/size      | both `LossPolicy` columns + `Collects` + `Lane.Durable`   |
|  [03]   | reachability GC | mark over EVERY AS-OF cut                                 | full-history, never head; historical refs survive (`H10`) |
|  [04]   | one executor    | `Execute` (`evict` + `demote`)                            | every lane, operator purge, demotion routes through       |
|  [05]   | cold-tiering    | `Cool` demotes never-evict; `EvictOrphan` collects debris | `Demote` ladder; orphan gated on declared `OrphanAge`     |
|  [06]   | holds           | first-class, late-bound, union                            | a hold today protects tomorrow's admissions               |
|  [07]   | conservation    | `inventory = kept + held + cooled + evicted`              | the run summary proves the partition closes               |
|  [08]   | sweep grain     | rows, or partitions on a `RollingWindow` family           | one `DropPartition` per aged partition; one executor      |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
