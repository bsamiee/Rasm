# [PERSISTENCE_TRANSACTION]

Rasm.Persistence makes isolation, MVCC locking, savepoints, two-phase commit, and deadlock classification one closed concurrency axis on the existing `StoreOp` surface rather than a parallel concurrency error family. `TxnScope` is the transaction-shape record over an `IsolationPolicy` `[SmartEnum<string>]` crossed with a `LockMode` `[Union]` plus a `Savepoint` ladder; it threads through `StoreRail.Run` as an arm-family on `StoreOp<T>`, projects `StoreProfile.ConcurrencyToken` into the dormant `StoreFault.Concurrency` 7001 rail, classifies the SQLSTATE 40001 serialization-failure and 40P01 deadlock states into the shared `ConflictVerdict` the `EnableRetryOnFailure` owner consumes, and folds the 2PC arm through raw `PREPARE TRANSACTION`/`COMMIT PREPARED`/`ROLLBACK PREPARED`. `Npgsql`, `Microsoft.Data.Sqlite`, and `linq2db.EntityFrameworkCore` carry the per-engine transaction surfaces; the `StoreOp` 8-case surface, `StoreRail.Run`, `StoreFault`, `StoreProfile.ConcurrencyToken`, and `EnableRetryOnFailure` arrive settled. This owner projects the 2PC in-doubt set through `TwoPhase.InDoubt`; the AppHost drain conductor consumes that projection and resolves it on recovery.

Wire posture: this page is host-local — transaction scope and lock mode are provider statements executed server-side or on the embedded connection, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The 2PC in-doubt set crosses to the AppHost drain conductor through the `Query/transaction ← Rasm.AppHost/Runtime # [PORT]: drain 2PC in-doubt set` seam as a settled identifier set, never a client-facing projection.

## [01]-[INDEX]

- [01]-[TXN_SCOPE]: isolation axis, lock-mode family, savepoint ladder, and the `StoreOp` arm-family.
- [02]-[SQLSTATE_CLASSIFIER]: the 40001/40P01 verdict fold feeding the retry owner and `StoreFault.Concurrency`.
- [03]-[TWO_PHASE_COMMIT]: the prepare/commit-prepared fold and the in-doubt-set drain reconciliation.

## [02]-[TXN_SCOPE]

