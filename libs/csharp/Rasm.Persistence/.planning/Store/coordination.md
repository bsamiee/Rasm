# [PERSISTENCE_STORE_COORDINATION]

Rasm.Persistence owns the token-VALIDATING fenced-lease coordination store — the ONE durable substrate behind the four AppHost PORT contracts (`Agent/capability` Budget debit, `Runtime/orchestration` step-state CAS, `Wire/outbox` transactional outbox, `Wire/Coordination` CAS+lease+membership) — as one closed `CoordinationOp` `[Union]` dispatched by one `Coordinate.Run` bracket over the generated total `Switch`, exactly the `Element/graph#STORE_RAIL` idiom. Every guarded write folds through ONE fenced-CAS predicate (`pg_advisory_xact_lock` + guarded `UPDATE … RETURNING` in one round trip, the token validated against the row's monotone lease generation so a stale holder is the typed `CoordinationFault.LeaseFenced`, never a lost update); every READ case folds through ONE `Received`-style projection carrying the frame's tenant RLS predicate STRUCTURALLY, so no read leaks cross-tenant in-flight/lease/membership state. The Budget is a fenced predicated compare-and-decrement PER-UNIT VECTOR (`HashMap<string, long>` mirroring the AppHost `CostVector` string keys — the smart-enum key crosses as its STRING, never the AppHost type), every requested unit debited atomically in one statement. The Marten event stream IS the outbox (`ONE_OUTBOX_EGRESS_SPINE` — same-`IDocumentSession` guarantee, so the domain event and its egress obligation commit in one transaction and a second envelope-table outbox is the deleted parallel store); this page mints the durable PER-SINK drain cursor `outbox_cursor(SinkKey, long Sequence)` — distinct from the per-origin `SyncCursor` (`Version/ledger#CHANGEFEED`) — and `OutboxAdvance(Sink, Through)` is the cursor-advance case the `Version/egress` pump calls, forward-only: the pump reads the cursor, coordination never reads the pump. Coordination composes Marten `FetchForWriting`/`QueueSqlCommand` + Npgsql `pg_advisory_xact_lock` + LISTEN/NOTIFY — never a second event store, never a distributed-lock sidecar (`DistributedLock.Postgres` carries no fencing token; the token-validating CAS is strictly stronger). The op-union, fencing tokens, membership rows, balance vectors, and receipts are Persistence-OWNED types AppHost's `Wire/Coordination`/`Wire/outbox` adapters DECODE — no AppHost type crosses down; tenant, wall clock, and correlation ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame, and a `ClockPolicy`/`CorrelationId`/`TenantContext` parameter on any signature here is the named strata inversion. `FaultBand` arrives from `Element/graph#FAULT_TABLES`; `IValidityEvidence`/`ValidityClaim` from the `Rasm` kernel; `IDocumentSession`/`NpgsqlDataSource` from the substrate.

## [01]-[INDEX]

- [01]-[COORDINATION_OP]: the closed `CoordinationOp` family, the key/token/state vocabularies, the ONE fenced-CAS write fold and the ONE `Received` read projection, the per-unit-vector Budget debit, the `CoordinationReceipt` validity fold, and the 8430 fault band.
- [02]-[OUTBOX_CURSOR]: the `ONE_OUTBOX_EGRESS_SPINE` law, the per-sink `outbox_cursor` row, the `OutboxAdvance` at-least-once advance CAS, and the LISTEN/NOTIFY pump wake.

## [02]-[COORDINATION_OP]

- Owner: `CoordinationOp` the closed `[Union]` every durable coordination interaction is a value in — five write cases through the one fenced-CAS fold, four READ cases through the one `Received` projection, plus the `#OUTBOX_CURSOR` advance; `LeaseToken` the `[ValueObject<long>]` fencing generation (Kleppmann token — minted monotonically, validated on every guarded write, never merely issued); `WorkflowKey`/`StepKey`/`LeaseKey`/`HolderId`/`MembershipKey`/`MemberId`/`SinkKey` the `[ValueObject<string>]` key vocabulary; `StepState` the `[SmartEnum<string>]` step lifecycle whose `Terminal` column gates re-entry; `CoordRow` the one loaded-row projection every READ case returns; `CoordinationReceipt` the per-op typed evidence implementing the kernel `IValidityEvidence` (`IsValid` is one `ValidityClaim.All` fold — a hand-rolled `&&` chain is the deleted form); `CoordinationFault` the 8430 band; `Coordinate` the static surface owning the one bracket.
- Cases: `BudgetDebit(HashMap<string, long> Debit)` the per-unit-vector fenced compare-and-decrement (`capability.md` `CostVector` crosses as its `[SmartEnum<string>]` STRING key, so the row is `HashMap<string, long>` per unit and AppHost maps its smart-enum at the boundary — a scalar debit is falsified by the multi-unit consumer); `StepStateCas(WorkflowKey, StepKey, StepState Expected, StepState Next)` the orchestration step transition; `StepStateInFlight(WorkflowKey)` READ (the `CrashResume` scan — every non-terminal step of a workflow); `StepStateLoad(WorkflowKey, StepKey)` READ; `LeaseAcquire(LeaseKey, HolderId, Duration Ttl)` MINTS the generation monotonically (`generation + 1` via PG row-CAS `RETURNING generation` — the mint side that makes the token VALIDATED); `LeaseRenew(LeaseKey, LeaseToken, Duration Ttl)` and `LeaseRelease(LeaseKey, LeaseToken)` re-validate the held token; `ExpiredScan` READ (orphan-reclaim — every lease whose deadline trails `frame.Now()`); `MembershipUpsert(MembershipKey, MemberId, Duration Ttl)` the lease-expiring membership row (`MembershipView.Serving`, `Rasm.AppHost/Wire/coordination.md`, is the in-process consumer); `MembershipScan(MembershipKey)` READ; `OutboxAdvance(SinkKey Sink, long Through)` the `#OUTBOX_CURSOR` case.
- Entry: `public static IO<Fin<CoordinationReceipt>> Run(IDocumentSession session, CoordinationOp op, Option<LeaseToken> held, ProjectionContext frame)` is the one rail — the generated total `CoordinationOp.Switch` dispatches each case (compile-time exhaustive, a new op breaks the build here, never a runtime-silent `_` arm); every guarded write composes `Fenced` (one `QueueSqlCommand` batch: `pg_advisory_xact_lock(hashtext(@key))` then the guarded `UPDATE … WHERE tenant = @tenant AND fence <= @token AND <case predicate> … RETURNING` — lock and CAS share the Marten transaction, auto-released at commit) and every READ composes `Received` (the tenant-guarded `SELECT` projection to `Seq<CoordRow>`); `SaveChangesAsync` commits the guarded write WITH any same-session domain events, so a step transition and the event it consequences are one transaction.
- Auto: the fencing law is structural — a guarded row carries the highest lease generation it has observed (`fence`), the write predicate `fence <= @token` rejects a token older than that watermark and the write stamps `fence = @token`, so a paused holder resuming with a superseded token is `LeaseFenced(stale, current)` read off the zero-row CAS plus one fence read, never a silent overwrite; `LeaseAcquire` takes the advisory lock on the lease key, then `UPDATE lease SET generation = generation + 1, holder = @holder, until = @until WHERE key = @key AND (holder = @holder OR until < @now) RETURNING generation` — an unexpired foreign hold returns zero rows and rails `LeaseExpired`-inverse refusal as `LeaseFenced`, an expired hold is reclaimed in the same statement; the Budget debit runs ONE statement over every requested unit — `UPDATE budget_ledger SET balance = balance - d.debit FROM (VALUES …) AS d(unit, debit) WHERE tenant = @tenant AND budget_ledger.unit = d.unit AND fence <= @token AND balance >= d.debit RETURNING budget_ledger.unit, balance` — and a returned-row count short of the requested unit count rolls back and rails `BudgetExhausted(unit, requested, available)` for the first refusing unit, so the vector debits atomically or not at all; every READ carries `tenant = @tenant` structurally (the same guard the writes hold); the receipts project PER-OP with zero follow-up reads — `BudgetDebit` returns the POST-debit balance vector the metering consumer needs, a CAS/lease/membership write returns its committed row, a READ returns its loaded rows.
- Receipt: a debit rides `store.coordination.debit` carrying the post-debit vector; a step CAS rides `store.coordination.step`; a lease verb rides `store.coordination.lease` carrying the generation; a membership upsert rides `store.coordination.member`; a READ rides `store.coordination.read` carrying the row count; the cursor advance rides `store.coordination.outbox` (`#OUTBOX_CURSOR`).
- Packages: Marten (`IDocumentSession.QueueSqlCommand`/`SaveChangesAsync`/`FetchForWriting`), Npgsql (`pg_advisory_xact_lock`/`pg_try_advisory_xact_lock` + guarded `UPDATE … RETURNING` as composed SQL, `pg_notify`), Rasm (`IValidityEvidence`/`ValidityClaim`), LanguageExt.Core (`IO`/`Fin`/`HashMap`/`Seq`), NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new coordination concern is one `CoordinationOp` case plus one arm in the generated total `Switch` — a write case composes `Fenced` with its case predicate as row data, a read case composes `Received` with its filter; a new budget unit is one ledger row (the vector statement already spans N units); a new step lifecycle state is one `StepState` row; zero new surface — a second lease store, a distributed-lock sidecar, a per-port service family, an envelope-table outbox, a scalar-debit sibling beside the vector, or a read surface per consumer is the deleted form because the op family discriminates by value shape, the fenced predicate and the `Received` projection are owned once, and the four AppHost ports decode ONE op-union.
- Boundary: the four PORT rows are AppHost→Persistence READs/decodes (correct HOST-BOUNDARY→APP-PLATFORM direction) — `Agent/capability` debits the Budget vector, `Runtime/orchestration` drives `StepStateCas`+`StepStateInFlight` (`CrashResume` reads the in-flight scan), `Wire/outbox` rides the same-transaction outbox spine, `Wire/Coordination` drives CAS+lease+membership and `MembershipView.Serving` folds the membership rows in-process — no AppHost type crosses down and no Persistence signature names `ClockPolicy`/`CorrelationId`/`TenantContext` ([A.1]); the fenced-CAS is strictly stronger than any lease library because the token is VALIDATED at every guarded write, not merely held — `DistributedLock.Postgres` (no fencing token) and `WolverineFx` (envelope-table outbox beside the stream-IS-outbox law) stay the recorded rejections; the advisory lock is the `_xact_` family (auto-released at transaction end — a session lock requiring explicit unlock is the leak form); deadline comparisons read `frame.Now()` (the injected clock value), never a wall-clock call; a failed `OutboxAdvance` cursor-CAS is `CoordinationFault.OutboxDrain` — the coordination-side write fault, kept inside this fenced store's rail, NEVER a `Version/egress` `EgressFault` delivery fault.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected

namespace Rasm.Persistence.Store;

// --- [TYPES] ----------------------------------------------------------------------------

// The Kleppmann fencing token: the lease generation minted monotonically by LeaseAcquire and VALIDATED by
// every guarded write's `fence <= @token` predicate — a token is proof of currency, never mere possession.
[ValueObject<long>]
public readonly partial struct LeaseToken;

[ValueObject<string>] public readonly partial struct WorkflowKey;
[ValueObject<string>] public readonly partial struct StepKey;
[ValueObject<string>] public readonly partial struct LeaseKey;
[ValueObject<string>] public readonly partial struct HolderId;
[ValueObject<string>] public readonly partial struct MembershipKey;
[ValueObject<string>] public readonly partial struct MemberId;
[ValueObject<string>] public readonly partial struct SinkKey;

// The orchestration step lifecycle — `Terminal` gates re-entry (a CAS into a terminal state is final; the
// `StepStateInFlight` scan returns every non-terminal step so `CrashResume` resumes exactly the open work).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StepState {
    public static readonly StepState Pending  = new("pending",  terminal: false);
    public static readonly StepState InFlight = new("in-flight", terminal: false);
    public static readonly StepState Done     = new("done",     terminal: true);
    public static readonly StepState Faulted  = new("faulted",  terminal: true);
    public bool Terminal { get; }
    private StepState(string key, bool terminal) : this(key) => Terminal = terminal;
}

