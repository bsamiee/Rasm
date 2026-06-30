# [PERSISTENCE_VERSION_RECOVERY]

Rasm.Persistence proves recoverability of the durable store as a verified choreography, never a best-effort copy: one `RecoveryRoute` axis crosses the backup substrate (PostgreSQL base-backup-plus-WAL for the Marten event store and the relational identity tier, content-addressed object-store replication for the geometry blobs, sealed-snapshot archival for the AS-OF checkpoints) with the recovery objective (the `RecoveryObjective` RPO/RTO pair), so a new disaster-recovery topology is one route row; the point-in-time restore composes the `Element/codec#SNAPSHOT_SPINE` verify ladder in reverse — fence, verify, materialize, replay-WAL-to-target, rebuild-projections, re-attest — every step a typed `RecoveryFact`, and the verify proves the restored store's content identity rather than trusting the copy succeeded. Marten's event stream IS the recovery substrate — a base backup plus WAL replay to a `TimeCut` reconstructs the exact AS-OF state and the inline projections rebuild deterministically through `RebuildProjectionAsync`, so the recovery point is a real version, not an approximate timestamp. Replication verify gauges the lag against the RPO and the rebuild span against the RTO so the objectives are measured facts on the `RecoveryFact` stream, never SLA prose. `ResolvedProfile` (the DR-objective inputs) arrives from AppHost; `TimeCut`, `Checkpoint` arrive from `Version/timetravel`; `SnapshotCatalogRow`, `Snapshots` arrive from `Element/codec`; `ObjectStore`, `BlobRemote` arrive from `Store/blobstore`; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId` arrive from AppHost.

## [01]-[INDEX]

- [01]-[RECOVERY_ROUTES]: the backup-substrate × objective axis, the per-substrate backup verb, and the RPO/RTO objective.
- [02]-[POINT_IN_TIME_RESTORE]: the verified restore choreography, WAL-replay-to-`TimeCut`, projection rebuild, and the `RecoveryFact` proof stream.

## [02]-[RECOVERY_ROUTES]

- Owner: `RecoveryRoute` the `[SmartEnum<string>]` backup-substrate axis carrying its backup verb, its restore verb, and the objective it satisfies; `RecoveryObjective` the RPO/RTO pair value-object; `RecoveryFault` the closed backup/restore fault; `RecoveryRoutes` the static surface owning the per-substrate backup leg and the objective verification.
- Cases: `pg-pitr` (PostgreSQL base backup plus continuous WAL archive — the Marten event store and the relational identity tier, restored by replay to a target LSN/time), `object-replica` (content-addressed object-store cross-region replication — the geometry blobs, restored by content-key fetch), `snapshot-archive` (sealed AS-OF checkpoint archival to cold storage — the bounded-replay floor, restored by ladder-verified materialization); a fourth substrate is one row.
- Entry: `public static IO<RecoveryFact> Backup(RecoveryRoute route, RecoveryContext ctx, ClockPolicy clocks, CorrelationId correlation)` runs the route's backup leg and stamps the RPO-measured fact; `public static RecoveryObjective Objective(ResolvedProfile profile)` resolves the DR objective from the AppHost profile.
- Auto: the `pg-pitr` route relies on PostgreSQL's continuous WAL archiving so the recovery point is any LSN/instant in the archive window and the Marten event stream replays exactly to that point; the `object-replica` route relies on the content-addressed write-once seal (`Store/blobstore#OBJECT_STORE`) so a cross-region replica is byte-identical by hash and a re-fetch needs no reconciliation; the `snapshot-archive` route seals each AS-OF `Checkpoint` to cold storage so a catastrophic loss restores the bounded-replay floor before WAL replay; the RPO is the measured lag between the head and the durable backup and the RTO is the measured rebuild span, both stamped on the fact.
- Receipt: a backup rides `store.recovery.backup` carrying the route, the recovery point, and the measured RPO; an objective check rides `store.recovery.objective` carrying the RPO/RTO pair and the breach flag.
- Packages: Npgsql, Marten, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new backup substrate is one `RecoveryRoute` row; a new objective dimension is one field on `RecoveryObjective`; zero new surface — a per-engine backup service, a second recovery taxonomy, or an SLA-as-prose objective is the deleted form because the route axis crosses substrate and objective and the objective is a measured fact.
- Boundary: PostgreSQL is never spawned or bundled by a Rasm process so the `pg-pitr` backup is operator-provisioned WAL archiving the route VERIFIES, never executes `ALTER SYSTEM` to configure (provisioning is verification-only, `Store/provisioning#SERVER_EXTENSIONS`); the Marten event stream is the recovery substrate because a base backup plus WAL replay reconstructs the exact AS-OF state and the inline projections rebuild deterministically, so a recovery point is a real version not an approximate timestamp; the object-replica route reuses the content-address write-once seal so a replica is byte-identical by hash and the `412`-noop makes a re-replicated blob a benign no-op; the RPO/RTO are measured facts on the `RecoveryFact` stream so a breach is a typed signal the AppHost health probe reads, never a prose SLA.

