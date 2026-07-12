# [PERSISTENCE_VERSION_RECOVERY]

Rasm.Persistence proves recoverability of the durable store as a verified choreography that re-establishes the store's CONTENT IDENTITY, never a best-effort copy that trusts a backup succeeded: one `RecoveryRoute` axis crosses the backup substrate (PostgreSQL base-backup-plus-WAL for the Marten event store and the relational identity tier, content-addressed object-store replication for the geometry blobs, sealed-checkpoint archival for the AS-OF replay floor) with the recovery objective read settled off `ResolvedProfile.Recovery` (the Persistence-owned `RecoveryObjective` ingredient shape `Element/graph#STORE_RAIL` defines and AppHost FILLS at the port), so a new disaster-recovery topology is one route row; the recovery point is a real `RecoveryPoint` coordinate — the PostgreSQL `(Timeline, Lsn)` the `Npgsql.Replication` `IdentifySystem` yields plus the PER-STREAM Marten head version (`Events.FetchStreamStateAsync` → `StreamState.Version`, never the store-wide `EventSequenceNumber` high-water, a different version axis) and the HLC instant — never an approximate wall-clock timestamp, because a watermark replayed against a different timeline is re-bootstrap, not resume; the point-in-time restore composes the `Element/codec#SNAPSHOT_SPINE` `Snapshots.Verify` tier ladder in reverse over raw bytes — fence, verify, materialize, replay-WAL-to-coordinate, rebuild-projections, re-attest — every step a typed `StepFact` and every verify a real probe (`Snapshots.Verify` over the sealed checkpoints, the daemon `WaitForNonStaleData` gate plus the `FetchEventStoreStatistics` head proving the rebuild reached the event head, `Version/provenance#ATTESTED_LEDGER` `AttestedLedger.Verify` re-folding the chain, the AS-OF `AggregateStreamAsync` fold's `ContentAddress` compared to the target), so the restorer proves the restored store reconstructs the exact AS-OF state rather than trusting the copy landed. Marten's event stream IS the recovery substrate — a base backup plus WAL replay to the `RecoveryPoint` reconstructs the exact AS-OF state and the inline `GraphProjection` rebuilds deterministically through the daemon, so the recovery point is a real version. Replication verify gauges the head-minus-durable lag against the RPO and the rebuild span against the RTO so the objectives are measured facts on the `RecoveryFact` stream, never SLA prose. `ResolvedProfile`/`RecoveryObjective` are the Persistence-owned port-input shapes `Element/graph#STORE_RAIL` defines (AppHost fills the slots at the boundary); `RecoveryPoint` is local but bridges to `Version/timetravel`'s `TimeCut` as the AS-OF projection; `ContentAddress`, `Snapshots`, `SnapshotCatalogRow` arrive from `Element/codec`; `GraphProjection`, `ModelId` arrive from `Element/graph`; `AttestedEntry`, `AttestVerdict`, `AttestedLedger`, `SignedAuthorship`, `SigningKeyring`, `OpDigest` arrive from `Version/provenance` (the `SigningKeyring`/`OpDigest` sourced through it from `Element/identity#KMS_CUSTODY`); `Hlc` from `Version/commits`; `ObjectStore`, `ObjectClient` arrive from `Store/blobstore`; the mark/elapsed/now delegates and the correlation ride the injected `ProjectionContext` frame values — a `ClockPolicy`/`CorrelationId` parameter on any signature here is the deleted strata inversion.

## [01]-[INDEX]

- [01]-[RECOVERY_ROUTES]: the backup-substrate × objective axis, the per-substrate backup leg, the real `RecoveryPoint` coordinate, and the RPO/RTO measured fact.
- [02]-[POINT_IN_TIME_RESTORE]: the verified restore choreography, the `Snapshots.Verify` ladder in reverse, WAL-replay-to-`RecoveryPoint`, the head-caught-up projection rebuild, the re-attest content-identity proof, and the `StepFact` ledger.

## [02]-[RECOVERY_ROUTES]

