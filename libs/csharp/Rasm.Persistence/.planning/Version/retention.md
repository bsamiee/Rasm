# [PERSISTENCE_VERSION_RETENTION]

Rasm.Persistence owns the lifecycle of every durable artifact through one classification-and-retention sweep whose reachability GC runs over the FULL event history, never head: a content-addressed snapshot, a Marten event stream, a geometry blob (its content-defined chunks keyed under the same content-keyed lane), an evidence bundle, a cache blob, or an ephemeral awareness row belongs to exactly one `RetentionClass` row carrying its five decisions — storage lane, retention schedule, classification ceiling, loss policy, and identity scheme; the sweep is a pure single-pass state-threaded fold (holds and reachable keys exit first; a never-evict class past its age cold-tiers one rung; an evicting-class survivor takes the first deciding verdict in the declared age → count → size order, the size stage evicting oldest-first until under budget); and the reachability GC marks every content key referenced by ANY AS-OF cut over the whole event stream so a blob a historical version still references is never collected — geometry GC over head alone is forbidden, and the only permitted alternative is dedup-plus-cold-tiering, the `Cool` verdict demoting an aged never-evict artifact down the `StorageTier` ladder instead of collecting it. This is the GC owner the `Store/blobstore#BLOB_GC` geometry object store feeds its content-lineage catalog into (`H10` — the object store gets the same content-lineage and retention catalog row the snapshot spine has, and GC reachability runs over every AS-OF cut), and the one receipted mutation executor every lane routes through (delete plus cold-tier demote) so the receipt stream is the system's complete deletion, demotion, and non-deletion ledger. `DataClassification` (the `[SmartEnum<string>]` redaction taxonomy with no ordinal column, so the sensitivity comparison is a seam-local rank table here) arrives from AppHost; `ContentAddress` (`[ValueObject<UInt128>]`), `SnapshotCatalogRow`, and the `ChunkManifest` content keys arrive from `Element/codec`; the `TimeCut` AS-OF cut set arrives from `Version/timetravel`; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId` arrive from AppHost. Export-then-readmit reclassification is THIS owner's own admission re-run (class membership is immutable, so a reclassification exports the artifact and re-`Admit`s it under the new class, the import re-verifying its stamp so the round-trip cannot launder a ceiling), the two-tier cache lane mechanics ride `Query/cache`, and incident capture rides AppHost diagnostics — this owner classifies their artifacts and is the sole deletion executor, never re-implementing their capture.

## [01]-[INDEX]

- [01]-[RETENTION_CLASSES]: the closed class axis, the five-decision class row, the seam-local classification-ceiling rank table, identity-scheme behavioral families, and the budget/loss policies.
- [02]-[SWEEP_AND_GC]: the pure state-threaded verdict fold, first-class holds, the full-history reachability GC, and the one receipted deletion executor every lane routes through.

## [02]-[RETENTION_CLASSES]

