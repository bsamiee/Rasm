# [PERSISTENCE_RECOVERY]

Rasm.Persistence proves durability rather than assuming it. One recovery owner verifies backup, point-in-time recovery, object-store WORM residence, and replication across every engine, then folds the per-engine evidence into one typed `RecoveryFact` stream that measures achieved RPO and RTO against the declared objective and never lets a breach pass silent. `RecoveryProfile` is the `[SmartEnum<string>]` per-engine recovery-strategy axis: each row carries its steady-state and drill `RecoveryMode` pair, archive-retention horizon, restore-tier, and `BackupShape` classifier as data. `BackupKind` is the `[Union]` backup-shape family the one total `Backup` `Switch` executes or verifies — the input case selects the engine arm, so dispatch is one exhaustive generated `Switch` with no profile-keyed indirection and no silent arm. `RecoveryFault` is the closed `[Union]` DR-fault family on the doctrine `Expected` shape that every breach lifts into; `PitrWindow` is the `[ComplexValueObject]` measuring the recoverable WAL span against the row's `ArchiveRetention` horizon and the timeline lineage, while the recoverable-point recency rides the `replication-lag` fact's `Rpo` gauge. The declared `(Rpo, Rto)` target is the AppHost-owned `RecoveryObjective` this owner consumes off `ResolvedProfile.Recovery`, never minting it.

Per-engine the strategy is exact. The sqlite arm composes the settled `SqliteMaintenance.Maintain(connection, clocks, SqliteFactKind.Backup, destination)` paced raw paged session and admits the copy only after `quick_check` on the copy itself plus content-identity parity, because the backup verb succeeding is never the proof. The PG arm is verification-plus-failover-time-measurement through `ClusterConfig.Verify` on the replication GUCs, the live standby watermark triad (`LastReceivedLsn`/`LastFlushedLsn`/`LastAppliedLsn`), and `IdentifySystem`/`TimelineHistory` for the PITR timeline lineage — the engine's WAL archiver owns the bytes, this owner gauges the slot lag and the recoverable PITR window read-only, and the row's drill `RecoveryMode.Failover` measures standby-promotion time while its steady-state `RecoveryMode.Verify` gauges lag. The object arm carries the full `WormResidence` surface — S3 Object Lock retention (`Governance`/`Compliance` mode + `RetainUntilDate`), legal hold (`On`/`Off`), per-object cross-region `ReplicationStatus`, and Glacier cold-restore (`RestoreObjectAsync` with `RestoreInProgress`/`RestoreExpiration`) — and folds its transfer and lock-verify evidence into the typed `TransferReceipt`. `RecoveryDrill` runs the settled `StoreCeremony.Restore` fence-verify-materialize-sidecar-swap-epoch-bump-reopen choreography, stamps the measured RTO, and emits the typed `RestoreReceipt`; the `RecoveryFact` stream feeds `StoreFact`/`StoreEvidence` so observability and audit read one surface, and the drill cadence rides one `ScheduleEntry` under the maintenance lease so durability is audited, never assumed. `Npgsql` supplies the PG replication/PITR surface read verification-only; `AWSSDK.S3` carries the modeled WORM-lock, cross-region, and Glacier surface while `Azure.Storage.Blobs` carries the object residence and transfer with its immutability-policy parity research-held on `[WORM_OBJECT_LOCK]`; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive settled.

Wire posture: this page is host-local — every owner emits backup/restore/lock-verify evidence read server-side or on the embedded file, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The `RecoveryObjective` and the `ResolvedProfile` DR inputs arrive from the AppHost runtime port and are consumed as settled vocabulary, never minted here.

## [01]-[INDEX]

- [01]-[RECOVERY_AXIS]: per-engine recovery-profile axis, backup-kind family, the typed DR-fault union, the PITR window, and the WORM object-residence surface.
- [02]-[RECOVERY_DRILL]: the fence-restore-reopen drill cycle composing `StoreCeremony.Restore`, stamping measured RTO, the recovery-fact fold, and the drill-cadence schedule row.

## [02]-[RECOVERY_AXIS]