- Owner: `RecoveryRoute` the `[SmartEnum<string>]` backup-substrate axis carrying its continuity flag (the live-lag-versus-checkpoint-age RPO discriminant); `RecoveryPoint` the `[ComplexValueObject]` recovery coordinate (PostgreSQL `Timeline`+`Lsn`, the Marten `StreamVersion`, the HLC `At`); `RecoveryFault` the closed backup/restore `[Union]` deriving from `Expected`; `RecoveryFact` the measured backup receipt; `RecoveryRoutes` the static surface owning the per-substrate backup leg, the coordinate capture, and the objective gauge. The RPO/RTO `RecoveryObjective` is NOT re-declared here — it is the `Element/graph#STORE_RAIL` `ResolvedProfile.Recovery` value read settled (AppHost fills it at the port), and the route→retention-class binding is the `Version/retention#RETENTION_CLASSES` owner's (`stream`/`blob`/`snapshot`), never duplicated here.
- Cases: `RecoveryRoute` is three rows — `pg-pitr` (PostgreSQL base backup plus continuous WAL archive — the Marten event store and the relational identity tier, restored by replay to a target `(Timeline, Lsn)`), `object-replica` (content-addressed object-store cross-region replication — the geometry blobs, restored by content-key `Head`-confirm), `snapshot-archive` (sealed AS-OF `Checkpoint` archival to cold storage — the bounded-replay floor, restored by `Snapshots.Verify`-gated materialization); a fourth substrate is one row carrying its continuity flag. `RecoveryFault` is `BackupFailed | RestoreFailed | ObjectiveBreach | VerifyFailed | TimelineDivergence | ReplicationLag` — `TimelineDivergence` the re-bootstrap-vs-resume guard (a captured coordinate on a timeline the archive does not continue), `ReplicationLag` the live-lag RPO breach on a continuous route, `ObjectiveBreach` the RTO over the objective.
- Entry: `public static IO<RecoveryFact> Backup(RecoveryRoute route, RecoveryContext ctx, RecoveryObjective objective, ProjectionContext frame)` runs the route's real backup leg, captures the `RecoveryPoint` coordinate, and stamps the RPO/RTO-measured fact under `objective`; `public static RecoveryObjective Objective(ResolvedProfile profile)` reads the settled DR window off the injected profile (`profile.Recovery`), never re-deriving it.
- Auto: the `pg-pitr` leg opens a `LogicalReplicationConnection`, reads the live coordinate through `IdentifySystem` (the `XLogPos` head LSN plus the `Timeline`), takes the durable-archive flush LSN from the operator-supplied `RecoveryContext.ArchiveFlushed` cursor (a Rasm process verifies the archive, never queries a standby it does not own), measures the RPO as the head-minus-flushed WAL byte lag (an `NpgsqlLogSequenceNumber` comparison clamped at zero against a probe-race cursor inversion, projected to a `Duration` through the EXPLICIT `RecoveryContext.WalBytesPerSecond` throughput POLICY row — a hardcoded segment-size-as-rate literal is the deleted fabrication), and binds the PER-STREAM Marten head from `Events.FetchStreamStateAsync(ctx.Model).Version` when the DR exercise targets a model (`None` otherwise — the restore then folds by `timestamp:`), NEVER the store-wide `FetchEventStoreStatistics().EventSequenceNumber` high-water, a different version axis that folds a per-stream `AggregateStreamAsync(version:)` to the head; the `object-replica` leg folds the geometry-blob manifest (content key + local seal instant) through `ObjectStore.Head` against the replica client so an EMPTY missing set proves the cross-region replica byte-identical by content key (the write-once seal makes a re-replicated blob a benign `412`-noop) and the RPO is the age of the OLDEST locally-sealed blob the replica still lacks — the true data-loss window, never a count-of-absent-blobs fabricated as minutes; the `snapshot-archive` leg seals the newest `Checkpoint` to cold storage and immediately re-reads it through `Snapshots.Verify` on raw bytes so an archived checkpoint that fails the ladder faults at backup time, never at restore; the RTO is the `frame.Elapsed(mark)` backup span and both objectives gauge against the settled `RecoveryObjective` by direct `Duration` comparison.
- Receipt: a backup rides `store.recovery.backup` carrying the route, the `RecoveryPoint`, and the measured RPO; an objective gauge rides `store.recovery.objective` carrying the RPO/RTO pair and the breach flag.
- Packages: Npgsql (`LogicalReplicationConnection.IdentifySystem`, `ReplicationSystemIdentification.XLogPos`/`Timeline`, `NpgsqlLogSequenceNumber` comparison operators + cast to `ulong`), Marten (`DocumentStore.For`, `IQuerySession.Events.FetchStreamStateAsync` → `StreamState.Version`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new backup substrate is one `RecoveryRoute` row carrying its continuity flag; a new objective dimension is one field on the AppHost `RecoveryObjective` (never re-declared here); a new backup-time fault is one `RecoveryFault` case; zero new surface — a per-engine backup service, a second recovery taxonomy, an SLA-as-prose objective, or a locally re-declared objective record is the deleted form because the route axis crosses substrate and objective, the objective is the AppHost-settled measured fact, and the recovery point is the real PostgreSQL coordinate.
- Boundary: PostgreSQL is never spawned or bundled by a Rasm process so the `pg-pitr` backup is operator-provisioned WAL archiving the route VERIFIES, never executes `ALTER SYSTEM` to configure (provisioning is verification-only, `Store/provisioning#SERVER_EXTENSIONS`); the recovery point is the `(Timeline, Lsn)` coordinate `IdentifySystem` yields, NEVER a `clock_timestamp()` wall-clock instant, because a base backup plus WAL replay reconstructs the exact AS-OF state only when the replay target rides the same timeline the archive continues — a coordinate captured on a forked timeline faults `RecoveryFault.TimelineDivergence` at restore rather than silently replaying onto a divergent history; the `object-replica` route reuses the `Store/blobstore#OBJECT_STORE` content-addressed write-once seal so a replica is byte-identical by hash and the `412`-noop makes a re-replicated blob a benign no-op (the seal IS the concurrency primitive, no read-before-write), and the measured replication lag is the age of the oldest locally-sealed blob the replica still lacks (the point since which the replica is provably incomplete — every later seal is also unreplicated), a `RecoveryFault.ReplicationLag` when it exceeds the RPO; the `snapshot-archive` route seals each AS-OF `Checkpoint` and re-verifies it through the ONE `Snapshots.Verify` tier ladder so the archived bytes self-reject before cold storage if torn; the RPO/RTO are measured facts on the `RecoveryFact` stream so a breach is a typed signal the AppHost health probe reads, never a prose SLA, and the `RecoveryObjective` is the AppHost-owned vocabulary read settled, never a parallel local record.

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

// The recovery coordinate is the PostgreSQL `(Timeline, Lsn)` `IdentifySystem` yields plus the PER-STREAM Marten
// head (`Events.FetchStreamStateAsync` → `StreamState.Version` for the DR-target model — NEVER the store-wide
// `EventSequenceNumber` high-water, a different version axis) and the HLC instant — a real version the WAL replay
// reaches, never a wall-clock approximation. `Lsn` rides the `NpgsqlLogSequenceNumber` cast to `ulong`
// (monotone-comparable); `Timeline` guards re-bootstrap.
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
// is the deleted form. Band membership derives `Code => FaultBand.Recovery + n` through the registry row
// (`Element/graph#FAULT_TABLES` — a bare integer literal is the deleted form) and `Message`/`Category` project through the
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

    public override int Code => FaultBand.Recovery + Switch(
        backupFailed:       static _ => 1,
        restoreFailed:      static _ => 2,
        objectiveBreach:    static _ => 3,
        verifyFailed:       static _ => 4,
        timelineDivergence: static _ => 5,
        replicationLag:     static _ => 6);

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

// `WalBytesPerSecond` is the operator-declared archiver-throughput POLICY row the WAL byte lag divides through —
// an observed-rate estimator may replace the declared row later, but a hardcoded segment-size-as-rate literal is
// the deleted fabrication. `Model` names the DR-exercise target stream whose per-stream head the coordinate
// carries; `None` captures a store-wide coordinate the restore folds by `timestamp:`. `ReplicaManifest` pairs
// each content key with its LOCAL seal instant so the replica lag is a real age.
public readonly record struct RecoveryContext(
    string Dsn, string ArchiveRoot, uint ArchiveTimeline, NpgsqlLogSequenceNumber ArchiveFlushed, long WalBytesPerSecond,
    Option<ModelId> Model, ObjectStore BlobStore, ObjectClient BlobClient, Seq<(ContentAddress Key, Instant SealedAt)> ReplicaManifest,
    Seq<SnapshotCatalogRow> Checkpoints, ulong SchemaFingerprint, ulong Epoch);

// The recovery receipt on the kernel validity floor ([C]): `IsValid` is ONE `ValidityClaim.All` fold —
// non-negative measured lags plus the objective bit — never a hand-rolled `&&` chain.
public readonly record struct RecoveryFact(
    RecoveryRoute Route, RecoveryPoint Point, Duration MeasuredRpo, Duration MeasuredRto,
    bool MeetsObjective, Instant At, Guid Correlation) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(MeasuredRpo >= Duration.Zero),
        ValidityClaim.Of(MeasuredRto >= Duration.Zero),
        ValidityClaim.Of(MeetsObjective));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class RecoveryRoutes {
    public static RecoveryObjective Objective(ResolvedProfile profile) => profile.Recovery;

    public static IO<RecoveryFact> Backup(RecoveryRoute route, RecoveryContext ctx, RecoveryObjective objective, ProjectionContext frame) =>
        // The leg dispatch is the GENERATED `RecoveryRoute.Switch` (compile-time exhaustive over the closed three-row
        // axis) so a fourth backup substrate breaks the build here — a `route.Key switch { ... _ => SnapshotFloor }`
        // string switch with a runtime-silent `_` arm is the rejected form (a new route would silently run the floor leg).
        from mark in IO.lift(frame.Mark)
        from leg in route.Switch(
            state: (ctx, frame),
            pgPitr: static s => PgPitr(s.ctx, s.frame),
            objectReplica: static s => ObjectReplica(s.ctx, s.frame),
            snapshotArchive: static s => SnapshotFloor(s.ctx, s.frame))
        let rto = frame.Elapsed(mark)
        let fact = new RecoveryFact(route, leg.Point, leg.Rpo, rto, (leg.Rpo <= objective.Rpo) && (rto <= objective.Rto), frame.Now(), frame.Correlation)
        // A CONTINUOUS route's RPO breach is an actionable live-lag fault (`ReplicationLag` on a replica leg, the
        // measured WAL/replication gap); a DISCRETE route's breach is the expected checkpoint-age the next seal
        // closes, so it records `MeetsObjective: false` in the fact without faulting. The RTO over objective faults
        // every route alike because a slow backup is never expected.
        from gauged in route.Continuous && (leg.Rpo > objective.Rpo)
            ? IO.fail<RecoveryFact>(new RecoveryFault.ReplicationLag(route.Key, leg.Rpo, objective.Rpo))
            : rto > objective.Rto
                ? IO.fail<RecoveryFact>(new RecoveryFault.ObjectiveBreach(route.Key, rto, objective.Rto))
                : IO.pure(fact)
        select gauged;

    // `pg-pitr`: the coordinate is the live `(Timeline, XLogPos)` `IdentifySystem` yields plus the PER-STREAM Marten
    // head for the DR-target model (`FetchStreamStateAsync(model).Version` — the store-wide `EventSequenceNumber`
    // high-water is a DIFFERENT version axis that would fold a per-stream `AggregateStreamAsync(version:)` to the
    // head, falsifying the point-in-time proof); the RPO is the head-minus-archive-flushed WAL byte lag divided
    // through the `WalBytesPerSecond` policy row. The lag is CLAMPED at zero — the head is always >= the
    // durable-archive flush cursor, so an archive cursor momentarily ahead (a probe race) reads zero lag rather
    // than a `ulong` underflow to a spurious near-`MaxValue` Duration. A coordinate on a timeline the archive does
    // not continue is the restore-time guard.
    static IO<(RecoveryPoint Point, Duration Rpo)> PgPitr(RecoveryContext ctx, ProjectionContext frame) =>
        IO.liftAsync<(RecoveryPoint, Duration)>(async () => {
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
            // The throughput row floors at one byte/second — a zero policy row reads as maximal finite lag,
            // never a division blow-up minting an unrepresentable Duration inside the capture seam.
            return (RecoveryPoint.Of(system, head, frame.Now()), Duration.FromSeconds(lagBytes / double.Max(ctx.WalBytesPerSecond, 1d)));
        }) | @catch<IO, (RecoveryPoint, Duration)>(static error => error.IsExceptional, static error => IO.fail<(RecoveryPoint, Duration)>(new RecoveryFault.BackupFailed("pg-pitr", error.Message)));

    // `object-replica`: fold the (content key, local seal instant) manifest through `ObjectStore.Head` against the
    // replica client so an EMPTY absent set proves the replica byte-identical by hash (the write-once seal makes any
    // re-replicate a benign `412`-noop); the RPO is the age of the OLDEST locally-sealed blob the replica still
    // lacks — every later seal is also unreplicated, so this age IS the data-loss window, never a count-as-minutes.
    static IO<(RecoveryPoint Point, Duration Rpo)> ObjectReplica(RecoveryContext ctx, ProjectionContext frame) =>
        ctx.ReplicaManifest.TraverseM(entry => ctx.BlobStore.Head(ctx.BlobClient, entry.Key).Map(present => (entry.Key, entry.SealedAt, Present: present.IsSome))).As()
            .Map(probes => probes.Filter(static p => !p.Present))
            .Map(absent => (RecoveryPoint.Floor(frame.Now()),
                absent.Fold(Option<Instant>.None, static (oldest, a) => Some(oldest.Match(Some: m => Instant.Min(m, a.SealedAt), None: () => a.SealedAt)))
                    .Match(Some: oldest => frame.Now() - oldest, None: () => Duration.Zero)));

    // `snapshot-archive`: seal the newest `Checkpoint` and re-read it through the ONE `Snapshots.Verify` tier ladder
    // on raw bytes so an archived checkpoint that fails the ladder faults at BACKUP time, never at restore; the RPO
    // is the newest-checkpoint age. A torn or foreign sealed checkpoint self-rejects before cold storage.
    static IO<(RecoveryPoint Point, Duration Rpo)> SnapshotFloor(RecoveryContext ctx, ProjectionContext frame) =>
        toSeq(ctx.Checkpoints.OrderByDescending(static c => c.WrittenAt)).Head.Match(
            Some: newest => IO.lift(() => ReadSealed(ctx.ArchiveRoot, newest.Id))
                .Bind(bytes => Snapshots.Verify(bytes, ctx.SchemaFingerprint, ctx.Epoch).Match(
                    Succ: _ => IO.pure((RecoveryPoint.Floor(newest.WrittenAt), frame.Now() - newest.WrittenAt)),
                    Fail: _ => IO.fail<(RecoveryPoint, Duration)>(new RecoveryFault.VerifyFailed("snapshot-archive", newest.Hash, ContentAddress.Of(UInt128.Zero)))))
                | @catch<IO, (RecoveryPoint, Duration)>(static error => error.IsExceptional, static error => IO.fail<(RecoveryPoint, Duration)>(new RecoveryFault.BackupFailed("snapshot-archive", error.Message))),
            None: () => IO.pure((RecoveryPoint.Floor(frame.Now()), Duration.Zero)));

    // Shared with `PointInTimeRestore.Verify` — ONE sealed-read spelling for the archive layout, never re-derived.
    internal static byte[] ReadSealed(string root, Guid id) => File.ReadAllBytes(Path.Combine(root, $"{id}{Snapshots.Suffix}"));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                                    | [BINDING]                                                                                                             |
| :-----: | :----------------- | :--------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | recovery point     | `(Timeline, Lsn)` + per-stream `StreamState.Version`       | a real version via `IdentifySystem` + `FetchStreamStateAsync`; never the global `EventSequenceNumber` or a wall clock |
|  [02]   | objective source   | `ResolvedProfile.Recovery` (graph-defined, AppHost-filled) | never a locally re-declared `RecoveryObjective` record                                                                |
|  [03]   | pg recovery        | base backup + WAL replay to coordinate                     | the Marten stream restores to an exact version                                                                        |
|  [04]   | WAL RPO projection | byte lag ÷ `RecoveryContext.WalBytesPerSecond`             | an explicit throughput policy row; a segment-size-as-rate literal is deleted                                          |
|  [05]   | blob recovery      | `ObjectStore.Head` over `(key, sealedAt)` manifest         | byte-identical by hash; `412`-noop on re-replicate; lag = oldest missing seal's age                                   |
|  [06]   | snapshot floor     | `Snapshots.Verify` at backup time                          | a torn sealed checkpoint faults before cold storage                                                                   |
|  [07]   | objective gauge    | measured RPO/RTO `RecoveryFact`                            | a breach is a typed health signal, never SLA prose                                                                    |

## [03]-[POINT_IN_TIME_RESTORE]

- Owner: `RestoreStep` the `[SmartEnum<string>]` ordered choreography step carrying its rank; `StepFact` the per-step receipt; `RestoreLedger` the per-run step sequence proving completeness; `RestoreContext` the restore inputs (the target `RecoveryPoint`, the expected content address, the settled `RecoveryObjective` whose `Rto` bounds the projection catch-up wait, the document store, the `Fence`/`Materialize`/`ReplayTo` platform delegates, the `AttestedChain`/`KeyringFor`/`DigestOf` verify delegates); `PointInTimeRestore` the static surface owning the verified restore choreography that re-establishes content identity.
- Cases: `RestoreStep` is `Fence | Verify | Materialize | ReplayWal | RebuildProjections | ReAttest` in declared rank order — each step verifies before the next runs and the restore never best-efforts past a failed step; the run short-circuits on the first `Fin` failure and flushes the ledger so a half-restored store classifies unambiguously at the next open.
- Entry: `public static IO<(RestoreLedger Ledger, Fin<RecoveryPoint> Outcome)> Run(RecoveryRoute route, RecoveryContext ctx, RestoreContext restore, ProjectionContext frame)` composes the choreography step by step through one `FoldM`, each step emitting a `StepFact` and short-circuiting on the first failure; the outcome carries the reached `RecoveryPoint` on success.
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

// The restore ledger on the kernel validity floor ([C]): completeness IS the claim fold — every ranked
// `RestoreStep` row landed exactly once.
public readonly record struct RestoreLedger(Seq<StepFact> Steps) : IValidityEvidence {
    public bool Complete => Steps.Count == RestoreStep.Items.Count;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(Steps.Count, RestoreStep.Items.Count),
        ValidityClaim.CountExactly(Steps.Map(static s => s.Step).Distinct().Count, RestoreStep.Items.Count));
    public RestoreLedger With(StepFact fact) => new(Steps.Add(fact));
}

