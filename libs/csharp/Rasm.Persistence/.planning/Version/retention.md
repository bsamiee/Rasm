# [PERSISTENCE_REDACTION_RETENTION]

Rasm.Persistence enforces the AppHost data-classification taxonomy at every store write and export, owns the retention algebra whose pure verdict fold mints one closed `SweepVerdict` per artifact under first-class holds and injected non-deletion fences, projects the store's support-bundle contribution with hash-proved export receipts, and folds classification into the server-side pgaudit category every tenant policy binds. One registry owns the durable-artifact governance: `ArtifactClass` is the `[SmartEnum<string>]` row carrying every per-class column — classification, retention policy and its bounds, object-store residence, and the derived redactor and audit obligations — so a retained family is one row and the sweep, the export, the GC, and the audit binding all read that one owner rather than three parallel tables.

`DataClassification`, `RedactorKind`, `SupportContributorPort`, `SupportArtifact`, `ScheduleEntry`, `OccurrenceSpec`, `LeasePolicy`, `DeadlineClass`, `ClockPolicy`, `ReceiptSinkPort`, and `CorrelationId` arrive settled from the AppHost owners and compose as given. The concern lands as columns on `ArtifactClass`, bound cases on `RetentionPolicy`, verdict cases on `SweepVerdict`, hold cases on `HoldSelector`, fence cases on `EligibilityPredicate`, statement-class rows on `AuditCategory`, typed receipts, and one closed `RetentionFault` `[Union]` deriving from `Expected` — never a parallel taxonomy, a second redactor table, a second deletion executor, a second scheduler, or a bare untyped `Error` whose failure identity collapses every cause to one key.

## [01]-[INDEX]

- [01]-[CLASSIFICATION_ENFORCEMENT]: `ArtifactClass` registry, write-guard admission, unpersistable classes, and the derived encryption demand.
- [02]-[RETENTION_SWEEPS]: `RetentionPolicy` bound grammar, the pure `SweepVerdict` decision fold, `HoldSelector` and `EligibilityPredicate` fences, budget pressure, object-store reachability on the one sweep, dry-run gate, and schedule rows.
- [03]-[EXPORT_PROOF]: support contribution rows, classification-resolved redaction fold, and hash-proved export proof.
- [04]-[AUDIT_BINDING]: classification-to-pgaudit `AuditCategory` table, per-tenant binding, and the typed `Verify` fold over the observed `pg_settings`.

## [02]-[CLASSIFICATION_ENFORCEMENT]

- Owner: `ArtifactClass` `[SmartEnum<string>]` registry under the `RetentionKeyPolicy` ordinal accessor — the single durable-artifact governance row; `ClassificationGuard` the admission and projection surface; `RetentionFault` the closed `[Union]` failure family on the doctrine `Expected, IValidationError<RetentionFault>` shape in the 8250 band that every admission, conservation, and audit-verify rejection lifts into.
- Cases: 7 registry rows over the 5 persistable classification values; `Credential` and `Secret` are unpersistable and never become a row; `RetentionFault` carries 6 cases — `Unpersistable`, `ClassificationMismatch`, `Unclassified`, `Unconserved`, `AuditCategoryMismatch`, `PreloadMissing`.
- Entry: `public static Fin<DataClassification> Admit(Option<DataClassification> classification, ArtifactClass row)` — `Fin<T>` aborts through the typed `RetentionFault` cases; one polymorphic admission discriminates on the classification's persistability, the absent case, and the row mismatch, each rejection carrying its `DataClassification`/`ArtifactClass` evidence rather than a string-tagged `Error`.
- Auto: every write path admits through `Admit` before a row materializes, and survey delegates project the per-row classification column, so an absent or unpersistable classification never reaches a sweep, a GC, an export, or the wire.
- Derived: `Residence` selects the eviction delegate the one `RetentionSweep` runs per class — relational row eviction for `Relational`, the `BlobRemote.Delete` blob eviction under the injected `EligibilityPredicate.Reachability` fence for `ObjectStore` — with no second registry and no second deletion executor; `Redactor` is the `DataClassification.Redactor` column the export fold resolves; `Audit` is the per-class pgaudit obligation `AuditBinding` folds; the at-rest encryption demand is NOT a column here — `Store/encryption#ENCRYPTION_AXIS` owns the canonical `{Personal, Credential, Secret}` `DemandsEncryption` set and reads the registry's classification column, never a second copy.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: one retained family is one `ArtifactClass` row; one enforcement decision is one column on that row; zero new surface.

The registry is the one source the downstream concerns trace to. Per-column obligations:

| [COLUMN]          | [SOURCE]                                                  | [CONSUMER]                            |
| :---------------- | :------------------------------------------------------- | :------------------------------------ |
| `Classification`  | settled `DataClassification` from the AppHost taxonomy   | write guard, redactor, audit, encrypt |
| `Policy` + bounds | `RetentionPolicy` case + `Horizon`/`Count`/`Bytes`       | `RetentionSweep` eligibility fold     |
| `Residence`       | `Relational` vs `ObjectStore` loose blob                 | per-residence `evict` delegate on the one `Sweep` |
| `Redactor`        | `DataClassification.Redactor` column                     | `StoreEvidence` export redaction fold |
| `Audit`           | `AuditBinding.CategoryOf` derived `AuditCategory`        | `AuditBinding` provisioning verifier  |

- Boundary law: the taxonomy and redactor policy arrive settled from the AppHost classification owner; a local taxonomy, a second redactor table, or ad hoc string masking is the rejected form.
- Boundary law: secret and credential material routes to the secrets-store config source and folds to the unpersistable rejection at any store write, so neither ever becomes an `ArtifactClass` row.
- Boundary law: the at-rest encryption demand is owned by `Store/encryption#ENCRYPTION_AXIS` — its `{Personal, Credential, Secret}` set reads this registry's classification column and the gate stays research-held on its owning row; a second `DemandsEncryption` set or a single-class check here is the rejected form.
- Boundary law: every horizon, count, and byte bound on the registry is the axis row its consumers trace to, never a call-site literal.

