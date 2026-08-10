# [PERSISTENCE_STORE_COORDINATION]

Rasm.Persistence owns the token-VALIDATING fenced-lease coordination store — the ONE durable substrate behind the four AppHost PORT contracts (`Agent/capability` Budget debit/credit, `Runtime/orchestration` step-state CAS + durable signal, `Wire/outbox` transactional outbox, `Wire/coordination` CAS+lease+membership) — as one closed `CoordinationOp` `[Union]` dispatched by one `Coordinate.Run` bracket over the generated total `Switch`, exactly the `Element/graph#STORE_RAIL` idiom. `Run` takes the op SEQUENCE, so composing several ops into one atomic unit is the entry's own shape rather than caller-improvised repetition: it acquires every advisory key its rows name in `LockRank` order FIRST, executes the ops in CALLER order, and commits once — acquisition order and execution order are two orders. Every outcome emits through the injected `ReceiptSinkPort` at that one fold, a committed receipt under its verb slot and a typed refusal under `store.coordination.fault`. Every guarded write folds through ONE fenced-CAS predicate (`pg_advisory_xact_lock` + guarded `UPDATE … RETURNING` in one round trip, the token validated against the row's monotone lease generation so a stale holder is the typed `CoordinationFault.LeaseFenced`, never a lost update); every READ case folds through that same leg's truth projection carrying the frame's tenant RLS predicate STRUCTURALLY, so no read leaks cross-tenant in-flight/lease/membership state. Budget is a fenced compare-and-decrement over a PER-UNIT VECTOR (`HashMap<string, long>` mirroring the AppHost `CostVector` string keys — the smart-enum key crosses as its STRING, never the AppHost type) whose guard is PostgreSQL's own per-row `WHERE` re-check, the one conditional decrement the engine settles against a concurrent writer's committed version; all-or-nothing across the vector rides the batch `SAVEPOINT`, because a single statement drops each re-check failure from its result set and commits every unit that passed. Marten's event stream IS the outbox (`ONE_OUTBOX_EGRESS_SPINE` — same-`IDocumentSession` guarantee, so the domain event and its egress obligation commit in one transaction and a second message-envelope outbox table is the deleted parallel store); this page mints the durable PER-SINK drain cursor `outbox_cursor(SinkKey, long Sequence)` — distinct from the per-origin `SyncCursor` (`Version/ledger#CHANGEFEED`) — and `OutboxAdvance(Sink, Through)` is the cursor-advance case the `Version/egress` pump calls, forward-only: the pump reads the cursor, coordination never reads the pump. Coordination composes one direct `NpgsqlBatch` on the Marten session's transacted connection (the session transaction force-opened first, so lock + CAS + event commit share it; `QueueSqlCommand` is reserved for no-RETURNING side-writes — it defers to `SaveChangesAsync` and surfaces no result set, so it can never carry the RETURNING-vector CAS) + Npgsql `pg_advisory_xact_lock` + LISTEN/NOTIFY — never a second event store, never a distributed-lock sidecar (`DistributedLock.Postgres` carries no fencing token; the token-validating CAS is strictly stronger). Every throw crossing folds through ONE `CoordinationFault.Lift` into the 8430 band, which carries a per-case `IsTransient` discriminant because this rail states refusal as a `Fin` RESULT: the advisory lock and the guarded CAS raise serialization failures and deadlocks under the contention the fence arbitrates, so `Contended`/`Unreachable` publish that retriable class where a bare `Error` hid it from every caller predicate, deterministic refusals stay false, and `store.coordination.fault` carries the union's own category and bit onto the fact stream. Persistence OWNS the op-union, fencing tokens, membership rows, balance vectors, and receipts, which AppHost's `Wire/coordination`/`Wire/outbox` adapters DECODE — no AppHost type crosses down; tenant, wall clock, and correlation ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame — the kernel `CorrelationId`/`TenantContext` pair SEATED on it, so the RLS bind spells `frame.Tenant.Entry` and every receipt spells `frame.Correlation` with no per-seam lift. `FaultBand` arrives from `Element/graph#FAULT_TABLES`; `IValidityEvidence`/`ValidityClaim` from the `Rasm` kernel; `IDocumentSession`/`NpgsqlDataSource` from the substrate.

## [01]-[INDEX]

- [02]-[COORDINATION_OP]: `CoordinationOp`'s closed case family, the key/token/state vocabularies, the `LockRank` containment ladder every acquisition sorts on, the ONE fenced-CAS fold consuming the `CaseSql` data rows under one savepoint, the per-unit-vector Budget debit/credit, the durable-signal cases, the `CoordinationReceipt` validity fold, and the 8430 fault band with its `Lift` throw crossing, its per-case retriability discriminant, and its emitted refusal fact.
- [03]-[OUTBOX_CURSOR]: `outbox_cursor`'s per-sink row under the `ONE_OUTBOX_EGRESS_SPINE` law, the `OutboxAdvance` at-least-once advance CAS, and the LISTEN/NOTIFY pump wake.

## [02]-[COORDINATION_OP]

