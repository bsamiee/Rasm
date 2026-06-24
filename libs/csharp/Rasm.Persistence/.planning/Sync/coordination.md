# [PERSISTENCE_COORDINATION]

Rasm.Persistence owns the durable cross-process coordination store the AppHost runtime spine fences its distributed correctness through: a `FencingToken`-fenced compare-and-set cell store under the `TenantId` row-level-security predicate, serving the per-tenant `Budget` quota ledger, the crash-durable workflow step-state row, the transactional-outbox row, and the distributed maintenance-lease/membership cell as rows on one store rather than four tables and four owners. `CoordCell` is the one fenced-CAS shape — a `(TenantId, CoordKind, CoordKey)` primary identity carrying a `FencingToken` column, an `xmin` optimistic-concurrency token, and a `jsonb` payload — and `CoordStore` is the static surface whose `Cas` fold gates every mutation: it admits a write only when the incoming `FencingToken` is monotone (the Kleppmann reject-lower the AppHost `Runtime/time#FENCING_TOKEN` `FencingToken.Admits` predicate names) AND the row-side invariant the kind declares holds INSIDE the one `Query/transaction#TXN_SCOPE` `Transactions.Scoped` serializable transaction, so the gate is the atomic store-side check, never a read-then-write the caller races. The AppHost `Agent/capability#GRANT_BROKER` `DistributedBudget`, the AppHost `Runtime/orchestration#STEP_STATE_SEAM` `StepStateSeam`, the AppHost `Wire/outbox#DISPATCH_SWEEP` `OutboxRelay.Runtime`, and the AppHost `Runtime/time#FENCING_TOKEN` `LeaseElection` (the maintenance-lease/membership CAS the `Wire/coordination` and `Wire/companion#PROCESS_MODALITY` sidecar fence through) are the delegate seams this store satisfies; `TenantContext`/`TenantId`, `FencingToken`, `CostVector`/`CostUnit`, `WorkflowInstance`/`WorkflowStep`/`StepKind`, `DomainEvent`, `ClockPolicy`, and `ReceiptSinkPort` arrive settled as AppHost vocabulary, and the store mints no second changefeed — a relayed outbox row rides the one `Sync/collaboration#OPLOG_CHANGEFEED` `OpLog` the `Sync/egress#EGRESS_PUMP` pump already drains. This `CoordStore` distributed maintenance-lease cell is the cross-process election the AppHost `LeaseElection.Acquire` fences at the server tier; it is a distinct altitude from the per-store first-opener `Store/profiles#STORE_LIFECYCLE` `StoreLeaseRow` (the embedded-sqlite single-writer epoch lease), the two leases never duplicated — the `StoreLeaseRow` arbitrates one embedded file's writer, the `lease` `CoordCell` arbitrates a fleet-wide role across nodes.

Wire posture: this page is host-local — the fenced CAS runs server-side on the self-provisioned PostgreSQL tier under RLS, and the embedded-sqlite floor maps the same CAS onto its `version`-token write path, so the coordination store crosses no browser or peer wire and carries no `TS_PROJECTION` cluster. The three contracts cross only the AppHost inward seam as settled delegate records; the relayed outbox payload reaches external consumers only through the settled `Sync/egress#EGRESS_PUMP` CloudEvents projection, never a second egress shape minted here.

## [01]-[INDEX]

- [01]-[FENCED_CAS]: the `CoordCell` fenced-CAS shape, the `CoordKind` axis, and the `CoordStore.Cas` atomic-gate fold under `TenantId` RLS.
- [02]-[BUDGET_LEDGER]: the per-tenant `Budget` quota row satisfying `DistributedBudget` — `Spent` read, the ceiling-inside-CAS `Debit`, and the `Token` mint.
- [03]-[STEP_STATE]: the workflow step-state CAS row satisfying `StepStateSeam` — `Persist`/`Rehydrate`/`InFlight` over wire-stable keys plus the resume cursor.
- [04]-[OUTBOX_TABLE]: the transactional-outbox row satisfying `OutboxRelay.Runtime` — `Pending`/`Advance`/`DeadLetter`, committing same-transaction with the step-state row and relaying over the one op-log.

## [02]-[FENCED_CAS]