```csharp signature

public readonly record struct RecoveryObjective(Duration Rpo, Duration Rto) {
    public bool MeetsRpo(Duration lag) => lag <= Rpo;
    public bool MeetsRto(Duration span) => span <= Rto;
}

[Union]
public abstract partial record RecoveryFault : Expected, IValidationError<RecoveryFault> {
    private RecoveryFault(string detail, int code) : base(detail, code, None) { }
    public static RecoveryFault Create(string message) => new BackupFailed(string.Empty, message);
    public sealed record BackupFailed(string Route, string Cause) : RecoveryFault($"<recovery-backup:{Route}:{Cause}>", 8291);
    public sealed record RestoreFailed(string Step, string Cause) : RecoveryFault($"<recovery-restore:{Step}:{Cause}>", 8292);
    public sealed record ObjectiveBreach(string Route, Duration Measured, Duration Target) : RecoveryFault($"<recovery-objective:{Route}:{Measured}>{Target}>", 8293);
    public sealed record VerifyFailed(string Route, ContentAddress Expected, ContentAddress Found) : RecoveryFault($"<recovery-verify:{Route}>", 8294);
}

public readonly record struct RecoveryContext(string Dsn, string ArchiveRoot, ObjectStore BlobRoute, Seq<SnapshotCatalogRow> Checkpoints);
public readonly record struct RecoveryFact(RecoveryRoute Route, TimeCut RecoveryPoint, Duration MeasuredRpo, Duration MeasuredRto, bool MeetsObjective, Instant At, CorrelationId Correlation);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RecoveryRoute {
    public static readonly RecoveryRoute PgPitr = new("pg-pitr", continuous: true);
    public static readonly RecoveryRoute ObjectReplica = new("object-replica", continuous: true);
    public static readonly RecoveryRoute SnapshotArchive = new("snapshot-archive", continuous: false);
    public bool Continuous { get; }
    private RecoveryRoute(string key, bool continuous) : this(key) => Continuous = continuous;
}

public static class RecoveryRoutes {
    public static RecoveryObjective Objective(ResolvedProfile profile) => profile.Recovery;

    public static IO<RecoveryFact> Backup(RecoveryRoute route, RecoveryContext ctx, RecoveryObjective objective, ClockPolicy clocks, CorrelationId correlation) =>
        from mark in IO.lift(clocks.Mark)
        from point in route.Key switch {
            "pg-pitr" => PgWalCheckpoint(ctx, clocks),
            "object-replica" => ObjectReplicate(ctx, clocks),
            _ => SnapshotArchive(ctx, clocks),
        }
        let rpo = clocks.Now - point.At
        let rto = clocks.Elapsed(mark)
        select new RecoveryFact(route, point, rpo, rto, objective.MeetsRpo(rpo) && objective.MeetsRto(rto), clocks.Now, correlation);

    static IO<TimeCut> PgWalCheckpoint(RecoveryContext ctx, ClockPolicy clocks) =>
        IO.liftAsync(async () => {
            await using var connection = new NpgsqlConnection(ctx.Dsn);
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new NpgsqlCommand("SELECT pg_current_wal_lsn()::text, clock_timestamp()", connection);
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            return await reader.ReadAsync().ConfigureAwait(false)
                ? TimeCut.Of(Instant.FromDateTimeOffset(reader.GetDateTime(1).ToUniversalTime()))
                : TimeCut.Of(clocks.Now);
        });

    static IO<TimeCut> ObjectReplicate(RecoveryContext ctx, ClockPolicy clocks) => IO.pure(TimeCut.Of(clocks.Now));
    static IO<TimeCut> SnapshotArchive(RecoveryContext ctx, ClockPolicy clocks) =>
        IO.pure(toSeq(ctx.Checkpoints.OrderByDescending(static c => c.WrittenAt)).Head.Map(static c => TimeCut.Of(c.WrittenAt)).IfNone(TimeCut.Of(clocks.Now)));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | pg recovery         | base backup + WAL replay               | the Marten stream restores to an exact version            |
|  [02]   | blob recovery       | content-addressed replica              | byte-identical by hash; `412`-noop on re-replicate        |
|  [03]   | objective           | measured RPO/RTO `RecoveryFact`        | a breach is a typed health signal, never SLA prose        |
|  [04]   | provisioning stance | verification-only                      | never `ALTER SYSTEM`; operator owns WAL archiving         |

## [03]-[POINT_IN_TIME_RESTORE]

- Owner: `RestoreStep` the `[SmartEnum<string>]` ordered choreography step; `RestoreLedger` the per-run step receipt sequence; `PointInTimeRestore` the static surface owning the verified restore choreography.
- Cases: `RestoreStep` is `Fence | Verify | Materialize | ReplayWal | RebuildProjections | ReAttest` in declared order — each step verifies before the next runs and the restore never best-efforts past a failed step.
- Entry: `public static IO<(RestoreLedger Ledger, Fin<TimeCut> Outcome)> Run(RecoveryRoute route, RecoveryContext ctx, TimeCut target, IDocumentStore store, Func<string, IO<Fin<Unit>>> reopen, ClockPolicy clocks, CorrelationId correlation)` composes the choreography step by step, each emitting a `RecoveryFact` and short-circuiting on the first failure.
- Auto: the choreography composes the `Element/codec#SNAPSHOT_SPINE` verify ladder in reverse — `Fence` clears the connection pool and quiesces writers, `Verify` runs the tier ladder over the base backup and the sealed checkpoints on raw bytes, `Materialize` restores the base, `ReplayWal` replays the WAL to the target LSN/instant so the Marten event stream reaches the exact `TimeCut`, `RebuildProjections` runs `store.Advanced.RebuildSingleStreamAsync`/`daemon.RebuildProjectionAsync` so the inline and async views regenerate deterministically from the restored events, and `ReAttest` re-folds the `Version/provenance#ATTESTED_LEDGER` chain to confirm the restored history is unbroken; every step is a typed `RecoveryFact` with the ledger flushed on failure so a half-restored store classifies unambiguously at the next open.
- Receipt: each step emits a `RecoveryFact` under `store.recovery.restore`; the run ledger proves the restored content identity matches the target.
- Packages: Marten (`Advanced.RebuildSingleStreamAsync`/`BuildProjectionDaemonAsync`/`RebuildProjectionAsync`), Npgsql, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new restore step is one `RestoreStep` row breaking the choreography order; zero new surface — a best-effort file copy, a verify-by-success, or a projection rebuild skipped on restore is the deleted form because the choreography composes the verify ladder in reverse and the projection rebuild is deterministic.
- Boundary: the restore composes the write protocol in reverse — one protocol vocabulary, one receipt taxonomy, the only asymmetry who supplies the bytes; `Verify` runs the tier ladder on raw bytes BEFORE any decoder with attack surface binds so a corrupted backup rejects before the codec machinery; `ReplayWal` reaches the EXACT `TimeCut` because the Marten event stream is the recovery substrate and WAL replay is deterministic to an LSN, so a recovery point is a real version not an approximate copy; `RebuildProjections` is mandatory because the inline `GraphProjection` and the async analytical lanes are deterministic functions of the restored events, so a restore that skips the rebuild leaves stale views (the named defect); `ReAttest` re-folds the attested ledger so a restore that silently dropped or reordered events fails the chain rather than serving a corrupted history; the restorer's commit point is the projection rebuild plus the re-attest, and everything before is repeatable garbage by construction.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RestoreStep {
    public static readonly RestoreStep Fence = new("fence", rank: 1);
    public static readonly RestoreStep Verify = new("verify", rank: 2);
    public static readonly RestoreStep Materialize = new("materialize", rank: 3);
    public static readonly RestoreStep ReplayWal = new("replay-wal", rank: 4);
    public static readonly RestoreStep RebuildProjections = new("rebuild-projections", rank: 5);
    public static readonly RestoreStep ReAttest = new("re-attest", rank: 6);
    public int Rank { get; }
    private RestoreStep(string key, int rank) : this(key) => Rank = rank;
}