```csharp signature
public sealed class RetentionKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum]
public sealed partial class ArtifactResidence {
    public static readonly ArtifactResidence Relational = new();
    public static readonly ArtifactResidence ObjectStore = new();
}

[Union]
public abstract partial record RetentionFault : Expected, IValidationError<RetentionFault> {
    private RetentionFault(string detail, int code) : base(detail, code, None) { }

    public static RetentionFault Create(string message) => new Unclassified(message);

    public sealed record Unpersistable : RetentionFault {
        public Unpersistable(DataClassification classification, string artifactClass) : base($"unpersistable {classification.Key} routed at {artifactClass}", 8250) => (Classification, ArtifactClass) = (classification, artifactClass);
        public DataClassification Classification { get; }
        public string ArtifactClass { get; }
    }
    public sealed record ClassificationMismatch : RetentionFault {
        public ClassificationMismatch(DataClassification observed, DataClassification expected, string artifactClass) : base($"classification {observed.Key} at {artifactClass} expected {expected.Key}", 8251) => (Observed, Expected, ArtifactClass) = (observed, expected, artifactClass);
        public DataClassification Observed { get; }
        public DataClassification Expected { get; }
        public string ArtifactClass { get; }
    }
    public sealed record Unclassified : RetentionFault {
        public Unclassified(string artifactClass) : base($"unclassified write at {artifactClass}", 8252) => ArtifactClass = artifactClass;
        public string ArtifactClass { get; }
    }
    public sealed record Unconserved : RetentionFault {
        public Unconserved(string artifactClass, long surveyed, long kept, long held, long evicted) : base($"unconserved {artifactClass}: surveyed {surveyed} != kept {kept} + held {held} + evicted {evicted}", 8253) => (ArtifactClass, Surveyed, Kept, Held, Evicted) = (artifactClass, surveyed, kept, held, evicted);
        public string ArtifactClass { get; }
        public long Surveyed { get; }
        public long Kept { get; }
        public long Held { get; }
        public long Evicted { get; }
    }
    public sealed record AuditCategoryMismatch : RetentionFault {
        public AuditCategoryMismatch(string setting, string expected, string observed) : base($"pgaudit {setting}={observed} expected {expected}", 8254) => (Setting, Expected, Observed) = (setting, expected, observed);
        public string Setting { get; }
        public string Expected { get; }
        public string Observed { get; }
    }
    public sealed record PreloadMissing : RetentionFault {
        public PreloadMissing(string observed) : base($"pgaudit absent from shared_preload_libraries: {observed}", 8255) => Observed = observed;
        public string Observed { get; }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<RetentionKeyPolicy, string>]
[KeyMemberComparer<RetentionKeyPolicy, string>]
public sealed partial class ArtifactClass {
    public static readonly ArtifactClass SnapshotCatalog   = new("snapshot-catalog", DataClassification.UserContent, RetentionPolicy.VersionBound, Duration.Zero, count: 16L, bytes: 0L, ArtifactResidence.Relational);
    public static readonly ArtifactClass KvRow             = new("kv-row", DataClassification.Operational, RetentionPolicy.AgeBound, Duration.Zero, count: 0L, bytes: 0L, ArtifactResidence.Relational);
    public static readonly ArtifactClass OpLog             = new("op-log", DataClassification.UserContent, RetentionPolicy.AgeBound, Duration.FromDays(30), count: 0L, bytes: 0L, ArtifactResidence.Relational);
    public static readonly ArtifactClass SyncTombstone     = new("sync-tombstone", DataClassification.UserContent, RetentionPolicy.AgeBound, Duration.FromDays(30), count: 0L, bytes: 0L, ArtifactResidence.Relational);
    public static readonly ArtifactClass BlobIndex         = new("blob-index", DataClassification.Operational, RetentionPolicy.SizeBound, Duration.Zero, count: 0L, bytes: 2_147_483_648L, ArtifactResidence.ObjectStore);
    public static readonly ArtifactClass BenchmarkRow      = new("benchmark-row", DataClassification.Operational, RetentionPolicy.CountBound, Duration.Zero, count: 1024L, bytes: 0L, ArtifactResidence.Relational);
    public static readonly ArtifactClass IdempotencyDedup  = new("idempotency-dedup", DataClassification.Operational, RetentionPolicy.AgeBound, Duration.FromHours(24), count: 0L, bytes: 0L, ArtifactResidence.Relational);

    public DataClassification Classification { get; }
    public RetentionPolicy Policy { get; }
    public Duration Horizon { get; }
    public long Count { get; }
    public long Bytes { get; }
    public ArtifactResidence Residence { get; }

    public RedactorKind Redactor => Classification.Redactor;
    public AuditCategory Audit => AuditBinding.CategoryOf(Classification);
}

public static class ClassificationGuard {
    public static readonly FrozenSet<DataClassification> Unpersistable =
        new[] { DataClassification.Credential, DataClassification.Secret }.ToFrozenSet();

    public static Fin<DataClassification> Admit(Option<DataClassification> classification, ArtifactClass row) =>
        classification switch {
            { IsSome: true, Case: DataClassification c } when Unpersistable.Contains(c) =>
                Fin.Fail<DataClassification>(new RetentionFault.Unpersistable(c, row.Key)),
            { IsSome: true, Case: DataClassification c } when c != row.Classification =>
                Fin.Fail<DataClassification>(new RetentionFault.ClassificationMismatch(c, row.Classification, row.Key)),
            { IsSome: true, Case: DataClassification c } => Fin.Succ(c),
            _ => Fin.Fail<DataClassification>(new RetentionFault.Unclassified(row.Key)),
        };
}
```

## [03]-[RETENTION_SWEEPS]

