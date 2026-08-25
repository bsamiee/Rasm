# [PERSISTENCE_STORE_COORDINATION]

Rasm.Persistence owns the token-VALIDATING fenced-lease coordination store — the ONE durable substrate behind the four AppHost PORT contracts (`Agent/capability` Budget debit/credit, `Runtime/orchestration` step-state CAS + durable signal, `Wire/outbox` transactional outbox, `Wire/coordination` CAS+lease+membership) — as one closed `CoordinationOp` `[Union]` dispatched by one `Coordinate.Run` bracket over the generated total `Switch`, exactly the `Element/graph#STORE_RAIL` idiom. `Run` takes the op SEQUENCE, so composing several ops into one atomic unit is the entry's own shape rather than caller-improvised repetition: it acquires every advisory key its rows name in `LockRank` order FIRST, executes the ops in CALLER order, and commits once — acquisition order and execution order are two orders. Every outcome emits through the injected `ReceiptSinkPort` at that one fold, a committed receipt under its verb slot and a typed refusal under `store.coordination.fault`. Every guarded write folds through ONE fenced-CAS predicate (`pg_advisory_xact_lock` + guarded `UPDATE … RETURNING` in one round trip, the token validated against the row's monotone lease generation so a stale holder is the typed `CoordinationFault.LeaseFenced`, never a lost update); every READ case folds through that same leg's truth projection carrying the frame's tenant RLS predicate STRUCTURALLY, so no read leaks cross-tenant in-flight/lease/membership state. Budget is a fenced compare-and-decrement over a PER-UNIT VECTOR (`HashMap<string, long>` mirroring the AppHost `MeterVector` string keys — the smart-enum key crosses as its STRING, never the AppHost type) whose guard is PostgreSQL's own per-row `WHERE` re-check, the one conditional decrement the engine settles against a concurrent writer's committed version; all-or-nothing across the vector rides the batch `SAVEPOINT`, because a single statement drops each re-check failure from its result set and commits every unit that passed. Marten's event stream IS the outbox (`ONE_OUTBOX_EGRESS_SPINE` — same-`IDocumentSession` guarantee, so the domain event and its egress obligation commit in one transaction and a second message-envelope outbox table is the deleted parallel store); this page mints the durable PER-SINK drain cursor `outbox_cursor(SinkKey, long Sequence)` — distinct from the per-origin `SyncCursor` (`Version/ledger#CHANGEFEED`) — and `OutboxAdvance(Sink, Through)` is the cursor-advance case the `Version/egress` pump calls, forward-only: the pump reads the cursor, coordination never reads the pump. Coordination composes one direct `NpgsqlBatch` on the Marten session's transacted connection (the session transaction force-opened first, so lock + CAS + event commit share it; `QueueSqlCommand` is reserved for no-RETURNING side-writes — it defers to `SaveChangesAsync` and surfaces no result set, so it can never carry the RETURNING-vector CAS) + Npgsql `pg_advisory_xact_lock` + LISTEN/NOTIFY — never a second event store, never a distributed-lock sidecar (`DistributedLock.Postgres` carries no fencing token; the token-validating CAS is strictly stronger). Every throw crossing folds through ONE `CoordinationFault.Lift` into the 8430 `StoreCoordination` band, whose cases OVERRIDE the kernel `Retriability` and publish a `RetryShape` route beside it because this rail states refusal as a `Fin` RESULT: the advisory lock and the guarded CAS raise serialization failures and deadlocks under the contention the fence arbitrates, so `Contended`/`Unreachable` publish that retriable class where a bare `Error` hid it from every caller predicate, a fenced token and a moved step state publish the WIDER re-plan they recover under, every other refusal is terminal by construction, and `store.coordination.fault` carries the union's generated code and route onto the fact stream. Persistence OWNS the op-union, fencing tokens, membership rows, balance vectors, and receipts, which AppHost's `Wire/coordination`/`Wire/outbox` adapters DECODE — no AppHost type crosses down; tenant, wall clock, and correlation ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame — the kernel `CorrelationId`/`TenantContext` pair SEATED on it, so the RLS bind spells `frame.Tenant.Entry` and every receipt spells `frame.Correlation` with no per-seam lift. `FaultBand` arrives from the kernel roster; `IValidityEvidence`/`ValidityClaim` from the `Rasm` kernel; `IDocumentSession`/`NpgsqlDataSource` from the substrate.

## [01]-[INDEX]

- [02]-[COORDINATION_OP]: `CoordinationOp`'s closed case family, the key/token/state vocabularies, the `LockRank` containment ladder every acquisition sorts on, the ONE fenced-CAS fold consuming the `CaseSql` data rows under one savepoint, the per-unit-vector Budget debit/credit, the durable-signal cases, the `CoordinationReceipt` validity fold, and the 8430 fault band with its `Lift` throw crossing, its per-case retriability discriminant, and its emitted refusal fact.
- [03]-[OUTBOX_CURSOR]: Per-sink cursor and deferred-head state, exact pending read, fenced advance, and LISTEN/NOTIFY wake.

## [02]-[COORDINATION_OP]

