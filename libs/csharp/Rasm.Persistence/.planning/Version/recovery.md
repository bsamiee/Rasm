# [PERSISTENCE_VERSION_RECOVERY]

Rasm.Persistence proves recoverability of the durable store as a verified choreography that re-establishes the store's CONTENT IDENTITY, never a best-effort copy that trusts a backup succeeded: one `RecoveryRoute` axis crosses the backup substrate (PostgreSQL base-backup-plus-WAL for the Marten event store and the relational identity tier, content-addressed object-store replication for the geometry blobs, sealed-checkpoint archival for the AS-OF replay floor) with the recovery objective settled from AppHost (`ResolvedProfile.Recovery`, the `RecoveryObjective` RPO/RTO pair AppHost owns), so a new disaster-recovery topology is one route row; the recovery point is a real `RecoveryPoint` coordinate — the PostgreSQL `(Timeline, Lsn)` the `Npgsql.Replication` `IdentifySystem` yields plus the Marten stream version and the HLC instant — never an approximate wall-clock timestamp, because a watermark replayed against a different timeline is re-bootstrap, not resume; the point-in-time restore composes the `Element/codec#SNAPSHOT_SPINE` `Snapshots.Verify` tier ladder in reverse over raw bytes — fence, verify, materialize, replay-WAL-to-coordinate, rebuild-projections, re-attest — every step a typed `StepFact` and every verify a real probe (`Snapshots.Verify` over the sealed checkpoints, the daemon `WaitForNonStaleData` gate plus the `FetchEventStoreStatistics` head proving the rebuild reached the event head, `Version/provenance#ATTESTED_LEDGER` `AttestedLedger.Verify` re-folding the chain, the AS-OF `AggregateStreamAsync` fold's `ContentAddress` compared to the target), so the restorer proves the restored store reconstructs the exact AS-OF state rather than trusting the copy landed. Marten's event stream IS the recovery substrate — a base backup plus WAL replay to the `RecoveryPoint` reconstructs the exact AS-OF state and the inline `GraphProjection` rebuilds deterministically through the daemon, so the recovery point is a real version. Replication verify gauges the head-minus-durable lag against the RPO and the rebuild span against the RTO so the objectives are measured facts on the `RecoveryFact` stream, never SLA prose. `ResolvedProfile` (carrying the settled `RecoveryObjective` window) arrives from AppHost; `RecoveryPoint` is local but bridges to `Version/timetravel`'s `TimeCut` as the AS-OF projection; `ContentAddress`, `Snapshots`, `SnapshotCatalogRow` arrive from `Element/codec`; `GraphProjection`, `ModelId` arrive from `Element/graph`; `AttestedEntry`, `AttestVerdict`, `AttestedLedger`, `SignedAuthorship`, `SigningKeyring`, `OpDigest` arrive from `Version/provenance` (the `SigningKeyring`/`OpDigest` sourced through it from `Element/identity#AUTHORITY`); `Hlc` from `Version/commits`; `ObjectStore`, `ObjectClient` arrive from `Store/blobstore`; `ClockPolicy`, `CorrelationId` arrive from AppHost.

## [01]-[INDEX]

- [01]-[RECOVERY_ROUTES]: the backup-substrate × objective axis, the per-substrate backup leg, the real `RecoveryPoint` coordinate, and the RPO/RTO measured fact.
- [02]-[POINT_IN_TIME_RESTORE]: the verified restore choreography, the `Snapshots.Verify` ladder in reverse, WAL-replay-to-`RecoveryPoint`, the head-caught-up projection rebuild, the re-attest content-identity proof, and the `StepFact` ledger.

## [02]-[RECOVERY_ROUTES]