- Owner: `CoordKind` the `[SmartEnum<string>]` coordination-row axis (`budget` | `step-state` | `outbox` | `dead-letter` | `lease`) under the `SyncKeyPolicy` ordinal accessor; `CoordCell` the one fenced-CAS row carrying the `(TenantId, CoordKind, CoordKey)` identity, the `FencingToken` column, the `xmin` concurrency token, and the `jsonb` payload; `CoordFault` the `[Union]` fault family in the 7100 band (`Fenced` 7101 | `CeilingExceeded` 7102 | `Stale` 7103 | `Missing` 7104); `CoordStore` the static fenced-CAS surface.
- Cases: `CoordKind` `budget` (the per-tenant quota cell), `step-state` (one row per `WorkflowInstance`), `outbox` (one row per pending `DomainEvent`), `dead-letter` (a poison outbox row past its attempt budget), `lease` (the distributed maintenance-lease/membership cell the AppHost `LeaseElection.Acquire` fences for a fleet-wide role); a write carrying a `FencingToken` strictly below the row's accepted token rejects `Fenced`; a write whose `xmin` differs from the read snapshot rejects `Stale` so the caller re-reads rather than clobbering a concurrent commit.
- Entry: `public static IO<Fin<CoordCell>> Cas(CoordStore.Runtime runtime, TenantContext tenant, CoordKind kind, string key, FencingToken token, Func<Option<CoordCell>, Fin<CoordCell>> transition)` — the one gate: it opens the `Transactions.Scoped` serializable transaction under the tenant RLS GUC, reads the addressed `CoordCell` `FOR UPDATE` (the `LockMode.RowExclusive` clause), checks the held token through `FencingToken.Admits` rejecting `Fenced` on a lower token, applies the `transition` fold over the current cell (the fold returns the next cell or the typed reject — the `budget` ceiling check lives in the fold so `spent + cost > ceiling` rejects `CeilingExceeded` INSIDE the lock), stamps the next cell with the incoming token, and upserts it through the `StoreOp.Upsert` arm, all in one transaction so the gate is atomic; `Read(...)` reads one cell past the RLS predicate without locking (the dry-run/`Spent`/`Rehydrate` read path), `Tenant(...)` pages a kind's rows for the `InFlight` set, and `Sweep(..., watermark, lockMode)` dequeues outbox rows past the watermark `FOR UPDATE SKIP LOCKED`.
- Auto: every coordination mutation is the one `Cas` fold so the budget debit, the step-state commit, and the outbox enqueue share one atomic-gate semantic, never three CAS implementations; the fence is the AppHost `FencingToken` monotone token checked store-side under the row lock so two nodes racing a write cannot both win — the store serializes the debits and rejects the lower token, the Kleppmann reject-lower a timeout-only lease cannot give; the `xmin` system column is the optimistic-concurrency token the `Query/transaction#SQLSTATE_CLASSIFIER` already projects into `StoreFault.Concurrency` so a lost-update on the same token surfaces as `Stale` rather than a silent overwrite; the RLS predicate is the `Store/tenancy#TENANCY_RLS` `current_setting('rasm.tenant')::uuid` policy so a tenant reads and writes only its own coordination rows and a cross-tenant read returns the empty set by construction; the `coord_cell` table creation pins to the provisioning migration bundle so every node reads one coordination surface, and the `(tenant_id, kind, coord_key)` unique constraint makes the upsert the single arbiter; the sqlite floor maps the same gate onto its `version`-token write under the writer lease so the embedded single-process store satisfies the same contracts without a second code path.
- Receipt: a fenced CAS commit rides `coord.cas` carrying the kind, the key, and the accepted token; a `CeilingExceeded`/`Fenced`/`Stale` reject rides `coord.reject` carrying the offending kind and the typed fault; the per-kind sub-sections add their typed receipts (`BudgetReceipt`, the step `CommandReceipt` the orchestrator mints, the `DeliveryReceipt` the relay mints) — this owner adds no generic coordination receipt, the typed evidence stays with the kind that mints it.
- Packages: Npgsql, Microsoft.Data.Sqlite, linq2db.EntityFrameworkCore, System.Text.Json, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new coordination concern is one `CoordKind` row plus one `transition` fold the `Cas` gate runs, never a second table or a second CAS surface; a new fault is one `CoordFault` case; zero new surface — a per-concern coordination table, a second fencing-token column, or a CAS loop in the caller is the deleted form because the gate is one store-side atomic fold under one row lock.
- Boundary: `CoordStore` is the suite's only durable cross-process coordination owner — a per-tenant quota service, a workflow-instance table that bypasses the fence, a second transactional-message store, and a second distributed-lock store are the deleted forms; the store satisfies the AppHost delegate seams (`DistributedBudget`, `StepStateSeam`, `OutboxRelay.Runtime`, and the `LeaseElection` membership/lease CAS the `Wire/coordination` `CLUSTER_MEMBERSHIP_ELECTION` ripple binds) and never reverses the dependency — AppHost names the seam and holds the runtime policy, atomicity and the fenced CAS stay here; the fence is the AppHost `FencingToken` checked store-side under the row lock so the correctness proof is the atomic store check, never a held-lease-without-fence (the rejected form); the ceiling check executes inside the `budget` `transition` fold within the one transaction so the multi-node ceiling overshoot a per-process gate would open is foreclosed at the store — the AppHost gates the ceiling outside the fenced write ONLY for a dry-run pre-flight off `Spent`; the `TenantId` RLS predicate is the `Store/tenancy#TENANCY_RLS` policy so the coordination rows are tenant-scoped by construction and never a coarse table-level grant; the store mints no second changefeed — a relayed outbox row rides the one `Sync/collaboration#OPLOG_CHANGEFEED` op-log the `Sync/egress#EGRESS_PUMP` pump drains, so a second egress table or a re-minted `CdcEnvelope` is the drift defect the `[ONE_OUTBOX_EGRESS_SPINE]` branch forecloses; the database stays excluded from the AppHost hop law — `EnableRetryOnFailure` on the pg row reruns the serialization-failure-aborted `Cas` closure and is the only retry owner.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class CoordKind {
    public static readonly CoordKind Budget = new("budget");
    public static readonly CoordKind StepState = new("step-state");
    public static readonly CoordKind Outbox = new("outbox");
    public static readonly CoordKind DeadLetter = new("dead-letter");
    public static readonly CoordKind Lease = new("lease");
}