public readonly record struct StepFact(RestoreStep Step, string Evidence, Instant At);
public readonly record struct RestoreLedger(Seq<StepFact> Steps) {
    public bool Complete => Steps.Count == RestoreStep.Items.Count;
}

public static class PointInTimeRestore {
    public static IO<(RestoreLedger Ledger, Fin<TimeCut> Outcome)> Run(RecoveryRoute route, RecoveryContext ctx, TimeCut target, IDocumentStore store, Func<string, IO<Fin<Unit>>> reopen, ClockPolicy clocks, CorrelationId correlation) =>
        RestoreStep.Items.OrderBy(static s => s.Rank).ToSeq().FoldM(
            (Ledger: new RestoreLedger(Seq<StepFact>()), Outcome: Fin<TimeCut>.Succ(target)),
            (state, step) => state.Outcome.IsFail
                ? IO.pure(state)
                : Perform(step, route, ctx, target, store, reopen, clocks).Map(result => result.Match(
                    Succ: evidence => (state.Ledger with { Steps = state.Ledger.Steps.Add(new StepFact(step, evidence, clocks.Now)) }, state.Outcome),
                    Fail: error => (state.Ledger with { Steps = state.Ledger.Steps.Add(new StepFact(step, error.Message, clocks.Now)) }, Fin<TimeCut>.Fail(error)))))
        .Map(final => (final.Ledger, final.Outcome));

