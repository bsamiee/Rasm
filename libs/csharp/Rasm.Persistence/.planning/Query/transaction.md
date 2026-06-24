# [PERSISTENCE_TRANSACTION]

`Rasm.Persistence` folds isolation, MVCC row/table/advisory locking, savepoint nesting, two-phase commit, and SQLSTATE contention classification into one concurrency axis braced over the settled `StoreOp<T>` surface — never a parallel transaction service, a second isolation enum, or a sync-divergent concurrency rail. `TxnScope` is the transaction-shape record over an `IsolationPolicy` `[SmartEnum<string>]` crossed with a `LockMode` `[Union]` (whose `Row` arm carries the orthogonal `LockStrength` × `WaitPolicy` `RowLock`, whose `Advisory` arm executes a real `pg_advisory_xact_lock`, and whose `Table` arm runs a real `LOCK TABLE`) plus a `Savepoint` ladder and an optional `GlobalTxnId` `[ValueObject<string>]`. `Transactions.Scoped` threads the scope through one `CreateExecutionStrategy` retry closure: it leases one pooled context, begins one explicit transaction at the scope's per-engine begin, acquires the declared advisory/table locks as real statements, establishes the named savepoint marks, runs the `StoreOp<T>` shape, rolls a failed inner leg back to its mark without aborting the enclosing transaction, and commits whole; the write arms are idempotent by construction (`RETURNING` upserts, fenced reject-lower CAS) so an ambiguous-commit retry replays a closed value rather than double-applying a delta. `ConcurrencyClass` folds the SQLSTATE contention classes — `40001` serialization-failure and `40P01` deadlock-detected absorbed silently by the strategy, `55P03`/`55006` `NOWAIT` lock-unavailable and `25P02` aborted-statement surfaced — into one typed `RetryDisposition` the execution strategy and the `StoreFault` rail consume; the EF tracked-graph `DbUpdateConcurrencyException` optimistic-token violation surfaces through `StoreFault.Concurrency` 7001 and is never retried (a lost update no retry can fix), and the `Sync/coordination#FENCED_CAS` `xmin` reject-lower runs its check in-process to `CoordFault.Stale` on a path that bypasses `SaveChangesAsync` entirely, so it never enters this classifier. `TwoPhase` folds `PREPARE TRANSACTION` plus the one polymorphic `Resolve` over a `TwoPhaseDecision` (`COMMIT PREPARED`/`ROLLBACK PREPARED`) and projects the `pg_prepared_xacts` in-doubt set the AppHost drain conductor reconciles. The `StoreOp` 8-case surface, `StoreRail.Run`, `StoreFact`, `StoreFault`, `StoreProfile.ConcurrencyToken`, `EnableRetryOnFailure`, and `ClockPolicy` arrive settled; `Npgsql`, `Microsoft.Data.Sqlite`, and `linq2db.EntityFrameworkCore` carry the per-engine transaction surfaces.

Wire posture: this page is host-local — transaction scope and lock mode are provider statements executed server-side on PostgreSQL or on the embedded SQLite connection, crossing no browser or peer wire, so it mints no `TS_PROJECTION` cluster. The 2PC in-doubt set crosses to the AppHost drain conductor through the `Query/transaction ← Rasm.AppHost/Runtime # [PORT]: drain 2PC in-doubt set` seam as a settled `PreparedTxn` set, never a client-facing projection.

## [01]-[INDEX]

- [01]-[TXN_SCOPE]: the isolation axis, the orthogonal lock family, the savepoint ladder, the `GlobalTxnId` value object, and the `Transactions.Scoped` bracket over `StoreOp<T>`.
- [02]-[SQLSTATE_CLASSIFIER]: the `40001`/`40P01` retry-absorb plus the `55P03`/`55006`/`25P02` lock-and-abort surface fold to `RetryDisposition`, feeding the execution strategy and the `StoreFault` rail.
- [03]-[TWO_PHASE_COMMIT]: the prepare and the one polymorphic `Resolve` over `TwoPhaseDecision`, the `pg_prepared_xacts` in-doubt reader, and the AppHost drain reconciliation.

## [02]-[TXN_SCOPE]

