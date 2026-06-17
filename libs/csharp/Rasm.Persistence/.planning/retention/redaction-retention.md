# [PERSISTENCE_REDACTION_RETENTION]

Rasm.Persistence enforces the AppHost data-classification taxonomy at every store write and export, owns the `RetentionPolicy` axis with its receipted sweep fold, assembles the store's support-bundle contribution with hash-proved export receipts, and binds classification rows to server-side audit categories. Owned axes: the `ArtifactClassRow` registry, the four-row `RetentionPolicy` vocabulary, the `StoreEvidence` contribution rows, and the `AuditBinding` category table. `DataClassification`, `RedactorKind`, `SupportContributorPort`, `ScheduleEntry`, `LeasePolicy`, `DeadlineClass`, and `ClockPolicy` arrive settled and compose as given — the concern lands as enforcement columns, sweep rows, and receipts, never a parallel taxonomy, a redactor table, or a second scheduler.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]                  | [OWNS]                                                                 |
| :-----: | :------------------------- | :--------------------------------------------------------------------- |
|   [1]   | CLASSIFICATION_ENFORCEMENT | Artifact-class registry, write-guard admission, unpersistable classes  |
|   [2]   | RETENTION_SWEEPS           | Policy axis; hold-first sweep fold; object-store GC; schedule rows     |
|   [3]   | EXPORT_PROOF               | Support contribution rows; redacted assembly; hash-proved export proof |
|   [4]   | AUDIT_BINDING              | Classification-to-pgaudit category table; per-tenant binding; verification |

## [2]-[CLASSIFICATION_ENFORCEMENT]

- Owner: `ArtifactClassRow` registry inside `ArtifactClasses`; `ClassificationGuard` admission surface.
- Cases: 7 registry rows over 5 persistable classification values; Credential and Secret are unpersistable.
- Entry: `public static Fin<DataClassification> Admit(Option<DataClassification> classification, string artifactClass)` — `Fin<T>` aborts.
- Auto: every write path admits through the guard before a row materializes, and survey delegates project the per-row classification column, so an absent classification never reaches a sweep or an export.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: one retained artifact family is one registry row; one enforcement decision is one policy value; zero new surface.
- Boundary: the taxonomy and redactor policy arrive settled from the AppHost classification owner — a local taxonomy, a redactor table, or ad hoc string masking is the rejected form; secret and credential material routes to the secrets-store config source and folds to the `unpersistable` rejection at any store write; `DemandsEncryption` is the projection the native at-rest encryption gate consumes, and that gate stays research-held on its owning row; every horizon, count, and byte bound on the registry is the axis row its consumers trace to.

```csharp signature
public sealed record ArtifactClassRow(
    string Key,
    DataClassification Classification,
    RetentionPolicy Policy,
    Duration Horizon,
    long Count,
    long Bytes);

public static class ArtifactClasses {
    public static readonly FrozenDictionary<string, ArtifactClassRow> Registry = new[] {
        new ArtifactClassRow("snapshot-catalog", DataClassification.UserContent, RetentionPolicy.CountBound, Duration.Zero, 16L, 0L),
        new ArtifactClassRow("kv-row", DataClassification.Operational, RetentionPolicy.AgeBound, Duration.Zero, 0L, 0L),
        new ArtifactClassRow("op-log", DataClassification.UserContent, RetentionPolicy.AgeBound, Duration.FromDays(30), 0L, 0L),
        new ArtifactClassRow("sync-tombstone", DataClassification.UserContent, RetentionPolicy.AgeBound, Duration.FromDays(30), 0L, 0L),
        new ArtifactClassRow("blob-index", DataClassification.Operational, RetentionPolicy.SizeBound, Duration.Zero, 0L, 2_147_483_648L),
        new ArtifactClassRow("benchmark-row", DataClassification.Operational, RetentionPolicy.CountBound, Duration.Zero, 1024L, 0L),
        new ArtifactClassRow("idempotency-dedup", DataClassification.Operational, RetentionPolicy.AgeBound, Duration.FromHours(24), 0L, 0L),
    }.ToFrozenDictionary(static row => row.Key, StringComparer.Ordinal);
}

public static class ClassificationGuard {
    public static readonly FrozenSet<DataClassification> Unpersistable =
        new[] { DataClassification.Credential, DataClassification.Secret }.ToFrozenSet();

    public static Fin<DataClassification> Admit(Option<DataClassification> classification, string artifactClass) =>
        classification switch {
            { IsSome: true, Case: DataClassification row } when Unpersistable.Contains(row) =>
                Fin.Fail<DataClassification>(Error.New($"<unpersistable:{row.Key}:{artifactClass}>")),
            { IsSome: true, Case: DataClassification row } => Fin.Succ(row),
            _ => Fin.Fail<DataClassification>(Error.New($"<unclassified:{artifactClass}>")),
        };

    public static bool DemandsEncryption(Seq<DataClassification> present) =>
        present.Exists(static row => row == DataClassification.Personal);
}
```