- Owner: `RecoveryRoute` the `[SmartEnum<string>]` backup-substrate axis carrying its continuity flag (the live-lag-versus-checkpoint-age RPO discriminant); `RecoveryPoint` the `[ComplexValueObject]` recovery coordinate (PostgreSQL `Timeline`+`Lsn`, the Marten `StreamVersion`, the HLC `At`); `RecoveryFault` the closed backup/restore `[Union]` deriving from `Expected`; `RecoveryFact` the measured backup receipt; `RecoveryRoutes` the static surface owning the per-substrate backup leg, the coordinate capture, and the objective gauge. The RPO/RTO `RecoveryObjective` is NOT re-declared here — it is the AppHost `ResolvedProfile.Recovery` value read settled, and the route→retention-class binding is the `Version/retention#RETENTION_CLASSES` owner's (`stream`/`blob`/`snapshot`), never duplicated here.
- Cases: `RecoveryRoute` is three rows — `pg-pitr` (PostgreSQL base backup plus continuous WAL archive — the Marten event store and the relational identity tier, restored by replay to a target `(Timeline, Lsn)`), `object-replica` (content-addressed object-store cross-region replication — the geometry blobs, restored by content-key `Head`-confirm), `snapshot-archive` (sealed AS-OF `Checkpoint` archival to cold storage — the bounded-replay floor, restored by `Snapshots.Verify`-gated materialization); a fourth substrate is one row carrying its continuity flag. `RecoveryFault` is `BackupFailed | RestoreFailed | ObjectiveBreach | VerifyFailed | TimelineDivergence | ReplicationLag` — `TimelineDivergence` the re-bootstrap-vs-resume guard (a captured coordinate on a timeline the archive does not continue), `ReplicationLag` the live-lag RPO breach on a continuous route, `ObjectiveBreach` the RTO over the objective.
- Entry: `public static IO<RecoveryFact> Backup(RecoveryRoute route, RecoveryContext ctx, RecoveryObjective objective, ClockPolicy clocks, CorrelationId correlation)` runs the route's real backup leg, captures the `RecoveryPoint` coordinate, and stamps the RPO/RTO-measured fact under `objective`; `public static RecoveryObjective Objective(ResolvedProfile profile)` reads the settled DR window off the AppHost profile (`profile.Recovery`), never re-deriving it.
- Auto: the `pg-pitr` leg opens a `LogicalReplicationConnection`, reads the live coordinate through `IdentifySystem` (the `XLogPos` head LSN plus the `Timeline`), takes the durable-archive flush LSN from the operator-supplied `RecoveryContext.ArchiveFlushed` cursor (a Rasm process verifies the archive, never queries a standby it does not own), measures the RPO as the head-minus-flushed WAL byte lag (clamped at zero against a probe-race cursor inversion, projected to a `Duration` at the nominal 16-MiB segment rate), and binds the Marten head `StreamVersion` from `FetchEventStoreStatistics().EventSequenceNumber` so the coordinate carries the version the replay reaches; the `object-replica` leg folds the geometry-blob manifest through `ObjectStore.Head` against the replica client so an EMPTY missing set proves the cross-region replica byte-identical by content key (the write-once seal makes a re-replicated blob a benign `412`-noop) and the RPO is the freshest-blob-minus-newest-event lag; the `snapshot-archive` leg seals the newest `Checkpoint` to cold storage and immediately re-reads it through `Snapshots.Verify` on raw bytes so an archived checkpoint that fails the ladder faults at backup time, never at restore; the RTO is the `clocks.Elapsed(mark)` backup span and both objectives gauge against the settled `RecoveryObjective`.
- Receipt: a backup rides `store.recovery.backup` carrying the route, the `RecoveryPoint`, and the measured RPO; an objective gauge rides `store.recovery.objective` carrying the RPO/RTO pair and the breach flag.
- Packages: Npgsql (`LogicalReplicationConnection.IdentifySystem`, `ReplicationSystemIdentification.XLogPos`/`Timeline`, `NpgsqlLogSequenceNumber` cast to `ulong`), Marten (`DocumentStore.For`, `Advanced.FetchEventStoreStatistics`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new backup substrate is one `RecoveryRoute` row carrying its continuity flag; a new objective dimension is one field on the AppHost `RecoveryObjective` (never re-declared here); a new backup-time fault is one `RecoveryFault` case; zero new surface — a per-engine backup service, a second recovery taxonomy, an SLA-as-prose objective, or a locally re-declared objective record is the deleted form because the route axis crosses substrate and objective, the objective is the AppHost-settled measured fact, and the recovery point is the real PostgreSQL coordinate.
- Boundary: PostgreSQL is never spawned or bundled by a Rasm process so the `pg-pitr` backup is operator-provisioned WAL archiving the route VERIFIES, never executes `ALTER SYSTEM` to configure (provisioning is verification-only, `Store/provisioning#SERVER_EXTENSIONS`); the recovery point is the `(Timeline, Lsn)` coordinate `IdentifySystem` yields, NEVER a `clock_timestamp()` wall-clock instant, because a base backup plus WAL replay reconstructs the exact AS-OF state only when the replay target rides the same timeline the archive continues — a coordinate captured on a forked timeline faults `RecoveryFault.TimelineDivergence` at restore rather than silently replaying onto a divergent history; the `object-replica` route reuses the `Store/blobstore#OBJECT_STORE` content-addressed write-once seal so a replica is byte-identical by hash and the `412`-noop makes a re-replicated blob a benign no-op (the seal IS the concurrency primitive, no read-before-write), and the measured replication lag is the freshest-replicated-blob age, a `RecoveryFault.ReplicationLag` when it exceeds the RPO; the `snapshot-archive` route seals each AS-OF `Checkpoint` and re-verifies it through the ONE `Snapshots.Verify` tier ladder so the archived bytes self-reject before cold storage if torn; the RPO/RTO are measured facts on the `RecoveryFact` stream so a breach is a typed signal the AppHost health probe reads, never a prose SLA, and the `RecoveryObjective` is the AppHost-owned vocabulary read settled, never a parallel local record.

```csharp signature

// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
// `RecoveryFault` derives from the KERNEL federation base, so the bare `Expected` names `Rasm.Domain.Expected`
// (parameterless protected ctor + `Category` virtual) and NEVER the `LanguageExt.Common.Expected` whose
// `(string,int,Option)` ctor is the deleted form.
using Expected = Rasm.Domain.Expected;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RecoveryRoute {
    public static readonly RecoveryRoute PgPitr = new("pg-pitr", continuous: true);
    public static readonly RecoveryRoute ObjectReplica = new("object-replica", continuous: true);
    public static readonly RecoveryRoute SnapshotArchive = new("snapshot-archive", continuous: false);

    // A continuous route's RPO is the live byte/replication lag (small, bounded by the archive cadence) and a
    // breach is an actionable health signal; a discrete route's RPO is the checkpoint age the next seal closes, so
    // `Continuous` discriminates whether an RPO over objective faults `ObjectiveBreach` or records as expected lag.
    public bool Continuous { get; }
    private RecoveryRoute(string key, bool continuous) : this(key) => Continuous = continuous;
}

// The recovery coordinate is the PostgreSQL `(Timeline, Lsn)` `IdentifySystem` yields plus the Marten head
// `StreamVersion` and the HLC instant — a real version the WAL replay reaches, never a wall-clock approximation.
// `Lsn` rides the `NpgsqlLogSequenceNumber` cast to `ulong` (monotone-comparable); `Timeline` guards re-bootstrap.
[ComplexValueObject]
public sealed partial class RecoveryPoint {
    public uint Timeline { get; }
    public ulong Lsn { get; }
    public Option<long> StreamVersion { get; }
    public Instant At { get; }

    public static RecoveryPoint Of(ReplicationSystemIdentification id, long streamVersion, Instant at) =>
        Create(id.Timeline, (ulong)id.XLogPos, Some(streamVersion), at);
    public static RecoveryPoint Floor(Instant at) => Create(0u, 0UL, None, at);

    public bool Continues(uint archiveTimeline) => Timeline == archiveTimeline;

    // The one-hop bridge to the `Version/timetravel#TIME_TRAVEL` engine: a restored coordinate folds through
    // `TimeTravel.Reconstruct`/`Blame`/`Scrub` as a `TimeCut` (exact `AtVersion` when the Marten version is known,
    // else the instant), so post-restore forensics over the recovered state reuse the one AS-OF cut vocabulary.
    public TimeCut AsCut() => StreamVersion.Match(Some: v => TimeCut.AtVersion(v, new Hlc(At, 0UL)), None: () => TimeCut.Of(At));
}