// One fenced-CAS row: (TenantId, CoordKind, CoordKey) identity, the FencingToken column, the xmin
// optimistic token, and the jsonb payload. Spelled once here; the per-kind rows project this shape.
public sealed record CoordCell(
    TenantId Tenant,
    CoordKind Kind,
    string Key,
    FencingToken Token,
    uint Xmin,
    JsonElement Payload,
    Instant At) {
    public bool Admits(FencingToken incoming) => Token.Admits(incoming);
}

public abstract partial record CoordFault : Expected, IValidationError<CoordFault> {
    private CoordFault(string detail, int code) : base(detail, code, None) { }
    public static CoordFault Create(string message) => new Fenced(message);
    public sealed record Fenced : CoordFault { public Fenced(string detail) : base(detail, 7101) { } }
    public sealed record CeilingExceeded : CoordFault { public CeilingExceeded(string unit, long over) : base($"{unit}:+{over}", 7102) => Unit = unit; public string Unit { get; } }
    public sealed record Stale : CoordFault { public Stale(string detail) : base(detail, 7103) { } }
    public sealed record Missing : CoordFault { public Missing(string detail) : base(detail, 7104) { } }
}

public static class CoordStore {
    public sealed record Runtime(
        PooledDbContextFactory<StoreDb> Contexts,
        InterceptPolicy Policy,
        ClockPolicy Clocks,
        DeadlineClass Deadline);

    // The one atomic gate: serializable transaction, FOR UPDATE row read, fence check, transition fold,
    // upsert carrying the incoming token — all in one Transactions.Scoped transaction so the gate is
    // store-side atomic. The write rides the StoreOp.Upsert arm; no ninth StoreOp case is minted.
    public static IO<Fin<CoordCell>> Cas(
        Runtime runtime, TenantContext tenant, CoordKind kind, string key,
        FencingToken token, Func<Option<CoordCell>, Fin<CoordCell>> transition) =>
        Transactions.Scoped(
            runtime.Contexts,
            new TxnScope(IsolationPolicy.Serializable, new LockMode.RowExclusive(), Seq<Savepoint>.Empty, Prepared: false, None),
            new StoreOp<Fin<CoordCell>>.Upsert(async (db, ct) => {
                var current = await Locate(db, tenant.TenantId, kind, key, ct);
                var gate = current.Filter(cell => !cell.Admits(token)).Match(
                    Some: _ => Fin<CoordCell>.Fail(new CoordFault.Fenced($"{kind}:{key}")),
                    None: () => transition(current).Map(next => next with { Token = token, At = runtime.Clocks.Now }));
                return await gate.MatchAsync(
                    SuccAsync: next => Upsert(db, next, ct),
                    Fail: fault => Fin<CoordCell>.Fail(fault));
            }),
            runtime.Policy, runtime.Clocks, runtime.Deadline);

    public static IO<Option<CoordCell>> Read(Runtime runtime, TenantContext tenant, CoordKind kind, string key) =>
        StoreRail.Run(runtime.Contexts, new StoreOp<Option<CoordCell>>.Get((db, ct) => Locate(db, tenant.TenantId, kind, key, ct)), runtime.Policy, runtime.Clocks, runtime.Deadline);