- Owner: `RetentionClass` the `[SmartEnum<string>]` artifact-class axis carrying its five decisions (storage lane, retention schedule, classification ceiling, loss policy, identity scheme); `StorageLane` the `[SmartEnum<string>]` durable-home axis; `LossPolicy` the receipted-evict-versus-declared-expiry vocabulary; `IdentityScheme` the content-keyed-versus-name-plus-epoch vocabulary; `RetentionCeiling` the static frozen sensitivity-rank table that supplies the ordering `DataClassification` does not carry; `RetentionFault` the closed admission fault; `RetentionCatalog` the static surface owning the one-fold admission (classify-check, identity-derive, race-admit, lane-write).
- Cases: the canonical class set closes at six rows — `snapshot` (sealed AS-OF snapshot, `SnapshotArchive` lane, content-keyed, newest-N), `stream` (Marten event stream, `EventStream` lane, append-only, never evicted), `blob` (geometry/coverage object, `ObjectStore` lane, content-keyed, full-history-reachable), `evidence` (incident bundle, `SnapshotArchive` lane, name-plus-epoch, declared-expiry), `cache` (transient content blob, `ObjectStore` lane, content-keyed, receipted-evict), `ephemeral` (presence/awareness, `Transient` lane, name-plus-epoch, declared-expiry, never durable); a class fitting no row is an admission rejection, never a default; class membership is immutable, reclassification is export-then-readmit so every lived lifecycle stays receipted.
- Entry: `public static Fin<RetentionFact> Admit(RetentionClass cls, ContentAddress key, DataClassification stamp, long bytes, StorageTier tier, Func<ContentAddress, bool> resident, ClockPolicy clocks)` is the one admission fold (the artifact's current `StorageTier` rides into the fact so the sweep cold-tiering verdict reads it); `public RetentionSchedule Schedule { get; }` projects the class's sweep cadence and budget; `public static bool RetentionCeiling.Ranked(DataClassification stamp)` is the fail-closed mapped-ness gate and `public static bool RetentionCeiling.Admits(DataClassification stamp, DataClassification ceiling)` the seam-local sensitivity comparison the admission fold reads directly (no per-class forwarder, no `Exceeds`).
- Auto: admission is one fold — classify-check (an UNRANKED stamp — a newer upstream `DataClassification` tier this seam rank table has not yet ordered — rejects `Unstamped` BEFORE the compare because absence of a seam rank is not clearance; a ranked stamp exceeding the ceiling rejects `CeilingBreach`), identity-derive (content-key or name-plus-epoch per the scheme), race-admit (content-keyed classes get dedup and race-loser disposal free, name-plus-epoch classes get versioned replacement free, zero conditional code), lane-write; the sensitivity rank is a frozen `RetentionCeiling` table keyed by `DataClassification` because the AppHost taxonomy carries only a `RedactorKind` column and no ordinal — the ordering the "escalating sensitivity" doctrine asserts lives HERE as a policy value, never re-derived per call; byte counts record from the artifact's own sealed length fields (`SnapshotCatalogRow.StoredLength`, `ChunkManifest.Length`, `BlobResidence.Length`), never a later filesystem stat.
- Receipt: an admission rides `store.retention.admit` carrying the class and bytes; a ceiling breach rides `store.retention.reject` carrying the stamp and ceiling; an unranked stamp rides `store.retention.reject` carrying the key.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` rows + `Switch` + `Items`), LanguageExt.Core (`Fin`/`Option`), NodaTime (`Duration`/`Instant`), System.IO.Hashing, BCL inbox.
- Growth: a new artifact class is one `RetentionClass` row carrying its five decisions; a new durable home is one `StorageLane` row; a new loss policy is one `LossPolicy` row; a new sensitivity tier is one `RetentionCeiling` rank entry keyed by the AppHost `DataClassification` row; a new cold-tier rung is one `RetentionCeiling.Colder` ladder entry over the blobstore `StorageTier` vocabulary; zero new surface — a per-artifact retention table, a second classification taxonomy, an ordinal added to `DataClassification` upstream (the rank is a Persistence policy, not an AppHost concern), a `StorageTier`-ordering owner duplicated from blobstore, or a default class for an unfit artifact is the deleted form because the class set is closed and admission is one fold over the five decisions.
- Boundary: every stored thing belongs to exactly one class row carrying five decisions, the storage lane naming its durable home (`SnapshotArchive` for sealed artifacts, `EventStream` for the Marten system of record, `ObjectStore` for content-keyed blobs and cache, `Transient` for awareness) so the sweep budgets and the deletion executor route by lane row, never a `cls.Key == "blob"` string compare; the identity scheme alone yields two complete behavioral families (content-keyed classes get dedup and race-loser disposal free, name-plus-epoch classes get versioned replacement free); a budget breach truncates with an embedded receipt (capture must succeed degraded) while a ceiling breach rejects outright (security never degrades), the two overflow responses never interchangeable; the sensitivity comparison is seam-local because `DataClassification` is the AppHost redaction taxonomy whose only column is its `RedactorKind` — `RetentionCeiling.Admits` supplies the rank rather than calling a non-existent `Exceeds`, and an UNRANKED classification (a newer upstream tier this seam table has not ordered) is rejected as a distinct `Unstamped` fault, fail-closed, rather than silently collapsed to `int.MaxValue` and reported as a `CeilingBreach` that never compared — absence of a seam rank is not clearance; import re-verifies stamps so an export round-trip cannot launder a ceiling; the `stream` class is append-only and never evicted because the Marten event stream is the system of record (only the AS-OF snapshot density and the blob reachability are reclaimable); the `blob` class is content-keyed and full-history-reachable so a geometry blob a historical version references is never collected (`#SWEEP_AND_GC`), and the `Store/blobstore#BLOB_GC` lane registers its `BlobCatalogRow` in this class so the one GC governs both the snapshot spine and the geometry object store.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
// `RetentionFault` derives from the KERNEL federation base, so the bare `Expected` names `Rasm.Domain.Expected`
// (parameterless protected ctor + `Category` virtual) and NEVER the `LanguageExt.Common.Expected` whose
// `(string,int,Option)` ctor is the deleted form.
using Expected = Rasm.Domain.Expected;

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
}