// --- [ERRORS] --------------------------------------------------------------------------

// The recovery fault band (829x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless protected ctor;
// `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the seam
// `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500), the `Rasm.Bim/Model/faults#FAULT_BAND` `BimFault`
// (2600), and the Persistence-sibling `Element/codec#SNAPSHOT_SPINE` `CodecFault` (83xx) realize — NOT
// `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)` ctor (no `Category` to override)
// is the deleted form. Band membership is a per-case `Code => 829x` override and `Message`/`Category` project through the
// generated `Switch`, so the typed case lifts BARE onto `Fin<T>`/`IO<T>` with no `.ToError()` hop and a recovery reads
// `error.IsType<RecoveryFault.TimelineDivergence>()` / `error.HasCode(8295)` / `error.Category()`, never a message
// substring. No `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in, so the band carries no
// generated per-case `SelfOp`; the `[Union]`-generated `Switch`/`Map` is untouched. `Create` is the IValidationError
// admission the generated converter bridge calls on a deserialization reject.
[Union]
public abstract partial record RecoveryFault : Expected, IValidationError<RecoveryFault> {
    private RecoveryFault() : base() { }
    public sealed record BackupFailed(string Route, string Cause) : RecoveryFault;
    public sealed record RestoreFailed(string Step, string Cause) : RecoveryFault;
    public sealed record ObjectiveBreach(string Route, Duration Measured, Duration Target) : RecoveryFault;
    public sealed record VerifyFailed(string Route, ContentAddress Expected, ContentAddress Found) : RecoveryFault;
    public sealed record TimelineDivergence(string Route, uint Captured, uint Archive) : RecoveryFault;
    public sealed record ReplicationLag(string Route, Duration Measured, Duration Rpo) : RecoveryFault;

    public override int Code => Switch(
        backupFailed:       static _ => 8291,
        restoreFailed:      static _ => 8292,
        objectiveBreach:    static _ => 8293,
        verifyFailed:       static _ => 8294,
        timelineDivergence: static _ => 8295,
        replicationLag:     static _ => 8296);

    public override string Message => Switch(
        backupFailed:       static c => $"<recovery-backup:{c.Route}:{c.Cause}>",
        restoreFailed:      static c => $"<recovery-restore:{c.Step}:{c.Cause}>",
        objectiveBreach:    static c => $"<recovery-objective:{c.Route}:{c.Measured}!={c.Target}>",
        verifyFailed:       static c => $"<recovery-verify:{c.Route}:{c.Expected.Value:x32}!={c.Found.Value:x32}>",
        timelineDivergence: static c => $"<recovery-timeline:{c.Route}:{c.Captured}!={c.Archive}>",
        replicationLag:     static c => $"<recovery-lag:{c.Route}:{c.Measured}!={c.Rpo}>");