// The closed coordination family: five guarded writes through ONE fenced-CAS fold, four tenant-guarded READs
// through ONE `Received` projection, one cursor advance (#OUTBOX_CURSOR). A new concern is a case here, never
// a per-port service.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinationOp {
    private CoordinationOp() { }

    public sealed record BudgetDebit(HashMap<string, long> Debit) : CoordinationOp;
    public sealed record StepStateCas(WorkflowKey Workflow, StepKey Step, StepState Expected, StepState Next) : CoordinationOp;
    public sealed record StepStateInFlight(WorkflowKey Workflow) : CoordinationOp;
    public sealed record StepStateLoad(WorkflowKey Workflow, StepKey Step) : CoordinationOp;
    public sealed record LeaseAcquire(LeaseKey Lease, HolderId Holder, Duration Ttl) : CoordinationOp;
    public sealed record LeaseRenew(LeaseKey Lease, LeaseToken Token, Duration Ttl) : CoordinationOp;
    public sealed record LeaseRelease(LeaseKey Lease, LeaseToken Token) : CoordinationOp;
    public sealed record ExpiredScan : CoordinationOp;
    public sealed record MembershipUpsert(MembershipKey Group, MemberId Member, Duration Ttl) : CoordinationOp;
    public sealed record MembershipScan(MembershipKey Group) : CoordinationOp;
    public sealed record OutboxAdvance(SinkKey Sink, long Through) : CoordinationOp;
}