[SmartEnum]
public sealed partial class IdentityScheme {
    public static readonly IdentityScheme ContentKeyed = new(dedups: true);
    public static readonly IdentityScheme NamePlusEpoch = new(dedups: false);
    public bool Dedups { get; }
}

// The retention fault band (828x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless protected ctor;
// `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the Persistence-sibling
// `Element/codec#SNAPSHOT_SPINE` `CodecFault` (83xx) and `Element/identity#SCHEMA_VERDICT` `IdentityFault` (834x) realize
// — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)` ctor (no `Category` to
// override) is the deleted form. Band membership is a per-case `Code => 828x` override, `Message`/`Category` projecting
// through the generated `Switch`, so the typed case lifts BARE onto `Fin<T>` with no `.ToError()` hop and a recovery
// reads `error.IsType<RetentionFault.Unstamped>()` / `error.HasCode(8283)` / `error.Category()`, never a message
// substring. `[SkipUnionOps]` is the canonical fault-band annotation. `Create` is the IValidationError admission the
// generated converter bridge calls on a deserialization reject — `Unclassed` (an admitted artifact never mints it
// directly); `CeilingBreach` is a RANKED stamp whose seam rank exceeds the class ceiling (the genuine sensitivity
// comparison); `Unstamped` is an UNRANKED stamp the seam table does not order (fail-closed — a `CeilingBreach` reporting
// a comparison that never happened is the deleted form).
[SkipUnionOps]
[Union]
public abstract partial record RetentionFault : Expected, IValidationError<RetentionFault> {
    private RetentionFault() : base() { }
    public sealed record Unclassed(string Artifact) : RetentionFault;
    public sealed record CeilingBreach(DataClassification Stamp, DataClassification Ceiling) : RetentionFault;
    public sealed record Unstamped(ContentAddress Key) : RetentionFault;

    public override int Code => Switch(
        unclassed:     static _ => 8281,
        ceilingBreach: static _ => 8282,
        unstamped:     static _ => 8283);

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

public readonly record struct RetentionSchedule(Duration Cadence, long ByteBudget, int CountBound, Duration AgeBound);
// The admitted artifact's measured fact. `Tier` is its CURRENT durable storage tier (the `Store/blobstore#OBJECT_STORE`
// `StorageTier` row the blob/snapshot lane sealed it at), read by the cold-tiering verdict so a `NeverEvict`-class artifact
// past its age demotes one rung instead of evicting and an artifact already at the coldest tier is `Kept` idempotently —
// a `Transient`/`EventStream`-lane fact rides `StorageTier.Standard` (the demotion ladder never reaches it). A fact with
// no tier field would foreclose the dedup-plus-cold-tiering alternative `H10` admits, the deleted thin slice.
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
    public static bool Admits(DataClassification stamp, DataClassification ceiling) => Ranked(stamp) && Of(stamp) <= Of(ceiling);

