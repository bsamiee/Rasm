# [PERSISTENCE_STORE_COORDINATION]

Rasm.Persistence owns the token-VALIDATING fenced-lease coordination store — the ONE durable substrate behind the four AppHost PORT contracts (`Agent/capability` Budget debit/credit, `Runtime/orchestration` step-state CAS + durable signal, `Wire/outbox` transactional outbox, `Wire/coordination` CAS+lease+membership) — as one closed `CoordinationOp` `[Union]` dispatched by one `Coordinate.Run` bracket over the generated total `Switch`, exactly the `Element/graph#STORE_RAIL` idiom. Every guarded write folds through ONE fenced-CAS predicate (`pg_advisory_xact_lock` + guarded `UPDATE … RETURNING` in one round trip, the token validated against the row's monotone lease generation so a stale holder is the typed `CoordinationFault.LeaseFenced`, never a lost update); every READ case folds through ONE `Received`-style projection carrying the frame's tenant RLS predicate STRUCTURALLY, so no read leaks cross-tenant in-flight/lease/membership state. The Budget is a fenced predicated compare-and-decrement PER-UNIT VECTOR (`HashMap<string, long>` mirroring the AppHost `CostVector` string keys — the smart-enum key crosses as its STRING, never the AppHost type), every requested unit debited atomically in one statement. The Marten event stream IS the outbox (`ONE_OUTBOX_EGRESS_SPINE` — same-`IDocumentSession` guarantee, so the domain event and its egress obligation commit in one transaction and a second envelope-table outbox is the deleted parallel store); this page mints the durable PER-SINK drain cursor `outbox_cursor(SinkKey, long Sequence)` — distinct from the per-origin `SyncCursor` (`Version/ledger#CHANGEFEED`) — and `OutboxAdvance(Sink, Through)` is the cursor-advance case the `Version/egress` pump calls, forward-only: the pump reads the cursor, coordination never reads the pump. Coordination composes one direct `NpgsqlBatch` on the Marten session's transacted connection (the session transaction force-opened first, so lock + CAS + event commit share it; `QueueSqlCommand` is reserved for no-RETURNING side-writes — it defers to `SaveChangesAsync` and surfaces no result set, so it can never carry the RETURNING-vector CAS) + Npgsql `pg_advisory_xact_lock` + LISTEN/NOTIFY — never a second event store, never a distributed-lock sidecar (`DistributedLock.Postgres` carries no fencing token; the token-validating CAS is strictly stronger). The op-union, fencing tokens, membership rows, balance vectors, and receipts are Persistence-OWNED types AppHost's `Wire/coordination`/`Wire/outbox` adapters DECODE — no AppHost type crosses down; tenant, wall clock, and correlation ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame, and a `ClockPolicy`/`CorrelationId`/`TenantContext` parameter on any signature here is the named strata inversion. `FaultBand` arrives from `Element/graph#FAULT_TABLES`; `IValidityEvidence`/`ValidityClaim` from the `Rasm` kernel; `IDocumentSession`/`NpgsqlDataSource` from the substrate.

## [01]-[INDEX]

- [01]-[COORDINATION_OP]: the closed `CoordinationOp` family, the key/token/state vocabularies, the ONE fenced-CAS write fold consuming the `CaseSql` data row and the ONE `Received` read projection, the per-unit-vector Budget debit/credit, the durable-signal cases, the `CoordinationReceipt` validity fold, and the 8430 fault band.
- [02]-[OUTBOX_CURSOR]: the `ONE_OUTBOX_EGRESS_SPINE` law, the per-sink `outbox_cursor` row, the `OutboxAdvance` at-least-once advance CAS, and the LISTEN/NOTIFY pump wake.

## [02]-[COORDINATION_OP]

- Owner: `CoordinationOp` is the closed interaction family; writes fold through `Fenced`, reads through `Received`, and `OutboxAdvance` rides the same fence. `LeaseToken` carries the monotone generation; the key and state vocabularies close their domains. `CoordRow` is the canonical `(key, state, fence, value, until, payload)` projection: `Fence` never aliases a budget balance or cursor sequence, and `Value` carries those case scalars. `CaseSql.For` is the total parameterized SQL generator over the op family, carrying lock key, token requirement, guarded statement, truth statement, and binds as data. `CoordinationReceipt`, `CoordinationFault`, and `Coordinate` own evidence, failures, and execution.
- Cases: `BudgetDebit(HashMap<string, long> Debit)` the per-unit-vector fenced compare-and-decrement (`capability.md` `CostVector` crosses as its `[SmartEnum<string>]` STRING key, so the row is `HashMap<string, long>` per unit and AppHost maps its smart-enum at the boundary — a scalar debit is falsified by the multi-unit consumer); `BudgetCredit(HashMap<string, long> Credit)` the fenced vector increment — the compensation inverse a workflow that must RETURN budget rides, the same one-statement vector shape with no sufficiency gate; `StepStateCas(WorkflowKey, StepKey, StepState Expected, StepState Next)` the orchestration step transition; `StepStateInFlight(WorkflowKey)` READ (the `CrashResume` scan — every non-terminal step of a workflow); `StepStateLoad(WorkflowKey, StepKey)` READ; `SignalPut(WorkflowKey Instance, SignalKey Channel, JsonElement Payload)` the fenced durable-signal upsert the AppHost `Runtime/orchestration#STEP_STATE_SEAM` `StepStateSeam.SignalPut` decodes — one `signal` row per `(workflow, channel)` under the same tenant fence, so a waiting `Signal` step's wake-or-fault decision survives crash, resume, and peer handoff; `SignalLoad(WorkflowKey Instance, SignalKey Channel)` READ (the `StepStateSeam.SignalOf` leg — the loaded row's `Payload` slot carries the channel JSON); `LeaseAcquire(LeaseKey, HolderId, Duration Ttl)` MINTS the generation monotonically (`generation + 1` via PG row-CAS `RETURNING generation` — the mint side that makes the token VALIDATED); `LeaseRenew(LeaseKey, LeaseToken, Duration Ttl)` and `LeaseRelease(LeaseKey, LeaseToken)` re-validate the held token; `ExpiredScan` READ (orphan-reclaim — every lease whose deadline trails `frame.Now()`); `MembershipUpsert(MembershipKey, MemberId, Duration Ttl)` the lease-expiring membership row (`MembershipView.Serving`, `Rasm.AppHost/Wire/coordination.md`, is the in-process consumer); `MembershipRelease(MembershipKey, MemberId)` the explicit fenced departure — a clean shutdown removes its row NOW instead of waiting out the TTL lapse, the AppHost `MembershipView` `Departed` transition's durable half; `MembershipScan(MembershipKey)` READ; `OutboxAdvance(SinkKey Sink, long Through)` the `#OUTBOX_CURSOR` case.
- Entry: `public static IO<Fin<CoordinationReceipt>> Run(IDocumentSession session, CoordinationOp op, Option<LeaseToken> held, ProjectionContext frame, CancellationToken cancellationToken)` is the one rail — the generated total `CoordinationOp.Switch` dispatches each case (compile-time exhaustive, a new op breaks the build here, never a runtime-silent `_` arm); every guarded write composes `Fenced`, which consumes the case's `CaseSql` row STRUCTURALLY — the Marten session transaction force-opened, then ONE direct `NpgsqlBatch` on the session's live connection whose commands the rail itself composes from the row parts: `SELECT pg_advisory_xact_lock(hashtext(@tenant || ':' || @lock))` from `CaseSql.LockKey` (tenant-prefixed, so one tenant's hot lock key never stalls a sibling tenant's), the guarded `UPDATE … WHERE tenant = @tenant AND fence <= @token AND <case predicate> … RETURNING` from `CaseSql.Guarded`, and the trailing tenant-guarded current-truth `SELECT` from `CaseSql.Truth` — lock and CAS share the Marten transaction, auto-released at commit (`QueueSqlCommand` cannot carry this batch because it defers to `SaveChangesAsync` and returns no rows); every READ composes `Received` (the tenant-guarded `CaseSql.Truth` `SELECT` projection to `Seq<CoordRow>`); `SaveChangesAsync(cancellationToken)` commits the guarded write WITH any same-session domain events, so a step transition and the event it consequences are one transaction.
- Auto: the fencing law is structural — a guarded row carries the highest lease generation it has observed (`fence`), the write predicate `fence <= @token` rejects a token older than that watermark and the write stamps `fence = @token`, so a paused holder resuming with a superseded token is `LeaseFenced(stale, current)` read off the zero-row CAS plus the batch's trailing current-truth `SELECT` (one round trip, never a follow-up read), never a silent overwrite; `LeaseAcquire` takes the advisory lock on the lease key, then `UPDATE lease SET generation = generation + 1, holder = @holder, until = @until WHERE key = @key AND (holder = @holder OR until < @now) RETURNING generation` — an unexpired foreign hold returns zero rows and rails `LeaseExpired`-inverse refusal as `LeaseFenced`, an expired hold is reclaimed in the same statement; the Budget debit runs ONE statement over every requested unit AND returns every requested unit's verdict — `WITH requested(unit, debit) AS (VALUES …), verdict AS (SELECT r.unit, r.debit, COALESCE(b.balance, 0) AS balance, (b.unit IS NOT NULL AND b.fence <= @token AND b.balance >= r.debit) AS sufficient FROM requested r LEFT JOIN budget_ledger b ON b.tenant = @tenant AND b.unit = r.unit), applied AS (UPDATE budget_ledger SET balance = budget_ledger.balance - v.debit FROM verdict v WHERE budget_ledger.tenant = @tenant AND budget_ledger.unit = v.unit AND NOT EXISTS (SELECT 1 FROM verdict WHERE NOT sufficient) RETURNING budget_ledger.unit) SELECT v.unit, CASE WHEN NOT EXISTS (SELECT 1 FROM verdict WHERE NOT sufficient) THEN 'sufficient' ELSE CASE WHEN v.sufficient THEN 'sufficient' ELSE 'insufficient' END END, CASE WHEN NOT EXISTS (SELECT 1 FROM verdict WHERE NOT sufficient) THEN v.balance - v.debit ELSE v.balance END FROM verdict v` — the `applied` CTE fires only when EVERY unit is sufficient, so the vector debits atomically or not at all, and the result set carries the refusing unit's identity AND its current balance (the `Project` `BudgetExhausted(unit, requested, available)` fill needs no second read — the missing-from-RETURNING refusal was the un-fillable form this verdict CTE deletes); `BudgetCredit` runs the SAME vector statement with the sufficiency predicate dropped and the sign flipped (`balance + v.credit`, an absent unit row upserted at the credited value), so debit and credit are one statement family, never a sibling rail; `SignalPut` is a fenced `(workflow, channel)` upsert — `INSERT … ON CONFLICT (tenant, workflow, channel) DO UPDATE SET payload = excluded.payload, fence = @token WHERE signal.fence <= @token RETURNING` — so a paused holder's stale re-signal is the typed `LeaseFenced` refusal, never a silent payload overwrite, and `SignalLoad` reads the row's `payload` back through the canonical row's `Payload` slot; `MembershipRelease` is the fenced row delete whose `RETURNING` proves the departure (zero rows on an already-lapsed member is the benign `MembershipLapsed` the caller treats as done); every OTHER fenced batch ends with a trailing tenant-guarded current-truth `SELECT`, so a missed guarded `UPDATE` still returns the row's current generation/state and every typed refusal (`LeaseFenced` current, `CasConflict` found) populates from the ONE round trip; every READ carries `tenant = @tenant` structurally (the same guard the writes hold); the receipts project PER-OP with zero follow-up reads — `BudgetDebit` returns the POST-debit balance vector the metering consumer needs, a CAS/lease/membership write returns its committed row, a READ returns its loaded rows.
- Receipt: a debit rides `store.coordination.debit` and a credit `store.coordination.credit`, both carrying the post-op balance vector; a step CAS rides `store.coordination.step`; a signal upsert rides `store.coordination.signal`; a lease verb rides `store.coordination.lease` carrying the generation; a membership upsert or release rides `store.coordination.member`; a READ rides `store.coordination.read` carrying the row count; the cursor advance rides `store.coordination.outbox` (`#OUTBOX_CURSOR`).
- Packages: Marten (`IDocumentSession.SaveChangesAsync`/transaction control — the fenced batch rides the session's transacted connection; `QueueSqlCommand` only for no-RETURNING side-writes such as the `pg_notify` wake), Npgsql (`NpgsqlBatch` — `pg_advisory_xact_lock` + guarded `UPDATE … RETURNING` + current-truth `SELECT` in one round trip, `pg_notify`), Rasm (`IValidityEvidence`/`ValidityClaim`), LanguageExt.Core (`IO`/`Fin`/`HashMap`/`Seq`), NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new coordination concern is one `CoordinationOp` case plus one arm in the generated total `Switch` — a write case composes `Fenced` with its case predicate as row data, a read case composes `Received` with its filter; a new budget unit is one ledger row (the vector statement already spans N units); a new step lifecycle state is one `StepState` row; zero new surface — a second lease store, a distributed-lock sidecar, a per-port service family, an envelope-table outbox, a scalar-debit sibling beside the vector, or a read surface per consumer is the deleted form because the op family discriminates by value shape, the fenced predicate and the `Received` projection are owned once, and the four AppHost ports decode ONE op-union.
- Boundary: the four PORT rows are AppHost→Persistence READs/decodes (correct HOST-BOUNDARY→APP-PLATFORM direction) — `Agent/capability` debits and credits the Budget vector, `Runtime/orchestration` drives `StepStateCas`+`StepStateInFlight`+`SignalPut`/`SignalLoad` (`CrashResume` reads the in-flight scan; the `StepStateSeam.SignalPut`/`SignalOf` delegates decode the signal cases), `Wire/outbox` rides the same-transaction outbox spine, `Wire/coordination` drives CAS+lease+membership and `MembershipView.Serving` folds the membership rows in-process — no AppHost type crosses down and no Persistence signature names `ClockPolicy`/`CorrelationId`/`TenantContext` ([A.1]); the fenced-CAS is strictly stronger than any lease library because the token is VALIDATED at every guarded write, not merely held — `DistributedLock.Postgres` (no fencing token) and `WolverineFx` (envelope-table outbox beside the stream-IS-outbox law) stay the recorded rejections; the advisory lock is the `_xact_` family (auto-released at transaction end — a session lock requiring explicit unlock is the leak form); deadline comparisons read `frame.Now()` (the injected clock value), never a wall-clock call; a failed `OutboxAdvance` cursor-CAS is `CoordinationFault.OutboxDrain` — the coordination-side write fault, kept inside this fenced store's rail, NEVER a `Version/egress` `EgressFault` delivery fault.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using System.Text.Json;
using Npgsql;
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
[ValueObject<string>] public readonly partial struct SignalKey;
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

// The closed coordination family: guarded writes through ONE fenced-CAS fold, tenant-guarded READs through
// ONE `Received` projection, one cursor advance (#OUTBOX_CURSOR). A new concern is a case here, never a
// per-port service. `SignalPut`/`SignalLoad` are the durable-signal cases the AppHost
// `Runtime/orchestration#STEP_STATE_SEAM` `StepStateSeam.SignalPut`/`SignalOf` delegates decode.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinationOp {
    private CoordinationOp() { }

    public sealed record BudgetDebit(HashMap<string, long> Debit) : CoordinationOp;
    public sealed record BudgetCredit(HashMap<string, long> Credit) : CoordinationOp;
    public sealed record StepStateCas(WorkflowKey Workflow, StepKey Step, StepState Expected, StepState Next) : CoordinationOp;
    public sealed record StepStateInFlight(WorkflowKey Workflow) : CoordinationOp;
    public sealed record StepStateLoad(WorkflowKey Workflow, StepKey Step) : CoordinationOp;
    public sealed record SignalPut(WorkflowKey Instance, SignalKey Channel, JsonElement Payload) : CoordinationOp;
    public sealed record SignalLoad(WorkflowKey Instance, SignalKey Channel) : CoordinationOp;
    public sealed record LeaseAcquire(LeaseKey Lease, HolderId Holder, Duration Ttl) : CoordinationOp;
    public sealed record LeaseRenew(LeaseKey Lease, LeaseToken Token, Duration Ttl) : CoordinationOp;
    public sealed record LeaseRelease(LeaseKey Lease, LeaseToken Token) : CoordinationOp;
    public sealed record ExpiredScan : CoordinationOp;
    public sealed record MembershipUpsert(MembershipKey Group, MemberId Member, Duration Ttl) : CoordinationOp;
    public sealed record MembershipRelease(MembershipKey Group, MemberId Member) : CoordinationOp;
    public sealed record MembershipScan(MembershipKey Group) : CoordinationOp;
    public sealed record OutboxAdvance(SinkKey Sink, long Through) : CoordinationOp;
}

