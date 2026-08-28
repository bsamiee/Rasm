# [PERSISTENCE_VERSION_RETENTION]

`ArtifactKind` binds the asset class every durable artifact admits under and DERIVES its `RetentionClass`, which in turn binds storage lane, schedule, classification ceiling, loss capabilities, and identity scheme. `RetentionCatalog` owns admission. `RetentionSweep` derives one conserved verdict partition, marks reachability from reconstructed cuts or Marten event tags, and executes every eviction or cold-tier demotion through one tallied executor. Full-history reachability shields referenced content; `OrphanAge` gates never-referenced debris.

## [01]-[INDEX]

- [02]-[RETENTION_CLASSES]: `ArtifactKind` fixes the asset-class axis every catalog derives retention from; `RetentionClass` closes the lifecycle axis carrying its five decisions, the page-local classification-ceiling rank table, the identity-scheme behavioral families, and the loss capability set under its declared corner law.
- [03]-[SWEEP_AND_GC]: `RetentionSweep` folds one pure state-threaded verdict list at row or partition grain, honors first-class holds, marks reachability over the full history, and executes every deletion through one tallied lane under the kernel re-drive policy.

## [02]-[RETENTION_CLASSES]

- Owner: `ArtifactKind` the `[SmartEnum<string>]` ASSET-CLASS axis every durable artifact admits under, carrying its derived `RetentionClass` and `CacheTier` and owning the two provenance selectors (`Texture`, `Representation`) that answer both the row and the origin key one producer value projects; `CacheTier` the two-row recency-lane vocabulary the app platform reads settled; `RetentionClass` the `[SmartEnum<string>]` lifecycle axis carrying its five decisions; `StorageLane` the durable-home axis; `RetentionCapability` the loss vocabulary and `RetentionLoss` its three legal corners under one `CapabilityLaw`; `IdentityScheme` the content-keyed-versus-name-plus-epoch families carrying the total-`Switch` `Identity` mint; `RetentionCeiling` the sensitivity rank and cold-tier ladder; `RetentionFault` the closed admission fault; `RetentionCatalog` the one-fold admission.
- Cases: each artifact family is one `ArtifactKind` row, and a family whose retention derives from PROVENANCE is selected behind one fold reading the discriminant off a value the producer already holds — `Texture(planKey)` answers the rebuildable `TextureSet` against the unreproducible `TextureAcquired`, and `Representation(slot, bodyKey)` is a total `Switch` over the COMPOSED `Rasm.Element` `Graph/element#NODE_MODEL` `RepresentationSlot` roster. Its proven reconstructible arms select their cache rows only when the body origin is present and otherwise preserve durably; `Box` selects its lossless row with no origin; every other opaque slot selects the one durable `RepresentationPreserved` row with no invented reconstruction source. Retention closes at six rows — `snapshot`, `stream`, `blob`, `evidence`, `cache`, `ephemeral` — and a class fitting no row is an admission rejection, never a default. Class membership is immutable and reclassification is export-then-readmit, so every lived lifecycle stays recorded.
- Entry: `RetentionCatalog.Admit(cls, contentKey, name, epoch, stamp, tier, resident, write, frame)` is the one admission fold answering `IO<Fin<RetentionFact>>`; `RetentionCeiling.Admit(stamp, ceiling)` is the ONE ceiling gate answering the typed fault (`Unstamped` for an unranked stamp, `CeilingBreach` for a ranked one over the ceiling), so no call site pairs a mapped-ness probe with a comparison; `cls.Scheme.Identity(contentKey, name, epoch)` mints the catalog key inside the fold, so a caller never pre-mints identity; `cls.Schedule` projects cadence, budget, and the class's own `RedrivePolicy`.
- Auto: admission is one pipeline — ceiling-admit, identity-derive, race-admit, lane-write. Ceiling admission rejects an UNRANKED stamp before any compare, because absence of a rank is not clearance and a `CeilingBreach` reporting a comparison that never happened is the deleted form. Identity-derive dispatches on the scheme's generated total `Switch`: content-keyed passes its address through, name-plus-epoch streams `(name, epoch)` through the ONE kernel `CanonicalWriter` field stream, so the preimage is length-framed and no separator literal can forge a key. Race-admit gives content-keyed classes dedup and race-loser disposal free and name-plus-epoch classes versioned replacement free, with zero conditional code. Byte counts come from the artifact's own sealed length fields, never a later filesystem stat.
- Packages: Marten (`EventTagQuery`/`QueryByTagsAsync` adapter), Rasm (`Rasm.Domain` `ContentHash.Of<TState>` + `CanonicalWriter` — the framed name-plus-epoch preimage; `CapabilitySet`/`CapabilityLaw`; `FaultBand`), Rasm.Element (`Graph` `RepresentationSlot` — the composed identifier roster the `Representation` selector dispatches over), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` rows + `Switch` + `Items`), LanguageExt.Core (`Fin`/`Option`/`IO`/`Traverse`), NodaTime (`Duration`/`Instant`), BCL inbox (`FrozenDictionary`/`FrozenSet`).
- Growth: a new asset family is one `ArtifactKind` row, and a provenance-derived family two rows behind one selector; a new representation slot lands at its `Rasm.Element` owner and breaks the `Representation` `Switch` here until this page answers its `ArtifactKind` arm; a new lifecycle class is one `RetentionClass` row carrying its five decisions; a new durable home is one `StorageLane` row; a new loss capability is one `RetentionCapability` row with the `RetentionLoss` corners that admit it; a new sensitivity tier is one `RetentionCeiling` rank entry; a new cold rung is one `Colder` ladder entry. Zero new surface — a per-artifact retention table, a second classification taxonomy, a STORED retention column beside the kind that derives it, an origin flag beside the value that already discriminates, or a default class for an unfit artifact is the deleted form.
- Boundary: `Rasm.Element` owns the representation identifier vocabulary and this page COMPOSES it — a local slot roster is the deleted fork, exactly as an index-local `ArtifactKind` is (`Query/cache#ARTIFACT_BLOB_INDEX`) — so this page holds only the slot→kind correspondence, and a measured `Node.Coverage` grid stays off that roster because it is no representation slot. Asset class is the DISCRIMINANT and retention its derived column, so every catalog storing bytes stores the kind and reads `Kind.Retention`; the taxonomy seats HERE because the object-plane catalog and the artifact index are strata peers and a concept two peers reach seats at the lowest stratum either reaches. Each lane names its durable home, so sweep budgets and the deletion executor route by row, never a `cls.Key == "blob"` compare. Budget breach truncates and records the truncation (capture must succeed degraded) while a ceiling breach rejects outright (security never degrades) — the two overflow responses never interchange. Class `stream` IS the Marten system of record and never evicts, while class `blob` is full-history-reachable, so an aged blob cold-tiers and only never-referenced debris collects (`#SWEEP_AND_GC`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Version;

// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetentionCapability : ICapability<RetentionCapability> {
    public static readonly RetentionCapability Evict = new("evict");
    public static readonly RetentionCapability Expire = new("expire");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CacheTier {
    public static readonly CacheTier ModelResult = new("model-result");
    public static readonly CacheTier ArtifactBlob = new("artifact-blob");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IdentityScheme {
    public static readonly IdentityScheme ContentKeyed = new("content-keyed", dedups: true);
    public static readonly IdentityScheme NamePlusEpoch = new("name-plus-epoch", dedups: false);
    public bool Dedups { get; }
    private IdentityScheme(string key, bool dedups) : this(key) => Dedups = dedups;

    public ContentAddress Identity(ContentAddress contentKey, string name, ulong epoch) => Switch(
        state: (Key: contentKey, Name: name, Epoch: epoch),
        contentKeyed: static row => row.Key,
        namePlusEpoch: static row => ContentAddress.Create(
            ContentHash.Of(row, static (r, writer) => writer.String(r.Name).U128(r.Epoch))));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct RetentionSchedule(
    Duration Cadence, long ByteBudget, int CountBound, Duration AgeBound, Duration OrphanAge, RedrivePolicy Redrive);

public readonly record struct PartitionSpan(string Name, int Rows);

public readonly record struct RetentionFact(
    RetentionClass Class, ContentAddress Key, long Bytes, StorageTier Tier, Instant At, Option<PartitionSpan> Partition = default);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LaneOutcome {
    private LaneOutcome() { }
    public sealed record Stored(long Bytes) : LaneOutcome;
    public sealed record Replaced(long Bytes, ulong PriorEpoch) : LaneOutcome;
    public sealed record Deduped : LaneOutcome;

    public long Committed => this.Switch(stored: static s => s.Bytes, replaced: static r => r.Bytes, deduped: static _ => 0L);
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetentionFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Retention;
    private RetentionFault() { }
    [FaultCase(0)]
    public sealed partial record Unclassed(string Artifact) : RetentionFault();
    [FaultCase(1)]
    public sealed partial record CeilingBreach(DataClassification Stamp, DataClassification Ceiling) : RetentionFault();
    [FaultCase(2)]
    public sealed partial record Unstamped(DataClassification Stamp) : RetentionFault();

    public override string Message => Switch(
        unclassed:     static c => $"<retention-unclassed:{c.Artifact}>",
        ceilingBreach: static c => $"<retention-ceiling:{c.Stamp}>{c.Ceiling}>",
        unstamped:     static c => $"<retention-unstamped:{c.Stamp}>");
}

// --- [TABLES] --------------------------------------------------------------------------
public static class RetentionLoss {
    public static readonly CapabilitySet<RetentionCapability> Evict = CapabilitySet<RetentionCapability>.Of(RetentionCapability.Evict);
    public static readonly CapabilitySet<RetentionCapability> DeclaredExpiry = CapabilitySet<RetentionCapability>.Of(RetentionCapability.Expire);
    public static readonly CapabilitySet<RetentionCapability> NeverEvict = CapabilitySet<RetentionCapability>.None;
    public static readonly CapabilitySet<RetentionCapability> Collecting = CapabilitySet<RetentionCapability>.All;
    public static readonly CapabilityLaw<RetentionCapability> Law = new(Seq(Evict, DeclaredExpiry, NeverEvict));
}

public static class RetentionCeiling {
    static readonly FrozenDictionary<DataClassification, int> Rank = new[] {
        DataClassification.None, DataClassification.Operational, DataClassification.Internal, DataClassification.HostIdentity,
        DataClassification.UserContent, DataClassification.Personal, DataClassification.Confidential, DataClassification.Credential, DataClassification.Secret,
    }.Select(static (row, ordinal) => (row, ordinal)).ToFrozenDictionary(static t => t.row, static t => t.ordinal);

    public static Fin<DataClassification> Admit(DataClassification stamp, DataClassification ceiling) =>
        (Rank.TryGetValue(stamp, out int held), Rank.TryGetValue(ceiling, out int bound)) switch {
            (false, _) => Fin<DataClassification>.Fail(new RetentionFault.Unstamped(stamp)),
            (true, _) when held <= bound => Fin<DataClassification>.Succ(stamp),
            _ => Fin<DataClassification>.Fail(new RetentionFault.CeilingBreach(stamp, ceiling)),
        };

    static readonly FrozenDictionary<StorageTier, StorageTier> Colder = new[] {
        (StorageTier.Standard, StorageTier.Infrequent), (StorageTier.Infrequent, StorageTier.Cold), (StorageTier.Cold, StorageTier.Archive),
    }.ToFrozenDictionary(static t => t.Item1, static t => t.Item2);
    public static Option<StorageTier> Demote(StorageTier tier) => Colder.TryGetValue(tier, out StorageTier next) ? Some(next) : None;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetentionClass {
    static RetentionSchedule Cadence(Duration cadence, long bytes, int count, Duration age, Duration orphan) =>
        new(cadence, bytes, count, age, orphan, RedrivePolicy.Of(law: Schedule.exponential(Duration.FromSeconds(2)), bound: 5));

    public static readonly RetentionClass Snapshot = new("snapshot", StorageLane.SnapshotArchive, RetentionLoss.Evict, IdentityScheme.ContentKeyed, DataClassification.Internal, Cadence(Duration.FromHours(6), 64L * 1024 * 1024 * 1024, 32, Duration.FromDays(365), Duration.FromDays(7)));
    public static readonly RetentionClass Stream = new("stream", StorageLane.EventStream, RetentionLoss.NeverEvict, IdentityScheme.ContentKeyed, DataClassification.Confidential, new RetentionSchedule(Duration.MaxValue, long.MaxValue, int.MaxValue, Duration.MaxValue, Duration.MaxValue, RedrivePolicy.None));
    public static readonly RetentionClass Blob = new("blob", StorageLane.ObjectStore, RetentionLoss.NeverEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, Cadence(Duration.FromHours(12), 512L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromDays(90), Duration.FromDays(7)));
    public static readonly RetentionClass Evidence = new("evidence", StorageLane.SnapshotArchive, RetentionLoss.DeclaredExpiry, IdentityScheme.NamePlusEpoch, DataClassification.Confidential, Cadence(Duration.FromDays(1), 8L * 1024 * 1024 * 1024, 256, Duration.FromDays(90), Duration.FromDays(7)));
    public static readonly RetentionClass Cache = new("cache", StorageLane.ObjectStore, RetentionLoss.Evict, IdentityScheme.ContentKeyed, DataClassification.Internal, Cadence(Duration.FromHours(1), 16L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromDays(7), Duration.FromHours(24)));
    public static readonly RetentionClass Ephemeral = new("ephemeral", StorageLane.Transient, RetentionLoss.DeclaredExpiry, IdentityScheme.NamePlusEpoch, DataClassification.Internal, Cadence(Duration.FromMinutes(1), 1L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromMinutes(5), Duration.FromMinutes(10)));

    public StorageLane Lane { get; }
    public CapabilitySet<RetentionCapability> Loss { get; }
    public IdentityScheme Scheme { get; }
    public DataClassification Ceiling { get; }
    public RetentionSchedule Schedule { get; }
    private RetentionClass(string key, StorageLane lane, CapabilitySet<RetentionCapability> loss, IdentityScheme scheme, DataClassification ceiling, RetentionSchedule schedule) : this(key) =>
        (Lane, Loss, Scheme, Ceiling, Schedule) = (lane, loss, scheme, ceiling, schedule);

    public bool Collects => Loss.Held.Overlaps(RetentionLoss.Collecting.Held);

    public static readonly Fin<Unit> Lawful =
        toSeq(Items).Traverse(static row => RetentionLoss.Law.Admit(row.Loss)).As().Map(static _ => unit);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArtifactKind {
    public static readonly ArtifactKind Interchange = new("interchange", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind EpContext = new("ep-context", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind OnnxProfile = new("onnx-profile", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind ParityVerdict = new("parity-verdict", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind IfcSemantic = new("ifc-semantic", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind Scan = new("scan", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind ChunkContent = new("chunk-content", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind CloudRun = new("cloud-run", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind Assessment = new("assessment", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind RepresentationBody = new("representation-body", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind RepresentationAxis = new("representation-axis", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind RepresentationFootprint = new("representation-footprint", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind RepresentationBox = new("representation-box", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind RepresentationPreserved = new("representation-preserved", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind CoverageRaster = new("coverage-raster", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind TextureSet = new("texture-set", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind TextureAcquired = new("texture-acquired", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind CapacityHull = new("capacity-hull", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind ArchiveCorpus = new("archive-corpus", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind ArchiveSolve = new("archive-solve", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind GraduationEnvelope = new("graduation-envelope", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind InitializerPack = new("initializer-pack", RetentionClass.Blob, CacheTier.ModelResult);
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

    public static (ArtifactKind Kind, Option<UInt128> Source) Texture(Option<UInt128> planKey) =>
        (planKey.IsSome ? TextureSet : TextureAcquired, planKey);

    public static (ArtifactKind Kind, Option<UInt128> Source) Representation(RepresentationSlot slot, Option<UInt128> bodyKey) =>
        slot.Switch(
            state: bodyKey,
            body: static key => Derived(RepresentationBody),
            axis: static key => Derived(RepresentationAxis),
            footPrint: static key => Derived(RepresentationFootprint),
            box: static _ => (RepresentationBox, Option<UInt128>.None),
            annotation: static _ => (RepresentationPreserved, Option<UInt128>.None),
            surface: static _ => (RepresentationPreserved, Option<UInt128>.None),
            profile: static _ => (RepresentationPreserved, Option<UInt128>.None),
            clearance: static _ => (RepresentationPreserved, Option<UInt128>.None),
            cog: static _ => (RepresentationPreserved, Option<UInt128>.None),
            lighting: static _ => (RepresentationPreserved, Option<UInt128>.None),
            reference: static _ => (RepresentationPreserved, Option<UInt128>.None));

    private static (ArtifactKind Kind, Option<UInt128> Source) Derived(ArtifactKind kind, Option<UInt128> bodyKey) =>
        bodyKey.Match(
            Some: key => (kind),
            None: static () => (RepresentationPreserved, Option<UInt128>.None));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RetentionCatalog {
    public static IO<Fin<RetentionFact>> Admit(
        RetentionClass cls, ContentAddress contentKey, string name, ulong epoch, DataClassification stamp,
        StorageTier tier, Func<ContentAddress, bool> resident, Func<ContentAddress, IO<Fin<LaneOutcome>>> write, ProjectionContext frame) =>
        RetentionCeiling.Admit(stamp, cls.Ceiling).Match(
            Succ: _ => Land(cls, cls.Scheme.Identity(contentKey, name, epoch), tier, resident, write, frame),
            Fail: fault => IO.pure(Fin<RetentionFact>.Fail(fault)));

    static IO<Fin<RetentionFact>> Land(
        RetentionClass cls, ContentAddress key, StorageTier tier,
        Func<ContentAddress, bool> resident, Func<ContentAddress, IO<Fin<LaneOutcome>>> write, ProjectionContext frame) =>
        cls.Scheme.Dedups && resident(key)
            ? IO.pure(Fin<RetentionFact>.Succ(new RetentionFact(cls, key, 0L, tier, frame.Now())))
            : write(key).Map(outcome => outcome.Map(committed => new RetentionFact(cls, key, committed.Committed, tier, frame.Now())));
}
```

| [INDEX] | [POLICY]          | [VALUE]                                             | [BINDING]                                                |
| :-----: | :---------------- | :-------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | class membership  | one of six closed `RetentionClass` rows             | unfit artifact rejects, never defaults                   |
|  [02]   | storage lane      | `StorageLane` row per class                         | sweep/delete route by lane, not a key string compare     |
|  [03]   | sensitivity rank  | page-local `RetentionCeiling` frozen table          | `DataClassification` carries no ordinal; rank lives here |
|  [04]   | identity scheme   | content-keyed vs name-plus-epoch, `Scheme.Identity` | dedup/race-disposal vs versioned-replacement             |
|  [05]   | loss capabilities | `CapabilitySet<RetentionCapability>` + `Law`        | three legal corners; evict-and-expire refuses at init    |
|  [06]   | ceiling vs budget | ceiling rejects, budget truncates                   | security never degrades; capture succeeds degraded       |
|  [07]   | stream class      | append-only, never evicted                          | the event SoR; only snapshots and blobs reclaim          |

## [03]-[SWEEP_AND_GC]

- Owner: `SweepVerdict` the closed per-artifact verdict carrying the artifact's own byte figure, its `Releases` case predicate, and the `Book` fold each case tallies itself through; `Hold` the first-class hold row (whole-class, identity-set, stamp-range selectors composing by union); `SweepTally` the run summary proving `inventory = kept + held + cooled + evicted + refused` and carrying the lane's per-key refusal roster; `Reachability` the full-history mark; `ReachabilitySource` the two mark inputs; `RetentionSweep` the static surface owning the pure state-threaded verdict fold and the one tallied mutation executor over the `Store/blobstore#OBJECT_STORE` `EraseTally` the erase arrow answers in.
- Cases: `SweepVerdict` is `Kept | Held | HeldOverBudget | EvictAge | EvictCount | EvictSize | EvictAdministrative | EvictOrphan | DropPartition | Cool`. `DropPartition` is the PARTITION-grained age eviction — an aged trailing partition leaves whole through the database's constant-time drop, one verdict naming the partition and the rows it retired. Held bytes count against the budget but cannot evict, so preservation pressure surfaces as `HeldOverBudget` rather than displacing onto unheld artifacts. `EvictAdministrative` routes an operator purge through the same executor rather than a side channel. `EvictOrphan` is the never-evict class's ONLY collection: an unreachable artifact no cut ever referenced, past the class's declared `OrphanAge`. `Cool` is its cold-tiering verdict, carrying `From`/`To` so an aged never-evict artifact demotes rather than collects or keeps forever.
- Entry: `RetentionSweep.Run(cls, inventory, holds, live, eligible, now)` is the pure verdict fold answering the verdict list beside its PLANNED tally; `Mark(source)` dispatches one full-history reachability entry over reconstructed `Cuts` or a Marten `EventTagQuery` `Tags` adapter; `Execute(cls, verdicts, evict, demote, frame)` is the one tallied mutation executor — the WHOLE evict set through `evict` in one pass, every demotion through `demote`, both under the class's own `RedrivePolicy` — returning the executed tally with released bytes and per-key refusals and writing `Store/observability#STORE_INSTRUMENTS` `RetentionSwept` by class and deciding rule through the frame's instrument set.
- Auto: the sweep is one state-threaded fold walking the inventory newest-first, and `Decide` reads ONE joint discriminant rather than a guard ladder: fenced-or-shielded, then the class's own loss capabilities. Fencing yields `Held`, or `HeldOverBudget` once its running bytes clear the budget. Declared-expiry classes evict AT its declared `AgeBound`, because capture-side truncation owns the budget response and pressure never expires an artifact early. Never-evict classes consume the mark itself — an unreachable, fence-cleared artifact past its `OrphanAge` is `EvictOrphan`, a younger one `Kept`, and a reachable aged one `Cool` gated on `Lane.Durable`, since a `Transient` artifact has no colder home. Evicting classes take the first deciding verdict in the declared order: age, then count, then size. Both aging arms mint through the one `Aged` projection, so the fact's own GRAIN decides whether it leaves as `EvictAge` or as one whole-partition `DropPartition`.
- Auto: the newest-first walk threads `(Live, Bytes)` over the retained-newest, so `EvictCount` fires once `CountBound` newer survivors are kept and `EvictSize` once running bytes clear `ByteBudget` — newest-N and oldest-beyond-budget in one pass, where the two stages demand opposite directions under an ascending walk. Verdict `Cool` stays RETAINED and threads exactly like `Kept`. Every verdict books itself into the tally through its own `Book` arm, so the planned tally and the executed tally read one generated dispatch and a new case cannot silently miss a slot. Verdicts are a pure function of the inventory, policy, holds, and eligibility under one clock instant, so a partial sweep resumes by re-folding with no journal. Reachability marks over EVERY AS-OF cut, so a blob a prior version references survives after head drops it, and blob bytes delete after the catalog row commits — the crash window produces collectible orphans, never dangling rows.
- Output: `SweepTally` — every dropped partition names the partition and the rows it retired, every demotion its from-and-to tier, and every per-key refusal its code; the run summary proves `inventory = kept + held + cooled + evicted + refused`, with reclaimed bytes counting only keys the lane released, and a deletion or demotion outside the executor is a typed rejection. Evict verdicts cross the `rasm.persistence.retention.sweep` veto point before `Execute`, where a subscriber refusal downgrades the verdict to `Held`, never aborting the sweep.
- Packages: Marten (`store.Advanced.DropAgedRollingPartitionsAsync` — the rolling family's bound `evict` arrow), Rasm.Persistence (`Store/blobstore#OBJECT_STORE` `EraseTally`), Rasm (`Rasm.Domain` `Redrive.Run` + `RedrivePolicy` — the ONE re-drive executor over both effect arrows), LanguageExt.Core (`Seq`/`Fold`/`IO`/`TraverseM`/`HashSet`/`Option`), Thinktecture.Runtime.Extensions (`[Union]` + `Switch`), NodaTime, BCL inbox.
- Growth: a new sweep rule is one stage in the declared verdict order; a new hold selector one `Hold` case; a new deletion provenance one evict case with its `Book` arm; a new preservation-side transition one retaining case with one executor delegate; a new partition-retired family one `RollingWindow` row with the partition-grained inventory its sweep reads. Zero new surface — a second sweeper, a head-only GC, an untallied cleanup, a tier-transition side channel beside the one executor, a hand retry loop around an erase, or an export-to-preserve workaround is the deleted form.
- Boundary: the GC marks over the FULL event history, not head — an artifact any AS-OF cut references is `Reachable` and survives, and a head-only GC that collects a blob a prior version cites is the deleted form. Dedup-plus-cold-tiering is the permitted alternative, expressed as the `blob` row's never-evict loss set making its age threshold a `Cool` demotion rather than an eviction; a never-evict class that merely keeps forever, or a prose-only "tiering" with no verdict, is the deleted thin slice. Reachability shields ONLY a collecting class, so `EvictOrphan` collects only what no cut ever referenced. Arm `Cool` demands `Lane.Durable` because a tier re-PUT is a durable-home operation, and the `stream` class never reaches any arm because its cadence never schedules a sweep.
- Boundary: holds are first-class rows bound late at sweep time, so a hold placed today protects tomorrow's admissions, release deletes the row with no eviction side effect, and every run emits an active-hold inventory because forgotten holds are the dominant retention failure. Eligibility predicates inject (sync fences, projection floors, export pins, the `Store/blobstore#BLOB_GC` WORM fence), so the sweep owns zero domain-safety rules — but the orphan AGE condition is NOT one of them: age is a fact the sweep already holds and a key-only predicate cannot see it, so smuggling it into `eligible` is the deleted form. Every injected `evict` arrow is SET-shaped and lane-owned, answering in `EraseTally`, whose two columns are the two failure grains.
- Boundary: a family carrying a `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` row sweeps at PARTITION grain — its class's declared `AgeBound` still decides and the roster's aged edge sits one period beyond it, so a drop never outruns the verdict. Its inventory enumerates partitions, so the conservation identity closes over exactly the units the run walked and one `DropPartition` accounts the rows a drop retired. Holds and the eligibility fence keep their meaning at that grain, while the content-keyed classes keep per-row sweep and full-history reachability untouched, because a partition drop cannot consult a mark. Drops still execute through the ONE `Execute` surface, and the single-writer boot pass's `ApplyRollingPartitionsAsync` is the same trailing drop composed with the leading provision — so boot and cadence performing one act makes a cron rotation job beside them the deleted form.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
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
    public sealed record EvictOrphan(ContentAddress Key, long Bytes, Duration Age) : SweepVerdict;
    public sealed record DropPartition(ContentAddress Key, string Partition, int Rows, long Bytes) : SweepVerdict;
    public sealed record Cool(ContentAddress Key, long Bytes, StorageTier From, StorageTier To) : SweepVerdict;

    public abstract ContentAddress Key { get; }
    public abstract long Bytes { get; }

    public bool Releases => Switch(
        kept: static _ => false, held: static _ => false, heldOverBudget: static _ => false,
        evictAge: static _ => true, evictCount: static _ => true, evictSize: static _ => true,
        evictAdministrative: static _ => true, evictOrphan: static _ => true, dropPartition: static _ => true,
        cool: static _ => false);

    public SweepTally Book(SweepTally sum) => Switch(
        state: sum,
        kept:                static (s, _) => s with { Kept = s.Kept + 1 },
        held:                static (s, _) => s with { Held = s.Held + 1 },
        heldOverBudget:      static (s, _) => s with { Held = s.Held + 1 },
        cool:                static (s, _) => s with { Cooled = s.Cooled + 1 },
        evictAge:            static (s, v) => s with { Evicted = s.Evicted + 1, EvictedBytes = s.EvictedBytes + v.Bytes },
        evictCount:          static (s, v) => s with { Evicted = s.Evicted + 1, EvictedBytes = s.EvictedBytes + v.Bytes },
        evictSize:           static (s, v) => s with { Evicted = s.Evicted + 1, EvictedBytes = s.EvictedBytes + v.Bytes },
        evictAdministrative: static (s, v) => s with { Evicted = s.Evicted + 1, EvictedBytes = s.EvictedBytes + v.Bytes },
        evictOrphan:         static (s, v) => s with { Evicted = s.Evicted + 1, EvictedBytes = s.EvictedBytes + v.Bytes },
        dropPartition:       static (s, v) => s with { Evicted = s.Evicted + 1, EvictedBytes = s.EvictedBytes + v.Bytes });

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
    public static readonly Reachability None = new(LanguageExt.HashSet<ContentAddress>());
    public bool Reachable(ContentAddress key) => Live.Contains();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record ReachabilitySource {
    private ReachabilitySource() { }
    public sealed record Cuts(Seq<TimeCut> EveryCut, Func<TimeCut, Seq<ContentAddress>> ReferencedAt) : ReachabilitySource;
    public sealed record Tags(EventTagQuery Query, Func<EventTagQuery, IO<Seq<ContentAddress>>> QueryByTags) : ReachabilitySource;
}

public readonly record struct SweepTally(
    RetentionClass Class, int Inventory, int Kept, int Held, int Cooled, int Evicted, int Refused,
    long EvictedBytes, Seq<(ContentAddress Key, string Code)> Refusals) {
    public static SweepTally Empty(RetentionClass cls, int inventory) =>
        new(cls, inventory, 0, 0, 0, 0, 0, 0L, Seq<(ContentAddress Key, string Code)>());

    public bool Conserves => Inventory == (Kept + Held + Cooled + Evicted + Refused);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RetentionSweep {
    public static IO<LanguageExt.HashSet<ContentAddress>> Mark(ReachabilitySource source) => source.Switch(
        cuts: static row => IO.pure(row.EveryCut.Fold(LanguageExt.HashSet<ContentAddress>(), cut => row.ReferencedAt(cut).Fold(static key => set.Add(key)))),
        tags: static row => row.QueryByTags(row.Query).Map(static keys => keys.Fold(LanguageExt.HashSet<ContentAddress>(), static key => set.Add(key))));

    public static (Seq<SweepVerdict> Verdicts, SweepTally Tally) Run(
        RetentionClass cls, Seq<RetentionFact> inventory, Seq<Hold> holds, Reachability live,
        Func<ContentAddress, bool> eligible, Instant now) {
        (Seq<SweepVerdict> Ledger, int Live, long Bytes) scan = toSeq(inventory.OrderByDescending(static f => f.At))
            .Fold((Ledger: Seq<SweepVerdict>(), Live: 0, Bytes: 0L),
                (state, fact) => Advance(state, fact, Decide(cls, state, fact, holds, live, eligible, now)));
        return (scan.Ledger, scan.Ledger.Fold(SweepTally.Empty(cls, inventory.Count), static (sum, v) => v.Book(sum)));
    }

    static SweepVerdict Decide(
        RetentionClass cls, (Seq<SweepVerdict> Ledger, int Live, long Bytes) state, RetentionFact fact,
        Seq<Hold> holds, Reachability live, Func<ContentAddress, bool> eligible, Instant now) =>
        (Fenced: holds.Exists(h => h.Holds(fact)) || !eligible(fact.Key) || (cls.Collects && live.Reachable(fact.Key)),
         Expires: cls.Loss.Admits(RetentionCapability.Expire),
         Evicts: cls.Loss.Admits(RetentionCapability.Evict)) switch {
            { Fenced: true } => Fence(cls, state, fact),
            { Expires: true } => Expire(cls, fact, now),
            { Evicts: false } => Preserve(cls, fact, live, now),
            _ => Reclaim(cls, state, fact, now),
        };

    static SweepVerdict Fence(RetentionClass cls, (Seq<SweepVerdict> Ledger, int Live, long Bytes) state, RetentionFact fact) =>
        (state.Bytes + fact.Bytes) > cls.Schedule.ByteBudget
            ? new SweepVerdict.HeldOverBudget(fact.Key, fact.Bytes, (state.Bytes + fact.Bytes) - cls.Schedule.ByteBudget)
            : new SweepVerdict.Held(fact.Key, fact.Bytes, "hold-or-reachable");

    static SweepVerdict Expire(RetentionClass cls, RetentionFact fact, Instant now) =>
        (now - fact.At) >= cls.Schedule.AgeBound ? Aged(fact, now - fact.At) : new SweepVerdict.Kept(fact.Key);

    static SweepVerdict Preserve(RetentionClass cls, RetentionFact fact, Reachability live, Instant now) =>
        !live.Reachable(fact.Key)
            ? (now - fact.At) >= cls.Schedule.OrphanAge
                ? new SweepVerdict.EvictOrphan(fact.Key, fact.Bytes, now - fact.At)
                : new SweepVerdict.Kept(fact.Key)
            : cls.Lane.Durable && ((now - fact.At) >= cls.Schedule.AgeBound) && RetentionCeiling.Demote(fact.Tier) is { IsSome: true, Case: StorageTier colder }
                ? new SweepVerdict.Cool(fact.Key, fact.Bytes, fact.Tier, colder)
                : new SweepVerdict.Kept(fact.Key);

    static SweepVerdict Reclaim(RetentionClass cls, (Seq<SweepVerdict> Ledger, int Live, long Bytes) state, RetentionFact fact, Instant now) =>
        (now - fact.At) >= cls.Schedule.AgeBound ? Aged(fact, now - fact.At)
        : fact.Partition.IsSome ? new SweepVerdict.Kept(fact.Key)
        : (state.Live + 1) > cls.Schedule.CountBound ? new SweepVerdict.EvictCount(fact.Key, fact.Bytes, state.Live + 1)
        : (state.Bytes + fact.Bytes) > cls.Schedule.ByteBudget
            ? new SweepVerdict.EvictSize(fact.Key, fact.Bytes, (state.Bytes + fact.Bytes) - cls.Schedule.ByteBudget)
            : new SweepVerdict.Kept(fact.Key);

    static SweepVerdict Aged(RetentionFact fact, Duration age) =>
        fact.Partition.Case is PartitionSpan span
            ? new SweepVerdict.DropPartition(fact.Key, span.Name, span.Rows, fact.Bytes)
            : new SweepVerdict.EvictAge(fact.Key, fact.Bytes, age);

    static (Seq<SweepVerdict> Ledger, int Live, long Bytes) Advance(
        (Seq<SweepVerdict> Ledger, int Live, long Bytes) state, RetentionFact fact, SweepVerdict verdict) =>
        state with {
            Ledger = verdict.Cons(state.Ledger),
            Live = state.Live + (verdict.Releases ? 0 : 1),
            Bytes = state.Bytes + (verdict.Releases ? 0L : fact.Bytes),
        };

    public static IO<SweepTally> Execute(
        RetentionClass cls, Seq<SweepVerdict> verdicts,
        Func<Seq<ContentAddress>, IO<EraseTally>> evict, Func<ContentAddress, StorageTier, IO<Unit>> demote, ProjectionContext frame) =>
        from tally in Redrive.Run(cls.Schedule.Redrive, evict(verdicts.Filter(static v => v.Releases).Map(static v => v.Key)))
        from _ in verdicts.Choose(static v => v is SweepVerdict.Cool c ? Some(c) : None)
                          .TraverseM(c => Redrive.Run(cls.Schedule.Redrive, demote(c.Key, c.To))).As()
        let summed = Summed(cls, verdicts, tally)
        from _swept in IO.lift(Seq(("kept", summed.Kept), ("hold", summed.Held), ("cool", summed.Cooled), ("evict", summed.Evicted))
            .TraverseM(row => frame.Instruments.Write(StoreInstruments.RetentionSwept.Spec, row.Item2,
                InstrumentSet.Tags((StoreInstruments.ClassSlot, cls.Key), (StoreInstruments.RuleSlot, row.Item1)))).As().Map(static _ => unit))
        select summed;

    static SweepTally Summed(RetentionClass cls, Seq<SweepVerdict> verdicts, EraseTally tally) {
        Set<ContentAddress> refused = toSet(tally.Refused.Map(static row => row.Key));
        SweepTally planned = verdicts.Filter(v => !(v.Releases && refused.Contains(v.Key)))
            .Fold(SweepTally.Empty(cls, verdicts.Count), static (sum, v) => v.Book(sum));
        return planned with { Refused = tally.Refused.Count, Refusals = tally.Refused };
    }
}
```

| [INDEX] | [POLICY]        | [VALUE]                                                   | [BINDING]                                                 |
| :-----: | :-------------- | :-------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | sweep fold      | state-threaded newest-first fold                          | pure verdict list; re-fold resumes, no journal            |
|  [02]   | declared order  | fenced → expires → never-evict → age/count/size           | one joint discriminant over the loss capability set       |
|  [03]   | reachability GC | mark over EVERY AS-OF cut                                 | full-history, never head; historical refs survive (`H10`) |
|  [04]   | one executor    | `Execute` (set `evict` + `demote`)                        | every lane, operator purge, demotion routes through       |
|  [05]   | re-drive        | class `RedrivePolicy` on both effect arrows               | kernel `Redrive.Run`; no hand loop, no delay window       |
|  [06]   | cold-tiering    | `Cool` demotes never-evict; `EvictOrphan` collects debris | `Demote` ladder; orphan gated on declared `OrphanAge`     |
|  [07]   | holds           | first-class, late-bound, union                            | a hold today protects tomorrow's admissions               |
|  [08]   | conservation    | `inventory = kept + held + cooled + evicted + refused`    | one `Book` fold; refused bytes stay resident              |
|  [09]   | sweep grain     | rows, or partitions on a `RollingWindow` family           | one `DropPartition` per aged partition; one executor      |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