    // The cold-tiering demotion ladder — a retention POLICY (the lifecycle cadence lives here) over the blobstore-owned
    // `StorageTier` vocabulary: `Standard -> Infrequent -> Cold -> Archive`, `Archive` the floor returning `None`. The
    // `NeverEvict`-class cold-tiering alternative (`H10`: geometry-GC-forbidden = dedup-plus-cold-tiering) demotes one rung
    // per age threshold, idempotent at the floor, so an aged-but-reachable geometry blob colds-tiers rather than collects.
    static readonly FrozenDictionary<StorageTier, StorageTier> Colder = new[] {
        (StorageTier.Standard, StorageTier.Infrequent), (StorageTier.Infrequent, StorageTier.Cold), (StorageTier.Cold, StorageTier.Archive),
    }.ToFrozenDictionary(static t => t.Item1, static t => t.Item2);
    public static Option<StorageTier> Demote(StorageTier tier) => Colder.TryGetValue(tier, out var next) ? Some(next) : None;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetentionClass {
    public static readonly RetentionClass Snapshot = new("snapshot", StorageLane.SnapshotArchive, LossPolicy.ReceiptedEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, new RetentionSchedule(Duration.FromHours(6), 64L * 1024 * 1024 * 1024, 32, Duration.FromDays(365)));
    public static readonly RetentionClass Stream = new("stream", StorageLane.EventStream, LossPolicy.NeverEvict, IdentityScheme.ContentKeyed, DataClassification.Confidential, new RetentionSchedule(Duration.MaxValue, long.MaxValue, int.MaxValue, Duration.MaxValue));
    public static readonly RetentionClass Blob = new("blob", StorageLane.ObjectStore, LossPolicy.ReceiptedEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, new RetentionSchedule(Duration.FromHours(12), 512L * 1024 * 1024 * 1024, int.MaxValue, Duration.MaxValue));
    public static readonly RetentionClass Evidence = new("evidence", StorageLane.SnapshotArchive, LossPolicy.DeclaredExpiry, IdentityScheme.NamePlusEpoch, DataClassification.Confidential, new RetentionSchedule(Duration.FromDays(1), 8L * 1024 * 1024 * 1024, 256, Duration.FromDays(90)));
    public static readonly RetentionClass Cache = new("cache", StorageLane.ObjectStore, LossPolicy.ReceiptedEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, new RetentionSchedule(Duration.FromHours(1), 16L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromDays(7)));
    public static readonly RetentionClass Ephemeral = new("ephemeral", StorageLane.Transient, LossPolicy.DeclaredExpiry, IdentityScheme.NamePlusEpoch, DataClassification.Internal, new RetentionSchedule(Duration.FromMinutes(1), 1L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromMinutes(5)));

    public StorageLane Lane { get; }
    public LossPolicy Loss { get; }
    public IdentityScheme Scheme { get; }
    public DataClassification Ceiling { get; }
    public RetentionSchedule Schedule { get; }
    private RetentionClass(string key, StorageLane lane, LossPolicy loss, IdentityScheme scheme, DataClassification ceiling, RetentionSchedule schedule) : this(key) =>
        (Lane, Loss, Scheme, Ceiling, Schedule) = (lane, loss, scheme, ceiling, schedule);
}

public static class RetentionCatalog {
    // The one admission fold: classify-check (an UNRANKED stamp fails `Unstamped` fail-closed BEFORE the ceiling compare —
    // absence of a seam rank is not clearance; a ranked-but-exceeding stamp fails `CeilingBreach`), identity-derive +
    // race-admit (a content-keyed class dedups a resident key to a zero-byte fact, a name-plus-epoch class versions-replaces),
    // lane-write. The artifact's CURRENT `StorageTier` rides the fact so the sweep's cold-tiering verdict reads it.
    public static Fin<RetentionFact> Admit(RetentionClass cls, ContentAddress key, DataClassification stamp, long bytes, StorageTier tier, Func<ContentAddress, bool> resident, ClockPolicy clocks) =>
        !RetentionCeiling.Ranked(stamp)
            ? Fin<RetentionFact>.Fail(new RetentionFault.Unstamped(key))
            : !RetentionCeiling.Admits(stamp, cls.Ceiling)
                ? Fin<RetentionFact>.Fail(new RetentionFault.CeilingBreach(stamp, cls.Ceiling))
                : Fin<RetentionFact>.Succ(new RetentionFact(cls, key, cls.Scheme.Dedups && resident(key) ? 0L : bytes, tier, clocks.Now));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                      | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | class membership    | one of six closed `RetentionClass` rows      | unfit artifact rejects, never defaults                    |
|  [02]   | storage lane        | `StorageLane` row per class                  | sweep + delete route by lane, never a key string compare  |
|  [03]   | sensitivity rank    | seam-local `RetentionCeiling` frozen table   | `DataClassification` carries no ordinal; rank lives here  |
|  [04]   | identity scheme     | content-keyed vs name-plus-epoch             | dedup/race-disposal vs versioned-replacement, zero branch |
|  [05]   | ceiling vs budget   | ceiling rejects, budget truncates            | security never degrades; capture succeeds degraded        |
|  [06]   | stream class        | append-only, never evicted                   | the event SoR; only snapshots and blobs reclaim           |

## [03]-[SWEEP_AND_GC]

- Owner: `SweepVerdict` the closed per-artifact verdict carrying the artifact's own byte figure; `Hold` the first-class hold row (whole-class, identity-set, stamp-range selectors composing by union); `SweepReceipt` the run summary proving `inventory = kept + held + cooled + evicted`; `Reachability` the full-history mark surface; `RetentionSweep` the static surface owning the pure state-threaded verdict fold and the one receipted mutation executor (delete plus cold-tier demote).
- Cases: `SweepVerdict` is `Kept | Held | HeldOverBudget | EvictAge | EvictCount | EvictSize | EvictAdministrative | Cool`; held bytes count against the budget but cannot evict so preservation pressure surfaces as `HeldOverBudget` rather than displacing onto unheld artifacts; `EvictAdministrative` is the operator-deletion verdict so a manual purge rides the same single executor and receipt stream rather than a side channel; `Cool` is the never-evict cold-tiering verdict (`H10`'s geometry-GC-forbidden alternative made real) carrying the `From`/`To` `StorageTier` so an aged never-evict artifact demotes one rung rather than collects or keeps-forever; every evict verdict and `HeldOverBudget` carries both the `Key` and the artifact `Bytes` so the executor emits a truthful per-eviction byte figure and the run summary sums real reclaimed bytes; the reachability mark is `Reachable | Orphan` over the full-history cut set.
- Entry: `public static (Seq<SweepVerdict> Verdicts, SweepReceipt Receipt) Sweep(RetentionClass cls, Seq<RetentionFact> inventory, Seq<Hold> holds, Reachability live, Func<ContentAddress, bool> eligible, Instant now, CorrelationId correlation)` is the pure verdict fold; `public static LanguageExt.HashSet<ContentAddress> Mark(Func<TimeCut, Seq<ContentAddress>> referencedAt, Seq<TimeCut> everyCut)` is the full-history reachability mark; `public static IO<SweepReceipt> Execute(RetentionClass cls, Seq<SweepVerdict> verdicts, Func<ContentAddress, IO<Unit>> evict, Func<ContentAddress, StorageTier, IO<Unit>> demote, CorrelationId correlation, ClockPolicy clocks)` is the one receipted mutation executor — every eviction routing through `evict`, every cold-tier demotion through `demote` — the returned receipt carrying the sweep's real `RetentionClass`, the real reclaimed bytes, and the threaded `CorrelationId`.
- Auto: the sweep is one pure state-threaded `Fold` walking the inventory newest-first (`OrderByDescending` on the admission `Instant`) — holds and reachable keys exit first (yielding `Held`, or `HeldOverBudget` once their running bytes clear the budget); a NEVER-EVICT class then runs the cold-tiering arm (an artifact past `AgeBound` whose `StorageTier` can still demote yields `Cool`, an already-coldest or still-young one `Kept`); an EVICTING class takes the first deciding verdict in the declared order (age past `AgeBound`, then count past `CountBound`, then size past `ByteBudget`); the newest-first walk threads `(LiveCount, RunningBytes)` over the retained-newest so `EvictCount` fires once `CountBound` newer survivors are already kept and `EvictSize` once running bytes would clear `ByteBudget`, which keeps newest-N and evicts the OLDEST beyond budget in one pass (the size and count stages would want opposite walk directions under an ascending walk — newest-first reconciles both), every evict verdict carrying the artifact's own bytes and the prepend-built ledger reading oldest-first; a `Cool` is RETAINED (the bytes stay resident, demoted) so it threads `Live`/`Bytes` exactly like `Kept`; verdicts are a pure function of the inventory snapshot, the policy snapshot, the hold rows, and the eligibility predicate under one clock instant, so the verdict list is a testable value and a partial sweep resumes by re-folding with no journal; the reachability mark runs over EVERY AS-OF cut, not head — a content key referenced by any historical `TimeCut`'s reconstructed graph is `Reachable` and never collected, so a blob a prior version still references survives even after head drops it; blob bytes delete after the catalog row commit (the crash window produces collectible orphans, never dangling rows) and the age-gated orphan pass closes the loop.
- Receipt: every removed artifact emits `(class, identity, deciding rule, bytes)` and every demotion `(class, identity, "cool", from-tier, to-tier)`; the run summary proves `inventory = kept + held + cooled + evicted`; unreceipted deletion OR demotion anywhere is a rail rejection, and the receipt stream is itself a count-and-age-bounded class closing meta-retention at depth one.
- Packages: LanguageExt.Core (`Seq`/`Fold`/`IO`/`TraverseM`/`HashSet`/`Option`), Thinktecture.Runtime.Extensions (`[Union]` + `Switch`), NodaTime, System.IO.Hashing, BCL inbox.
- Growth: a new sweep rule is one stage in the declared verdict order; a new hold selector is one `Hold` case; a new deletion provenance is one `SweepVerdict` evict case (as `EvictAdministrative` is); a new preservation-side transition is one retaining `SweepVerdict` case (as `Cool` is) plus one executor delegate; zero new surface — a second sweeper, a head-only GC, an unreceipted cleanup, a tier-transition side channel beside the one executor, or an export-to-preserve workaround is the deleted form because the sweep is the single mutation executor and the GC marks over the full history.
- Boundary: the reachability GC runs over the FULL event history, not head (`H10`) — `Mark` folds the referenced content keys of every AS-OF cut's reconstructed graph so a geometry blob or snapshot a historical version references is `Reachable` and survives, and a head-only GC that collects a blob a prior version still cites is the deleted form; the alternative permitted by `H10` is geometry-GC-forbidden (dedup-plus-cold-tiering with no collection), expressed as a `blob`-class schedule whose `LossPolicy.NeverEvict` makes the age-threshold a `Cool` cold-tier demotion (the `RetentionCeiling.Demote` ladder over the blobstore `StorageTier`, re-PUT through the `Execute` `demote` delegate) rather than an eviction — a `NeverEvict` class that merely keeps-forever, or a prose-only "tiering" with no verdict, is the deleted thin slice; holds are first-class rows bound late at sweep time so a hold placed today protects artifacts admitted tomorrow, release deletes the row with no eviction side effect, and every run emits an active-hold inventory because forgotten holds are the dominant retention failure; the executor is the one mutation surface every lane routes through (a snapshot sweep, a blob GC, a cache eviction, an operator purge through `EvictAdministrative`, a cold-tier demotion through `Cool`) so the receipt stream is the complete lifecycle ledger; eligibility predicates inject (sync fences, projection floors, export pins, the orphan age gate, the `Store/blobstore#BLOB_GC` WORM/object-lock fence holding a blob under an active retention-until) so the sweep owns zero domain-safety rules and every refusal names the predicate that held it; the injected `evict` arrow is itself a lane-owned effect that MAY fail with a lane-specific typed fault (the blob lane's `WormEvict` surfaces `Store/blobstore#OBJECT_STORE` `RemoteStoreFault.Locked` when a compliance-window blob is targeted, the defense-in-depth second gate behind the eligibility fence) — `Execute` lifts that fault through the run rail rather than swallowing it, so a WORM violation is a typed refusal on the receipt stream, never a silent skip or a generic provider 403.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepVerdict {
    private SweepVerdict() { }
    public sealed record Kept(ContentAddress Key) : SweepVerdict;
    public sealed record Held(ContentAddress Key, long Bytes, string By) : SweepVerdict;
    public sealed record HeldOverBudget(ContentAddress Key, long Bytes, long OverBy) : SweepVerdict;
    public sealed record EvictAge(ContentAddress Key, long Bytes, Duration Age) : SweepVerdict;
    public sealed record EvictCount(ContentAddress Key, long Bytes, int Rank) : SweepVerdict;
    public sealed record EvictSize(ContentAddress Key, long Bytes, long OverBy) : SweepVerdict;
    public sealed record EvictAdministrative(ContentAddress Key, long Bytes, string By) : SweepVerdict;
    // The cold-tiering verdict (`H10`: geometry-GC-forbidden = dedup-plus-cold-tiering): a `NeverEvict`-class artifact past
    // its `AgeBound` whose `StorageTier` can still demote rides `Cool` carrying the next-colder tier, so eviction is REPLACED
    // by a tier transition the `Execute` `demote` delegate re-PUTs at — preservation pressure on a never-evict class flows
    // to colder storage, never to collection or to displacing onto unheld artifacts.
    public sealed record Cool(ContentAddress Key, long Bytes, StorageTier From, StorageTier To) : SweepVerdict;

    public ContentAddress Key => this switch {
        Kept k => k.Key, Held h => h.Key, HeldOverBudget o => o.Key,
        EvictAge a => a.Key, EvictCount c => c.Key, EvictSize s => s.Key, EvictAdministrative d => d.Key, Cool l => l.Key };
    public long Bytes => this switch {
        Held h => h.Bytes, HeldOverBudget o => o.Bytes, EvictAge a => a.Bytes,
        EvictCount c => c.Bytes, EvictSize s => s.Bytes, EvictAdministrative d => d.Bytes, Cool l => l.Bytes, _ => 0L };
    public bool Evicts => this is EvictAge or EvictCount or EvictSize or EvictAdministrative;
    public bool Retains => this is Held or HeldOverBudget;
    public bool Cools => this is Cool;
    public string Rule => this switch {
        EvictAge => "age", EvictCount => "count", EvictSize => "size", EvictAdministrative => "administrative", Cool => "cool", _ => "kept" };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Hold {
    private Hold() { }
    public sealed record WholeClass(RetentionClass Class) : Hold;
    public sealed record IdentitySet(Set<ContentAddress> Keys) : Hold;
    public sealed record StampRange(Instant From, Instant Until) : Hold;

    public bool Holds(RetentionFact fact) => this switch {
        WholeClass c => c.Class == fact.Class,
        IdentitySet s => s.Keys.Contains(fact.Key),
        StampRange r => fact.At >= r.From && fact.At < r.Until,
    };
}

public readonly record struct Reachability(LanguageExt.HashSet<ContentAddress> Live) {
    public static readonly Reachability None = new(LanguageExt.HashSet<ContentAddress>.Empty);
    public bool Reachable(ContentAddress key) => Live.Contains(key);
}

public readonly record struct SweepReceipt(RetentionClass Class, int Inventory, int Kept, int Held, int Cooled, int Evicted, long EvictedBytes, Instant At, CorrelationId Correlation) {
    // A cooled artifact is RETAINED (demoted, not collected), so it partitions with kept/held — the conservation identity
    // closes over all four retention-side counts plus the evicted count, never silently dropping the cold-tiering rung.
    public bool Conserves => Inventory == Kept + Held + Cooled + Evicted;
}

public static class RetentionSweep {
    // The full-history reachability mark: a nested fold over every AS-OF cut's referenced content keys through the
    // idempotent `LanguageExt.HashSet.Add` (a present key is a no-op return of the same set — `TryAddRange` is not a
    // `LanguageExt.HashSet` member, the singular `Add`/`TryAdd` is, so the deleted form is a phantom bulk-add). A key
    // referenced by ANY historical cut is `Reachable` and survives, so a blob a prior version still cites is never collected.
    public static LanguageExt.HashSet<ContentAddress> Mark(Func<TimeCut, Seq<ContentAddress>> referencedAt, Seq<TimeCut> everyCut) =>
        everyCut.Fold(LanguageExt.HashSet<ContentAddress>.Empty, static (live, cut) => referencedAt(cut).Fold(live, static (set, key) => set.Add(key)));

    public static (Seq<SweepVerdict> Verdicts, SweepReceipt Receipt) Sweep(
        RetentionClass cls, Seq<RetentionFact> inventory, Seq<Hold> holds, Reachability live, Func<ContentAddress, bool> eligible, Instant now, CorrelationId correlation) {
        var scan = toSeq(inventory.OrderByDescending(static f => f.At))
            .Fold((Ledger: Seq<SweepVerdict>(), Live: 0, Bytes: 0L),
                (state, fact) => Advance(state, fact, Decide(cls, state, fact, holds, live, eligible, now)));
        var verdicts = scan.Ledger;
        var receipt = verdicts.Fold(new SweepReceipt(cls, inventory.Count, 0, 0, 0, 0, 0L, now, correlation), static (sum, v) =>
            v.Evicts ? sum with { Evicted = sum.Evicted + 1, EvictedBytes = sum.EvictedBytes + v.Bytes }
            : v.Cools ? sum with { Cooled = sum.Cooled + 1 }
            : v.Retains ? sum with { Held = sum.Held + 1 }
            : sum with { Kept = sum.Kept + 1 });
        return (verdicts, receipt);
    }

    static SweepVerdict Decide(RetentionClass cls, (Seq<SweepVerdict> Ledger, int Live, long Bytes) state, RetentionFact fact, Seq<Hold> holds, Reachability live, Func<ContentAddress, bool> eligible, Instant now) =>
        holds.Exists(h => h.Holds(fact)) || live.Reachable(fact.Key) || !eligible(fact.Key)
            ? state.Bytes + fact.Bytes > cls.Schedule.ByteBudget
                ? new SweepVerdict.HeldOverBudget(fact.Key, fact.Bytes, state.Bytes + fact.Bytes - cls.Schedule.ByteBudget)
                : new SweepVerdict.Held(fact.Key, fact.Bytes, "hold-or-reachable")
            // A NEVER-EVICT class past its age cold-tiers (one rung per pass, idempotent at `Archive`) rather than keeping
            // forever: the `H10` dedup-plus-cold-tiering alternative made real, NOT a silent `Kept`. A still-young or
            // already-coldest never-evict artifact is `Kept`. Only an EVICTING class runs the age/count/size eviction ladder.
            : !cls.Loss.Evicts
                ? now - fact.At >= cls.Schedule.AgeBound && RetentionCeiling.Demote(fact.Tier) is { IsSome: true, Case: StorageTier colder }
                    ? new SweepVerdict.Cool(fact.Key, fact.Bytes, fact.Tier, colder)
                    : new SweepVerdict.Kept(fact.Key)
                : now - fact.At >= cls.Schedule.AgeBound
                    ? new SweepVerdict.EvictAge(fact.Key, fact.Bytes, now - fact.At)
                    : state.Live + 1 > cls.Schedule.CountBound
                        ? new SweepVerdict.EvictCount(fact.Key, fact.Bytes, state.Live + 1)
                        : state.Bytes + fact.Bytes > cls.Schedule.ByteBudget
                            ? new SweepVerdict.EvictSize(fact.Key, fact.Bytes, state.Bytes + fact.Bytes - cls.Schedule.ByteBudget)
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
    public static IO<SweepReceipt> Execute(RetentionClass cls, Seq<SweepVerdict> verdicts, Func<ContentAddress, IO<Unit>> evict, Func<ContentAddress, StorageTier, IO<Unit>> demote, CorrelationId correlation, ClockPolicy clocks) =>
        from freed in verdicts.Filter(static v => v.Evicts).TraverseM(v => evict(v.Key).Map(_ => v.Bytes)).As()
        from _ in verdicts.Choose(static v => v is SweepVerdict.Cool c ? Some(c) : None).TraverseM(c => demote(c.Key, c.To)).As()
        select new SweepReceipt(cls, verdicts.Count, verdicts.Count(static v => v is SweepVerdict.Kept), verdicts.Count(static v => v.Retains), verdicts.Count(static v => v.Cools), freed.Count, freed.Sum(), clocks.Now, correlation);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | sweep fold          | one state-threaded newest-first fold   | pure verdict list; resumes by re-fold, no journal         |
|  [02]   | declared order      | holds → (never-evict: cool) → age → count → size, keep newest-N | every evict stage reachable; `EvictCount` is not dead |
|  [03]   | reachability GC     | mark over EVERY AS-OF cut              | full-history, never head; historical refs survive (`H10`) |
|  [04]   | one executor        | `Execute` (`evict` + `demote`)         | every lane + operator purge + cold-tier demotion routes through; receipt ledger |
|  [05]   | cold-tiering        | `Cool` demotes a never-evict artifact  | `RetentionCeiling.Demote` ladder over `StorageTier`; `H10` GC-forbidden alternative made real |
|  [06]   | holds               | first-class, late-bound, union         | a hold today protects tomorrow's admissions               |
|  [07]   | conservation        | `inventory = kept + held + cooled + evicted` | the run summary proves the partition closes         |