## [3]-[RETENTION_SWEEPS]

- Owner: `RetentionPolicy` `[SmartEnum<string>]` under the `RetentionKeyPolicy` ordinal accessor; `RetentionSweep` fold; `ClosureGc` reachability-sweep fold; `ArtifactFacts` survey row; `SweepReceipt`; `ClosureGcReceipt`.
- Cases: 4 policy rows — age-bound, count-bound, size-bound, legal-hold.
- Entry: `public static IO<Seq<SweepReceipt>> Sweep(ClockPolicy clocks, Func<ArtifactClassRow, IO<Seq<ArtifactFacts>>> survey, Func<ArtifactClassRow, Seq<ArtifactFacts>, IO<(long Rows, long Bytes)>> evict)` — `IO<T>` carries the deletion effect.
- Auto: `SweepEntry` registers the fold as one schedule row under the maintenance lease with the `@daily` config-sourced default, the store drain order invokes the same fold once before close, and receipts ride the receipt sink as `retention-sweep` kind rows stamped with the ambient correlation.
- Receipt: one `SweepReceipt` per registry row — surveyed, held, eligible, deleted rows, bytes, `Instant` stamp; deletion is never silent.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new bound grammar is one `RetentionPolicy` case; a new cadence is one policy value on the schedule row; a new retained class lands as one `ArtifactClassRow` registry row and one matching `RetentionPolicy` reference, and the `Sweep` fold is the realized hold-first transformer — its `let live = facts.Filter(!Hold)` step precedes every `Eligible` evaluation so the `LegalHold` row dominates the `RetentionPolicy` bound by construction, composing the new class with zero consumer change; the object-store reachability GC is the `ClosureGc` fold beside `RetentionSweep`, not a new surface; zero new surface.
- Boundary: hold exclusion precedes every eligibility fold inside `Sweep`, so legal-hold beats sweep by construction and a held row is invisible to every bound; the idempotency-dedup registry row is the suite dedup-window horizon the transaction dedup table consumes; destructive migration steps wait on a `retention-approval` kind row on the same fact stream; survey orders rows newest-first so `Rank` and `RunningBytes` derive in recency order and count and size bounds evict oldest first; the kv-row class projects `Stamp` as the row expiry instant and every other class projects the creation instant; horizons and stamps read `ClockPolicy` only — `DateTime.UtcNow` and direct timers are the deleted patterns; `ClosureGc.Collect` is the object-store garbage collector — it reads the live reachable content-key set as the union over every live sync `Closure` manifest (the `collaboration#TRANSPORT_AXIS` descendant content-key manifest, consumed here as settled), set-differences it against the object-store residence listing (`object-store#OBJECT_RESIDENCE`), and evicts each unreferenced blob, holding any key in a `LegalHold` artifact class so a held blob is never collected regardless of reachability — a mark-and-sweep over loose blobs, a second reachability walk, or a polling scan that re-derives the closure is the deleted form, the reachable set composes the existing `Closure` manifest rather than re-walking the graph, and eviction routes through the same object-store delete the residence axis owns; the sweep stays SPIKE on the live object-store-plus-closure probe in this page's RESEARCH cluster.