- Owner: `RetentionPolicy` `[SmartEnum<string>]` bound grammar; `SweepVerdict` `[SmartEnum<string>]` the per-artifact disposition the sweep mints; `HoldSelector` `[Union]` the late-bound hold algebra; `EligibilityPredicate` `[Union<T1,...>]` the injected non-deletion fences; `SweepMode` the dry-run gate; `RetentionSweep` the pure decision fold plus the effect-carrying executor; `ArtifactFacts` survey row; `ArtifactVerdict` per-artifact ledger row; `SweepReceipt`.
- Cases: 5 bound rows — `AgeBound`, `CountBound`, `VersionBound`, `SizeBound`, `LegalHold` (never evictable); 7 verdict cases — `Kept`, `Held`, `HeldOverBudget`, `EvictAge`, `EvictCount`, `EvictLineage`, `EvictSize` — so `VersionBound`'s lineage-depth prune and `CountBound`'s flat row-count prune mint distinct verdicts a receipt consumer reads apart.
- Entry: `public static Seq<ArtifactVerdict> Decide(ArtifactClass rule, Seq<ArtifactFacts> live, Seq<HoldSelector> holds, Seq<EligibilityPredicate> fences, Instant now)` is the pure verdict fold — a testable value over an inventory snapshot, the hold rows, the injected fences, and one clock instant; `public static IO<Seq<SweepReceipt>> Sweep(ReceiptSinkPort sink, ClockPolicy clocks, CorrelationId correlation, SweepMode mode, Seq<HoldSelector> holds, Seq<EligibilityPredicate> fences, Func<ArtifactClass, IO<Seq<ArtifactFacts>>> survey, Func<ArtifactClass, Seq<ArtifactVerdict>, IO<long>> evict)` carries the deletion effect over the decided eviction set — the residence-dispatched `evict` delegate projects either the relational `Key` or the object-store `ContentKey` from each verdict row. Both residence families — relational rows and object-store loose blobs — run this one fold; the `ObjectStore`-residence survey emits `ArtifactFacts` whose reachability rides one injected `EligibilityPredicate.Reachability` fence, so a closure GC is rows on the one sweep, never a parallel walk.
- Auto: `SweepEntry` registers the executor as one `ScheduleEntry` under `LeasePolicy.Maintenance` with the AppHost `retention-sweep-cadence` `@daily` config-sourced default; the store drain order invokes the same fold once before close; receipts ride `sink.Send` as `store.retention.sweep` rows stamped with the ambient `CorrelationId` and the HLC two-half frame.
- Receipt: one `SweepReceipt` per registry row carrying the full `ArtifactVerdict` ledger (each row its `(Key, SweepVerdict, deciding rule, policy stamp, bytes)`), the `Surveyed`/`Kept`/`Held`/`HeldOverBudget`/`Evicted`/`EvictedBytes` counts, the `SweepMode` it ran under, the `CorrelationId`, and the `Instant` stamp — the `HeldOverBudget` count surfaces preservation pressure as a receipt metric, not only a per-row verdict; `SweepReceipt.Conserves` proves `surveyed = kept + held + evicted`, a breach folding to the typed `RetentionFault.Unconserved` carrying the four counts rather than a string-tagged miscount. Under `DryRun` the verdict ledger is the staged decision and no eviction runs, so deletion is never silent and never unapproved.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm.AppHost (project).
- Growth: a new bound grammar is one `RetentionPolicy` case plus its `EvictX` verdict — `Decide` dispatches the bound through the generated total `rule.Policy.Switch(...)`, so a new row breaks every dispatch site at compile time rather than falling silently to `Kept`; a new non-deletion fence is one `EligibilityPredicate` case; a new hold shape is one `HoldSelector` case; a new cadence is one policy value on the schedule row; a new retained class is one `ArtifactClass` row; a new failure cause is one `RetentionFault` case in the 8250 band.

Decision and execution split: `Decide` is a pure function of the inventory snapshot, hold rows, injected fences, and one clock instant, so the verdict ledger is a testable value and a partial sweep resumes by re-folding with no journal. Holds and fences run FIRST and short-circuit every bound; the surviving artifact then takes the verdict of its class's single `RetentionPolicy` through the generated total `rule.Policy.Switch(...)`, and the `SizeBound` stage evicts oldest-first only until the class falls under its byte budget. Held bytes count against the budget but cannot evict, so preservation pressure surfaces as `HeldOverBudget` instead of displacing onto unheld artifacts; `LegalHold` is the bound whose every-row hold dominates by construction.

| [BOUND]        | [STAGE]                                                          | [EVICT_ORDER]        |
| :------------- | :-------------------------------------------------------------- | :------------------- |
| `AgeBound`     | `facts.Stamp + rule.Horizon < now` -> `EvictAge`                  | oldest-first         |
| `CountBound`   | `facts.Rank >= rule.Count` (per-class row count) -> `EvictCount`  | oldest-first         |
| `VersionBound` | `facts.LineageRank >= rule.Count` (newest-N per lineage) -> `EvictLineage` | oldest-edition-first |
| `SizeBound`    | drain oldest-first while `running > rule.Bytes` -> `EvictSize`    | oldest-first         |
| `LegalHold`    | every row `Held` — invisible to every later bound                | dominant             |

`SweepVerdict` is the closed per-artifact disposition, not a boolean:

| [VERDICT]        | [MEANING]                                                       | [DELETES] |
| :--------------- | :------------------------------------------------------------- | :-------: |
| `Kept`           | under every bound and budget                                   |    no     |
| `Held`           | a `HoldSelector` or an `EligibilityPredicate` fenced it         |    no     |
| `HeldOverBudget` | held bytes pushed the class over budget but cannot evict        |    no     |
| `EvictAge`       | aged past `Horizon`                                             |    yes    |
| `EvictCount`     | beyond the per-class `Count` (flat row count)                   |    yes    |
| `EvictLineage`   | beyond the newest-`Count` editions per content lineage          |    yes    |
| `EvictSize`      | oldest-first drain below the `Bytes` budget                     |    yes    |