    // The tenant sweep: all non-locked rows of a kind past the RLS predicate (InFlight, the dispatch sweep).
    public static IO<Seq<CoordCell>> Tenant(Runtime runtime, TenantContext tenant, CoordKind kind) =>
        StoreRail.Run(runtime.Contexts, new StoreOp<Seq<CoordCell>>.Query((db, ct) => Page(db, tenant.TenantId, kind, ct)), runtime.Policy, runtime.Clocks, runtime.Deadline);

    // The outbox dispatch dequeue: rows past the watermark, FOR UPDATE SKIP LOCKED so two nodes never
    // relay one row, ordered by the HLC Logical the OutboxRow watermark carries.
    public static IO<Seq<CoordCell>> Sweep(Runtime runtime, TenantContext tenant, CoordKind kind, ulong watermark, LockMode lockMode) =>
        Transactions.Scoped(
            runtime.Contexts,
            new TxnScope(IsolationPolicy.ReadCommitted, lockMode, Seq<Savepoint>.Empty, Prepared: false, None),
            new StoreOp<Seq<CoordCell>>.Query((db, ct) => PageAfter(db, tenant.TenantId, kind, watermark, ct)),
            runtime.Policy, runtime.Clocks, runtime.Deadline);
}
```

## [03]-[BUDGET_LEDGER]

- Owner: `Budget` the per-tenant quota cell projected from the `budget`-kind `CoordCell` payload (the spent `CostVector` plus the resolved ceiling); `BudgetReceipt` the typed debit evidence; the `DistributedBudget` triple this row satisfies — the AppHost `Agent/capability#GRANT_BROKER` opt-in seam the broker debits a durable per-tenant budget through.
- Cases: the `budget` cell is one row per `TenantId` (the `CoordKey` is the tenant slug) so the broker meters each tenant independently against one store; a `Debit` whose `spent + cost` exceeds the `ceiling` rejects `CeilingExceeded` with the offending `CostUnit` named, a debit carrying a stale token rejects `Fenced`, and a fresh-token debit under the ceiling commits the advanced spent vector.
- Entry: the AppHost `GrantBroker` constructs its `DistributedBudget` triple from this surface — `Spent(TenantId) => Fin<CostVector>` maps `CoordStore.Read` of the `budget` cell to the decoded spent vector (the dry-run pre-flight price, never touching the lock); `Debit(TenantId, FencingToken, CostVector cost, CostVector ceiling) => Fin<CostVector>` runs `CoordStore.Cas` supplying the `Budget.Debit` `transition` fold that reads the current spent vector, adds the cost, and rejects `CeilingExceeded` when `Spent.Add(cost).Of(unit) > ceiling.Of(unit)` for any metered unit else returns the advanced cell so the ceiling gate executes inside the one transaction; `Token(TenantId) => Fin<FencingToken>` maps the cell's current `FencingToken.Next()` so the broker presents the next monotone token on its debit. `Budget` and `BudgetReceipt` are the Persistence-owned projections; the triple construction is the AppHost half (`WorkflowInstance`/`DistributedBudget` are AppHost types Persistence never builds).
- Auto: the ceiling check is the `transition` fold's own arm so the atomic store write decides `spent + cost <= ceiling` under the row lock — two nodes presenting fresh tokens cannot both overshoot because the store serializes the debits and the second reads the first's committed spent vector and rejects, the fleet-wide ceiling enforcement a per-process gate cannot give; the `CostVector` is a per-`CostUnit` map so each metered resource caps independently and a command under the `Calls` ceiling but over the `BytesEgress` ceiling rejects on bytes-egress with the unit named; the spent vector resets on the window roll the broker's `GrantScope.Window` carries (the broker writes the reset through one `Cas` carrying the next token) so a new window starts at zero spent without a second cell; the model-governance charge, the plugin `GrantHandle` charge, and the operator call all debit this one `budget` cell so a multi-node identity plane cannot let a tenant exceed its ceiling N-fold.
- Receipt: a debit rides `coord.budget.debit` carrying the tenant, the charged `CostVector`, and the accepted token; a ceiling rejection rides `coord.budget.ceiling` carrying the offending `CostUnit` and the overage; the typed `BudgetReceipt(Tenant, Charged, Spent, Ceiling, FencingToken, Instant)` carries the debit evidence; an over-ceiling debit is the typed `CoordFault.CeilingExceeded`, never a silent clamp.
- Packages: Npgsql, linq2db.EntityFrameworkCore, System.Text.Json, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new metered resource rides the `CostUnit` axis already so the budget cell absorbs it without a schema change; a new quota concern is one field on the `Budget` payload, never a second quota table; zero new surface.
- Boundary: the `budget` cell is the AppHost `GrantBroker`'s opt-in durable backing, never a second meter — with no seam bound the broker debits its per-process `Cell`, and this store is the cross-process upgrade the one broker entry consumes through the `DistributedBudget` triple; the ceiling gate is store-side inside the `Cas` transaction so the AppHost gates the ceiling outside the fenced write ONLY for the dry-run pre-flight off `Spent`, foreclosing the read-then-write TOCTOU a multi-node per-process gate would open; the budget cell is the `ONE_FENCED_LEASE_STORE` leg the AppHost `DEEPEN_DISTRIBUTED_QUOTA_STORE_SEAM` and `STRUCTURE_SANDBOX_GRANT_MEDIATION` cards bind, consumed at the `Agent/capability ⇄ Rasm.AppHost/Runtime # [PORT]: fenced per-tenant Budget debit` seam — Persistence owns the durable Budget and the fenced CAS, AppHost owns the metering policy and the broker entry.