- Owner: `CoordinationOp` is the closed interaction family; every case — write, read, and cursor advance alike — folds through ONE `Bracket`. `LeaseToken` carries the monotone generation; the key and state vocabularies close their domains. `LockRank` is the containment ladder over the advisory-lock families and `LockScope` pairs a rank with its scope text, so the lock key's own prefix IS the rank key and the two cannot drift. `CoordRow` is the canonical `(key, state, fence, value, until, payload)` projection: `Fence` never aliases a budget balance or cursor sequence, and `Value` carries those case scalars. `PendingOutbox` and `OutboxDeferred` admit the outbox read into its typed state instead of leaking the row projection. `CaseSql.For` is the total parameterized SQL generator over the op family, carrying lock scopes, token requirement, guarded statement, truth statement, and binds as data. `CoordinationReceipt`, `CoordinationFault`, and `Coordinate` own evidence, failures, and execution.
- Cases: `BudgetDebit(HashMap<string, long> Debit)` the per-unit-vector fenced compare-and-decrement (`capability.md` `MeterVector` crosses as its `[SmartEnum<string>]` STRING key, so the row is `HashMap<string, long>` per unit and AppHost maps its smart-enum at the boundary — a scalar debit is falsified by the multi-unit consumer); `BudgetCredit(HashMap<string, long> Credit)` the fenced vector increment — the compensation inverse a workflow that must RETURN budget rides, the same one-statement vector shape with no sufficiency gate, and the SEED for a unit no row yet holds because its `ON CONFLICT` establishes the absent row; the vector's polarity is REMAINING balance per unit and never cumulative spend, so the ceiling never crosses this seam and a consumer reporting spend derives it as `ceiling - remaining`; `StepStateCas(WorkflowKey, StepKey, StepState Expected, StepState Next)` the orchestration step transition; `StepStateInFlight(WorkflowKey)` READ (the `CrashResume` scan — every non-terminal step of a workflow); `StepStateLoad(WorkflowKey, StepKey)` READ; `SignalPut(WorkflowKey Instance, SignalKey Channel, JsonElement Payload)` the fenced durable-signal upsert the AppHost `Runtime/orchestration#STEP_STATE_SEAM` `StepStateSeam.SignalPut` decodes — one `signal` row per `(workflow, channel)` under the same tenant fence, so a waiting `Signal` step's wake-or-fault decision survives crash, resume, and peer handoff; `SignalLoad(WorkflowKey Instance, SignalKey Channel)` READ (the `StepStateSeam.SignalOf` leg — the loaded row's `Payload` slot carries the channel JSON); `LeaseAcquire(LeaseKey, HolderId, Duration Ttl)` MINTS the generation monotonically (`generation + 1` via PG row-CAS `RETURNING generation` — the mint side that makes the token VALIDATED); `LeaseRenew(LeaseKey, LeaseToken, Duration Ttl)` and `LeaseRelease(LeaseKey, LeaseToken)` re-validate the held token; `ExpiredScan` READ (orphan-reclaim — every lease whose deadline trails `frame.Now()`); `MembershipUpsert(MembershipKey, MemberId, Duration Ttl)` the lease-expiring membership row (`MembershipView.Serving`, `Rasm.AppHost/Wire/coordination.md`, is the in-process consumer); `MembershipRelease(MembershipKey, MemberId)` the explicit fenced departure — a clean shutdown removes its row NOW instead of waiting out the TTL lapse, the AppHost `MembershipView` `Departed` transition's durable half; `MembershipScan(MembershipKey)` READ; `OutboxAdvance(SinkKey Sink, long Through)` the `#OUTBOX_CURSOR` case; `OutboxPending(SinkKey Sink, long After, int Take)` READ (the relay's bounded drain window off the committed op-log) and `OutboxPark(SinkKey Sink, long Sequence, int Attempt, string Status)` the fenced head-of-line failure written onto the sink's OWN cursor row; `LeaseGuard(LeaseKey Lease, LeaseToken Token)` READ (advisory detection a holder reads before spending work, never a gate); `BudgetLoad` and `BudgetToken` the two nullary ledger READs whose tenant rides the frame. `CoordinationFault` closes over the seven deterministic refusals and the three provider classes `CoordinationFault.Lift` folds — `Contended(SqlState, Cause)` and `Unreachable(Cause)` overriding the kernel `Retriability` to `Transient`, `Unmapped(SqlState, Cause)` inheriting `Terminal`, and `LeaseFenced`/`CasConflict` publishing the `Rescoped` route their wider re-plan takes — while unknown errors remain exact.
- Entry: `public static IO<Fin<Seq<CoordinationReceipt>>> Run(IDocumentSession session, ReceiptSinkPort sink, Seq<CoordinationOp> ops, Option<LeaseToken> held, ProjectionContext frame, CancellationToken cancellationToken)` is the ONE rail at every arity — a port passes one op, a composed unit passes several, and the trailing frame and token parameters are why arity rides `Seq` rather than a `params` tail. One entry makes the transaction boundary and the acquisition set the SAME value; a second single-op entry beside it leaves the unsafe composition reachable, since two calls on one session are two transactions no ordering law reaches across. `Bracket` force-opens the Marten session transaction, then composes ONE direct `NpgsqlBatch` on the session's live connection: `SELECT pg_advisory_xact_lock(hashtext(@tenant || ':' || @key))` per DISTINCT `LockScope` every row names, sorted by `(Rank.Depth, Scope)` and tenant-prefixed so one tenant's hot key never stalls a sibling's; `SAVEPOINT rasm_coord`; then per op in CALLER order the guarded `UPDATE … WHERE tenant = @tenant AND fence <= @token AND <case predicate> … RETURNING` from `CaseSql.Guarded`, the tenant-guarded current-truth `SELECT` from `CaseSql.Truth`, and the optional `pg_notify` wake. `QueueSqlCommand` cannot carry this batch because it defers to `SaveChangesAsync` and returns no rows. Read ops name no `LockScope` and carry no guarded statement, so reads and writes ride one leg. `Verified` is the truth-only replay the relational retry owner passes as `verifySucceeded` — same ops, same `CaseSql`, same `Verdict`, no lock and no guarded statement. `SaveChangesAsync(cancellationToken)` commits WITH any same-session domain events, so a step transition and the event it consequences are one transaction; a refusal `ROLLBACK TO SAVEPOINT rasm_coord` first, so the batch commits nothing of its own and the caller's unit of work stays the caller's to decide.
- Auto: the fencing law is structural — a guarded row carries the highest lease generation it has observed (`fence`), the write predicate `fence <= @token` rejects a token older than that watermark and the write stamps `fence = @token`, so a paused holder resuming with a superseded token is `LeaseFenced(stale, current)` read off the zero-row CAS and the batch's trailing current-truth `SELECT` (one round trip, never a follow-up read), never a silent overwrite; `LeaseAcquire` takes the advisory lock on the lease key, then `UPDATE lease SET generation = generation + 1, holder = @holder, until = @until WHERE key = @key AND (holder = @holder OR until < @now) RETURNING generation` — an unexpired foreign hold returns zero rows and rails `LeaseExpired`-inverse refusal as `LeaseFenced`, an expired hold is reclaimed in the same statement; the Budget debit is ONE `UPDATE … FROM unnest(@units, @amounts) … WHERE b.balance >= r.amount AND b.fence <= @token RETURNING unit, balance` whose guard PostgreSQL RE-EVALUATES against the concurrent writer's committed row version once the block clears, so the decrement cannot overdraw and takes no lock of any kind; the units array binds SORTED and its keys are unique by `HashMap` construction, so every caller walks the ledger's row locks in one order and a repeated unit — a hard `ON CONFLICT` error on one statement family and a silently dropped amount on the other — is unrepresentable; that re-check is per-ROW atomic and nothing further, so a unit failing it leaves the result set while its siblings COMMIT, and all-or-nothing is the rail's own count gate — applied rows short of requested units rolls back to the savepoint and rails `BudgetExhausted(unit, requested, available)` off the trailing truth `SELECT`, whose row for the refusing unit was never written and reads true either side of the undo (an absent ledger row is a structural zero balance, the domain's own reading of an unheld unit, never an unmeasured one); a snapshot-computed whole-vector sufficiency predicate is the DELETED form; `BudgetCredit` is that shape with the sufficiency term dropped and the sign flipped, establishing an absent unit through `INSERT … ON CONFLICT (tenant, unit) DO UPDATE SET balance = budget_ledger.balance + excluded.balance WHERE budget_ledger.fence <= @token` — the one construct guaranteeing insert-or-update across the absent/present split — so debit and credit stay one statement family, never a sibling rail; `SignalPut` is a fenced `(workflow, channel)` upsert — `INSERT … ON CONFLICT (tenant, workflow, channel) DO UPDATE SET payload = excluded.payload, fence = @token WHERE signal.fence <= @token RETURNING` — so a paused holder's stale re-signal is the typed `LeaseFenced` refusal, never a silent payload overwrite, and `SignalLoad` reads the row's `payload` back through the canonical row's `Payload` slot; `MembershipRelease` is the fenced row delete whose `RETURNING` proves the departure (zero rows on an already-lapsed member is the benign `MembershipLapsed` the caller treats as done); every op ends with a trailing tenant-guarded current-truth `SELECT`, so a missed guarded `UPDATE` still returns the row's current generation/state and every typed refusal (`LeaseFenced` current, `CasConflict` found) populates from the ONE round trip; every READ carries `tenant = @tenant` structurally (the same guard the writes hold); the receipts project PER-OP with zero follow-up reads — `BudgetDebit` returns the POST-debit balance vector the metering consumer needs, a CAS/lease/membership write returns its committed row, a READ returns its loaded rows.
- Receipt: a debit rides `store.coordination.debit` and a credit `store.coordination.credit`, both carrying the post-op balance vector; a step CAS rides `store.coordination.step`; a signal upsert rides `store.coordination.signal`; a lease verb rides `store.coordination.lease` carrying the generation; a membership upsert or release rides `store.coordination.member`; a READ rides `store.coordination.read` carrying the row count; the cursor advance rides `store.coordination.outbox` (`#OUTBOX_CURSOR`); every typed refusal rides `store.coordination.fault` carrying its generated numeric identity and retry route. Emission is `Run`'s OWN fold — `SlotOf` resolves each op's verb slot off the op discriminant and the fold sends through the injected `ReceiptSinkPort`.
- Packages: Marten (`IDocumentSession.SaveChangesAsync`/transaction control — the fenced batch rides the session's transacted connection; `QueueSqlCommand` only for no-RETURNING side-writes such as the `pg_notify` wake), Npgsql (`NpgsqlBatch` — `pg_advisory_xact_lock` + guarded `UPDATE … RETURNING` + current-truth `SELECT` in one round trip, `pg_notify`; `PostgresException.IsTransient`/`SqlState`/`MessageText` and `NpgsqlException.IsTransient` — the provider's own retriable classification the `Lift` fold reads instead of a re-spelled SQLSTATE roster), Rasm (`IValidityEvidence`/`ValidityClaim`), LanguageExt.Core (`IO`/`Fin`/`HashMap`/`Seq`), NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a coordination concern is one `CoordinationOp` case, total dispatch arm, and `CaseSql` row. Uniform reads return `Loaded`; `OutboxPending` earns its typed receipt because cursor state and envelope must stay one snapshot. A second lease store, message table, scalar budget twin, single-op entry, or caller-side parse is rejected.
- Boundary: the four PORT rows are AppHost→Persistence READs/decodes (correct HOST-BOUNDARY→APP-PLATFORM direction) — `Agent/capability` debits and credits the Budget vector, `Runtime/orchestration` drives `StepStateCas`+`StepStateInFlight`+`SignalPut`/`SignalLoad` (`CrashResume` reads the in-flight scan; the `StepStateSeam.SignalPut`/`SignalOf` delegates decode the signal cases), `Wire/outbox` rides the same-transaction outbox spine, `Wire/coordination` drives CAS+lease+membership and `MembershipView.Serving` folds the membership rows in-process — no AppHost type crosses down and no Persistence signature names `ClockPolicy` or `Principal` ([A.1] — the kernel `CorrelationId`/`TenantContext` pair is S0 vocabulary this package composes directly off the frame); the fenced-CAS is strictly stronger than any lease library because the token is VALIDATED at every guarded write rather than held — `DistributedLock.Postgres` (no fencing token) and `WolverineFx` (message-envelope outbox table beside the stream-IS-outbox law) stay the recorded rejections; the advisory lock is the `_xact_` family (auto-released at transaction end AND at rollback — a session lock survives its own transaction's rollback and requires explicit unlock, the leak form); `LockRank` is the CONTAINMENT ladder the AppHost ports compose along — a node's membership encloses the leases it may hold, a lease fences the work beneath it, a step is that work's unit, a signal is a channel detail inside one instance, and the cursor advance is the terminal drain position nothing nests inside — so acquiring in rank order is a discipline every caller shares by construction rather than by convention, and the budget seats NO rank because its grain is row-level inside its own statement, ordered by the sorted unit array; row-level locking cannot serve that vector at all, since a unit's ledger row may be absent and `FOR UPDATE` over the requested-vector `LEFT JOIN` is refused at PLAN time (`0A000`, the nullable side of an outer join) while a lock on an absent row is a lock on nothing — the engine's own conditional-`UPDATE` re-check is what replaces it, and it is strictly stronger than the whole-ledger advisory key it deletes because it neither serializes unrelated units nor depends on a lock domain the server does not enforce; `hashtext` is a 32-bit digest, so two distinct keys can share one advisory slot and serialize needlessly — a throughput cost the rank ordering never turns into a correctness one; deadline comparisons read `frame.Now()` (the injected clock value), never a wall-clock call; a failed `OutboxAdvance` cursor-CAS is `CoordinationFault.OutboxDrain` — the coordination-side write fault, kept inside this fenced store's rail, NEVER a `Version/egress` `EgressFault` delivery fault; this tier CLASSIFIES and executes nothing, publishing the kernel `Retriability` its cases OVERRIDE and the `RetryShape` route beside it exactly as the object plane's `RemoteStoreFault` does, and the two discriminants are what make the classification legible to a caller whose refusal arrives as a result rather than a throw — a bare `Error` on the rail leaves the whole retriable class unreadable to every predicate, the deleted form `Lift` closes, and a single bool spanning both axes drops the distinction between a fenced token recovering under a WIDER re-plan and a contended one recovering under a wait; the executing rail is the STORE EXECUTION STRATEGY (`docs/stacks/csharp/domain/resilience.md` `[04]-[LAYER_SPLIT]` row `[01]` — this callee owns transactional semantics, so no hop pipeline may bracket it, since a pipeline there replays from the wrong boundary), seated at the relational owner `Element/identity#IDENTITY_RAIL` holds under the `StoreCapability.StrategyRedrive` row that profile carries, ABOVE `Run` because `Lift` converts a throw to a value and a strategy beneath it has nothing left to classify; every guarded statement here is a conditional write, so that strategy admits this rail only under `verifySucceeded` and `Verified` is the probe it passes; the discriminant then drives a WIDER-scope caller re-offer, re-planning the step rather than re-executing one statement.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Text.Json;
using NodaTime.Text;
using Npgsql;
using Rasm.Domain;
using Rasm.Persistence.Element;

namespace Rasm.Persistence.Store;

// --- [TYPES] ---------------------------------------------------------------------------

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LockRank {
    public static readonly LockRank Member = new("member", depth: 1);
    public static readonly LockRank Lease  = new("lease",  depth: 2);
    public static readonly LockRank Step   = new("step",   depth: 3);
    public static readonly LockRank Signal = new("signal", depth: 4);
    public static readonly LockRank Outbox = new("outbox", depth: 5);
    public int Depth { get; }
    private LockRank(string key, int depth) : this(key) => Depth = depth;
}

public readonly record struct LockScope(LockRank Rank, string Scope) {
    public string Key => $"{Rank.Key}:{Scope}";
}

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
    public sealed record LeaseGuard(LeaseKey Lease, LeaseToken Token) : CoordinationOp;
    public sealed record BudgetLoad : CoordinationOp;
    public sealed record BudgetToken : CoordinationOp;
    public sealed record MembershipUpsert(MembershipKey Group, MemberId Member, Duration Ttl) : CoordinationOp;
    public sealed record MembershipRelease(MembershipKey Group, MemberId Member) : CoordinationOp;
    public sealed record MembershipScan(MembershipKey Group) : CoordinationOp;
    public sealed record OutboxAdvance(SinkKey Sink, long Through) : CoordinationOp;
    public sealed record OutboxPending(SinkKey Sink, long After, int Take) : CoordinationOp;
    public sealed record OutboxPark(SinkKey Sink, long Sequence, int Attempt, string Status) : CoordinationOp;
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct CoordRow(string Key, string State, long Fence, Option<long> Value, Instant Until, Option<JsonElement> Payload);

public readonly record struct CoordinationFact(FaultId Identity, string Route);

public sealed record OutboxDeferred(long Sequence, int Attempt, string Status, Instant At);

public sealed record OutboxCursor(SinkKey Sink, long Sequence, Option<OutboxDeferred> Deferred) {
    public static OutboxCursor Genesis(SinkKey sink) => new(sink, 0L, None);
}

public sealed record PendingOutbox(long Sequence, JsonElement Envelope, Option<OutboxDeferred> Deferred);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinationReceipt : IValidityEvidence {
    private CoordinationReceipt() { }

    public sealed record Debited(HashMap<string, long> Balances, Option<long> Fence, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Stepped(WorkflowKey Workflow, StepKey Step, StepState Committed, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Signaled(WorkflowKey Instance, SignalKey Channel, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Leased(LeaseKey Lease, LeaseToken Token, Instant Until, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Member(MembershipKey Group, MemberId Id, Instant Until, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Advanced(OutboxCursor Cursor, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Parked(SinkKey Sink, long Sequence, int Attempt, string Status, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Pending(SinkKey Sink, OutboxCursor Cursor, Seq<PendingOutbox> Rows, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Loaded(Seq<CoordRow> Rows, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;

    public bool IsValid => Switch(
        debited:  static c => ValidityClaim.All(c.Balances.Values.ForAll(static b => b >= 0L), c.Fence.ForAll(static fence => fence >= 0L)),
        stepped:  static c => ValidityClaim.All(!string.IsNullOrEmpty(c.Step.Value)),
        signaled: static c => ValidityClaim.All(!string.IsNullOrEmpty(c.Channel.Value)),
        leased:   static c => ValidityClaim.All(c.Token.Value > 0L, c.Until > c.At),
        member:   static c => ValidityClaim.All(c.Until >= c.At),
        advanced: static c => ValidityClaim.All(ValidityClaim.Nonnegative(c.Cursor.Sequence)),
        parked:   static c => ValidityClaim.All(ValidityClaim.Nonnegative(c.Sequence), ValidityClaim.Nonnegative(c.Attempt)),
        pending:  static c => ValidityClaim.All(
            ValidityClaim.Nonnegative(c.Cursor.Sequence),
            c.Rows.ForAll(row => row.Sequence > c.Cursor.Sequence),
            c.Cursor.Deferred.Match(
                Some: deferred => c.Rows.Head.Exists(row => deferred.Sequence == row.Sequence),
                None: static () => true),
            c.Rows.Tail.ForAll(static row => row.Deferred.IsNone)),
        loaded:   static c => ValidityClaim.All(ValidityClaim.CountAtLeast(c.Rows.Count, 0)));
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinationFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.StoreCoordination;
    private CoordinationFault() { }

    [FaultCase(0)] public sealed partial record Unmapped(string SqlState, Error Cause) : CoordinationFault, ICausedFault;

    [FaultCase(1)] public sealed partial record LeaseFenced(Option<LeaseToken> Stale, Option<long> Current) : CoordinationFault {
        public override RetryShape Route => RetryShape.Rescoped;
    }
    [FaultCase(2)] public sealed partial record CasConflict(WorkflowKey Workflow, StepKey Step, StepState Expected, StepState Found) : CoordinationFault {
        public override RetryShape Route => RetryShape.Rescoped;
    }
    [FaultCase(3)] public sealed partial record BudgetExhausted(string Unit, long Requested, long Available) : CoordinationFault;
    [FaultCase(4)] public sealed partial record LeaseExpired(LeaseKey Lease, HolderId Holder) : CoordinationFault;
    [FaultCase(5)] public sealed partial record MembershipLapsed(MembershipKey Group, MemberId Member) : CoordinationFault;
    [FaultCase(6)] public sealed partial record OutboxDrain(SinkKey Sink, long Through) : CoordinationFault;
    [FaultCase(7)] public sealed partial record Refused(string Detail) : CoordinationFault;
    [FaultCase(8)] public sealed partial record Contended(string SqlState, Error Cause) : CoordinationFault, ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(9)] public sealed partial record Unreachable(Error Cause) : CoordinationFault, ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }
    public virtual RetryShape Route =>
        Retriability is Retriability.TerminalCase ? RetryShape.Terminal : RetryShape.Waited;

    public override string Message => Switch(
        leaseFenced:      static c => $"<lease-fenced:{c.Stale}<{c.Current}>",
        casConflict:      static c => $"<cas-conflict:{c.Workflow.Value}/{c.Step.Value}:{c.Expected.Key}!={c.Found.Key}>",
        budgetExhausted:  static c => $"<budget-exhausted:{c.Unit}:{c.Requested}>{c.Available}>",
        leaseExpired:     static c => $"<lease-expired:{c.Lease.Value}:{c.Holder.Value}>",
        membershipLapsed: static c => $"<membership-lapsed:{c.Group.Value}:{c.Member.Value}>",
        outboxDrain:      static c => $"<outbox-drain:{c.Sink.Value}@{c.Through}>",
        refused:          static c => $"<coordination-refused:{c.Detail}>",
        contended:        static c => $"<coordination-contended:{c.SqlState}>:{c.Cause.Message}",
        unreachable:      static c => $"<coordination-unreachable:{c.Cause.Message}>",
        unmapped:         static c => $"<sqlstate:{c.SqlState}>:{c.Cause.Message}");

    public static Error Lift(Error error) => error switch {
        CoordinationFault fault => fault,
        { Exception.Case: PostgresException pg } => pg.IsTransient
            ? new Contended(pg.SqlState, error)
            : new Unmapped(pg.SqlState, error),
        { Exception.Case: NpgsqlException { IsTransient: true } } => new Unreachable(error),
        _ => error,
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Coordinate {
    const string LockSql = "SELECT pg_advisory_xact_lock(hashtext(@tenant || ':' || @key))";
    const string WakeSql = "SELECT pg_notify(@channel, @sink)";
    const string Mark = "SAVEPOINT rasm_coord";
    const string Undo = "ROLLBACK TO SAVEPOINT rasm_coord";

    public static readonly StoreSlot DebitSlot = StoreSlot.Create("store.coordination.debit");
    public static readonly StoreSlot CreditSlot = StoreSlot.Create("store.coordination.credit");
    public static readonly StoreSlot StepSlot = StoreSlot.Create("store.coordination.step");
    public static readonly StoreSlot SignalSlot = StoreSlot.Create("store.coordination.signal");
    public static readonly StoreSlot LeaseSlot = StoreSlot.Create("store.coordination.lease");
    public static readonly StoreSlot MemberSlot = StoreSlot.Create("store.coordination.member");
    public static readonly StoreSlot ReadSlot = StoreSlot.Create("store.coordination.read");
    public static readonly StoreSlot OutboxSlot = StoreSlot.Create("store.coordination.outbox");
    public static readonly StoreSlot FaultSlot = StoreSlot.Create("store.coordination.fault");

    public static readonly Seq<StoreSlot> Slots = Seq(
        DebitSlot, CreditSlot, StepSlot, SignalSlot, LeaseSlot, MemberSlot, ReadSlot, OutboxSlot, FaultSlot);

    public static CoordinationFact Refusal(CoordinationFault fault) => new(fault.Identity, fault.Route.Key);

    public static IO<Fin<Seq<CoordinationReceipt>>> Run(IDocumentSession session, ReceiptSinkPort sink, Seq<CoordinationOp> ops, Option<LeaseToken> held, ProjectionContext frame, CancellationToken cancellationToken) =>
        from mark in IO.lift(frame.Mark)
        from sql in IO.lift(() => ops.Map(op => CaseSql.For(op, frame.Now())))
        from outcome in sql.Exists(static row => row.RequiresToken) && held.IsNone
            ? IO.pure(Fin<Seq<CoordinationReceipt>>.Fail(new CoordinationFault.Refused("<missing-fence-token>")))
            : Bracket(session, ops, sql, held, frame, mark, None, cancellationToken)
        from _ in Emit(sink, ops, outcome, frame)
        select outcome;

    public static IO<Fin<OutboxCursor>> QuarantineAndAdvance(
        IDocumentSession session,
        ReceiptSinkPort sink,
        global::Rasm.Persistence.Version.DeadLetterRow letter,
        LeaseToken held,
        ProjectionContext frame,
        CancellationToken cancellationToken) {
        CoordinationOp op = new CoordinationOp.OutboxAdvance(letter.Sink, letter.Sequence);
        Seq<CoordinationOp> ops = Seq(op);
        return from mark in IO.lift(frame.Mark)
               from sql in IO.lift(() => ops.Map(value => CaseSql.For(value, frame.Now())))
               from outcome in Bracket(
                   session, ops, sql, Some(held), frame, mark,
                   Some<Action<IDocumentSession>>(opened => opened.Store(letter)), cancellationToken)
               from _ in Emit(sink, ops, outcome, frame)
               select outcome.Bind(receipts => receipts.Head.Match(
                   Some: receipt => receipt is CoordinationReceipt.Advanced { Cursor: var cursor }
                       && cursor.Sink == letter.Sink
                       && cursor.Sequence >= letter.Sequence
                           ? Fin<OutboxCursor>.Succ(cursor)
                           : Fin<OutboxCursor>.Fail(new CoordinationFault.OutboxDrain(letter.Sink, letter.Sequence)),
                   None: () => Fin<OutboxCursor>.Fail(new CoordinationFault.OutboxDrain(letter.Sink, letter.Sequence))));
    }

    static StoreSlot SlotOf(CoordinationOp op) => op.Switch(
        budgetDebit:       static _ => DebitSlot,  budgetCredit:      static _ => CreditSlot,
        stepStateCas:      static _ => StepSlot,   stepStateInFlight: static _ => ReadSlot,
        stepStateLoad:     static _ => ReadSlot,   signalPut:         static _ => SignalSlot,
        signalLoad:        static _ => ReadSlot,   leaseAcquire:      static _ => LeaseSlot,
        leaseRenew:        static _ => LeaseSlot,  leaseRelease:      static _ => LeaseSlot,
        expiredScan:       static _ => ReadSlot,   membershipUpsert:  static _ => MemberSlot,
        membershipRelease: static _ => MemberSlot, membershipScan:    static _ => ReadSlot,
        outboxAdvance:     static _ => OutboxSlot, outboxPending:     static _ => ReadSlot,
        outboxPark:        static _ => OutboxSlot, leaseGuard:        static _ => ReadSlot,
        budgetLoad:        static _ => ReadSlot,   budgetToken:       static _ => ReadSlot);

    static IO<Unit> Emit(ReceiptSinkPort sink, Seq<CoordinationOp> ops, Fin<Seq<CoordinationReceipt>> outcome, ProjectionContext frame) =>
        outcome.Match(
            Succ: receipts => ops.Zip(receipts).TraverseM(pair => Send(sink, frame, SlotOf(pair.Item1), pair.Item2)).As().Map(static _ => unit),
            Fail: error => Send(sink, frame, FaultSlot, Refusal(CoordinationFault.Lift(error))));

    static IO<Unit> Send<TFact>(ReceiptSinkPort sink, ProjectionContext frame, StoreSlot slot, TFact fact) =>
        sink.Send(frame.Correlation, frame.Tenant, TelemetrySource.Persistence.Key, slot,
            JsonSerializer.SerializeToElement(fact, ElementJson.Options)).Map(static _ => unit);

    static IO<Fin<Seq<CoordinationReceipt>>> Bracket(
        IDocumentSession session,
        Seq<CoordinationOp> ops,
        Seq<CaseSql> sql,
        Option<LeaseToken> held,
        ProjectionContext frame,
        long mark,
        Option<Action<IDocumentSession>> commit,
        CancellationToken cancellationToken) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async token => {
            await session.BeginTransactionAsync(token).ConfigureAwait(false);
            await using NpgsqlBatch batch = new((NpgsqlConnection)session.Connection!);
            Seq<LockScope> locks = toSeq(sql.Bind(static row => row.Locks).Distinct()
                .OrderBy(static scope => scope.Rank.Depth).ThenBy(static scope => scope.Scope, StringComparer.Ordinal));
            locks.Iter(scope => batch.BatchCommands.Add(Bound(LockSql, Seq(("key", (object)scope.Key)), held, frame)));
            batch.BatchCommands.Add(new NpgsqlBatchCommand(Mark));
            sql.Iter(row => {
                row.Guarded.IfSome(statement => batch.BatchCommands.Add(Bound(statement, row.Binds, held, frame)));
                batch.BatchCommands.Add(Bound(row.Truth, row.Binds, held, frame));
                row.Wake.IfSome(channel => batch.BatchCommands.Add(Bound(WakeSql, row.Binds.Add(("channel", (object)channel)), held, frame)));
            });
            Seq<Seq<CoordRow>> sets = await Sets(batch, token).ConfigureAwait(false);
            Fin<Seq<CoordinationReceipt>> outcome = Project(ops, sql, sets.Skip(locks.Count + 1), held, frame, mark);
            if (outcome.IsFail) { await Rollback(batch.Connection!, token).ConfigureAwait(false); }
            else { commit.IfSome(apply => apply(session)); }
            await session.SaveChangesAsync(token).ConfigureAwait(false);
            return outcome;
        }, cancellationToken).ConfigureAwait(false)).MapFail(CoordinationFault.Lift));

    public static IO<Fin<Seq<CoordinationReceipt>>> Verified(IDocumentSession session, Seq<CoordinationOp> ops, Option<LeaseToken> held, ProjectionContext frame, CancellationToken cancellationToken) =>
        from mark in IO.lift(frame.Mark)
        from sql in IO.lift(() => ops.Map(op => CaseSql.For(op, frame.Now())))
        from outcome in Truths(session, ops, sql, held, frame, mark, cancellationToken)
        select outcome;

    static IO<Fin<Seq<CoordinationReceipt>>> Truths(IDocumentSession session, Seq<CoordinationOp> ops, Seq<CaseSql> sql, Option<LeaseToken> held, ProjectionContext frame, long mark, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async token => {
            await session.BeginTransactionAsync(token).ConfigureAwait(false);
            await using NpgsqlBatch batch = new((NpgsqlConnection)session.Connection!);
            Seq<CaseSql> probes = sql.Map(static row => row with { Guarded = None, Wake = None });
            probes.Iter(row => batch.BatchCommands.Add(Bound(row.Truth, row.Binds, held, frame)));
            Seq<Seq<CoordRow>> sets = await Sets(batch, token).ConfigureAwait(false);
            return Project(ops, probes, sets, held, frame, mark);
        }, cancellationToken).ConfigureAwait(false)).MapFail(CoordinationFault.Lift));

    static async Task Rollback(NpgsqlConnection connection, CancellationToken cancellationToken) {
        await using NpgsqlCommand undo = new(Undo, connection);
        _ = await undo.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    static NpgsqlBatchCommand Bound(string statement, Seq<(string Name, object Value)> binds, Option<LeaseToken> held, ProjectionContext frame) {
        NpgsqlBatchCommand command = new(statement) {
            Parameters = {
                new NpgsqlParameter<string>("tenant", frame.Tenant.Entry),
                new NpgsqlParameter<long?>("token", held.Map(static t => t.Value).ToNullable()),
            },
        };
        foreach ((string name, object value) in binds) { command.Parameters.Add(new NpgsqlParameter(name, value)); }
        return command;
    }

    static async Task<Seq<Seq<CoordRow>>> Sets(NpgsqlBatch batch, CancellationToken cancellationToken) {
        List<Seq<CoordRow>> sets = [];
        await using NpgsqlDataReader reader = await batch.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        do {
            List<CoordRow> rows = [];
            while (reader.FieldCount >= 6 && await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) { rows.Add(Read(reader)); }
            sets.Add(toSeq(rows));
        } while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));
        return toSeq(sets);
    }

    static CoordRow Read(NpgsqlDataReader reader) => new(
        reader.GetString(0), reader.GetString(1), reader.GetInt64(2),
        reader.IsDBNull(3) ? None : Some(reader.GetInt64(3)),
        Instant.FromDateTimeUtc(DateTime.SpecifyKind(reader.GetDateTime(4), DateTimeKind.Utc)),
        reader.IsDBNull(5) ? None : Some(ReadPayload(reader.GetString(5))));

    static JsonElement ReadPayload(string json) { using JsonDocument document = JsonDocument.Parse(json); return document.RootElement.Clone(); }

    static Fin<Seq<CoordinationReceipt>> Project(Seq<CoordinationOp> ops, Seq<CaseSql> sql, Seq<Seq<CoordRow>> sets, Option<LeaseToken> held, ProjectionContext frame, long mark) =>
        ops.Zip(Slice(sql, sets)).TraverseM(pair => Verdict(pair.Item1, pair.Item2, held, frame, mark)).As();

    static Seq<Seq<CoordRow>> Slice(Seq<CaseSql> sql, Seq<Seq<CoordRow>> sets) =>
        sql.Fold((Cursor: 0, Slices: Seq<Seq<CoordRow>>()), (state, row) =>
            (state.Cursor + row.Sets, state.Slices.Add(sets.Skip(state.Cursor).Take(row.Sets).Bind(static rows => rows)))).Slices;

    static Fin<CoordinationReceipt> Verdict(CoordinationOp op, Seq<CoordRow> rows, Option<LeaseToken> held, ProjectionContext frame, long mark) => op.Switch(
        budgetDebit: d => Settled(d.Debit, rows, held, frame, mark),
        budgetCredit: c => Settled(c.Credit, rows, held, frame, mark),
        signalPut: s => rows.Head.Match(
            Some: r  => r.State == "signaled"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Signaled(s.Instance, s.Channel, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held, Some(r.Fence))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.Refused($"<signal:{s.Instance.Value}/{s.Channel.Value}>"))),
        signalLoad: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        stepStateCas: c => rows.Head.Match(
            Some: r => r.State == c.Next.Key
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Stepped(c.Workflow, c.Step, c.Next, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : held.Exists(token => r.Fence > token.Value)
                    ? Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held, Some(r.Fence)))
                    : Fin<CoordinationReceipt>.Fail(new CoordinationFault.CasConflict(c.Workflow, c.Step, c.Expected, StepState.Get(r.State))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.Refused($"<step-missing:{c.Workflow.Value}/{c.Step.Value}>"))),
        stepStateInFlight: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        stepStateLoad:     _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        leaseAcquire: a => rows.Head.Match(
            Some: r => r.State == "held"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Leased(a.Lease, LeaseToken.Create(r.Fence), r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(None, Some(r.Fence))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseExpired(a.Lease, a.Holder))),
        leaseRenew: n => Held(rows, n.Lease, n.Token, frame, mark),
        leaseRelease: n => Held(rows, n.Lease, n.Token, frame, mark),
        expiredScan: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        membershipUpsert: m => rows.Head.Match(
            Some: r  => r.State == "serving"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Member(m.Group, m.Member, r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held, Some(r.Fence))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.MembershipLapsed(m.Group, m.Member))),
        membershipRelease: m => rows.Head.Match(
            Some: r  => r.State == "departed"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Member(m.Group, m.Member, r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held, Some(r.Fence))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.MembershipLapsed(m.Group, m.Member))),
        membershipScan: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        outboxAdvance: a => rows.Head.Match(
            Some: row => Cursor(row, a.Sink).Bind(cursor => cursor.Sequence >= a.Through
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Advanced(
                    cursor, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(a.Sink, a.Through))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(a.Sink, a.Through))),
        outboxPending: p => Pending(p, rows, frame, mark),
        outboxPark: k => Parked(k, rows, held, frame, mark),
        leaseGuard:  _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        budgetLoad:  _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        budgetToken: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))));

    static Fin<CoordinationReceipt> Pending(
        CoordinationOp.OutboxPending op, Seq<CoordRow> rows, ProjectionContext frame, long mark) =>
        rows.Head.Match(
            Some: cursor => cursor.State == "cursor" && cursor.Value.IsSome
                ? Cursor(cursor, op.Sink).Bind(held => held.Sequence == op.After
                    ? rows.Tail.Traverse(PendingRow).As().Map(pending =>
                        (CoordinationReceipt)new CoordinationReceipt.Pending(
                            op.Sink, held, pending, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                    : Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(op.Sink, op.After)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(op.Sink, op.After)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(op.Sink, op.After)));

    static Fin<OutboxCursor> Cursor(CoordRow row, SinkKey sink) => row.Payload.Match(
        Some: payload => row.Value.Match(
            Some: sequence => Deferred(payload).Map(deferred => new OutboxCursor(sink, sequence, deferred)),
            None: () => Fin<OutboxCursor>.Fail(new CoordinationFault.Refused($"<outbox-cursor-sequence:{sink.Value}>"))),
        None: () => Fin<OutboxCursor>.Fail(new CoordinationFault.Refused($"<outbox-cursor-payload:{sink.Value}>")));

    static Fin<PendingOutbox> PendingRow(CoordRow row) => row.Payload.Match(
        Some: payload => row.Value.Match(
            Some: sequence => payload.TryGetProperty("envelope", out JsonElement envelope)
                ? Deferred(payload).Bind(deferred => deferred.ForAll(state => state.Sequence == sequence)
                    ? Fin<PendingOutbox>.Succ(new PendingOutbox(sequence, envelope.Clone(), deferred))
                    : Fin<PendingOutbox>.Fail(new CoordinationFault.Refused($"<outbox-deferred-mismatch:{sequence}>")))
                : Fin<PendingOutbox>.Fail(new CoordinationFault.Refused($"<outbox-envelope-missing:{sequence}>")),
            None: () => Fin<PendingOutbox>.Fail(new CoordinationFault.Refused($"<outbox-sequence-missing:{row.Key}>"))),
        None: () => Fin<PendingOutbox>.Fail(new CoordinationFault.Refused($"<outbox-payload-missing:{row.Key}>")));

    static Fin<Option<OutboxDeferred>> Deferred(JsonElement payload) {
        if (!payload.TryGetProperty("parked", out JsonElement parked) || parked.ValueKind == JsonValueKind.Null) {
            return Fin<Option<OutboxDeferred>>.Succ(None);
        }
        return parked.TryGetInt64(out long sequence)
            && payload.TryGetProperty("attempt", out JsonElement attemptElement)
            && attemptElement.TryGetInt32(out int attempt)
            && attempt > 0
            && payload.TryGetProperty("status", out JsonElement statusElement)
            && statusElement.ValueKind == JsonValueKind.String
            && statusElement.GetString() is { Length: > 0 } status
            && payload.TryGetProperty("parkedAt", out JsonElement atElement)
            && atElement.ValueKind == JsonValueKind.String
            && InstantPattern.ExtendedIso.Parse(atElement.GetString() ?? "").TryGetValue(default, out Instant at)
                ? Fin<Option<OutboxDeferred>>.Succ(Some(new OutboxDeferred(sequence, attempt, status, at)))
                : Fin<Option<OutboxDeferred>>.Fail(new CoordinationFault.Refused("<outbox-deferred-malformed>"));
    }

    static Fin<CoordinationReceipt> Parked(
        CoordinationOp.OutboxPark op, Seq<CoordRow> rows, Option<LeaseToken> held, ProjectionContext frame, long mark) =>
        rows.Head.Match(
            Some: row => row.Payload.Match(
                Some: payload => Deferred(payload).Bind(deferred => deferred.Match(
                    Some: state => row.State == "parked"
                        && state.Sequence == op.Sequence
                        && state.Attempt == op.Attempt
                        && StringComparer.Ordinal.Equals(state.Status, op.Status)
                            ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Parked(
                                op.Sink, state.Sequence, state.Attempt, state.Status, state.At,
                                frame.Correlation, frame.Elapsed(mark)))
                            : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held, Some(row.Fence))),
                    None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(op.Sink, op.Sequence)))),
                None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(op.Sink, op.Sequence))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(op.Sink, op.Sequence)));

    static Fin<CoordinationReceipt> Settled(HashMap<string, long> amounts, Seq<CoordRow> rows, Option<LeaseToken> held, ProjectionContext frame, long mark) {
        Seq<CoordRow> applied = rows.Filter(static row => row.State == "applied");
        if (applied.Find(static row => row.Value.IsNone) is { IsSome: true, Case: CoordRow malformed }) {
            return Fin<CoordinationReceipt>.Fail(new CoordinationFault.Refused($"<budget-result-missing:{malformed.Key}>"));
        }
        return toSeq(amounts).Filter(request => !applied.Exists(row => row.Key == request.Key)).Head.Match(
            Some: request => rows.Find(row => row.State == "current" && row.Key == request.Key).Match(
                Some: truth => held.Exists(token => truth.Fence > token.Value)
                    ? Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held, Some(truth.Fence)))
                    : truth.Value.Match(
                        Some: available => Fin<CoordinationReceipt>.Fail(
                            new CoordinationFault.BudgetExhausted(request.Key, request.Value, available)),
                        None: () => Fin<CoordinationReceipt>.Fail(
                            new CoordinationFault.Refused($"<budget-truth-missing:{request.Key}>"))),
                None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.BudgetExhausted(request.Key, request.Value, 0L))),
            None: () => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Debited(
                toHashMap(applied.Choose(static row => row.Value.Map(value => (row.Key, value)))),
                applied.Head.Map(static row => row.Fence), frame.Now(), frame.Correlation, frame.Elapsed(mark))));
    }

    static Fin<CoordinationReceipt> Held(Seq<CoordRow> rows, LeaseKey lease, LeaseToken token, ProjectionContext frame, long mark) =>
        rows.Head.Match(
            Some: r => r.State is "held" or "released"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Leased(lease, LeaseToken.Create(r.Fence), r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(Some(token), Some(r.Fence))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(Some(token), None)));
}

