# [PERSISTENCE_TRANSACTION]

`Rasm.Persistence` folds isolation, MVCC row/table/advisory locking, lock-wait bounding, savepoint nesting, two-phase commit, and SQLSTATE contention classification into one concurrency axis braced over the settled `StoreOp<T>` surface — never a parallel transaction service, a second isolation enum, or a sync-divergent concurrency rail. `TxnScope` is the transaction-shape record over an `IsolationPolicy` `[SmartEnum<string>]` (carrying its `System.Data.IsolationLevel`, its transaction-scoped GUC bundle — `lock_timeout`, `statement_timeout`, `idle_in_transaction_session_timeout`, and the `READ ONLY DEFERRABLE` knob — and its per-engine `Begin`) crossed with a `LockMode` `[Union]` whose `Row` arm carries the orthogonal `LockStrength` × `WaitPolicy` `RowLock` that lowers the lock through linq2db's typed `PostgreSQLHints.SubQueryTableHint(hint, hint2, Sql.SqlID[])` primitive over `AsPostgreSQL()` (the strength is the `hint` argument, the OF relations a typed `Sql.SqlID[]`, the wait policy the optional `hint2`, so `FOR UPDATE … SKIP LOCKED` is one typed dispatch the bridge lowers server-side, never a hand-built clause string the surface cannot inject and never one of 12 named hint wrappers), whose `Advisory` arm executes a real `pg_advisory_xact_lock`, and whose `Table` arm runs a real `LOCK TABLE`; plus a `Savepoint` ladder, the `RetrySafety` `[Union]` disposition (`Idempotent` | `Verified(probe)` | `NonRetryable` — one closed family, never a `(bool, Option<probe>)` pair admitting the double-apply state), and an optional `GlobalTxnId` `[ValueObject<string>]`. `Transactions.Scoped` threads the scope through one `CreateExecutionStrategy` retry closure: it leases one pooled context, begins one explicit transaction at the scope's per-engine begin, applies the transaction-scoped GUC bundle and the declared advisory/table locks as real statements, establishes the named savepoint marks, runs the `StoreOp<T>` shape, rolls a failed inner leg back to its mark without aborting the enclosing transaction, and prepares-or-commits whole — folding the scope's `RetrySafety` into the `verifySucceeded` projection so an `Idempotent` tail replays bit-stable under `null` (`RETURNING` upserts, fenced reject-lower CAS), a `Verified` tail re-checks its probe on an ambiguous commit rather than double-applying, and a `NonRetryable` delta tail (legal only on a non-retrying isolation) is never silently trusted. `ContentionClass` is the SQLSTATE-keyed `[SmartEnum<string>]` that owns the one EF-strategy seam the rail does not: it documents the connector's built-in `PostgresException.IsTransient` retry floor (the verified `40001`/`40P01`/`55P03`/`55006`/`55000` set the strategy absorbs already, which GROUNDS the `RetrySafety.Idempotent` contract — a tail under an absorbed class must be idempotent) and projects the `errorCodesToAdd` set the `EnableRetryOnFailure` row admits BEYOND that floor (empty for contention, the floor being complete — the honest seam a deployment-specific non-contention transient would populate); an escaped fault folds through the rail's one `StoreFault.From`/`OfSqlState` SQLSTATE classifier (the typed `Conflict` 7711/`Defect` 7712/`Contention` 7713/`Escaped` 7714 arms reading `ConstraintName`/`ColumnName`/`TableName` per the `postgres.md` `[FAULT_DISPOSITION]` law) — a transaction-local SQLSTATE dispatch is the duplicate classifier `Query/rail#OPERATION_ALGEBRA` names the deleted form. There is no per-fault disposition callback into `IExecutionStrategy.ShouldRetryOn`, so a classifier that "feeds the execution strategy" by a `Retry`/`Contend` return value is the illusory shape this rebuild deletes. The EF tracked-graph `DbUpdateConcurrencyException` optimistic-token violation surfaces through `StoreFault.Concurrency` 7001 and is never retried (a lost update no retry can fix); the `Sync/coordination#FENCED_CAS` `xmin` reject-lower runs its check in-process to `CoordFault.Stale` on a `MergeWithOutputAsync` path that bypasses `SaveChangesAsync` entirely, so it never enters this classifier. `TwoPhase` folds `PREPARE TRANSACTION` plus the one polymorphic `Resolve` over a `TwoPhaseDecision` (`COMMIT PREPARED`/`ROLLBACK PREPARED`) and projects the `pg_prepared_xacts` in-doubt set — gid, owner, database, and `transaction` xid so the drain joins `pg_locks.virtualtransaction` — the AppHost drain conductor reconciles. The `StoreOp` 8-case surface, `StoreRail.Run`, `StoreFact`, `StoreFault`, `StoreProfile.ConcurrencyToken`, `EnableRetryOnFailure`, and `ClockPolicy` arrive settled; `Npgsql`, `Microsoft.Data.Sqlite`, and `linq2db.EntityFrameworkCore` carry the per-engine transaction surfaces.

Wire posture: this page is host-local — transaction scope and lock mode are provider statements executed server-side on PostgreSQL or on the embedded SQLite connection, crossing no browser or peer wire, so it mints no `TS_PROJECTION` cluster. The 2PC in-doubt set crosses to the AppHost drain conductor through the `Query/transaction ← Rasm.AppHost/Runtime # [PORT]: drain 2PC in-doubt set` seam as a settled `PreparedTxn` set, never a client-facing projection.

## [01]-[INDEX]

- [01]-[TXN_SCOPE]: the isolation axis with its GUC bundle, the orthogonal hint-projecting row-lock family, the advisory/table locks, the savepoint ladder, the `RetrySafety` `[Union]` disposition, the `GlobalTxnId` value object, and the `Transactions.Scoped` bracket over `StoreOp<T>`.
- [02]-[CONTENTION_CLASS]: the SQLSTATE-keyed `ContentionClass` fold owning the one EF-strategy seam the rail does not — the documented connector `IsTransient` retry floor (grounding the `Idempotent` contract) plus the `errorCodesToAdd` extra set beyond it; escaped-fault projection routes through the rail's one `StoreFault.From` SQLSTATE classifier, never a transaction-local copy.
- [03]-[TWO_PHASE_COMMIT]: the prepare and the one polymorphic `Resolve` over `TwoPhaseDecision`, the `pg_prepared_xacts` in-doubt reader, and the AppHost drain reconciliation.

## [02]-[TXN_SCOPE]