```csharp signature
// The Persistence-owned budget projection the `budget`-kind CoordCell payload encodes/decodes.
// Debit is the ceiling-gating transition fold the AppHost DistributedBudget.Debit supplies to
// CoordStore.Cas, so `spent + cost <= ceiling` decides INSIDE the one fenced transaction.
public sealed record Budget(CostVector Spent, CostVector Ceiling) {
    static readonly Seq<CostUnit> Metered = Seq(CostUnit.CpuMillis, CostUnit.WallMillis, CostUnit.BytesEgress, CostUnit.ModelTokens, CostUnit.Calls);

    public Fin<Budget> Debit(CostVector cost) =>
        Spent.Add(cost) is var next
            ? Metered.Find(unit => next.Of(unit) > Ceiling.Of(unit)).Match(
                Some: unit => Fin<Budget>.Fail(new CoordFault.CeilingExceeded(unit.ToString(), next.Of(unit) - Ceiling.Of(unit))),
                None: () => Fin<Budget>.Succ(this with { Spent = next }))
            : Fin<Budget>.Succ(this);

    public CoordCell Encode(CoordCell cell) => cell with { Payload = JsonSerializer.SerializeToElement(this) };
    public static Budget Decode(Option<CoordCell> cell, CostVector ceiling) =>
        cell.Map(static c => JsonSerializer.Deserialize<Budget>(c.Payload)!).IfNone(new Budget(CostVector.Zero, ceiling));
}

public sealed record BudgetReceipt(TenantId Tenant, CostVector Charged, CostVector Spent, CostVector Ceiling, FencingToken Token, Instant At);
```

## [04]-[STEP_STATE]