- Boundary law: holds are first-class `HoldSelector` rows composing by union and bound LATE at sweep time, so a hold placed today protects artifacts admitted tomorrow — `WholeClass` fences an entire `ArtifactClass`, `IdentitySet` a frozen content-key set, `StampRange` a `[t0, t1]` interval; release deletes the row with no eviction side effect, and every hold row's `Covers` runs before any bound so a covered artifact is `Held` regardless of policy. A flat `bool Hold` field is the deleted form.
- Boundary law: every run emits its `ActiveHolds` inventory — the in-effect `HoldSelector` rows with `at - hold.Placed` age and the placement `Reason` — because forgotten holds are the dominant retention failure, so a hold ageing past its purpose surfaces as a receipt fact rather than a silent indefinite preservation.
- Boundary law: eligibility fences inject as `EligibilityPredicate` cases — `Reachability` (object-store closure membership), `SyncFence` (an op-log cursor floor), `ProjectionFloor` (a live projection watermark), `ExportPin`, `OrphanAgeGate` — and a fenced survivor is `Held`, so the sweep stays the single deletion executor owning zero domain-safety rules and every non-deletion names the fence that held it. A bound checking a sync or export rule directly is the deleted form.
- Boundary law: survey emits the recency-ranked `Rank` and lineage-ranked `LineageRank` per row, and `Decide` orders by `Stamp` ascending so the size stage drains its running total oldest-first; per-artifact `Bytes` is the artifact's own sealed length field (`Version/snapshots#SNAPSHOT_SPINE` `SnapshotHeader.StoredLength` / the `BlobIndex` residence descriptor `Length`), never a later filesystem stat.
- Boundary law: `VersionBound` is the snapshot-retention bound `snapshot-catalog` carries — keep the newest `Count` editions per content lineage (`Version/snapshots#SNAPSHOT_SPINE` content-addressed editions), distinct from `CountBound`'s flat per-class row count, so a multi-version artifact prunes by lineage depth and mints `EvictLineage`, never `EvictCount` by total catalog size; the two verdicts stay distinct so a lineage-depth prune and a flat-count prune never collapse onto one receipt count.
- Boundary law: the `idempotency-dedup` registry row is the suite dedup-window horizon the transaction dedup table consumes; the kv-row class projects `Stamp` as the row expiry instant and every other class projects the creation instant.
- Boundary law: `DryRun` is the `retention-approval` gate — a destructive migration step runs the sweep under `SweepMode.DryRun`, the staged-eviction `ArtifactVerdict` ledger rides the fact stream as the approval evidence, and the `Enforce` pass runs only after the approval row commits, so an unreviewed destructive eviction is unrepresentable.
- Boundary law: horizons and stamps read `ClockPolicy` only; `DateTime.UtcNow` and direct timers are the deleted patterns.

The object-store residence family is the one sweep with the reachability fence injected — a second mark-and-sweep, a re-walk of the graph, or a polling scan that re-derives the closure is the deleted form:

- The reachable content-key set is the union over every live sync `Closure` manifest (`Sync/collaboration#TRANSPORT_AXIS` `OpLogEntry.Closure`, consumed here as settled) folded into one `EligibilityPredicate.Reachability` membership fence, never a re-walk; an `ObjectStore`-residence artifact unreferenced by the closure is evictable, fenced reachable is `Held`.
- The residence survey set-differences against the object-store listing (`Store/remote#OBJECT_RESIDENCE` `BlobRemote.List` content-key descriptors) and the executor evicts each unreferenced blob through the same `BlobRemote.Delete` the residence axis owns, so blob eviction reuses the relational eviction path with the residence-specific delete delegate.
- A `LegalHold` class or any `HoldSelector.IdentitySet` covering the blob's content key holds it, so a held blob is never collected regardless of reachability.
- The reachability fence stays SPIKE on the live object-store-plus-closure probe in the `[CLOSURE_GC]` research cluster.