// --- [MODELS] ---------------------------------------------------------------------------

// The ONE loaded-row projection every statement returns: key, state/holder, generation, deadline, payload —
// the canonical shape every guarded `RETURNING` and every truth/read `SELECT` projects, so the fenced rail
// reads one row form structurally; `Payload` fills only on the signal rows (`NULL` everywhere else) and a
// per-read row record is the deleted form.
public readonly record struct CoordRow(string Key, string State, long Fence, Option<long> Value, Instant Until, Option<JsonElement> Payload);

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

    // `Debited` is the balance-vector receipt BOTH budget verbs project — a debit carries the post-debit
    // vector, a credit the post-credit vector; the op case discriminates the verb, never a sibling receipt.
    public sealed record Debited(HashMap<string, long> Balances, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Stepped(WorkflowKey Workflow, StepKey Step, StepState Committed, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Signaled(WorkflowKey Instance, SignalKey Channel, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Leased(LeaseKey Lease, LeaseToken Token, Instant Until, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Member(MembershipKey Group, MemberId Id, Instant Until, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Advanced(SinkKey Sink, long Through, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Loaded(Seq<CoordRow> Rows, Instant At, Guid Correlation, Duration Elapsed) : CoordinationReceipt;

    public bool IsValid => Switch(
        debited:  static c => ValidityClaim.All(ValidityClaim.Of(c.Balances.Values.ForAll(static b => b >= 0L))),
        stepped:  static c => ValidityClaim.All(ValidityClaim.Of(!string.IsNullOrEmpty(c.Step.Value))),
        signaled: static c => ValidityClaim.All(ValidityClaim.Of(!string.IsNullOrEmpty(c.Channel.Value))),
        leased:   static c => ValidityClaim.All(ValidityClaim.Of(c.Token.Value > 0L), ValidityClaim.Of(c.Until > c.At)),
        member:   static c => ValidityClaim.All(ValidityClaim.Of(c.Until >= c.At)),
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
    public sealed record Refused(string Detail) : CoordinationFault;

    public override int Code => FaultBand.Coordination + Switch(
        leaseFenced:      static _ => 1,
        casConflict:      static _ => 2,
        budgetExhausted:  static _ => 3,
        leaseExpired:     static _ => 4,
        membershipLapsed: static _ => 5,
        outboxDrain:      static _ => 6,
        refused:          static _ => 7);

    public override string Message => Switch(
        leaseFenced:      static c => $"<lease-fenced:{c.Stale.Value}<{c.Current}>",
        casConflict:      static c => $"<cas-conflict:{c.Workflow.Value}/{c.Step.Value}:{c.Expected.Key}!={c.Found.Key}>",
        budgetExhausted:  static c => $"<budget-exhausted:{c.Unit}:{c.Requested}>{c.Available}>",
        leaseExpired:     static c => $"<lease-expired:{c.Lease.Value}:{c.Holder.Value}>",
        membershipLapsed: static c => $"<membership-lapsed:{c.Group.Value}:{c.Member.Value}>",
        outboxDrain:      static c => $"<outbox-drain:{c.Sink.Value}@{c.Through}>",
        refused:          static c => $"<coordination-refused:{c.Detail}>");

    public override string Category => Switch(
        leaseFenced:      static _ => "Fencing",
        casConflict:      static _ => "Cas",
        budgetExhausted:  static _ => "Budget",
        leaseExpired:     static _ => "Lease",
        membershipLapsed: static _ => "Membership",
        outboxDrain:      static _ => "Outbox",
        refused:          static _ => "Refused");

    public static CoordinationFault Create(string message) => new Refused(message);
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Coordinate {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.coordination.debit"), StoreSlot.Create("store.coordination.credit"), StoreSlot.Create("store.coordination.step"),
        StoreSlot.Create("store.coordination.signal"), StoreSlot.Create("store.coordination.lease"), StoreSlot.Create("store.coordination.member"),
        StoreSlot.Create("store.coordination.read"), StoreSlot.Create("store.coordination.outbox"));

    // The one rail — the generated total `CoordinationOp.Switch` (compile-time exhaustive, no `_` arm): every
    // guarded write composes `Fenced` with its case predicate as SQL row data, every READ composes `Received`
    // with its tenant-guarded filter, and `SaveChangesAsync` commits the write WITH any same-session domain
    // events (the outbox spine's same-transaction guarantee). `held` is the caller's fencing token — required
    // by every fenced case, `None` legal only on READs and the initial `LeaseAcquire`.
    public static IO<Fin<CoordinationReceipt>> Run(IDocumentSession session, CoordinationOp op, Option<LeaseToken> held, ProjectionContext frame, CancellationToken cancellationToken) =>
        from mark in IO.lift(frame.Mark)
        from sql in IO.lift(() => CaseSql.For(op, frame.Now()))
        from outcome in op.Switch(
            budgetDebit:       d => Fenced(session, held, frame, mark, sql, rows => Project(d, held, rows, frame, mark), cancellationToken),
            budgetCredit:      c => Fenced(session, held, frame, mark, sql, rows => Project(c, held, rows, frame, mark), cancellationToken),
            stepStateCas:      c => Fenced(session, held, frame, mark, sql, rows => Project(c, held, rows, frame, mark), cancellationToken),
            stepStateInFlight: _ => Received(session, frame, mark, sql, cancellationToken),
            stepStateLoad:     _ => Received(session, frame, mark, sql, cancellationToken),
            signalPut:         s => Fenced(session, held, frame, mark, sql, rows => Project(s, held, rows, frame, mark), cancellationToken),
            signalLoad:        _ => Received(session, frame, mark, sql, cancellationToken),
            leaseAcquire:      a => Fenced(session, None, frame, mark, sql, rows => Project(a, None, rows, frame, mark), cancellationToken),
            leaseRenew:        r => Fenced(session, Some(r.Token), frame, mark, sql, rows => Project(r, Some(r.Token), rows, frame, mark), cancellationToken),
            leaseRelease:      r => Fenced(session, Some(r.Token), frame, mark, sql, rows => Project(r, Some(r.Token), rows, frame, mark), cancellationToken),
            expiredScan:       _ => Received(session, frame, mark, sql, cancellationToken),
            membershipUpsert:  m => Fenced(session, held, frame, mark, sql, rows => Project(m, held, rows, frame, mark), cancellationToken),
            membershipRelease: m => Fenced(session, held, frame, mark, sql, rows => Project(m, held, rows, frame, mark), cancellationToken),
            membershipScan:    _ => Received(session, frame, mark, sql, cancellationToken),
            outboxAdvance:     a => Fenced(session, held, frame, mark, sql, rows => Project(a, held, rows, frame, mark), cancellationToken))
        select outcome;

    // The ONE fenced-CAS write fold, consuming the CaseSql row STRUCTURALLY: the Marten session transaction is
    // force-OPENED first (Marten otherwise defers it to `SaveChangesAsync`, which would leave the batch outside
    // the transaction the lock/CAS law requires), then the rail itself composes ONE NpgsqlBatch on the session's
    // live connection — the advisory lock from `sql.LockKey`, the guarded `UPDATE … WHERE tenant = @tenant AND
    // fence <= @token AND <case predicate> RETURNING` from `sql.Guarded`, the trailing current-truth `SELECT`
    // from `sql.Truth` — so the `_xact_` lock auto-releases at commit and the guarded write commits WITH any
    // same-session domain events. Every statement projects `(key, state, fence, value, until, payload)`; applied rows
    // land first, truth rows append, and `Project` discriminates the typed refusal
    // (LeaseFenced / CasConflict / BudgetExhausted / OutboxDrain) off the ONE round trip — an opaque per-case
    // Execute delegate hiding the fence is the deleted form. A provider throw converts HERE, once.
    static IO<Fin<CoordinationReceipt>> Fenced(IDocumentSession session, Option<LeaseToken> held, ProjectionContext frame, long mark, CaseSql sql, Func<Seq<CoordRow>, Fin<CoordinationReceipt>> project, CancellationToken cancellationToken) =>
        sql.RequiresToken && held.IsNone
            ? IO.pure(Fin<CoordinationReceipt>.Fail(new CoordinationFault.Refused("<missing-fence-token>")))
            : IO.liftAsync(async () => {
            await session.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using NpgsqlBatch batch = new((NpgsqlConnection)session.Connection!);
            batch.BatchCommands.Add(Bound("SELECT pg_advisory_xact_lock(hashtext(@tenant || ':' || @lock))", Seq(("lock", (object)sql.LockKey)), held, frame));
            batch.BatchCommands.Add(Bound(sql.Guarded, sql.Binds, held, frame));
            batch.BatchCommands.Add(Bound(sql.Truth, sql.Binds, held, frame));
            sql.Wake.IfSome(channel => batch.BatchCommands.Add(Bound("SELECT pg_notify(@channel, @sink)", sql.Binds.Add(("channel", (object)channel)), held, frame)));
            Seq<CoordRow> rows = await Rows(batch, cancellationToken).ConfigureAwait(false);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return project(rows);
            }) | @catch<IO, Fin<CoordinationReceipt>>(static _ => true, e => IO.pure(Fin<CoordinationReceipt>.Fail(e)));

    // The ONE Received read projection: the tenant-guarded `sql.Truth` SELECT to `Seq<CoordRow>` — every READ
    // case is a statement + binds row on this one leg, and the loaded rows project through the shared receipt arm.
    static IO<Fin<CoordinationReceipt>> Received(IDocumentSession session, ProjectionContext frame, long mark, CaseSql sql, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => {
            await session.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using NpgsqlBatch batch = new((NpgsqlConnection)session.Connection!);
            batch.BatchCommands.Add(Bound(sql.Truth, sql.Binds, None, frame));
            Seq<CoordRow> rows = await Rows(batch, cancellationToken).ConfigureAwait(false);
            return Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark)));
        }) | @catch<IO, Fin<CoordinationReceipt>>(static _ => true, e => IO.pure(Fin<CoordinationReceipt>.Fail(e)));

    // One bound command per statement: `@tenant` and `@token` ride every case (the structural fence), the
    // case's own binds append after — a lock skipped on the READ leg is the only shape difference.
    static NpgsqlBatchCommand Bound(string statement, Seq<(string Name, object Value)> binds, Option<LeaseToken> held, ProjectionContext frame) {
        NpgsqlBatchCommand command = new(statement) {
            Parameters = {
                new NpgsqlParameter<string>("tenant", frame.Tenant.ToString("x32")),
                new NpgsqlParameter<long>("token", held.Map(static t => t.Value).IfNone(0L)),
            },
        };
        foreach ((string name, object value) in binds) { command.Parameters.Add(new NpgsqlParameter(name, value)); }
        return command;
    }

    // Drain every result set into the canonical row shape — the lock scalar yields no matching columns and is
    // skipped by ordinal count, applied rows precede truth rows by batch order.
    static async Task<Seq<CoordRow>> Rows(NpgsqlBatch batch, CancellationToken cancellationToken) {
        List<CoordRow> rows = [];
        await using NpgsqlDataReader reader = await batch.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        do {
            while (reader.FieldCount >= 6 && await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
                rows.Add(new CoordRow(
                    reader.GetString(0), reader.GetString(1), reader.GetInt64(2),
                    reader.IsDBNull(3) ? None : Some(reader.GetInt64(3)),
                    Instant.FromDateTimeUtc(DateTime.SpecifyKind(reader.GetDateTime(4), DateTimeKind.Utc)),
                    reader.IsDBNull(5) ? None : Some(ReadPayload(reader.GetString(5)))));
            }
        } while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));
        return toSeq(rows);
    }

    static JsonElement ReadPayload(string json) { using JsonDocument document = JsonDocument.Parse(json); return document.RootElement.Clone(); }

    // The ONE per-case projector — the verdict-row discriminator every Fenced arm folds through. The refusal
    // reads off the rows the batch RETURNED (the debit verdict CTE carries every unit's sufficiency + balance;
    // the trailing current-truth SELECT carries the row's generation/state when the guarded UPDATE missed), so
    // no typed fault needs a second round trip. Read cases project Loaded — the Received leg's own arm, kept
    // here only because the generated Switch is total.
    static Fin<CoordinationReceipt> Project(CoordinationOp op, Option<LeaseToken> held, Seq<CoordRow> rows, ProjectionContext frame, long mark) => op.Switch(
        budgetDebit: d => rows.Find(static r => r.State == "insufficient").Match(
            Some: r  => Fin<CoordinationReceipt>.Fail(new CoordinationFault.BudgetExhausted(r.Key, d.Debit.Find(r.Key).IfNone(0L), r.Value.IfNone(0L))),
            None: () => rows.Find(static r => r.State == "fenced").Match(
                Some: r => Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence)),
                None: () => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Debited(toHashMap(rows.Map(static r => (r.Key, r.Value.IfNone(0L)))), frame.Now(), frame.Correlation, frame.Elapsed(mark)))),
        budgetCredit: _ => rows.Find(static r => r.State == "fenced").Match(
            Some: r => Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Debited(toHashMap(rows.Map(static r => (r.Key, r.Value.IfNone(0L)))), frame.Now(), frame.Correlation, frame.Elapsed(mark)))),
        signalPut: s => rows.HeadOrNone().Match(
            Some: r  => r.State == "signaled"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Signaled(s.Instance, s.Channel, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.Refused($"<signal:{s.Instance.Value}/{s.Channel.Value}>"))),
        signalLoad: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        stepStateCas: c => rows.HeadOrNone().Match(
            Some: r => r.State == c.Next.Key
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Stepped(c.Workflow, c.Step, c.Next, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : r.Fence > held.Map(static token => token.Value).IfNone(0L)
                    ? Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence))
                    : Fin<CoordinationReceipt>.Fail(new CoordinationFault.CasConflict(c.Workflow, c.Step, c.Expected, StepState.Get(r.State))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.Refused($"<step-missing:{c.Workflow.Value}/{c.Step.Value}>"))),
        stepStateInFlight: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        stepStateLoad:     _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        leaseAcquire: a => rows.HeadOrNone().Match(
            Some: r => r.State == "held"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Leased(a.Lease, LeaseToken.Create(r.Fence), r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(LeaseToken.Create(0L), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseExpired(a.Lease, a.Holder))),
        leaseRenew: n => Held(rows, n.Lease, n.Token, frame, mark),
        leaseRelease: n => Held(rows, n.Lease, n.Token, frame, mark),
        expiredScan: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        membershipUpsert: m => rows.HeadOrNone().Match(
            Some: r  => r.State == "serving"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Member(m.Group, m.Member, r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.MembershipLapsed(m.Group, m.Member))),
        membershipRelease: m => rows.HeadOrNone().Match(
            Some: r  => r.State == "departed"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Member(m.Group, m.Member, r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.MembershipLapsed(m.Group, m.Member))),
        membershipScan: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        outboxAdvance: a => rows.HeadOrNone().Match(
            Some: r => r.Value.IfNone(0L) >= a.Through
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Advanced(a.Sink, r.Value.IfNone(0L), frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(a.Sink, a.Through)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(a.Sink, a.Through))));

    // The shared renew/release verdict arm: an applied row carries the validated generation ("held"/"released"),
    // a current-truth row with a higher fence is the stale-token refusal carrying the CURRENT generation.
    static Fin<CoordinationReceipt> Held(Seq<CoordRow> rows, LeaseKey lease, LeaseToken token, ProjectionContext frame, long mark) =>
        rows.HeadOrNone().Match(
            Some: r => r.State is "held" or "released"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Leased(lease, LeaseToken.Create(r.Fence), r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(token, r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(token, 0L)));
}

