# [PERSISTENCE_RECOVERY]

Rasm.Persistence proves durability rather than assuming it: one recovery owner verifies backup, point-in-time recovery, and replication across every engine and folds the per-engine evidence into one typed `RecoveryFact` stream that measures the achieved RPO and RTO against the declared objective. `RecoveryProfile` is the `[SmartEnum<string>]` per-engine recovery-strategy axis, `BackupKind` the `[Union]` backup-shape family the strategy executes or verifies, and the declared `(Rpo, Rto)` target a drill measures against is the AppHost-owned `RecoveryObjective` this owner consumes settled off `ResolvedProfile.Recovery`, never minting it. The sqlite arm drives `SqliteMaintenance.Backup` paged copy, the PG arm is verification-only through `ClusterConfig.Verify` on `StoreProfile.Replication` plus the WAL-slot `SyncCursor.Lsn` watermark, and the object arm carries `ObjectResidence` plus `LegalHold`. `RecoveryDrill` runs a fence-restore-reopen cycle through the settled `StoreCeremony.Restore`, stamps the measured RTO, and emits the typed `RestoreReceipt`/`TransferReceipt`; the `RecoveryFact` stream feeds `StoreFact`/`StoreEvidence` so observability and audit read one surface. `Npgsql` supplies the PG replication/PITR surface read verification-only; the admitted `AWSSDK.S3` and `Azure.Storage.Blobs` carry object-store residence and cross-region transfer; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive settled. The AppHost `ResolvedProfile` DR-objective inputs are the one port this owner reads.

Wire posture: this page is host-local — every owner emits backup/restore evidence read server-side or on the embedded file, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The `RecoveryObjective` and the `ResolvedProfile` DR inputs arrive from the AppHost runtime port and are consumed as settled vocabulary, never minted here.

## [01]-[INDEX]

- [01]-[RECOVERY_AXIS]: per-engine recovery-profile axis, backup-kind family, and the declared objective.
- [02]-[RECOVERY_DRILL]: the fence-restore-reopen drill cycle stamping measured RTO and the recovery-fact fold.

## [02]-[RECOVERY_AXIS]