- Owner: `RecoveryProfile` the `[SmartEnum<string>]` per-engine recovery-strategy axis under the `StoreKeyPolicy` ordinal accessor, each row carrying its `(Steady, Drill)` `RecoveryMode` pair, archive-retention horizon, restore-tier, and `BackupShape` classifier; `BackupShape` the `[SmartEnum]` keyless backup-family classifier (paged | replication | object | snapshot) the row declares; `BackupKind` the `[Union]` backup-shape family the one total `Backup` `Switch` executes or verifies; `RecoveryFault` the closed `[Union]` DR-fault family on `Expected, IValidationError<RecoveryFault>` in the 5500 band; `PitrWindow` the `[ComplexValueObject]` recoverable-span-and-timeline measure; `WormResidence` the `[ComplexValueObject]` WORM object-store durability product carrying the verified S3 Object Lock retention, legal hold, replication-status, and Glacier cold-restore state with its `WormVerified`/`ColdReady` invariants; `RecoveryFact` with `RecoveryFactKind` the page-wide fact stream every arm emits onto, carrying the engine profile, the breach status, and the correlation.
- Cases: `RecoveryProfile` sqlite-paged | pg-replication | object-residence | file-snapshot, each declaring its steady-state and drill modes; `RecoveryMode` `Execute` (the arm performs the backup) | `Verify` (the arm reads engine-owned durability state) | `Failover` (the drill measures standby-promotion time) — the pg row is `(Verify, Failover)`, the object row `(Execute, Failover)`, so no mode value is dead vocabulary; `BackupKind` `Paged` (sqlite raw paged copy, verified), `Replication` (PG WAL slot + archive, verify-only — the `Backup` `Switch` rejects it as `ReplicationUnready` because the engine archiver owns the bytes), `ObjectTransfer` (cross-region SSE-KMS WORM copy), `Snapshot` (content-addressed sealed snapshot), `ColdRestore` (Glacier/deep-archive thaw) on the union; `RecoveryFault` `RpoBreach` 5501 | `RtoBreach` 5502 | `ReplicationUnready` 5503 | `WormViolation` 5504 | `RestoreIntegrity` 5505 | `ColdRestorePending` 5506; `RecoveryFactKind` backup-step | backup-complete | backup-verified | replication-lag | pitr-window | timeline-history | worm-lock | transfer-step | cold-restore | drill-rto | objective-breach.
- Entry: `public static RecoveryObjective Objective(ResolvedProfile host)` is the pure `host.Recovery` projection — the per-modality `(Rpo, Rto)` window is the AppHost `HostProfile.Recovery` column the runtime owns (a web service targets a tighter RPO/RTO than a Rhino plugin), and `RecoveryProfile` selects the per-engine strategy that meets it. `public static Fin<RecoveryFact> VerifyReplication(RecoveryProfile profile, RecoveryObjective objective, FrozenDictionary<string, string> observed, StandbyWatermark standby, Duration replicaLag, ReplicationSystemIdentification system, CorrelationId correlation, ClockPolicy clocks)` folds the PG replication state into one fact — `ClusterConfig.Verify` `MapFail`-lifts a missing GUC into `RecoveryFault.ReplicationUnready` and the measured `replicaLag` `Duration` (the time the standby trails, the byte-distance `standby.FlushLag` riding the fact `Bytes` slot as evidence) past the objective `Rpo` lifts `RecoveryFault.RpoBreach`; `public static IO<Seq<RecoveryFact>> Backup(RecoveryProfile profile, BackupKind kind, CorrelationId correlation, ClockPolicy clocks, RecoveryArms arms)` is one total `kind.Switch` driving the verified sqlite paged session, the WORM object transfer (grading the live read-back `WormResidence.WormVerified(contentKey, profile, now)`), the sealed snapshot, or the Glacier cold-restore initiation, with the verify-only `Replication` case lifting `RecoveryFault.ReplicationUnready` rather than silently returning empty; `public static RecoveryFact PitrFact(RecoveryProfile profile, PitrWindow window, CorrelationId correlation, ClockPolicy clocks)` grades the recoverable WAL `ArchiveSpan` against the row's `ArchiveRetention` horizon (the recoverable-point recency against the `Rpo` is the `replication-lag` fact's gauge, not the window's), and `public static RecoveryFact TimelineFact(RecoveryProfile profile, uint expected, TimelineHistoryFile history, CorrelationId correlation, ClockPolicy clocks)` grades the `TimelineHistory(tli)` lineage against the expected timeline so a post-failover divergence is a `timeline-history` fact rather than a silent split; `public static ScheduleEntry SteadyEntry(RecoveryProfile profile, OccurrenceSpec cadence, Func<IO<Seq<RecoveryFact>>> backup, Func<IO<RecoveryFact>> verify)` registers the per-engine steady-state cadence under the maintenance lease, reading `profile.Executes` to dispatch the `Execute`-mode rows onto the `Backup` fold and the `Verify`-mode pg row onto `VerifyReplication`, so the steady cadence and the `#RECOVERY_DRILL` drill cadence are two `ScheduleEntry` rows under one lease, never a backup that runs on the verify-only engine.
- Auto: the per-engine variance is data, not branches. `RecoveryProfile` rows carry the `(Steady, Drill)` `RecoveryMode` pair, the archive-retention `Duration`, and the `BackupShape` classifier, so a new engine is one row, and the `Backup` fold is one total `kind.Switch` whose `BackupKind` case selects the engine arm — a new backup shape breaks the `Switch` at compile time rather than slipping through a `_` arm. The PG arm never executes a backup — it verifies the replication GUCs (`wal_level=logical`, `archive_mode=on`, `archive_command` set) read-only through `ClusterConfig.Verify`, reads the standby's `LastReceivedLsn`/`LastFlushedLsn`/`LastAppliedLsn` triad and computes the slot lag as the `durable - applied` `NpgsqlLogSequenceNumber` byte-distance gauged against the `Rpo`, and projects a `PitrWindow` from `IdentifySystem`'s `XLogPos`/`Timeline` plus the WAL-archive retention `ArchiveSpan` gauged against the row's `ArchiveRetention` horizon — so a PITR window is the WAL-archive retention span proving the recoverable horizon, the recoverable-point recency rides the `replication-lag` `Rpo` gauge, never a managed `pg_basebackup` spawn, and a timeline-divergence after a prior failover surfaces through `TimelineHistory(tli)` and `PitrWindow.OnTimeline` as a `timeline-history` fact rather than a silent split. The sqlite arm composes the settled `SqliteMaintenance.Maintain(connection, clocks, SqliteFactKind.Backup, destination)` paced raw `sqlite3_backup_*` session so a hot embedded store backs up under concurrent writes with one `SqliteFact` per `SqliteMaintenancePolicy.BackupStepPages` step, then re-opens the destination and admits it only after `quick_check` returns `ok` on the copy and the copy's content hash matches — a `backup-verified` fact carries the proof and a mismatch is `RecoveryFault.RestoreIntegrity`. The object arm runs a cross-region `ObjectTransfer` through the `Store/remote#OBJECT_STORE` `BlobRemote` contract so an off-site copy is SSE-KMS encrypted, then verifies its WORM durability through the live S3 Object Lock surface — `GetObjectRetentionAsync` (`ObjectLockRetention.Mode`/`RetainUntilDate`), `GetObjectLegalHoldAsync` (`ObjectLockLegalHold.Status`), and the per-object `ReplicationStatus` read off `GetObjectMetadataResponse` — so `WormResidence.WormVerified` honors the legal-hold-OR-compliance-retention disjunction (a `Held` object is immutable even with no retention date), gauges the `RetainUntil - now` window against the row's `ArchiveRetention` horizon (a compliance lock expiring before the retention obligation is a violation), and folds a `worm-lock` fact and `RecoveryFault.WormViolation` when the object is unencrypted at rest (the `Sse` descriptor is empty), no lock holds, the retention window underruns the horizon, or a cross-region replica is `Failed`/`Pending`; a fetch of a `StorageClass`-archived object first initiates `RestoreObjectAsync` and rides `RecoveryFact.ColdRestore` with `RecoveryFault.ColdRestorePending` (the `WormResidence.ColdReady` gate over `RestoreInProgress`/`RestoreExpiration`) until the thaw clears. The file-snapshot arm reuses the `Version/snapshots#SNAPSHOT_PROTOCOL` sealed content-addressed write so a point-in-time snapshot is one catalog row. Every arm's evidence is a `RecoveryFact` row discriminated by `RecoveryFactKind` carrying its `RecoveryProfile`, breach `RecoveryStatus`, and `CorrelationId`, so the per-engine recovery buckets fold into one stream with slot/kind metadata rather than parallel construction sites.
- Receipt: every backup, verify, or lock-check folds a `RecoveryFact` row into the open receipt's proof rows feeding `StoreFact`/`StoreEvidence`; the typed `RestoreReceipt` (`#RECOVERY_DRILL`, owned at `Version/snapshots#RESTORE_AND_DIFF`) and `TransferReceipt` (owned at `Store/remote#MULTIPART_TRANSFER`) carry the algorithm evidence and never degrade to a generic receipt; an objective breach (measured RPO past `Rpo` or measured RTO past `Rto`) emits the `objective-breach` fact and the matching typed `RecoveryFault` case lifted onto the rail (the GUC-verify path through `MapFail`, the lag/RTO disjunctions as a direct `Fin.Fail`), never a bare `Error` whose failure identity collapses every cause to one key.
- Packages: Npgsql, Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, AWSSDK.S3, Azure.Storage.Blobs, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new engine recovery strategy is one `RecoveryProfile` row carrying its `(Steady, Drill)` modes, retention, tier, and `BackupShape`; a new backup shape is one `BackupKind` case plus one `BackupShape` row plus one `Backup` `Switch` arm the generated total dispatch forces every site to add; a new DR-fault cause is one `RecoveryFault` case in the 5500 band; a new objective dimension is one column on the AppHost `RecoveryObjective`; a new evidence bucket is one `RecoveryFactKind` row; a new object-store durability column is one field on the `WormResidence` `[ComplexValueObject]`; a new verb mode is one `RecoveryMode` value; zero new surface — a second backup tool, a parallel restore receipt, a per-engine evidence record, a profile-keyed dispatch table beside the `BackupKind` `Switch`, or a bare-`Error` breach is the deleted form.
- Boundary: the PG arm is verification-plus-failover-time-measurement — runtime `pg_basebackup`, `ALTER SYSTEM`, or a spawned backup binary is the rejected form; the engine's WAL archiver owns the bytes and this owner gauges the slot lag, the standby watermark triad, and the PITR timeline read-only through `ClusterConfig.Verify`, `LogicalReplicationConnection.IdentifySystem`, `LastReceivedLsn`/`LastFlushedLsn`/`LastAppliedLsn`, and `TimelineHistory`, so a Rasm process never spawns or bundles a PG backup tool; the `StandbyWatermark` physical-standby triad this arm reads is the DR replication plane, distinct from the logical-changefeed `SyncCursor.Lsn` apply cursor the `Sync/collaboration#TRANSPORT_AXIS` pump acknowledges — the two replication planes never collapse onto one cursor. The sqlite arm composes the settled `SqliteMaintenance.Maintain(connection, clocks, SqliteFactKind.Backup, destination)` paced raw `sqlite3_backup_*` session over the `Handle` bridge, re-opens the copy, and admits it only after `quick_check` plus content-identity — a copy admitted on the backup verb's success alone is the rejected form, because the verb succeeding is never the proof; the provider's whole-file `BackupDatabase` is subsumed by this paged session, which alone carries `BackupStepPages` progress facts. The object arm composes the settled `BlobRemote` contract and the `Store/remote#MULTIPART_TRANSFER` window and never a second object-store client, and its WORM durability rides the live S3 Object Lock surface rather than a `bool` standing in for an unverified retention claim — a `LegalHold` column that is never read against `GetObjectLegalHoldAsync`, or a `ReplicationRule` never gauged against the per-object `ReplicationStatus`, is the dead-field rejected form. The `RecoveryObjective` DR target is the AppHost `HostProfile.Recovery` per-modality column projected onto `ResolvedProfile.Recovery`, read through the `Version/recovery ← Rasm.AppHost/Runtime # [PORT]: ResolvedProfile DR-objective inputs` seam as settled vocabulary — `Recovery.Objective` is the pure `host.Recovery` projection and the per-engine arms gauge their measured RPO/RTO against it, so a host-band-keyed `(Rpo, Rto)` switch on this side is the deleted form and a DR target is never minted locally. The `LegalHold` and retention-class columns ride the `Version/retention` classification so a held backup is `HeldOverBudget` rather than evicted, never a second retention taxonomy. The `RecoveryFact` stream is the one recovery-evidence surface every engine arm emits onto so downstream observability and drill verification read it without re-learning per-engine receipt shapes, and a parallel per-engine evidence record is the deleted form.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class RecoveryFactKind {
    public static readonly RecoveryFactKind BackupStep = new("backup-step");
    public static readonly RecoveryFactKind BackupComplete = new("backup-complete");
    public static readonly RecoveryFactKind BackupVerified = new("backup-verified");
    public static readonly RecoveryFactKind ReplicationLag = new("replication-lag");
    public static readonly RecoveryFactKind PitrWindow = new("pitr-window");
    public static readonly RecoveryFactKind TimelineHistory = new("timeline-history");
    public static readonly RecoveryFactKind WormLock = new("worm-lock");
    public static readonly RecoveryFactKind TransferStep = new("transfer-step");
    public static readonly RecoveryFactKind ColdRestore = new("cold-restore");
    public static readonly RecoveryFactKind DrillRto = new("drill-rto");
    public static readonly RecoveryFactKind ObjectiveBreach = new("objective-breach");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class RecoveryMode {
    public static readonly RecoveryMode Execute = new("execute");
    public static readonly RecoveryMode Verify = new("verify");
    public static readonly RecoveryMode Failover = new("failover");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class RecoveryStatus {
    public static readonly RecoveryStatus Met = new("met");
    public static readonly RecoveryStatus Breached = new("breached");
    public static readonly RecoveryStatus Pending = new("pending");
}

[SmartEnum]
public sealed partial class BackupShape {
    public static readonly BackupShape Paged = new();
    public static readonly BackupShape Replication = new();
    public static readonly BackupShape Object = new();
    public static readonly BackupShape Snapshot = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class RecoveryProfile {
    public static readonly RecoveryProfile SqlitePaged = new(
        "sqlite-paged", steady: RecoveryMode.Execute, drill: RecoveryMode.Execute,
        archiveRetention: Duration.FromDays(7), restoreTier: "hot", shape: BackupShape.Paged);
    public static readonly RecoveryProfile PgReplication = new(
        "pg-replication", steady: RecoveryMode.Verify, drill: RecoveryMode.Failover,
        archiveRetention: Duration.FromDays(7), restoreTier: "standby", shape: BackupShape.Replication);
    public static readonly RecoveryProfile ObjectResidence = new(
        "object-residence", steady: RecoveryMode.Execute, drill: RecoveryMode.Failover,
        archiveRetention: Duration.FromDays(90), restoreTier: "cold", shape: BackupShape.Object);
    public static readonly RecoveryProfile FileSnapshot = new(
        "file-snapshot", steady: RecoveryMode.Execute, drill: RecoveryMode.Execute,
        archiveRetention: Duration.FromDays(30), restoreTier: "hot", shape: BackupShape.Snapshot);

    public RecoveryMode Steady { get; }
    public RecoveryMode Drill { get; }
    public Duration ArchiveRetention { get; }
    public string RestoreTier { get; }
    public BackupShape Shape { get; }

    public bool Executes => Steady == RecoveryMode.Execute;
}

[Union]
public abstract partial record RecoveryFault : Expected, IValidationError<RecoveryFault> {
    private RecoveryFault(string detail, int code) : base(detail, code, None) { }

    public static RecoveryFault Create(string message) => new RestoreIntegrity("recovery", message);

    public sealed record RpoBreach : RecoveryFault {
        public RpoBreach(Duration lag, Duration budget, string slot) : base($"rpo breach on {slot}: lag {lag} > {budget}", 5501) => (Lag, Budget, Slot) = (lag, budget, slot);
        public Duration Lag { get; }
        public Duration Budget { get; }
        public string Slot { get; }
    }
    public sealed record RtoBreach : RecoveryFault {
        public RtoBreach(Duration measured, Duration budget, string store) : base($"rto breach on {store}: {measured} > {budget}", 5502) => (Measured, Budget, Store) = (measured, budget, store);
        public Duration Measured { get; }
        public Duration Budget { get; }
        public string Store { get; }
    }
    public sealed record ReplicationUnready : RecoveryFault {
        public ReplicationUnready(string setting, string observed) : base($"replication unready: {setting}={observed}", 5503) => (Setting, Observed) = (setting, observed);
        public string Setting { get; }
        public string Observed { get; }
    }
    public sealed record WormViolation : RecoveryFault {
        public WormViolation(UInt128 contentKey, string detail) : base($"worm violation {contentKey:x32}: {detail}", 5504) => (ContentKey, Detail) = (contentKey, detail);
        public UInt128 ContentKey { get; }
        public string Detail { get; }
    }
    public sealed record RestoreIntegrity : RecoveryFault {
        public RestoreIntegrity(string store, string evidence) : base($"restore integrity on {store}: {evidence}", 5505) => (Store, Evidence) = (store, evidence);
        public string Store { get; }
        public string Evidence { get; }
    }
    public sealed record ColdRestorePending : RecoveryFault {
        public ColdRestorePending(UInt128 contentKey, string tier) : base($"cold restore pending {contentKey:x32} from {tier}", 5506) => (ContentKey, Tier) = (contentKey, tier);
        public UInt128 ContentKey { get; }
        public string Tier { get; }
    }
}

[ComplexValueObject]
public sealed partial class PitrWindow {
    public NpgsqlLogSequenceNumber EarliestRecoverable { get; }
    public NpgsqlLogSequenceNumber LatestRecoverable { get; }
    public Duration ArchiveSpan { get; }
    public uint Timeline { get; }

    public ulong RecoverableBytes => LatestRecoverable - EarliestRecoverable;
    public bool Spans => LatestRecoverable > EarliestRecoverable;
    public bool Covers(NpgsqlLogSequenceNumber target) => target >= EarliestRecoverable && target <= LatestRecoverable;
    public bool OnTimeline(uint expected) => Timeline == expected;
    public bool Meets(RecoveryProfile profile) => Spans && ArchiveSpan >= profile.ArchiveRetention;
}

public readonly record struct StandbyWatermark(
    NpgsqlLogSequenceNumber Received, NpgsqlLogSequenceNumber Flushed, NpgsqlLogSequenceNumber Applied) {
    public ulong FlushLag => Received - Flushed;
    public ulong ApplyLag => Flushed - Applied;
}

[ComplexValueObject]
public sealed partial class WormResidence {
    public string Bucket { get; }
    public string Region { get; }
    public S3StorageClass StorageClass { get; }
    public string Sse { get; }
    public ObjectLockRetentionMode RetentionMode { get; }
    public Instant RetainUntil { get; }
    public ObjectLockLegalHoldStatus LegalHold { get; }
    public Option<string> ReplicationRule { get; }
    public ReplicationStatus CrossRegion { get; }
    public Option<Instant> RestoreExpiration { get; }
    public bool RestoreInProgress { get; }

    public bool Held => LegalHold == ObjectLockLegalHoldStatus.On;
    public bool Archived => StorageClass == S3StorageClass.Glacier || StorageClass == S3StorageClass.DeepArchive;
    public bool Encrypted => Sse.Length > 0;
    public bool Locked(Instant now) => Held || (RetentionMode == ObjectLockRetentionMode.Compliance && RetainUntil > now);
    public bool RetentionMeets(RecoveryProfile profile, Instant now) => Held || RetainUntil - now >= profile.ArchiveRetention;
    public bool CrossRegionDurable => ReplicationRule.IsNone || CrossRegion == ReplicationStatus.Completed || CrossRegion == ReplicationStatus.Replica;

    public Fin<Unit> WormVerified(UInt128 contentKey, RecoveryProfile profile, Instant now) =>
        !Encrypted
            ? Fin.Fail<Unit>(new RecoveryFault.WormViolation(contentKey, $"unencrypted at rest: sse '{Sse}'"))
            : !Locked(now)
                ? Fin.Fail<Unit>(new RecoveryFault.WormViolation(contentKey, $"unlocked: retention {RetentionMode} until {RetainUntil}, hold {LegalHold}"))
                : !RetentionMeets(profile, now)
                    ? Fin.Fail<Unit>(new RecoveryFault.WormViolation(contentKey, $"retention {RetainUntil - now} < archive-horizon {profile.ArchiveRetention}"))
                    : CrossRegionDurable
                        ? Fin.Succ(unit)
                        : Fin.Fail<Unit>(new RecoveryFault.WormViolation(contentKey, $"cross-region {CrossRegion}"));

    public Fin<Unit> ColdReady(UInt128 contentKey) =>
        !Archived || (!RestoreInProgress && RestoreExpiration.IsSome)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new RecoveryFault.ColdRestorePending(contentKey, StorageClass.Value));
}

public readonly record struct RecoveryFact(
    RecoveryFactKind Kind,
    RecoveryProfile Profile,
    RecoveryStatus Status,
    string Slot,
    long Count,
    long Bytes,
    Duration Elapsed,
    Duration Measured,
    Instant At,
    CorrelationId Correlation);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BackupKind {
    private BackupKind() { }

    public sealed record Paged(string Source, string Destination, SqliteMaintenancePolicy Policy) : BackupKind;
    public sealed record Replication(string Slot, string Publication) : BackupKind;
    public sealed record ObjectTransfer(WormResidence Residence, UInt128 ContentKey) : BackupKind;
    public sealed record Snapshot(string Directory, SnapshotCodec Codec, CompressionPolicy Compression) : BackupKind;
    public sealed record ColdRestore(WormResidence Residence, UInt128 ContentKey, int Days) : BackupKind;
}

public sealed record RecoveryArms(
    Func<BackupKind.Paged, IO<(Seq<SqliteFact> Steps, Fin<string> Verify)>> Paged,
    Func<BackupKind.ObjectTransfer, IO<(TransferReceipt Receipt, WormResidence Live)>> Transfer,
    Func<BackupKind.Snapshot, IO<RecoveryFact>> Snapshot,
    Func<BackupKind.ColdRestore, IO<(bool InProgress, RestoreObjectResponse Response)>> Cold);

public static class Recovery {
    public static RecoveryObjective Objective(ResolvedProfile host) => host.Recovery;

    public static Fin<RecoveryFact> VerifyReplication(
        RecoveryProfile profile, RecoveryObjective objective, FrozenDictionary<string, string> observed,
        StandbyWatermark standby, Duration replicaLag, ReplicationSystemIdentification system, CorrelationId correlation, ClockPolicy clocks) =>
        ClusterConfig.Verify([("wal_level", "logical", "logical"), ("archive_mode", "on", "on"), ("archive_command", "set", "set")], observed)
            .MapFail(static error => (Error)new RecoveryFault.ReplicationUnready("wal_level|archive_mode|archive_command", error.Message))
            .Bind(_ => replicaLag <= objective.Rpo
                ? Fin.Succ(new RecoveryFact(
                    RecoveryFactKind.ReplicationLag, profile, RecoveryStatus.Met, system.XLogPos.ToString(),
                    0, (long)standby.FlushLag, Duration.Zero, replicaLag, clocks.Now, correlation))
                : Fin.Fail<RecoveryFact>(new RecoveryFault.RpoBreach(replicaLag, objective.Rpo, system.XLogPos.ToString())));

    public static RecoveryFact PitrFact(RecoveryProfile profile, PitrWindow window, CorrelationId correlation, ClockPolicy clocks) =>
        new(RecoveryFactKind.PitrWindow, profile, window.Meets(profile) ? RecoveryStatus.Met : RecoveryStatus.Breached,
            window.LatestRecoverable.ToString(), window.Timeline, (long)window.RecoverableBytes, Duration.Zero, window.ArchiveSpan, clocks.Now, correlation);

    public static RecoveryFact TimelineFact(RecoveryProfile profile, uint expected, TimelineHistoryFile history, CorrelationId correlation, ClockPolicy clocks) =>
        new(RecoveryFactKind.TimelineHistory, profile,
            history.FileName.StartsWith($"{expected:X8}", StringComparison.Ordinal) ? RecoveryStatus.Met : RecoveryStatus.Breached,
            history.FileName, expected, history.Content.LongLength, Duration.Zero, Duration.Zero, clocks.Now, correlation);

    public static IO<Seq<RecoveryFact>> Backup(
        RecoveryProfile profile, BackupKind kind, CorrelationId correlation, ClockPolicy clocks, RecoveryArms arms) =>
        kind.Switch(
            state: (Profile: profile, Correlation: correlation, Clocks: clocks, Arms: arms),
            paged: static (s, step) => s.Arms.Paged(step).Map(outcome => outcome.Steps
                .Map(f => new RecoveryFact(
                    f.Kind == SqliteFactKind.Backup && f.After.IsSome ? RecoveryFactKind.BackupComplete : RecoveryFactKind.BackupStep,
                    s.Profile, RecoveryStatus.Met, f.Slot, f.Count, f.Bytes, f.Elapsed, Duration.Zero, f.At, s.Correlation))
                .Add(outcome.Verify.Match(
                    Succ: hash => new RecoveryFact(RecoveryFactKind.BackupVerified, s.Profile, RecoveryStatus.Met, hash, 0, 0, Duration.Zero, Duration.Zero, s.Clocks.Now, s.Correlation),
                    Fail: refusal => new RecoveryFact(RecoveryFactKind.BackupVerified, s.Profile, RecoveryStatus.Breached, refusal.Message, 0, 0, Duration.Zero, Duration.Zero, s.Clocks.Now, s.Correlation)))),
            objectTransfer: static (s, hop) => s.Arms.Transfer(hop).Map(outcome => Seq(
                new RecoveryFact(RecoveryFactKind.TransferStep, s.Profile, RecoveryStatus.Met, outcome.Receipt.ContentKey.ToString("x32"), outcome.Receipt.Parts, outcome.Receipt.Bytes, outcome.Receipt.Elapsed, Duration.Zero, outcome.Receipt.At, outcome.Receipt.Correlation),
                new RecoveryFact(RecoveryFactKind.WormLock, s.Profile, outcome.Live.WormVerified(hop.ContentKey, s.Profile, s.Clocks.Now).IsSucc ? RecoveryStatus.Met : RecoveryStatus.Breached, hop.ContentKey.ToString("x32"), 0, 0, Duration.Zero, Duration.Zero, s.Clocks.Now, outcome.Receipt.Correlation))),
            snapshot: static (s, snap) => s.Arms.Snapshot(snap).Map(Seq),
            coldRestore: static (s, cold) => s.Arms.Cold(cold).Map(outcome => Seq(
                new RecoveryFact(RecoveryFactKind.ColdRestore, s.Profile,
                    outcome.InProgress || cold.Residence.ColdReady(cold.ContentKey).IsFail ? RecoveryStatus.Pending : RecoveryStatus.Met,
                    cold.ContentKey.ToString("x32"), 0, 0, Duration.Zero, Duration.Zero, s.Clocks.Now, s.Correlation))),
            replication: static (s, slot) => IO.fail<Seq<RecoveryFact>>(
                new RecoveryFault.ReplicationUnready(slot.Slot, "<pg-arm-is-verify-only:route-through-VerifyReplication>")));

    public static ScheduleEntry SteadyEntry(RecoveryProfile profile, OccurrenceSpec cadence, Func<IO<Seq<RecoveryFact>>> backup, Func<IO<RecoveryFact>> verify) =>
        new(
            Key: $"persistence-recovery-steady-{profile.Key}",
            Spec: cadence,
            Deadline: DeadlineClass.SupportWindow,
            Lease: Optional(LeasePolicy.Maintenance),
            Work: () => (profile.Executes ? backup().Map(static _ => unit) : verify().Map(static _ => unit)));
}
```

## [03]-[RECOVERY_DRILL]

- Owner: `RestoreReceipt` the typed restore evidence (at `Version/snapshots#RESTORE_AND_DIFF`); `TransferReceipt` the typed cross-region transfer evidence (at `Store/remote#MULTIPART_TRANSFER`); `RecoveryDrill` the static surface composing the settled `StoreCeremony.Restore` fence-restore-reopen choreography, stamping the measured RTO, folding the drill evidence into the `RecoveryFact` stream, and registering the drill cadence.
- Entry: `public static IO<Fin<RecoveryFact>> Run(RecoveryProfile profile, RecoveryObjective objective, StoreProfile target, StorePlacement placement, Atom<(StoreLifecycle State, Option<StoreOpenReceipt> Latest)> cell, BackupKind source, RestoreArms arms, ClockPolicy clocks)` — marks the clock, runs the settled `StoreCeremony.Restore(row, placement, cell, fenceWriters, materialize, check, swap, prove, clocks)` fence-verify-materialize-sidecar-swap-epoch-bump-reopen cycle against the backup `source`, stamps the elapsed as the measured RTO against the objective `Rto`, and folds a `drill-rto` `RecoveryFact` keyed by `target.Key@RestoreTier` so the breach reads against its restore tier, the over-budget case lifting `RecoveryFault.RtoBreach`; `public static ScheduleEntry DrillEntry(OccurrenceSpec cadence, Func<IO<Fin<RecoveryFact>>> drill)` registers the periodic restore-drill under the maintenance lease so durability is audited.
- Auto: the drill never re-implements the restore choreography — it composes the settled `StoreCeremony.Restore` whose `(fenceWriters, materialize, check, swap, prove)` delegate set carries the seven-step fence-verify-materialize-sidecar-rename-epoch-bump-reopen cycle, and the drill only times the cycle and grades the measured RTO. The `RestoreArms` record threads those five delegates plus the open-proof `prove` so the drill stamps the realized `StoreOpenReceipt.MigrationsApplied` into the `drill-rto` fact, and the underlying restore folds the typed `RestoreReceipt`. A drill against the PG arm reopens a standby from the verified replication state — its `materialize` is the standby-promotion wait and its `RecoveryMode` is `Failover`, so the measured RTO is the promotion-plus-reconnect time, never a managed `pg_restore`. A drill against the object arm whose `source` is a `ColdRestore` first thaws the archived object and rides `RecoveryFact.ColdRestore` with `RecoveryStatus.Pending` until the thaw completes, so the drill RTO includes the cold-tier retrieval latency. The drill cadence and the steady cadence ride two `ScheduleEntry` rows under `LeasePolicy.Maintenance` exactly as `RetentionSweep.SweepEntry` registers the retention sweep, so a periodic restore-drill stamps a `RecoveryFact` the workspace audits rather than an unverified durability assumption.
- Receipt: a drill rides the `drill-rto` `RecoveryFact` carrying the measured RTO `Duration`, the `RecoveryProfile`, the `target.Key@RestoreTier` slot, and the breach `RecoveryStatus`; the underlying restore folds the typed `RestoreReceipt` (source id, verified hash, target profile, elapsed, instant, correlation) and a cross-region transfer folds the typed `TransferReceipt`; an RTO breach is the typed `RecoveryFault.RtoBreach` case carrying the measured/budget/slot triple, never a bare `Error` whose failure identity coalesces every breach cause to one key.
- Packages: Microsoft.EntityFrameworkCore.Sqlite, Npgsql, AWSSDK.S3, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new drill stage is one step on the `StoreCeremony.Restore` choreography (owned there); a new measured dimension is one column on `RecoveryFact`; a new drill cadence is one policy value on the `ScheduleEntry` spec; zero new surface — a second restore choreography, a parallel drill receipt, or a re-implemented fence-materialize-swap cycle is the deleted form because the drill composes the settled `StoreCeremony.Restore` and stamps RTO onto the one `RecoveryFact` stream.
- Boundary: the drill composes the settled `Store/profiles#STORE_LIFECYCLE` `StoreCeremony.Restore` — it never re-implements the fence-verify-materialize-sidecar-rename-epoch-bump-reopen choreography, it only times the cycle and grades the measured RTO against the objective; the opaque single-`restore`-delegate form that hides the choreography behind one `Func<IO<StoreOpenReceipt>>` is the deleted form, because the drill must thread the same `(fenceWriters, materialize, check, swap, prove)` delegates the lifecycle owner declares. The measured RTO is `clocks.Elapsed(mark)` so the drill rides the one `ClockPolicy` seam and never `Stopwatch`. A drill against the PG arm reopens a standby from the verified replication state through the `Failover` mode rather than a managed restore, so the PG drill stays verification-plus-failover-time-measurement. The `RestoreReceipt`/`TransferReceipt` stay typed algorithm receipts carrying recovery evidence and never collapse to a generic `IReceipt`. The drill cadence rides the persistence-maintenance `ScheduleEntry` row under the maintenance lease so a periodic restore-drill stamps a `RecoveryFact` the workspace audits.

```csharp signature
public sealed record RestoreArms(
    Func<IO<Unit>> FenceWriters,
    Func<IO<string>> Materialize,
    Func<string, IO<string>> Check,
    Func<string, IO<Unit>> Swap,
    Func<DbConnection, IO<StoreOpenReceipt>> Prove);

public static class RecoveryDrill {
    public static IO<Fin<RecoveryFact>> Run(
        RecoveryProfile profile, RecoveryObjective objective, StoreProfile target, StorePlacement placement,
        Atom<(StoreLifecycle State, Option<StoreOpenReceipt> Latest)> cell, BackupKind source, RestoreArms arms, ClockPolicy clocks) =>
        IO.lift(clocks.Mark).Bind(mark =>
            StoreCeremony.Restore(target, placement, cell, arms.FenceWriters, arms.Materialize, arms.Check, arms.Swap, arms.Prove, clocks)
                .Map(reopened => {
                    var measured = clocks.Elapsed(mark);
                    var slot = $"{target.Key}@{profile.RestoreTier}";
                    return measured <= objective.Rto
                        ? Fin.Succ(new RecoveryFact(
                            RecoveryFactKind.DrillRto, profile, RecoveryStatus.Met, slot,
                            reopened.MigrationsApplied, 0, measured, measured, clocks.Now, reopened.Correlation))
                        : Fin.Fail<RecoveryFault>(new RecoveryFault.RtoBreach(measured, objective.Rto, slot));
                }));

    public static ScheduleEntry DrillEntry(OccurrenceSpec cadence, Func<IO<Fin<RecoveryFact>>> drill) =>
        new(
            Key: "persistence-recovery-drill",
            Spec: cadence,
            Deadline: DeadlineClass.SupportWindow,
            Lease: Optional(LeasePolicy.Maintenance),
            Work: () => drill().Map(static _ => unit));
}
```

| [INDEX] | [ENGINE]          | [STEADY]    | [DRILL]     | [STRATEGY]                                                | [EVIDENCE]                                                       |
| :-----: | :---------------- | :---------- | :---------- | :-------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | sqlite-paged      | `Execute`   | `Execute`   | `SqliteMaintenance.Maintain(SqliteFactKind.Backup)` paged copy, `quick_check`+hash | per-`BackupStepPages` step + complete + `backup-verified`        |
|  [02]   | pg-replication    | `Verify`    | `Failover`  | slot lag + standby triad + `IdentifySystem` PITR timeline | `ClusterConfig.Verify` + lag vs `Rpo` + `PitrWindow.ArchiveSpan` vs `ArchiveRetention`; drill measures standby-promotion RTO |
|  [03]   | object-residence  | `Execute`   | `Failover`  | cross-region SSE-KMS WORM `ObjectTransfer` + Object Lock, `ColdRestore` thaw | `TransferReceipt` + `worm-lock` (`Sse` encrypted, retention vs `ArchiveRetention`, legal-hold/replica) + `cold-restore` (`ColdReady` over `RestoreInProgress`/`RestoreExpiration`) |
|  [04]   | file-snapshot     | `Execute`   | `Execute`   | sealed content-addressed snapshot                         | `Version/snapshots` catalog row                                 |

## [04]-[RESEARCH]

- [PG_REPLICATION_PROBE]: the live-PG18 replication readiness round-trip — the standby watermark triad (`LogicalReplicationConnection.LastReceivedLsn`/`LastFlushedLsn`/`LastAppliedLsn`), the `IdentifySystem` `XLogPos`/`Timeline`/`SystemId` projection, the `TimelineHistory(tli)` lineage after a prior promotion, the `Show("archive_command")`/`Show("wal_level")` GUC reads, and the `pg_stat_replication`/`pg_replication_slots` `restart_lsn`/`confirmed_flush_lsn`/`active` columns the `VerifyReplication` fold reads against a configured slot, plus the `PitrWindow` archive-span gauged against the `Rpo`, proven against a live standby before the verification fold pins.
- [WORM_OBJECT_LOCK]: the live S3 Object Lock round-trip against MinIO/AWS — `GetObjectRetentionAsync` (`ObjectLockRetention.Mode` `Governance`/`Compliance`, `RetainUntilDate`), `GetObjectLegalHoldAsync` (`ObjectLockLegalHold.Status` `On`/`Off`), the per-object `ReplicationStatus` off `GetObjectMetadataResponse`, the `GetBucketReplicationAsync` cross-region rule, and the `RestoreObjectAsync` Glacier thaw with `RestoreInProgress`/`RestoreExpiration`, confirming the `WormResidence.WormVerified` fold and the `ColdRestorePending` gate before the WORM fences pin; Azure immutability-policy parity (`BlobContainerClient` legal-hold/time-based-retention) gauged for the azure-blob arm.
- [DRILL_RTO_MEASUREMENT]: the measured failover-and-reopen RTO of the `StoreCeremony.Restore` cycle against a staged sqlite backup (with the `quick_check`+content-identity copy admission) and a PG standby promotion — whether the fence-materialize-swap-reopen cycle's elapsed is a stable RTO measurement under concurrent load, the standby-promotion time the `Failover` PG drill stamps, and the cold-tier retrieval latency the object-cold drill includes, measured before the `RecoveryObjective` `Rto` defaults finalize.