```csharp signature
public sealed class RetentionKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<RetentionKeyPolicy, string>]
[KeyMemberComparer<RetentionKeyPolicy, string>]
public sealed partial class RetentionPolicy {
    public static readonly RetentionPolicy AgeBound = new("age-bound", static (facts, rule, now) => facts.Stamp + rule.Horizon < now);
    public static readonly RetentionPolicy CountBound = new("count-bound", static (facts, rule, _) => facts.Rank >= rule.Count);
    public static readonly RetentionPolicy SizeBound = new("size-bound", static (facts, rule, _) => facts.RunningBytes > rule.Bytes);
    public static readonly RetentionPolicy LegalHold = new("legal-hold", static (_, _, _) => false);

    [UseDelegateFromConstructor]
    public partial bool Eligible(ArtifactFacts facts, ArtifactClassRow rule, Instant now);
}

public readonly record struct ArtifactFacts(
    string Key,
    DataClassification Classification,
    Instant Stamp,
    int Rank,
    long RunningBytes,
    bool Hold);

public readonly record struct SweepReceipt(
    string ArtifactClass,
    RetentionPolicy Policy,
    int Surveyed,
    int Held,
    int Eligible,
    long Deleted,
    long Bytes,
    Instant At);

public static class RetentionSweep {
    public static IO<Seq<SweepReceipt>> Sweep(
        ClockPolicy clocks,
        Func<ArtifactClassRow, IO<Seq<ArtifactFacts>>> survey,
        Func<ArtifactClassRow, Seq<ArtifactFacts>, IO<(long Rows, long Bytes)>> evict) =>
        toSeq(ArtifactClasses.Registry.Values)
            .TraverseM(rule =>
                from at in IO.lift(() => clocks.Now)
                from facts in survey(rule)
                let live = facts.Filter(static row => !row.Hold)
                let eligible = live.Filter(row => rule.Policy.Eligible(row, rule, at))
                from swept in evict(rule, eligible)
                select new SweepReceipt(rule.Key, rule.Policy, facts.Count, facts.Count - live.Count, eligible.Count, swept.Rows, swept.Bytes, at))
            .As();

    public static ScheduleEntry SweepEntry(OccurrenceSpec cadence, Func<IO<Seq<SweepReceipt>>> sweep) =>
        new(
            Key: "persistence-retention-sweep",
            Spec: cadence,
            Deadline: DeadlineClass.SupportWindow,
            Lease: Optional(LeasePolicy.Maintenance),
            Work: () => sweep().Map(static _ => unit));
}

public readonly record struct ClosureGcReceipt(
    int Listed,
    int Reachable,
    int Held,
    long Collected,
    long Bytes,
    Instant At);

public static class ClosureGc {
    public static IO<ClosureGcReceipt> Collect(
        ClockPolicy clocks,
        IO<HashSet<UInt128>> reachable,
        IO<Seq<(UInt128 ContentKey, long Bytes)>> residence,
        Func<UInt128, bool> held,
        Func<Seq<UInt128>, IO<(long Rows, long Bytes)>> evict) =>
        from at in IO.lift(() => clocks.Now)
        from live in reachable
        from listed in residence
        let unreferenced = listed.Filter(row => !live.Contains(row.ContentKey) && !held(row.ContentKey))
        from swept in evict(unreferenced.Map(static row => row.ContentKey))
        select new ClosureGcReceipt(listed.Count, live.Count, listed.Count(row => held(row.ContentKey)), swept.Rows, swept.Bytes, at);

    public static ScheduleEntry GcEntry(OccurrenceSpec cadence, Func<IO<ClosureGcReceipt>> collect) =>
        new(
            Key: "persistence-object-store-gc",
            Spec: cadence,
            Deadline: DeadlineClass.SupportWindow,
            Lease: Optional(LeasePolicy.Maintenance),
            Work: () => collect().Map(static _ => unit));
}
```

| [INDEX] | [POLICY]      | [VALUE]                               | [BINDING]                                                 |
| :-----: | :------------ | :------------------------------------ | :-------------------------------------------------------- |
|   [1]   | sweep cadence | config-sourced cron, `@daily` default | `SweepEntry` schedule row under `LeasePolicy.Maintenance` |
|   [2]   | drain sweep   | one fold invocation before close      | band-300 store drain order                                |

## [4]-[EXPORT_PROOF]