- Owner: `IsolationPolicy` the `[SmartEnum<string>]` isolation-level axis under the `StoreKeyPolicy` ordinal accessor carrying its `System.Data.IsolationLevel` projection; `LockMode` the `[Union]` lock-request family; `Savepoint` the named nested-rollback ladder record; `TxnScope` the transaction-shape record threading isolation, lock mode, and savepoints; `Transactions` the static surface owning the scoped bracket that runs a `StoreOp<T>` shape on a transaction-bearing leased context under the same `CreateExecutionStrategy` discipline `StoreRail.Run` uses.
- Cases: `IsolationPolicy` read-committed | repeatable-read | serializable | snapshot; `LockMode` `RowShare` (`FOR SHARE`), `RowExclusive` (`FOR UPDATE`), `SkipLocked` (`FOR UPDATE SKIP LOCKED`), `NoWait` (`FOR UPDATE NOWAIT`), `Advisory` (`pg_advisory_xact_lock`) on the union; a `Savepoint` is a named mark a failed inner op rolls back to without aborting the enclosing transaction.
- Entry: `public static IO<T> Scoped<TDb, T>(PooledDbContextFactory<TDb> contexts, TxnScope scope, StoreOp<T> op, InterceptPolicy policy, ClockPolicy clocks, DeadlineClass deadline)` — leases one pooled context, opens one begin-explicit transaction on it at the scope's `IsolationLevel`, establishes the named `Savepoint` marks, runs the `StoreOp<T>` shape on the same transaction-bearing context inside the `CreateExecutionStrategy` retry closure, and commits whole (or aborts on an uncaught fault); a composed op rolls back to an established mark through `RollbackToSavepointAsync` mid-chain so a failed inner leg restates without aborting the enclosing transaction; `LockMode.Clause()` is the `[Union]` `.Switch` projecting the row-lock SQL suffix a `Query` arm appends.
- Auto: the transaction begins at the declared `IsolationPolicy.IsolationLevel` through `BeginTransactionAsync` so a serializable PG transaction reads a consistent snapshot and sqlite maps the serializable request to its exclusive/IMMEDIATE acquisition (so a deferred read that later writes never burns the busy budget on a stale-snapshot BUSY); the `LockMode` clause appends to the `Query` arm's projection so a `SKIP LOCKED` work-queue dequeue is `SELECT ... FOR UPDATE SKIP LOCKED` pushed into SQL, never a client-side claim race, and the `Advisory` arm folds `pg_advisory_xact_lock` so a cross-row coordination lock releases on transaction end; the savepoint ladder establishes the named `Savepoint` marks through `CreateSavepointAsync` (EF `AutoSavepointsEnabled` adding an implicit per-save mark) so a composed op rolls back to a mark through `RollbackToSavepointAsync` while the enclosing transaction survives; the begin-savepoints-op-commit closure runs inside the `CreateExecutionStrategy` callback so a retry re-runs the whole closure; the database stays excluded from the AppHost hop law — the retry owner is the profile-row `EnableRetryOnFailure` and sqlite busy-retry, never a second retry surface.
- Receipt: a transaction commit rides the existing `StoreFact.Transaction` (`store.transaction`) carrying the isolation level and elapsed; a savepoint release/rollback rides `store.transaction.savepoint`; the `StoreInterceptor.TransactionCommittedAsync` hook already stamps the commit fact so this owner adds the isolation and savepoint slots, never a second interceptor.
- Packages: Npgsql, Microsoft.Data.Sqlite, linq2db.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Sqlite, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new isolation level is one `IsolationPolicy` row; a new lock request is one `LockMode` case; a new savepoint concern is one column on `Savepoint`; the 2PC arm is the `Prepared` `TxnScope` flag, never a parallel transaction surface; zero new surface — a transaction-per-entity service, a second isolation enum, or a parallel concurrency error family is the deleted form.
- Boundary: `TxnScope` is an arm-family over the existing `StoreOp` surface, never a ninth `StoreOp` case and never a transaction service — the scoped bracket leases one context, begins the transaction at the scope's isolation, establishes the savepoint marks, and runs the `StoreOp<T>` shape on that same transaction-bearing context inside the `CreateExecutionStrategy` retry closure (the same execution-strategy discipline `StoreRail.Run` applies), so a new transaction posture is a `TxnScope` field, not a new owner; `IsolationPolicy.IsolationLevel` projects the `System.Data.IsolationLevel` the provider's `BeginTransaction` takes so the isolation level is data, never a per-call literal; the `LockMode` clause is pushed into the SQL `FOR UPDATE`/`FOR SHARE`/`SKIP LOCKED`/`NOWAIT` suffix so locking is server-side and a client-side claim loop is the deleted form, and `SKIP LOCKED` is the one work-queue-dequeue mechanism rather than an advisory-lock poll; the savepoint ladder reuses EF `AutoSavepointsEnabled` and explicit named marks so a hand-rolled nested-transaction emulation is the deleted form; the concurrency token projects `StoreProfile.ConcurrencyToken` (`xmin` on PG, `version` on sqlite) into `StoreFault.Concurrency` 7001 at the one `StoreFault.From` site so optimistic-concurrency violations route to the dormant 7001 rail rather than degrading to 7000 `Text`; the 2PC `Prepared` arm is the only cross-store-commit mechanism and reconciles the in-doubt set through the AppHost drain port, never a managed XA coordinator.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class IsolationPolicy {
    public static readonly IsolationPolicy ReadCommitted = new("read-committed", IsolationLevel.ReadCommitted);
    public static readonly IsolationPolicy RepeatableRead = new("repeatable-read", IsolationLevel.RepeatableRead);
    public static readonly IsolationPolicy Serializable = new("serializable", IsolationLevel.Serializable);
    public static readonly IsolationPolicy Snapshot = new("snapshot", IsolationLevel.Snapshot);

    public IsolationLevel IsolationLevel { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LockMode {
    private LockMode() { }

    public sealed record RowShare : LockMode;
    public sealed record RowExclusive : LockMode;
    public sealed record SkipLocked : LockMode;
    public sealed record NoWait : LockMode;
    public sealed record Advisory(long Key) : LockMode;

    public string Clause() =>
        this switch {
            RowShare => " FOR SHARE",
            RowExclusive => " FOR UPDATE",
            SkipLocked => " FOR UPDATE SKIP LOCKED",
            NoWait => " FOR UPDATE NOWAIT",
            Advisory a => $" /* pg_advisory_xact_lock({a.Key}) */",
            _ => string.Empty,
        };
}

public sealed record Savepoint(string Name);

public sealed record TxnScope(IsolationPolicy Isolation, Option<LockMode> Lock, Seq<Savepoint> Savepoints, bool Prepared, Option<string> GlobalTxnId) {
    public static readonly TxnScope Default = new(IsolationPolicy.ReadCommitted, None, Seq<Savepoint>(), Prepared: false, None);

    public static TxnScope Serializable(LockMode lockMode) =>
        new(IsolationPolicy.Serializable, Some(lockMode), Seq<Savepoint>(), Prepared: false, None);
}

public static class Transactions {
    public static IO<T> Scoped<TDb, T>(
        PooledDbContextFactory<TDb> contexts, TxnScope scope, StoreOp<T> op,
        InterceptPolicy policy, ClockPolicy clocks, DeadlineClass deadline) where TDb : DbContext where T : notnull =>
        IO.liftAsync(env => contexts.CreateDbContextAsync(env.Token)).Bracket(
            Use: db => IO.liftAsync(env => db.Database.CreateExecutionStrategy().ExecuteAsync(
                (Db: (DbContext)db, Scope: scope, Shape: Shape(op)),
                static async (state, ct) => {
                    await using var txn = await state.Db.Database.BeginTransactionAsync(state.Scope.Isolation.IsolationLevel, ct);
                    var value = await Saved(state.Db, state.Scope.Savepoints, state.Shape, ct);
                    await txn.CommitAsync(ct);
                    return value;
                },
                env.Token)).Timeout(deadline.Allotted.ToTimeSpan()),
            Catch: static error => IO.fail<T>(StoreFault.From(error)),
            Fin: static db => IO.liftVAsync<Unit>(async _ => { await db.DisposeAsync(); return unit; }));

    private static Func<DbContext, CancellationToken, ValueTask<T>> Shape<T>(StoreOp<T> op) where T : notnull =>
        op.Switch<Func<DbContext, CancellationToken, ValueTask<T>>>(
            get: static g => g.Shape, query: static q => q.Shape, stream: static s => s.Shape,
            aggregate: static a => a.Shape, upsert: static u => u.Shape, delete: static d => d.Shape,
            maintain: static m => m.Shape, bulk: static b => async (db, ct) => (await b.Shape(db, ct)).Value);

    private static async ValueTask<T> Saved<T>(
        DbContext db, Seq<Savepoint> savepoints, Func<DbContext, CancellationToken, ValueTask<T>> shape, CancellationToken token) {
        foreach (var mark in savepoints)
            await db.Database.CreateSavepointAsync(mark.Name, token);
        return await shape(db, token);
    }
}
```

## [03]-[SQLSTATE_CLASSIFIER]

- Owner: `ConcurrencyClass` the SQLSTATE-to-verdict classifier reusing the `Sync/collaboration#MERGE_LAW` `ConflictVerdict` so deadlock classification and merge resolution speak one verdict vocabulary; the classifier feeds the `StoreFault.Concurrency` 7001 rail and the `EnableRetryOnFailure` owner.
- Entry: `public static ConflictVerdict Classify(string sqlState)` — folds the SQLSTATE class into the shared verdict: 40001 serialization-failure and 40P01 deadlock-detected classify to `Rejected` (the retry-eligible verdict the strategy absorbs), 23505 unique-violation under optimistic concurrency classifies to `RemoteWin`, and every other state defaults to `LocalWin` — the non-retry, hold-local verdict — so a non-concurrency fault is never retry-eligible (`Retryable` admits only `Rejected`) and the fault surfaces through its own `StoreFault` arm rather than the concurrency rail.
- Auto: the classifier reads `PostgresException.SqlState` matched against the `PostgresErrorCodes.SerializationFailure` (40001) and `PostgresErrorCodes.DeadlockDetected` (40P01) constants — the only two classes a store-level strategy absorbs silently — and the `StoreProfile.ConcurrencyToken` violation (`DbUpdateConcurrencyException`) folds to the same verdict family so the existing `StoreFault.From` `Concurrency` 7001 arm carries the verdict; the verdict is the `EnableRetryOnFailure` consumer's signal so a 40001/40P01 retries inside the execution strategy while a 23505 surfaces the held-row conflict, and the shared `ConflictVerdict` means the `Sync/collaboration` merge fold and this deadlock fold never diverge.
- Receipt: a classified concurrency fault rides `StoreFault.Concurrency` 7001 carrying the SQLSTATE and the verdict; a retried serialization failure rides `store.transaction.retry` on the existing interceptor stream, never a second signal owner.
- Packages: Npgsql, Microsoft.Data.Sqlite, LanguageExt.Core.
- Growth: a new retry-eligible SQLSTATE is one arm on `Classify`; zero new surface — a parallel deadlock error family, a second retry classifier, or a sync-divergent verdict enum is the deleted form because the classifier reuses the one `ConflictVerdict` and feeds the one `StoreFault.Concurrency` rail.
- Boundary: the classifier reuses the `Sync/collaboration#MERGE_LAW` `ConflictVerdict` so the deadlock-classification and merge-resolution verdicts are one vocabulary — a sync-side outcome enum and a transaction-side deadlock enum diverging is the deleted form, which is exactly what the `CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE` collapse enables; the only retry-absorbed classes are 40001 and 40P01 per the postgres SQLSTATE-fold law, so an admission defect (foreign-key, check, not-null) or a 22-prefixed data exception is never classified retry-eligible; the verdict feeds the existing `EnableRetryOnFailure` owner and the dormant `StoreFault.Concurrency` 7001 rail, never a second retry surface — AppHost owns the hop law and the database stays excluded from it.

```csharp signature
public static class ConcurrencyClass {
    public static ConflictVerdict Classify(string sqlState) =>
        sqlState switch {
            PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected => ConflictVerdict.Rejected,
            PostgresErrorCodes.UniqueViolation => ConflictVerdict.RemoteWin,
            _ => ConflictVerdict.LocalWin,
        };

    public static bool Retryable(ConflictVerdict verdict) => verdict == ConflictVerdict.Rejected;
}
```

## [04]-[TWO_PHASE_COMMIT]

- Owner: `PreparedTxn` the in-doubt prepared-transaction record; `TwoPhase` the static surface folding `PREPARE TRANSACTION`/`COMMIT PREPARED`/`ROLLBACK PREPARED` and reconciling the in-doubt set the AppHost drain surfaces.
- Entry: `public static IO<Unit> Prepare(DbContext db, string globalTxnId)` runs `PREPARE TRANSACTION '<id>'` so the transaction's work is durable but uncommitted; `public static IO<Unit> Commit(DbContext db, string globalTxnId)` runs `COMMIT PREPARED '<id>'` and `public static IO<Unit> Rollback(DbContext db, string globalTxnId)` runs `ROLLBACK PREPARED '<id>'`; `public static IO<Seq<PreparedTxn>> InDoubt(DbContext db)` reads `pg_prepared_xacts` so the drain reconciles every prepared-but-unresolved transaction.
- Auto: the 2PC arm activates only on the `TxnScope.Prepared` flag carrying a `GlobalTxnId` so a cross-store commit prepares each participant, the coordinator commits-prepared on all-prepared or rolls-back-prepared on any participant abort, and a crash between prepare and the second-phase resolution leaves the transaction in `pg_prepared_xacts` for the drain to reconcile; the in-doubt set crosses to the AppHost drain conductor through the `Query/transaction ← Rasm.AppHost/Runtime # [PORT]: drain 2PC in-doubt set` seam as a Stores-band (300) `DrainParticipantPort` consumer so the recovery reconciliation is the drain owner's concern — the conductor reads this owner's `InDoubt` projection and resolves each prepared transaction through `TwoPhase.Commit`/`TwoPhase.Rollback` per the coordinator's last-recorded second-phase decision while store writes stay open at band 300, never a managed XA transaction manager and never a Persistence-side recovery loop; sqlite carries no 2PC so a `Prepared` scope on the embedded profile is a typed `Unsupported` 7003 rejection at the rail.
- Receipt: a prepared transaction rides `store.transaction.prepared` carrying the global id; a second-phase commit/rollback rides `store.transaction.resolved`; an in-doubt reconciliation rides the AppHost drain receipt, never a second drain owner.
- Packages: Npgsql, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new second-phase resolution is one arm on `TwoPhase`; zero new surface — a managed XA coordinator, a second drain owner, or a parallel prepared-transaction table is the deleted form because 2PC rides the PG-native `PREPARE TRANSACTION` and the in-doubt set drains through the one AppHost drain port.
- Boundary: 2PC is PG-native `PREPARE TRANSACTION`/`COMMIT PREPARED` raw SQL through the typed-SQL door, never a managed `System.Transactions` XA coordinator — the cross-store atomic commit is the prepared-transaction protocol the engine owns and this owner folds; the in-doubt set is reconciled by the AppHost drain conductor through the seam so the recovery half is the drain owner's concern and this owner only projects `InDoubt` from `pg_prepared_xacts`, never a second drain owner; a `Prepared` scope on a non-2PC engine (sqlite) is a typed `StoreFault.Unsupported` 7003 rejection at the rail because the lane-by-profile capability denies the shape, never a silent single-phase fallback; the global transaction id is the one cross-store coordination key the coordinator threads, never a per-participant id.

```csharp signature
public readonly record struct PreparedTxn(string GlobalId, string Owner, Instant Prepared);

public static class TwoPhase {
    public const string InDoubtProbe = "SELECT gid, owner, prepared FROM pg_prepared_xacts";

    public static IO<Unit> Prepare(DbContext db, string globalTxnId) =>
        IO.liftVAsync<Unit>(async env => { _ = await db.Database.ExecuteSqlRawAsync($"PREPARE TRANSACTION '{globalTxnId}'", env.Token); return unit; });

    public static IO<Unit> Commit(DbContext db, string globalTxnId) =>
        IO.liftVAsync<Unit>(async env => { _ = await db.Database.ExecuteSqlRawAsync($"COMMIT PREPARED '{globalTxnId}'", env.Token); return unit; });

    public static IO<Unit> Rollback(DbContext db, string globalTxnId) =>
        IO.liftVAsync<Unit>(async env => { _ = await db.Database.ExecuteSqlRawAsync($"ROLLBACK PREPARED '{globalTxnId}'", env.Token); return unit; });
}
```

| [INDEX] | [CONCERN]          | [SURFACE]                                       | [LAW]                                                        |
| :-----: | :----------------- | :---------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | isolation          | `IsolationPolicy.IsolationLevel` on begin       | data, never a per-call literal                              |
|  [02]   | row locking        | `LockMode.Clause()` SQL suffix                  | server-side `FOR UPDATE`/`SKIP LOCKED`, never a claim loop   |
|  [03]   | nested rollback    | `Savepoint` ladder + EF `AutoSavepointsEnabled` | inner failure rolls to mark, enclosing txn survives         |
|  [04]   | deadlock classify  | `ConcurrencyClass.Classify` → `ConflictVerdict` | one verdict shared with merge resolution                    |
|  [05]   | cross-store commit | `TwoPhase` `PREPARE`/`COMMIT PREPARED`          | PG-native 2PC, in-doubt set drains through AppHost          |

## [05]-[RESEARCH]

- [TXN_ISOLATION_PROBE]: the live-PG18 `SET TRANSACTION ISOLATION LEVEL` round-trip for the four `IsolationPolicy` rows and the `FOR UPDATE SKIP LOCKED`/`NOWAIT` work-queue-dequeue contract under concurrent dequeuers — whether the `LockMode` clause pushes into the EF `Query` arm's SQL and the SKIP LOCKED skip is exact under contention, proven before the lock-clause fence pins.
- [TWO_PHASE_DRAIN]: the `PREPARE TRANSACTION`/`COMMIT PREPARED`/`pg_prepared_xacts` 2PC round-trip and the in-doubt reconciliation the AppHost drain conductor runs against a crash between prepare and second-phase — the `max_prepared_transactions` GUC floor verified through `ClusterConfig.Verify` and the drain-port in-doubt-set hand-off, proven against a live PG18 server before the 2PC fence pins.
- [SQLSTATE_RETRY_BAND]: the 40001/40P01 retry-eligibility against the `EnableRetryOnFailure` owner's absorbed-class set and the `DbUpdateConcurrencyException` → `StoreFault.Concurrency` 7001 fold, confirming the shared `ConflictVerdict` classifies both the SQLSTATE deadlock and the optimistic-token violation without a sync-divergent enum.