// --- [MODELS] ---------------------------------------------------------------------------

// The ONE loaded-row projection every READ case returns: key, state/holder, generation, deadline — the four
// slots the in-flight scan, the expired scan, the membership scan, and the step load all project; a per-read
// row record is the deleted form.
public readonly record struct CoordRow(string Key, string State, long Generation, Instant Until);

// The per-sink durable drain cursor (#OUTBOX_CURSOR): one row per EgressSink, advanced only by the fenced
// `OutboxAdvance` CAS — distinct from the per-origin `SyncCursor` (Version/ledger#CHANGEFEED).
public sealed record OutboxCursor(SinkKey Sink, long Sequence) {
    public static OutboxCursor Genesis(SinkKey sink) => new(sink, 0L);
}

// Per-op typed evidence on the kernel validity floor: `IsValid` is ONE `ValidityClaim.All` fold over the
// case's own claims — the post-debit vector is non-negative per unit, a committed write carries a positive
// generation, a read's row count is conserved — never a hand-rolled `&&` chain ([C]).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinationReceipt : IValidityEvidence {
    private CoordinationReceipt() { }

    public sealed record Debited(HashMap<string, long> Balances, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Stepped(WorkflowKey Workflow, StepKey Step, StepState Committed, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Leased(LeaseKey Lease, LeaseToken Token, Instant Until, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Member(MembershipKey Group, MemberId Id, Instant Until, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Advanced(SinkKey Sink, long Through, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Loaded(Seq<CoordRow> Rows, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;

    public bool IsValid => Switch(
        debited:  static c => ValidityClaim.All(ValidityClaim.Of(c.Balances.Values.ForAll(static b => b >= 0L))),
        stepped:  static c => ValidityClaim.All(ValidityClaim.Of(!string.IsNullOrEmpty(c.Step.Value))),
        leased:   static c => ValidityClaim.All(ValidityClaim.Of(c.Token.Value > 0L), ValidityClaim.Of(c.Until > c.At)),
        member:   static c => ValidityClaim.All(ValidityClaim.Of(c.Until > c.At)),
        advanced: static c => ValidityClaim.All(ValidityClaim.Nonnegative(c.Through)),
        loaded:   static c => ValidityClaim.All(ValidityClaim.CountAtLeast(c.Rows.Count, 0)));
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Band 8430 (Element/graph#FAULT_TABLES registry row `Coordination`): a closed [Union] over the KERNEL
// `Rasm.Domain.Expected` — Code derives `FaultBand.Coordination + n` through the registry pointer, so a
// duplicate decade integer fails at type initialization, never prose. `OutboxDrain` is the failed cursor-CAS
// — the coordination-side write fault the V3 pump depends on, never an EgressFault delivery fault.
[Union]
public abstract partial record CoordinationFault : Expected, IValidationError<CoordinationFault> {
    private CoordinationFault() : base() { }

    public sealed record LeaseFenced(LeaseToken Stale, long Current) : CoordinationFault;
    public sealed record CasConflict(WorkflowKey Workflow, StepKey Step, StepState Expected, StepState Found) : CoordinationFault;
    public sealed record BudgetExhausted(string Unit, long Requested, long Available) : CoordinationFault;
    public sealed record LeaseExpired(LeaseKey Lease, HolderId Holder) : CoordinationFault;
    public sealed record MembershipLapsed(MembershipKey Group, MemberId Member) : CoordinationFault;
    public sealed record OutboxDrain(SinkKey Sink, long Through) : CoordinationFault;

    public override int Code => FaultBand.Coordination + Switch(
        leaseFenced:      static _ => 1,
        casConflict:      static _ => 2,
        budgetExhausted:  static _ => 3,
        leaseExpired:     static _ => 4,
        membershipLapsed: static _ => 5,
        outboxDrain:      static _ => 6);

    public override string Message => Switch(
        leaseFenced:      static c => $"<lease-fenced:{c.Stale.Value}<{c.Current}>",
        casConflict:      static c => $"<cas-conflict:{c.Workflow.Value}/{c.Step.Value}:{c.Expected.Key}!={c.Found.Key}>",
        budgetExhausted:  static c => $"<budget-exhausted:{c.Unit}:{c.Requested}>{c.Available}>",
        leaseExpired:     static c => $"<lease-expired:{c.Lease.Value}:{c.Holder.Value}>",
        membershipLapsed: static c => $"<membership-lapsed:{c.Group.Value}:{c.Member.Value}>",
        outboxDrain:      static c => $"<outbox-drain:{c.Sink.Value}@{c.Through}>");

    public override string Category => Switch(
        leaseFenced:      static _ => "Fencing",
        casConflict:      static _ => "Cas",
        budgetExhausted:  static _ => "Budget",
        leaseExpired:     static _ => "Lease",
        membershipLapsed: static _ => "Membership",
        outboxDrain:      static _ => "Outbox");

    public static CoordinationFault Create(string message) => new CasConflict(WorkflowKey.Create("<none>"), StepKey.Create(message), StepState.Pending, StepState.Pending);
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Coordinate {
    // The one rail — the generated total `CoordinationOp.Switch` (compile-time exhaustive, no `_` arm): every
    // guarded write composes `Fenced` with its case predicate as SQL row data, every READ composes `Received`
    // with its tenant-guarded filter, and `SaveChangesAsync` commits the write WITH any same-session domain
    // events (the outbox spine's same-transaction guarantee). `held` is the caller's fencing token — required
    // by every fenced case, `None` legal only on READs and the initial `LeaseAcquire`.
    public static IO<Fin<CoordinationReceipt>> Run(IDocumentSession session, CoordinationOp op, Option<LeaseToken> held, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from outcome in op.Switch(
            budgetDebit:       d => Fenced(session, held, frame, mark, DebitSql(d.Debit), rows => Project(d, rows, frame, mark)),
            stepStateCas:      c => Fenced(session, held, frame, mark, StepCasSql(c), rows => Project(c, rows, frame, mark)),
            stepStateInFlight: q => Received(session, frame, mark, InFlightSql(q.Workflow)),
            stepStateLoad:     q => Received(session, frame, mark, StepLoadSql(q.Workflow, q.Step)),
            leaseAcquire:      a => Fenced(session, None, frame, mark, AcquireSql(a), rows => Project(a, rows, frame, mark)),
            leaseRenew:        r => Fenced(session, Some(r.Token), frame, mark, RenewSql(r), rows => Project(r, rows, frame, mark)),
            leaseRelease:      r => Fenced(session, Some(r.Token), frame, mark, ReleaseSql(r), rows => Project(r, rows, frame, mark)),
            expiredScan:       _ => Received(session, frame, mark, ExpiredSql()),
            membershipUpsert:  m => Fenced(session, held, frame, mark, MemberSql(m), rows => Project(m, rows, frame, mark)),
            membershipScan:    q => Received(session, frame, mark, MemberScanSql(q.Group)),
            outboxAdvance:     a => Fenced(session, held, frame, mark, AdvanceSql(a), rows => Project(a, rows, frame, mark)))
        select outcome;
}
```

| [INDEX] | [POLICY]           | [VALUE]                                       | [BINDING]                                                        |
| :-----: | :----------------- | :-------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | fencing            | `fence <= @token` on every guarded write      | stale token → typed `LeaseFenced`, never a lost update           |
|  [02]   | generation mint    | `LeaseAcquire` row-CAS `RETURNING generation` | monotone `++`; the token is validated, not merely issued         |
|  [03]   | budget shape       | per-unit vector, one statement                | `HashMap<string,long>` mirrors `CostVector` string keys ([A.1])  |
|  [04]   | read guard         | tenant RLS predicate structural on every READ | no cross-tenant in-flight/lease/membership leak                  |
|  [05]   | lock family        | `pg_advisory_xact_lock` (transaction-scoped)  | auto-released at commit; session locks are the leak form         |
|  [06]   | receipt floor      | `IsValid => ValidityClaim.All(...)` per case  | kernel validity fold ([C]); `&&` chains deleted                  |
|  [07]   | port direction     | AppHost decodes Persistence-owned types       | four PORT rows + `MembershipView.Serving`; nothing crosses down  |

## [03]-[OUTBOX_CURSOR]

- Owner: `OutboxCursor` the per-sink durable drain cursor row — `(SinkKey Sink, long Sequence)`, one row per `EgressSink`, so each sink drains the ONE Marten stream independently; the `ONE_OUTBOX_EGRESS_SPINE` law this section NAMES; the `OutboxAdvance` case (`#COORDINATION_OP`) whose fenced CAS is the only cursor writer; the `pg_notify` pump wake emitted in the same transaction.
- Entry: the `Version/egress` pump reads `OutboxCursor.Sequence`, drains `Version/ledger#CHANGEFEED` `ReplayWindow.DurableOps(cursor.Sequence, take)` rows, delivers, and calls `Coordinate.Run(new OutboxAdvance(sink, through), held, frame)` — the advance CAS is `UPDATE outbox_cursor SET sequence = @through WHERE sink = @sink AND tenant = @tenant AND sequence < @through RETURNING sequence`, so a concurrent pump instance's stale advance returns zero rows and rails `CoordinationFault.OutboxDrain(sink, through)` (at-least-once law: the cursor advances only forward, only after delivery confirmation, and a crash between delivery and advance re-drains — the sink's dedup composition absorbs the replay).
- Auto: the Marten event stream IS the outbox — a domain commit and its egress obligation are ONE `SaveChangesAsync` because the committed event itself is the drainable row (`OpLogEntry` projects from it), so there is no envelope table to fill, no relay to poll, and no dual-write gap; the same transaction that advances a cursor or commits an event `pg_notify('rasm_outbox', @sink)`s the channel, and the egress pump's idle connection `WaitAsync` wakes on it (the low-latency wake beside the bounded poll floor — a missed NOTIFY is absorbed by the next poll, so the wake is latency, never correctness); the cursor is PER-SINK so a slow webhook never holds back the NATS drain, and DISTINCT from the per-origin `SyncCursor` (`ledger.md` `#CHANGEFEED`) which positions peer replication, not sink delivery.
- Receipt: a cursor advance rides `store.coordination.outbox` carrying the sink and the through-sequence; the held-cursor stall evidence is the egress pump's (`Version/egress#EGRESS_PUMP` `CursorStall`), never minted here.
- Growth: a new egress sink is ONE `outbox_cursor` row minted on first drain (the advance CAS upserts), zero coordination edits; zero new surface — an envelope-table outbox, a per-sink advance verb, a trigger-based cursor writer, or a coordination-side read of the pump is the deleted form.
- Boundary: forward-only intra-leg edge — `Version/egress` drains this cursor and coordination NEVER reads the pump (the acyclicity proof's one intra-leg egress edge); the failed advance CAS stays `CoordinationFault.OutboxDrain` in THIS band (the cursor write is fenced-store work) while every delivery fault is the pump's `EgressFault`; the presence/awareness lane (`ColumnFamily.Presence`, `durable: false`) never has a cursor row — only `Family.Durable` lanes drain past this cursor.

| [INDEX] | [POLICY]           | [VALUE]                                     | [BINDING]                                                         |
| :-----: | :----------------- | :------------------------------------------ | :---------------------------------------------------------------- |
|  [01]   | outbox spine       | the Marten stream IS the outbox             | same-`IDocumentSession`; envelope tables are the deleted store    |
|  [02]   | cursor grain       | per-sink `outbox_cursor(SinkKey, Sequence)` | distinct from the per-origin `SyncCursor`; slow sinks isolate     |
|  [03]   | advance law        | forward-only CAS, post-confirmation         | at-least-once; replay absorbed by sink dedup                      |
|  [04]   | pump wake          | `pg_notify('rasm_outbox', sink)` same-tx    | latency only — the bounded poll floor owns correctness            |
|  [05]   | edge direction     | egress reads cursor; coordination never reads pump | the one forward-only intra-leg egress edge                 |