```csharp signature
[SmartEnum]
public sealed partial class SweepMode {
    public static readonly SweepMode DryRun = new();
    public static readonly SweepMode Enforce = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<RetentionKeyPolicy, string>]
[KeyMemberComparer<RetentionKeyPolicy, string>]
public sealed partial class SweepVerdict {
    public static readonly SweepVerdict Kept           = new("kept", evicts: false, overBudget: false);
    public static readonly SweepVerdict Held           = new("held", evicts: false, overBudget: false);
    public static readonly SweepVerdict HeldOverBudget = new("held-over-budget", evicts: false, overBudget: true);
    public static readonly SweepVerdict EvictAge       = new("evict-age", evicts: true, overBudget: false);
    public static readonly SweepVerdict EvictCount     = new("evict-count", evicts: true, overBudget: false);
    public static readonly SweepVerdict EvictLineage   = new("evict-lineage", evicts: true, overBudget: false);
    public static readonly SweepVerdict EvictSize      = new("evict-size", evicts: true, overBudget: false);

    public bool Evicts { get; }
    public bool OverBudget { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<RetentionKeyPolicy, string>]
[KeyMemberComparer<RetentionKeyPolicy, string>]
public sealed partial class RetentionPolicy {
    public static readonly RetentionPolicy AgeBound     = new("age-bound");
    public static readonly RetentionPolicy CountBound   = new("count-bound");
    public static readonly RetentionPolicy VersionBound = new("version-bound");
    public static readonly RetentionPolicy SizeBound    = new("size-bound");
    public static readonly RetentionPolicy LegalHold    = new("legal-hold");
}

[Union]
public abstract partial record HoldSelector {
    private HoldSelector() { }
    public sealed record WholeClass(string ArtifactClass, Instant Placed, string Reason) : HoldSelector;
    public sealed record IdentitySet(FrozenSet<UInt128> Keys, Instant Placed, string Reason) : HoldSelector;
    public sealed record StampRange(Instant From, Instant To, Instant Placed, string Reason) : HoldSelector;

    public Instant Placed => Switch(wholeClass: static c => c.Placed, identitySet: static s => s.Placed, stampRange: static r => r.Placed);

    public bool Covers(ArtifactClass rule, ArtifactFacts facts) => Switch(
        wholeClass:  c => c.ArtifactClass == rule.Key,
        identitySet: s => facts.ContentKey is { IsSome: true, Case: UInt128 key } && s.Keys.Contains(key),
        stampRange:  r => facts.Stamp >= r.From && facts.Stamp <= r.To);
}

[Union<Reachability, SyncFence, ProjectionFloor, ExportPin, OrphanAgeGate>(
    T1Name = "Reachability", T2Name = "SyncFence", T3Name = "ProjectionFloor", T4Name = "ExportPin", T5Name = "OrphanAgeGate")]
public readonly partial struct EligibilityPredicate;

public sealed record Reachability(FrozenSet<UInt128> ReachableKeys);
public sealed record SyncFence(long CursorFloor);
public sealed record ProjectionFloor(long Watermark);
public sealed record ExportPin(FrozenSet<UInt128> PinnedKeys);
public sealed record OrphanAgeGate(Duration MinAge);

public readonly record struct ArtifactFacts(
    string Key,
    DataClassification Classification,
    Instant Stamp,
    long Rank,
    long LineageRank,
    long Bytes,
    long Cursor,
    Option<UInt128> ContentKey);

public readonly record struct ArtifactVerdict(
    string Key,
    SweepVerdict Verdict,
    RetentionPolicy Rule,
    Instant PolicyStamp,
    long Bytes,
    Option<UInt128> ContentKey);

public readonly record struct SweepReceipt(
    string ArtifactClass,
    RetentionPolicy Policy,
    SweepMode Mode,
    long Surveyed,
    long Kept,
    long Held,
    long HeldOverBudget,
    long Evicted,
    long EvictedBytes,
    Seq<ArtifactVerdict> Ledger,
    Seq<(HoldSelector Hold, Duration Age)> ActiveHolds,
    CorrelationId Correlation,
    Instant At) {
    public bool Conserves => Surveyed == Kept + Held + Evicted;
}

public static class RetentionSweep {
    public static Seq<ArtifactVerdict> Decide(
        ArtifactClass rule, Seq<ArtifactFacts> live, Seq<HoldSelector> holds, Seq<EligibilityPredicate> fences, Instant now) {
        var budget = rule.Bytes;
        var (running, ledger) = live.OrderBy(static row => row.Stamp).ToSeq()
            .Fold((Running: 0L, Out: Seq<ArtifactVerdict>()), (state, facts) => {
                var verdict =
                    holds.Exists(hold => hold.Covers(rule, facts)) || fences.Exists(fence => fence.Fenced(facts, now))
                        ? SweepVerdict.Held
                        : rule.Policy.Switch(
                            legalHold:   () => SweepVerdict.Held,
                            ageBound:    () => facts.Stamp + rule.Horizon < now ? SweepVerdict.EvictAge : SweepVerdict.Kept,
                            countBound:  () => facts.Rank >= rule.Count ? SweepVerdict.EvictCount : SweepVerdict.Kept,
                            versionBound: () => facts.LineageRank >= rule.Count ? SweepVerdict.EvictLineage : SweepVerdict.Kept,
                            sizeBound:   () => budget > 0L && state.Running + facts.Bytes > budget ? SweepVerdict.EvictSize : SweepVerdict.Kept);
                var charged = verdict.Evicts ? state.Running : state.Running + facts.Bytes;
                return (charged, state.Out.Add(new ArtifactVerdict(facts.Key, verdict, rule.Policy, now, facts.Bytes, facts.ContentKey)));
            });
        return budget > 0L && running > budget
            ? ledger.Map(static v => v.Verdict == SweepVerdict.Held ? v with { Verdict = SweepVerdict.HeldOverBudget } : v)
            : ledger;
    }

    public static IO<Seq<SweepReceipt>> Sweep(
        ReceiptSinkPort sink,
        ClockPolicy clocks,
        CorrelationId correlation,
        SweepMode mode,
        Seq<HoldSelector> holds,
        Seq<EligibilityPredicate> fences,
        Func<ArtifactClass, IO<Seq<ArtifactFacts>>> survey,
        Func<ArtifactClass, Seq<ArtifactVerdict>, IO<long>> evict) =>
        ArtifactClass.Items.ToSeq()
            .TraverseM(rule =>
                from at in IO.lift(() => clocks.Now)
                from facts in survey(rule)
                let ledger = Decide(rule, facts, holds, fences, at)
                let evictable = ledger.Filter(static v => v.Verdict.Evicts)
                from deleted in mode == SweepMode.Enforce ? evict(rule, evictable) : IO.pure((long)evictable.Count)
                let active = holds.Filter(hold => facts.Exists(f => hold.Covers(rule, f))).Map(hold => (hold, Age: at - hold.Placed))
                let receipt = new SweepReceipt(rule.Key, rule.Policy, mode,
                    Surveyed: ledger.Count,
                    Kept: ledger.Count(static v => v.Verdict == SweepVerdict.Kept),
                    Held: ledger.Count(static v => !v.Verdict.Evicts && v.Verdict != SweepVerdict.Kept),
                    HeldOverBudget: ledger.Count(static v => v.Verdict.OverBudget),
                    Evicted: deleted,
                    EvictedBytes: evictable.Sum(static v => v.Bytes),
                    Ledger: ledger, ActiveHolds: active, Correlation: correlation, At: at)
                from _ in sink.Send(correlation, TenantContext.Current, "Rasm.Persistence", "store.retention.sweep", JsonSerializer.SerializeToElement(receipt))
                from gated in receipt.Conserves
                    ? IO.pure(receipt)
                    : IO.fail<SweepReceipt>(new RetentionFault.Unconserved(rule.Key, receipt.Surveyed, receipt.Kept, receipt.Held, receipt.Evicted))
                select gated)
            .As();

    public static ScheduleEntry SweepEntry(OccurrenceSpec cadence, Func<IO<Seq<SweepReceipt>>> sweep) =>
        new("persistence-retention-sweep", cadence, DeadlineClass.SupportWindow, Optional(LeasePolicy.Maintenance), () => sweep().Map(static _ => unit));

    extension(EligibilityPredicate fence) {
        public bool Fenced(ArtifactFacts facts, Instant now) => fence.Switch(
            reachability:    r => facts.ContentKey is { IsSome: true, Case: UInt128 key } && r.ReachableKeys.Contains(key),
            syncFence:       f => facts.Cursor > f.CursorFloor,
            projectionFloor: p => facts.Cursor > p.Watermark,
            exportPin:       e => facts.ContentKey is { IsSome: true, Case: UInt128 key } && e.PinnedKeys.Contains(key),
            orphanAgeGate:   g => facts.Stamp + g.MinAge > now);
    }
}
```