- Owner: `IsolationPolicy` the `[SmartEnum<string>]` isolation-level axis under the `StoreKeyPolicy` ordinal accessor, carrying its `System.Data.IsolationLevel` projection, its retry-eligibility flag, its transaction-scoped GUC bundle (`LockTimeout`/`StatementTimeout`/`IdleTxnTimeout` as `Option<Duration>` plus the `ReadOnly` `DEFERRABLE` knob), and its per-engine `Begin` projection; `LockStrength` and `WaitPolicy` the two orthogonal `[SmartEnum<string>]` row-lock axes carrying the `LinqToDB.DataProvider.PostgreSQL.PostgreSQLHints` SQL-fragment constants; `RowLock` the `[ComplexValueObject]` crossing strength × wait-policy × target relation, projecting the OF set to a typed `Sql.SqlID[]` the typed `PostgreSQLHints.SubQueryTableHint` weaves into a queryable; `AdvisoryLock` the `[ComplexValueObject]` PG advisory-lock request; `TableLockMode` the `[SmartEnum<string>]` table-lock-strength axis; `LockMode` the `[Union]` lock-request family (`Row` | `Advisory` | `Table`); `Savepoint` the named nested-rollback ladder record; `RetrySafety` the `[Union]` retry-safety disposition (`Idempotent` | `Verified` | `NonRetryable`); `GlobalTxnId` the `[ValueObject<string>]` 2PC coordination key; `TxnScope` the transaction-shape record threading isolation, lock mode, savepoints, the `RetrySafety` disposition, the prepared flag, and the global id; `Transactions` the static surface owning the scoped bracket that runs a `StoreOp<T>` shape on a transaction-bearing leased context under the same `CreateExecutionStrategy` discipline `StoreRail.Run` uses; `RowLockQuery` the `extension(IQueryable<TRow>)` block applying the `RowLock` typed hint to a caller queryable.
- Cases: `IsolationPolicy` read-committed | snapshot (PG MVCC snapshot isolation IS `IsolationLevel.RepeatableRead`, the only level PG accepts beyond read-committed and serializable) | serializable | serializable-deferred; `LockStrength` `Update` (`PostgreSQLHints.ForUpdate`), `NoKeyUpdate` (`ForNoKeyUpdate`), `Share` (`ForShare`), `KeyShare` (`ForKeyShare`); `WaitPolicy` `Wait` | `SkipLocked` (`PostgreSQLHints.SkipLocked`) | `NoWait` (`PostgreSQLHints.NoWait`); `TableLockMode` `AccessShare` | `RowExclusive` | `Share` | `Exclusive` | `AccessExclusive`; `LockMode` `Row(RowLock)` | `Advisory(AdvisoryLock)` | `Table(TableLockMode, Seq<string>)` with the `Row.ForUpdate`/`ForShare` and `Advisory.Coord`/`Table.Of` named factories so a consumer spells a lock by intent rather than constructing the union arm raw; `RetrySafety` `Idempotent` (replay bit-stable → `verifySucceeded` null) | `Verified(probe)` (ambiguous commit re-checks) | `NonRetryable` (delta tail, legal only on a non-retrying isolation); a `Savepoint` is a named mark a failed inner op rolls back to without aborting the enclosing transaction.
- Entry: `public static IO<T> Scoped<TDb, T>(PooledDbContextFactory<TDb> contexts, TxnScope scope, StoreOp<T> op, InterceptPolicy policy, ClockPolicy clocks, DeadlineClass deadline)` — leases one pooled context, opens one explicit transaction at the scope's `IsolationPolicy.Begin` (a serializable PG transaction reads a consistent snapshot; SQLite maps serializable to `BEGIN IMMEDIATE` and snapshot/read-committed to deferred), runs the scope's transaction-scoped `SET LOCAL` GUC bundle, acquires the declared `LockMode` server-side (the `Advisory` arm runs `SELECT pg_advisory_xact_lock(<key>)` so a cross-row coordination lock releases on transaction end, and a `Try` advisory reads the `pg_try_advisory_xact_lock` bool scalar and rejects `StoreFault.Transient` on a contended false rather than dropping the verdict, the `Table` arm runs `LOCK TABLE … IN <mode> MODE`; the `Row` arm carries no standalone statement — it is the caller-woven typed `SubQueryTableHint` the `Query` shape already applied through `RowLockQuery.Locked`), establishes the named `Savepoint` marks, runs the `StoreOp<T>` shape, rolls a failed inner leg back to its mark through `RollbackToSavepointAsync` while the enclosing transaction survives, and prepares-or-commits whole through the `Close` arm; the whole begin-guc-lock-savepoints-op-close closure runs inside the `CreateExecutionStrategy` callback so a serialization-failure retry re-runs the closed `TState` value, never a captured closure, and the `verifySucceeded` argument is the total fold of the scope's `RetrySafety` union — `Idempotent` projects `null` (the `RETURNING` upserts and the `Sync/coordination#FENCED_CAS` reject-lower CAS re-run to the identical committed state, discharging the doctrine's `verifySucceeded` mandate by op idempotency), `Verified` lifts the caller probe `Func<DbContext, CancellationToken, ValueTask<bool>>` into `ExecutionResult<T>` so an ambiguous commit re-checks rather than double-applies, and `NonRetryable` (the delta tail) is the construction-gated state legal only on a non-retrying isolation — a blanket `null` over a delta-shaped `Upsert`/`Delete` and the illegal non-idempotent-without-probe pairing a `(bool, Option)` shape would admit are the deleted illusory-safety forms.
- Auto: the transaction begins through `IsolationPolicy.Begin(db, ct)` so PostgreSQL takes the declared `BeginTransactionAsync(IsolationLevel)` (`snapshot` is `IsolationLevel.RepeatableRead` because PG's MVCC snapshot isolation IS repeatable-read — `RepeatableRead` is the canonical level the connector begins, the spelling Npgsql also folds `IsolationLevel.Snapshot` onto, so the row carries the honest level rather than the SQL-Server alias) while SQLite folds serializable to the synchronous `BeginTransaction(IsolationLevel.Serializable, deferred: false)` enrolled into EF through `UseTransactionAsync` (the `BEGIN IMMEDIATE` that reserves the writer up front so a deferred read that later writes never burns the busy budget on a stale-snapshot `SQLITE_BUSY`) and snapshot/read-committed to `deferred: true`; the `serializable-deferred` row begins serializable then runs `SET TRANSACTION READ ONLY DEFERRABLE` on PG so a long analytical read takes a guaranteed-consistent snapshot that never raises a serialization failure and never needs a retry, and its `IdleTxnTimeout` GUC caps an idle reader holding back vacuum; every row's `SET LOCAL lock_timeout`/`statement_timeout` bundle bounds the bracket so a `Wait` policy under a contended row is a bounded wait the connector surfaces as `LockNotAvailable` rather than an unbounded block; the `LockMode.Row` arm is woven into the caller's `Query` shape at authoring time through `RowLockQuery.Locked(rowLock)` over the bridged `ToLinqToDB().AsPostgreSQL().SubQueryTableHint(strength, wait, targets)` so a work-queue dequeue is `SELECT … FOR UPDATE SKIP LOCKED` lowered server-side by linq2db's typed hint, never a hand-built clause string and never a client-side claim race; the `LockMode.Advisory` arm folds `pg_advisory_xact_lock`/`pg_try_advisory_xact_lock` (the share variant and the `(int4, int4)` two-key variant its `AdvisoryLock` value selects) as a real executed statement releasing on transaction end — the blocking variant blocks until granted, the `Try` variant reads its bool scalar through `ExecuteScalarAsync` and rejects `StoreFault.Transient` on a contended false so the non-blocking verdict is never silently dropped — and the `LockMode.Table` arm runs an explicit `LOCK TABLE` for a coarse coordination grant; the savepoint ladder establishes the named marks through the `IDbContextTransaction.CreateSavepointAsync` the begin handle exposes (EF `AutoSavepointsEnabled` adding an implicit per-save mark) so a composed op's failed leg rolls back to its mark through `RollbackToSavepointAsync` on `Database.CurrentTransaction` while the enclosing transaction survives; the database stays excluded from the AppHost hop law — retry is the profile-row `EnableRetryOnFailure` connector strategy (its complete built-in `IsTransient` contention floor plus the `Contention.RetryCodes()` extra set, empty for contention) plus SQLite busy-retry, never a second retry surface.
- Receipt: every transaction signal rides the one settled `StoreFact.Transaction` kind (`store.transaction`) the `StoreInterceptor.TransactionCommittedAsync` hook already stamps — the commit fact's `Subject` carries the isolation level, a savepoint release/rollback carries the mark name, a slow lock-acquisition carries the lock clause, and a strategy retry carries the absorbed SQLSTATE — so the variant lands in the existing `Subject`/`Count` slots of the one fact shape rather than a new dotted kind constant the closed `StoreFact` vocabulary does not carry; this owner adds no second interceptor and mints no parallel fact kind.
- Packages: Npgsql, Microsoft.Data.Sqlite, linq2db.EntityFrameworkCore (the `ToLinqToDB` bridge lift), linq2db (core, owning `PostgreSQLHints.SubQueryTableHint`/`AsPostgreSQL` and the `For*Hint` family, plus `Sql.SqlID`), Microsoft.EntityFrameworkCore.Sqlite, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new isolation level is one `IsolationPolicy` row carrying its level, retry flag, GUC bundle, and per-engine begin; a new row-lock strength is one `LockStrength` row carrying its `PostgreSQLHints` SQL-fragment constant, a new wait policy one `WaitPolicy` row, a new table-lock strength one `TableLockMode` row, a new lock kind one `LockMode` case; a new transaction-scoped guard is one `Option<Duration>` column on `IsolationPolicy`; a new savepoint concern is one column on `Savepoint`; a new retry-safety disposition is one `RetrySafety` case (the total `Verify` fold breaks at compile time until it is handled); the 2PC arm is the `Prepared` `TxnScope` flag carrying a `GlobalTxnId`; zero new surface — a transaction-per-entity service, a second isolation enum, a flat lock-mode enum that conflates strength with wait policy, a `(bool, Option)` retry-safety pair, or a parallel concurrency error family is the deleted form.
- Boundary: `TxnScope` is an arm-family over the existing `StoreOp` surface, never a ninth `StoreOp` case and never a transaction service — the scoped bracket leases one context, begins the transaction at the scope's per-engine begin, runs the GUC bundle, acquires the declared locks server-side, establishes the savepoint marks, and runs the `StoreOp<T>` shape inside the `CreateExecutionStrategy` retry closure, so a new transaction posture is a `TxnScope` field, not a new owner; `IsolationPolicy.IsolationLevel` projects the `System.Data.IsolationLevel` the provider's begin takes so the isolation level is data, never a per-call literal, and `IsolationPolicy.Begin` owns the PG-vs-SQLite begin divergence so the embedded floor's `BEGIN IMMEDIATE`/deferred mapping is a row projection rather than a call-site branch; lock STRENGTH and wait POLICY are orthogonal `RowLock` axes so `FOR SHARE SKIP LOCKED` is one expressible cross rather than a missing flat-enum combination, and the row lock lowers through linq2db's typed `SubQueryTableHint(hint, hint2, Sql.SqlID[])` primitive — the prior "raw `Clause()` string the bracket appends to the `Query` arm's projection" was illusory because the EF `Query` shape is an opaque `Func<DbContext, CancellationToken, ValueTask<T>>` with no seam to inject a suffix, and a hand-concatenated lock clause is below the package's typed-hint depth (the strength is the `hint` argument, the OF set a typed `Sql.SqlID[]`, the wait the `hint2`), so the row lock is the caller-composed `RowLockQuery.Locked(rowLock)` typed hint, never a string `Transactions.Scoped` splices into an opaque delegate, and a client-side claim loop is the deleted form; the `Advisory` arm executes a real `pg_advisory_xact_lock` statement (a SQL-comment placeholder is the illusory deleted form), `SKIP LOCKED` is the one work-queue-dequeue mechanism rather than an advisory poll, and the advisory lock is the one cross-row coordination mechanism the engine releases on transaction end; lock-wait is bounded by the `lock_timeout` GUC the scope carries (a `Wait` policy is never an unbounded block — the bound surfaces as `LockNotAvailable` the `ContentionClass` classifies), distinct from the per-row `NOWAIT`/`SKIP LOCKED` no-wait policies; the savepoint ladder reuses EF `AutoSavepointsEnabled` and explicit named marks so a hand-rolled nested-transaction emulation is the deleted form (PostgreSQL and SQLite both forbid true nested transactions, so savepoints are the only nesting mechanism); `verifySucceeded` is honest — the scope's `RetrySafety` union is the closed retry-safety disposition the fold reads, so an `Idempotent` tail passes `null` (its replay is bit-stable: `RETURNING` upsert, fenced CAS), a `Verified` tail supplies the caller probe the strategy lifts into `ExecutionResult<T>`, and a `NonRetryable` tail is construction-gated to a non-retrying isolation so the connector never re-runs it — the illegal `(non-idempotent, no-probe)` state a `(bool, Option)` shape would admit is unreachable, never a blanket `null` over a delta-shaped `Upsert`/`Delete` the doctrine forbids; the EF tracked-graph optimistic-token violation (`DbUpdateConcurrencyException`, raised by `SaveChangesAsync` on a `StoreProfile.ConcurrencyToken` mismatch — `xmin` on PG, `version` on SQLite) folds to `StoreFault.Concurrency` 7001 at the one `StoreFault.From` site because a lost update is never retried, while the `Sync/coordination#FENCED_CAS` reject-lower runs its `xmin` snapshot check in-process to `CoordFault.Stale` on a `MergeWithOutputAsync` path that bypasses `SaveChangesAsync` entirely, so the CAS optimistic check never enters this classifier — conflating the fenced-CAS reject with the EF tracked-graph concurrency fault is the illusory cross-page coupling this owner refuses; the 2PC `Prepared` arm is the only cross-store-commit mechanism and reconciles the in-doubt set through the AppHost drain port, never a managed XA coordinator; the `GlobalTxnId` is a validated `[ValueObject<string>]` because a 2PC gid lands in a `PREPARE TRANSACTION '<id>'` utility statement that takes no bound parameter, so the value is admitted once against a strict charset rather than interpolated raw.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class IsolationPolicy {
    public static readonly IsolationPolicy ReadCommitted = new("read-committed", IsolationLevel.ReadCommitted, Retryable: false, Deferred: true, ReadOnly: false, LockTimeout: None, StatementTimeout: None, IdleTxnTimeout: None);
    public static readonly IsolationPolicy Snapshot = new("snapshot", IsolationLevel.RepeatableRead, Retryable: true, Deferred: true, ReadOnly: false, LockTimeout: Some(Duration.FromSeconds(3)), StatementTimeout: None, IdleTxnTimeout: None);
    public static readonly IsolationPolicy Serializable = new("serializable", IsolationLevel.Serializable, Retryable: true, Deferred: false, ReadOnly: false, LockTimeout: Some(Duration.FromSeconds(3)), StatementTimeout: Some(Duration.FromSeconds(30)), IdleTxnTimeout: None);
    public static readonly IsolationPolicy SerializableDeferred = new("serializable-deferred", IsolationLevel.Serializable, Retryable: false, Deferred: false, ReadOnly: true, LockTimeout: None, StatementTimeout: None, IdleTxnTimeout: Some(Duration.FromMinutes(5)));

    public IsolationLevel IsolationLevel { get; }
    public bool Retryable { get; }              // snapshot/serializable may raise 40001 the connector strategy absorbs; PG snapshot isolation IS RepeatableRead
    public bool Deferred { get; }               // SQLite: deferred begin vs the IMMEDIATE writer reservation
    public bool ReadOnly { get; }               // PG: SERIALIZABLE READ ONLY DEFERRABLE consistent read snapshot
    public Option<Duration> LockTimeout { get; }       // SET LOCAL lock_timeout: a Wait policy is bounded, never an unbounded block
    public Option<Duration> StatementTimeout { get; }  // SET LOCAL statement_timeout: a runaway serializable write is capped
    public Option<Duration> IdleTxnTimeout { get; }    // SET LOCAL idle_in_transaction_session_timeout: an idle long reader never pins vacuum

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
            // BeginTransaction(bool deferred) is the verified SQLite overload; the engine has no isolation
            // levels (the IsolationLevel overload accepts only Unspecified/Serializable/ReadUncommitted and maps
            // them onto the deferred flag), so the PG IsolationLevel never crosses here — Deferred is the whole
            // seam, deferred: false is the IMMEDIATE writer reservation. The sync begin enrolls through
            // UseTransactionAsync, whose IDbContextTransaction?
            // return is non-null on a live ADO transaction the call just opened; SQLite carries none of the PG
            // GUCs (its busy budget is the connection-string timeout).
            return await db.UseTransactionAsync(sqlite.BeginTransaction(deferred: Deferred), token)
                ?? throw new InvalidOperationException("<sqlite-transaction-enrollment-failed>");
        }
        var txn = await db.BeginTransactionAsync(IsolationLevel, token);
        if (ReadOnly)
            _ = await db.ExecuteSqlRawAsync("SET TRANSACTION READ ONLY DEFERRABLE", token);
        // The transaction-scoped GUC bundle is SET LOCAL so the value reverts at commit/abort; a
        // millisecond integer is invariant-culture-formatted into the SET text because a GUC argument
        // takes no bound parameter — the source is the row, never raw runtime input.
        await Guc(db, "lock_timeout", LockTimeout, token);
        await Guc(db, "statement_timeout", StatementTimeout, token);
        await Guc(db, "idle_in_transaction_session_timeout", IdleTxnTimeout, token);
        return txn;
    }

    private static ValueTask Guc(DatabaseFacade db, string knob, Option<Duration> window, CancellationToken token) =>
        window.Match(
            Some: w => new ValueTask(db.ExecuteSqlRawAsync($"SET LOCAL {knob} = {(long)w.TotalMilliseconds}", token)),
            None: () => ValueTask.CompletedTask);
}