- Owner: the `step-state`-kind `CoordCell` carrying the wire-stable `WorkflowInstance` projection (status key, resume cursor, and each committed step's `StepKind` payload), satisfying the AppHost `Runtime/orchestration#STEP_STATE_SEAM` `StepStateSeam` triple; `StepStateRow` the durable projection record; the row commits same-transaction with the `outbox` row under one tenant-scoped transaction.
- Cases: one `step-state` cell per `WorkflowInstance` (the `CoordKey` is the instance id); a `Persist` carrying a `FencingToken` strictly below the instance's accepted token rejects `Fenced` so a resumed stale instance presenting a lower token fails rather than double-committing; the cell carries only wire-stable keys plus the resume cursor — the instance id, the workflow id, the status key, the cursor, and each committed step's `StepKind` payload (descriptor + serialized arguments for an `Activity`, the fire instant for a `Timer`, the channel for a `Signal`, the schedule key for a `PersistentJob`), never a live closure.
- Entry: the AppHost orchestrator constructs its `StepStateSeam` triple from this surface — `Persist(WorkflowInstance, WorkflowStep, FencingToken) => Fin<Unit>` projects the advanced instance to a `StepStateRow` and runs `CoordStore.Cas` upserting the `step-state` cell under the fenced token (the same transaction the `outbox` enqueue rides when the step publishes a domain event); `Rehydrate(string instanceId) => Fin<WorkflowInstance>` maps `CoordStore.Read` of the `step-state` cell to the decoded `StepStateRow`, which the orchestrator reconstructs into a `WorkflowInstance` from the wire-stable bytes so a crash-surviving process resumes mid-saga; `InFlight(TenantContext) => Fin<Seq<string>>` maps `CoordStore.Tenant` filtered to the non-terminal `step-state` rows (status `running`/`suspended`/`compensating`) to their instance ids so the orchestrator drives the resume sweep. `StepStateRow`/`StepPayload` are the Persistence-owned wire-stable projections; the `WorkflowInstance`/`StepStateSeam` construction is the AppHost half.
- Auto: each `Persist` is one fenced `Cas` so a committed step persists by compare-and-set carrying only wire-stable keys plus the resume cursor and a `StepKind.Activity` carries a `CommandIntent` (descriptor + serialized arguments + caller modality), not a `Func`, so a step rehydrates from durable bytes; the fence is the instance's `FencingToken` so a resumed stale instance presenting a lower token fails `Fenced` rather than double-committing — the same Kleppmann reject-lower the budget cell enforces; the `step-state` cell and the `outbox` cell upsert under one `Transactions.Scoped` transaction when the step publishes a domain event so a step commit and its event enqueue ride one transaction boundary and crash recovery is re-delivery for both; `Rehydrate` reads the cursor and the committed-step payloads so the executor folds the remaining steps from the resume cursor, never replaying committed steps; a step's `CommandReceipt` chains into the AppHost `EventLog` on the one op-log so the workflow log and the command log are one stream, never a second event store.
- Receipt: a step persist rides `coord.step.persist` carrying the instance id, the step index, and the accepted token; the step's own `CommandReceipt` (the executor's) and the instance `SpineLog` event stay the AppHost orchestrator's evidence — this owner adds the durable-commit fact, never a second workflow receipt; a fence-stale persist is the typed `CoordFault.Fenced` the orchestrator maps to its `OrchestrationFault.FenceStale`.
- Packages: Npgsql, linq2db.EntityFrameworkCore, System.Text.Json, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new step shape is one AppHost `StepKind` case the wire-stable payload carries (the projection serializes the union by its key) so the cell absorbs it without a schema change; a new instance status is one status-key value; zero new surface — a per-workflow state-machine table is the deleted form.
- Boundary: the step-state cell is the `ONE_FENCED_LEASE_STORE` leg the AppHost `DURABLE_ORCHESTRATION_SPINE` and `SEAM_OUTBOX_AND_WORKFLOW_PERSISTENCE_TABLE` cards bind, consumed at the `Runtime/orchestration ⇄ Rasm.AppHost/Runtime # [PORT]: workflow step-state CAS` seam — the AppHost orchestrator writes through the `Persist`/`Rehydrate`/`InFlight` contract and the durable CAS row, the fenced-token column, and the `TenantId` RLS predicate stay here; the cell persists only wire-stable keys plus the resume cursor by compare-and-set, never a live closure, so a per-process workflow table that bypasses the fenced store is the rejected form; the `step-state` row and the `outbox` row commit under one tenant-scoped transaction so crash-durable step resumption and exactly-once-effective delivery share one durable boundary — coupling to the CAS + outbox contract, never the store interior; the orchestrator's saga compensation and step dispatch stay AppHost runtime policy, durability stays here.

```csharp signature
// The Persistence-owned wire-stable step-state projection the `step-state`-kind CoordCell carries —
// keys plus the resume cursor, never a live closure. StepPayload is the wire-stable serialization of
// the AppHost StepKind union (its key plus the kind's payload), so a step rehydrates from durable bytes.
public sealed record StepPayload(string Kind, int Index, string Status, JsonElement Body);

public sealed record StepStateRow(
    string InstanceId,
    string WorkflowId,
    string Status,
    int Cursor,
    Seq<StepPayload> Steps,
    FencingToken Fence) {
    static readonly Seq<string> Terminal = Seq("completed", "faulted");
    public bool NonTerminal => !Terminal.Contains(Status);

    public CoordCell Encode(CoordCell cell) => cell with { Payload = JsonSerializer.SerializeToElement(this) };
    public static Option<StepStateRow> Decode(Option<CoordCell> cell) =>
        cell.Map(static c => JsonSerializer.Deserialize<StepStateRow>(c.Payload)!);
}
```

## [05]-[OUTBOX_TABLE]