- Owner: `CoordinationOp` is the closed interaction family; every case — write, read, and cursor advance alike — folds through ONE `Bracket`. `LeaseToken` carries the monotone generation; the key and state vocabularies close their domains. `LockRank` is the containment ladder over the advisory-lock families and `LockScope` pairs a rank with its scope text, so the lock key's own prefix IS the rank key and the two cannot drift. `CoordRow` is the canonical `(key, state, fence, value, until, payload)` projection: `Fence` never aliases a budget balance or cursor sequence, and `Value` carries those case scalars. `CaseSql.For` is the total parameterized SQL generator over the op family, carrying lock scopes, token requirement, guarded statement, truth statement, and binds as data. `CoordinationReceipt`, `CoordinationFault`, and `Coordinate` own evidence, failures, and execution.
- Cases: `BudgetDebit(HashMap<string, long> Debit)` the per-unit-vector fenced compare-and-decrement (`capability.md` `CostVector` crosses as its `[SmartEnum<string>]` STRING key, so the row is `HashMap<string, long>` per unit and AppHost maps its smart-enum at the boundary — a scalar debit is falsified by the multi-unit consumer); `BudgetCredit(HashMap<string, long> Credit)` the fenced vector increment — the compensation inverse a workflow that must RETURN budget rides, the same one-statement vector shape with no sufficiency gate, and the SEED for a unit no row yet holds because its `ON CONFLICT` establishes the absent row; the vector's polarity is REMAINING balance per unit and never cumulative spend, so the ceiling never crosses this seam and a consumer reporting spend derives it as `ceiling - remaining`; `StepStateCas(WorkflowKey, StepKey, StepState Expected, StepState Next)` the orchestration step transition; `StepStateInFlight(WorkflowKey)` READ (the `CrashResume` scan — every non-terminal step of a workflow); `StepStateLoad(WorkflowKey, StepKey)` READ; `SignalPut(WorkflowKey Instance, SignalKey Channel, JsonElement Payload)` the fenced durable-signal upsert the AppHost `Runtime/orchestration#STEP_STATE_SEAM` `StepStateSeam.SignalPut` decodes — one `signal` row per `(workflow, channel)` under the same tenant fence, so a waiting `Signal` step's wake-or-fault decision survives crash, resume, and peer handoff; `SignalLoad(WorkflowKey Instance, SignalKey Channel)` READ (the `StepStateSeam.SignalOf` leg — the loaded row's `Payload` slot carries the channel JSON); `LeaseAcquire(LeaseKey, HolderId, Duration Ttl)` MINTS the generation monotonically (`generation + 1` via PG row-CAS `RETURNING generation` — the mint side that makes the token VALIDATED); `LeaseRenew(LeaseKey, LeaseToken, Duration Ttl)` and `LeaseRelease(LeaseKey, LeaseToken)` re-validate the held token; `ExpiredScan` READ (orphan-reclaim — every lease whose deadline trails `frame.Now()`); `MembershipUpsert(MembershipKey, MemberId, Duration Ttl)` the lease-expiring membership row (`MembershipView.Serving`, `Rasm.AppHost/Wire/coordination.md`, is the in-process consumer); `MembershipRelease(MembershipKey, MemberId)` the explicit fenced departure — a clean shutdown removes its row NOW instead of waiting out the TTL lapse, the AppHost `MembershipView` `Departed` transition's durable half; `MembershipScan(MembershipKey)` READ; `OutboxAdvance(SinkKey Sink, long Through)` the `#OUTBOX_CURSOR` case; `OutboxPending(SinkKey Sink, long After, int Take)` READ (the relay's bounded drain window off the committed op-log) and `OutboxPark(SinkKey Sink, long Sequence, int Attempt, string Status)` the fenced head-of-line failure written onto the sink's OWN cursor row; `LeaseGuard(LeaseKey Lease, LeaseToken Token)` READ (advisory detection a holder reads before spending work, never a gate); `BudgetLoad` and `BudgetToken` the two nullary ledger READs whose tenant rides the frame. `CoordinationFault` closes over the seven deterministic refusals and the three provider classes `CoordinationFault.Lift` folds — `Contended(SqlState, Detail)` and `Unreachable(Detail)` carrying `IsTransient` true, `Unmapped(SqlState, Detail)` false — so every leg lands a banded case and no rail yields a bare `Error`.
- Entry: `public static IO<Fin<Seq<CoordinationReceipt>>> Run(IDocumentSession session, ReceiptSinkPort sink, Seq<CoordinationOp> ops, Option<LeaseToken> held, ProjectionContext frame, CancellationToken cancellationToken)` is the ONE rail at every arity — a port passes one op, a composed unit passes several, and the trailing frame and token parameters are why arity rides `Seq` rather than a `params` tail. One entry makes the transaction boundary and the acquisition set the SAME value; a second single-op entry beside it leaves the unsafe composition reachable, since two calls on one session are two transactions no ordering law reaches across. `Bracket` force-opens the Marten session transaction, then composes ONE direct `NpgsqlBatch` on the session's live connection: `SELECT pg_advisory_xact_lock(hashtext(@tenant || ':' || @key))` per DISTINCT `LockScope` every row names, sorted by `(Rank.Depth, Scope)` and tenant-prefixed so one tenant's hot key never stalls a sibling's; `SAVEPOINT rasm_coord`; then per op in CALLER order the guarded `UPDATE … WHERE tenant = @tenant AND fence <= @token AND <case predicate> … RETURNING` from `CaseSql.Guarded`, the tenant-guarded current-truth `SELECT` from `CaseSql.Truth`, and the optional `pg_notify` wake. `QueueSqlCommand` cannot carry this batch because it defers to `SaveChangesAsync` and returns no rows. Read ops name no `LockScope` and carry no guarded statement, so reads and writes ride one leg. `Verified` is the truth-only replay the relational retry owner passes as `verifySucceeded` — same ops, same `CaseSql`, same `Verdict`, no lock and no guarded statement. `SaveChangesAsync(cancellationToken)` commits WITH any same-session domain events, so a step transition and the event it consequences are one transaction; a refusal `ROLLBACK TO SAVEPOINT rasm_coord` first, so the batch commits nothing of its own and the caller's unit of work stays the caller's to decide.
- Auto: the fencing law is structural — a guarded row carries the highest lease generation it has observed (`fence`), the write predicate `fence <= @token` rejects a token older than that watermark and the write stamps `fence = @token`, so a paused holder resuming with a superseded token is `LeaseFenced(stale, current)` read off the zero-row CAS and the batch's trailing current-truth `SELECT` (one round trip, never a follow-up read), never a silent overwrite; `LeaseAcquire` takes the advisory lock on the lease key, then `UPDATE lease SET generation = generation + 1, holder = @holder, until = @until WHERE key = @key AND (holder = @holder OR until < @now) RETURNING generation` — an unexpired foreign hold returns zero rows and rails `LeaseExpired`-inverse refusal as `LeaseFenced`, an expired hold is reclaimed in the same statement; the Budget debit is ONE `UPDATE … FROM unnest(@units, @amounts) … WHERE b.balance >= r.amount AND b.fence <= @token RETURNING unit, balance` whose guard PostgreSQL RE-EVALUATES against the concurrent writer's committed row version once the block clears, so the decrement cannot overdraw and takes no lock of any kind; the units array binds SORTED and its keys are unique by `HashMap` construction, so every caller walks the ledger's row locks in one order and a repeated unit — a hard `ON CONFLICT` error on one statement family and a silently dropped amount on the other — is unrepresentable; that re-check is per-ROW atomic and nothing further, so a unit failing it leaves the result set while its siblings COMMIT, and all-or-nothing is the rail's own count gate — applied rows short of requested units rolls back to the savepoint and rails `BudgetExhausted(unit, requested, available)` off the trailing truth `SELECT`, whose row for the refusing unit was never written and reads true either side of the undo (an absent ledger row is a structural zero balance, the domain's own reading of an unheld unit, never an unmeasured one); a snapshot-computed whole-vector sufficiency predicate is the DELETED form; `BudgetCredit` is that shape with the sufficiency term dropped and the sign flipped, establishing an absent unit through `INSERT … ON CONFLICT (tenant, unit) DO UPDATE SET balance = budget_ledger.balance + excluded.balance WHERE budget_ledger.fence <= @token` — the one construct guaranteeing insert-or-update across the absent/present split — so debit and credit stay one statement family, never a sibling rail; `SignalPut` is a fenced `(workflow, channel)` upsert — `INSERT … ON CONFLICT (tenant, workflow, channel) DO UPDATE SET payload = excluded.payload, fence = @token WHERE signal.fence <= @token RETURNING` — so a paused holder's stale re-signal is the typed `LeaseFenced` refusal, never a silent payload overwrite, and `SignalLoad` reads the row's `payload` back through the canonical row's `Payload` slot; `MembershipRelease` is the fenced row delete whose `RETURNING` proves the departure (zero rows on an already-lapsed member is the benign `MembershipLapsed` the caller treats as done); every op ends with a trailing tenant-guarded current-truth `SELECT`, so a missed guarded `UPDATE` still returns the row's current generation/state and every typed refusal (`LeaseFenced` current, `CasConflict` found) populates from the ONE round trip; every READ carries `tenant = @tenant` structurally (the same guard the writes hold); the receipts project PER-OP with zero follow-up reads — `BudgetDebit` returns the POST-debit balance vector the metering consumer needs, a CAS/lease/membership write returns its committed row, a READ returns its loaded rows.
- Receipt: a debit rides `store.coordination.debit` and a credit `store.coordination.credit`, both carrying the post-op balance vector; a step CAS rides `store.coordination.step`; a signal upsert rides `store.coordination.signal`; a lease verb rides `store.coordination.lease` carrying the generation; a membership upsert or release rides `store.coordination.member`; a READ rides `store.coordination.read` carrying the row count; the cursor advance rides `store.coordination.outbox` (`#OUTBOX_CURSOR`); every typed refusal rides `store.coordination.fault` as the `Coordinate.Refusal` projection carrying the union's `Category` and its retriability bit. Emission is `Run`'s OWN fold — `SlotOf` resolves each op's verb slot off the op discriminant (the two budget verbs share one receipt shape, so only the op tells a debit from a credit) and the fold sends through the injected `ReceiptSinkPort`; the rejected form seats emission at each of the four port adapters, where four holders of one failed `Fin` re-construct one fact outside the owner that minted it and nine registered slots stand without a producer.
- Packages: Marten (`IDocumentSession.SaveChangesAsync`/transaction control — the fenced batch rides the session's transacted connection; `QueueSqlCommand` only for no-RETURNING side-writes such as the `pg_notify` wake), Npgsql (`NpgsqlBatch` — `pg_advisory_xact_lock` + guarded `UPDATE … RETURNING` + current-truth `SELECT` in one round trip, `pg_notify`; `PostgresException.IsTransient`/`SqlState`/`MessageText` and `NpgsqlException.IsTransient` — the provider's own retriable classification the `Lift` fold reads instead of a re-spelled SQLSTATE roster), Rasm (`IValidityEvidence`/`ValidityClaim`), LanguageExt.Core (`IO`/`Fin`/`HashMap`/`Seq`), NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new coordination concern is one `CoordinationOp` case, one arm in the generated total `Switch`, and one `CaseSql` row carrying its predicate and its `LockScope` as data; a new lock family is one `LockRank` row seated at its depth in the containment ladder, and every acquisition orders on it with zero call-site edits; a new budget unit is one ledger row (the vector statement already spans N units); a new step lifecycle state is one `StepState` row; a new boundary cause is one `CoordinationFault` case carrying its own retriability bit, with zero `Lift` edits where the provider already classifies it; a new write shape is one `CoordinationReceipt` case while every READ answers `Loaded`; zero new surface — a second lease store, a distributed-lock sidecar, a per-port service family, a message-envelope outbox table, a scalar-debit sibling beside the vector, a single-op entry beside the sequence one, a per-case advisory lock minted outside the rank, a re-spelled SQLSTATE retry roster beside the provider's own `IsTransient`, a caller-side message parse standing in for the discriminant, or a read surface per consumer is the deleted form because the op family discriminates by value shape, the fold and its ordering law are owned once, and the four AppHost ports decode ONE op-union.
- Boundary: the four PORT rows are AppHost→Persistence READs/decodes (correct HOST-BOUNDARY→APP-PLATFORM direction) — `Agent/capability` debits and credits the Budget vector, `Runtime/orchestration` drives `StepStateCas`+`StepStateInFlight`+`SignalPut`/`SignalLoad` (`CrashResume` reads the in-flight scan; the `StepStateSeam.SignalPut`/`SignalOf` delegates decode the signal cases), `Wire/outbox` rides the same-transaction outbox spine, `Wire/coordination` drives CAS+lease+membership and `MembershipView.Serving` folds the membership rows in-process — no AppHost type crosses down and no Persistence signature names `ClockPolicy` or `Principal` ([A.1] — the kernel `CorrelationId`/`TenantContext` pair is S0 vocabulary this package composes directly off the frame); the fenced-CAS is strictly stronger than any lease library because the token is VALIDATED at every guarded write, not merely held — `DistributedLock.Postgres` (no fencing token) and `WolverineFx` (message-envelope outbox table beside the stream-IS-outbox law) stay the recorded rejections; the advisory lock is the `_xact_` family (auto-released at transaction end AND at rollback — a session lock survives its own transaction's rollback and requires explicit unlock, the leak form); `LockRank` is the CONTAINMENT ladder the AppHost ports compose along — a node's membership encloses the leases it may hold, a lease fences the work beneath it, a step is that work's unit, a signal is a channel detail inside one instance, and the cursor advance is the terminal drain position nothing nests inside — so acquiring in rank order is a discipline every caller shares by construction rather than by convention, and the budget seats NO rank because its grain is row-level inside its own statement, ordered by the sorted unit array; row-level locking cannot serve that vector at all, since a unit's ledger row may be absent and `FOR UPDATE` over the requested-vector `LEFT JOIN` is refused at PLAN time (`0A000`, the nullable side of an outer join) while a lock on an absent row is a lock on nothing — the engine's own conditional-`UPDATE` re-check is what replaces it, and it is strictly stronger than the whole-ledger advisory key it deletes because it neither serializes unrelated units nor depends on a lock domain the server does not enforce; `hashtext` is a 32-bit digest, so two distinct keys can share one advisory slot and serialize needlessly — a throughput cost the rank ordering never turns into a correctness one; deadline comparisons read `frame.Now()` (the injected clock value), never a wall-clock call; a failed `OutboxAdvance` cursor-CAS is `CoordinationFault.OutboxDrain` — the coordination-side write fault, kept inside this fenced store's rail, NEVER a `Version/egress` `EgressFault` delivery fault; this tier CLASSIFIES retriability and executes none, publishing `CoordinationFault.IsTransient` for the executing rail exactly as the object plane publishes `RemoteStoreFault.IsTransient`, and the discriminant is what makes the classification legible to a caller whose refusal arrives as a result rather than a throw — a bare `Error` on the rail leaves the whole retriable class unreadable to every predicate, the deleted form `Lift` closes; the executing rail is the STORE EXECUTION STRATEGY (`docs/stacks/csharp/domain/resilience.md` `[04]-[LAYER_SPLIT]` row `[01]` — this callee owns transactional semantics, so no hop pipeline may bracket it, since a pipeline there replays from the wrong boundary), seated at the relational owner `Element/identity#IDENTITY_RAIL` holds under `StoreProfile.RetriesInStrategy`, ABOVE `Run` because `Lift` converts a throw to a value and a strategy beneath it has nothing left to classify; every guarded statement here is a conditional write, so that strategy admits this rail only under `verifySucceeded` and `Verified` is the probe it passes; the discriminant then drives a WIDER-scope caller re-offer, re-planning the step rather than re-executing one statement.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using System.Text.Json;
using Npgsql;
using Rasm.Domain;                                // CorrelationId/TenantContext — the S0 causal pair the frame seats
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected

namespace Rasm.Persistence.Store;

// --- [TYPES] ----------------------------------------------------------------------------

// `LeaseAcquire` mints this Kleppmann fencing token as a monotone lease generation, and every guarded write's
// `fence <= @token` predicate VALIDATES it — a token proves currency, never possession.
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

// `StepState` closes the orchestration step lifecycle: `Terminal` gates re-entry, so a CAS into a terminal state
// is final and the `StepStateInFlight` scan returns every non-terminal step, resuming exactly the open work under
// `CrashResume`.
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

// Advisory-lock containment ladder. `Depth` is the domain rank every acquisition sorts ascending on, so opposed
// callers walk one order and the deadlock the per-case key otherwise builds in cannot form. The row's KEY is the
// lock-key PREFIX itself, so a scope and its rank are one value and cannot drift. Budget seats no row: its grain
// is the ledger's own row locks inside one conditional `UPDATE`, ordered by the sorted unit array.
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

// One advisory key as DATA: `Key` renders the text the batch hashes, `(Depth, Scope)` orders the acquisition.
public readonly record struct LockScope(LockRank Rank, string Scope) {
    public string Key => $"{Rank.Key}:{Scope}";
}

// `CoordinationOp` closes the coordination family: guarded writes through ONE fenced-CAS fold, tenant-guarded
// READs through ONE truth projection, one cursor advance (#OUTBOX_CURSOR). Every new concern lands as a case
// here, never a per-port service; `SignalPut`/`SignalLoad` are the durable-signal cases the AppHost
// `Runtime/orchestration#STEP_STATE_SEAM` `StepStateSeam.SignalPut`/`SignalOf` delegates decode.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinationOp {
    private CoordinationOp() { }

    // POLARITY IS REMAINING BALANCE per unit, never cumulative spend. Sufficiency is therefore `balance >= amount`,
    // decided INSIDE the atomic vector CAS with no ceiling crossing at all — a spend ledger would have to re-supply and
    // re-compare its ceiling at the caller, which is precisely the read-then-write window this fenced write exists to
    // close. So a debit is a ONE-field case: a ceiling argument beside the vector maps to nothing here and no statement
    // or bind reads one. Spend is a CONSUMER derivation (`ceiling - remaining`), and a ceiling's only write-side role is
    // SEEDING an opening balance — which `BudgetCredit` already is, since its `ON CONFLICT` establishes an absent row
    // and a credit against a never-seeded unit and a seed are ONE write under this reading, so no fifth case exists.
    // Declaring polarity here is what stops the next reader re-forking it (`docs/laws/scars.md`
    // `[DISCARDED_DISCRIMINANT]` — a value crossing a seam whose polarity neither end declares).
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
    // ADVISORY DETECTION, never a gate: it loads the lease row's current generation so a holder can notice a token it
    // no longer owns BEFORE spending work, and it takes NO advisory scope for the same reason — locking the lease key
    // to read it would serialize against the very holder the read exists to observe. Authoritative rejection stays
    // each guarded write's own `fence <= @token` predicate, because a guard evaluated apart from the write it protects
    // passes on a value another writer moved (`docs/laws/scars.md` `[SNAPSHOT_FROZEN_GUARD]`); reading this case as a
    // gate re-mints exactly that scar.
    public sealed record LeaseGuard(LeaseKey Lease, LeaseToken Token) : CoordinationOp;
    // Budget READs, both nullary: tenant rides the injected frame ([A.1]), so a `TenantId` field here would re-mint the
    // ambient source the RLS bind already fixes, and an AppHost arrow taking a tenant argument mints the frame with it
    // rather than a case column. `BudgetLoad` returns the per-unit balance rows; `BudgetToken` returns the tenant's
    // fence WATERMARK — the highest generation its ledger has observed — the one read both the budget port and the
    // outbox relay need, so ONE case serves both and a per-consumer twin is the deleted form.
    public sealed record BudgetLoad : CoordinationOp;
    public sealed record BudgetToken : CoordinationOp;
    public sealed record MembershipUpsert(MembershipKey Group, MemberId Member, Duration Ttl) : CoordinationOp;
    public sealed record MembershipRelease(MembershipKey Group, MemberId Member) : CoordinationOp;
    public sealed record MembershipScan(MembershipKey Group) : CoordinationOp;
    public sealed record OutboxAdvance(SinkKey Sink, long Through) : CoordinationOp;
    public sealed record OutboxPending(SinkKey Sink, long After, int Take) : CoordinationOp;
    public sealed record OutboxPark(SinkKey Sink, long Sequence, int Attempt, string Status) : CoordinationOp;
}

// --- [MODELS] ---------------------------------------------------------------------------

// `CoordRow` projects the ONE loaded row every statement returns — key, state/holder, generation, deadline,
// payload — so every guarded `RETURNING` and every truth/read `SELECT` projects one canonical shape and the
// fenced rail reads one row form structurally; `Payload` fills only on the signal rows (`NULL` everywhere else),
// and a per-read row record is the deleted form.
public readonly record struct CoordRow(string Key, string State, long Fence, Option<long> Value, Instant Until, Option<JsonElement> Payload);

// `CoordinationFact` rides `store.coordination.fault` with both columns projected off the union: `Category`
// bounds the fault axis a board partitions on and `Transient` separates contention a caller re-drives from a
// deterministic refusal it must not. No instant — the receipt's message envelope stamps the HLC.
public readonly record struct CoordinationFact(string Category, bool Transient);

// Per-sink durable drain cursor (#OUTBOX_CURSOR): one row per sink, advanced only by the fenced `OutboxAdvance` CAS
// — distinct from the per-origin `SyncCursor` (Version/ledger#CHANGEFEED). `Parked`/`Attempt`/`Status` carry the
// sink's head-of-line failure on this SAME row rather than a second table: the committed op-log IS the outbox
// (`Rasm.AppHost/RULINGS.md` `[02]-[SHAPE]`), so per-event delivery state has no message-envelope row to occupy, and the
// attempt count is PER SINK — writing it onto the shared event row would let one sink's retries overwrite another's.
// Forward-only draining means at most one parked head per sink, so one slot is the whole shape.
public sealed record OutboxCursor(SinkKey Sink, long Sequence, long Parked, int Attempt, string Status) {
    public static OutboxCursor Genesis(SinkKey sink) => new(sink, 0L, 0L, 0, "clear");
}

// Per-op typed evidence on the kernel validity floor: `IsValid` is ONE `ValidityClaim.All` fold over the
// case's own claims — the post-debit vector is non-negative per unit, a committed write carries a positive
// generation, a read's row count is conserved — never a hand-rolled `&&` chain ([C]).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinationReceipt : IValidityEvidence {
    private CoordinationReceipt() { }

    // `Debited` is the balance-vector receipt BOTH budget verbs project — a debit carries the post-debit
    // vector, a credit the post-credit vector; the op case discriminates the verb, never a sibling receipt.
    // `Fence` is the generation the applied rows stamped, threaded UPWARD beside the vector so a metering consumer
    // reads the post-op balances and the tenant watermark off ONE unit — the scalar-only return that dropped both is
    // what forced a second load round trip to exist at all.
    public sealed record Debited(HashMap<string, long> Balances, long Fence, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Stepped(WorkflowKey Workflow, StepKey Step, StepState Committed, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Signaled(WorkflowKey Instance, SignalKey Channel, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Leased(LeaseKey Lease, LeaseToken Token, Instant Until, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Member(MembershipKey Group, MemberId Id, Instant Until, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Advanced(SinkKey Sink, long Through, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    // Parking is a WRITE and gets its own evidence: an advance and a park move different columns of one row, so
    // reporting a park as an `Advanced` would publish a drain position the cursor never reached. Every READ still
    // answers `Loaded`, so the receipt family grows per write shape and never per verb.
    public sealed record Parked(SinkKey Sink, long Sequence, int Attempt, string Status, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;
    public sealed record Loaded(Seq<CoordRow> Rows, Instant At, CorrelationId Correlation, Duration Elapsed) : CoordinationReceipt;

    public bool IsValid => Switch(
        debited:  static c => ValidityClaim.All(ValidityClaim.Of(c.Balances.Values.ForAll(static b => b >= 0L)), ValidityClaim.Nonnegative(c.Fence)),
        stepped:  static c => ValidityClaim.All(ValidityClaim.Of(!string.IsNullOrEmpty(c.Step.Value))),
        signaled: static c => ValidityClaim.All(ValidityClaim.Of(!string.IsNullOrEmpty(c.Channel.Value))),
        leased:   static c => ValidityClaim.All(ValidityClaim.Of(c.Token.Value > 0L), ValidityClaim.Of(c.Until > c.At)),
        member:   static c => ValidityClaim.All(ValidityClaim.Of(c.Until >= c.At)),
        advanced: static c => ValidityClaim.All(ValidityClaim.Nonnegative(c.Through)),
        parked:   static c => ValidityClaim.All(ValidityClaim.Nonnegative(c.Sequence), ValidityClaim.Nonnegative(c.Attempt)),
        loaded:   static c => ValidityClaim.All(ValidityClaim.CountAtLeast(c.Rows.Count, 0)));
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Band 8430 (Element/graph#FAULT_TABLES registry row `Coordination`): a closed [Union] over the KERNEL
// `Rasm.Domain.Expected` — Code derives `FaultBand.Coordination + n` through the registry pointer, so a
// duplicate decade integer fails at type initialization, never prose. `OutboxDrain` is the failed cursor-CAS
// — the coordination-side write fault the V3 pump depends on, never an EgressFault delivery fault.
// `IsTransient` is the per-case retriability discriminant the sibling bands already publish, and it is
// load-bearing HERE above every other store rail: a fenced store advertising one advisory lock per key and
// committing its guarded CAS beside the session's domain events manufactures serialization failures and
// deadlocks under exactly the contention it exists to arbitrate, and those SQLSTATEs (`40001`, `40P01`,
// `55P03`, the `53xxx` resource family, the `08xxx` connection family, the `57Pxx` shutdown family) succeed on
// a re-drive of the identical op. Because this rail STATES its refusal as a `Fin` result rather than raising,
// an executor whose predicate reads exceptions alone observes nothing here — so the retriable class has to
// arrive as a discriminant a result-shaped predicate can read, which is what `Contended`/`Unreachable` mint
// and what a bare `Error` on the rail withheld. Every deterministic refusal stays false, so a caller can never
// re-drive a genuinely fenced token or an exhausted budget into a spin.
[Union]
public abstract partial record CoordinationFault : Expected, IValidationError<CoordinationFault> {
    private CoordinationFault() : base() { }
    public abstract bool IsTransient { get; }

    public sealed record LeaseFenced(LeaseToken Stale, long Current) : CoordinationFault { public override bool IsTransient => false; }
    public sealed record CasConflict(WorkflowKey Workflow, StepKey Step, StepState Expected, StepState Found) : CoordinationFault { public override bool IsTransient => false; }
    public sealed record BudgetExhausted(string Unit, long Requested, long Available) : CoordinationFault { public override bool IsTransient => false; }
    public sealed record LeaseExpired(LeaseKey Lease, HolderId Holder) : CoordinationFault { public override bool IsTransient => false; }
    public sealed record MembershipLapsed(MembershipKey Group, MemberId Member) : CoordinationFault { public override bool IsTransient => false; }
    public sealed record OutboxDrain(SinkKey Sink, long Through) : CoordinationFault { public override bool IsTransient => false; }
    public sealed record Refused(string Detail) : CoordinationFault { public override bool IsTransient => false; }
    // `Contended` bands the server-side retriable class and carries the SQLSTATE the server itself reported
    // rather than a reason string, so a board partitions contention by code and the retry decision reads one bit.
    public sealed record Contended(string SqlState, string Detail) : CoordinationFault { public override bool IsTransient => true; }
    // Transport-level loss with no SQLSTATE to carry — the connection dropped before the batch settled, so the
    // op's own outcome is unknown and a re-drive is safe exactly because every guarded statement is a fenced CAS.
    public sealed record Unreachable(string Detail) : CoordinationFault { public override bool IsTransient => true; }
    // `Unmapped` bands a deterministic server refusal outside this band's own vocabulary — a constraint, a
    // missing relation, a privilege denial — so telemetry reads it as coordination work and no caller re-drives it.
    public sealed record Unmapped(string SqlState, string Detail) : CoordinationFault { public override bool IsTransient => false; }

    public override int Code => FaultBand.Coordination + Switch(
        leaseFenced:      static _ => 1,
        casConflict:      static _ => 2,
        budgetExhausted:  static _ => 3,
        leaseExpired:     static _ => 4,
        membershipLapsed: static _ => 5,
        outboxDrain:      static _ => 6,
        refused:          static _ => 7,
        contended:        static _ => 8,
        unreachable:      static _ => 9,
        unmapped:         static _ => 10);

    public override string Message => Switch(
        leaseFenced:      static c => $"<lease-fenced:{c.Stale.Value}<{c.Current}>",
        casConflict:      static c => $"<cas-conflict:{c.Workflow.Value}/{c.Step.Value}:{c.Expected.Key}!={c.Found.Key}>",
        budgetExhausted:  static c => $"<budget-exhausted:{c.Unit}:{c.Requested}>{c.Available}>",
        leaseExpired:     static c => $"<lease-expired:{c.Lease.Value}:{c.Holder.Value}>",
        membershipLapsed: static c => $"<membership-lapsed:{c.Group.Value}:{c.Member.Value}>",
        outboxDrain:      static c => $"<outbox-drain:{c.Sink.Value}@{c.Through}>",
        refused:          static c => $"<coordination-refused:{c.Detail}>",
        contended:        static c => $"<coordination-contended:{c.SqlState}>:{c.Detail}",
        unreachable:      static c => $"<coordination-unreachable:{c.Detail}>",
        unmapped:         static c => $"<sqlstate:{c.SqlState}>:{c.Detail}");

    public override string Category => Switch(
        leaseFenced:      static _ => "Fencing",
        casConflict:      static _ => "Cas",
        budgetExhausted:  static _ => "Budget",
        leaseExpired:     static _ => "Lease",
        membershipLapsed: static _ => "Membership",
        outboxDrain:      static _ => "Outbox",
        refused:          static _ => "Refused",
        contended:        static _ => "Contention",
        unreachable:      static _ => "Unreachable",
        unmapped:         static _ => "Unmapped");

    public static CoordinationFault Create(string message) => new Refused(message);

    // `Lift` is the ONE provider-throw conversion, so no rail leg lands a bare `Error` outside the band. Retriability
    // reads the provider's OWN classification (`PostgresException.IsTransient` folds the `40001`/`40P01`
    // serialization and deadlock pair, the `55xxx` lock family, the `53xxx` resource family, the `08xxx`
    // connection family, and the `57Pxx` shutdown family) rather than a re-spelled SQLSTATE roster this page
    // would then have to keep in step with the driver. A non-transient `PostgresException` keeps its SQLSTATE
    // as `Unmapped`; `NpgsqlException` beneath the server layer is `Unreachable` on its own transient bit and
    // `Refused` otherwise; an already-banded fault passes through untouched.
    public static CoordinationFault Lift(Error error) => error switch {
        CoordinationFault fault => fault,
        { Exception.Case: PostgresException pg } => pg.IsTransient
            ? new Contended(pg.SqlState, pg.MessageText)
            : new Unmapped(pg.SqlState, pg.MessageText),
        { Exception.Case: NpgsqlException { IsTransient: true } transport } => new Unreachable(transport.Message),
        { Exception.Case: { } exception } => new Refused($"{exception.GetType().Name}:{exception.Message}"),
        _ => new Refused(error.Message),
    };
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Coordinate {
    const string LockSql = "SELECT pg_advisory_xact_lock(hashtext(@tenant || ':' || @key))";
    const string WakeSql = "SELECT pg_notify(@channel, @sink)";
    const string Mark = "SAVEPOINT rasm_coord";
    const string Undo = "ROLLBACK TO SAVEPOINT rasm_coord";

    // `FaultSlot` seats the refusal stream beside the eight verb slots: every typed refusal this rail mints — a
    // fenced token, a CAS conflict, an exhausted budget, a contended re-drive — reached no evidence surface, so a
    // split-brain storm and a quiet cluster read identically on every board. `Refusal` projects the union's OWN
    // `Category` and its OWN retriability bit, so the vocabulary stays the union's and no second roster drifts.
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

    public static CoordinationFact Refusal(CoordinationFault fault) => new(fault.Category, fault.IsTransient);

    // `Run` is the ONE rail at every arity. `ops` carries one op from a port and several from a composed unit; trailing
    // frame and token parameters are why arity rides `Seq` and not a `params` tail. `held` is the caller's fencing
    // token — required by every fenced case, `None` legal only on READs and the initial `LeaseAcquire`, and the
    // gate reads the whole set so one tokenless write refuses the unit before a single statement dispatches.
    public static IO<Fin<Seq<CoordinationReceipt>>> Run(IDocumentSession session, ReceiptSinkPort sink, Seq<CoordinationOp> ops, Option<LeaseToken> held, ProjectionContext frame, CancellationToken cancellationToken) =>
        from mark in IO.lift(frame.Mark)
        from sql in IO.lift(() => ops.Map(op => CaseSql.For(op, frame.Now())))
        from outcome in sql.Exists(static row => row.RequiresToken) && held.IsNone
            ? IO.pure(Fin<Seq<CoordinationReceipt>>.Fail(new CoordinationFault.Refused("<missing-fence-token>")))
            : Bracket(session, ops, sql, held, frame, mark, cancellationToken)
        from _ in Emit(sink, ops, outcome, frame)
        select outcome;

    // Verb slot resolves off the OP, never the receipt: `Debited` is the receipt shape BOTH budget verbs project,
    // so only the op discriminant separates a debit fact from a credit one.
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

    // ONE emitter, at the owner that mints the outcome: a committed unit sends each receipt under its own op's
    // verb slot, a refusal sends one `CoordinationFact` under `FaultSlot`. Every published field is one the fold
    // measured — the union's own `Category` and retriability bit — so no slot carries a value this call site
    // cannot read, and no port adapter re-constructs a fact outside the rail that produced it.
    static IO<Unit> Emit(ReceiptSinkPort sink, Seq<CoordinationOp> ops, Fin<Seq<CoordinationReceipt>> outcome, ProjectionContext frame) =>
        outcome.Match(
            Succ: receipts => ops.Zip(receipts).TraverseM(pair => Send(sink, frame, SlotOf(pair.Item1), pair.Item2)).As().Map(static _ => unit),
            Fail: error => Send(sink, frame, FaultSlot, Refusal(CoordinationFault.Lift(error))));

    static IO<Unit> Send<TFact>(ReceiptSinkPort sink, ProjectionContext frame, StoreSlot slot, TFact fact) =>
        sink.Send(frame.Correlation, frame.Tenant, TelemetrySource.Persistence.Key, slot,
            JsonSerializer.SerializeToElement(fact, ElementJson.Options)).Map(static _ => unit);

    // Every case rides this ONE fold. Marten's session transaction force-OPENS first (Marten otherwise defers it
    // to `SaveChangesAsync`, leaving the batch outside the transaction the lock law requires), then the rail
    // composes ONE NpgsqlBatch on the session's live connection: every DISTINCT `LockScope` the rows name, sorted
    // `(Depth, Scope)` so acquisition order is the rank's and never the caller's; the savepoint; then each op's
    // guarded statement, its current-truth `SELECT`, and its wake, in CALLER order. A read names no scope and no
    // guarded statement, so reads and writes ride this one leg. Every
    // row statement projects `(key, state, fence, value, until, payload)`, `Slice` carves each op's own result
    // sets back out, and a refusal rolls back to the savepoint BEFORE the commit, so a partially-applied vector
    // leaves nothing behind and the `_xact_` locks release either way. A provider throw converts through
    // `CoordinationFault.Lift` HERE, once, so the serialization failures and deadlocks contention still raises
    // reach the caller as a retriable BAND case rather than a bare `Error` no result-shaped predicate classifies.
    // HERE is also this rail's OUTERMOST edge: a strategy composed beneath it would receive a completed `Fin` and have
    // nothing left to classify, so any re-drive owner seats ABOVE `Run` and pairs with `Verified`.
    static IO<Fin<Seq<CoordinationReceipt>>> Bracket(IDocumentSession session, Seq<CoordinationOp> ops, Seq<CaseSql> sql, Option<LeaseToken> held, ProjectionContext frame, long mark, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => {
            await session.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
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
            Seq<Seq<CoordRow>> sets = await Sets(batch, cancellationToken).ConfigureAwait(false);
            Fin<Seq<CoordinationReceipt>> outcome = Project(ops, sql, sets.Skip(locks.Count + 1), held, frame, mark);
            if (outcome.IsFail) { await Rollback(batch.Connection!, cancellationToken).ConfigureAwait(false); }
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return outcome;
        }) | @catch<IO, Fin<Seq<CoordinationReceipt>>>(static _ => true, e => IO.pure(Fin<Seq<CoordinationReceipt>>.Fail(CoordinationFault.Lift(e))));

    // `verifySucceeded` for this rail, and the reason `Lift` seats where it does. EF's execution strategy classifies
    // EXCEPTIONS while `CoordinationFault.Lift` turns a throw into a VALUE, so a strategy composed INSIDE `Run` is
    // handed a completed `Fin` and never sees a retryable exception — `Lift` therefore seats at that strategy's OUTER
    // edge and this fold composes no strategy of its own. Relational retry stays the owner
    // `Element/identity#IDENTITY_RAIL` holds under `StoreProfile.RetriesInStrategy`, and this entry is the probe that
    // owner passes as `verifySucceeded`: it re-runs each op's TRUTH statement alone — no locks, no guarded statement,
    // no savepoint, no wake — and folds the sets through the SAME `Verdict`, so a `Succ` proves an ambiguous commit
    // already landed and a `Fail` proves it did not. Re-driving blind double-applies: a fenced CAS still satisfies
    // `fence <= @token` once it has applied, `LeaseAcquire` mints a SECOND generation that invalidates the token its
    // first attempt returned, and `BudgetDebit` decrements twice. Every guarded statement here is a CONDITIONAL write,
    // so its guard must re-evaluate per attempt for exactly the reason `docs/laws/scars.md` `[SNAPSHOT_FROZEN_GUARD]`
    // states — a verdict frozen at the first attempt passes on a value another writer moved. Which is why the
    // discriminant drives a WIDER-scope caller re-offer (re-plan the step) and never a re-execute of one statement.
    public static IO<Fin<Seq<CoordinationReceipt>>> Verified(IDocumentSession session, Seq<CoordinationOp> ops, Option<LeaseToken> held, ProjectionContext frame, CancellationToken cancellationToken) =>
        from mark in IO.lift(frame.Mark)
        from sql in IO.lift(() => ops.Map(op => CaseSql.For(op, frame.Now())))
        from outcome in Truths(session, ops, sql, held, frame, mark, cancellationToken)
        select outcome;

    // Truth-only replay. Stripping `Guarded` and `Wake` off each row is what keeps `Slice` honest: every row then
    // contributes exactly ONE result set, matching a batch that dispatched exactly one statement per op, so the same
    // positional carve reads the probe's sets and the run's alike.
    static IO<Fin<Seq<CoordinationReceipt>>> Truths(IDocumentSession session, Seq<CoordinationOp> ops, Seq<CaseSql> sql, Option<LeaseToken> held, ProjectionContext frame, long mark, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => {
            await session.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using NpgsqlBatch batch = new((NpgsqlConnection)session.Connection!);
            Seq<CaseSql> probes = sql.Map(static row => row with { Guarded = None, Wake = None });
            probes.Iter(row => batch.BatchCommands.Add(Bound(row.Truth, row.Binds, held, frame)));
            Seq<Seq<CoordRow>> sets = await Sets(batch, cancellationToken).ConfigureAwait(false);
            return Project(ops, probes, sets, held, frame, mark);
        }) | @catch<IO, Fin<Seq<CoordinationReceipt>>>(static _ => true, e => IO.pure(Fin<Seq<CoordinationReceipt>>.Fail(CoordinationFault.Lift(e))));

    static async Task Rollback(NpgsqlConnection connection, CancellationToken cancellationToken) {
        await using NpgsqlCommand undo = new(Undo, connection);
        _ = await undo.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    // One bound command per statement: `@tenant` and `@token` ride every case (the structural fence), the
    // case's own binds append after — a lock skipped on the READ leg is the only shape difference.
    static NpgsqlBatchCommand Bound(string statement, Seq<(string Name, object Value)> binds, Option<LeaseToken> held, ProjectionContext frame) {
        NpgsqlBatchCommand command = new(statement) {
            Parameters = {
                // `Entry` is the kernel's one tenant text; the RLS predicate and the `hashtext` lock prefix
                // compare against exactly this spelling, so no call site re-formats the key scalar.
                new NpgsqlParameter<string>("tenant", frame.Tenant.Entry),
                new NpgsqlParameter<long>("token", held.Map(static t => t.Value).IfNone(0L)),
            },
        };
        foreach ((string name, object value) in binds) { command.Parameters.Add(new NpgsqlParameter(name, value)); }
        return command;
    }

    // Drain PER RESULT SET, keeping the batch's positional shape: a lock scalar and the savepoint yield sets with
    // no matching columns and drain empty, so the walk stays total and an op reads only its own statements' rows.
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

    // Per-op verdicts in CALLER order, short-circuiting on the first refusal — the savepoint undo is what makes
    // that short circuit honest, since every op ahead of it already wrote.
    static Fin<Seq<CoordinationReceipt>> Project(Seq<CoordinationOp> ops, Seq<CaseSql> sql, Seq<Seq<CoordRow>> sets, Option<LeaseToken> held, ProjectionContext frame, long mark) =>
        ops.Zip(Slice(sql, sets)).TraverseM(pair => Verdict(pair.Item1, pair.Item2, held, frame, mark)).As();

    // Positional carve: each row's declared `Sets` count takes its own window, so an absent guarded statement or
    // a wake command never shifts a sibling op's rows onto the wrong verdict.
    static Seq<Seq<CoordRow>> Slice(Seq<CaseSql> sql, Seq<Seq<CoordRow>> sets) =>
        sql.Fold((Cursor: 0, Slices: Seq<Seq<CoordRow>>()), (state, row) =>
            (state.Cursor + row.Sets, state.Slices.Add(sets.Skip(state.Cursor).Take(row.Sets).Bind(static rows => rows)))).Slices;

    // Per-case verdict: the row discriminator every op folds through, reading its refusal off the rows the
    // batch RETURNED: an `applied` row names what the guarded statement committed and the trailing current-truth
    // `SELECT` carries the row's generation and state where that statement missed, so no typed fault needs a
    // second round trip. Read cases project Loaded through this same total Switch.
    static Fin<CoordinationReceipt> Verdict(CoordinationOp op, Seq<CoordRow> rows, Option<LeaseToken> held, ProjectionContext frame, long mark) => op.Switch(
        budgetDebit: d => Settled(d.Debit, rows, held, frame, mark),
        budgetCredit: c => Settled(c.Credit, rows, held, frame, mark),
        signalPut: s => rows.Head.Match(
            Some: r  => r.State == "signaled"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Signaled(s.Instance, s.Channel, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.Refused($"<signal:{s.Instance.Value}/{s.Channel.Value}>"))),
        signalLoad: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        stepStateCas: c => rows.Head.Match(
            Some: r => r.State == c.Next.Key
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Stepped(c.Workflow, c.Step, c.Next, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : r.Fence > held.Map(static token => token.Value).IfNone(0L)
                    ? Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence))
                    : Fin<CoordinationReceipt>.Fail(new CoordinationFault.CasConflict(c.Workflow, c.Step, c.Expected, StepState.Get(r.State))),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.Refused($"<step-missing:{c.Workflow.Value}/{c.Step.Value}>"))),
        stepStateInFlight: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        stepStateLoad:     _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        leaseAcquire: a => rows.Head.Match(
            Some: r => r.State == "held"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Leased(a.Lease, LeaseToken.Create(r.Fence), r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(LeaseToken.Create(0L), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseExpired(a.Lease, a.Holder))),
        leaseRenew: n => Held(rows, n.Lease, n.Token, frame, mark),
        leaseRelease: n => Held(rows, n.Lease, n.Token, frame, mark),
        expiredScan: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        membershipUpsert: m => rows.Head.Match(
            Some: r  => r.State == "serving"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Member(m.Group, m.Member, r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.MembershipLapsed(m.Group, m.Member))),
        membershipRelease: m => rows.Head.Match(
            Some: r  => r.State == "departed"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Member(m.Group, m.Member, r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.MembershipLapsed(m.Group, m.Member))),
        membershipScan: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        outboxAdvance: a => rows.Head.Match(
            Some: r => r.Value.IfNone(0L) >= a.Through
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Advanced(a.Sink, r.Value.IfNone(0L), frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(a.Sink, a.Through)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(a.Sink, a.Through))),
        outboxPending: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        outboxPark: k => rows.Head.Match(
            Some: r => r.State == "parked"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Parked(k.Sink, k.Sequence, k.Attempt, k.Status, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.OutboxDrain(k.Sink, k.Sequence))),
        leaseGuard:  _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        budgetLoad:  _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))),
        budgetToken: _ => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Loaded(rows, frame.Now(), frame.Correlation, frame.Elapsed(mark))));

    // Both budget verbs fold through this shared verdict: an unapplied unit is the refusal, and its identity comes
    // from the requested vector rather than a count comparison, so the success arm is "every unit applied" stated
    // as the predicate it actually is. The refusing unit's truth row was never written, so it reads true either
    // side of the savepoint undo; a unit with NO truth row is the domain's structural zero balance — an unheld
    // unit — never an unmeasured one, and a higher fence on that row is the stale-token refusal instead.
    static Fin<CoordinationReceipt> Settled(HashMap<string, long> amounts, Seq<CoordRow> rows, Option<LeaseToken> held, ProjectionContext frame, long mark) {
        Seq<CoordRow> applied = rows.Filter(static row => row.State == "applied");
        return toSeq(amounts.Keys).Filter(unit => !applied.Exists(row => row.Key == unit)).Head.Match(
            Some: unit => rows.Find(row => row.State == "current" && row.Key == unit).Match(
                Some: truth => truth.Fence > held.Map(static token => token.Value).IfNone(0L)
                    ? Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(held.IfNone(LeaseToken.Create(0L)), truth.Fence))
                    : Fin<CoordinationReceipt>.Fail(new CoordinationFault.BudgetExhausted(unit, amounts.Find(unit).IfNone(0L), truth.Value.IfNone(0L))),
                None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.BudgetExhausted(unit, amounts.Find(unit).IfNone(0L), 0L))),
            None: () => Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Debited(
                toHashMap(applied.Map(static row => (row.Key, row.Value.IfNone(0L)))),
                applied.Head.Map(static row => row.Fence).IfNone(0L), frame.Now(), frame.Correlation, frame.Elapsed(mark))));
    }

    // `Held` folds the renew and release verdicts alike: an applied row carries the validated generation
    // ("held"/"released"), and a current-truth row with a higher fence rails the stale-token refusal carrying the
    // CURRENT generation.
    static Fin<CoordinationReceipt> Held(Seq<CoordRow> rows, LeaseKey lease, LeaseToken token, ProjectionContext frame, long mark) =>
        rows.Head.Match(
            Some: r => r.State is "held" or "released"
                ? Fin<CoordinationReceipt>.Succ(new CoordinationReceipt.Leased(lease, LeaseToken.Create(r.Fence), r.Until, frame.Now(), frame.Correlation, frame.Elapsed(mark)))
                : Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(token, r.Fence)),
            None: () => Fin<CoordinationReceipt>.Fail(new CoordinationFault.LeaseFenced(token, 0L)));
}

// `For` generates every statement and bind row from the op discriminant. `RequiresToken` makes a missing write fence
// refuse before SQL dispatch; reads and lease acquisition are the only false rows.
public readonly record struct CaseSql(Seq<LockScope> Locks, bool RequiresToken, Option<string> Guarded, string Truth, Option<string> Wake, Seq<(string Name, object Value)> Binds) {
    // Result sets this row contributes, in batch order — the count `Slice` walks back out: the guarded statement
    // where the case writes, the truth `SELECT` always, the wake where a case names a channel.
    public int Sets => (Guarded.IsSome ? 1 : 0) + 1 + (Wake.IsSome ? 1 : 0);

    public static CaseSql For(CoordinationOp op, Instant now) => op.Switch(
        budgetDebit: value => Budget(value.Debit, debit: true),
        budgetCredit: value => Budget(value.Credit, debit: false),
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
        leaseAcquire: value => Write(new LockScope(LockRank.Lease, value.Lease.Value),
            "INSERT INTO lease(tenant,key,holder,generation,until) VALUES(@tenant,@lease,@holder,1,@until) ON CONFLICT(tenant,key) DO UPDATE SET holder=excluded.holder,generation=lease.generation+1,until=excluded.until WHERE lease.holder=@holder OR lease.until<@now RETURNING key,'held',generation,NULL::bigint,until,NULL::jsonb",
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND key=@lease",
            false, ("lease", value.Lease.Value), ("holder", value.Holder.Value), ("until", now + value.Ttl), ("now", now)),
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
            "INSERT INTO outbox_cursor(tenant,sink,sequence,updated_at,fence) VALUES(@tenant,@sink,@through,@now,@token) ON CONFLICT(tenant,sink) DO UPDATE SET sequence=excluded.sequence,updated_at=excluded.updated_at,fence=excluded.fence WHERE outbox_cursor.fence<=@token AND outbox_cursor.sequence<@through RETURNING sink,'advanced',fence,sequence,updated_at,NULL::jsonb",
            "SELECT sink,'current',fence,sequence,updated_at,NULL::jsonb FROM outbox_cursor WHERE tenant=@tenant AND sink=@sink",
            "rasm_outbox",
            ("sink", value.Sink.Value), ("through", value.Through), ("now", now)),
        // Pending rows come off the COMMITTED op-log the outbox spine already owns — no message-envelope table, so the read is
        // a bounded window past the sink's own cursor and `Take` is caller-supplied because only the relay knows the
        // batch it can deliver. `After` binds separately from the cursor row so a relay may re-drain behind its own
        // position under at-least-once without a second case.
        outboxPending: value => Read(
            "SELECT id::text,'pending',fence,sequence,committed_at,envelope FROM op_log WHERE tenant=@tenant AND sequence>@after ORDER BY sequence LIMIT @take",
            ("after", value.After), ("take", value.Take)),
        // Parking writes the head-of-line failure onto the sink's OWN cursor row under the same fence every advance
        // takes, so delivery state stays per sink and the committed event row keeps one owner.
        outboxPark: value => Write(new LockScope(LockRank.Outbox, value.Sink.Value),
            "INSERT INTO outbox_cursor(tenant,sink,sequence,parked,attempt,status,updated_at,fence) VALUES(@tenant,@sink,0,@sequence,@attempt,@status,@now,@token) ON CONFLICT(tenant,sink) DO UPDATE SET parked=excluded.parked,attempt=excluded.attempt,status=excluded.status,updated_at=excluded.updated_at,fence=excluded.fence WHERE outbox_cursor.fence<=@token RETURNING sink,'parked',fence,parked,updated_at,NULL::jsonb",
            "SELECT sink,'current',fence,parked,updated_at,NULL::jsonb FROM outbox_cursor WHERE tenant=@tenant AND sink=@sink",
            ("sink", value.Sink.Value), ("sequence", value.Sequence), ("attempt", value.Attempt), ("status", value.Status), ("now", now)),
        leaseGuard: value => Read(
            "SELECT key,holder,generation,NULL::bigint,until,NULL::jsonb FROM lease WHERE tenant=@tenant AND key=@lease",
            ("lease", value.Lease.Value)),
        // Both budget reads answer off the ledger the debit and credit already write. The token read is an AGGREGATE
        // over that ledger — `coalesce` floors an untouched tenant at generation zero rather than handing the reader a
        // null the row projection cannot carry — so the watermark needs no second table and no per-tenant row to mint.
        budgetLoad: _ => Read(
            "SELECT unit,'current',fence,balance,'epoch'::timestamptz,NULL::jsonb FROM budget_ledger WHERE tenant=@tenant ORDER BY unit"),
        budgetToken: _ => Read(
            "SELECT @tenant,'current',coalesce(max(fence),0),count(*),'epoch'::timestamptz,NULL::jsonb FROM budget_ledger WHERE tenant=@tenant"));

    // Units bind SORTED, and `HashMap` keys are unique by construction, so every caller walks the ledger's rows in
    // one order and a repeated unit — raising `21000` on the credit's `ON CONFLICT`, silently dropping an amount
    // on the debit's `UPDATE … FROM` — cannot be spelled. Debit guards ride the statement's own `WHERE`, which
    // PostgreSQL re-evaluates against a concurrent writer's committed version, so decrements settle with no lock;
    // credits ride `ON CONFLICT DO UPDATE`, the one construct guaranteeing insert-or-update for a unit whose row
    // may be absent. Both RETURN what they applied and truth reads every requested unit's balance and fence, so
    // `Settled` names its refusing unit and that unit's held balance off ONE round trip.
    static CaseSql Budget(HashMap<string, long> amounts, bool debit) {
        string[] units = amounts.Keys.Order(StringComparer.Ordinal).ToArray();
        long[] values = units.Map(unit => amounts.Find(unit).IfNone(0L)).ToArray();
        string guarded = debit
            ? "UPDATE budget_ledger b SET balance=b.balance-r.amount,fence=@token FROM unnest(@units,@amounts) AS r(unit,amount) WHERE b.tenant=@tenant AND b.unit=r.unit AND b.balance>=r.amount AND b.fence<=@token RETURNING b.unit,'applied',b.fence,b.balance,'epoch'::timestamptz,NULL::jsonb"
            : "INSERT INTO budget_ledger(tenant,unit,balance,fence) SELECT @tenant,r.unit,r.amount,@token FROM unnest(@units,@amounts) AS r(unit,amount) ORDER BY r.unit ON CONFLICT(tenant,unit) DO UPDATE SET balance=budget_ledger.balance+excluded.balance,fence=excluded.fence WHERE budget_ledger.fence<=@token RETURNING unit,'applied',fence,balance,'epoch'::timestamptz,NULL::jsonb";
        return Ledger(guarded,
            "SELECT unit,'current',fence,balance,'epoch'::timestamptz,NULL::jsonb FROM budget_ledger WHERE tenant=@tenant AND unit=ANY(@units) ORDER BY unit",
            ("units", units), ("amounts", values));
    }

    // Budget verbs take NO advisory scope: the ledger's own row locks are the grain, so one tenant's debits
    // over disjoint units never serialize against each other and the whole-ledger key that made them is deleted.
    static CaseSql Ledger(string guarded, string truth, params (string Name, object Value)[] binds) =>
        new(Seq<LockScope>(), true, Some(guarded), truth, None, toSeq(binds));

    static CaseSql Write(LockScope scope, string guarded, string truth, params (string Name, object Value)[] binds) =>
        new(Seq(scope), true, Some(guarded), truth, None, toSeq(binds));

    static CaseSql Write(LockScope scope, string guarded, string truth, bool requiresToken, params (string Name, object Value)[] binds) =>
        new(Seq(scope), requiresToken, Some(guarded), truth, None, toSeq(binds));

    static CaseSql WriteWake(LockScope scope, string guarded, string truth, string wake, params (string Name, object Value)[] binds) =>
        new(Seq(scope), true, Some(guarded), truth, Some(wake), toSeq(binds));

    // Reads name no scope and no guarded statement, so each contributes ONE result set and takes no lock at all.
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
|  [06]   | receipt floor   | `IsValid => ValidityClaim.All(...)` per case  | kernel validity fold ([C]); `&&` chains deleted                      |
|  [07]   | port direction  | AppHost decodes Persistence-owned types       | four PORT rows + `MembershipView.Serving`; nothing crosses down      |
|  [08]   | row canon       | `(key, state, fence, value, until, payload)`  | fence and case scalar never alias; `CaseSql.For` generates every row |
|  [09]   | signal row      | fenced `(workflow, channel)` upsert           | `SignalPut`/`SignalLoad` decode `StepStateSeam.SignalPut`/`SignalOf` |
|  [10]   | throw crossing  | one `CoordinationFault.Lift` per rail leg     | banded case on every `Fin`; a bare `Error` is the deleted form       |
|  [11]   | retriability    | `CoordinationFault.IsTransient` per case      | provider-classified contention; deterministic refusals stay false    |
|  [12]   | refusal fact    | `store.coordination.fault`, emitted at `Run`  | union's own `Category` and bit; no adapter re-mints it               |
|  [13]   | unit atomicity  | `SAVEPOINT rasm_coord` per batch              | a refusal undoes the unit; per-row re-check alone commits siblings   |
|  [14]   | entry arity     | `Run` takes the op `Seq`                      | two calls are two transactions no ordering law reaches across        |
|  [15]   | retry owner     | store execution strategy ABOVE `Run`          | `Lift` seats outer; `Verified` is the `verifySucceeded` probe        |
|  [16]   | budget polarity | vector is REMAINING per unit                  | no ceiling crosses; spend derives `ceiling - remaining`              |
|  [17]   | outbox park     | attempt and status on the per-sink cursor row | committed op-log stays one owner; no message-envelope table  |

## [03]-[OUTBOX_CURSOR]

- Owner: `OutboxCursor` the per-sink durable drain cursor row — `(Sink, Sequence, Parked, Attempt, Status)`, one row per sink, so each sink drains the ONE Marten stream independently and carries its own head-of-line failure on that same row; the `ONE_OUTBOX_EGRESS_SPINE` law this section NAMES; the `OutboxAdvance`, `OutboxPending`, and `OutboxPark` cases (`#COORDINATION_OP`), whose fenced CAS and fenced upsert are the only cursor writers; the `pg_notify` pump wake emitted in the same transaction.
- Entry: the `Version/egress` pump reads `OutboxCursor.Sequence`, drains `Version/ledger#CHANGEFEED` `ReplayWindow.DurableOps(cursor.Sequence, take)` rows, delivers, and calls `Coordinate.Run(session, sink, Seq<CoordinationOp>(new OutboxAdvance(egress, through)), held, frame, cancellationToken)` through its session-closed `EgressPorts.Coordinate` arrow — the advance CAS is `UPDATE outbox_cursor SET sequence = @through WHERE sink = @sink AND tenant = @tenant AND sequence < @through RETURNING sequence`, so a concurrent pump instance's stale advance returns zero rows and rails `CoordinationFault.OutboxDrain(sink, through)` (at-least-once law: the cursor advances only forward, only after delivery confirmation, and a crash between delivery and advance re-drains — the sink's dedup composition absorbs the replay).
- Auto: the Marten event stream IS the outbox — a domain commit and its egress obligation are ONE `SaveChangesAsync` because the committed event itself is the drainable row (`OpLogEntry` projects from it), so no message-envelope table fills, no relay polls, and no dual-write gap opens; the same transaction that advances a cursor or commits an event `pg_notify('rasm_outbox', @sink)`s the channel, and the egress pump's idle connection `WaitAsync` wakes on it (the low-latency wake beside the bounded poll floor — a missed NOTIFY is absorbed by the next poll, so the wake is latency, never correctness); the cursor is PER-SINK so a slow webhook never holds back the NATS drain, and DISTINCT from the per-origin `SyncCursor` (`ledger.md` `#CHANGEFEED`) which positions peer replication, not sink delivery.
- Receipt: a cursor advance rides `store.coordination.outbox` carrying the sink and the through-sequence; the held-cursor stall evidence is the egress pump's (`Version/egress#EGRESS_PUMP` `CursorStall`), never minted here.
- Growth: a new egress sink is ONE `outbox_cursor` row minted on first drain (advance and park both upsert), zero coordination edits; zero new surface — a message-envelope outbox table, a per-event delivery-state table, a per-sink advance verb, a trigger-based cursor writer, or a coordination-side read of the pump is the deleted form.
- Boundary: forward-only intra-leg edge — `Version/egress` drains this cursor and coordination NEVER reads the pump (the acyclicity proof's one intra-leg egress edge); the cursor is keyed PER SINK, so that pump and the AppHost relay are two consumers holding two rows of one table rather than two writers of one row, and the edge stays forward-only for each; delivery attempt and status ride the sink's own row, the committed op-log being the outbox; a caller composing an advance crosses WIRE-STABLE PRIMITIVES — the sink NAME beside the through-sequence, the shape the sibling dead-letter arrow already takes — because a relay holding no `SinkKey` cannot mint one and a port demanding it forecloses its own consumer; the failed advance CAS stays `CoordinationFault.OutboxDrain` in THIS band (the cursor write is fenced-store work) while every delivery fault is the pump's `EgressFault`; the presence/awareness lane (`ColumnFamily.Presence`, `durable: false`) never has a cursor row — only `Family.Durable` lanes drain past this cursor.

| [INDEX] | [POLICY]       | [VALUE]                                            | [BINDING]                                                      |
| :-----: | :------------- | :------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | outbox spine   | the Marten stream IS the outbox                    | same-`IDocumentSession`; message-envelope tables are deleted   |
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