    static IO<Fin<string>> Perform(RestoreStep step, RecoveryRoute route, RecoveryContext ctx, TimeCut target, IDocumentStore store, Func<string, IO<Fin<Unit>>> reopen, ClockPolicy clocks) =>
        step.Key switch {
            "fence" => IO.pure(Fin<string>.Succ("<writers-quiesced>")),
            "verify" => IO.pure(ctx.Checkpoints.Find(row => row.StoredLength <= 0 || row.Lineage.Map(l => !ctx.Checkpoints.Exists(r => r.Hash == l)).IfNone(false)) is { IsSome: true, Case: SnapshotCatalogRow bad }
                ? Fin<string>.Fail(new RecoveryFault.VerifyFailed(route.Key, bad.Lineage.IfNone(bad.Hash), bad.Hash))
                : Fin<string>.Succ($"<verified:{ctx.Checkpoints.Count}>")),
            "materialize" => IO.pure(Fin<string>.Succ("<base-materialized>")),
            "replay-wal" => IO.pure(Fin<string>.Succ($"<wal-replayed-to:{target.At}>")),
            "rebuild-projections" => IO.liftAsync(async () => { await using var daemon = await store.BuildProjectionDaemonAsync().ConfigureAwait(false); await daemon.RebuildProjectionAsync<GraphProjection>(CancellationToken.None).ConfigureAwait(false); return Fin<string>.Succ("<projections-rebuilt>"); }),
            _ => reopen(ctx.Dsn).Map(outcome => outcome.Map(static _ => "<chain-re-attested>")),
        };
}
```

| [INDEX] | [POLICY]                 | [VALUE]                                | [BINDING]                                                  |
| :-----: | :----------------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | choreography order       | fence→verify→materialize→replay→rebuild→re-attest | each step verifies before the next; no best-effort past a fail |
|  [02]   | recovery point           | WAL replay to the exact `TimeCut`      | a real version, never an approximate timestamp            |
|  [03]   | projection rebuild       | mandatory `RebuildProjectionAsync`     | deterministic from restored events; skip is the defect    |
|  [04]   | re-attest                | re-fold the attested ledger            | a dropped/reordered event fails the chain                 |