- Owner: `StoreEvidence` contribution rows; `ExportProof` receipt.
- Cases: 7 artifact rows — store-metadata, open-receipts, schema-history, pragma-snapshot, fault-receipts, sweep-receipts, export-proofs.
- Entry: `public static ExportProof Prove(ClockPolicy clocks, string artifact, DataClassification classification, ReadOnlyMemory<byte> redacted, int redactions, int failures)` — pure value.
- Auto: `Contribution` lands the seven rows as `SupportArtifact` entries on the package's one `SupportContributorPort`, so the AppHost capture owner drives trigger, window, correlation, and bundle caps while assembly and proof stay store-side.
- Receipt: one `ExportProof` per artifact — classification, redactor kind, redaction and failure counts, `XxHash128` content hash, bytes, `Instant` stamp; proofs ship inside the bundle through the export-proofs row.
- Packages: System.IO.Hashing, Microsoft.Extensions.Compliance.Redaction, NodaTime, LanguageExt.Core, Rasm.AppHost (project).
- Growth: one artifact row lands a new contribution; zero new surface.
- Boundary: produce delegates emit only redacted bytes with their redaction count over the frozen capture `Interval` — the redactor resolves from the row classification's `Redactor` column through the AppHost-registered `IRedactorProvider`, sizes through `GetRedactedLength(ReadOnlySpan<char>)`, and fills through `Redact(ReadOnlySpan<char>, Span<char>)`, deleting ad hoc string masking and a local redactor map; the three concrete redactors arrive settled from the AppHost registration and are never re-registered here — `RedactorKind.None` resolves the `NullRedactor` pass-through for Operational and None columns, `RedactorKind.Hmac` resolves the join-preserving `HmacRedactor` for HostIdentity and Personal columns so a pseudonymized identifier stays correlatable across artifacts without exposing the value, and an unmapped classification falls to the AppHost `SetFallbackRedactor<ErasingRedactor>` default so an unrecognized class fails closed by erasure rather than leaking; a failed redaction folds its source row into a redaction-failure entry written into the same artifact stream, never silent evidence loss; estimated bytes are row values on the `Rows` table and the AppHost artifact cap truncates above them.

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

    public static SupportContributorPort Contribution(Func<string, DataClassification, Func<Interval, IO<(ReadOnlyMemory<byte> Bytes, int Redactions)>>> produce) =>
        new("Rasm.Persistence", Rows.Map(row => new SupportArtifact(row.Artifact, row.Classification, row.EstimatedBytes, produce(row.Artifact, row.Classification))));

    public static ExportProof Prove(ClockPolicy clocks, string artifact, DataClassification classification, ReadOnlyMemory<byte> redacted, int redactions, int failures) =>
        new(
            Artifact: artifact,
            Classification: classification,
            Redactor: classification.Redactor,
            Redactions: redactions,
            RedactionFailures: failures,
            ContentHash: XxHash128.HashToUInt128(redacted.Span),
            Bytes: redacted.Length,
            At: clocks.Now);
}
```

## [5]-[AUDIT_BINDING]

- Owner: `AuditBinding` category table with the per-tenant audit-binding projection.
- Cases: 5 binding rows over the persistable classification values in escalating rank order.
- Entry: `public static string Bind(Seq<DataClassification> present)` — pure value.
- Packages: LanguageExt.Core, BCL inbox, Rasm.AppHost (project).
- Growth: one classification binding is one `Categories` row; a tenant-scoped audit binding is one `BindTenant` projection over the same table, never a second category vocabulary; zero new surface.
- Boundary: audit execution is server-log-side operations — the runtime obligation is the bound `pgaudit.log` value verified against `pg_settings` through the store provisioning probe, and a client-side audit log pipeline is the rejected form; the provisioning verifier folds the registry classification column plus observed per-row classifications through `Bind`, and a mismatch or missing `shared_preload_libraries=pgaudit` entry folds into the provisioning fault; the RLS-policy mechanics — the per-tenant `CREATE POLICY` and the `TenancyModel` single/schema/rls/db-per-tenant axis — are owned on `provisioning#TENANCY_RLS`, and this binding owns only the consequence: a tenant-id-bearing classification binds to one pgaudit category through `BindTenant`, so a per-tenant policy ties to the audit category and a per-tenant RLS-policy-applied fact plus the `Bind` result fact fold into the provisioning verifier where a tenant-isolation mismatch is a typed provisioning fault rather than silent; `Bind` admits only persistable rows because the write guard rejects Credential and Secret upstream.

```csharp signature
public static class AuditBinding {
    public static readonly FrozenDictionary<DataClassification, (int Rank, string Log)> Categories = new[] {
        (DataClassification.None, (0, "")),
        (DataClassification.Operational, (1, "ddl, role")),
        (DataClassification.HostIdentity, (2, "write, ddl, role")),
        (DataClassification.UserContent, (3, "write, ddl, role")),
        (DataClassification.Personal, (4, "read, write, ddl, role")),
    }.ToFrozenDictionary(static row => row.Item1, static row => row.Item2);

    public static string Bind(Seq<DataClassification> present) =>
        Categories[present.Fold(DataClassification.None, static (acc, row) =>
            Categories[row].Rank > Categories[acc].Rank ? row : acc)].Log;

    public static (string Tenant, string Log) BindTenant(string tenant, Seq<DataClassification> present) =>
        (tenant, Bind(present));
}
```

## [6]-[RESEARCH]

- [PGAUDIT_CATEGORIES]: pgaudit session-audit category semantics on PG18 under `shared_preload_libraries=pgaudit` against the `Categories` rows, and the per-tenant `BindTenant` category emission verified against a per-tenant `CREATE POLICY` (the policy mechanics owned on `provisioning#TENANCY_RLS`) on a live PG18 server.
- [CLOSURE_GC]: `ClosureGc.Collect` reachable-set-versus-residence eviction verified against a live object-store residence listing and a live sync `Closure` membership union — the unreferenced-blob set, the `LegalHold` exemption under a live class registry, and the eviction-delete round-trip against the object-store delete the residence axis owns.