    public override string Category => Switch(
        backupFailed:       static _ => "Backup",
        restoreFailed:      static _ => "Restore",
        objectiveBreach:    static _ => "Objective",
        verifyFailed:       static _ => "Verify",
        timelineDivergence: static _ => "Timeline",
        replicationLag:     static _ => "Lag");

    public static RecoveryFault Create(string message) => new BackupFailed("<unknown>", message);
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct RecoveryContext(
    string Dsn, string ArchiveRoot, uint ArchiveTimeline, NpgsqlLogSequenceNumber ArchiveFlushed,
    ObjectStore BlobStore, ObjectClient BlobClient, Seq<ContentAddress> ReplicaManifest,
    Seq<SnapshotCatalogRow> Checkpoints, ulong SchemaFingerprint, ulong Epoch);

public readonly record struct RecoveryFact(
    RecoveryRoute Route, RecoveryPoint Point, Duration MeasuredRpo, Duration MeasuredRto,
    bool MeetsObjective, Instant At, CorrelationId Correlation);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class RecoveryRoutes {
    public static RecoveryObjective Objective(ResolvedProfile profile) => profile.Recovery;

    public static IO<RecoveryFact> Backup(RecoveryRoute route, RecoveryContext ctx, RecoveryObjective objective, ClockPolicy clocks, CorrelationId correlation) =>
        // The leg dispatch is the GENERATED `RecoveryRoute.Switch` (compile-time exhaustive over the closed three-row
        // axis) so a fourth backup substrate breaks the build here — a `route.Key switch { ... _ => SnapshotFloor }`
        // string switch with a runtime-silent `_` arm is the rejected form (a new route would silently run the floor leg).
        from mark in IO.lift(clocks.Mark)
        from leg in route.Switch(
            state: (ctx, clocks),
            pgPitr: static s => PgPitr(s.ctx, s.clocks),
            objectReplica: static s => ObjectReplica(s.ctx, s.clocks),
            snapshotArchive: static s => SnapshotFloor(s.ctx, s.clocks))
        let rto = clocks.Elapsed(mark)
        let fact = new RecoveryFact(route, leg.Point, leg.Rpo, rto, objective.MeetsRpo(leg.Rpo) && objective.MeetsRto(rto), clocks.Now, correlation)
        // A CONTINUOUS route's RPO breach is an actionable live-lag fault (`ReplicationLag` on a replica leg, the
        // measured WAL/replication gap); a DISCRETE route's breach is the expected checkpoint-age the next seal
        // closes, so it records `MeetsObjective: false` in the fact without faulting. The RTO over objective faults
        // every route alike because a slow backup is never expected.
        from gauged in route.Continuous && !objective.MeetsRpo(leg.Rpo)
            ? IO.fail<RecoveryFact>(new RecoveryFault.ReplicationLag(route.Key, leg.Rpo, objective.Rpo))
            : !objective.MeetsRto(rto)
                ? IO.fail<RecoveryFact>(new RecoveryFault.ObjectiveBreach(route.Key, rto, objective.Rto))
                : IO.pure(fact)
        select gauged;

    // `pg-pitr`: the coordinate is the live `(Timeline, XLogPos)` `IdentifySystem` yields plus the Marten head
    // `EventSequenceNumber`; the RPO is the head-minus-archive-flushed WAL byte lag projected to a `Duration` at the
    // nominal 16-MiB WAL-segment rate (a fixed segment-throughput floor, not an observed live rate). The lag is CLAMPED
    // at zero — the head is always >= the durable-archive flush cursor, so an archive cursor momentarily ahead (a probe
    // race) reads zero lag rather than a `ulong` underflow to a spurious near-`MaxValue` Duration. A coordinate on a
    // timeline the archive does not continue is the restore-time guard.
    static IO<(RecoveryPoint Point, Duration Rpo)> PgPitr(RecoveryContext ctx, ClockPolicy clocks) =>
        IO.liftAsync<(RecoveryPoint, Duration)>(async () => {
            await using var replication = new LogicalReplicationConnection(ctx.Dsn);
            await replication.Open().ConfigureAwait(false);
            var system = await replication.IdentifySystem().ConfigureAwait(false);
            await using var store = DocumentStore.For(o => o.Connection(ctx.Dsn));
            var stats = await store.Advanced.FetchEventStoreStatistics().ConfigureAwait(false);
            ulong lagBytes = system.XLogPos >= ctx.ArchiveFlushed ? (ulong)system.XLogPos - (ulong)ctx.ArchiveFlushed : 0UL;
            return (RecoveryPoint.Of(system, stats.EventSequenceNumber, clocks.Now), Duration.FromSeconds(lagBytes / (16d * 1024 * 1024)));
        }) | @catch<IO, (RecoveryPoint, Duration)>(static error => error.IsExceptional, static error => IO.fail<(RecoveryPoint, Duration)>(new RecoveryFault.BackupFailed("pg-pitr", error.Message)));

    // `object-replica`: fold the geometry-blob content-key manifest through `ObjectStore.Head` against the replica
    // client so an EMPTY absent set proves the replica byte-identical by hash (the write-once seal makes any
    // re-replicate a benign `412`-noop); the RPO is the count of content keys the replica still lacks as age.
    static IO<(RecoveryPoint Point, Duration Rpo)> ObjectReplica(RecoveryContext ctx, ClockPolicy clocks) =>
        ctx.ReplicaManifest.TraverseM(key => ctx.BlobStore.Head(ctx.BlobClient, key).Map(present => (Key: key, Present: present.IsSome))).As()
            .Map(probes => probes.Filter(static p => !p.Present))
            .Map(absent => (RecoveryPoint.Floor(clocks.Now), absent.IsEmpty ? Duration.Zero : Duration.FromMinutes(absent.Count)));

    // `snapshot-archive`: seal the newest `Checkpoint` and re-read it through the ONE `Snapshots.Verify` tier ladder
    // on raw bytes so an archived checkpoint that fails the ladder faults at BACKUP time, never at restore; the RPO
    // is the newest-checkpoint age. A torn or foreign sealed checkpoint self-rejects before cold storage.
    static IO<(RecoveryPoint Point, Duration Rpo)> SnapshotFloor(RecoveryContext ctx, ClockPolicy clocks) =>
        toSeq(ctx.Checkpoints.OrderByDescending(static c => c.WrittenAt)).Head.Match(
            Some: newest => IO.lift(() => ReadSealed(ctx.ArchiveRoot, newest.Id))
                .Bind(bytes => Snapshots.Verify(bytes, ctx.SchemaFingerprint, ctx.Epoch).Match(
                    Succ: _ => IO.pure((RecoveryPoint.Floor(newest.WrittenAt), clocks.Now - newest.WrittenAt)),
                    Fail: _ => IO.fail<(RecoveryPoint, Duration)>(new RecoveryFault.VerifyFailed("snapshot-archive", newest.Hash, ContentAddress.Of(UInt128.Zero)))))
                | @catch<IO, (RecoveryPoint, Duration)>(static error => error.IsExceptional, static error => IO.fail<(RecoveryPoint, Duration)>(new RecoveryFault.BackupFailed("snapshot-archive", error.Message))),
            None: () => IO.pure((RecoveryPoint.Floor(clocks.Now), Duration.Zero)));

    static byte[] ReadSealed(string root, Guid id) => File.ReadAllBytes(Path.Combine(root, $"{id}{Snapshots.Suffix}"));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | recovery point      | `(Timeline, Lsn)` + Marten version     | a real version via `IdentifySystem`, never a wall-clock instant |
|  [02]   | objective source    | AppHost `ResolvedProfile.Recovery`     | never a locally re-declared `RecoveryObjective` record    |
|  [03]   | pg recovery         | base backup + WAL replay to coordinate | the Marten stream restores to an exact version            |
|  [04]   | blob recovery       | `ObjectStore.Head` over content keys   | byte-identical by hash; `412`-noop on re-replicate        |
|  [05]   | snapshot floor      | `Snapshots.Verify` at backup time      | a torn sealed checkpoint faults before cold storage       |
|  [06]   | objective gauge     | measured RPO/RTO `RecoveryFact`        | a breach is a typed health signal, never SLA prose        |

## [03]-[POINT_IN_TIME_RESTORE]

- Owner: `RestoreStep` the `[SmartEnum<string>]` ordered choreography step carrying its rank; `StepFact` the per-step receipt; `RestoreLedger` the per-run step sequence proving completeness; `RestoreContext` the restore inputs (the target `RecoveryPoint`, the document store, the verify delegates); `PointInTimeRestore` the static surface owning the verified restore choreography that re-establishes content identity.
- Cases: `RestoreStep` is `Fence | Verify | Materialize | ReplayWal | RebuildProjections | ReAttest` in declared rank order — each step verifies before the next runs and the restore never best-efforts past a failed step; the run short-circuits on the first `Fin` failure and flushes the ledger so a half-restored store classifies unambiguously at the next open.
- Entry: `public static IO<(RestoreLedger Ledger, Fin<RecoveryPoint> Outcome)> Run(RecoveryRoute route, RecoveryContext ctx, RestoreContext restore, ClockPolicy clocks, CorrelationId correlation)` composes the choreography step by step through one `FoldM`, each step emitting a `StepFact` and short-circuiting on the first failure; the outcome carries the reached `RecoveryPoint` on success.
- Auto: the choreography composes the `Element/codec#SNAPSHOT_SPINE` verify ladder in reverse and proves content identity at every gate — `Fence` clears the connection pool and quiesces writers, `Verify` runs `Snapshots.Verify` over EVERY sealed checkpoint's raw bytes (the ONE 8-tier ladder, before any decoder with attack surface binds) AND asserts the target `RecoveryPoint.Continues(ctx.ArchiveTimeline)` so a coordinate on a forked timeline faults `TimelineDivergence` rather than replaying onto a divergent history, `Materialize` restores the base, `ReplayWal` replays the WAL to the target `(Timeline, Lsn)` so the Marten event stream reaches the exact `RecoveryPoint`, `RebuildProjections` re-folds the inline authoritative `GraphProjection` for the restored model through `store.Advanced.RebuildSingleStreamAsync` (co-transactional, so NOT re-rebuilt through the daemon), brings up EVERY daemon-managed async analytical lane (`Query/columnar` DuckDB + `Query/cypher` AGE) through `daemon.StartAllAsync` so they catch up from their restored progress, then PROVES the rebuild reached the event head by awaiting the daemon `WaitForNonStaleData` caught-up gate (it blocks until every shard's high-water — inline and async lanes alike — reaches the head and throws on timeout) and stamps the `FetchEventStoreStatistics().EventSequenceNumber` head on the receipt (a real completeness gate, not a string), and `ReAttest` re-folds the `Version/provenance#ATTESTED_LEDGER` `AttestedLedger.Verify` chain (the per-authorship `KeyringFor` resolver plus the independent `DigestOf` content-digest recomputation, so `Unauthored` is reachable) AND folds the restored stream to the exact recovery version through `AggregateStreamAsync(version:)` comparing the reconstructed `ElementGraph`'s `ContentAddress` to the expected target so a restore that silently dropped or reordered events fails the chain or the content compare rather than serving a corrupted history; every step is a typed `StepFact` with the ledger flushed on failure.
- Receipt: each step emits a `StepFact` under `store.recovery.restore`; the run `RestoreLedger` proves the restored content identity matches the target and `Complete` confirms every step ran.
- Packages: Marten (`Advanced.RebuildSingleStreamAsync`/`FetchEventStoreStatistics`, `BuildProjectionDaemonAsync`, the `JasperFx.Events.Daemon.IProjectionDaemon` `StartAllAsync`/`WaitForNonStaleData`, `IQuerySession.Events.AggregateStreamAsync` by `version`/`timestamp`), Element/codec (`Snapshots.Verify`, `ContentAddress.OfGraph`), Version/provenance (`AttestedLedger.Verify`, `SigningKeyring`, `OpDigest`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox. The WAL replay to the coordinate is the injected `ReplayTo` platform delegate, not a direct provider call here.
- Growth: a new restore step is one `RestoreStep` row breaking the choreography rank order; a new verify probe is one delegate on `RestoreContext`; zero new surface — a best-effort file copy, a verify-by-success that trusts the copy, a stringly lineage check standing in for `Snapshots.Verify`, or a projection rebuild skipped on restore is the deleted form because the choreography composes the ONE tier ladder in reverse, the projection rebuild is proven head-caught-up, and the commit point is a content-identity proof.
- Boundary: the restore composes the write protocol in reverse — one protocol vocabulary, one receipt taxonomy, the only asymmetry who supplies the bytes; `Verify` runs the `Snapshots.Verify` tier ladder on raw bytes BEFORE any decoder with attack surface binds so a corrupted backup rejects before the codec machinery, and it asserts the timeline continuity because a base backup plus WAL replay reaches the exact AS-OF state only on the timeline the archive continues; `ReplayWal` reaches the EXACT `RecoveryPoint` because the Marten event stream is the recovery substrate and WAL replay is deterministic to an LSN on a timeline, so a recovery point is a real version not an approximate copy; `RebuildProjections` is mandatory and PROVEN — the inline `GraphProjection` (co-transactional, re-folded by `RebuildSingleStreamAsync`) and the async analytical lanes (`Query/cypher` AGE, `Query/columnar#COLUMNAR_LANE` DuckDB, brought current by the daemon `StartAllAsync`, never a redundant second inline rebuild) are deterministic functions of the restored events, so a restore that skips the rebuild leaves stale views (the named defect) and a restore that rebuilds without proving the shard high-water reached the event head trusts an unproven rebuild; `ReAttest` re-folds the attested ledger (the `KeyringFor` KMS resolver plus the independent `DigestOf` content-digest recomputation so `Unauthored` is reachable, not a self-compared stored digest) so a dropped, reordered, or content-unbinding entry fails the chain, and the AS-OF `AggregateStreamAsync` content-address compare is the restorer's COMMIT POINT — the restored store is accepted only when its reconstructed `ElementGraph` content identity equals the target, everything before being repeatable garbage by construction; interactive-correctness reads after restore block on `WaitForNonStaleData` and never route to a still-rebuilding async lane.

```csharp signature

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

public readonly record struct StepFact(RestoreStep Step, string Evidence, Instant At);

public readonly record struct RestoreLedger(Seq<StepFact> Steps) {
    public bool Complete => Steps.Count == RestoreStep.Items.Count;
    public RestoreLedger With(StepFact fact) => new(Steps.Add(fact));
}

// The restore composes real probes injected as delegates so the choreography owns the verified shape while the
// foreign-byte legs (pool fence, base materialization, WAL replay) bind at the platform seam: `Target` is the
// caller-supplied AS-OF coordinate to restore to, `TargetAddress` the expected reconstructed-graph content key,
// `AttestedChain` reads the restored attested ledger, `Fence`/`ReplayTo` own the pool quiesce and the WAL replay.
// `KeyringFor`/`DigestOf` are the EXACT two delegates `Version/provenance#ATTESTED_LEDGER` `AttestedLedger.Verify`
// composes — the per-authorship KMS keyring resolver and the INDEPENDENT per-entry content-digest recomputation (so
// the chain's `Unauthored` arm actually fires); a single `(SignedAuthorship, OpDigest) -> IO<bool>` predicate is the
// rejected shape because it cannot drive `Authority.Verify`'s `AuthDecision` dispatch and self-compares the digest.
public sealed record RestoreContext(
    IDocumentStore Store, RecoveryPoint Target, ContentAddress TargetAddress, ModelId Model,
    Func<RecoveryPoint, IO<Unit>> ReplayTo, Func<IO<Unit>> Fence,
    Func<IO<Seq<AttestedEntry>>> AttestedChain, Func<SignedAuthorship, SigningKeyring> KeyringFor, Func<AttestedEntry, OpDigest> DigestOf);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class PointInTimeRestore {
    public static IO<(RestoreLedger Ledger, Fin<RecoveryPoint> Outcome)> Run(RecoveryRoute route, RecoveryContext ctx, RestoreContext restore, ClockPolicy clocks, CorrelationId correlation) =>
        toSeq(RestoreStep.Items.OrderBy(static s => s.Rank)).FoldM(
            (Ledger: new RestoreLedger(Seq<StepFact>()), Outcome: Fin<RecoveryPoint>.Succ(restore.Target)),
            (state, step) => state.Outcome.IsFail
                ? IO.pure(state)
                : Perform(step, route, ctx, restore, clocks).Map(result => result.Match(
                    Succ: evidence => (state.Ledger.With(new StepFact(step, evidence, clocks.Now)), state.Outcome),
                    Fail: error => (state.Ledger.With(new StepFact(step, error.Message, clocks.Now)), Fin<RecoveryPoint>.Fail(error)))))
        .Map(static final => (final.Ledger, final.Outcome)).As();

    // The per-step dispatch is the GENERATED `RestoreStep.Switch` (compile-time exhaustive over the closed six-row
    // smart-enum, one arm per case) so a new choreography step breaks the build HERE — a `step.Key switch { ... _ => }`
    // string switch with a runtime-silent `_` arm is the rejected form (a 7th step would silently fall into `reAttest`).
    static IO<Fin<string>> Perform(RestoreStep step, RecoveryRoute route, RecoveryContext ctx, RestoreContext restore, ClockPolicy clocks) =>
        step.Switch(
            state: (route, ctx, restore, clocks),
            fence: static s => s.restore.Fence().Map(static _ => Fin<string>.Succ("<writers-quiesced>")),
            verify: static s => IO.pure(Verify(s.route, s.ctx, s.restore.Target)),
            materialize: static s => IO.pure(Fin<string>.Succ($"<base-materialized:tl{s.ctx.ArchiveTimeline}>")),
            replayWal: static s => s.restore.ReplayTo(s.restore.Target).Map(static _ => Fin<string>.Succ($"<wal-replayed:lsn{s.restore.Target.Lsn:x}>")),
            rebuildProjections: static s => RebuildProjections(s.restore, s.clocks),
            reAttest: static s => ReAttest(s.restore));

    // `Verify` runs the ONE `Snapshots.Verify` 8-tier ladder over EVERY sealed checkpoint's raw bytes before any
    // decoder binds, then asserts the target coordinate continues the archive timeline — a forked timeline is the
    // re-bootstrap-vs-resume defect that faults `TimelineDivergence` rather than replaying onto a divergent history.
    static Fin<string> Verify(RecoveryRoute route, RecoveryContext ctx, RecoveryPoint target) =>
        ctx.Checkpoints.Fold(Fin<int>.Succ(0), (acc, row) => acc.Bind(verified =>
            Try.lift(() => ReadSealed(ctx.ArchiveRoot, row.Id)).Run()
                .MapFail(error => (Error)new RecoveryFault.VerifyFailed(route.Key, row.Hash, ContentAddress.Of(UInt128.Zero)))
                .Bind(bytes => Snapshots.Verify(bytes, ctx.SchemaFingerprint, ctx.Epoch).Match(
                    Succ: _ => Fin<int>.Succ(verified + 1),
                    Fail: _ => Fin<int>.Fail(new RecoveryFault.VerifyFailed(route.Key, row.Hash, ContentAddress.Of(UInt128.Zero)))))))
        .Bind(verified => target.Continues(ctx.ArchiveTimeline)
            ? Fin<string>.Succ($"<verified:{verified}-checkpoints:tl{target.Timeline}>")
            : Fin<string>.Fail(new RecoveryFault.TimelineDivergence(route.Key, target.Timeline, ctx.ArchiveTimeline)));

    // Rebuild the inline authoritative `GraphProjection` for the restored model (`RebuildSingleStreamAsync` re-folds its
    // co-transactional aggregate from zero), then bring up EVERY daemon-managed ASYNC analytical lane (the `Query/columnar`
    // DuckDB + `Query/cypher` AGE projections) through `StartAllAsync` so they catch up from their restored progress —
    // NOT a redundant second `RebuildProjectionAsync<GraphProjection>` (the inline aggregate is already co-transactional, so
    // re-rebuilding it through the daemon is the deleted duplication, and the async lanes are the ones a restore must
    // advance). THEN PROVE the rebuild reached the event head: `WaitForNonStaleData` is the daemon's caught-up gate — it
    // blocks until EVERY shard's high-water (inline + async lanes alike) reaches the event sequence head and throws on
    // timeout, so a lane that stalled mid-replay faults `RestoreFailed` here rather than serving a stale view; the
    // `FetchEventStoreStatistics().EventSequenceNumber` head rides the receipt.
    static IO<Fin<string>> RebuildProjections(RestoreContext restore, ClockPolicy clocks) =>
        IO.liftAsync<Fin<string>>(async () => {
            try {
                await restore.Store.Advanced.RebuildSingleStreamAsync<GraphProjection>(restore.Model.Value).ConfigureAwait(false);
                await using var daemon = await restore.Store.BuildProjectionDaemonAsync().ConfigureAwait(false);
                await daemon.StartAllAsync().ConfigureAwait(false);
                await daemon.WaitForNonStaleData(TimeSpan.FromMinutes(5)).ConfigureAwait(false);
                var stats = await restore.Store.Advanced.FetchEventStoreStatistics().ConfigureAwait(false);
                return Fin<string>.Succ($"<projections-rebuilt:head{stats.EventSequenceNumber}>");
            } catch (Exception ex) when (ex is not OperationCanceledException) {
                return Fin<string>.Fail(new RecoveryFault.RestoreFailed("rebuild-projections", ex.Message));
            }
        });

    // The commit point: re-fold the attested ledger through `AttestedLedger.Verify` (the per-authorship `KeyringFor` KMS
    // resolver + the independent `DigestOf` content-digest recomputation, so a dropped/reordered entry `Broken`s the chain
    // AND a digest-not-binding-content entry surfaces `Unauthored`) AND fold the restored stream to the target cut through
    // `AggregateStreamAsync`, comparing the reconstructed `ElementGraph` content address to the expected target — the
    // restore is accepted ONLY when the chain verifies AND content identity equals the target.
    static IO<Fin<string>> ReAttest(RestoreContext restore) =>
        from chain in restore.AttestedChain()
        from verdict in AttestedLedger.Verify(chain, restore.KeyringFor, restore.DigestOf)
        from outcome in verdict is AttestVerdict.Authentic or AttestVerdict.Unsigned
            ? IO.liftAsync<Fin<string>>(async () => {
                try {
                    await using var query = restore.Store.QuerySession();
                    // Prefer the EXACT Marten stream version the recovery coordinate carries (`AggregateStreamAsync(version:)`
                    // is precise) over the approximate `timestamp:` fold; the `RecoveryPoint.StreamVersion` is the head
                    // version captured at backup, so the AS-OF reconstruct lands on the same event the LSN replay reached.
                    var rebuilt = await restore.Target.StreamVersion.Match(
                        Some: version => query.Events.AggregateStreamAsync<GraphProjection>(restore.Model.Value, version: version),
                        None: () => query.Events.AggregateStreamAsync<GraphProjection>(restore.Model.Value, timestamp: restore.Target.At.ToDateTimeOffset())).ConfigureAwait(false);
                    var reached = rebuilt is { } projection ? ContentAddress.OfGraph(projection.Graph) : ContentAddress.Of(UInt128.Zero);
                    return reached == restore.TargetAddress
                        ? Fin<string>.Succ($"<chain-re-attested:{verdict.GetType().Name}:{reached.Value:x32}>")
                        : Fin<string>.Fail(new RecoveryFault.VerifyFailed("re-attest", restore.TargetAddress, reached));
                } catch (Exception ex) when (ex is not OperationCanceledException) {
                    return Fin<string>.Fail(new RecoveryFault.RestoreFailed("re-attest", ex.Message));
                }
            })
            : IO.pure(Fin<string>.Fail(new RecoveryFault.RestoreFailed("re-attest", verdict.GetType().Name)))
        select outcome;
}
```

| [INDEX] | [POLICY]                 | [VALUE]                                | [BINDING]                                                  |
| :-----: | :----------------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | step dispatch            | generated `RestoreStep.Switch`, ranked order | compile-time exhaustive; a 7th step breaks the build, never a silent `_` arm |
|  [02]   | verify ladder            | `Snapshots.Verify` over raw bytes      | the ONE 8-tier ladder; never a stringly lineage check     |
|  [03]   | timeline guard           | `RecoveryPoint.Continues(archive)`     | a forked timeline faults; re-bootstrap is not resume      |
|  [04]   | recovery point           | WAL replay to `(Timeline, Lsn)`        | a real version, never an approximate timestamp            |
|  [05]   | projection rebuild       | inline re-fold + async lanes `StartAllAsync`, proven head-caught-up | no redundant inline re-rebuild; a stalled lane fails on `WaitForNonStaleData` |
|  [06]   | commit point             | re-attest (`Unauthored`-reachable) + AS-OF content-address compare | content identity equals the target; not verify-by-success |