| [INDEX] | [POLICY]        | [VALUE]                               | [BINDING]                                                     |
| :-----: | :-------------- | :------------------------------------ | :----------------------------------------------------------- |
|  [01]   | sweep cadence   | config-sourced cron, `@daily` default | `SweepEntry` schedule row under `LeasePolicy.Maintenance`     |
|  [02]   | drain sweep     | one `Enforce` fold before close       | band-300 store drain order                                   |
|  [03]   | object-store gc | reachability fence on the one sweep   | `EligibilityPredicate.Reachability` over the residence survey |

## [04]-[EXPORT_PROOF]

- Owner: `StoreEvidence` contribution rows and the classification-resolved redaction fold; `ExportProof` receipt.
- Cases: 7 artifact rows — store-metadata, open-receipts, schema-history, pragma-snapshot, fault-receipts, sweep-receipts, export-proofs.
- Entry: `public static Func<Interval, IO<(ReadOnlyMemory<byte> Bytes, int Redactions)>> Producer(IRedactorProvider redaction, DataClassification classification, Func<Interval, ReadOnlyMemory<char>> render)` — the one redacting producer every contribution row composes; `public static ExportProof Prove(ClockPolicy clocks, string artifact, DataClassification classification, ReadOnlyMemory<byte> redacted, int redactions, int failures)` mints the proof over the already-redacted bytes.
- Auto: `Contribution` lands the seven rows as `SupportArtifact` entries on the package's one `SupportContributorPort`, so the AppHost capture owner drives trigger, window, correlation, and bundle caps while assembly, redaction, and proof stay store-side.
- Receipt: one `ExportProof` per artifact — classification, redactor kind, redaction and failure counts, `XxHash128` content hash, bytes, and `Instant` stamp; proofs ship inside the bundle through the export-proofs row.
- Packages: System.IO.Hashing, Microsoft.Extensions.Compliance.Redaction, NodaTime, LanguageExt.Core, Rasm.AppHost (project).
- Growth: one artifact row lands a new contribution; zero new surface.

Redaction resolves through the AppHost-registered provider, never a local redactor map. The fold is the verified `IRedactorProvider`/`Redactor` span path:

- `redaction.GetRedactor(new DataClassificationSet(new(nameof(DataClassification), classification.Key)))` resolves the concrete redactor by the row's class — the `(category, name)` key is byte-identical to the `Observability/telemetry#REDACTION_TAXONOMY` `RedactionRegistration` binding, so the provider returns the redactor that registration sealed for that class; the provider, not Persistence, owns the class-to-redactor map and the `RedactorKind` column is only that registration's input.
- `GetRedactedLength(ReadOnlySpan<char>)` sizes the destination, `Redact(ReadOnlySpan<char>, Span<char>)` fills it, and the redaction count rises when the redacted length diverges from the source.
- The registration maps each class to its terminal: `Operational`/`None` to `NullRedactor` pass-through, `HostIdentity`/`Personal` to the keyed `HmacRedactor` so a pseudonymized identifier stays correlatable across artifacts without exposing the value, `UserContent`/`Credential`/`Secret` to `ErasingRedactor`, and any unmapped class to the `SetFallbackRedactor<ErasingRedactor>` default so an unrecognized class fails closed by erasure rather than leaking.

- Boundary law: a producer emits only redacted bytes with their redaction count over the frozen capture `Interval`; ad hoc string masking and a local redactor table are the deleted forms.
- Boundary law: the three concrete redactors arrive settled from the AppHost registration and are never re-registered here — Persistence reads the provider through `GetRedactor` and never opens a second redaction builder.
- Boundary law: a failed redaction folds its source row into a redaction-failure entry written into the same artifact stream, never silent evidence loss.
- Boundary law: estimated bytes are row values on the `Rows` table and the AppHost artifact cap truncates above them.
- Boundary law: `Redact` is the one measured span kernel — `GetRedactedLength` sizes the destination, a `StackCeiling`-gated `stackalloc`/`ArrayPool<char>` rent backs it so a large artifact never overflows the stack, and `Encoding.UTF8.GetBytes(ReadOnlySpan<char>, Span<byte>)` writes the redacted span straight to bytes with no intermediate `string` — the `ToString()`-then-encode round-trip the catalog rejects (it would itself transit the unredacted value) is the deleted form; the bracketed `stackalloc`/`Rent` statement form is this kernel's named exemption.