public readonly record struct CaseSql(Seq<LockScope> Locks, bool RequiresToken, Option<string> Guarded, string Truth, Option<string> Wake, Seq<(string Name, object Value)> Binds) {
    public int Sets => (Guarded.IsSome ? 1 : 0) + 1 + (Wake.IsSome ? 1 : 0);

    public static CaseSql For(CoordinationOp op, Instant now) => op.Switch(
        budgetDebit: value => Budget(value.Debit, DebitSql),
        budgetCredit: value => Budget(value.Credit, CreditSql),
        stepStateCas: value => Write(new LockScope(LockRank.Step, $"{value.Workflow.Value}:{value.Step.Value}"),
            "UPDATE workflow_step SET state=@next, fence=@token WHERE tenant=@tenant AND workflow=@workflow AND step=@step AND fence<=@token AND state=@expected RETURNING step, state, fence, NULL::bigint, updated_at, NULL::jsonb",
            "SELECT step, state, fence, NULL::bigint, updated_at, NULL::jsonb FROM workflow_step WHERE tenant=@tenant AND workflow=@workflow AND step=@step",
            ("workflow", value.Workflow.Value), ("step", value.Step.Value), ("expected", value.Expected.Key), ("next", value.Next.Key)),
        stepStateInFlight: value => Read(
            "SELECT step, state, fence, NULL::bigint, updated_at, NULL::jsonb FROM workflow_step WHERE tenant=@tenant AND workflow=@workflow AND state NOT IN ('done','faulted') ORDER BY step",
            ("workflow", value.Workflow.Value)),
        stepStateLoad: value => Read(
            "SELECT step, state, fence, NULL::bigint, updated_at, NULL::jsonb FROM workflow_step WHERE tenant=@tenant AND workflow=@workflow AND step=@step",
            ("workflow", value.Workflow.Value), ("step", value.Step.Value)),
        signalPut: value => Write(new LockScope(LockRank.Signal, $"{value.Instance.Value}:{value.Channel.Value}"),
            "INSERT INTO workflow_signal(tenant,workflow,channel,payload,fence,updated_at) VALUES(@tenant,@workflow,@channel,@payload,@token,@now) ON CONFLICT(tenant,workflow,channel) DO UPDATE SET payload=excluded.payload,fence=excluded.fence,updated_at=excluded.updated_at WHERE workflow_signal.fence<=@token RETURNING channel,'signaled',fence,NULL::bigint,updated_at,payload",
            "SELECT channel,'current',fence,NULL::bigint,updated_at,payload FROM workflow_signal WHERE tenant=@tenant AND workflow=@workflow AND channel=@channel",
            ("workflow", value.Instance.Value), ("channel", value.Channel.Value), ("payload", value.Payload), ("now", now)),
        signalLoad: value => Read(
            "SELECT channel,'signaled',fence,NULL::bigint,updated_at,payload FROM workflow_signal WHERE tenant=@tenant AND workflow=@workflow AND channel=@channel",
            ("workflow", value.Instance.Value), ("channel", value.Channel.Value)),
        leaseAcquire: value => Mint(new LockScope(LockRank.Lease, value.Lease.Value),
            "INSERT INTO lease(tenant,key,holder,generation,until) VALUES(@tenant,@lease,@holder,1,@until) ON CONFLICT(tenant,key) DO UPDATE SET holder=excluded.holder,generation=lease.generation+1,until=excluded.until WHERE lease.holder=@holder OR lease.until<@now RETURNING key,'held',generation,NULL::bigint,until,NULL::jsonb",
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND key=@lease",
            ("lease", value.Lease.Value), ("holder", value.Holder.Value), ("until", now + value.Ttl), ("now", now)),
        leaseRenew: value => Write(new LockScope(LockRank.Lease, value.Lease.Value),
            "UPDATE lease SET until=@until WHERE tenant=@tenant AND key=@lease AND generation=@token RETURNING key,'held',generation,NULL::bigint,until,NULL::jsonb",
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND key=@lease",
            ("lease", value.Lease.Value), ("until", now + value.Ttl)),
        leaseRelease: value => Write(new LockScope(LockRank.Lease, value.Lease.Value),
            "DELETE FROM lease WHERE tenant=@tenant AND key=@lease AND generation=@token RETURNING key,'released',generation,NULL::bigint,until,NULL::jsonb",
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND key=@lease",
            ("lease", value.Lease.Value)),
        expiredScan: _ => Read(
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND until<@now ORDER BY until,key",
            ("now", now)),
        membershipUpsert: value => Write(new LockScope(LockRank.Member, $"{value.Group.Value}:{value.Member.Value}"),
            "INSERT INTO membership(tenant,group_key,member,until,fence) VALUES(@tenant,@group,@member,@until,@token) ON CONFLICT(tenant,group_key,member) DO UPDATE SET until=excluded.until,fence=excluded.fence WHERE membership.fence<=@token RETURNING member,'serving',fence,NULL::bigint,until,NULL::jsonb",
            "SELECT member,'current',fence,NULL::bigint,until,NULL::jsonb FROM membership WHERE tenant=@tenant AND group_key=@group AND member=@member",
            ("group", value.Group.Value), ("member", value.Member.Value), ("until", now + value.Ttl)),
        membershipRelease: value => Write(new LockScope(LockRank.Member, $"{value.Group.Value}:{value.Member.Value}"),
            "DELETE FROM membership WHERE tenant=@tenant AND group_key=@group AND member=@member AND fence<=@token RETURNING member,'departed',fence,NULL::bigint,until,NULL::jsonb",
            "SELECT member,'current',fence,NULL::bigint,until,NULL::jsonb FROM membership WHERE tenant=@tenant AND group_key=@group AND member=@member",
            ("group", value.Group.Value), ("member", value.Member.Value)),
        membershipScan: value => Read(
            "SELECT member,'serving',fence,NULL::bigint,until,NULL::jsonb FROM membership WHERE tenant=@tenant AND group_key=@group AND until>=@now ORDER BY member",
            ("group", value.Group.Value), ("now", now)),
        outboxAdvance: value => WriteWake(new LockScope(LockRank.Outbox, value.Sink.Value),
            "INSERT INTO outbox_cursor(tenant,sink,sequence,parked,attempt,status,parked_at,updated_at,fence) VALUES(@tenant,@sink,@through,NULL,0,NULL,NULL,@now,@token) ON CONFLICT(tenant,sink) DO UPDATE SET sequence=excluded.sequence,parked=NULL,attempt=0,status=NULL,parked_at=NULL,updated_at=excluded.updated_at,fence=excluded.fence WHERE outbox_cursor.fence<=@token AND outbox_cursor.sequence<@through RETURNING sink,'advanced',fence,sequence,updated_at,jsonb_build_object('parked',parked,'attempt',attempt,'status',status,'parkedAt',parked_at)",
            "SELECT sink,'current',fence,sequence,updated_at,jsonb_build_object('parked',parked,'attempt',attempt,'status',status,'parkedAt',parked_at) FROM outbox_cursor WHERE tenant=@tenant AND sink=@sink",
            "rasm_outbox",
            ("sink", value.Sink.Value), ("through", value.Through), ("now", now)),
        outboxPending: value => Read(
            "WITH held AS (SELECT sink,sequence,parked,attempt,status,parked_at,updated_at,fence FROM outbox_cursor WHERE tenant=@tenant AND sink=@sink), position AS (SELECT * FROM held UNION ALL SELECT @sink,0,NULL,0,NULL,NULL,'epoch'::timestamptz,0 WHERE NOT EXISTS (SELECT 1 FROM held)), projected AS (SELECT sink::text AS key,'cursor'::text AS state,fence,sequence AS value,updated_at AS until,jsonb_build_object('parked',parked,'attempt',attempt,'status',status,'parkedAt',parked_at) AS payload FROM position UNION ALL SELECT entry.id::text,CASE WHEN position.parked=entry.sequence THEN 'deferred' ELSE 'pending' END,entry.fence,entry.sequence,CASE WHEN position.parked=entry.sequence THEN position.parked_at ELSE entry.committed_at END,jsonb_build_object('envelope',entry.envelope,'parked',CASE WHEN position.parked=entry.sequence THEN position.parked END,'attempt',CASE WHEN position.parked=entry.sequence THEN position.attempt END,'status',CASE WHEN position.parked=entry.sequence THEN position.status END,'parkedAt',CASE WHEN position.parked=entry.sequence THEN position.parked_at END) FROM position JOIN LATERAL (SELECT id,fence,sequence,committed_at,envelope FROM op_log WHERE tenant=@tenant AND sequence>position.sequence AND @after=position.sequence ORDER BY sequence LIMIT @take) entry ON true) SELECT key,state,fence,value,until,payload FROM projected ORDER BY value",
            ("sink", value.Sink.Value), ("after", value.After), ("take", value.Take)),
        outboxPark: value => Write(new LockScope(LockRank.Outbox, value.Sink.Value),
            "INSERT INTO outbox_cursor(tenant,sink,sequence,parked,attempt,status,parked_at,updated_at,fence) SELECT @tenant,@sink,0,@sequence,@attempt,@status,@now,@now,@token WHERE @attempt>0 AND @sequence=(SELECT min(sequence) FROM op_log WHERE tenant=@tenant AND sequence>0) ON CONFLICT(tenant,sink) DO UPDATE SET parked=excluded.parked,attempt=excluded.attempt,status=excluded.status,parked_at=excluded.parked_at,updated_at=excluded.updated_at,fence=excluded.fence WHERE outbox_cursor.fence<=@token AND (outbox_cursor.parked IS NULL OR outbox_cursor.parked=excluded.parked) AND excluded.attempt>outbox_cursor.attempt AND excluded.parked=(SELECT min(sequence) FROM op_log WHERE tenant=@tenant AND sequence>outbox_cursor.sequence) RETURNING sink,'parked',fence,parked,parked_at,jsonb_build_object('parked',parked,'attempt',attempt,'status',status,'parkedAt',parked_at)",
            "SELECT sink,CASE WHEN parked IS NULL THEN 'current' ELSE 'parked' END,fence,parked,coalesce(parked_at,updated_at),jsonb_build_object('parked',parked,'attempt',attempt,'status',status,'parkedAt',parked_at) FROM outbox_cursor WHERE tenant=@tenant AND sink=@sink",
            ("sink", value.Sink.Value), ("sequence", value.Sequence), ("attempt", value.Attempt), ("status", value.Status), ("now", now)),
        leaseGuard: value => Read(
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND key=@lease",
            ("lease", value.Lease.Value)),
        budgetLoad: _ => Read(
            "SELECT unit,'current',fence,balance,'epoch'::timestamptz,NULL::jsonb FROM budget_ledger WHERE tenant=@tenant ORDER BY unit"),
        budgetToken: _ => Read(
            "SELECT @tenant,'current',coalesce(max(fence),0),count(*),'epoch'::timestamptz,NULL::jsonb FROM budget_ledger WHERE tenant=@tenant"));

    const string DebitSql = "UPDATE budget_ledger b SET balance=b.balance-r.amount,fence=@token FROM unnest(@units,@amounts) AS r(unit,amount) WHERE b.tenant=@tenant AND b.unit=r.unit AND b.balance>=r.amount AND b.fence<=@token RETURNING b.unit,'applied',b.fence,b.balance,'epoch'::timestamptz,NULL::jsonb";
    const string CreditSql = "INSERT INTO budget_ledger(tenant,unit,balance,fence) SELECT @tenant,r.unit,r.amount,@token FROM unnest(@units,@amounts) AS r(unit,amount) ORDER BY r.unit ON CONFLICT(tenant,unit) DO UPDATE SET balance=budget_ledger.balance+excluded.balance,fence=excluded.fence WHERE budget_ledger.fence<=@token RETURNING unit,'applied',fence,balance,'epoch'::timestamptz,NULL::jsonb";

    static CaseSql Budget(HashMap<string, long> amounts, string guarded) {
        Seq<(string Key, long Value)> ordered = toSeq(amounts).OrderBy(static row => row.Key, StringComparer.Ordinal).ToSeq();
        string[] units = ordered.Map(static row => row.Key).ToArray();
        long[] values = ordered.Map(static row => row.Value).ToArray();
        return Ledger(guarded,
            "SELECT unit,'current',fence,balance,'epoch'::timestamptz,NULL::jsonb FROM budget_ledger WHERE tenant=@tenant AND unit=ANY(@units) ORDER BY unit",
            ("units", units), ("amounts", values));
    }

    static CaseSql Ledger(string guarded, string truth, params (string Name, object Value)[] binds) =>
        new(Seq<LockScope>(), true, Some(guarded), truth, None, toSeq(binds));

    static CaseSql Write(LockScope scope, string guarded, string truth, params (string Name, object Value)[] binds) =>
        new(Seq(scope), true, Some(guarded), truth, None, toSeq(binds));

    static CaseSql Mint(LockScope scope, string guarded, string truth, params (string Name, object Value)[] binds) =>
        new(Seq(scope), false, Some(guarded), truth, None, toSeq(binds));

    static CaseSql WriteWake(LockScope scope, string guarded, string truth, string wake, params (string Name, object Value)[] binds) =>
        new(Seq(scope), true, Some(guarded), truth, Some(wake), toSeq(binds));

    static CaseSql Read(string truth, params (string Name, object Value)[] binds) =>
        new(Seq<LockScope>(), false, None, truth, None, toSeq(binds));
}
```

| [INDEX] | [POLICY]        | [VALUE]                                       | [BINDING]                                                            |
| :-----: | :-------------- | :-------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | fencing         | `fence <= @token` on every guarded write      | stale token → typed `LeaseFenced`, never a lost update               |
|  [02]   | generation mint | `LeaseAcquire` row-CAS `RETURNING generation` | monotone `++`; the token is validated, not merely issued             |
|  [03]   | budget shape    | per-unit vector, one conditional `UPDATE`     | the engine re-checks the guard post-block; no lock, no overdraw      |
|  [04]   | read guard      | tenant RLS predicate structural on every READ | no cross-tenant in-flight/lease/membership leak                      |
|  [05]   | lock order      | `pg_advisory_xact_lock` in `LockRank` order   | released at commit AND rollback; a session lock survives both        |
|  [06]   | receipt floor   | `IsValid => ValidityClaim.All(...)` per case  | kernel validity fold; `&&` chains deleted                            |
|  [07]   | port direction  | AppHost decodes Persistence-owned types       | four PORT rows + `MembershipView.Serving`; nothing crosses down      |
|  [08]   | row canon       | `(key, state, fence, value, until, payload)`  | fence and case scalar never alias; `CaseSql.For` generates every row |
|  [09]   | signal row      | fenced `(workflow, channel)` upsert           | `SignalPut`/`SignalLoad` decode `StepStateSeam.SignalPut`/`SignalOf` |
|  [10]   | throw crossing  | one `CoordinationFault.Lift` per rail leg     | banded case on every `Fin`; a bare `Error` is the deleted form       |
|  [11]   | retry axes      | kernel `Retriability` + `RetryShape` per case | provider-classified contention waits; a fenced token rescopes        |
|  [12]   | refusal fact    | `store.coordination.fault`, emitted at `Run`  | generated identity and retry route; no adapter re-mints it           |
|  [13]   | unit atomicity  | `SAVEPOINT rasm_coord` per batch              | a refusal undoes the unit; per-row re-check alone commits siblings   |
|  [14]   | entry arity     | `Run` takes the op `Seq`                      | two calls are two transactions no ordering law reaches across        |
|  [15]   | retry owner     | store execution strategy ABOVE `Run`          | `Lift` seats outer; `Verified` is the `verifySucceeded` probe        |
|  [16]   | budget polarity | vector is REMAINING per unit                  | no ceiling crosses; spend derives `ceiling - remaining`              |
|  [17]   | outbox park     | attempt and status on the per-sink cursor row | committed op-log stays one owner; no message-envelope table          |

## [03]-[OUTBOX_CURSOR]

- Owner: `OutboxCursor(Sink, Sequence, Deferred)` is the per-sink durable drain row. `OutboxDeferred(Sequence, Attempt, Status, At)` is its one optional head state; no sentinel or per-event delivery row exists. `OutboxPending` returns the cursor snapshot plus typed `PendingOutbox` rows, `OutboxPark` moves only the exact head with a strictly larger attempt, and `OutboxAdvance` clears the deferred value while moving forward. These are the only cursor writers under `ONE_OUTBOX_EGRESS_SPINE`.
- Entry: `OutboxPending(Sink, After, Take)` joins the named sink cursor and op-log in one snapshot. The cursor row is always returned, so `After` must equal its sequence even when the event window is empty. The first op-log sequence after it carries the persisted deferred attempt, status, and park instant when present; later rows remain ordered behind that head. Delivered prefixes call `Coordinate.Run(... OutboxAdvance ...)`. Terminal rows call `Coordinate.QuarantineAndAdvance`, which stores one `DeadLetterRow` and advances that exact sequence in the same session commit.
- Auto: the Marten event stream IS the outbox — a domain commit and its egress obligation are one `SaveChangesAsync`, so no message-envelope table or dual-write gap exists. Park requires the current op-log head and a monotone attempt. Advance clears `parked`, `attempt`, `status`, and `parked_at`; the dead-letter document is the terminal evidence, never residue on an already-advanced cursor. The same transaction emits `pg_notify('rasm_outbox', @sink)`; bounded polling owns correctness. Cursor `Sequence` is a store-local drain position, not an HLC or portable order.
- Receipt: a cursor advance rides `store.coordination.outbox` carrying the sink and the through-sequence; the held-cursor stall evidence is the egress pump's (`Version/egress#EGRESS_PUMP` `CursorStall`), never minted here.
- Growth: a new egress sink is one cursor row minted on first read, park, or advance. A second deferred store, terminal-status residue, per-event delivery row, trigger writer, or coordination-side pump read is rejected.
- Boundary: forward-only intra-leg edge — `Version/egress` drains this cursor and coordination NEVER reads the pump (the acyclicity proof's one intra-leg egress edge); the cursor is keyed PER SINK, so that pump and the AppHost relay are two consumers holding two rows of one table rather than two writers of one row, and the edge stays forward-only for each; delivery attempt and status ride the sink's own row, the committed op-log being the outbox; a caller composing an advance crosses WIRE-STABLE PRIMITIVES — the sink NAME beside the through-sequence, the shape the sibling dead-letter arrow already takes — because a relay holding no `SinkKey` cannot mint one and a port demanding it forecloses its own consumer; the failed advance CAS stays `CoordinationFault.OutboxDrain` in THIS band (the cursor write is fenced-store work) while every delivery fault is the pump's `EgressFault`; the presence/awareness lane (`ColumnFamily.Presence`, `durable: false`) never has a cursor row — only `Family.Durable` lanes drain past this cursor.

| [INDEX] | [POLICY]       | [VALUE]                                            | [BINDING]                                                    |
| :-----: | :------------- | :------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | outbox spine   | the Marten stream IS the outbox                    | same-`IDocumentSession`; message-envelope tables are deleted |
|  [02]   | cursor grain   | per sink, one optional deferred head               | distinct from `SyncCursor`; no sentinel or delivery table    |
|  [03]   | advance law    | forward-only CAS; clear deferred state             | quarantine and exact advance share one session commit        |
|  [04]   | pump wake      | `pg_notify('rasm_outbox', sink)` same-tx           | latency only — the bounded poll floor owns correctness       |
|  [05]   | edge direction | egress reads cursor; coordination never reads pump | the one forward-only intra-leg egress edge                   |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