- Owner: `IsolationPolicy` the `[SmartEnum<string>]` isolation-level axis under the `StoreKeyPolicy` ordinal accessor, carrying its `System.Data.IsolationLevel` projection, its retry-eligibility flag, and its per-engine `Begin` projection; `LockStrength` and `WaitPolicy` the two orthogonal `[SmartEnum<string>]` row-lock axes; `RowLock` the `[ComplexValueObject]` crossing strength × wait-policy × target relation; `AdvisoryLock` the `[ComplexValueObject]` PG advisory-lock request; `LockMode` the `[Union]` lock-request family (`Row` | `Advisory` | `Table`); `Savepoint` the named nested-rollback ladder record; `GlobalTxnId` the `[ValueObject<string>]` 2PC coordination key; `TxnScope` the transaction-shape record threading isolation, lock mode, savepoints, the prepared flag, and the global id; `Transactions` the static surface owning the scoped bracket that runs a `StoreOp<T>` shape on a transaction-bearing leased context under the same `CreateExecutionStrategy` discipline `StoreRail.Run` uses.
- Cases: `IsolationPolicy` read-committed | snapshot (PG MVCC snapshot isolation IS `IsolationLevel.RepeatableRead`, the only level PG accepts beyond read-committed and serializable) | serializable | serializable-deferred; `LockStrength` `Update` (`FOR UPDATE`), `NoKeyUpdate` (`FOR NO KEY UPDATE`), `Share` (`FOR SHARE`), `KeyShare` (`FOR KEY SHARE`); `WaitPolicy` `Wait` | `SkipLocked` (`SKIP LOCKED`) | `NoWait` (`NOWAIT`); `LockMode` `Row(RowLock)` | `Advisory(AdvisoryLock)` | `Table(TableLockMode, Seq<string>)` with the `Row.ForUpdate`/`ForShare` and `Advisory.Coord`/`Table.Of` named factories so a consumer spells a lock by intent rather than constructing the union arm raw; a `Savepoint` is a named mark a failed inner op rolls back to without aborting the enclosing transaction.
- Entry: `public static IO<T> Scoped<TDb, T>(PooledDbContextFactory<TDb> contexts, TxnScope scope, StoreOp<T> op, InterceptPolicy policy, ClockPolicy clocks, DeadlineClass deadline)` — leases one pooled context, opens one explicit transaction at the scope's `IsolationPolicy.Begin` (a serializable PG transaction reads a consistent snapshot; SQLite maps serializable to `BEGIN IMMEDIATE` and snapshot/read-committed to deferred), acquires the declared `LockMode` server-side (the `Advisory` arm runs `SELECT pg_advisory_xact_lock($1)` so a cross-row coordination lock releases on transaction end, the `Table` arm runs `LOCK TABLE … IN <mode> MODE`, the `Row` clause appends to the `Query` arm's projection), establishes the named `Savepoint` marks, runs the `StoreOp<T>` shape, rolls a failed inner leg back to its mark through `RollbackToSavepointAsync` while the enclosing transaction survives, and commits whole; the whole begin-lock-savepoints-op-commit closure runs inside the `CreateExecutionStrategy` callback so a serialization-failure retry re-runs the closed `TState` value, never a captured closure, and the `verifySucceeded: null` probe is correct because the `StoreOp` write arms are idempotent by construction (the `RETURNING` upserts and the `Sync/coordination#FENCED_CAS` reject-lower CAS re-run to the identical committed state), so an ambiguous-commit retry replays a closed idempotent value rather than double-applying a delta — the doctrine's `verifySucceeded` mandate is satisfied by op idempotency, not a bracket-level re-check the generic surface cannot author.
- Auto: the transaction begins through `IsolationPolicy.Begin(db, ct)` so PostgreSQL takes the declared `BeginTransactionAsync(IsolationLevel)` (`snapshot` is `IsolationLevel.RepeatableRead` because PG's MVCC snapshot isolation IS repeatable-read and `IsolationLevel.Snapshot` is the SQL-Server level the Npgsql connector rejects) while SQLite folds serializable to the synchronous `BeginTransaction(IsolationLevel.Serializable, deferred: false)` enrolled into EF through `UseTransactionAsync` (the `BEGIN IMMEDIATE` that reserves the writer up front so a deferred read that later writes never burns the busy budget on a stale-snapshot `SQLITE_BUSY`) and snapshot/read-committed to `deferred: true`; the `serializable-deferred` row begins serializable then runs the raw `SET TRANSACTION READ ONLY DEFERRABLE` on PG so a long analytical read takes a guaranteed-consistent snapshot that never raises a serialization failure and never needs a retry; the `LockMode.Row` clause appends to the `Query` arm's projection so a work-queue dequeue is `SELECT … FOR UPDATE SKIP LOCKED` pushed into SQL, never a client-side claim race, the `LockMode.Advisory` arm folds `pg_advisory_xact_lock`/`pg_try_advisory_xact_lock` (the share variant and the `(int4, int4)` two-key variant its `AdvisoryLock` value selects) as a real executed statement releasing on transaction end, and the `LockMode.Table` arm runs an explicit `LOCK TABLE` for a coarse coordination grant; the savepoint ladder establishes the named marks through `CreateSavepointAsync` (EF `AutoSavepointsEnabled` adding an implicit per-save mark) so a composed op's failed leg rolls back to its mark through `RollbackToSavepointAsync` while the enclosing transaction survives; the database stays excluded from the AppHost hop law — retry is the profile-row `EnableRetryOnFailure` SQLSTATE strategy plus SQLite busy-retry, never a second retry surface.
- Receipt: every transaction signal rides the one settled `StoreFact.Transaction` kind (`store.transaction`) the `StoreInterceptor.TransactionCommittedAsync` hook already stamps — the commit fact's `Subject` carries the isolation level, a savepoint release/rollback carries the mark name, a slow lock-acquisition carries the lock clause, and a strategy retry carries the absorbed SQLSTATE — so the variant lands in the existing `Subject`/`Count` slots of the one fact shape rather than a new dotted kind constant the closed `StoreFact` vocabulary does not carry; this owner adds no second interceptor and mints no parallel fact kind.
- Packages: Npgsql, Microsoft.Data.Sqlite, linq2db.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Sqlite, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new isolation level is one `IsolationPolicy` row carrying its level, retry flag, and per-engine begin; a new row-lock strength is one `LockStrength` row, a new wait policy one `WaitPolicy` row, a new lock kind one `LockMode` case; a new savepoint concern is one column on `Savepoint`; the 2PC arm is the `Prepared` `TxnScope` flag carrying a `GlobalTxnId`; zero new surface — a transaction-per-entity service, a second isolation enum, a flat lock-mode enum that conflates strength with wait policy, or a parallel concurrency error family is the deleted form.
- Boundary: `TxnScope` is an arm-family over the existing `StoreOp` surface, never a ninth `StoreOp` case and never a transaction service — the scoped bracket leases one context, begins the transaction at the scope's per-engine begin, acquires the declared locks server-side, establishes the savepoint marks, and runs the `StoreOp<T>` shape inside the `CreateExecutionStrategy` retry closure, so a new transaction posture is a `TxnScope` field, not a new owner; `IsolationPolicy.IsolationLevel` projects the `System.Data.IsolationLevel` the provider's begin takes so the isolation level is data, never a per-call literal, and `IsolationPolicy.Begin` owns the PG-vs-SQLite begin divergence so the embedded floor's `BEGIN IMMEDIATE`/deferred mapping is a row projection rather than a call-site branch; lock STRENGTH and wait POLICY are orthogonal `RowLock` axes so `FOR SHARE SKIP LOCKED` is one expressible cross rather than a missing flat-enum combination, and the lock clause is pushed into the SQL suffix so a client-side claim loop is the deleted form; the `Advisory` arm executes a real `pg_advisory_xact_lock` statement (a SQL-comment placeholder is the illusory deleted form), `SKIP LOCKED` is the one work-queue-dequeue mechanism rather than an advisory poll, and the advisory lock is the one cross-row coordination mechanism the engine releases on transaction end; the savepoint ladder reuses EF `AutoSavepointsEnabled` and explicit named marks so a hand-rolled nested-transaction emulation is the deleted form (PostgreSQL and SQLite both forbid true nested transactions, so savepoints are the only nesting mechanism); the EF tracked-graph optimistic-token violation (`DbUpdateConcurrencyException`, raised by `SaveChangesAsync` on a `StoreProfile.ConcurrencyToken` mismatch — `xmin` on PG, `version` on SQLite) folds to `StoreFault.Concurrency` 7001 at the one `StoreFault.From` site as a `Surface` disposition because a lost update is never retried, while the `Sync/coordination#FENCED_CAS` reject-lower runs its `xmin` snapshot check in-process to `CoordFault.Stale` on a `MergeWithOutputAsync` path that bypasses `SaveChangesAsync` entirely, so the CAS optimistic check never enters this classifier — conflating the fenced-CAS reject with the EF tracked-graph concurrency fault is the illusory cross-page coupling this owner refuses; the 2PC `Prepared` arm is the only cross-store-commit mechanism and reconciles the in-doubt set through the AppHost drain port, never a managed XA coordinator; the `GlobalTxnId` is a validated `[ValueObject<string>]` because a 2PC gid lands in a `PREPARE TRANSACTION '<id>'` utility statement that takes no bound parameter, so the value is admitted once against a strict charset rather than interpolated raw.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class IsolationPolicy {
    public static readonly IsolationPolicy ReadCommitted = new("read-committed", IsolationLevel.ReadCommitted, Retryable: false, Deferred: true, ReadOnly: false);
    public static readonly IsolationPolicy Snapshot = new("snapshot", IsolationLevel.RepeatableRead, Retryable: true, Deferred: true, ReadOnly: false);
    public static readonly IsolationPolicy Serializable = new("serializable", IsolationLevel.Serializable, Retryable: true, Deferred: false, ReadOnly: false);
    public static readonly IsolationPolicy SerializableDeferred = new("serializable-deferred", IsolationLevel.Serializable, Retryable: false, Deferred: false, ReadOnly: true);

    public IsolationLevel IsolationLevel { get; }
    public bool Retryable { get; }     // snapshot/serializable may raise 40001 the strategy absorbs; PG snapshot isolation IS RepeatableRead
    public bool Deferred { get; }       // SQLite: deferred begin vs the IMMEDIATE writer reservation
    public bool ReadOnly { get; }       // PG: SERIALIZABLE READ ONLY DEFERRABLE consistent read snapshot

    // The PG-vs-SQLite begin divergence as a row projection: PG takes the declared level (the
    // serializable-deferred row sets READ ONLY DEFERRABLE through the raw SET so a consistent read
    // never raises a serialization failure); SQLite has no isolation levels, so the row reduces to the
    // one knob the engine exposes — deferred begin vs the IMMEDIATE writer reservation.
    public async ValueTask<IDbContextTransaction> Begin(DatabaseFacade db, CancellationToken token) {
        // SQLite: begin the ADO SqliteTransaction at the deferred/IMMEDIATE posture, then enroll it
        // in the EF context through UseTransactionAsync so the op shape's SaveChanges shares it; PG
        // takes the declared level directly. A bare ADO begin unenrolled in EF is the deleted form.
        if (db.GetDbConnection() is SqliteConnection sqlite) {
            await db.OpenConnectionAsync(token);
            // BeginTransaction(bool deferred) is the catalogued SQLite overload; the engine has no
            // isolation levels, so the PG IsolationLevel never crosses here (BeginTransaction rejects
            // any level but Serializable/ReadUncommitted) — Deferred is the whole seam, deferred: false
            // is the IMMEDIATE writer reservation. The sync begin enrolls through UseTransactionAsync.
            return await db.UseTransactionAsync(sqlite.BeginTransaction(deferred: Deferred), token);
        }
        var txn = await db.BeginTransactionAsync(IsolationLevel, token);
        if (ReadOnly)
            _ = await db.ExecuteSqlRawAsync("SET TRANSACTION READ ONLY DEFERRABLE", token);
        return txn;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class LockStrength {
    public static readonly LockStrength Update = new("update", " FOR UPDATE");
    public static readonly LockStrength NoKeyUpdate = new("no-key-update", " FOR NO KEY UPDATE");
    public static readonly LockStrength Share = new("share", " FOR SHARE");
    public static readonly LockStrength KeyShare = new("key-share", " FOR KEY SHARE");

    public string Sql { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class WaitPolicy {
    public static readonly WaitPolicy Wait = new("wait", string.Empty);
    public static readonly WaitPolicy SkipLocked = new("skip-locked", " SKIP LOCKED");
    public static readonly WaitPolicy NoWait = new("nowait", " NOWAIT");

    public string Sql { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class TableLockMode {
    public static readonly TableLockMode AccessShare = new("access-share", "ACCESS SHARE");
    public static readonly TableLockMode RowExclusiveTable = new("row-exclusive", "ROW EXCLUSIVE");
    public static readonly TableLockMode ShareTable = new("share", "SHARE");
    public static readonly TableLockMode ExclusiveTable = new("exclusive", "EXCLUSIVE");
    public static readonly TableLockMode AccessExclusive = new("access-exclusive", "ACCESS EXCLUSIVE");

    public string Sql { get; }
}

// Lock STRENGTH and wait POLICY are orthogonal: every (strength, wait) cross is expressible, so a
// `FOR SHARE SKIP LOCKED` is one value rather than a missing flat-enum combination. `Of` names the
// relations the lock binds in a multi-table join (`FOR UPDATE OF queue, claim`) so a lock binds the
// queue tables without locking their joined-but-unlocked rows — empty Of locks every joined relation.
[ComplexValueObject]
public sealed partial class RowLock {
    public LockStrength Strength { get; }
    public WaitPolicy Wait { get; }
    public Seq<string> Of { get; }

    public string Clause() =>
        $"{Strength.Sql}{(Of.IsEmpty ? string.Empty : $" OF {string.Join(", ", Of)}")}{Wait.Sql}";
}

// A real PG advisory lock: the (int4, int4) two-key form, the xact-vs-session scope, the share mode,
// and the try (non-blocking) mode select the exact `pg_advisory*` function. The lock is a SELECT
// executed before the op runs, releasing on transaction end (xact scope) — never a SQL-comment
// placeholder. The two-key form binds the int4 overload from the bigint key's high/low 32-bit halves
// (the canonical PG namespacing idiom), so the same bigint key surface drives both engine overloads
// without a truncating `(int)` narrowing that would silently collide distinct high-half keys.
[ComplexValueObject]
public sealed partial class AdvisoryLock {
    public long Key { get; }
    public bool TwoKey { get; }         // true => the (int4, int4) overload over the key's 32-bit halves
    public bool Shared { get; }
    public bool Try { get; }            // pg_try_advisory* — a contended lock returns false, never blocks

    public (string Sql, object[] Args) Acquire() {
        var fn = (Try, Shared) switch {
            (true, true) => "pg_try_advisory_xact_lock_shared",
            (true, false) => "pg_try_advisory_xact_lock",
            (false, true) => "pg_advisory_xact_lock_shared",
            (false, false) => "pg_advisory_xact_lock",
        };
        return TwoKey
            ? ($"SELECT {fn}({{0}}, {{1}})", [(int)(Key >> 32), unchecked((int)Key)])
            : ($"SELECT {fn}({{0}})", [Key]);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LockMode {
    private LockMode() { }

    public sealed record Row(RowLock Lock) : LockMode;
    public sealed record Advisory(AdvisoryLock Lock) : LockMode;
    public sealed record Table(TableLockMode Mode, Seq<string> Relations) : LockMode;

    public static LockMode ForUpdate(WaitPolicy wait) => new Row(RowLock.Create(LockStrength.Update, wait, Seq<string>()));
    public static LockMode ForShare(WaitPolicy wait) => new Row(RowLock.Create(LockStrength.Share, wait, Seq<string>()));
    public static LockMode Coord(long key, bool shared = false, bool tryOnly = false) =>
        new Advisory(AdvisoryLock.Create(key, TwoKey: false, shared, tryOnly));
    public static LockMode On(TableLockMode mode, params ReadOnlySpan<string> relations) =>
        new Table(mode, [.. relations]);
}

[ValueObject<string>(EqualityComparisonOperators = OperatorsGeneration.Default)]
public sealed partial class GlobalTxnId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Length is > 0 and <= 200 && value.All(static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or ':')
            ? null
            : new ValidationError($"<global-txn-id-invalid:{value}>");   // a gid is a PREPARE TRANSACTION literal, not a bound parameter
}

public sealed record Savepoint(string Name);

public sealed record TxnScope(IsolationPolicy Isolation, Option<LockMode> Lock, Seq<Savepoint> Savepoints, bool Prepared, Option<GlobalTxnId> GlobalTxnId) {
    public static readonly TxnScope Default = new(IsolationPolicy.ReadCommitted, None, Seq<Savepoint>(), Prepared: false, None);

    public static TxnScope Serializable(LockMode lockMode) =>
        new(IsolationPolicy.Serializable, Some(lockMode), Seq<Savepoint>(), Prepared: false, None);

    public static TxnScope ReadSnapshot =>
        new(IsolationPolicy.SerializableDeferred, None, Seq<Savepoint>(), Prepared: false, None);
}

public static class Transactions {
    public static IO<T> Scoped<TDb, T>(
        PooledDbContextFactory<TDb> contexts, TxnScope scope, StoreOp<T> op,
        InterceptPolicy policy, ClockPolicy clocks, DeadlineClass deadline) where TDb : DbContext where T : notnull =>
        IO.liftAsync(env => contexts.CreateDbContextAsync(env.Token)).Bracket(
            Use: db => IO.liftAsync(env => db.Database.CreateExecutionStrategy().ExecuteAsync(
                (Db: (DbContext)db, Scope: scope, Shape: Shape(op, policy)),
                static async (state, ct) => {
                    await using var txn = await state.Scope.Isolation.Begin(state.Db.Database, ct);
                    await Acquire(state.Db, state.Scope.Lock, ct);
                    await Mark(state.Db, state.Scope.Savepoints, ct);
                    var value = await state.Shape(state.Db, ct);
                    await state.Db.Database.CommitTransactionAsync(ct);
                    return value;
                },
                verifySucceeded: null,
                env.Token)).Timeout(deadline.Allotted.ToTimeSpan()),
            Catch: static error => IO.fail<T>(StoreFault.From(error)),
            Fin: static db => IO.liftVAsync<Unit>(async _ => { await db.DisposeAsync(); return unit; }));

    // The arm semantics the rail owns are preserved inside the transaction, never flattened to the bare
    // Shape: the Upsert arm runs SaveChangesAsync, the Delete/Bulk arms replay their Tags invalidation,
    // and the Bulk arm emits its receipt fact — all committed inside the one transaction so the post-commit
    // invalidation and the receipt ride the same boundary the StoreRail.Run dispatch establishes.
    private static Func<DbContext, CancellationToken, ValueTask<T>> Shape<T>(StoreOp<T> op, InterceptPolicy policy) where T : notnull =>
        op.Switch<InterceptPolicy, Func<DbContext, CancellationToken, ValueTask<T>>>(
            state: policy,
            get: static (p, g) => g.Shape, query: static (p, q) => q.Shape,
            stream: static (p, s) => s.Shape, aggregate: static (p, a) => a.Shape,
            upsert: static (p, u) => async (db, ct) => { var value = await u.Shape(db, ct); _ = await db.SaveChangesAsync(ct); return value; },
            delete: static (p, d) => async (db, ct) => { var value = await d.Shape(db, ct); await p.Invalidate(d.Tags, ct); return value; },
            maintain: static (p, m) => m.Shape,
            bulk: static (p, b) => async (db, ct) => { var moved = await b.Shape(db, ct); _ = p.Facts(moved.Receipt.Fact); await p.Invalidate(b.Tags, ct); return moved.Value; });

    private static async ValueTask Acquire(DbContext db, Option<LockMode> mode, CancellationToken token) {
        switch (mode) {
            case { Case: LockMode.Advisory a }:
                var (sql, args) = a.Lock.Acquire();
                _ = await db.Database.ExecuteSqlRawAsync(sql, args, token);
                break;
            // LOCK TABLE takes identifiers, not bound parameters, so the relation list rides quoted
            // interpolation at this one declared identifier seam over the closed relation vocabulary.
            case { Case: LockMode.Table t }:
                _ = await db.Database.ExecuteSqlRawAsync($"LOCK TABLE {string.Join(", ", t.Relations)} IN {t.Mode.Sql} MODE", token);
                break;
            // LockMode.Row appends its clause to the Query arm's SELECT, not a standalone statement.
        }
    }

    private static async ValueTask Mark(DbContext db, Seq<Savepoint> savepoints, CancellationToken token) {
        foreach (var mark in savepoints)
            await db.Database.CreateSavepointAsync(mark.Name, token);
    }
}
```

The savepoint rollback is the composed-op spelling: a `StoreOp<T>.Then` continuation whose leg may fail wraps the inner shape in `RollbackTo` so the failed leg restates to its named mark while the enclosing transaction survives, and a leg that needs the whole-transaction abort simply throws, lifting through the `Bracket` catch into `StoreFault`. `RollbackTo` runs `RollbackToSavepointAsync` and re-runs the recovery continuation under the same execution strategy, never aborting the enclosing transaction — a hand-rolled nested-transaction emulation is the deleted form.

```csharp signature
public static class SavepointFold {
    extension(DbContext db) {
        // A failed inner leg rolls back to its named mark and runs the recovery continuation; the
        // enclosing transaction survives. PG and SQLite both forbid true nested transactions, so the
        // savepoint is the only nesting mechanism — RollbackToSavepointAsync is the engine verb.
        public async ValueTask<T> RollbackTo<T>(
            string mark, Func<DbContext, CancellationToken, ValueTask<T>> leg,
            Func<DbContext, CancellationToken, ValueTask<T>> recover, CancellationToken token) {
            try { return await leg(db, token); }
            catch (DbException) {
                await db.Database.RollbackToSavepointAsync(mark, token);
                return await recover(db, token);
            }
        }
    }
}
```

## [03]-[SQLSTATE_CLASSIFIER]

- Owner: `RetryDisposition` the `[SmartEnum<string>]` contention verdict (`Retry` | `Contend` | `Pass`) carrying its absorb flag and its `StoreFault` projector; `ConcurrencyClass` the SQLSTATE-to-disposition classifier folding the contention classes the store-level execution strategy absorbs silently into `Retry`, the lock-unavailable and aborted-statement classes into `Contend`, and every non-contention fault into `Pass`, feeding the `EnableRetryOnFailure` owner and the `StoreFault.Transient`/`StoreFault.Concurrency` rails.
- Cases: `RetryDisposition` `Retry` (the strategy absorbs and re-runs the closed `TState` closure) | `Contend` (the lock could not be taken or the statement aborted — surfaces as `StoreFault.Transient` because retrying the whole transaction under a backoff may clear it, but the strategy never absorbs it silently) | `Pass` (a non-contention fault the classifier hands back to the `StoreFault.From` provider-exception fold); the `Retry` SQLSTATE classes are `40001` serialization-failure and `40P01` deadlock-detected, the `Contend` classes are `55P03` `NOWAIT` lock-not-available, `55006` object-in-use, and `25P02` in-failed-sql-transaction, and the EF tracked-graph `DbUpdateConcurrencyException` optimistic-token violation is `Pass` — a lost update no retry can fix surfaces through `StoreFault.Concurrency` 7001.
- Entry: `public static RetryDisposition Classify(Error error)` — the one polymorphic classifier: it folds a `PostgresException.SqlState` through the SQLSTATE table and a `DbUpdateConcurrencyException` to `Pass`, discriminating on the error's exception shape rather than a sibling `Classify(string)`/`Classify(Error)` pair, so the SQLSTATE deadlock and the optimistic-token violation share one entry; a non-contention SQLSTATE (a 22-prefixed data exception, a 23 constraint violation, a 42 syntax/undefined-object class) is `Pass` because retry cannot fix an admission defect, and the fault surfaces through its own `StoreFault` arm.
- Auto: the classifier reads `PostgresException.SqlState` matched against the `PostgresErrorCodes` constants — `SerializationFailure` (`40001`) and `DeadlockDetected` (`40P01`) are the only two classes the postgres SQLSTATE-fold law admits a store-level strategy may absorb silently into `Retry`, while `LockNotAvailable` (`55P03`), `ObjectInUse` (`55006`), and `InFailedSqlTransaction` (`25P02`) fold to `Contend` so a `NOWAIT` lock the `WaitPolicy.NoWait` row took fails fast as a typed contention signal a backoff may clear rather than a hidden retry; the EF tracked-graph optimistic violation (`DbUpdateConcurrencyException`) is `Pass` so `StoreFault.From` carries it to `StoreFault.Concurrency` 7001 unretried; the disposition is the `EnableRetryOnFailure` consumer's signal so a `40001`/`40P01` retries inside the execution strategy while a `23505` unique violation passes through; the `Sync/coordination#FENCED_CAS` `Cas` closure rides only the `Retry` half of this fold — a serialization-failure-aborted conditional upsert re-runs the closed `TState`, and its `xmin` reject-lower is the in-process `CoordFault.Stale` check on a `SaveChangesAsync`-bypassing `MergeWithOutputAsync` path, so it never enters this classifier at all.
- Receipt: a classified contention fault rides `StoreFault.Transient` 7002 (`Contend`) or `StoreFault.Concurrency` 7001 (the `DbUpdateConcurrencyException` `Pass`); a retried serialization failure rides the settled `StoreFact.Transaction` kind carrying the absorbed SQLSTATE in its `Subject`, never a second signal owner.
- Packages: Npgsql, Microsoft.EntityFrameworkCore.Sqlite, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new retry-eligible SQLSTATE is one arm on `Classify`; a new disposition is one `RetryDisposition` row carrying its absorb flag and fault projector; zero new surface — a parallel deadlock error family, a second retry classifier, or a sync-merge-divergent verdict is the deleted form because the classifier owns the one `RetryDisposition` and feeds the settled `StoreFault` rail.
- Boundary: retry classification is the SQLSTATE-class fold the postgres `[FAULT_DISPOSITION]` law names — a typed disposition over `PostgresErrorCodes` constants, never the sync-merge `ConflictVerdict` whose `Applies` flag answers op-application, not retry-eligibility (mapping a unique-violation to a merge `RemoteWin` is a semantic category error, the illusory cross-page unification this rebuild deletes); only `40001` and `40P01` are silently retry-absorbed per the postgres SQLSTATE-fold law, the `55P03`/`55006`/`25P02` lock-and-abort classes surface as typed `Transient` contention rather than a hidden retry or a degraded `Text`, and an admission defect (foreign-key, check, not-null, a 22-prefixed data exception, a `23505` unique violation) is `Pass` so it surfaces through its own `StoreFault` arm; the disposition feeds the existing `EnableRetryOnFailure` owner and the settled `StoreFault.Transient`/`Concurrency` rails, never a second retry surface — AppHost owns the hop law and the database stays excluded from it.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class RetryDisposition {
    public static readonly RetryDisposition Retry = new("retry", absorbs: true, static (sqlState, detail) => null);
    public static readonly RetryDisposition Contend = new("contend", absorbs: false, static (sqlState, detail) => new StoreFault.Transient($"<contend:{sqlState}>:{detail}"));
    public static readonly RetryDisposition Pass = new("pass", absorbs: false, static (sqlState, detail) => null);

    public bool Absorbs { get; }   // Retry: the execution strategy re-runs the closed TState closure
    public Func<string, string, StoreFault?> Project { get; }   // Contend mints the typed surface; Pass hands back to StoreFault.From
}

public static class ConcurrencyClass {
    public static RetryDisposition Classify(Error error) =>
        error switch {
            { Exception.Case: PostgresException pg } => Classify(pg.SqlState),
            { Exception.Case: DbUpdateConcurrencyException } => RetryDisposition.Pass,
            _ => RetryDisposition.Pass,
        };

    private static RetryDisposition Classify(string sqlState) =>
        sqlState switch {
            PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected => RetryDisposition.Retry,
            PostgresErrorCodes.LockNotAvailable or PostgresErrorCodes.ObjectInUse or PostgresErrorCodes.InFailedSqlTransaction => RetryDisposition.Contend,
            _ => RetryDisposition.Pass,
        };
}
```

## [04]-[TWO_PHASE_COMMIT]

- Owner: `TwoPhaseDecision` the `[SmartEnum<string>]` second-phase verdict (`Commit` | `Rollback`) carrying its `COMMIT PREPARED`/`ROLLBACK PREPARED` verb; `PreparedTxn` the in-doubt prepared-transaction record projected from `pg_prepared_xacts`; `TwoPhase` the static surface folding `PREPARE TRANSACTION` plus the one polymorphic `Resolve` over `TwoPhaseDecision`, reading the in-doubt set, and exposing the per-decision resolution the AppHost drain conductor names.
- Entry: `public static IO<Unit> Prepare(DbContext db, GlobalTxnId gid)` runs `PREPARE TRANSACTION '<gid>'` so the transaction's work is durable but uncommitted; `public static IO<Unit> Resolve(DbContext db, GlobalTxnId gid, TwoPhaseDecision decision)` runs the decision's verb — `COMMIT PREPARED '<gid>'` or `ROLLBACK PREPARED '<gid>'` — as one polymorphic second-phase entrypoint the coordinator's last-recorded decision value drives, never two sibling `Commit`/`Rollback` names and never a `bool` knob reconstructing the verb at the call site; `public static IO<Seq<PreparedTxn>> InDoubt(DbContext db)` reads `pg_prepared_xacts` so the drain reconciles every prepared-but-unresolved transaction; `public static IO<Unit> PrepareOn(DbContext db, GlobalTxnId gid)` gates on the live connection's engine identity (a `NpgsqlConnection` prepares, the embedded SQLite connection does not) so a `Prepared` scope on the embedded SQLite profile is a typed `StoreFault.Unsupported` 7003 rejection rather than a silent single-phase fallback.
- Auto: the 2PC arm activates only on the `TxnScope.Prepared` flag carrying a `GlobalTxnId` so a cross-store commit prepares each participant, the coordinator commits-prepared on all-prepared or rolls-back-prepared on any participant abort through `Resolve(db, gid, decision)`, and a crash between prepare and the second-phase resolution leaves the transaction in `pg_prepared_xacts` for the drain to reconcile; the in-doubt set crosses to the AppHost drain conductor through the `Query/transaction ← Rasm.AppHost/Runtime # [PORT]: drain 2PC in-doubt set` seam as a Stores-band (300) `DrainParticipantPort` consumer so the recovery reconciliation is the drain owner's concern — the conductor reads this owner's `InDoubt` projection and resolves each prepared transaction through `TwoPhase.Resolve(db, gid, decision)` per the coordinator's last-recorded `TwoPhaseDecision` while store writes stay open at band 300, never a managed XA transaction manager and never a Persistence-side recovery loop; the `max_prepared_transactions` GUC floor is the engine-side precondition the drain assumes, verified read-only through `Store/provisioning#CLUSTER_CONFIG` `ClusterConfig.Verify`; SQLite carries no 2PC so `PrepareOn` rejects a `Prepared` scope on the embedded profile as `StoreFault.Unsupported` 7003 at the rail because its live connection is not an `NpgsqlConnection`.
- Receipt: every 2PC signal rides the one settled `StoreFact.Transaction` kind — a prepare carries the global id in its `Subject`, a second-phase resolution carries the id plus the `TwoPhaseDecision` key — and an in-doubt reconciliation rides the AppHost drain receipt, never a second drain owner and never a dotted fact kind the closed `StoreFact` vocabulary does not carry.
- Packages: Npgsql, Microsoft.EntityFrameworkCore.Sqlite, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new second-phase outcome is one `TwoPhaseDecision` row carrying its verb; a new participant is one `Prepare` call against its `DbContext`; zero new surface — a managed XA coordinator, a second drain owner, a parallel prepared-transaction table, a `bool commit` knob, or sibling `Commit`/`Rollback` names are the deleted form because 2PC rides the PG-native `PREPARE TRANSACTION` and the in-doubt set drains through the one AppHost drain port.
- Boundary: 2PC is PG-native `PREPARE TRANSACTION`/`COMMIT PREPARED` raw SQL through the typed-SQL door, never a managed `System.Transactions` XA coordinator — the cross-store atomic commit is the prepared-transaction protocol the engine owns and this owner folds; the global transaction id is a validated `GlobalTxnId` `[ValueObject<string>]` because the gid lands in a utility statement taking no bound parameter, so the value is admitted once against a strict charset and a raw-string interpolation is the injection-prone deleted form; the in-doubt set is reconciled by the AppHost drain conductor through the seam so the recovery half is the drain owner's concern and this owner only projects `InDoubt` from `pg_prepared_xacts` and names the `Resolve` decision, never owning a second drain loop; a `Prepared` scope on a non-2PC engine (SQLite) is a typed `StoreFault.Unsupported` 7003 rejection at the rail because the lane-by-profile capability denies the shape, never a silent single-phase fallback; the global transaction id is the one cross-store coordination key the coordinator threads, never a per-participant id.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class TwoPhaseDecision {
    public static readonly TwoPhaseDecision Commit = new("commit", "COMMIT PREPARED");
    public static readonly TwoPhaseDecision Rollback = new("rollback", "ROLLBACK PREPARED");

    public string Verb { get; }
}

public readonly record struct PreparedTxn(GlobalTxnId GlobalId, string Owner, Instant Prepared);

public static class TwoPhase {
    public const string InDoubtProbe = "SELECT gid, owner, prepared FROM pg_prepared_xacts";

    public static IO<Unit> Prepare(DbContext db, GlobalTxnId gid) =>
        IO.liftVAsync<Unit>(async env => { _ = await db.Database.ExecuteSqlRawAsync($"PREPARE TRANSACTION '{gid.Value}'", env.Token); return unit; });

    public static IO<Unit> Resolve(DbContext db, GlobalTxnId gid, TwoPhaseDecision decision) =>
        IO.liftVAsync<Unit>(async env => {
            _ = await db.Database.ExecuteSqlRawAsync($"{decision.Verb} '{gid.Value}'", env.Token);
            return unit;
        });

    // 2PC capability is the engine's own property, read once off the live connection at the rail — a
    // Postgres connection prepares, the embedded SQLite connection does not. Gating on the concurrency
    // token string would be a category error (the token answers MVCC versioning, not prepared-xact support).
    public static IO<Unit> PrepareOn(DbContext db, GlobalTxnId gid) =>
        db.Database.GetDbConnection() is NpgsqlConnection
            ? Prepare(db, gid)
            : IO.fail<Unit>(new StoreFault.Unsupported($"<two-phase-unsupported:{db.Database.ProviderName}>"));

    public static IO<Seq<PreparedTxn>> InDoubt(DbContext db) =>
        IO.liftVAsync<Seq<PreparedTxn>>(async env => {
            await using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = InDoubtProbe;
            await using var reader = await cmd.ExecuteReaderAsync(env.Token);
            var rows = Seq<PreparedTxn>();
            while (await reader.ReadAsync(env.Token))
                rows = rows.Add(new PreparedTxn(
                    GlobalTxnId.Create(reader.GetString(0)), reader.GetString(1),
                    Instant.FromDateTimeOffset(reader.GetFieldValue<DateTimeOffset>(2))));
            return rows;
        });
}
```

| [INDEX] | [CONCERN]          | [SURFACE]                                            | [LAW]                                                              |
| :-----: | :----------------- | :-------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | isolation          | `IsolationPolicy.Begin` per-engine begin            | data, never a per-call literal; PG level vs SQLite IMMEDIATE       |
|  [02]   | row locking        | `RowLock(LockStrength × WaitPolicy)` SQL suffix     | server-side `FOR UPDATE SKIP LOCKED`; strength ⟂ wait, never a claim loop |
|  [03]   | coordination lock  | `LockMode.Advisory` real `pg_advisory_xact_lock`    | executed statement releasing on txn end, never a SQL comment       |
|  [04]   | nested rollback    | `Savepoint` ladder + EF `AutoSavepointsEnabled`     | inner leg rolls to mark, enclosing txn survives                    |
|  [05]   | retry classify     | `ConcurrencyClass.Classify` → `RetryDisposition`    | typed SQLSTATE fold; absorb `40001`/`40P01`, surface `55P03`/`55006`/`25P02`, pass the rest |
|  [06]   | cross-store commit | `TwoPhase` `PREPARE`/`Resolve(TwoPhaseDecision)` + `InDoubt` | PG-native 2PC, gid validated, in-doubt drains through AppHost      |

## [05]-[RESEARCH]

- [TXN_ISOLATION_PROBE]: the live-PG18 begin round-trip for the five `IsolationPolicy` rows — the `SERIALIZABLE READ ONLY DEFERRABLE` consistent-read snapshot that never raises a serialization failure, the SQLite `BEGIN IMMEDIATE`-vs-deferred mapping, and the `RowLock` `FOR UPDATE SKIP LOCKED`/`NOWAIT`/`FOR SHARE SKIP LOCKED` work-queue-dequeue contract under concurrent dequeuers — whether the `RowLock.Clause()` pushes into the EF `Query` arm's SQL and the `SKIP LOCKED` skip is exact under contention, proven before the lock-clause fence pins.
- [ADVISORY_LOCK_PROBE]: the `pg_advisory_xact_lock`/`pg_try_advisory_xact_lock` (single-key and two-int-key, share and exclusive) acquisition-and-auto-release round-trip — whether the `AdvisoryLock.Acquire()` statement holds the cross-row coordination lock for the transaction's lifetime and releases on commit/abort without an explicit unlock, and whether the `Try` form returns `false` rather than blocking under contention.
- [TWO_PHASE_DRAIN]: the `PREPARE TRANSACTION`/`Resolve`/`pg_prepared_xacts` 2PC round-trip and the in-doubt reconciliation the AppHost drain conductor runs against a crash between prepare and second-phase — the `max_prepared_transactions` GUC floor verified through `ClusterConfig.Verify`, the `GlobalTxnId` charset gate against the PG gid grammar, and the drain-port in-doubt-set hand-off, proven against a live PG18 server before the 2PC fence pins.
- [SQLSTATE_RETRY_BAND]: the 40001/40P01 retry-eligibility against the `EnableRetryOnFailure` owner's absorbed-class set and the `DbUpdateConcurrencyException` → `StoreFault.Concurrency` 7001 fold, confirming the `RetryDisposition` classifies both the SQLSTATE deadlock and the optimistic-token violation without resurrecting the deleted sync-merge `ConflictVerdict` cross-coupling and without re-running an admission defect.