```csharp signature
public readonly record struct ExportProof(
    string Artifact,
    DataClassification Classification,
    RedactorKind Redactor,
    int Redactions,
    int RedactionFailures,
    UInt128 ContentHash,
    long Bytes,
    Instant At);

public static class StoreEvidence {
    public static readonly Seq<(string Artifact, DataClassification Classification, long EstimatedBytes)> Rows = Seq(
        ("store-metadata", DataClassification.HostIdentity, 65_536L),
        ("open-receipts", DataClassification.Operational, 262_144L),
        ("schema-history", DataClassification.Operational, 131_072L),
        ("pragma-snapshot", DataClassification.Operational, 16_384L),
        ("fault-receipts", DataClassification.Operational, 262_144L),
        ("sweep-receipts", DataClassification.Operational, 131_072L),
        ("export-proofs", DataClassification.Operational, 16_384L));

    const int StackCeiling = 1024;

    public static Func<Interval, IO<(ReadOnlyMemory<byte> Bytes, int Redactions)>> Producer(
        IRedactorProvider redaction, DataClassification classification, Func<Interval, ReadOnlyMemory<char>> render) {
        var redactor = redaction.GetRedactor(new DataClassificationSet(new(nameof(DataClassification), classification.Key)));
        return window => IO.lift(() => Redact(redactor, render(window).Span));
    }

    static (ReadOnlyMemory<byte> Bytes, int Redactions) Redact(Redactor redactor, ReadOnlySpan<char> source) {
        var sized = redactor.GetRedactedLength(source);
        var rented = sized > StackCeiling ? ArrayPool<char>.Shared.Rent(sized) : null;
        try {
            Span<char> destination = rented is null ? stackalloc char[StackCeiling] : rented;
            var written = redactor.Redact(source, destination[..sized]);
            var redacted = destination[..written];
            var bytes = new byte[Encoding.UTF8.GetByteCount(redacted)];
            _ = Encoding.UTF8.GetBytes(redacted, bytes);
            return (bytes, written == source.Length ? 0 : 1);
        }
        finally { if (rented is not null) { ArrayPool<char>.Shared.Return(rented); } }
    }

    public static SupportContributorPort Contribution(IRedactorProvider redaction, Func<string, Interval, ReadOnlyMemory<char>> render) =>
        new("Rasm.Persistence", Rows.Map(row => new SupportArtifact(
            row.Artifact, row.Classification, row.EstimatedBytes,
            Producer(redaction, row.Classification, window => render(row.Artifact, window)))));

    public static ExportProof Prove(ClockPolicy clocks, string artifact, DataClassification classification, ReadOnlyMemory<byte> redacted, int redactions, int failures) =>
        new(artifact, classification, classification.Redactor, redactions, failures, XxHash128.HashToUInt128(redacted.Span), redacted.Length, clocks.Now);
}
```

## [05]-[AUDIT_BINDING]

- Owner: `AuditCategory` `[ComplexValueObject]` the typed pgaudit session-audit obligation carrying the full GUC product — the `StatementClass` set, the relation/parameter/catalog flags, the parameter-capture size cap, and its `Guc` `(setting, value)` projection; `AuditBinding` the classification-to-category table with the per-tenant binding projection and the typed `Verify` fold.
- Cases: 5 binding rows over the persistable classification values in escalating sensitivity rank.
- Entry: `public static AuditCategory Bind(Seq<DataClassification> present)` is the pure value folding the union of every present class's statement-class set into one category; `public static Validation<RetentionFault, AuditCategory> Verify(Seq<DataClassification> present, FrozenDictionary<string,string> observed, bool preloadPresent)` is the read-only verifier that binds the expected category, compares each `Guc` `(setting, value)` against the observed `pg_settings`, and ACCUMULATES every divergence into `RetentionFault.AuditCategoryMismatch`/`PreloadMissing` so a deploy missing two GUCs surfaces both at once.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox, Rasm.AppHost (project).
- Growth: one classification binding is one `Categories` row; a richer pgaudit obligation is one column on `AuditCategory` plus one `Guc` row; a tenant-scoped binding is one `BindTenant` projection over the same table; a new verify cause is one `RetentionFault` case; never a second category vocabulary; zero new surface.

`AuditCategory` is the full pgaudit session-audit GUC surface, not a bare `pgaudit.log` string. The binding is the set-UNION of statement classes across every present class — additive, never a max-rank single-class selection — so a store carrying both `Operational` and `Personal` audits the union of both class sets:

| [GUC]                            | [SOURCE]                                                              |
| :------------------------------- | :------------------------------------------------------------------- |
| `pgaudit.log`                    | union of `StatementClass` sets across present classifications        |
| `pgaudit.log_relation`           | `on` when any present class carries relation-level audit             |
| `pgaudit.log_parameter`          | `on` when any present class carries parameter capture                |
| `pgaudit.log_parameter_max_size` | the parameter-capture byte cap (`Personal` 2048, else 0)            |
| `pgaudit.log_catalog`            | `off` by default — catalog-relation audit is noise at this tier      |
| `pgaudit.log_level`              | `log` for the audit log line severity                                |

- Boundary law: audit execution is server-log-side; the runtime obligation is the bound `AuditCategory.Guc` set verified read-only against `pg_settings` through the `Store/provisioning#CLUSTER_CONFIG` `SettingsProbe`, and a client-side audit log pipeline is the rejected form.
- Boundary law: `Verify` folds the registry classification column plus the observed per-row classifications through `Bind` and compares every `Guc` `(setting, value)` — `pgaudit.log`, `pgaudit.log_relation`, `pgaudit.log_parameter`, `pgaudit.log_parameter_max_size`, `pgaudit.log_catalog`, `pgaudit.log_level` — against the observed settings on the accumulating `Validation` carrier; each divergence is a typed `RetentionFault.AuditCategoryMismatch` carrying the `(setting, expected, observed)` triple and a missing `shared_preload_libraries=pgaudit` entry (`Store/provisioning#CLUSTER_CONFIG` `Preload` row) is `RetentionFault.PreloadMissing`, so a multi-GUC drift surfaces every breach at once rather than the first.
- Boundary law: the RLS-policy mechanics — the per-tenant `CREATE POLICY` and the `TenancyModel` single/schema/rls/db-per-tenant axis — are owned on `Store/tenancy#TENANCY_RLS`; this binding owns only the consequence — a tenant-id-bearing classification binds to one `AuditCategory` through `BindTenant`, so a per-tenant policy ties to the audit category and a per-tenant RLS-policy-applied fact plus the `Bind` result fact fold into the provisioning verifier where a tenant-isolation mismatch is a typed provisioning fault rather than silent.
- Boundary law: `Bind` admits only persistable rows because the write guard rejects `Credential` and `Secret` upstream, so the table needs no unpersistable case.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<RetentionKeyPolicy, string>]
[KeyMemberComparer<RetentionKeyPolicy, string>]
public sealed partial class StatementClass {
    public static readonly StatementClass Read     = new("read");
    public static readonly StatementClass Write    = new("write");
    public static readonly StatementClass Function = new("function");
    public static readonly StatementClass Role     = new("role");
    public static readonly StatementClass Ddl      = new("ddl");
    public static readonly StatementClass Misc     = new("misc");
}