// `For` generates every statement and bind row from the op discriminant. `RequiresToken` makes a missing write fence
// refuse before SQL dispatch; reads and lease acquisition are the only false rows.
public readonly record struct CaseSql(string LockKey, bool RequiresToken, string Guarded, string Truth, Option<string> Wake, Seq<(string Name, object Value)> Binds) {
    const string Empty = "SELECT NULL::text, NULL::text, NULL::bigint, NULL::bigint, NULL::timestamptz, NULL::jsonb WHERE FALSE";

    public static CaseSql For(CoordinationOp op, Instant now) => op.Switch(
        budgetDebit: value => Budget(value.Debit, debit: true),
        budgetCredit: value => Budget(value.Credit, debit: false),
        stepStateCas: value => Write($"step:{value.Workflow.Value}:{value.Step.Value}",
            "UPDATE workflow_step SET state=@next, fence=@token WHERE tenant=@tenant AND workflow=@workflow AND step=@step AND fence<=@token AND state=@expected RETURNING step, state, fence, NULL::bigint, updated_at, NULL::jsonb",
            "SELECT step, state, fence, NULL::bigint, updated_at, NULL::jsonb FROM workflow_step WHERE tenant=@tenant AND workflow=@workflow AND step=@step",
            ("workflow", value.Workflow.Value), ("step", value.Step.Value), ("expected", value.Expected.Key), ("next", value.Next.Key)),
        stepStateInFlight: value => Read(
            "SELECT step, state, fence, NULL::bigint, updated_at, NULL::jsonb FROM workflow_step WHERE tenant=@tenant AND workflow=@workflow AND state NOT IN ('done','faulted') ORDER BY step",
            ("workflow", value.Workflow.Value)),
        stepStateLoad: value => Read(
            "SELECT step, state, fence, NULL::bigint, updated_at, NULL::jsonb FROM workflow_step WHERE tenant=@tenant AND workflow=@workflow AND step=@step",
            ("workflow", value.Workflow.Value), ("step", value.Step.Value)),
        signalPut: value => Write($"signal:{value.Instance.Value}:{value.Channel.Value}",
            "INSERT INTO workflow_signal(tenant,workflow,channel,payload,fence,updated_at) VALUES(@tenant,@workflow,@channel,@payload,@token,@now) ON CONFLICT(tenant,workflow,channel) DO UPDATE SET payload=excluded.payload,fence=excluded.fence,updated_at=excluded.updated_at WHERE workflow_signal.fence<=@token RETURNING channel,'signaled',fence,NULL::bigint,updated_at,payload",
            "SELECT channel,'current',fence,NULL::bigint,updated_at,payload FROM workflow_signal WHERE tenant=@tenant AND workflow=@workflow AND channel=@channel",
            ("workflow", value.Instance.Value), ("channel", value.Channel.Value), ("payload", value.Payload), ("now", now)),
        signalLoad: value => Read(
            "SELECT channel,'signaled',fence,NULL::bigint,updated_at,payload FROM workflow_signal WHERE tenant=@tenant AND workflow=@workflow AND channel=@channel",
            ("workflow", value.Instance.Value), ("channel", value.Channel.Value)),
        leaseAcquire: value => Write($"lease:{value.Lease.Value}",
            "INSERT INTO lease(tenant,key,holder,generation,until) VALUES(@tenant,@lease,@holder,1,@until) ON CONFLICT(tenant,key) DO UPDATE SET holder=excluded.holder,generation=lease.generation+1,until=excluded.until WHERE lease.holder=@holder OR lease.until<@now RETURNING key,'held',generation,NULL::bigint,until,NULL::jsonb",
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND key=@lease",
            false, ("lease", value.Lease.Value), ("holder", value.Holder.Value), ("until", now + value.Ttl), ("now", now)),
        leaseRenew: value => Write($"lease:{value.Lease.Value}",
            "UPDATE lease SET until=@until WHERE tenant=@tenant AND key=@lease AND generation=@token RETURNING key,'held',generation,NULL::bigint,until,NULL::jsonb",
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND key=@lease",
            ("lease", value.Lease.Value), ("until", now + value.Ttl)),
        leaseRelease: value => Write($"lease:{value.Lease.Value}",
            "DELETE FROM lease WHERE tenant=@tenant AND key=@lease AND generation=@token RETURNING key,'released',generation,NULL::bigint,until,NULL::jsonb",
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND key=@lease",
            ("lease", value.Lease.Value)),
        expiredScan: _ => Read(
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND until<@now ORDER BY until,key",
            ("now", now)),
        membershipUpsert: value => Write($"member:{value.Group.Value}:{value.Member.Value}",
            "INSERT INTO membership(tenant,group_key,member,until,fence) VALUES(@tenant,@group,@member,@until,@token) ON CONFLICT(tenant,group_key,member) DO UPDATE SET until=excluded.until,fence=excluded.fence WHERE membership.fence<=@token RETURNING member,'serving',fence,NULL::bigint,until,NULL::jsonb",
            "SELECT member,'current',fence,NULL::bigint,until,NULL::jsonb FROM membership WHERE tenant=@tenant AND group_key=@group AND member=@member",
            ("group", value.Group.Value), ("member", value.Member.Value), ("until", now + value.Ttl)),
        membershipRelease: value => Write($"member:{value.Group.Value}:{value.Member.Value}",
            "DELETE FROM membership WHERE tenant=@tenant AND group_key=@group AND member=@member AND fence<=@token RETURNING member,'departed',fence,NULL::bigint,until,NULL::jsonb",
            "SELECT member,'current',fence,NULL::bigint,until,NULL::jsonb FROM membership WHERE tenant=@tenant AND group_key=@group AND member=@member",
            ("group", value.Group.Value), ("member", value.Member.Value)),
        membershipScan: value => Read(
            "SELECT member,'serving',fence,NULL::bigint,until,NULL::jsonb FROM membership WHERE tenant=@tenant AND group_key=@group AND until>=@now ORDER BY member",
            ("group", value.Group.Value), ("now", now)),
        outboxAdvance: value => WriteWake($"outbox:{value.Sink.Value}",
            "INSERT INTO outbox_cursor(tenant,sink,sequence,updated_at,fence) VALUES(@tenant,@sink,@through,@now,@token) ON CONFLICT(tenant,sink) DO UPDATE SET sequence=excluded.sequence,updated_at=excluded.updated_at,fence=excluded.fence WHERE outbox_cursor.fence<=@token AND outbox_cursor.sequence<@through RETURNING sink,'advanced',fence,sequence,updated_at,NULL::jsonb",
            "SELECT sink,'current',fence,sequence,updated_at,NULL::jsonb FROM outbox_cursor WHERE tenant=@tenant AND sink=@sink",
            "rasm_outbox",
            ("sink", value.Sink.Value), ("through", value.Through), ("now", now)));

    static CaseSql Budget(HashMap<string, long> amounts, bool debit) {
        string[] units = amounts.Keys.ToArray();
        long[] values = units.Map(unit => amounts.Find(unit).IfNone(0L)).ToArray();
        string statement = debit
            ? "WITH requested AS (SELECT * FROM unnest(@units,@amounts) AS r(unit,amount)), verdict AS (SELECT r.unit,r.amount,coalesce(b.balance,0) balance,coalesce(b.fence,0) fence,(b.unit IS NOT NULL AND b.fence<=@token AND b.balance>=r.amount) sufficient FROM requested r LEFT JOIN budget_ledger b ON b.tenant=@tenant AND b.unit=r.unit), applied AS (UPDATE budget_ledger b SET balance=b.balance-v.amount,fence=@token FROM verdict v WHERE b.tenant=@tenant AND b.unit=v.unit AND NOT EXISTS(SELECT 1 FROM verdict WHERE NOT sufficient)) SELECT unit,CASE WHEN fence>@token THEN 'fenced' WHEN sufficient THEN 'sufficient' ELSE 'insufficient' END,fence,CASE WHEN NOT EXISTS(SELECT 1 FROM verdict WHERE NOT sufficient) THEN balance-amount ELSE balance END,'epoch'::timestamptz,NULL::jsonb FROM verdict ORDER BY unit"
            : "WITH requested AS (SELECT * FROM unnest(@units,@amounts) AS r(unit,amount)), applied AS (INSERT INTO budget_ledger(tenant,unit,balance,fence) SELECT @tenant,unit,amount,@token FROM requested ON CONFLICT(tenant,unit) DO UPDATE SET balance=budget_ledger.balance+excluded.balance,fence=excluded.fence WHERE budget_ledger.fence<=@token RETURNING unit,fence,balance), projected AS (SELECT r.unit,coalesce(a.fence,b.fence,0) fence,coalesce(a.balance,b.balance,0) balance,(a.unit IS NOT NULL) applied FROM requested r LEFT JOIN applied a USING(unit) LEFT JOIN budget_ledger b ON b.tenant=@tenant AND b.unit=r.unit) SELECT unit,CASE WHEN applied THEN 'sufficient' ELSE 'fenced' END,fence,balance,'epoch'::timestamptz,NULL::jsonb FROM projected ORDER BY unit";
        return Write("budget", statement, Empty, ("units", units), ("amounts", values));
    }

    static CaseSql Write(string lockKey, string guarded, string truth, params (string Name, object Value)[] binds) =>
        new(lockKey, true, guarded, truth, None, toSeq(binds));

    static CaseSql Write(string lockKey, string guarded, string truth, bool requiresToken, params (string Name, object Value)[] binds) =>
        new(lockKey, requiresToken, guarded, truth, None, toSeq(binds));

    static CaseSql WriteWake(string lockKey, string guarded, string truth, string wake, params (string Name, object Value)[] binds) =>
        new(lockKey, true, guarded, truth, Some(wake), toSeq(binds));

    static CaseSql Read(string truth, params (string Name, object Value)[] binds) =>
        new("", false, Empty, truth, None, toSeq(binds));
}
```

| [INDEX] | [POLICY]        | [VALUE]                                       | [BINDING]                                                       |
| :-----: | :-------------- | :-------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | fencing         | `fence <= @token` on every guarded write      | stale token → typed `LeaseFenced`, never a lost update          |
|  [02]   | generation mint | `LeaseAcquire` row-CAS `RETURNING generation` | monotone `++`; the token is validated, not merely issued        |
|  [03]   | budget shape    | per-unit vector, one statement                | `HashMap<string,long>` mirrors `CostVector` string keys ([A.1]) |
|  [04]   | read guard      | tenant RLS predicate structural on every READ | no cross-tenant in-flight/lease/membership leak                 |
|  [05]   | lock family     | `pg_advisory_xact_lock` (transaction-scoped)  | auto-released at commit; session locks are the leak form        |
|  [06]   | receipt floor   | `IsValid => ValidityClaim.All(...)` per case  | kernel validity fold ([C]); `&&` chains deleted                 |
|  [07]   | port direction  | AppHost decodes Persistence-owned types       | four PORT rows + `MembershipView.Serving`; nothing crosses down |
|  [08]   | row canon       | `(key, state, fence, value, until, payload)`  | fence and case scalar never alias; `CaseSql.For` generates every row    |
|  [09]   | signal row      | fenced `(workflow, channel)` upsert           | `SignalPut`/`SignalLoad` decode `StepStateSeam.SignalPut`/`SignalOf`    |

## [03]-[OUTBOX_CURSOR]

- Owner: `OutboxCursor` the per-sink durable drain cursor row — `(SinkKey Sink, long Sequence)`, one row per `EgressSink`, so each sink drains the ONE Marten stream independently; the `ONE_OUTBOX_EGRESS_SPINE` law this section NAMES; the `OutboxAdvance` case (`#COORDINATION_OP`) whose fenced CAS is the only cursor writer; the `pg_notify` pump wake emitted in the same transaction.
- Entry: the `Version/egress` pump reads `OutboxCursor.Sequence`, drains `Version/ledger#CHANGEFEED` `ReplayWindow.DurableOps(cursor.Sequence, take)` rows, delivers, and calls `Coordinate.Run(session, new OutboxAdvance(sink, through), held, frame)` through its session-closed `EgressPorts.Coordinate` arrow — the advance CAS is `UPDATE outbox_cursor SET sequence = @through WHERE sink = @sink AND tenant = @tenant AND sequence < @through RETURNING sequence`, so a concurrent pump instance's stale advance returns zero rows and rails `CoordinationFault.OutboxDrain(sink, through)` (at-least-once law: the cursor advances only forward, only after delivery confirmation, and a crash between delivery and advance re-drains — the sink's dedup composition absorbs the replay).
- Auto: the Marten event stream IS the outbox — a domain commit and its egress obligation are ONE `SaveChangesAsync` because the committed event itself is the drainable row (`OpLogEntry` projects from it), so there is no envelope table to fill, no relay to poll, and no dual-write gap; the same transaction that advances a cursor or commits an event `pg_notify('rasm_outbox', @sink)`s the channel, and the egress pump's idle connection `WaitAsync` wakes on it (the low-latency wake beside the bounded poll floor — a missed NOTIFY is absorbed by the next poll, so the wake is latency, never correctness); the cursor is PER-SINK so a slow webhook never holds back the NATS drain, and DISTINCT from the per-origin `SyncCursor` (`ledger.md` `#CHANGEFEED`) which positions peer replication, not sink delivery.
- Receipt: a cursor advance rides `store.coordination.outbox` carrying the sink and the through-sequence; the held-cursor stall evidence is the egress pump's (`Version/egress#EGRESS_PUMP` `CursorStall`), never minted here.
- Growth: a new egress sink is ONE `outbox_cursor` row minted on first drain (the advance CAS upserts), zero coordination edits; zero new surface — an envelope-table outbox, a per-sink advance verb, a trigger-based cursor writer, or a coordination-side read of the pump is the deleted form.
- Boundary: forward-only intra-leg edge — `Version/egress` drains this cursor and coordination NEVER reads the pump (the acyclicity proof's one intra-leg egress edge); the failed advance CAS stays `CoordinationFault.OutboxDrain` in THIS band (the cursor write is fenced-store work) while every delivery fault is the pump's `EgressFault`; the presence/awareness lane (`ColumnFamily.Presence`, `durable: false`) never has a cursor row — only `Family.Durable` lanes drain past this cursor.

| [INDEX] | [POLICY]       | [VALUE]                                            | [BINDING]                                                      |
| :-----: | :------------- | :------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | outbox spine   | the Marten stream IS the outbox                    | same-`IDocumentSession`; envelope tables are the deleted store |
|  [02]   | cursor grain   | per-sink `outbox_cursor(SinkKey, Sequence)`        | distinct from the per-origin `SyncCursor`; slow sinks isolate  |
|  [03]   | advance law    | forward-only CAS, post-confirmation                | at-least-once; replay absorbed by sink dedup                   |
|  [04]   | pump wake      | `pg_notify('rasm_outbox', sink)` same-tx           | latency only — the bounded poll floor owns correctness         |
|  [05]   | edge direction | egress reads cursor; coordination never reads pump | the one forward-only intra-leg egress edge                     |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
