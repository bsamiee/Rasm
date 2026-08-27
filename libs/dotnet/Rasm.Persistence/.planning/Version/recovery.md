# [PERSISTENCE_VERSION_RECOVERY]

`RecoveryRoutes` captures exact PostgreSQL timeline/LSN and per-stream Marten coordinates, verifies object replicas and sealed checkpoints, and records measured RPO/RTO facts. `PointInTimeRestore` owns the ordered fence, verify, materialize, WAL replay, projection rebuild, and re-attestation choreography. Recovery succeeds only when the restored event stream reconstructs the target `ContentAddress` and the attested ledger verifies.

## [01]-[INDEX]

- [02]-[RECOVERY_ROUTES]: `RecoveryRoutes` crosses backup substrate with objective — running each substrate's backup leg, capturing the real `RecoveryPoint` coordinate, and gauging the measured RPO/RTO fact.
- [03]-[POINT_IN_TIME_RESTORE]: `PointInTimeRestore` runs the verified choreography — `Snapshots.Verify` ladder in reverse, WAL replay to the `RecoveryPoint`, head-caught-up projection rebuild, re-attest content-identity proof — and flushes the `StepFact` ledger.

## [02]-[RECOVERY_ROUTES]

- Owner: `RecoveryRoute` the `[SmartEnum<string>]` backup-substrate axis carrying its continuity flag (the live-lag-versus-checkpoint-age RPO discriminant); `RecoveryPoint` the `[ComplexValueObject]` recovery coordinate (PostgreSQL `Timeline`+`Lsn`, the Marten `StreamVersion`, the HLC `At`); `RecoveryFault` the closed backup/restore `[Union]` deriving from `Fault`; `RecoveryFact` the measured backup fact; `RecoveryRoutes` the static surface owning the per-substrate backup leg, the coordinate capture, and the objective gauge; RPO/RTO `RecoveryObjective` is NOT re-declared here — it is the `dotnet:Rasm.AppHost/Runtime/profiles#PROFILE_AXIS` `RecoveryObjective` this package IMPORTS, the composition root threading `ResolvedProfile.Recovery` in as the value, and the route→retention-class binding is the `Version/retention#RETENTION_CLASSES` owner's (`stream`/`blob`/`snapshot`), never duplicated here.
- Cases: `RecoveryRoute` is three rows — `pg-pitr` (PostgreSQL base backup with continuous WAL archive — the Marten event store and the relational identity tier, restored by replay to a target `(Timeline, Lsn)`), `object-replica` (content-addressed object-store cross-region replication — the geometry blobs, restored by content-key `Head`-confirm), `snapshot-archive` (sealed AS-OF `Checkpoint` archival to cold storage — the bounded-replay floor, restored by `Snapshots.Verify`-gated materialization); a fourth substrate is one row carrying its continuity flag. `RecoveryFault` is `BackupFailed | RestoreFailed | ObjectiveBreach | VerifyFailed | TimelineDivergence | ReplicationLag` — `TimelineDivergence` the re-bootstrap-vs-resume guard (a captured coordinate on a timeline the archive does not continue), `ReplicationLag` the live-lag RPO breach on a continuous route, `ObjectiveBreach` the RTO over the objective.
- Entry: `public static IO<RecoveryFact> Backup(RecoveryRoute route, RecoveryContext ctx, RecoveryObjective objective, ProjectionContext frame)` runs the route's real backup leg, captures the `RecoveryPoint` coordinate, and stamps the RPO/RTO-measured fact under `objective`, the AppHost-settled window the composition root threads in.
- Auto: the `pg-pitr` leg opens a `LogicalReplicationConnection`, reads the live coordinate through `IdentifySystem` (the `XLogPos` head LSN and the `Timeline`), takes the durable-archive flush LSN from the operator-supplied `RecoveryContext.ArchiveFlushed` cursor (a Rasm process verifies the archive, never queries a standby it does not own), measures the RPO as the head-minus-flushed WAL byte lag (an `NpgsqlLogSequenceNumber` comparison clamped at zero against a probe-race cursor inversion, projected to a `Duration` through the EXPLICIT `RecoveryContext.WalBytesPerSecond` throughput POLICY row — a hardcoded segment-size-as-rate literal is the deleted fabrication), and binds the PER-STREAM Marten head from `Events.FetchStreamStateAsync(ctx.Model).Version` when the DR exercise targets a model (`None` otherwise — the restore then folds by `timestamp:`), NEVER the store-wide `FetchEventStoreStatistics().EventSequenceNumber` high-water, a different version axis that folds a per-stream `AggregateStreamAsync(version:)` to the head; the `object-replica` leg folds the geometry-blob manifest (content key + local seal instant) through `ObjectStore.Head` against the replica client so an EMPTY missing set proves the cross-region replica byte-identical by content key (the write-once seal makes a re-replicated blob a benign `412`-noop) and the RPO is the age of the OLDEST locally-sealed blob the replica still lacks — the true data-loss window, never a count-of-absent-blobs fabricated as minutes; the `snapshot-archive` leg seals the newest `Checkpoint` to cold storage and immediately re-reads it through `Snapshots.Verify` on raw bytes so an archived checkpoint that fails the ladder faults at backup time, never at restore; the RTO is the `frame.Elapsed(mark)` backup span and both objectives gauge against the settled `RecoveryObjective` by direct `Duration` comparison.
- Packages: Npgsql (`LogicalReplicationConnection.IdentifySystem`, `ReplicationSystemIdentification.XLogPos`/`Timeline`, `NpgsqlLogSequenceNumber` comparison operators + cast to `ulong`), Marten (`DocumentStore.For`, `IQuerySession.Events.FetchStreamStateAsync` → `StreamState.Version`), Rasm.AppHost (`Runtime/profiles#PROFILE_AXIS` `RecoveryObjective`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new backup substrate is one `RecoveryRoute` row carrying its continuity flag; a new objective dimension is one field on the AppHost `RecoveryObjective` (never re-declared here); a new backup-time fault is one `RecoveryFault` case; zero new surface — a per-engine backup service, a second recovery taxonomy, an SLA-as-prose objective, or a locally re-declared objective record is the deleted form because the route axis crosses substrate and objective, the objective is the AppHost-settled measured fact, and the recovery point is the real PostgreSQL coordinate.
- Boundary: PostgreSQL is never spawned or bundled by a Rasm process so the `pg-pitr` backup is operator-provisioned WAL archiving the route VERIFIES, never executes `ALTER SYSTEM` to configure (provisioning is verification-only, `Store/provisioning#SERVER_EXTENSIONS`); the recovery point is the `(Timeline, Lsn)` coordinate `IdentifySystem` yields, NEVER a `clock_timestamp()` wall-clock instant, because a base backup with WAL replay reconstructs the exact AS-OF state only when the replay target rides the same timeline the archive continues — a coordinate captured on a forked timeline faults `RecoveryFault.TimelineDivergence` at restore rather than silently replaying onto a divergent history; the `object-replica` route reuses the `Store/blobstore#OBJECT_STORE` content-addressed write-once seal so a replica is byte-identical by hash and the `412`-noop makes a re-replicated blob a benign no-op (the seal IS the concurrency primitive, no read-before-write), and the measured replication lag is the age of the oldest locally-sealed blob the replica still lacks (the point since which the replica is provably incomplete — every later seal is also unreplicated), a `RecoveryFault.ReplicationLag` when it exceeds the RPO; the `snapshot-archive` route seals each AS-OF `Checkpoint` and re-verifies it through the ONE `Snapshots.Verify` tier ladder so the archived bytes self-reject before cold storage if torn; the RPO/RTO are measured facts on the `RecoveryFact` stream so a breach is a typed signal the AppHost health probe reads, never a prose SLA, and the `RecoveryObjective` is the AppHost-owned vocabulary read settled, never a parallel local record.

```csharp

// --- [TYPES] ---------------------------------------------------------------------------

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

[ComplexValueObject]
public sealed partial class RecoveryPoint {
    public uint Timeline { get; }
    public ulong Lsn { get; }
    public Option<long> StreamVersion { get; }
    public Instant At { get; }

    public static RecoveryPoint Of(ReplicationSystemIdentification id, Option<long> streamVersion, Instant at) =>
        Create(id.Timeline, (ulong)id.XLogPos, streamVersion, at);
    public static RecoveryPoint Floor(Instant at) => Create(0u, 0UL, None, at);

    public bool Continues(uint archiveTimeline) => Timeline == archiveTimeline;

    public TimeCut AsCut() => StreamVersion.Match(Some: v => TimeCut.AtVersion(v, new Hlc(At, 0UL)), None: () => TimeCut.Of(At));
}

// --- [ERRORS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record RestoreRefusal {
    private RestoreRefusal() { }
    public sealed record TargetAbsent(ModelId Model) : RestoreRefusal;
    public sealed record Attestation(AttestVerdict Verdict) : RestoreRefusal;

    public string Detail => Map(
        targetAbsent: static row => $"<model-absent:{row.Model.Value}>",
        attestation: static row => $"<attestation:{row.Verdict.Key}>");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RecoveryFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Recovery;
    private RecoveryFault() { }
    [FaultCase(0)]
    public sealed partial record BackupFailed(string Route, string Detail) : RecoveryFault();
    [FaultCase(1)]
    public sealed partial record RestoreFailed(RestoreRefusal Refusal) : RecoveryFault();
    [FaultCase(2)]
    public sealed partial record ObjectiveBreach(string Route, Duration Measured, Duration Target) : RecoveryFault();
    [FaultCase(3)]
    public sealed partial record VerifyFailed(string Route, ContentAddress Expected, ContentAddress Found) : RecoveryFault();
    [FaultCase(4)]
    public sealed partial record TimelineDivergence(string Route, uint Captured, uint Archive) : RecoveryFault();
    [FaultCase(5)]
    public sealed partial record ReplicationLag(string Route, Duration Measured, Duration Rpo) : RecoveryFault();

    public override string Message => Switch(
        backupFailed:       static c => $"<recovery-backup:{c.Route}:{c.Detail}>",
        restoreFailed:      static c => $"<recovery-restore:{c.Refusal.Detail}>",
        objectiveBreach:    static c => $"<recovery-objective:{c.Route}:{c.Measured}!={c.Target}>",
        verifyFailed:       static c => $"<recovery-verify:{c.Route}:{c.Expected.Value:x32}!={c.Found.Value:x32}>",
        timelineDivergence: static c => $"<recovery-timeline:{c.Route}:{c.Captured}!={c.Archive}>",
        replicationLag:     static c => $"<recovery-lag:{c.Route}:{c.Measured}!={c.Rpo}>");
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct RecoveryContext(
    string Dsn, string ArchiveRoot, uint ArchiveTimeline, NpgsqlLogSequenceNumber ArchiveFlushed, long WalBytesPerSecond,
    Option<ModelId> Model, ObjectStore BlobStore, ObjectClient BlobClient, Seq<(ContentAddress Key, Instant SealedAt)> ReplicaManifest,
    Seq<SnapshotCatalogRow> Checkpoints, ulong SchemaFingerprint, ulong Epoch);

public readonly record struct RecoveryFact(
    RecoveryRoute Route, RecoveryPoint Point, Duration MeasuredRpo, Duration BackupDuration, Option<Duration> MeasuredRto,
    bool MeetsObjective, Instant At, CorrelationId Correlation) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        MeasuredRpo >= Duration.Zero,
        BackupDuration >= Duration.Zero,
        MeasuredRto.Map(static rto => rto >= Duration.Zero).IfNone(true),
        MeetsObjective);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class RecoveryRoutes {
    public static IO<RecoveryFact> Backup(RecoveryRoute route, RecoveryContext ctx, RecoveryObjective objective, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from leg in route.Switch(
            state: (ctx, frame),
            pgPitr: static s => PgPitr(s.ctx, s.frame),
            objectReplica: static s => ObjectReplica(s.ctx, s.frame),
            snapshotArchive: static s => SnapshotFloor(s.ctx, s.frame))
        let backup = frame.Elapsed(mark)
        let fact = new RecoveryFact(route, leg.Point, leg.Rpo, backup, Option<Duration>.None, leg.Rpo <= objective.Rpo, frame.Now(), frame.Correlation)
        from gauged in route.Continuous && (leg.Rpo > objective.Rpo)
            ? IO.fail<RecoveryFact>(new RecoveryFault.ReplicationLag(route.Key, leg.Rpo, objective.Rpo))
            : IO.pure(fact)
        select gauged;

    static IO<(RecoveryPoint Point, Duration Rpo)> PgPitr(RecoveryContext ctx, ProjectionContext frame) =>
        IO.liftAsync(async () => await Try.lift(async _ => {
            await using LogicalReplicationConnection replication = new(ctx.Dsn);
            await replication.Open().ConfigureAwait(false);
            ReplicationSystemIdentification system = await replication.IdentifySystem().ConfigureAwait(false);
            await using DocumentStore store = DocumentStore.For(o => o.Connection(ctx.Dsn));
            Option<long> head = None;
            if (ctx.Model.Case is ModelId model) {
                await using IQuerySession query = store.QuerySession();
                StreamState? state = await query.Events.FetchStreamStateAsync(model.Value).ConfigureAwait(false);
                head = Optional(state?.Version);
            }
            ulong lagBytes = system.XLogPos >= ctx.ArchiveFlushed ? (ulong)system.XLogPos - (ulong)ctx.ArchiveFlushed : 0UL;
            return Fin<(RecoveryPoint, Duration)>.Succ(
                (RecoveryPoint.Of(system, head, frame.Now()), Duration.FromSeconds(lagBytes / double.Max(ctx.WalBytesPerSecond, 1d))));
        }).Run().Bind(static inner => inner).ConfigureAwait(false)).Bind(IO.lift);

    static IO<(RecoveryPoint Point, Duration Rpo)> ObjectReplica(RecoveryContext ctx, ProjectionContext frame) =>
        ctx.ReplicaManifest.TraverseM(entry => ctx.BlobStore.Head(ctx.BlobClient, entry.Key).Map(present => (entry.Key, entry.SealedAt, Present: present.IsSome))).As()
            .Map(probes => probes.Filter(static p => !p.Present))
            .Map(absent => (RecoveryPoint.Floor(frame.Now()),
                absent.Fold(Option<Instant>.None, static (oldest, a) => Some(oldest.Match(Some: m => Instant.Min(m, a.SealedAt), None: () => a.SealedAt)))
                    .Match(Some: oldest => frame.Now() - oldest, None: () => Duration.Zero)));

    static IO<(RecoveryPoint Point, Duration Rpo)> SnapshotFloor(RecoveryContext ctx, ProjectionContext frame) =>
        toSeq(ctx.Checkpoints.OrderByDescending(static c => c.WrittenAt)).Head.Match(
            Some: newest => IO.lift(() => Try.lift(() => Fin.Succ(ReadSealed(ctx.ArchiveRoot, newest.Id))).Run().Bind(static inner => inner))
                .Bind(bytes => Snapshots.Verify(bytes, ctx.SchemaFingerprint, ctx.Epoch).Match(
                    Succ: _ => IO.pure((RecoveryPoint.Floor(newest.WrittenAt), frame.Now() - newest.WrittenAt)),
                    Fail: IO.fail<(RecoveryPoint, Duration)>)),
            None: () => IO.fail<(RecoveryPoint, Duration)>(new RecoveryFault.BackupFailed("snapshot-archive", "<no-checkpoint-inventory>")));

    internal static byte[] ReadSealed(string root, Guid id) => File.ReadAllBytes(Path.Combine(root, $"{id}{Snapshots.Suffix}"));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                              | [BINDING]                                                |
| :-----: | :----------------- | :--------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | recovery point     | `(Timeline, Lsn)` + per-stream `StreamState.Version` | never the global `EventSequenceNumber` or a wall clock   |
|  [02]   | objective source   | caller-threaded `RecoveryObjective`                  | never a locally re-declared record                       |
|  [03]   | pg recovery        | base backup + WAL replay to coordinate               | the Marten stream restores to an exact version           |
|  [04]   | WAL RPO projection | byte lag ÷ `RecoveryContext.WalBytesPerSecond`       | explicit throughput row; no segment-size-as-rate literal |
|  [05]   | blob recovery      | `ObjectStore.Head` over `(key, sealedAt)` manifest   | byte-identical by hash; lag = oldest missing seal's age  |
|  [06]   | snapshot floor     | `Snapshots.Verify` at backup time                    | a torn sealed checkpoint faults before cold storage      |
|  [07]   | objective gauge    | measured RPO/RTO `RecoveryFact`                      | a breach is a typed health signal, never SLA prose       |

## [03]-[POINT_IN_TIME_RESTORE]

- Owner: `RestoreStep` the `[SmartEnum<string>]` ordered choreography step carrying its rank; `StepFact` the per-step fact; `RestoreLedger` the per-run step sequence proving completeness; `RestoreContext` the restore inputs (the target `RecoveryPoint`, the target `Checkpoint` and the seal it chains from, the settled `RecoveryObjective` whose `Rto` bounds the projection catch-up wait, the document store, the `Fence`/`Materialize`/`ReplayTo` platform delegates, the `AttestedChain`/`KeyringFor`/`DigestOf` verify delegates); `PointInTimeRestore` the static surface owning the verified restore choreography that re-establishes content identity.
- Cases: `RestoreStep` is `Fence | Verify | Materialize | ReplayWal | RebuildProjections | ReAttest` in declared rank order — each step verifies before the next runs and the restore never best-efforts past a failed step; the run short-circuits on the first `Fin` failure and flushes the ledger so a half-restored store classifies unambiguously at the next open.
- Entry: `public static IO<(RestoreLedger Ledger, Fin<RecoveryPoint> Outcome)> Run(RecoveryRoute route, RecoveryContext ctx, RestoreContext restore, ProjectionContext frame)` composes the choreography step by step through one `FoldM`, each step emitting a `StepFact` and short-circuiting on the first failure; the outcome carries the reached `RecoveryPoint` on success.
- Auto: the choreography composes the `Element/codec#SNAPSHOT_SPINE` verify ladder in reverse and proves content identity at every gate — `Fence` clears the connection pool and quiesces writers, `Verify` runs `Snapshots.Verify` over EVERY sealed checkpoint's raw bytes (the ONE 8-tier ladder, before any decoder with attack surface binds) AND asserts the target `RecoveryPoint.Continues(ctx.ArchiveTimeline)` so a coordinate on a forked timeline faults `TimelineDivergence` rather than replaying onto a divergent history, `Materialize` restores the base, `ReplayWal` replays the WAL to the target `(Timeline, Lsn)` so the Marten event stream reaches the exact `RecoveryPoint`, `RebuildProjections` re-folds the inline authoritative `GraphProjection` for the restored model through `store.Advanced.RebuildSingleStreamAsync` (co-transactional, so NOT re-rebuilt through the daemon), brings up EVERY daemon-managed async analytical lane (`Query/columnar` DuckDB + `Query/cypher` AGE) through `daemon.StartAllAsync` so they catch up from their restored progress, then PROVES the rebuild reached the event head by awaiting the daemon `WaitForNonStaleData` caught-up gate (it blocks until every shard's high-water — inline and async lanes alike — reaches the head and throws on timeout) and stamps the `FetchEventStoreStatistics().EventSequenceNumber` head on the `StepFact` (a real completeness gate, not a string), and `ReAttest` re-folds the `Version/provenance#ATTESTED_LEDGER` `AttestedLedger.Verify` chain (the per-authorship `KeyringFor` resolver with the independent `DigestOf` content-digest recomputation, so `Unauthored` is reachable) AND folds the restored stream to the exact recovery version through `AggregateStreamAsync(version:)`, grading the reconstructed `ElementGraph` against the target `Checkpoint` through `Version/timetravel#TIME_TRAVEL` `TimeTravel.Verify` under `Require(CapabilitySet<SealLink>.All, …)` so the address, chain hash, back-link, and version monotonicity all re-fold and the missing links name themselves together — the two proofs absorb absence oppositely, the attested chain passing an unsigned ledger where the seal admits no absent link, so a restore that silently dropped or reordered events fails the attested chain and a tampered archive matching its own address fails the seal rather than serving a corrupted history; every step is a typed `StepFact` with the ledger flushed on failure.
- Output: the run `RestoreLedger` proves the restored content identity matches the target and `Complete` confirms every step ran; each `StepFact` fires the `rasm.persistence.recovery.replay` replay point (`Store/observability#HOOKS`) so a late panel drains the bounded recent choreography.
- Packages: Marten (`Advanced.RebuildSingleStreamAsync`/`FetchEventStoreStatistics`, `BuildProjectionDaemonAsync`, the `JasperFx.Events.Daemon.IProjectionDaemon` `StartAllAsync`/`WaitForNonStaleData`, `IQuerySession.Events.AggregateStreamAsync` by `version`/`timestamp`), Element/codec (`Snapshots.Verify`, `ContentAddress.OfGraph`), Version/provenance (`AttestedLedger.Verify`, `SigningKeyring`, `OpDigest`), Version/timetravel (`TimeTravel.Verify`, `Checkpoint`, `SealLink`), Rasm (`Rasm/Domain/validation#CAPABILITY` `CapabilitySet.All`/`Require`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox; WAL replay to the coordinate is the injected `ReplayTo` platform delegate, not a direct provider call here.
- Growth: a new restore step is one `RestoreStep` row breaking the choreography rank order; a new verify probe is one delegate on `RestoreContext`; zero new surface — a best-effort file copy, a verify-by-success that trusts the copy, a stringly lineage check standing in for `Snapshots.Verify`, or a projection rebuild skipped on restore is the deleted form because the choreography composes the ONE tier ladder in reverse, the projection rebuild is proven head-caught-up, and the commit point is a content-identity proof.
- Boundary: the restore composes the write protocol in reverse — one protocol vocabulary, one fact taxonomy, the only asymmetry who supplies the bytes; `Verify` runs the `Snapshots.Verify` tier ladder on raw bytes BEFORE any decoder with attack surface binds so a corrupted backup rejects before the codec machinery, and it asserts the timeline continuity because a base backup with WAL replay reaches the exact AS-OF state only on the timeline the archive continues; `ReplayWal` reaches the EXACT `RecoveryPoint` because the Marten event stream is the recovery substrate and WAL replay is deterministic to an LSN on a timeline, so a recovery point is a real version not an approximate copy; `RebuildProjections` is mandatory and PROVEN — the inline `GraphProjection` (co-transactional, re-folded by `RebuildSingleStreamAsync`) and the async analytical lanes (`Query/cypher` AGE, `Query/columnar#COLUMNAR_LANE` DuckDB, brought current by the daemon `StartAllAsync`, never a redundant second inline rebuild) are deterministic functions of the restored events, so a restore that skips the rebuild leaves stale views (the named defect) and a restore that rebuilds without proving the shard high-water reached the event head trusts an unproven rebuild; `ReAttest` re-folds the attested ledger (the `KeyringFor` KMS resolver with the independent `DigestOf` content-digest recomputation so `Unauthored` is reachable, not a self-compared stored digest) so a dropped, reordered, or content-unbinding entry fails the chain, and the AS-OF `AggregateStreamAsync` seal gate is the restorer's COMMIT POINT — the restored store is accepted only when its reconstructed `ElementGraph` satisfies EVERY `SealLink` of the target checkpoint, not the address link alone, everything before being repeatable garbage by construction; interactive-correctness reads after restore block on `WaitForNonStaleData` and never route to a still-rebuilding async lane.

```csharp

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StepEvidence {
    private StepEvidence() { }
    public sealed record Completed(string Detail) : StepEvidence;
    public sealed record Refused(Error Cause) : StepEvidence;
}

public readonly record struct StepFact(RestoreStep Step, StepEvidence Evidence, Instant At);

public readonly record struct RestoreLedger(Seq<StepFact> Steps) : IValidityEvidence {
    public bool Complete => Steps.Count == RestoreStep.Items.Count;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(Steps.Count, RestoreStep.Items.Count),
        ValidityClaim.CountExactly(Steps.Map(static s => s.Step).Distinct().Count, RestoreStep.Items.Count));
    public RestoreLedger With(StepFact fact) => new(Steps.Add(fact));
}

public sealed record RestoreContext(
    IDocumentStore Store, RecoveryPoint Target, Checkpoint TargetSeal, Option<Checkpoint> PriorSeal, ModelId Model, RecoveryObjective Objective,
    Func<RecoveryPoint, IO<Unit>> ReplayTo, Func<IO<Unit>> Fence, Func<IO<Unit>> Materialize,
    Func<IO<Seq<AttestedEntry>>> AttestedChain, Func<SignedAuthorship, SigningKeyring> KeyringFor, Func<AttestedEntry, OpDigest> DigestOf);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class PointInTimeRestore {
    public static IO<(RestoreLedger Ledger, Fin<RecoveryPoint> Outcome, Duration MeasuredRto)> Run(RecoveryRoute route, RecoveryContext ctx, RestoreContext restore, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from final in toSeq(RestoreStep.Items.OrderBy(static s => s.Rank)).FoldM(
            (Ledger: new RestoreLedger(Seq<StepFact>()), Outcome: Fin<RecoveryPoint>.Succ(restore.Target)),
            (state, step) => state.Outcome.IsFail
                ? IO.pure(state)
                : Perform(step, route, ctx, restore).Map(result => result.Match(
                    Succ: evidence => (state.Ledger.With(new StepFact(
                        step, new StepEvidence.Completed(evidence), frame.Now())), state.Outcome),
                    Fail: error => (state.Ledger.With(new StepFact(
                        step, new StepEvidence.Refused(error), frame.Now())), Fin<RecoveryPoint>.Fail(error)))))
        let rto = frame.Elapsed(mark)
        from _ in final.Outcome.IsSucc && rto > restore.Objective.Rto
            ? IO.fail<Unit>(new RecoveryFault.ObjectiveBreach(route.Key, rto, restore.Objective.Rto))
            : IO.pure(unit)
        select (final.Ledger, final.Outcome, rto);

    static IO<Fin<string>> Perform(RestoreStep step, RecoveryRoute route, RecoveryContext ctx, RestoreContext restore) =>
        step.Switch(
            state: (route, ctx, restore),
            fence: static s => s.restore.Fence().Map(static _ => Fin<string>.Succ("<writers-quiesced>")),
            verify: static s => IO.pure(Verify(s.route, s.ctx, s.restore.Target)),
            materialize: static s => s.restore.Materialize().Map(_ => Fin<string>.Succ($"<base-materialized:tl{s.ctx.ArchiveTimeline}>")),
            replayWal: static s => s.restore.ReplayTo(s.restore.Target).Map(static _ => Fin<string>.Succ($"<wal-replayed:lsn{s.restore.Target.Lsn:x}>")),
            rebuildProjections: static s => RebuildProjections(s.restore),
            reAttest: static s => ReAttest(s.restore));

    static Fin<string> Verify(RecoveryRoute route, RecoveryContext ctx, RecoveryPoint target) =>
        ctx.Checkpoints.Traverse(row =>
            Try.lift(() => Fin.Succ(RecoveryRoutes.ReadSealed(ctx.ArchiveRoot, row.Id))).Run().Bind(static inner => inner)
                .Bind(bytes => Snapshots.Verify(bytes, ctx.SchemaFingerprint, ctx.Epoch).Map(static _ => unit))
                .ToValidation()).As().ToFin()
        .Bind(verified => target.Continues(ctx.ArchiveTimeline)
            ? Fin<string>.Succ($"<verified:{verified.Count}-checkpoints:tl{target.Timeline}>")
            : Fin<string>.Fail(new RecoveryFault.TimelineDivergence(route.Key, target.Timeline, ctx.ArchiveTimeline)))
        .As();

    static IO<Fin<string>> RebuildProjections(RestoreContext restore) =>
        IO.liftAsync<Fin<string>>(async () => await Try.lift(async _ => {
            await restore.Store.Advanced.RebuildSingleStreamAsync<GraphProjection>(restore.Model.Value).ConfigureAwait(false);
            await using IProjectionDaemon daemon = await restore.Store.BuildProjectionDaemonAsync().ConfigureAwait(false);
            await daemon.StartAllAsync().ConfigureAwait(false);
            await daemon.WaitForNonStaleData(restore.Objective.Rto.ToTimeSpan()).ConfigureAwait(false);
            EventStoreStatistics stats = await restore.Store.Advanced.FetchEventStoreStatistics().ConfigureAwait(false);
            return Fin<string>.Succ($"<projections-rebuilt:head{stats.EventSequenceNumber}>");
        }).Run().Bind(static inner => inner).ConfigureAwait(false));

    static IO<Fin<string>> ReAttest(RestoreContext restore) =>
        from chain in restore.AttestedChain()
        from verdict in AttestedLedger.Verify(chain, restore.KeyringFor, restore.DigestOf)
        from outcome in verdict is AttestVerdict.Authentic or AttestVerdict.Unsigned
            ? IO.liftAsync<Fin<string>>(async () => await Try.lift(async _ => {
                await using IQuerySession query = restore.Store.QuerySession();
                    GraphProjection? rebuilt = await restore.Target.StreamVersion.Match(
                        Some: version => query.Events.AggregateStreamAsync<GraphProjection>(restore.Model.Value, version: version),
                        None: () => query.Events.AggregateStreamAsync<GraphProjection>(restore.Model.Value, timestamp: restore.Target.At.ToDateTimeOffset())).ConfigureAwait(false);
                    if (rebuilt is not { } projection) {
                        return Fin<string>.Fail(new RecoveryFault.RestoreFailed(new RestoreRefusal.TargetAbsent(restore.Model)));
                    }
                    ContentAddress reached = ContentAddress.OfGraph(projection.Graph);
                return TimeTravel.Verify(restore.TargetSeal, restore.PriorSeal, reached)
                    .Require(CapabilitySet<SealLink>.All, missing => new RecoveryFault.VerifyFailed(
                        $"re-attest:{string.Join('+', missing.Held.Select(static link => link.Key))}",
                        restore.TargetSeal.Address, reached))
                    .Map(_ => $"<chain-re-attested:{verdict.Key}:{reached.Value:x32}>");
            }).Run().Bind(static inner => inner).ConfigureAwait(false))
            : IO.pure(Fin<string>.Fail(new RecoveryFault.RestoreFailed(new RestoreRefusal.Attestation(verdict))))
        select outcome;
}
```

| [INDEX] | [POLICY]           | [VALUE]                                | [BINDING]                                                  |
| :-----: | :----------------- | :------------------------------------- | :--------------------------------------------------------- |
|  [01]   | step dispatch      | generated `RestoreStep.Switch`, ranked | exhaustive; a 7th step breaks the build                    |
|  [02]   | verify ladder      | `Snapshots.Verify` over raw bytes      | the ONE 8-tier ladder; never a stringly lineage check      |
|  [03]   | timeline guard     | `RecoveryPoint.Continues(archive)`     | a forked timeline faults; re-bootstrap is not resume       |
|  [04]   | recovery point     | WAL replay to `(Timeline, Lsn)`        | a real version, never an approximate timestamp             |
|  [05]   | projection rebuild | inline re-fold + async `StartAllAsync` | `WaitForNonStaleData` proves head-caught-up, `Rto`-bounded |
|  [06]   | commit point       | re-attest + `SealLink.All` seal gate   | `Unauthored`-reachable; every checkpoint link re-folds     |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
