# [PERSISTENCE_VERSION_RETENTION]

Rasm.Persistence owns the lifecycle of every durable artifact through one classification-and-retention sweep whose reachability GC runs over the FULL event history, never head: a content-addressed snapshot, a Marten stream, a geometry blob, or a chunk belongs to exactly one `RetentionClass` row carrying its storage lane, retention record, classification ceiling, loss policy, and identity scheme; the sweep is a pure three-stage fold (holds exit first, each survivor takes the first deciding verdict in declared order, the size stage evicts oldest-first under budget); and the reachability GC marks every content key referenced by ANY AS-OF cut over the whole event stream so a blob a historical version still references is never collected — geometry GC over head alone is forbidden, and the alternative is dedup-plus-cold-tiering with no collection at all. This is the GC owner the `Store/blobstore#OBJECT_STORE` geometry object store feeds its content-lineage catalog into (`H10` — the object store gets the same content-lineage and retention catalog row the snapshot spine has, and GC reachability runs over every AS-OF cut), and the one deletion executor every lane routes through so the receipt stream is the system's complete deletion and non-deletion ledger. `DataClassification` arrives from AppHost; `ContentAddress`, `SnapshotCatalogRow` arrive from `Element/codec`; `OpLogEntry`, `Checkpoint`, `BranchRef` arrive from the Version owners; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId` arrive from AppHost.

## [01]-[INDEX]

- [01]-[RETENTION_CLASSES]: the closed class axis, the five-decision class row, classification-ceiling admission, and the budget/loss policies.
- [02]-[SWEEP_AND_GC]: the pure three-stage sweep fold, first-class holds, the full-history reachability GC, and the one receipted deletion executor.

## [02]-[RETENTION_CLASSES]

- Owner: `RetentionClass` the `[SmartEnum<string>]` artifact-class axis carrying storage lane, retention record, classification ceiling, loss policy, and identity scheme; `LossPolicy` the receipted-evict-versus-declared-expiry vocabulary; `IdentityScheme` the content-keyed-versus-name-plus-epoch vocabulary; `RetentionFault` the closed admission fault; `RetentionCatalog` the static surface owning the one-fold admission (classify-check, identity-derive, race-admit, budget-check, lane-write).
- Cases: the canonical class set closes at six rows — `snapshot` (sealed snapshot, content-keyed, newest-N), `stream` (Marten event stream, append-only, never evicted), `blob` (geometry object, content-keyed, full-history-reachable), `evidence` (incident bundle, name-plus-epoch, declared-expiry), `cache` (transient blob, content-keyed, receipted-evict), `ephemeral` (presence/awareness, declared-expiry, never durable); a class fitting no row is an admission rejection, never a default; class membership is immutable, reclassification is export-then-readmit.
- Entry: `public static Fin<RetentionFact> Admit(RetentionClass cls, ContentAddress key, DataClassification stamp, long bytes, Func<ContentAddress, bool> resident, ClockPolicy clocks)` is the one admission fold; `public RetentionClass.Schedule Schedule()` projects the class's sweep cadence and budget.
- Auto: admission is one fold — classify-check (the artifact stamp against the class ceiling), identity-derive (content-key or name-plus-epoch per the scheme), race-admit (content-keyed classes get dedup and race-loser disposal free, name-plus-epoch classes get versioned replacement free, zero conditional code), budget-check, lane-write; classification stamps arrive settled so admission compares the stamp against the class ceiling and rejects typed, an unstamped artifact rejecting identically because absence of evidence is not clearance; byte counts record from the artifact's own sealed length fields (`SnapshotCatalogRow.StoredLength`), never a later filesystem stat.
- Receipt: an admission rides `store.retention.admit` carrying the class and bytes; a ceiling breach rides `store.retention.reject` carrying the stamp and ceiling.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new artifact class is one `RetentionClass` row carrying its five decisions; a new loss policy is one `LossPolicy` row; zero new surface — a per-artifact retention table, a second classification taxonomy, or a default class for an unfit artifact is the deleted form because the class set is closed and admission is one fold over the five decisions.
- Boundary: every stored thing belongs to exactly one class row carrying five decisions, and the identity scheme alone yields two complete behavioral families (content-keyed classes get dedup and race-loser disposal free, name-plus-epoch classes get versioned replacement free); a budget breach truncates with an embedded receipt (capture must succeed degraded) while a ceiling breach rejects outright (security never degrades), the two overflow responses never interchangeable; classification stamps arrive settled and import re-verifies stamps so an export round-trip cannot launder a ceiling; the `stream` class is append-only and never evicted because the Marten event stream is the system of record (only the AS-OF snapshot density and the blob reachability are reclaimable); the `blob` class is content-keyed and full-history-reachable so a geometry blob a historical version references is never collected (`#SWEEP_AND_GC`), and the `Store/blobstore#OBJECT_STORE` lane registers its catalog row in this class so the one GC governs both the snapshot spine and the geometry object store.