[ComplexValueObject]
public sealed partial class AuditCategory {
    public Set<StatementClass> Classes { get; }
    public bool LogRelation { get; }
    public bool LogParameter { get; }
    public bool LogCatalog { get; }
    public int LogParameterMaxSize { get; }

    public string Log => string.Join(", ", Classes.OrderBy(static c => c.Key, StringComparer.Ordinal).Map(static c => c.Key));

    public Seq<(string Setting, string Value)> Guc => Seq(
        ("pgaudit.log", Log),
        ("pgaudit.log_relation", LogRelation ? "on" : "off"),
        ("pgaudit.log_parameter", LogParameter ? "on" : "off"),
        ("pgaudit.log_catalog", LogCatalog ? "on" : "off"),
        ("pgaudit.log_parameter_max_size", LogParameterMaxSize.ToString(CultureInfo.InvariantCulture)),
        ("pgaudit.log_level", "log"));

    public AuditCategory Union(AuditCategory other) =>
        Create(Classes + other.Classes, LogRelation || other.LogRelation, LogParameter || other.LogParameter,
            LogCatalog || other.LogCatalog, Math.Max(LogParameterMaxSize, other.LogParameterMaxSize));
}

public static class AuditBinding {
    public static readonly FrozenDictionary<DataClassification, AuditCategory> Categories = new[] {
        (DataClassification.None,         AuditCategory.Create(Set<StatementClass>(), false, false, false, 0)),
        (DataClassification.Operational,  AuditCategory.Create(Set(StatementClass.Ddl, StatementClass.Role), false, false, false, 0)),
        (DataClassification.HostIdentity, AuditCategory.Create(Set(StatementClass.Write, StatementClass.Ddl, StatementClass.Role), true, false, false, 0)),
        (DataClassification.UserContent,  AuditCategory.Create(Set(StatementClass.Write, StatementClass.Ddl, StatementClass.Role), true, false, false, 0)),
        (DataClassification.Personal,     AuditCategory.Create(Set(StatementClass.Read, StatementClass.Write, StatementClass.Ddl, StatementClass.Role), true, true, false, 2048)),
    }.ToFrozenDictionary(static row => row.Item1, static row => row.Item2);

    public static AuditCategory CategoryOf(DataClassification classification) =>
        Categories.TryGetValue(classification, out var category) ? category : Categories[DataClassification.None];

    public static AuditCategory Bind(Seq<DataClassification> present) =>
        present.Fold(Categories[DataClassification.None], static (acc, row) => acc.Union(CategoryOf(row)));

    public static (string Tenant, AuditCategory Category) BindTenant(string tenant, Seq<DataClassification> present) =>
        (tenant, Bind(present));

    public static Validation<RetentionFault, AuditCategory> Verify(
        Seq<DataClassification> present, FrozenDictionary<string, string> observed, bool preloadPresent) {
        var bound = Bind(present);
        var preload = preloadPresent
            ? Success<RetentionFault, Unit>(unit)
            : Fail<RetentionFault, Unit>(new RetentionFault.PreloadMissing(
                observed.TryGetValue("shared_preload_libraries", out var libs) ? libs : "<absent>"));
        return bound.Guc
            .Map(row => observed.TryGetValue(row.Setting, out var live) && string.Equals(live, row.Value, StringComparison.OrdinalIgnoreCase)
                ? Success<RetentionFault, Unit>(unit)
                : Fail<RetentionFault, Unit>(new RetentionFault.AuditCategoryMismatch(
                    row.Setting, row.Value, observed.TryGetValue(row.Setting, out var seen) ? seen : "<absent>")))
            .Add(preload)
            .Traverse(static outcome => outcome)
            .Map(_ => bound);
    }
}
```

## [06]-[RESEARCH]

- [PGAUDIT_CATEGORIES]: pgaudit session-audit category semantics on PG18 under `shared_preload_libraries=pgaudit` against the `AuditCategory.Guc` rows — the `pgaudit.log` statement-class union, `pgaudit.log_relation`, `pgaudit.log_parameter`, `pgaudit.log_parameter_max_size`, `pgaudit.log_catalog`, and `pgaudit.log_level` GUCs verified through `AuditBinding.Verify` against the observed `pg_settings`, and the per-tenant `BindTenant` category emission verified against a per-tenant `CREATE POLICY` (policy mechanics owned on `Store/tenancy#TENANCY_RLS`) on a live PG18 server — the `log_parameter_max_size` byte-cap value being the one open literal before the `Personal` row finalizes.
- [CLOSURE_GC]: the `EligibilityPredicate.Reachability` fence over the `ObjectStore`-residence survey verified against a live object-store listing and a live sync `Closure` membership union — the unreferenced-blob set folding to `EvictSize`, the `HoldSelector.IdentitySet` and `LegalHold` exemptions folding to `Held` under a live `ArtifactClass` registry, the `DryRun` staged-eviction `ArtifactVerdict` ledger versus the `Enforce` eviction round-trip, the `SweepReceipt.Conserves` identity across the round-trip, and the eviction-delete against the `BlobRemote.Delete` the residence axis owns.