- Owner: the `outbox`-kind `CoordCell` carrying the pending `DomainEvent` projection (topic, dedup key, payload, dispatch status, attempt, HLC stamp), satisfying the AppHost `Wire/outbox#DISPATCH_SWEEP` `OutboxRelay.Runtime` triple; `OutboxRow`/`DeadLetterRow` the durable projection records; the dispatch-sweep cursor is the `(ConsumerId, Hlc)` watermark, the dedup-key index is the unique `(tenant_id, dedup_key)` constraint.
- Cases: one `outbox` cell per pending `DomainEvent` (the `CoordKey` is `{topic}:{idempotency-key}`); a row relayed advances to dispatched, a row exhausting its `MaxAttempts` budget transitions to a `dead-letter`-kind cell carrying the last fault and the attempt history so a poison message leaves the dispatch lane rather than blocking it; the watermark advance fences through `FencingToken.Admits` so two nodes cannot both advance it past one row.
- Entry: the AppHost outbox relay constructs its `OutboxRelay.Runtime` triple from this surface — `Pending(TenantContext, ulong watermark) => Fin<Seq<OutboxRow>>` maps `CoordStore.Sweep` of the tenant's `outbox` cells past the watermark cursor ordered by HLC `Logical` (the `LockMode.SkipLocked` dequeue so two nodes do not relay the same row simultaneously) to the decoded rows; `Advance(OutboxRow, FencingToken) => Fin<ulong>` runs `CoordStore.Cas` marking the row dispatched under the fenced token and returns the advanced `(ConsumerId, Hlc)` watermark (the row's `Logical`); `DeadLetter(DeadLetterRow) => Fin<Unit>` upserts the `dead-letter` cell for an exhausted row so an operator replays it through the AppHost relay's `Replay`. `OutboxRow`/`DeadLetterRow` are the Persistence-owned durable projections mirroring the AppHost row shape; the `OutboxRelay.Runtime` construction is the AppHost half.
- Auto: the `outbox` cell writes same-transaction with the producing write (the `step-state` `Cas` when a workflow step publishes, or any entity write that enqueues) so a domain event and its source state commit atomically — a crash between the state write and the event publish cannot lose the event because both ride one transaction, the transactional-outbox guarantee; the dispatch sweep reads pending rows `FOR UPDATE SKIP LOCKED` so the fleet-spread sweep across nodes never double-relays a row, and a successful relay advances the `(ConsumerId, Hlc)` watermark monotonically under the fenced token so a relayed row never re-relays — the at-least-once-with-watermark guarantee that, with the consumer-side dedup, is exactly-once-effective; the dedup key is the event's idempotency key and the unique `(tenant_id, dedup_key)` constraint makes a re-enqueued identical event the upsert no-op so the outbox dedup and the AppHost `DeliveryFanout` dedup are one cell, never two; a relayed row rides the one `Sync/collaboration#OPLOG_CHANGEFEED` op-log the `Sync/egress#EGRESS_PUMP` pump drains as one keyed `OutboundHop` consumer, never a second egress table or a re-minted `CdcEnvelope`; the durable leg is the at-least-once backbone, so a subscription whose in-process `BroadcastBlock` buffer is full when the AppHost bus fan offers misses the in-process copy and re-receives the event on this outbox sweep — the store-side cursor plus the dedup-key index is the durability backbone the bounded in-process bus is not.
- Receipt: a relayed row mints one `DeliveryReceipt` carrying the topic and the dispatched flag (the AppHost relay's own); the watermark advance is the relay's evidence carried as the `coord.outbox.advance` fact with the advanced `(ConsumerId, Hlc)` pair; a dead-letter transition rides `coord.outbox.dead-letter` carrying the last fault and attempt count; no parallel relay receipt — the typed `DeliveryReceipt` stays the relay's evidence.
- Packages: Npgsql, linq2db.EntityFrameworkCore, System.Text.Json, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new dispatch status is one status-key value on the `OutboxRow` projection; a new relay target is one AppHost `OutboundHop` the topic binds; zero new surface — a per-topic outbox table, a second egress table, or a second dedup map is the deleted form.
- Boundary: the outbox table is the `ONE_OUTBOX_EGRESS_SPINE` leg the AppHost `EVENT_BUS_OUTBOX_FABRIC`, `SEAM_OUTBOX_AND_WORKFLOW_PERSISTENCE_TABLE`, and `DEEPEN_EVENTBUS_DATAFLOW_TOPOLOGY` cards bind, consumed at the `Wire/outbox ⇄ Rasm.AppHost/Runtime # [PORT]: transactional outbox same-tx` seam — the AppHost names the seam and the relay, the outbox row writes atomically with the producing transaction here, and the relay registers as one keyed `OutboundHop` consumer advancing its own `(ConsumerId, Hlc)` watermark over the op-log rather than re-minting the `CdcEnvelopeWire` or a second egress table; the at-least-once delivery guarantee is THIS durable outbox leg's (outbox row + watermark + consumer dedup), NOT the in-process `BroadcastBlock` bus (bounded best-effort latest-value-to-slow-target), so a subscription re-receives a missed event on this sweep and the store-side dispatch cursor plus the dedup-key index is the durability backbone, never an in-process retry; the `outbox` row and the `step-state` row commit under one tenant-scoped transaction so exactly-once-effective delivery and crash-durable step resumption share one durable boundary; the relayed payload reaches external consumers only through the settled `Sync/egress#EGRESS_PUMP` CloudEvents projection so the cross-language consumer reads the one envelope vocabulary, never a shape minted here.

```csharp signature
// The Persistence-owned durable outbox projection mirroring the AppHost OutboxRow shape — written
// same-transaction with the producing write, swept FOR UPDATE SKIP LOCKED, the watermark its Logical.
public sealed record OutboxRow(
    string OutboxId, string Topic, string DedupKey, JsonElement Payload,
    string Status, int Attempt, ulong Logical, Instant Physical, TenantId Tenant) {
    public const int MaxAttempts = 8;
    public ulong Watermark => Logical;

    public OutboxRow Dispatched() => this with { Status = "dispatched" };
    public CoordCell Encode(CoordCell cell) => cell with { Payload = JsonSerializer.SerializeToElement(this) };
    public static OutboxRow Decode(CoordCell cell) => JsonSerializer.Deserialize<OutboxRow>(cell.Payload)!;
}

public sealed record DeadLetterRow(string OutboxId, string Topic, JsonElement Payload, string LastFault, int Attempts, Instant At) {
    public CoordCell Encode(CoordCell cell) => cell with { Payload = JsonSerializer.SerializeToElement(this) };
}
```

| [INDEX] | [POLICY]              | [VALUE]                                              | [BINDING]                                                              |
| :-----: | :-------------------- | :--------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | atomic gate           | one `CoordStore.Cas` serializable `FOR UPDATE` fold  | fence + invariant checked store-side; never a caller read-then-write   |
|  [02]   | fence                 | AppHost `FencingToken.Admits` reject-lower           | monotone token under the row lock; a held lease without fence rejected  |
|  [03]   | ceiling               | `budget` `transition` checks `spent+cost<=ceiling`   | inside the CAS transaction; AppHost gates outside only for dry-run      |
|  [04]   | same-transaction      | `step-state` + `outbox` upsert one `Transactions.Scoped` | crash-durable resume + exactly-once-effective delivery share one boundary |
|  [05]   | dedup                 | unique `(tenant_id, dedup_key)` constraint           | re-enqueue is the upsert no-op; one cell with `DeliveryFanout`, never two |
|  [06]   | watermark             | `(ConsumerId, Hlc)` advanced under the fenced token  | `SKIP LOCKED` dequeue; relayed row never re-relays; one op-log, not a second table |
|  [07]   | tenancy               | `Store/tenancy#TENANCY_RLS` `current_setting('rasm.tenant')::uuid` | coordination rows tenant-scoped by construction; cross-tenant read empty |

## [06]-[RESEARCH]

- [FENCED_CAS_SERIALIZATION]: the `Transactions.Scoped` serializable transaction over the `coord_cell` row read `FOR UPDATE` and the `FencingToken` reject-lower check — whether the fence check plus the `transition` fold plus the upsert execute as one atomic store-side gate and a concurrent debit on a lower token rejects `Fenced` under PG serializable, confirmed against the Forge-provisioned PG18 before the coordination fence pins; the `xmin` system-column read for the optimistic `Stale` token is the `Query/transaction#SQLSTATE_CLASSIFIER` `StoreProfile.ConcurrencyToken` projection already settled.
- [SKIP_LOCKED_SWEEP]: the `outbox` dispatch sweep `SELECT ... FOR UPDATE SKIP LOCKED` dequeue past the `(ConsumerId, Hlc)` watermark and the fleet-spread distribution — whether two nodes sweeping concurrently never relay the same row and the watermark advance fences through `FencingToken.Admits`, confirmed against the live PG tier so the at-least-once-with-watermark guarantee holds across nodes; the `LockMode.SkipLocked` clause is the `Query/transaction#TXN_SCOPE` work-queue-dequeue mechanism already settled.
- [COORD_RLS_PREDICATE]: the `coord_cell` `current_setting('rasm.tenant')::uuid` RLS policy keyed on the `tenant_id` column — whether the `Store/tenancy#TENANCY_RLS` `CREATE POLICY` extends to the coordination table so a tenant reads and writes only its own budget/step-state/outbox rows and a cross-tenant read returns the empty set, confirmed against the provisioned tenant GUC before the seam pins.