// The strength axis carries the LinqToDB.DataProvider.PostgreSQL.PostgreSQLHints SQL-fragment constant
// (ForUpdate == "FOR UPDATE") the typed SubQueryTableHint passes as its `hint` argument so the engine lowers
// the `FOR <strength>` clause; table-level coarse grants are the separate TableLockMode axis, so a row-lock
// strength never carries a LOCK TABLE mode string.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class LockStrength {
    public static readonly LockStrength Update = new("update", PostgreSQLHints.ForUpdate);
    public static readonly LockStrength NoKeyUpdate = new("no-key-update", PostgreSQLHints.ForNoKeyUpdate);
    public static readonly LockStrength Share = new("share", PostgreSQLHints.ForShare);
    public static readonly LockStrength KeyShare = new("key-share", PostgreSQLHints.ForKeyShare);

    public string Hint { get; }   // PostgreSQLHints.ForUpdate/ForNoKeyUpdate/ForShare/ForKeyShare
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class WaitPolicy {
    public static readonly WaitPolicy Wait = new("wait", string.Empty);
    public static readonly WaitPolicy SkipLocked = new("skip-locked", PostgreSQLHints.SkipLocked);
    public static readonly WaitPolicy NoWait = new("nowait", PostgreSQLHints.NoWait);

    public string Hint { get; }   // PostgreSQLHints.SkipLocked/NoWait; Wait is the empty no-suffix default
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
// The lock lowers through linq2db's typed `SubQueryTableHint` primitive (the one method the 12 named
// `ForUpdateSkipLockedHint`/`ForShareNoWaitHint`/… wrappers delegate to), so the OF list is a typed
// `Sql.SqlID[]` and the wait suffix the typed `hint2` argument — the lock CLAUSE is never a hand-built
// SQL string the bracket concatenates, the package owns OF placement (`FOR <strength> OF <rels> <wait>`).
// `Clause` survives only as the receipt display the slow-lock fact carries, never as the execution path.
[ComplexValueObject]
public sealed partial class RowLock {
    public LockStrength Strength { get; }
    public WaitPolicy Wait { get; }
    public Seq<string> Of { get; }

    // The OF relations as typed linq2db table identifiers (SqlIDType.TableName) the typed hint binds —
    // an empty Of is the no-array `params` call locking every joined relation.
    public Sql.SqlID[] Targets => [.. Of.Map(static r => new Sql.SqlID(Sql.SqlIDType.TableName, r))];

    // Receipt-only display; the execution path is the typed SubQueryTableHint over Targets, never this string.
    public string Clause =>
        Of.IsEmpty
            ? (Wait == WaitPolicy.Wait ? Strength.Hint : $"{Strength.Hint} {Wait.Hint}")
            : $"{Strength.Hint} OF {string.Join(", ", Of)}{(Wait == WaitPolicy.Wait ? string.Empty : $" {Wait.Hint}")}";
}

// The row lock as a real typed server-side hint: the EF queryable lifts onto the linq2db translator through
// ToLinqToDB, AsPostgreSQL narrows it to IPostgreSQLSpecificQueryable, and the typed
// LinqExtensions/PostgreSQLHints.SubQueryTableHint lowers `FOR <strength> [OF <rels>] [<wait>]` so the SELECT
// carries `… FOR UPDATE SKIP LOCKED` in PostgreSQL — the strength is the `hint` argument, the OF list a typed
// `Sql.SqlID[]`, and the wait policy the optional `hint2` argument, never a raw clause string the EF `Query`
// Func has no seam to receive and never a client-side claim race. The Wait policy selects the 2-arg (no-suffix)
// vs 3-arg (wait-suffix) overload, so `FOR SHARE` and `FOR SHARE SKIP LOCKED` are one typed dispatch rather
// than 12 named wrappers. The bridge lift enlists in Database.CurrentTransaction so the locked SELECT runs
// inside the bracket's transaction. A caller authors
// `q.Locked(RowLock.Create(LockStrength.Update, WaitPolicy.SkipLocked, Seq<string>()))` inside its
// `StoreOp.Query` shape; the TxnScope.Lock `Row` arm is the declaration the bracket reads to receipt the lock,
// the typed hint is the execution. The sqlite floor has no pushdown hint, so a Row lock on the embedded
// profile degrades to the single-writer lease the WAL begin already holds.
public static class RowLockQuery {
    extension<TRow>(IQueryable<TRow> source) where TRow : notnull {
        public IQueryable<TRow> Locked(RowLock rowLock) =>
            rowLock.Wait == WaitPolicy.Wait
                ? source.ToLinqToDB().AsPostgreSQL().SubQueryTableHint(rowLock.Strength.Hint, rowLock.Targets)
                : source.ToLinqToDB().AsPostgreSQL().SubQueryTableHint(rowLock.Strength.Hint, rowLock.Wait.Hint, rowLock.Targets);
    }
}

// A real PG advisory lock: the (int4, int4) two-key form, the xact-vs-session scope, the share mode,
// and the try (non-blocking) mode select the exact `pg_advisory*` function. The lock is a SELECT
// executed before the op runs, releasing on transaction end (xact scope) — never a SQL-comment placeholder.
[ComplexValueObject]
public sealed partial class AdvisoryLock {
    public long Key { get; }
    public bool TwoKey { get; }         // true => the (int4, int4) overload over the key's 32-bit halves
    public bool Shared { get; }
    public bool Try { get; }            // pg_try_advisory* — a contended lock returns false, never blocks

    // The acquire statement plus its disposition: a blocking pg_advisory_xact_lock returns void (the SELECT
    // blocks until granted, so the bracket reads only completion), but a Try variant returns the bool verdict
    // the caller MUST observe — IsTry carries that flag so Transactions.Acquire reads the scalar and rejects
    // StoreFault.Transient on a false (contended) result rather than discarding the verdict ExecuteSqlRawAsync's
    // rows-affected return would silently drop. The key arguments are invariant-culture-formatted bigint/int4
    // literals (never user input — a long admits no injection), so the same SELECT text drives both the EF
    // ExecuteSqlRawAsync blocking path and the raw ExecuteScalarAsync try path without a placeholder dialect.
    // The two-key form binds the int4 overload from the bigint key's high/low 32-bit halves (the canonical PG
    // namespacing idiom), so the same bigint surface drives both engine overloads without a truncating (int)
    // narrowing that would collide distinct high-half keys.
    public (string Sql, bool IsTry) Acquire() {
        var fn = (Try, Shared) switch {
            (true, true) => "pg_try_advisory_xact_lock_shared",
            (true, false) => "pg_try_advisory_xact_lock",
            (false, true) => "pg_advisory_xact_lock_shared",
            (false, false) => "pg_advisory_xact_lock",
        };
        return TwoKey
            ? ($"SELECT {fn}({(int)(Key >> 32)}, {unchecked((int)Key)})", Try)
            : ($"SELECT {fn}({Key})", Try);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LockMode {
    private LockMode() { }

    public sealed record Row(RowLock Lock) : LockMode;
    public sealed record Advisory(AdvisoryLock Lock) : LockMode;
    public sealed record Table(TableLockMode Mode, Seq<string> Relations) : LockMode;

    // The row-lock factories spell a lock by intent: ForUpdate/ForShare over a wait policy, with the OF-relation
    // overload (params ReadOnlySpan<string>) binding the multi-join lock to its named relations so a queue dequeue
    // locks the claim row without locking its joined lookup rows. Coord spans both advisory overloads — the bigint
    // single-key default and the (int4, int4) namespacing form (twoKey: true) the AdvisoryLock splits from the
    // bigint's 32-bit halves. On is the coarse table-lock grant.
    public static LockMode ForUpdate(WaitPolicy wait, params ReadOnlySpan<string> of) =>
        new Row(RowLock.Create(LockStrength.Update, wait, [.. of]));
    public static LockMode ForShare(WaitPolicy wait, params ReadOnlySpan<string> of) =>
        new Row(RowLock.Create(LockStrength.Share, wait, [.. of]));
    public static LockMode Coord(long key, bool shared = false, bool tryOnly = false, bool twoKey = false) =>
        new Advisory(AdvisoryLock.Create(key, twoKey, shared, tryOnly));
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

// The retry-safety disposition is one closed family, never a (bool Idempotent, Option<Verify>) pair whose
// (false, None) state — a non-idempotent tail with no probe under a retry-eligible isolation — is the
// double-apply hazard the doctrine forbids and the type cannot otherwise exclude: Idempotent declares the tail
// bit-stable so a retry replays to the identical committed state (the RETURNING upsert, the fenced reject-lower
// CAS) and verifySucceeded is null; Verified carries the caller probe the strategy lifts into ExecutionResult<T>
// so an ambiguous commit re-checks rather than double-applies; NonRetryable is the delta tail that must never be
// retried, legal ONLY on a non-retrying isolation (Retryable: false) so the connector strategy never re-runs it.
// A serializable scope therefore carries Idempotent or Verified, never NonRetryable — the construction gate the
// factories enforce, so the illegal pairing is unreachable rather than degraded to an always-retry verdict.
[Union]
public abstract partial record RetrySafety {
    private RetrySafety() { }
    public sealed record Idempotent : RetrySafety;
    public sealed record Verified(Func<DbContext, CancellationToken, ValueTask<bool>> Probe) : RetrySafety;
    public sealed record NonRetryable : RetrySafety;
}

public sealed record TxnScope(
    IsolationPolicy Isolation, Option<LockMode> Lock, Seq<Savepoint> Savepoints,
    RetrySafety Retry, bool Prepared, Option<GlobalTxnId> GlobalTxnId) {
    public static readonly TxnScope Default = new(IsolationPolicy.ReadCommitted, None, Seq<Savepoint>(), new RetrySafety.Idempotent(), Prepared: false, None);

    // A serializable write scope is retry-eligible only when its tail replays to a closed value — the
    // dequeue/upsert lane (Idempotent) or a delta tail that supplies its re-check probe (Verified); a
    // NonRetryable delta tail stays read-committed because the connector strategy must never re-run it.
    public static TxnScope Serializable(LockMode lockMode, RetrySafety retry) =>
        new(IsolationPolicy.Serializable, Some(lockMode), Seq<Savepoint>(), retry, Prepared: false, None);

    public static TxnScope ReadSnapshot =>
        new(IsolationPolicy.SerializableDeferred, None, Seq<Savepoint>(), new RetrySafety.Idempotent(), Prepared: false, None);
}

public static class Transactions {
    private readonly record struct ScopeState<T>(TxnScope Scope, Func<DbContext, CancellationToken, ValueTask<T>> Shape) where T : notnull;

    public static IO<T> Scoped<TDb, T>(
        PooledDbContextFactory<TDb> contexts, TxnScope scope, StoreOp<T> op,
        InterceptPolicy policy, ClockPolicy clocks, DeadlineClass deadline) where TDb : DbContext where T : notnull =>
        IO.liftAsync(env => contexts.CreateDbContextAsync(env.Token)).Bracket(
            Use: db => IO.liftAsync(env => db.Database.CreateExecutionStrategy().ExecuteAsync(
                new ScopeState<T>(scope, Shape(op, policy)),
                static async (DbContext ctx, ScopeState<T> state, CancellationToken ct) => {
                    // The EF execution-strategy operation delegate threads the leased DbContext as its FIRST
                    // argument (Func<DbContext, TState, CancellationToken, Task<TResult>>), so the closed state
                    // carries no context handle — a serialization-failure retry re-runs over the SAME ctx EF
                    // hands the closure. Begin returns the live IDbContextTransaction so the savepoint API — which
                    // lives on the transaction object, not on DatabaseFacade — runs against the one mark-bearing
                    // handle, and the connection's engine identity inside Begin gates PG-vs-SQLite begin without a branch.
                    await using var txn = await state.Scope.Isolation.Begin(ctx.Database, ct);
                    await Acquire(ctx, state.Scope.Lock, ct);
                    await Mark(txn, state.Scope.Savepoints, ct);
                    var value = await state.Shape(ctx, ct);
                    // Close: the Prepared scope ends the transaction with PREPARE TRANSACTION (engine-gated so
                    // SQLite rejects), leaving it durable-but-uncommitted in pg_prepared_xacts for the AppHost
                    // drain conductor to COMMIT/ROLLBACK PREPARED — the prepared transaction is dissociated from
                    // the session, so the await-using dispose is a no-op and never rolls it back; a single-phase
                    // scope commits whole. The branch is the union over the Prepared×GlobalTxnId shape, never a
                    // silent single-phase fallback on a Prepared scope.
                    await Close(ctx, txn, state.Scope, ct);
                    return value;
                },
                // verifySucceeded is the scope's RetrySafety fold over (DbContext, TState, CancellationToken): an
                // Idempotent tail replays bit-stable so null is correct, a Verified tail's probe lifts into
                // ExecutionResult<T> so an ambiguous commit re-checks rather than double-applies, and a NonRetryable
                // tail (legal only on a non-retrying isolation the connector never re-runs) reports the false verdict
                // so an ambiguous commit is never silently trusted — the illegal non-idempotent-without-probe pairing
                // a (bool, Option) shape would admit is unreachable here.
                verifySucceeded: Verify<T>(scope.Retry),
                env.Token)).Timeout(deadline.Allotted.ToTimeSpan()),
            Catch: static error => IO.fail<T>(StoreFault.From(error)),
            Fin: static db => IO.liftVAsync<Unit>(async _ => { await db.DisposeAsync(); return unit; }));

    // The EF execution-strategy verifySucceeded delegate is Func<DbContext, TState, CancellationToken,
    // Task<ExecutionResult<T>>> — EF threads the leased context as the leading argument, so the probe reads its
    // DbContext off that parameter, never off the state value. The RetrySafety fold is total: a fourth case breaks
    // this Switch at compile time rather than falling through a silent arm.
    private static Func<DbContext, ScopeState<T>, CancellationToken, Task<ExecutionResult<T>>>? Verify<T>(RetrySafety retry) where T : notnull =>
        retry.Switch<Func<DbContext, ScopeState<T>, CancellationToken, Task<ExecutionResult<T>>>?>(
            Idempotent: static _ => null,
            Verified: static v => async (DbContext ctx, ScopeState<T> _, CancellationToken ct) => new ExecutionResult<T>(await v.Probe(ctx, ct), default!),
            NonRetryable: static _ => static (DbContext _, ScopeState<T> _, CancellationToken _) => Task.FromResult(new ExecutionResult<T>(false, default!)));

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

    // The acquire statement IS the lock arm: Advisory runs a real pg_advisory_xact_lock SELECT releasing on
    // transaction end, Table runs an explicit LOCK TABLE over the closed relation vocabulary at the one declared
    // identifier seam (LOCK TABLE takes identifiers, not bound parameters). Row mints no standalone statement —
    // its typed SubQueryTableHint is woven into the caller's queryable through RowLockQuery.Locked at authoring
    // time, so the SELECT itself carries the FOR UPDATE … SKIP LOCKED linq2db lowers, and absence (no declared
    // lock) is the None arm — so the lock-emission variation is the union match, never an imperative fall-through.
    private static ValueTask Acquire(DbContext db, Option<LockMode> mode, CancellationToken token) =>
        mode.Match(
            Some: m => m switch {
                LockMode.Advisory a => Advisory(db, a.Lock, token),
                LockMode.Table t => Statement(db, $"LOCK TABLE {string.Join(", ", t.Relations)} IN {t.Mode.Sql} MODE", [], token),
                _ => ValueTask.CompletedTask,
            },
            None: () => ValueTask.CompletedTask);

    // The blocking advisory SELECT blocks until granted (its rows-affected return is meaningless and discarded),
    // but a Try advisory returns the bool verdict the caller MUST observe — so the Try path reads the scalar and
    // a false (contended) result fails the transaction with the typed StoreFault.Transient the rail's StoreFault.From
    // surfaces (the contention message rides the fault), never a silently-dropped verdict that proceeds without the
    // lock. The scalar read is the connection's ExecuteScalarAsync because ExecuteSqlRawAsync returns rows affected,
    // never the SELECT value; the key is an inlined bigint literal so neither path binds a parameter.
    private static async ValueTask Advisory(DbContext db, AdvisoryLock lock_, CancellationToken token) {
        var (sql, isTry) = lock_.Acquire();
        if (!isTry) { await Statement(db, sql, [], token); return; }
        await using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = sql;
        if (await cmd.ExecuteScalarAsync(token) is not true)
            throw new StoreFault.Transient($"<advisory-contended:{lock_.Key}>").ToException();
    }

    private static async ValueTask Statement(DbContext db, string sql, object[] args, CancellationToken token) =>
        _ = await db.Database.ExecuteSqlRawAsync(sql, args, token);

    private static async ValueTask Mark(IDbContextTransaction txn, Seq<Savepoint> savepoints, CancellationToken token) {
        foreach (var mark in savepoints)
            await txn.CreateSavepointAsync(mark.Name, token);
    }

    // The close arm is the union over the (Prepared, GlobalTxnId) shape, never a flag-then-commit fall-through.
    // A Prepared scope ends the transaction with PREPARE TRANSACTION — engine-gated, so the embedded SQLite
    // connection (which carries no 2PC) throws StoreFault.Unsupported the Bracket catch lifts rather than a silent
    // single-phase commit, and a Prepared scope missing its GlobalTxnId is the same typed rejection (the gid is the
    // cross-store coordination key the coordinator must thread). After PREPARE TRANSACTION the work is durable but
    // dissociated from the session, so EF's await-using dispose of `txn` no longer sees an active transaction and
    // never rolls it back; the AppHost drain conductor resolves it through TwoPhase.Resolve. A single-phase scope
    // commits whole. The gid is the GlobalTxnId-validated literal PREPARE TRANSACTION takes (no bound parameter).
    private static ValueTask Close(DbContext db, IDbContextTransaction txn, TxnScope scope, CancellationToken token) =>
        (scope.Prepared, scope.GlobalTxnId) switch {
            (true, { Case: GlobalTxnId gid }) when db.Database.GetDbConnection() is NpgsqlConnection =>
                Statement(db, $"PREPARE TRANSACTION '{gid.Value}'", [], token),
            (true, _) => ValueTask.FromException(new StoreFault.Unsupported($"<two-phase-unsupported:{db.Database.ProviderName}>").ToException()),
            (false, _) => new ValueTask(txn.CommitAsync(token)),
        };
}
```

The savepoint rollback is the composed-op spelling: a `StoreOp<T>.Then` continuation whose leg may fail wraps the inner shape in `RollbackTo` so the failed leg restates to its named mark while the enclosing transaction survives, and a leg that needs the whole-transaction abort simply throws, lifting through the `Bracket` catch into `StoreFault`. `RollbackTo` runs `RollbackToSavepointAsync` and re-runs the recovery continuation under the same execution strategy, never aborting the enclosing transaction — a hand-rolled nested-transaction emulation is the deleted form.

```csharp signature
public static class SavepointFold {
    extension(DbContext db) {
        // A failed inner leg rolls back to its named mark and runs the recovery continuation; the enclosing
        // transaction survives. PG and SQLite both forbid true nested transactions, so the savepoint is the only
        // nesting mechanism — RollbackToSavepointAsync is the IDbContextTransaction verb, read off the ambient
        // Database.CurrentTransaction the bracket already opened (a leg invoked outside a transaction is a caller
        // contract violation the null-conditional surfaces as the leg's own throw, never a silent no-op).
        public async ValueTask<T> RollbackTo<T>(
            string mark, Func<DbContext, CancellationToken, ValueTask<T>> leg,
            Func<DbContext, CancellationToken, ValueTask<T>> recover, CancellationToken token) {
            try { return await leg(db, token); }
            catch (DbException) {
                await db.Database.CurrentTransaction!.RollbackToSavepointAsync(mark, token);
                return await recover(db, token);
            }
        }
    }
}
```

## [03]-[CONTENTION_CLASS]

- Owner: `ContentionClass` the `[SmartEnum<string>]` SQLSTATE contention axis keyed on the `PostgresErrorCodes` constant, each row carrying its `Absorbs` flag (verified present in the connector's built-in `PostgresException.IsTransient` set, so the row DOCUMENTS the auto-retry floor and grounds the `RetrySafety.Idempotent` contract, never re-asserts the retry) and its `Extra` flag (a code OUTSIDE that floor an operator elects into `errorCodesToAdd` — empty for contention because the floor is complete); `Contention` the static surface owning the one real EF-strategy seam the rail does not already own — `RetryCodes` projecting the `Extra` SQLSTATE set the profile row passes to `EnableRetryOnFailure`. Escaped-fault projection is NOT a member here: the rail's `StoreFault.From`/`OfSqlState` is the package's one total SQLSTATE classifier, and a transaction-local copy is the duplicate surface `Query/rail#OPERATION_ALGEBRA` names the deleted form.
- Cases: `ContentionClass` `serialization` (`40001`) | `deadlock` (`40P01`) | `lock-unavailable` (`55P03`) | `object-in-use` (`55006`) — all four carry `Absorbs: true` because the verified Npgsql `IsTransient` set already retries them and none carries `Extra` because that floor is complete; the rows are the documented retry floor the idempotency contract reads, not a second strategy. The EF tracked-graph `DbUpdateConcurrencyException` optimistic-token violation has no SQLSTATE row — it folds through the rail's `StoreFault.From` to `StoreFault.Concurrency` 7001 because a lost update no retry can fix.
- Entry: `public static ICollection<string> RetryCodes()` — the `Extra` SQLSTATE set the `Store/profiles#PROFILE_ROWS` pg row passes to `EnableRetryOnFailure(maxRetryCount, maxRetryDelay, errorCodesToAdd: Contention.RetryCodes())`; it is empty for contention because the connector's `IsTransient` floor already covers `40001`/`40P01`/`55P03`/`55006`, so this is the honest seam a deployment-specific transient outside that floor would populate, never a re-listing of the floor as a hidden second strategy and the only configuration-time member this owner adds. A fault that escapes the exhausted strategy is projected by the rail's `StoreFault.From` (which folds a `PostgresException` through `OfSqlState` into the typed `Conflict` 7711/`Defect` 7712/`Contention` 7713/`Escaped` 7714 arms straight off the structured `ConstraintName`/`ColumnName`/`TableName` fields with zero message parsing) — there is no transaction-local `Classify`, because a second SQLSTATE dispatch is the duplicate classifier the rail forecloses.
- Auto: retry is owned end-to-end by the connector's `NpgsqlRetryingExecutionStrategy` — `ShouldRetryOn(exception)` is `(exception is PostgresException pg && errorCodesToAdd.Contains(pg.SqlState)) || NpgsqlTransientExceptionDetector.ShouldRetryOn(exception)`, and the detector reads `PostgresException.IsTransient` whose verified set is `40001`/`40P01`/`55P03`/`55006`/`55000` plus the connection and `53*` resource classes; so the contention floor is the connector's already and the only caller steering is the additive `errorCodesToAdd` set, never a per-fault disposition the strategy consults at runtime — a classifier returning a `Retry`/`Contend` verdict the strategy reads is architecturally impossible because the strategy has no such callback, and that decorative verdict is what this rebuild deletes; `Contention.RetryCodes()` is therefore the one configuration-time seam (empty for contention, the floor being complete), and a fault that ESCAPES the exhausted strategy folds through the rail's `StoreFault.From` to its typed arm carrying the structured `PostgresException` evidence (`ConstraintName`/`ColumnName`/`TableName`); because `55P03`/`55006` ARE in the retry floor, a `NOWAIT` failure auto-retries under the strategy too, so a fail-fast-once dequeue uses `SKIP LOCKED` (zero rows, no fault) rather than `NOWAIT` whose `55P03` the floor retries — the `WaitPolicy` choice is load-bearing against the connector floor; the `Sync/coordination#FENCED_CAS` `Cas` closure rides the connector's serialization retry transparently — a `40001`-aborted conditional upsert re-runs the closed `TState`, and its `xmin` reject-lower is the in-process `CoordFault.Stale` check on a `SaveChangesAsync`-bypassing `MergeWithOutputAsync` path, so it never enters fault classification at all.
- Receipt: an escaped contention fault rides the rail's `StoreFault.Contention` 7713 (a lock class that exhausted the connector's retries, carrying the SQLSTATE and relation) or `StoreFault.Concurrency` 7001 (the `DbUpdateConcurrencyException`); a connector-absorbed serialization retry rides the settled `StoreFact.Transaction` kind carrying the absorbed SQLSTATE in its `Subject` through the interceptor's failed-command hook, never a second signal owner.
- Packages: Npgsql, Microsoft.EntityFrameworkCore.Sqlite, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new documented retry-floor class is one `ContentionClass` row with `Absorbs: true` (a mirror of the connector's `IsTransient`, never a re-implementation); a deployment-specific transient outside that floor is one row with `Extra: true` (it joins `RetryCodes()` automatically); a new escaped-fault projection is one `OfSqlState` arm in the rail, never a transaction-local one; zero new surface — a parallel deadlock error family, a second retry classifier, a `RetryDisposition` verdict no strategy consumes, or a sync-merge-divergent verdict is the deleted form because retry is the connector's strategy and faults fold through the rail's one classifier.
- Boundary: retry classification is the SQLSTATE-class fold the postgres `[FAULT_DISPOSITION]` law names — a typed projection over `PostgresErrorCodes` constants reading the structured `PostgresException` fields, OWNED ONCE in the rail's `StoreFault.From`/`OfSqlState` and consumed here, never re-implemented and never the sync-merge `ConflictVerdict` whose `Applies` flag answers op-application, not retry-eligibility (mapping a unique-violation to a merge `RemoteWin` is a semantic category error, the illusory cross-page unification this rebuild deletes; the stale `[TRANSACTION_CONCURRENCY_CONTROL]`/`[CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE]` cards that mandate the `ConflictVerdict` reuse here are superseded by this re-founding); the connector's built-in `IsTransient` set (`40001`/`40P01`/`55P03`/`55006`/`55000` plus connection/resource classes) is the complete contention retry floor the page never re-implements — `RetryCodes()` is empty for contention and only ever ADDS a deployment-specific non-contention transient an operator elects, and an admission defect (foreign-key, check, not-null, a 22-prefixed data exception, a `23505` unique violation) is never retried and the rail's `OfSqlState` mints its typed `Conflict`/`Defect`/`Escaped` arm with constraint/column identity rather than a degraded `Text` 7000; because the connector retries `55P03` a `NOWAIT` no-wait intent is defeated under the strategy, so `SKIP LOCKED` (zero rows, no fault) is the fail-fast-once dequeue and `NOWAIT` is the explicit-abort intent a non-retrying caller spells outside the retrying strategy; AppHost owns the hop law and the database stays excluded from it.

```csharp signature
// The SQLSTATE contention axis: each row keyed on the PostgresErrorCodes constant. Absorbs mirrors the
// connector's built-in IsTransient set, which the verified Npgsql 10 implementation covers in full for
// contention (40001/40P01/55P03/55006/55000) — the row exists to DOCUMENT the auto-retry floor, never to
// re-assert it (re-implementing IsTransient is the deleted double-strategy). Extra marks a code OUTSIDE
// that floor an operator elects into errorCodesToAdd; no contention class qualifies because the floor is
// complete, so Extra is empty here and the field is the honest seam for a deployment-specific transient.
// There is NO Retry/Contend verdict ShouldRetryOn reads — it takes only the additive code set, so a
// verdict-returning classifier is the deleted decorative shape. The real value is the Absorbs mirror that
// grounds the RetrySafety.Idempotent contract (a tail under an absorbed class MUST be idempotent); escaped-fault
// projection is the rail's StoreFault.From, not a member here.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class ContentionClass {
    public static readonly ContentionClass Serialization = new(PostgresErrorCodes.SerializationFailure, Absorbs: true, Extra: false);
    public static readonly ContentionClass Deadlock = new(PostgresErrorCodes.DeadlockDetected, Absorbs: true, Extra: false);
    public static readonly ContentionClass LockUnavailable = new(PostgresErrorCodes.LockNotAvailable, Absorbs: true, Extra: false);
    public static readonly ContentionClass ObjectInUse = new(PostgresErrorCodes.ObjectInUse, Absorbs: true, Extra: false);

    public bool Absorbs { get; }   // verified in the connector's IsTransient — a tail under it must be idempotent
    public bool Extra { get; }     // outside IsTransient and operator-elected — empty for contention, the floor is complete
}

public static class Contention {
    // The additive SQLSTATE set the profile pg row threads into EnableRetryOnFailure — projected directly off the
    // Extra-flagged ContentionClass rows, empty for contention because the connector's IsTransient floor already
    // covers 40001/40P01/55P03/55006/55000; this is the seam a deployment-specific transient (not a contention
    // class) would populate, never a re-listing of the floor and never a second forwarding hop. This is the ONE
    // capability this owner adds over the rail: the documented retry floor (the Absorbs mirror that grounds the
    // Idempotent contract) plus the operator-elected Extra set. Escaped-fault projection is NOT re-implemented
    // here — the rail's StoreFault.From is the package's one total SQLSTATE classifier (it folds a PostgresException
    // through OfSqlState into the typed Conflict 7711/Defect 7712/Contention 7713/Escaped 7714 arms straight off
    // the structured ConstraintName/ColumnName/TableName fields), and a second classifier carrying the same
    // SQLSTATE dispatch with weaker arms is the duplicate surface Query/rail#OPERATION_ALGEBRA names the deleted
    // form; an escaped contention fault on this lane folds through that one From, never a transaction-local copy.
    public static ICollection<string> RetryCodes() =>
        ContentionClass.Items.Where(static c => c.Extra).Select(static c => c.Key).ToArray();
}
```

## [04]-[TWO_PHASE_COMMIT]

- Owner: `TwoPhaseDecision` the `[SmartEnum<string>]` second-phase verdict (`Commit` | `Rollback`) carrying its `COMMIT PREPARED`/`ROLLBACK PREPARED` verb; `PreparedTxn` the in-doubt prepared-transaction record projected from `pg_prepared_xacts` carrying the gid, owner, database, prepared instant, and the `transaction` xid the drain joins against `pg_locks.virtualtransaction` to read the locks a prepared transaction still holds; `TwoPhase` the static surface owning the one polymorphic second-phase `Resolve` over `TwoPhaseDecision` plus the `pg_prepared_xacts` in-doubt read and drain — the first-phase `PREPARE TRANSACTION` is owned by `Transactions.Scoped`'s `Close` arm where the participant transaction is open, so this surface is the coordinator-facing read+resolve, never a second prepare site.
- Entry: the first phase is `Transactions.Scoped` under a `TxnScope.Prepared` scope (its `Close` arm runs the engine-gated `PREPARE TRANSACTION '<gid>'`, leaving the work durable-but-uncommitted), not a standalone method, because every participant prepares through the one rail that opened its transaction; `public static IO<Unit> Resolve(DbContext db, GlobalTxnId gid, TwoPhaseDecision decision)` runs the decision's verb — `COMMIT PREPARED '<gid>'` or `ROLLBACK PREPARED '<gid>'` — as one polymorphic second-phase entrypoint the coordinator's last-recorded decision value drives, never two sibling `Commit`/`Rollback` names and never a `bool` knob reconstructing the verb at the call site; `public static IO<Seq<PreparedTxn>> InDoubt(DbContext db)` reads `pg_prepared_xacts` (gid, owner, database, prepared, transaction) so the drain reconciles every prepared-but-unresolved transaction and can join its held locks; `public static IO<Seq<PreparedTxn>> Drain(DbContext db, Func<PreparedTxn, TwoPhaseDecision> verdict)` is the drain-conductor convenience folding `InDoubt` through the coordinator's per-gid verdict and resolving each in one pass, returning the resolved set for the drain receipt — the conductor supplies the verdict from its last-recorded decision log, this owner owns only the read and the resolve.
- Auto: the 2PC arm activates only on the `TxnScope.Prepared` flag carrying a `GlobalTxnId` so a cross-store commit prepares each participant through `Transactions.Scoped`'s `Close` arm, the coordinator commits-prepared on all-prepared or rolls-back-prepared on any participant abort through `Resolve(db, gid, decision)`, and a crash between prepare and the second-phase resolution leaves the transaction in `pg_prepared_xacts` for the drain to reconcile; the in-doubt set crosses to the AppHost drain conductor through the `Query/transaction ← Rasm.AppHost/Runtime # [PORT]: drain 2PC in-doubt set` seam as a Stores-band (300) `DrainParticipantPort` consumer so the recovery reconciliation is the drain owner's concern — the conductor reads this owner's `InDoubt` projection and resolves each prepared transaction through `TwoPhase.Resolve(db, gid, decision)` per the coordinator's last-recorded `TwoPhaseDecision` while store writes stay open at band 300, never a managed XA transaction manager and never a Persistence-side recovery loop; the `max_prepared_transactions` GUC floor is the engine-side precondition the drain assumes, verified read-only through `Store/provisioning#CLUSTER_CONFIG` `ClusterConfig.Verify`; SQLite carries no 2PC so a `Prepared` scope on the embedded profile is the typed `StoreFault.Unsupported` 7003 rejection `Scoped`'s `Close` arm raises because its live connection is not an `NpgsqlConnection`, never a silent single-phase fallback.
- Receipt: every 2PC signal rides the one settled `StoreFact.Transaction` kind — a prepare carries the global id in its `Subject`, a second-phase resolution carries the id plus the `TwoPhaseDecision` key — and an in-doubt reconciliation rides the AppHost drain receipt, never a second drain owner and never a dotted fact kind the closed `StoreFact` vocabulary does not carry.
- Packages: Npgsql, Microsoft.EntityFrameworkCore.Sqlite, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new second-phase outcome is one `TwoPhaseDecision` row carrying its verb; a new participant is one `Transactions.Scoped` call under a `TxnScope.Prepared` scope; zero new surface — a managed XA coordinator, a second drain owner, a parallel prepared-transaction table, a standalone prepare method beside `Scoped`'s `Close`, a `bool commit` knob, or sibling `Commit`/`Rollback` names are the deleted form because 2PC rides the PG-native `PREPARE TRANSACTION` the rail owns and the in-doubt set drains through the one AppHost drain port.
- Boundary: 2PC is PG-native `PREPARE TRANSACTION`/`COMMIT PREPARED` raw SQL through the typed-SQL door, never a managed `System.Transactions` XA coordinator — the cross-store atomic commit is the prepared-transaction protocol the engine owns and this owner folds; the global transaction id is a validated `GlobalTxnId` `[ValueObject<string>]` because the gid lands in a utility statement taking no bound parameter, so the value is admitted once against a strict charset and a raw-string interpolation is the injection-prone deleted form; the in-doubt set is reconciled by the AppHost drain conductor through the seam so the recovery half is the drain owner's concern and this owner only projects `InDoubt`/`Drain` from `pg_prepared_xacts` and names the `Resolve` decision, never owning a second drain loop — the `PreparedTxn.Xid` is the join key the conductor uses against `pg_locks.virtualtransaction` to read a lingering prepared transaction's held locks, the diagnostic the bare gid-and-owner projection omitted; a `Prepared` scope on a non-2PC engine (SQLite) is a typed `StoreFault.Unsupported` 7003 rejection at the rail because the lane-by-profile capability denies the shape, never a silent single-phase fallback; the global transaction id is the one cross-store coordination key the coordinator threads, never a per-participant id.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class TwoPhaseDecision {
    public static readonly TwoPhaseDecision Commit = new("commit", "COMMIT PREPARED");
    public static readonly TwoPhaseDecision Rollback = new("rollback", "ROLLBACK PREPARED");

    public string Verb { get; }
}

// Xid is the pg_prepared_xacts `transaction` column the drain joins to pg_locks.virtualtransaction to
// read the locks a still-prepared transaction holds (a long in-doubt transaction blocks vacuum and may
// hold an exclusive table lock the operator must see); Database scopes the gid to its owning database.
public readonly record struct PreparedTxn(GlobalTxnId GlobalId, string Owner, string Database, uint Xid, Instant Prepared);

public static class TwoPhase {
    public const string InDoubtProbe = "SELECT gid, owner, database, transaction::text::bigint AS xid, prepared FROM pg_prepared_xacts";

    // The first phase (PREPARE TRANSACTION, engine-gated) is owned by Transactions.Scoped's Close arm, where the
    // participant's transaction is open — a standalone prepare over an ambient transaction the rail never opened is
    // unreachable because every store op runs through the rail. TwoPhase owns the coordinator-facing second phase
    // and the recovery read: Resolve runs the one polymorphic verb the last-recorded decision drives, never two
    // sibling Commit/Rollback names and never a bool knob reconstructing the verb at the call site.
    public static IO<Unit> Resolve(DbContext db, GlobalTxnId gid, TwoPhaseDecision decision) =>
        IO.liftVAsync<Unit>(async env => {
            _ = await db.Database.ExecuteSqlRawAsync($"{decision.Verb} '{gid.Value}'", env.Token);
            return unit;
        });

    // The forward-only DbDataReader cursor is the platform-forced statement seam: the while-loop materializes
    // pg_prepared_xacts rows the only way the ADO reader exposes, off a raw command that bypasses EF mapping so
    // the timestamptz reads as DateTimeOffset before the NodaTime lift. The gid is admitted through the
    // non-throwing TryCreate because pg_prepared_xacts can hold a foreign XA participant's gid violating this
    // system's charset — a foreign gid is skipped (this conductor resolves only its own coordinated gids), never
    // a throw inside the reader that aborts the whole drain over one alien row.
    public static IO<Seq<PreparedTxn>> InDoubt(DbContext db) =>
        IO.liftVAsync<Seq<PreparedTxn>>(async env => {
            await using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = InDoubtProbe;
            await using var reader = await cmd.ExecuteReaderAsync(env.Token);
            var rows = Seq<PreparedTxn>();
            while (await reader.ReadAsync(env.Token))
                if (GlobalTxnId.TryCreate(reader.GetString(0), out var gid))
                    rows = rows.Add(new PreparedTxn(
                        gid, reader.GetString(1), reader.GetString(2),
                        (uint)reader.GetFieldValue<long>(3),
                        Instant.FromDateTimeOffset(reader.GetFieldValue<DateTimeOffset>(4))));
            return rows;
        });

    // The drain-conductor pass: read the in-doubt set, fold each gid through the coordinator's recorded
    // verdict, resolve in one sweep, and return the resolved set for the drain receipt. The conductor owns
    // the verdict (its decision log), this owner owns the read and the resolve — no Persistence-side loop.
    public static IO<Seq<PreparedTxn>> Drain(DbContext db, Func<PreparedTxn, TwoPhaseDecision> verdict) =>
        InDoubt(db).Bind(rows =>
            rows.Traverse(row => Resolve(db, row.GlobalId, verdict(row)).Map(_ => row)).As());
}
```

| [INDEX] | [CONCERN]          | [SURFACE]                                            | [LAW]                                                              |
| :-----: | :----------------- | :-------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | isolation          | `IsolationPolicy.Begin` + GUC bundle                | data, never a per-call literal; PG level vs SQLite IMMEDIATE; `lock_timeout`/`statement_timeout` bound the bracket |
|  [02]   | row locking        | `RowLock(LockStrength × WaitPolicy × Of)` → `AsPostgreSQL().SubQueryTableHint(hint, hint2, Sql.SqlID[])` | server-side `FOR UPDATE SKIP LOCKED` via linq2db's typed hint; strength ⟂ wait, OF as typed `Sql.SqlID[]`, never a hand-built clause string |
|  [03]   | coordination lock  | `LockMode.Advisory` real `pg_advisory_xact_lock`    | executed statement releasing on txn end, never a SQL comment       |
|  [04]   | nested rollback    | `Savepoint` ladder + EF `AutoSavepointsEnabled`     | inner leg rolls to mark, enclosing txn survives                    |
|  [05]   | retry + fault fold | `Contention.RetryCodes()` → `EnableRetryOnFailure`; escaped fault → rail `StoreFault.From` | connector `IsTransient` owns the full contention floor; `RetryCodes` adds only extra non-contention transients; the rail's one `OfSqlState` projects escaped faults into typed `Conflict`/`Defect`/`Contention` arms, never a transaction-local classifier |
|  [06]   | retry safety       | `RetrySafety` `[Union]` → `verifySucceeded` fold     | `Idempotent` replays `null`; `Verified(probe)` re-checks an ambiguous commit; `NonRetryable` is construction-gated to a non-retrying isolation — no `(bool, Option)` double-apply state |
|  [07]   | cross-store commit | `TwoPhase` `PREPARE`/`Resolve`/`Drain` + `InDoubt`  | PG-native 2PC, gid validated, `Xid` joins `pg_locks`, drains through AppHost |

## [05]-[RESEARCH]

- [TXN_ISOLATION_PROBE]: the live-PG18 begin round-trip for the four `IsolationPolicy` rows — the `SERIALIZABLE READ ONLY DEFERRABLE` consistent-read snapshot that never raises a serialization failure, the `SET LOCAL lock_timeout`/`statement_timeout`/`idle_in_transaction_session_timeout` bundle bounding the bracket, the SQLite `BEGIN IMMEDIATE`-vs-deferred mapping, and the `RowLock` `FOR UPDATE SKIP LOCKED`/`NOWAIT`/`FOR SHARE SKIP LOCKED` work-queue-dequeue contract under concurrent dequeuers — whether `RowLockQuery.Locked` over the bridge's `AsPostgreSQL().SubQueryTableHint(PostgreSQLHints.ForUpdate, PostgreSQLHints.SkipLocked, Targets)` lowers the `FOR UPDATE … SKIP LOCKED` clause into the bridged SELECT and the `SKIP LOCKED` skip is exact under contention, proven before the lock fence pins.
- [ADVISORY_LOCK_PROBE]: the `pg_advisory_xact_lock`/`pg_try_advisory_xact_lock` (single-key and two-int-key, share and exclusive) acquisition-and-auto-release round-trip — whether the `AdvisoryLock.Acquire()` statement holds the cross-row coordination lock for the transaction's lifetime and releases on commit/abort without an explicit unlock, and whether the `Try` form returns `false` rather than blocking under contention.
- [TWO_PHASE_DRAIN]: the `Transactions.Scoped` `Close`-arm `PREPARE TRANSACTION` (under a `TxnScope.Prepared` scope) plus `Resolve`/`Drain`/`pg_prepared_xacts` 2PC round-trip and the in-doubt reconciliation the AppHost drain conductor runs against a crash between prepare and second-phase — the `max_prepared_transactions` GUC floor verified through `ClusterConfig.Verify`, the `GlobalTxnId` charset gate against the PG gid grammar, the `PreparedTxn.Xid` join to `pg_locks.virtualtransaction`, the drain-port in-doubt-set hand-off, AND the EF `IDbContextTransaction` dispose-after-`PREPARE TRANSACTION` interaction (the prepared transaction is dissociated from the session, so the `await using` dispose's rollback is the benign no-transaction-in-progress no-op rather than an error that aborts the prepared work), proven against a live PG18 server before the 2PC fence pins.
- [CONTENTION_RETRY_BAND]: that retry is owned end-to-end by `NpgsqlRetryingExecutionStrategy.ShouldRetryOn` (built-in `PostgresException.IsTransient` over `40001`/`40P01`/`55P03`/`55006`/`55000` plus connection/resource classes, plus the additive `errorCodesToAdd` set), confirming the connector floor already covers every contention class so `Contention.RetryCodes()` is empty for contention and consumes no nonexistent per-fault disposition callback, and confirming a `NOWAIT` `55P03` auto-retries under the strategy (so `SKIP LOCKED` zero-rows is the genuine fail-fast-once dequeue); the rail's one `StoreFault.From`/`OfSqlState` escaped-fault projection carrying `ConstraintName`/`ColumnName`/`TableName` into the typed `Conflict` 7711/`Defect` 7712/`Contention` 7713 arms and the `DbUpdateConcurrencyException` → `StoreFault.Concurrency` 7001 fold, confirming this owner adds NO second SQLSTATE classifier, without resurrecting the deleted sync-merge `ConflictVerdict` cross-coupling and without re-running an admission defect.
