# [PERSISTENCE_VERSION_RETENTION]

`RetentionClass` binds storage lane, schedule, classification ceiling, loss policy, and identity scheme for every durable artifact. `RetentionCatalog` owns admission. `RetentionSweep` derives one conserved verdict partition, marks reachability from reconstructed cuts or Marten event tags, and executes every eviction or cold-tier demotion through one receipt rail. Full-history reachability shields referenced content; `OrphanAge` gates never-referenced debris.

## [01]-[INDEX]

- [01]-[RETENTION_CLASSES]: the closed class axis, the five-decision class row, the seam-local classification-ceiling rank table, identity-scheme behavioral families, and the budget/loss policies.
- [02]-[SWEEP_AND_GC]: the pure state-threaded verdict fold, first-class holds, the full-history reachability GC, and the one receipted deletion executor every lane routes through.

## [02]-[RETENTION_CLASSES]

- Owner: `RetentionClass` the `[SmartEnum<string>]` artifact-class axis carrying its five decisions (storage lane, retention schedule, classification ceiling, loss policy, identity scheme); `StorageLane` the `[SmartEnum<string>]` durable-home axis; `LossPolicy` the receipted-evict/declared-expiry/never-evict vocabulary whose BOTH columns the sweep dispatch consumes; `IdentityScheme` the content-keyed-versus-name-plus-epoch vocabulary carrying the `Identity` mint (the name-plus-epoch key derives through the kernel `ContentHash.Of` seed-zero entry); `RetentionCeiling` the static frozen sensitivity-rank table that supplies the ordering `DataClassification` does not carry; `RetentionFault` the closed admission fault; `RetentionCatalog` the static surface owning the one-fold admission (classify-check, identity-derive, race-admit, lane-write).
- Cases: the canonical class set closes at six rows — `snapshot` (sealed AS-OF snapshot, `SnapshotArchive` lane, content-keyed, newest-N), `stream` (Marten event stream, `EventStream` lane, append-only, never evicted), `blob` (geometry/coverage object, `ObjectStore` lane, content-keyed, never-evict cold-tiering, full-history-reachable), `evidence` (incident bundle, `SnapshotArchive` lane, name-plus-epoch, declared-expiry), `cache` (transient content blob, `ObjectStore` lane, content-keyed, receipted-evict), `ephemeral` (presence/awareness, `Transient` lane, name-plus-epoch, declared-expiry, never durable); a class fitting no row is an admission rejection, never a default; class membership is immutable, reclassification is export-then-readmit so every lived lifecycle stays receipted.
- Entry: `public static Fin<RetentionFact> Admit(RetentionClass cls, ContentAddress contentKey, string name, ulong epoch, DataClassification stamp, long bytes, StorageTier tier, Func<ContentAddress, bool> resident, ProjectionContext frame)` is the one admission fold (the artifact's current `StorageTier` rides into the fact so the sweep cold-tiering verdict reads it; the catalog key is minted IN the fold through `cls.Scheme.Identity(contentKey, name, epoch)` — the content-keyed scheme passes the content address through and reads neither `name` nor `epoch`, the name-plus-epoch scheme derives `ContentHash.Of(name ++ epoch)` — so a caller never pre-mints identity); `public RetentionSchedule Schedule { get; }` projects the class's sweep cadence and budget; `public static bool RetentionCeiling.Ranked(DataClassification stamp)` is the fail-closed mapped-ness gate and `public static bool RetentionCeiling.Admits(DataClassification stamp, DataClassification ceiling)` the seam-local sensitivity comparison the admission fold reads directly (no per-class forwarder, no `Exceeds`).
- Auto: admission is one fold — classify-check (an UNRANKED stamp — a newer upstream `DataClassification` tier this seam rank table has not yet ordered — rejects `Unstamped` BEFORE the compare because absence of a seam rank is not clearance; a ranked stamp exceeding the ceiling rejects `CeilingBreach`), identity-derive (`cls.Scheme.Identity` — the content-keyed scheme IS its content address, the name-plus-epoch scheme mints its catalog key from the stable name plus admission epoch through the ONE kernel `ContentHash.Of` seed-zero entry, never a second hasher), race-admit (content-keyed classes get dedup and race-loser disposal free, name-plus-epoch classes get versioned replacement free, zero conditional code), lane-write; the sensitivity rank is a frozen `RetentionCeiling` table keyed by `DataClassification` because the AppHost taxonomy carries only a `RedactorKind` column and no ordinal — the ordering the "escalating sensitivity" doctrine asserts lives HERE as a policy value, never re-derived per call; byte counts record from the artifact's own sealed length fields (`SnapshotCatalogRow.StoredLength`, `ChunkManifest.Length`, `BlobResidence.Length`), never a later filesystem stat.
- Receipt: an admission rides `store.retention.admit` carrying the class and bytes; a ceiling breach rides `store.retention.reject` carrying the stamp and ceiling; an unranked stamp rides `store.retention.reject` carrying the key.
- Packages: Marten (`EventTagQuery`/`QueryByTagsAsync` adapter), Rasm (`Rasm.Domain` `ContentHash.Of` — the name-plus-epoch identity mint, [B]), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` rows + `Switch` + `Items`), LanguageExt.Core (`Fin`/`Option`), NodaTime (`Duration`/`Instant`), BCL inbox.
- Growth: a new artifact class is one `RetentionClass` row carrying its five decisions; a new durable home is one `StorageLane` row; a new loss policy is one `LossPolicy` row; a new sensitivity tier is one `RetentionCeiling` rank entry keyed by the AppHost `DataClassification` row; a new cold-tier rung is one `RetentionCeiling.Colder` ladder entry over the blobstore `StorageTier` vocabulary; zero new surface — a per-artifact retention table, a second classification taxonomy, an ordinal added to `DataClassification` upstream (the rank is a Persistence policy, not an AppHost concern), a `StorageTier`-ordering owner duplicated from blobstore, or a default class for an unfit artifact is the deleted form because the class set is closed and admission is one fold over the five decisions.
- Boundary: every stored thing belongs to exactly one class row carrying five decisions, the storage lane naming its durable home (`SnapshotArchive` for sealed artifacts, `EventStream` for the Marten system of record, `ObjectStore` for content-keyed blobs and cache, `Transient` for awareness) so the sweep budgets and the deletion executor route by lane row, never a `cls.Key == "blob"` string compare; the identity scheme alone yields two complete behavioral families (content-keyed classes get dedup and race-loser disposal free, name-plus-epoch classes get versioned replacement free); a budget breach truncates with an embedded receipt (capture must succeed degraded) while a ceiling breach rejects outright (security never degrades), the two overflow responses never interchangeable; the sensitivity comparison is seam-local because `DataClassification` is the AppHost redaction taxonomy whose only column is its `RedactorKind` — `RetentionCeiling.Admits` supplies the rank rather than calling a non-existent `Exceeds`, and an UNRANKED classification (a newer upstream tier this seam table has not ordered) is rejected as a distinct `Unstamped` fault, fail-closed, rather than silently collapsed to `int.MaxValue` and reported as a `CeilingBreach` that never compared — absence of a seam rank is not clearance; import re-verifies stamps so an export round-trip cannot launder a ceiling; the `stream` class is append-only and never evicted because the Marten event stream is the system of record (only the AS-OF snapshot density and the blob reachability are reclaimable); the `blob` class is content-keyed, `NeverEvict`, and full-history-reachable so a geometry blob a historical version references is never collected — an aged one cold-tiers down the `StorageTier` ladder and only never-referenced crash debris collects through the orphan pass (`#SWEEP_AND_GC`) — and the `Store/blobstore#BLOB_GC` lane registers its `BlobCatalogRow` in this class so the one GC governs both the snapshot spine and the geometry object store.

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
// reads `error.IsType<RetentionFault.Unstamped>()` / `error.HasCode(8283)` / `error.Category()`, never a message
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
public readonly record struct RetentionFact(RetentionClass Class, ContentAddress Key, long Bytes, StorageTier Tier, Instant At);

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
    // per age threshold, idempotent at the floor, so an aged-but-reachable geometry blob colds-tiers rather than collects.
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

- Owner: `SweepVerdict` the closed per-artifact verdict carrying the artifact's own byte figure; `Hold` the first-class hold row (whole-class, identity-set, stamp-range selectors composing by union); `SweepReceipt` the run summary proving `inventory = kept + held + cooled + evicted`; `Reachability` the full-history mark surface; `RetentionSweep` the static surface owning the pure state-threaded verdict fold and the one receipted mutation executor (delete plus cold-tier demote).
- Cases: `SweepVerdict` is `Kept | Held | HeldOverBudget | EvictAge | EvictCount | EvictSize | EvictAdministrative | EvictOrphan | Cool`; held bytes count against the budget but cannot evict so preservation pressure surfaces as `HeldOverBudget` rather than displacing onto unheld artifacts; `EvictAdministrative` is the operator-deletion verdict so a manual purge rides the same single executor and receipt stream rather than a side channel; `EvictOrphan` is the never-evict class's ONLY collection — an unreachable artifact no AS-OF cut ever referenced (crash debris, a race-loser row) past the class's DECLARED `RetentionSchedule.OrphanAge`, so a `NeverEvict` artifact younger than its orphan threshold stays non-evictable, `H10` holds for every referenced blob, and the crash-window loop still closes; `Cool` is the never-evict cold-tiering verdict (`H10`'s geometry-GC-forbidden alternative made real) carrying the `From`/`To` `StorageTier` so an aged never-evict artifact demotes one rung rather than collects or keeps-forever; every evict verdict and `HeldOverBudget` carries both the `Key` and the artifact `Bytes` so the executor emits a truthful per-eviction byte figure and the run summary sums real reclaimed bytes; the reachability mark is `Reachable | Orphan` over the full-history cut set.
- Entry: `public static (Seq<SweepVerdict> Verdicts, SweepReceipt Receipt) Run(RetentionClass cls, Seq<RetentionFact> inventory, Seq<Hold> holds, Reachability live, Func<ContentAddress, bool> eligible, Instant now, Guid correlation)` is the pure verdict fold; `public static IO<LanguageExt.HashSet<ContentAddress>> Mark(ReachabilitySource source)` dispatches one full-history reachability entry over reconstructed `Cuts` or a Marten `EventTagQuery` `Tags` adapter; `public static IO<SweepReceipt> Execute(RetentionClass cls, Seq<SweepVerdict> verdicts, Func<ContentAddress, IO<Unit>> evict, Func<ContentAddress, StorageTier, IO<Unit>> demote, ProjectionContext frame)` is the one receipted mutation executor — every eviction routing through `evict`, every cold-tier demotion through `demote` — the returned receipt carrying the sweep's real `RetentionClass`, the real reclaimed bytes, and the frame correlation.
- Auto: the sweep is one pure state-threaded `Fold` walking the inventory newest-first (`OrderByDescending` on the admission `Instant`) — holds and fenced keys exit first (yielding `Held`, or `HeldOverBudget` once their running bytes clear the budget), the reachability mark shielding ONLY a collecting class (`LossPolicy.Collects`); a DECLARED-EXPIRY class then expires at its declared bound (an artifact past `AgeBound` yields `EvictAge` — the declared expiry IS the age eviction, and budget pressure never expires a declared-expiry artifact early because capture-side truncation owns the budget response); a NEVER-EVICT class consumes the mark itself — an unreachable, fence-cleared artifact past its declared `OrphanAge` yields `EvictOrphan` (a younger orphan is `Kept` — the age condition is the schedule's own column, never a policy hidden in the key-only eligibility predicate) and a reachable one runs the cold-tiering arm GATED on `Lane.Durable` (a durable-lane artifact past `AgeBound` whose `StorageTier` can still demote yields `Cool`; a non-durable lane has no colder home so the arm never fires there; an already-coldest or still-young one is `Kept`); a RECEIPTED-EVICT class takes the first deciding verdict in the declared order (age past `AgeBound`, then count past `CountBound`, then size past `ByteBudget`); the newest-first walk threads `(LiveCount, RunningBytes)` over the retained-newest so `EvictCount` fires once `CountBound` newer survivors are already kept and `EvictSize` once running bytes clears `ByteBudget`, which keeps newest-N and evicts the OLDEST beyond budget in one pass (the size and count stages demand opposite walk directions under an ascending walk — newest-first reconciles both), every evict verdict carrying the artifact's own bytes and the prepend-built ledger reading oldest-first; a `Cool` is RETAINED (the bytes stay resident, demoted) so it threads `Live`/`Bytes` exactly like `Kept`; verdicts are a pure function of the inventory snapshot, the policy snapshot, the hold rows, and the eligibility predicate under one clock instant, so the verdict list is a testable value and a partial sweep resumes by re-folding with no journal; the reachability mark runs over EVERY AS-OF cut, not head — a content key referenced by any historical `TimeCut`'s reconstructed graph is `Reachable` and never collected, so a blob a prior version still references survives even after head drops it; blob bytes delete after the catalog row commit (the crash window produces collectible orphans, never dangling rows) and the age-gated orphan pass closes the loop.
- Receipt: every removed artifact emits `(class, identity, deciding rule, bytes)` and every demotion `(class, identity, "cool", from-tier, to-tier)`; the run summary proves `inventory = kept + held + cooled + evicted`; unreceipted deletion OR demotion anywhere is a rail rejection, and the receipt stream is itself a count-and-age-bounded class closing meta-retention at depth one.
- Packages: LanguageExt.Core (`Seq`/`Fold`/`IO`/`TraverseM`/`HashSet`/`Option`), Thinktecture.Runtime.Extensions (`[Union]` + `Switch`), NodaTime, BCL inbox.
- Growth: a new sweep rule is one stage in the declared verdict order; a new hold selector is one `Hold` case; a new deletion provenance is one `SweepVerdict` evict case (as `EvictAdministrative` and `EvictOrphan` are); a new preservation-side transition is one retaining `SweepVerdict` case (as `Cool` is) plus one executor delegate; zero new surface — a second sweeper, a head-only GC, an unreceipted cleanup, a tier-transition side channel beside the one executor, or an export-to-preserve workaround is the deleted form because the sweep is the single mutation executor and the GC marks over the full history.
- Boundary: the reachability GC runs over the FULL event history, not head (`H10`) — `Mark` folds the referenced content keys of every AS-OF cut's reconstructed graph so a geometry blob or snapshot a historical version references is `Reachable` and survives, and a head-only GC that collects a blob a prior version still cites is the deleted form; the alternative permitted by `H10` is geometry-GC-forbidden (dedup-plus-cold-tiering with no collection), expressed as a `blob`-class schedule whose `LossPolicy.NeverEvict` makes the age-threshold a `Cool` cold-tier demotion (the `RetentionCeiling.Demote` ladder over the blobstore `StorageTier`, re-PUT through the `Execute` `demote` delegate) rather than an eviction — the landed `Blob` row IS that schedule, and a `NeverEvict` class that merely keeps-forever, or a prose-only "tiering" with no verdict, is the deleted thin slice; reachability shields ONLY a collecting class — the never-evict arm consumes the mark as its orphan/cool discriminant, so `EvictOrphan` collects ONLY an artifact no cut ever referenced and never a referenced blob (`H10` holds structurally), while the `stream` class never reaches that arm because its cadence never schedules a sweep and the SoR fence rides the injected eligibility predicate; the sweep dispatch consumes BOTH `LossPolicy` columns, the derived `Collects` shield, and the lane's `Durable` flag — `Expires` selects the declared-expiry arm (the artifact evicts AT its declared `AgeBound`, so an aged evidence bundle or awareness row expires instead of nonsensically cold-tiering), and the `Cool` arm demands `Lane.Durable` because a tier re-PUT is a durable-home operation a `Transient`-lane artifact cannot take — a captured-but-never-read policy column is the deleted illusory form; holds are first-class rows bound late at sweep time so a hold placed today protects artifacts admitted tomorrow, release deletes the row with no eviction side effect, and every run emits an active-hold inventory because forgotten holds are the dominant retention failure; the executor is the one mutation surface every lane routes through (a snapshot sweep, a blob GC, a cache eviction, an operator purge through `EvictAdministrative`, a cold-tier demotion through `Cool`) so the receipt stream is the complete lifecycle ledger; eligibility predicates inject (sync fences, projection floors, export pins, the `Store/blobstore#BLOB_GC` WORM/object-lock fence holding a blob under an active retention-until) so the sweep owns zero domain-safety rules and every refusal names the predicate that held it — the orphan AGE condition is NOT one of them: age is a fact the sweep already holds (`fact.At` against `Schedule.OrphanAge`), and a key-only predicate cannot see it, so smuggling the age policy into `eligible` is the deleted form; the injected `evict` arrow is itself a lane-owned effect that can fail with a lane-specific typed fault (the blob lane's `WormEvict` surfaces `Store/blobstore#OBJECT_STORE` `RemoteStoreFault.Locked` when a compliance-window blob is targeted, the defense-in-depth second gate behind the eligibility fence) — `Execute` lifts that fault through the run rail rather than swallowing it, so a WORM violation is a typed refusal on the receipt stream, never a silent skip or a generic provider 403.

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
    // The cold-tiering verdict (`H10`: geometry-GC-forbidden = dedup-plus-cold-tiering): a `NeverEvict`-class artifact past
    // its `AgeBound` whose `StorageTier` can still demote rides `Cool` carrying the next-colder tier, so eviction is REPLACED
    // by a tier transition the `Execute` `demote` delegate re-PUTs at — preservation pressure on a never-evict class flows
    // to colder storage, never to collection or to displacing onto unheld artifacts.
    public sealed record Cool(ContentAddress Key, long Bytes, StorageTier From, StorageTier To) : SweepVerdict;

    public abstract ContentAddress Key { get; }
    public abstract long Bytes { get; }
    public bool Evicts => this is EvictAge or EvictCount or EvictSize or EvictAdministrative or EvictOrphan;
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

public readonly record struct SweepReceipt(RetentionClass Class, int Inventory, int Kept, int Held, int Cooled, int Evicted, long EvictedBytes, Instant At, Guid Correlation) {
    // A cooled artifact is RETAINED (demoted, not collected), so it partitions with kept/held — the conservation identity
    // closes over all four retention-side counts plus the evicted count, never silently dropping the cold-tiering rung.
    public bool Conserves => Inventory == (Kept + Held + Cooled + Evicted);
}

public static class RetentionSweep {
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
    // response); only a RECEIPTED-EVICT class runs the age/count/size eviction ladder.
    static SweepVerdict Decide(RetentionClass cls, (Seq<SweepVerdict> Ledger, int Live, long Bytes) state, RetentionFact fact, Seq<Hold> holds, Reachability live, Func<ContentAddress, bool> eligible, Instant now) =>
        holds.Exists(h => h.Holds(fact)) || !eligible(fact.Key) || (cls.Loss.Collects && live.Reachable(fact.Key))
            ? (state.Bytes + fact.Bytes) > cls.Schedule.ByteBudget
                ? new SweepVerdict.HeldOverBudget(fact.Key, fact.Bytes, (state.Bytes + fact.Bytes) - cls.Schedule.ByteBudget)
                : new SweepVerdict.Held(fact.Key, fact.Bytes, "hold-or-reachable")
            : cls.Loss.Expires
                ? (now - fact.At) >= cls.Schedule.AgeBound
                    ? new SweepVerdict.EvictAge(fact.Key, fact.Bytes, now - fact.At)
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
                        ? new SweepVerdict.EvictAge(fact.Key, fact.Bytes, now - fact.At)
                        : (state.Live + 1) > cls.Schedule.CountBound
                            ? new SweepVerdict.EvictCount(fact.Key, fact.Bytes, state.Live + 1)
                            : (state.Bytes + fact.Bytes) > cls.Schedule.ByteBudget
                                ? new SweepVerdict.EvictSize(fact.Key, fact.Bytes, (state.Bytes + fact.Bytes) - cls.Schedule.ByteBudget)
                                : new SweepVerdict.Kept(fact.Key);

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
        select new SweepReceipt(cls, verdicts.Count, verdicts.Count(static v => v is SweepVerdict.Kept), verdicts.Count(static v => v.Retains), verdicts.Count(static v => v.Cools), freed.Count, freed.Sum(), frame.Now(), frame.Correlation);
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