- Owner: `RecoveryProfile` the `[SmartEnum<string>]` per-engine recovery-strategy axis under the `StoreKeyPolicy` ordinal accessor; `BackupKind` the `[Union]` backup-shape family carrying the per-shape verify-or-execute discriminant; the declared `(Rpo, Rto)` target is the AppHost-owned `RecoveryObjective` consumed off `ResolvedProfile.Recovery`, never declared here; `ObjectResidence` the object-store durability columns with `LegalHold`; `RecoveryFact` with `RecoveryFactKind` the page-wide fact stream every arm emits onto.
- Cases: `RecoveryProfile` sqlite-paged | pg-replication | object-residence | file-snapshot; `BackupKind` `Paged` (sqlite raw paged copy), `Replication` (PG WAL slot + archive, verify-only), `ObjectTransfer` (cross-region SSE-KMS copy), `Snapshot` (content-addressed sealed snapshot) on the union; `RecoveryFactKind` backup-step | backup-complete | replication-lag | pitr-window | transfer-step | drill-rto | objective-breach.
- Entry: `public static RecoveryObjective Objective(ResolvedProfile host)` is the pure projection `host.Recovery` — the per-modality `(Rpo, Rto)` window is the AppHost `HostProfile.Recovery` column the runtime owns (a web service targets a tighter RPO/RTO than a Rhino plugin), surfaced on `ResolvedProfile` and read here as settled vocabulary, while `RecoveryProfile` selects the per-engine strategy that meets it; `public static Fin<RecoveryFact> VerifyReplication(RecoveryObjective objective, FrozenDictionary<string, string> observed, NpgsqlLogSequenceNumber applied, NpgsqlLogSequenceNumber durable, Duration lag, ClockPolicy clocks)` folds the PG replication state into one fact, `Fin.Fail` when the slot lag exceeds the objective's `Rpo` window; `public static IO<Seq<RecoveryFact>> Backup(RecoveryProfile profile, BackupKind kind, ClockPolicy clocks, ...)` dispatches the engine arm to the sqlite paged session or the object transfer.
- Auto: the PG arm never executes a backup — it verifies the replication GUCs (`wal_level=logical`, `archive_mode=on`, the slot's `restart_lsn`/`confirmed_flush_lsn`) read-only through `ClusterConfig.Verify` and reads the slot lag as `durable - applied` `NpgsqlLogSequenceNumber` distance through the subscription-stats read view, so a PITR window is the WAL-archive retention span gauged against the `Rpo` rather than a managed `pg_basebackup` spawn (the engine and its archiver own the bytes); the sqlite arm drives the settled `SqliteMaintenance.Backup` paged session so a hot embedded store backs up under concurrent writes with per-`BackupStepPages` progress facts and a terminal complete fact; the object arm carries `ObjectResidence(Bucket, Region, StorageClass, Sse, LegalHold, ReplicationRule)` and runs a cross-region `ObjectTransfer` through the `Store/remote#OBJECT_STORE` `BlobRemote` contract so an off-site copy is SSE-KMS encrypted and its transfer evidence folds a `TransferReceipt`; the file-snapshot arm reuses the `Version/snapshots#SNAPSHOT_PROTOCOL` sealed content-addressed write so a point-in-time snapshot is one catalog row; every arm's evidence is a `RecoveryFact` row discriminated by `RecoveryFactKind` so three-plus per-engine recovery buckets fold into one stream with slot/kind metadata rather than parallel construction sites.
- Receipt: every backup or verify folds a `RecoveryFact` row into the open receipt's proof rows feeding `StoreFact`/`StoreEvidence`; the typed `RestoreReceipt` (`#RECOVERY_DRILL`) and `TransferReceipt` carry the algorithm evidence and never degrade to a generic receipt; an objective breach (measured RPO past `Rpo` or measured RTO past `Rto`) emits the `objective-breach` fact and a typed recovery fault, never silent.
- Packages: Npgsql, Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, AWSSDK.S3, Azure.Storage.Blobs, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new engine recovery strategy is one `RecoveryProfile` row; a new backup shape is one `BackupKind` case; a new objective dimension is one column on `RecoveryObjective`; a new evidence bucket is one `RecoveryFactKind` row; a new object-store durability column is one column on `ObjectResidence`; zero new surface — a second backup tool, a parallel restore receipt, or a per-engine evidence record is the deleted form.
- Boundary: the PG arm is verification-only — runtime `pg_basebackup`, `ALTER SYSTEM`, or a spawned backup binary is the rejected form, the engine's WAL archiver owns the bytes and this owner gauges the slot lag and PITR window read-only through `ClusterConfig.Verify` and the subscription-stats read view, so a Rasm process never spawns or bundles a PG backup tool; the sqlite arm composes the settled `SqliteMaintenance.Backup` paged session and never re-implements the raw backup loop; the object arm composes the settled `BlobRemote` contract and the `Store/remote#MULTIPART_TRANSFER` window and never a second object-store client; the `RecoveryObjective` DR target is the AppHost `HostProfile.Recovery` per-modality column projected onto `ResolvedProfile.Recovery`, read through the `Version/recovery ← Rasm.AppHost/Runtime # [PORT]: ResolvedProfile DR-objective inputs` seam as settled vocabulary — `Recovery.Objective` is the pure `host.Recovery` projection and the per-engine arms gauge their measured RPO/RTO against it, so a host-band-keyed `(Rpo, Rto)` switch on this side is the deleted form and a DR target is never minted locally; the `LegalHold` and retention-class columns ride the `Version/retention` classification so a held backup is `HeldOverBudget` rather than evicted, never a second retention taxonomy; `SyncCursor.Lsn` is the one WAL watermark — the replication lag reads the same LSN the `Sync/collaboration#TRANSPORT_AXIS` pump acknowledges, never a second cursor; the `RecoveryFact` stream is the one recovery-evidence surface every engine arm emits onto so downstream observability and drill verification read it without re-learning per-engine receipt shapes, and a parallel per-engine evidence record is the deleted form.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class RecoveryFactKind {
    public static readonly RecoveryFactKind BackupStep = new("backup-step");
    public static readonly RecoveryFactKind BackupComplete = new("backup-complete");
    public static readonly RecoveryFactKind ReplicationLag = new("replication-lag");
    public static readonly RecoveryFactKind PitrWindow = new("pitr-window");
    public static readonly RecoveryFactKind TransferStep = new("transfer-step");
    public static readonly RecoveryFactKind DrillRto = new("drill-rto");
    public static readonly RecoveryFactKind ObjectiveBreach = new("objective-breach");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class RecoveryProfile {
    public static readonly RecoveryProfile SqlitePaged = new("sqlite-paged", executes: true);
    public static readonly RecoveryProfile PgReplication = new("pg-replication", executes: false);
    public static readonly RecoveryProfile ObjectResidence = new("object-residence", executes: true);
    public static readonly RecoveryProfile FileSnapshot = new("file-snapshot", executes: true);

    public bool Executes { get; }
}

public sealed record ObjectResidence(
    string Bucket, string Region, string StorageClass, string Sse, bool LegalHold, Option<string> ReplicationRule);

public readonly record struct RecoveryFact(
    RecoveryFactKind Kind, string Slot, long Count, long Bytes, Duration Elapsed, Duration Measured, Instant At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BackupKind {
    private BackupKind() { }

    public sealed record Paged(string Source, string Destination, SqliteMaintenancePolicy Policy) : BackupKind;
    public sealed record Replication(string Slot, string Publication) : BackupKind;
    public sealed record ObjectTransfer(ObjectResidence Residence, UInt128 ContentKey) : BackupKind;
    public sealed record Snapshot(string Directory, SnapshotCodec Codec, CompressionPolicy Compression) : BackupKind;
}

public static class Recovery {
    public static RecoveryObjective Objective(ResolvedProfile host) => host.Recovery;

    public static Fin<RecoveryFact> VerifyReplication(
        RecoveryObjective objective, FrozenDictionary<string, string> observed,
        NpgsqlLogSequenceNumber applied, NpgsqlLogSequenceNumber durable, Duration lag, ClockPolicy clocks) =>
        ClusterConfig.Verify([("wal_level", "logical", "logical"), ("archive_mode", "on", "on")], observed)
            .Bind(_ => objective.MeetsRpo(lag)
                ? Fin.Succ(new RecoveryFact(RecoveryFactKind.ReplicationLag, durable.ToString(), 0, (long)(durable - applied), Duration.Zero, lag, clocks.Now))
                : Fin.Fail<RecoveryFact>(Error.New($"<replication-rpo-breach:{lag}>")));

    public static IO<Seq<RecoveryFact>> Backup(
        RecoveryProfile profile, BackupKind kind, ClockPolicy clocks,
        Func<BackupKind.Paged, IO<Seq<SqliteFact>>> paged,
        Func<BackupKind.ObjectTransfer, IO<TransferReceipt>> transfer,
        Func<BackupKind.Snapshot, IO<RecoveryFact>> snapshot) =>
        kind.Switch(
            state: (Paged: paged, Transfer: transfer, Snapshot: snapshot),
            paged: static (s, step) => s.Paged(step).Map(facts => facts.Map(f =>
                new RecoveryFact(f.Kind == SqliteFactKind.Backup && f.After.IsSome ? RecoveryFactKind.BackupComplete : RecoveryFactKind.BackupStep,
                    f.Slot, f.Count, f.Bytes, f.Elapsed, Duration.Zero, f.At))),
            objectTransfer: static (s, hop) => s.Transfer(hop).Map(receipt =>
                Seq(new RecoveryFact(RecoveryFactKind.TransferStep, receipt.Destination, receipt.Objects, receipt.Bytes, receipt.Elapsed, Duration.Zero, receipt.At))),
            snapshot: static (s, snap) => s.Snapshot(snap).Map(Seq),
            replication: static (_, _) => IO.pure(Seq<RecoveryFact>()));
}
```

## [03]-[RECOVERY_DRILL]

- Owner: `RestoreReceipt` the typed restore evidence (at `Version/snapshots#RESTORE_AND_DIFF`); `TransferReceipt` the typed cross-region transfer evidence; `RecoveryDrill` the static surface running the fence-restore-reopen cycle, stamping the measured RTO, and folding the drill evidence into the `RecoveryFact` stream.
- Entry: `public static IO<Fin<RecoveryFact>> Run(RecoveryObjective objective, StoreProfile target, StorePlacement placement, BackupKind source, Func<IO<StoreOpenReceipt>> restore, ClockPolicy clocks)` — marks the clock, runs the settled `StoreCeremony.Restore` fence-materialize-swap-reopen cycle against the backup `source`, stamps the elapsed as the measured RTO, and folds a `drill-rto` `RecoveryFact` that is `Fin.Fail` when the measured RTO exceeds the objective's `Rto`.
- Receipt: a drill rides the `drill-rto` `RecoveryFact` carrying the measured RTO `Duration`; the underlying restore folds the typed `RestoreReceipt` (source id, verified hash, target profile, elapsed, instant) and a cross-region transfer folds the typed `TransferReceipt`; an RTO breach is a typed recovery fault folded onto the same stream, never silent.
- Packages: Microsoft.EntityFrameworkCore.Sqlite, Npgsql, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new drill stage is one step on the `StoreCeremony.Restore` choreography (owned there); a new measured dimension is one column on `RecoveryFact`; zero new surface — a second restore choreography or a parallel drill receipt is the deleted form because the drill composes the settled `StoreCeremony.Restore` and stamps RTO onto the one `RecoveryFact` stream.
- Boundary: the drill composes the settled `Store/profiles#STORE_LIFECYCLE` `StoreCeremony.Restore` — it never re-implements the fence-verify-materialize-sidecar-rename-epoch-bump-reopen choreography, it only times the cycle and grades the measured RTO against the objective; the measured RTO is `clocks.Elapsed(mark)` so the drill rides the one `ClockPolicy` seam and never `Stopwatch`; a drill against the PG arm reopens a standby from the verified replication state rather than a managed restore so the PG drill stays verification-plus-failover-time-measurement, never a managed `pg_restore`; the `RestoreReceipt`/`TransferReceipt` stay typed algorithm receipts carrying recovery evidence and never collapse to a generic `IReceipt`; the drill cadence rides the persistence-maintenance `ScheduleEntry` row under the maintenance lease so a periodic restore-drill stamps a `RecoveryFact` the workspace audits rather than an unverified durability assumption.

```csharp signature
public sealed record TransferReceipt(
    string Source, string Destination, long Objects, long Bytes, Duration Elapsed, Instant At, CorrelationId Correlation);

public static class RecoveryDrill {
    public static IO<Fin<RecoveryFact>> Run(
        RecoveryObjective objective, StoreProfile target, StorePlacement placement, BackupKind source,
        Func<IO<StoreOpenReceipt>> restore, ClockPolicy clocks) =>
        IO.lift(clocks.Mark).Bind(mark =>
            restore().Map(reopened => {
                var measured = clocks.Elapsed(mark);
                return objective.MeetsRto(measured)
                    ? Fin.Succ(new RecoveryFact(RecoveryFactKind.DrillRto, target.Key, reopened.MigrationsApplied, 0, measured, measured, clocks.Now))
                    : Fin.Fail<RecoveryFact>(Error.New($"<drill-rto-breach:{target.Key}:{measured}>"));
            }));
}
```

| [INDEX] | [ENGINE]          | [STRATEGY]                                    | [EVIDENCE]                                          |
| :-----: | :---------------- | :-------------------------------------------- | :------------------------------------------------- |
|  [01]   | sqlite-embedded   | `SqliteMaintenance.Backup` paged copy         | per-`BackupStepPages` `RecoveryFact` step + complete |
|  [02]   | postgres-server   | replication-slot + WAL-archive, verify-only   | `ClusterConfig.Verify` + slot lag vs `Rpo`         |
|  [03]   | object-store      | cross-region SSE-KMS `ObjectTransfer`         | typed `TransferReceipt` + `LegalHold`              |
|  [04]   | file-snapshot     | sealed content-addressed snapshot             | `Version/snapshots` catalog row                    |

## [04]-[RESEARCH]

- [PG_REPLICATION_PROBE]: the live-PG18 replication readiness round-trip — the `pg_stat_replication`/`pg_replication_slots` `restart_lsn`/`confirmed_flush_lsn`/`active` columns and the subscription-stats lag the `VerifyReplication` fold reads against a configured slot, the `archive_mode`/`wal_level`/`archive_command` GUC fragments verified read-only, and the PITR-window span gauged against the `Rpo`, proven against a live standby before the verification fold pins.
- [DRILL_RTO_MEASUREMENT]: the measured failover-and-reopen RTO of the `StoreCeremony.Restore` cycle against a staged sqlite backup and a PG standby promotion — whether the fence-materialize-swap-reopen cycle's elapsed is a stable RTO measurement under concurrent load and the standby-promotion time the PG drill stamps, measured before the `RecoveryObjective` `Rto` defaults finalize.