// The restore composes real probes injected as delegates so the choreography owns the verified shape while the
// foreign-byte legs bind at the platform seam: `Target` is the caller-supplied AS-OF coordinate to restore to (its
// `StreamVersion` the PER-STREAM head captured at backup), `TargetAddress` the expected reconstructed-graph content
// key — the `Version/timetravel#TIME_TRAVEL` `Checkpoint.AsOfKey` (= `Address`, the S3 seam `ContentAddress.OfGraph`
// digest) at the target cut, the ONE cross-runtime content-identity oracle — `Objective` the settled AppHost-filled
// DR window whose `Rto` bounds the projection catch-up wait (a hardcoded deadline literal is the deleted knob),
// `AttestedChain` reads the restored attested ledger, and `Fence`/`Materialize`/`ReplayTo` own the pool quiesce,
// the base materialization, and the WAL replay — a choreography step with no effect behind its receipt is the
// deleted illusory form.
// `KeyringFor`/`DigestOf` are the EXACT two delegates `Version/provenance#ATTESTED_LEDGER` `AttestedLedger.Verify`
// composes — the per-authorship KMS keyring resolver and the INDEPENDENT per-entry content-digest recomputation (so
// the chain's `Unauthored` arm actually fires); a single `(SignedAuthorship, OpDigest) -> IO<bool>` predicate is the
// rejected shape because it cannot drive `Custody.Verify`'s `CustodyVerdict` dispatch and self-compares the digest.
public sealed record RestoreContext(
    IDocumentStore Store, RecoveryPoint Target, ContentAddress TargetAddress, ModelId Model, RecoveryObjective Objective,
    Func<RecoveryPoint, IO<Unit>> ReplayTo, Func<IO<Unit>> Fence, Func<IO<Unit>> Materialize,
    Func<IO<Seq<AttestedEntry>>> AttestedChain, Func<SignedAuthorship, SigningKeyring> KeyringFor, Func<AttestedEntry, OpDigest> DigestOf);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class PointInTimeRestore {
    public static IO<(RestoreLedger Ledger, Fin<RecoveryPoint> Outcome)> Run(RecoveryRoute route, RecoveryContext ctx, RestoreContext restore, ProjectionContext frame) =>
        toSeq(RestoreStep.Items.OrderBy(static s => s.Rank)).FoldM(
            (Ledger: new RestoreLedger(Seq<StepFact>()), Outcome: Fin<RecoveryPoint>.Succ(restore.Target)),
            (state, step) => state.Outcome.IsFail
                ? IO.pure(state)
                : Perform(step, route, ctx, restore).Map(result => result.Match(
                    Succ: evidence => (state.Ledger.With(new StepFact(step, evidence, frame.Now())), state.Outcome),
                    Fail: error => (state.Ledger.With(new StepFact(step, error.Message, frame.Now())), Fin<RecoveryPoint>.Fail(error)))))
        .Map(static final => (final.Ledger, final.Outcome)).As();

    // The per-step dispatch is the GENERATED `RestoreStep.Switch` (compile-time exhaustive over the closed six-row
    // smart-enum, one arm per case) so a new choreography step breaks the build HERE — a `step.Key switch { ... _ => }`
    // string switch with a runtime-silent `_` arm is the rejected form (a 7th step would silently fall into `reAttest`).
    static IO<Fin<string>> Perform(RestoreStep step, RecoveryRoute route, RecoveryContext ctx, RestoreContext restore) =>
        step.Switch(
            state: (route, ctx, restore),
            fence: static s => s.restore.Fence().Map(static _ => Fin<string>.Succ("<writers-quiesced>")),
            verify: static s => IO.pure(Verify(s.route, s.ctx, s.restore.Target)),
            materialize: static s => s.restore.Materialize().Map(_ => Fin<string>.Succ($"<base-materialized:tl{s.ctx.ArchiveTimeline}>")),
            replayWal: static s => s.restore.ReplayTo(s.restore.Target).Map(static _ => Fin<string>.Succ($"<wal-replayed:lsn{s.restore.Target.Lsn:x}>")),
            rebuildProjections: static s => RebuildProjections(s.restore),
            reAttest: static s => ReAttest(s.restore));

    // `Verify` runs the ONE `Snapshots.Verify` 8-tier ladder over EVERY sealed checkpoint's raw bytes before any
    // decoder binds, then asserts the target coordinate continues the archive timeline — a forked timeline is the
    // re-bootstrap-vs-resume defect that faults `TimelineDivergence` rather than replaying onto a divergent history.
    static Fin<string> Verify(RecoveryRoute route, RecoveryContext ctx, RecoveryPoint target) =>
        ctx.Checkpoints.Fold(Fin<int>.Succ(0), (acc, row) => acc.Bind(verified =>
            Try.lift(() => RecoveryRoutes.ReadSealed(ctx.ArchiveRoot, row.Id)).Run()
                // An unreadable checkpoint file is an I/O restore fault carrying its cause — mapping it to a
                // `VerifyFailed` with a zero Found address would erase the diagnostic and mislabel the tier.
                .MapFail(error => (Error)new RecoveryFault.RestoreFailed("verify", error.Message))
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
    static IO<Fin<string>> RebuildProjections(RestoreContext restore) =>
        IO.liftAsync<Fin<string>>(async () => {
            try {
                await restore.Store.Advanced.RebuildSingleStreamAsync<GraphProjection>(restore.Model.Value).ConfigureAwait(false);
                await using IProjectionDaemon daemon = await restore.Store.BuildProjectionDaemonAsync().ConfigureAwait(false);
                await daemon.StartAllAsync().ConfigureAwait(false);
                // The catch-up deadline IS the settled DR window — a rebuild slower than the RTO is already a failed
                // restore, so the objective row bounds the wait and no second deadline literal exists to drift.
                await daemon.WaitForNonStaleData(restore.Objective.Rto.ToTimeSpan()).ConfigureAwait(false);
                EventStoreStatistics stats = await restore.Store.Advanced.FetchEventStoreStatistics().ConfigureAwait(false);
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
                    await using IQuerySession query = restore.Store.QuerySession();
                    // Prefer the EXACT Marten stream version the recovery coordinate carries (`AggregateStreamAsync(version:)`
                    // is precise) over the approximate `timestamp:` fold; `RecoveryPoint.StreamVersion` is the PER-STREAM
                    // head `FetchStreamStateAsync` captured at backup for this model — the same version axis this fold
                    // consumes — so the AS-OF reconstruct lands on the same event the LSN replay reached.
                    GraphProjection? rebuilt = await restore.Target.StreamVersion.Match(
                        Some: version => query.Events.AggregateStreamAsync<GraphProjection>(restore.Model.Value, version: version),
                        None: () => query.Events.AggregateStreamAsync<GraphProjection>(restore.Model.Value, timestamp: restore.Target.At.ToDateTimeOffset())).ConfigureAwait(false);
                    ContentAddress reached = rebuilt is { } projection ? ContentAddress.OfGraph(projection.Graph) : ContentAddress.Of(UInt128.Zero);
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

| [INDEX] | [POLICY]           | [VALUE]                                                             | [BINDING]                                                                                                  |
| :-----: | :----------------- | :------------------------------------------------------------------ | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | step dispatch      | generated `RestoreStep.Switch`, ranked order                        | compile-time exhaustive; a 7th step breaks the build, never a silent `_` arm                               |
|  [02]   | verify ladder      | `Snapshots.Verify` over raw bytes                                   | the ONE 8-tier ladder; never a stringly lineage check                                                      |
|  [03]   | timeline guard     | `RecoveryPoint.Continues(archive)`                                  | a forked timeline faults; re-bootstrap is not resume                                                       |
|  [04]   | recovery point     | WAL replay to `(Timeline, Lsn)`                                     | a real version, never an approximate timestamp                                                             |
|  [05]   | projection rebuild | inline re-fold + async lanes `StartAllAsync`, proven head-caught-up | no redundant inline re-rebuild; a stalled lane fails on `WaitForNonStaleData` bounded by the settled `Rto` |
|  [06]   | commit point       | re-attest (`Unauthored`-reachable) + AS-OF content-address compare  | content identity equals the target; not verify-by-success                                                  |