```csharp signature
public sealed class RetentionKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
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

[Union]
public abstract partial record RetentionFault : Expected, IValidationError<RetentionFault> {
    private RetentionFault(string detail, int code) : base(detail, code, None) { }
    public static RetentionFault Create(string message) => new Unclassed(message);
    public sealed record Unclassed(string Artifact) : RetentionFault($"<retention-unclassed:{Artifact}>", 8281);
    public sealed record CeilingBreach(DataClassification Stamp, DataClassification Ceiling) : RetentionFault($"<retention-ceiling:{Stamp}>{Ceiling}>", 8282);
    public sealed record Unstamped(ContentAddress Key) : RetentionFault($"<retention-unstamped:{Key.Value:x32}>", 8283);
}

public readonly record struct RetentionSchedule(Duration Cadence, long ByteBudget, int CountBound, Duration AgeBound);
public readonly record struct RetentionFact(RetentionClass Class, ContentAddress Key, long Bytes, Instant At);

[SmartEnum<string>]
[KeyMemberEqualityComparer<RetentionKeyPolicy, string>]
[KeyMemberComparer<RetentionKeyPolicy, string>]
public sealed partial class RetentionClass {
    public static readonly RetentionClass Snapshot = new("snapshot", LossPolicy.ReceiptedEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, new RetentionSchedule(Duration.FromHours(6), 64L * 1024 * 1024 * 1024, 32, Duration.FromDays(365)));
    public static readonly RetentionClass Stream = new("stream", LossPolicy.NeverEvict, IdentityScheme.ContentKeyed, DataClassification.Confidential, new RetentionSchedule(Duration.MaxValue, long.MaxValue, int.MaxValue, Duration.MaxValue));
    public static readonly RetentionClass Blob = new("blob", LossPolicy.ReceiptedEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, new RetentionSchedule(Duration.FromHours(12), 512L * 1024 * 1024 * 1024, int.MaxValue, Duration.MaxValue));
    public static readonly RetentionClass Evidence = new("evidence", LossPolicy.DeclaredExpiry, IdentityScheme.NamePlusEpoch, DataClassification.Confidential, new RetentionSchedule(Duration.FromDays(1), 8L * 1024 * 1024 * 1024, 256, Duration.FromDays(90)));
    public static readonly RetentionClass Cache = new("cache", LossPolicy.ReceiptedEvict, IdentityScheme.ContentKeyed, DataClassification.Internal, new RetentionSchedule(Duration.FromHours(1), 16L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromDays(7)));
    public static readonly RetentionClass Ephemeral = new("ephemeral", LossPolicy.DeclaredExpiry, IdentityScheme.NamePlusEpoch, DataClassification.Internal, new RetentionSchedule(Duration.FromMinutes(1), 1L * 1024 * 1024 * 1024, int.MaxValue, Duration.FromMinutes(5)));

    public LossPolicy Loss { get; }
    public IdentityScheme Scheme { get; }
    public DataClassification Ceiling { get; }
    public RetentionSchedule Schedule { get; }
    private RetentionClass(string key, LossPolicy loss, IdentityScheme scheme, DataClassification ceiling, RetentionSchedule schedule) : this(key) => (Loss, Scheme, Ceiling, Schedule) = (loss, scheme, ceiling, schedule);
}

public static class RetentionCatalog {
    public static Fin<RetentionFact> Admit(RetentionClass cls, ContentAddress key, DataClassification stamp, long bytes, Func<ContentAddress, bool> resident, ClockPolicy clocks) =>
        stamp.Exceeds(cls.Ceiling)
            ? Fin.Fail<RetentionFact>(new RetentionFault.CeilingBreach(stamp, cls.Ceiling))
            : cls.Scheme.Dedups && resident(key)
                ? Fin.Succ(new RetentionFact(cls, key, 0L, clocks.Now))
                : Fin.Succ(new RetentionFact(cls, key, bytes, clocks.Now));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | class membership    | one of six closed `RetentionClass` rows | unfit artifact rejects, never defaults                    |
|  [02]   | identity scheme     | content-keyed vs name-plus-epoch       | dedup/race-disposal vs versioned-replacement, zero branch |
|  [03]   | ceiling vs budget   | ceiling rejects, budget truncates      | security never degrades; capture succeeds degraded        |
|  [04]   | stream class        | append-only, never evicted             | the event SoR; only snapshots and blobs reclaim           |

## [03]-[SWEEP_AND_GC]

- Owner: `SweepVerdict` the closed per-artifact verdict; `Hold` the first-class hold row (whole-class, identity-set, stamp-range selectors composing by union); `SweepReceipt` the run summary proving `inventory = kept + held + evicted`; `Reachability` the full-history mark surface; `RetentionSweep` the static surface owning the pure three-stage fold and the one receipted deletion executor.
- Cases: `SweepVerdict` is `Kept | Held | HeldOverBudget | EvictAge | EvictCount | EvictSize`; held bytes count against the budget but cannot evict so preservation pressure surfaces as `HeldOverBudget` rather than displacing onto unheld artifacts; the reachability mark is `Reachable | Orphan` over the full-history cut set.
- Entry: `public static (Seq<SweepVerdict> Verdicts, SweepReceipt Receipt) Sweep(RetentionClass cls, Seq<RetentionFact> inventory, Seq<Hold> holds, Reachability live, Instant now)` is the pure verdict fold; `public static LanguageExt.HashSet<ContentAddress> Mark(Func<TimeCut, Seq<ContentAddress>> referencedAt, Seq<TimeCut> everyCut)` is the full-history reachability mark; `public static IO<Seq<SweepReceipt>> Execute(RetentionClass cls, Seq<SweepVerdict> verdicts, Func<ContentAddress, IO<Unit>> evict, ClockPolicy clocks)` is the one receipted deletion executor, every eviction receipt carrying the sweep's real `RetentionClass` and the verdict's own byte figure.
- Auto: the sweep is a pure three-stage fold — holds exit first and short-circuit every rule, each survivor takes the first deciding verdict in declared order (age, count, size), and the size stage evicts oldest-first until under budget; verdicts are a pure function of the inventory snapshot, the policy snapshot, and the hold rows under one clock instant, so the verdict list is a testable value and a partial sweep resumes by re-folding with no journal; the reachability mark runs over EVERY AS-OF cut, not head — a content key referenced by any historical `TimeCut`'s reconstructed graph is `Reachable` and never collected, so a blob a prior version still references survives even after head drops it; blob bytes delete after the catalog row commit (the crash window produces collectible orphans, never dangling rows) and the age-gated orphan pass closes the loop.
- Receipt: every removed artifact emits `(class, identity, deciding rule, policy stamp, bytes)` and the run summary proves `inventory = kept + held + evicted`; unreceipted deletion anywhere is a rail rejection, and the receipt stream is itself a count-and-age-bounded class closing meta-retention at depth one.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new sweep rule is one stage in the declared verdict order; a new hold selector is one `Hold` case; zero new surface — a second sweeper, a head-only GC, an unreceipted cleanup, or an export-to-preserve workaround is the deleted form because the sweep is the single deletion executor and the GC marks over the full history.
- Boundary: the reachability GC runs over the FULL event history, not head (`H10`) — `Mark` folds the referenced content keys of every AS-OF cut's reconstructed graph so a geometry blob or snapshot a historical version references is `Reachable` and survives, and a head-only GC that collects a blob a prior version still cites is the deleted form; the alternative permitted by `H10` is geometry-GC-forbidden (dedup-plus-cold-tiering with no collection), expressed as a `blob`-class schedule whose `LossPolicy.NeverEvict` and a cold-tier transition replace eviction; holds are first-class rows bound late at sweep time so a hold placed today protects artifacts admitted tomorrow, release deletes the row with no eviction side effect, and every run emits an active-hold inventory because forgotten holds are the dominant retention failure; the deletion is the one executor every lane routes through (a snapshot sweep, a blob GC, a cache eviction) so the receipt stream is the complete deletion ledger, operator deletion routing through an administrative verdict kind; eligibility predicates inject (sync fences, projection floors, export pins, the orphan age gate) so the sweep owns zero domain-safety rules and every refusal names the predicate that held it.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepVerdict {
    private SweepVerdict() { }
    public sealed record Kept(ContentAddress Key) : SweepVerdict;
    public sealed record Held(ContentAddress Key, string By) : SweepVerdict;
    public sealed record HeldOverBudget(ContentAddress Key, long OverBy) : SweepVerdict;
    public sealed record EvictAge(ContentAddress Key, Duration Age) : SweepVerdict;
    public sealed record EvictCount(ContentAddress Key, int Rank) : SweepVerdict;
    public sealed record EvictSize(ContentAddress Key, long OverBy) : SweepVerdict;

    public ContentAddress Key => this switch { Kept k => k.Key, Held h => h.Key, HeldOverBudget o => o.Key, EvictAge a => a.Key, EvictCount c => c.Key, EvictSize s => s.Key };
    public bool Evicts => this is EvictAge or EvictCount or EvictSize;
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
    public bool Reachable(ContentAddress key) => Live.Contains(key);
}

public readonly record struct SweepReceipt(RetentionClass Class, int Inventory, int Kept, int Held, int Evicted, long EvictedBytes, Instant At, CorrelationId Correlation) {
    public bool Conserves => Inventory == Kept + Held + Evicted;
}

public static class RetentionSweep {
    public static LanguageExt.HashSet<ContentAddress> Mark(Func<TimeCut, Seq<ContentAddress>> referencedAt, Seq<TimeCut> everyCut) =>
        everyCut.Fold(LanguageExt.HashSet<ContentAddress>.Empty, (live, cut) => live.TryAddRange(referencedAt(cut)));

    public static (Seq<SweepVerdict> Verdicts, SweepReceipt Receipt) Sweep(RetentionClass cls, Seq<RetentionFact> inventory, Seq<Hold> holds, Reachability live, Instant now) {
        var ordered = toSeq(inventory.OrderBy(static f => f.At));
        var running = 0L;
        var verdicts = ordered.Rev().Map(fact =>
            holds.Exists(h => h.Holds(fact)) || live.Reachable(fact.Key)
                ? (running += fact.Bytes) > cls.Schedule.ByteBudget
                    ? (SweepVerdict)new SweepVerdict.HeldOverBudget(fact.Key, running - cls.Schedule.ByteBudget)
                    : new SweepVerdict.Held(fact.Key, "hold-or-reachable")
                : now - fact.At >= cls.Schedule.AgeBound && cls.Loss.Evicts
                    ? new SweepVerdict.EvictAge(fact.Key, now - fact.At)
                    : (running += fact.Bytes) > cls.Schedule.ByteBudget && cls.Loss.Evicts
                        ? new SweepVerdict.EvictSize(fact.Key, running - cls.Schedule.ByteBudget)
                        : new SweepVerdict.Kept(fact.Key)).Rev();
        var (kept, held, evicted, evictedBytes) = verdicts.Fold((0, 0, 0, 0L), (acc, v) => v switch {
            SweepVerdict.Kept => (acc.Item1 + 1, acc.Item2, acc.Item3, acc.Item4),
            SweepVerdict.Held or SweepVerdict.HeldOverBudget => (acc.Item1, acc.Item2 + 1, acc.Item3, acc.Item4),
            _ => (acc.Item1, acc.Item2, acc.Item3 + 1, acc.Item4),
        });
        return (verdicts, new SweepReceipt(cls, inventory.Count, kept, held, evicted, evictedBytes, now, CorrelationId.None));
    }

    public static IO<Seq<SweepReceipt>> Execute(RetentionClass cls, Seq<SweepVerdict> verdicts, Func<ContentAddress, IO<Unit>> evict, ClockPolicy clocks) =>
        verdicts.Filter(static v => v.Evicts).TraverseM(v => evict(v.Key).Map(_ => new SweepReceipt(cls, 1, 0, 0, 1, v is SweepVerdict.EvictSize s ? s.OverBy : 0L, clocks.Now, CorrelationId.None))).As();
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | sweep fold          | holds-first, then age/count/size       | pure verdict list; resumes by re-fold, no journal         |
|  [02]   | reachability GC     | mark over EVERY AS-OF cut              | full-history, never head; historical refs survive (`H10`) |
|  [03]   | one deletion        | `Execute` the single executor          | every lane routes through; receipt stream is the ledger   |
|  [04]   | holds               | first-class, late-bound, union         | a hold today protects tomorrow's admissions               |
|  [05]   | conservation        | `inventory = kept + held + evicted`    | a breach fails the sweep rail                             |
