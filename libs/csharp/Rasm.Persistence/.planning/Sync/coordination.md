# [PERSISTENCE_COORDINATION]

Rasm.Persistence owns the durable cross-process coordination store the AppHost runtime spine fences its distributed correctness through: one `FencingToken`-fenced conditional-upsert cell store under the `TenantId` row-level-security predicate, serving the per-tenant quota ledger, the crash-durable workflow step-state row, the transactional-outbox row, the dead-letter lane, and the distributed maintenance-lease/membership cell as rows on one store rather than five tables and five owners. `CoordCell` is the one fenced-CAS shape — a `(TenantId, CoordKind, CoordKey)` primary identity carrying a `FencingToken` column, the `xmin` optimistic-concurrency token, and a `jsonb` payload the per-kind projections encode through one codec — and `CoordStore.Cas` is the atomic gate every mutation rides: a single `INSERT ... ON CONFLICT (tenant_id, kind, coord_key) DO UPDATE SET ... WHERE coord_cell.fence_token <= excluded.fence_token RETURNING *`, so the fence reject-lower predicate and the upsert are ONE store-side statement under the unique constraint, never a `SELECT … FOR UPDATE` then write the prose merely calls atomic. The `WHERE` clause is the Kleppmann reject-lower the AppHost `Runtime/time#FENCING_TOKEN` `FencingToken.Admits` predicate names — a zero-row `RETURNING` IS the typed `Fenced` reject — and because every advancing write (a budget debit, an outbox relay, a lease renew) mints a fresh fence token, the one fence guard subsumes the per-kind monotonicity (the watermark, the spent vector, the epoch) so no per-kind SQL column guard is needed; the kind invariant the value cannot express in the fence (the `budget` ceiling, the `outbox` attempt budget) rejects inside the `transition` fold before the statement; the `xmin` system column the `Point` read surfaces is the optimistic read-snapshot token a read-modify-CAS caller threads, so a `transition` observing a concurrently-advanced snapshot folds `CoordFault.Stale` itself — the fenced `MergeWithOutputAsync` is the sole write-ordering guard and consults no EF optimistic check on the CAS.

The AppHost `Agent/capability#GRANT_BROKER` `DistributedBudget`, the `Runtime/orchestration#STEP_STATE_SEAM` `StepStateSeam`, and the `Wire/outbox#DISPATCH_SWEEP` `OutboxRelay.Runtime` are the three delegate records this store satisfies — this page implements their `Func<…, Fin<…>>` slots over `CoordCell`, running each durable `IO<Fin<…>>` to the seam's synchronous `Fin` at the binding (the seam carries `Fin`, never `IO`), and it never re-declares them. The `Runtime/time#FENCING_TOKEN` `LeaseElection` is NOT a `Func`-slot record but a static `Acquire(Runtime) => Fin<FencingToken>`/`Fence` surface whose `LeaseElection.Runtime` mints the monotone token; this page is the durable lease/membership CAS that `Runtime` is bound to — the `lease`-cell `Acquire`/`Renew`/`Beat`/`Release` the maintenance election (the `SchedulePort` maintenance lease, the `Sandbox/provisioning#ROLLOVER_DRAIN` `FleetRoll` conductor, the `Wire/companion#PROCESS_MODALITY` sidecar write-forward) fences through, never a duplicate of `LeaseElection`'s static surface (`Acquire` subsumes crash-reclaim — a seat past a crash-stale holder is `Acquire`'s reclaim arm, not a fifth entry). `TenantContext`/`TenantId`, `FencingToken`, `CostVector`/`CostUnit`, `WorkflowInstance`/`WorkflowStep`/`StepKind`/`WorkflowStatus`/`StepStatus`, `OutboxRow`/`DeadLetterRow`/`DispatchStatus`, `DomainEvent`, `ClockPolicy`, and `ReceiptSinkPort` arrive settled as AppHost vocabulary and cross only the AppHost inward seam; the store mints no second changefeed — a relayed outbox row rides the one `Sync/collaboration#OPLOG_CHANGEFEED` `OpLog` the `Sync/egress#EGRESS_PUMP` pump drains. The `lease` `CoordCell` is the durable cross-process seat the AppHost `LeaseElection` acquires through its `AcquireLease` slot and fences server-side at this tier (the election's `Atom<FencingToken>` is the in-process cache, the durable seat is here); it is a distinct altitude from the per-store first-opener `Store/profiles#STORE_LIFECYCLE` `StoreLeaseRow` (the embedded-sqlite single-writer epoch lease), the two never duplicated — `StoreLeaseRow` arbitrates one embedded file's writer, the `lease` cell arbitrates a fleet-wide role across nodes through the same `FencingToken` the resource re-checks.

Wire posture: this page is host-local — the fenced CAS runs server-side on the self-provisioned PostgreSQL tier under RLS, and the embedded-sqlite floor maps the same conditional upsert onto its `version`-token write path, so the coordination store crosses no browser or peer wire and carries no `TS_PROJECTION` cluster. The three delegate records (`DistributedBudget`, `StepStateSeam`, `OutboxRelay.Runtime`) plus the static `LeaseElection.AcquireLease` slot cross only the AppHost inward seam as settled `Fin`-returning slots, each bridged from this page's `IO<Fin<…>>` at the binding; the relayed outbox payload reaches external consumers only through the settled `Sync/egress#EGRESS_PUMP` CloudEvents projection, never a second egress shape minted here.

## [01]-[INDEX]

- [01]-[FENCED_CAS]: the `CoordCell` fenced conditional-upsert shape, the `CoordKind` axis, the `CoordPayload` codec, the `CoordQuery` read family, and the `CoordStore.Cas` atomic gate under `TenantId` RLS.
- [02]-[BUDGET_LEDGER]: the per-tenant `QuotaLedger` row satisfying `DistributedBudget` — `Spent` read, the ceiling-inside-CAS `Debit`, the window roll, and the `Token` mint.
- [03]-[STEP_STATE]: the workflow step-state CAS row satisfying `StepStateSeam` — `Persist`/`Rehydrate`/`InFlight` over wire-stable keys plus the resume cursor.
- [04]-[OUTBOX_TABLE]: the transactional-outbox durable backing for the AppHost `OutboxRelay.Runtime` — `Pending`/`Advance`/`DeadLetter`, committing same-transaction with the step-state row and relaying over the one op-log.
- [05]-[LEASE_MEMBERSHIP]: the distributed maintenance-lease/membership cell the `LeaseElection.Runtime.AcquireLease` slot binds to — `Acquire`/`Renew`/`Beat`/`Release` with epoch-as-fence, TTL deadline, per-member heartbeat liveness, and the self-compacting membership roster (`Acquire` subsumes crash-reclaim, stamping the `reclaimed` fact when it seats past a crashed holder).

## [02]-[FENCED_CAS]

- Owner: `CoordKind` the `[SmartEnum<string>]` coordination-row axis (`budget` | `step-state` | `outbox` | `dead-letter` | `lease`) under the `SyncKeyPolicy` ordinal accessor, each row carrying its `Singleton` flag (one cell per tenant vs one per key); `CoordCell` the one fenced-CAS row carrying the `(TenantId, CoordKind, CoordKey)` identity, the `FencingToken` column, the `xmin` token, the `jsonb` payload, and the one `Read<T>`/`Write` codec; `CoordQuery` the `[Union]` read-shape family (`Point` | `Page` | `Sweep`) one polymorphic `Read` discriminates; `CoordFault` the `[Union]` fault family in the 7100 band; `CoordFact`/`CoordFactKind` the one coordination fact stream; `CoordStore` the static fenced-CAS surface.
- Cases: `CoordKind` `budget` (the per-tenant quota cell, singleton), `step-state` (one row per `WorkflowInstance`), `outbox` (one row per pending `DomainEvent`), `dead-letter` (a poison outbox row past its attempt budget), `lease` (the maintenance-lease/membership cell the AppHost `LeaseElection.Runtime.AcquireLease` slot seats for a fleet-wide role, singleton per role key); `CoordQuery` `Point(key)` (one addressed cell), `Page(kind)` (a kind's rows for the `InFlight`/roster set), `Sweep(kind, watermark, lockMode)` (outbox dequeue past the watermark `FOR UPDATE SKIP LOCKED`); a write whose `FencingToken` is strictly below the row's accepted token returns zero rows and rejects `Fenced` — the fenced `MergeWithOutputAsync` is the SOLE write-ordering guard (it bypasses `SaveChangesAsync`, so no EF `xmin` optimistic check fires on the CAS); the `xmin` the `Point` read returns is the read-snapshot token a caller threads through a read-modify-CAS so a `transition` that observes a changed snapshot folds `Stale` itself rather than clobbering a concurrent commit.
- Entry: `public static IO<Fin<CoordCell>> Cas(Runtime runtime, TenantContext tenant, CoordKind kind, string key, FencingToken token, Func<Option<CoordCell>, Fin<CoordCell>> transition)` — the one gate: it runs the kind's conditional upsert through the `Transactions.Scoped` serializable transaction under the tenant RLS GUC, applying the `transition` fold to mint the next cell (the `budget` ceiling and `outbox` attempt-budget rejects live INSIDE the fold so `spent + cost > ceiling` rejects `CeilingExceeded` before the write), then emits a `INSERT … ON CONFLICT DO UPDATE … WHERE fence_token <= excluded.fence_token RETURNING *` so the fence reject-lower is the SQL predicate and a zero-row return folds to `Fenced` — the gate is the single statement, never a read-then-write; `public static IO<T> Read<T>(Runtime runtime, TenantContext tenant, CoordKind kind, CoordQuery query, Func<Seq<CoordCell>, T> project)` is the one polymorphic read — `Point` reads one cell past the RLS predicate without locking, `Page` reads a kind's rows, `Sweep` dequeues outbox rows past the watermark `FOR UPDATE SKIP LOCKED` ordered by the HLC `Logical`, all three discriminating on the `CoordQuery` case so `Spent`/`Rehydrate`/`InFlight`/`Pending`/roster share one entry rather than four read names.
- Auto: every coordination mutation is the one `Cas` conditional upsert so the budget debit, the step-state commit, the outbox enqueue, and the lease acquire share one atomic-gate semantic, never five CAS implementations; the fence is the AppHost `FencingToken` reject-lower pushed into the `DO UPDATE … WHERE` predicate so two nodes racing a write cannot both win — the store serializes the writes and rejects the lower token in one statement, the Kleppmann reject-lower a timeout-only lease cannot give; because every advancing write mints a fresh fence token the one guard subsumes each kind's monotonicity (the outbox watermark, the budget spent vector, the lease epoch), so a stale node's advance carries a lower token and rejects without a per-kind SQL column guard; the `transition` fold runs in-process before the statement to compute the next payload and reject the kind invariant the value cannot express in the fence (`CeilingExceeded`, `Exhausted`); the `xmin` system column the `Point` read surfaces is the optimistic read-snapshot token a read-modify-CAS caller carries — the `transition` compares the cell's `Xmin` against the snapshot it read and folds `CoordFault.Stale` when a concurrent commit advanced it, so the optimistic check lives in the in-process fold (the fenced merge bypasses `SaveChangesAsync`, so no EF `DbUpdateConcurrencyException`/`StoreFault.Concurrency` fires on the CAS path and the fence remains the sole write-ordering guard); the RLS predicate is the `Store/tenancy#TENANCY_RLS` `current_setting('rasm.tenant')::uuid` policy so a tenant reads and writes only its own coordination rows and a cross-tenant read returns the empty set by construction; the `coord_cell` table creation and its `(tenant_id, kind, coord_key)` unique constraint pin to the provisioning migration bundle so the upsert is the single arbiter across every node; the upsert lowers through the one `Query/rail#BULK_LANE` `MergeWithOutputAsync` path the package's bulk lane already owns — the fence reject-lower is the `Merge().On(identity).InsertWhenNotMatched(...).UpdateWhenMatchedAnd(target => target.FenceToken <= source.FenceToken, ...)` conditional-update arm and the `RETURNING *` is the `MergeWithOutputAsync` projection, so a zero-row stream IS the typed `Fenced` (the linq2db `InsertOrUpdate` is the rejected spelling because it carries neither the conflict `WHERE` nor the `RETURNING` image); the sqlite floor maps the same conditional upsert onto its `version`-token `ON CONFLICT (...) DO UPDATE ... WHERE version <= excluded.version` write under the writer lease so the embedded single-process store satisfies the same contracts without a second code path; `CoordPayload.Write<T>`/`Read<T>` is the one `jsonb` codec the five kinds share so a per-kind encode/decode method pair is the deleted form, and a malformed payload rails through `Option`/`CoordFault.Corrupt` rather than a null-forgiving `Deserialize<T>(…)!`.
- Receipt: every fenced commit, reject, and read rides the one `CoordFact` stream keyed by `CoordFactKind` (`committed` | `fenced` | `stale` | `ceiling` | `dead-letter` | `acquired` | `released` | `reclaimed`) carrying the kind, the key, the accepted token, and the elapsed `Duration`, so a per-kind receipt scatter is the deleted form; the per-kind sub-sections keep only the typed evidence the AppHost owner mints (the AppHost `DeliveryReceipt`, the step `CommandReceipt`, the `BudgetReceipt`) and never a generic `IReceipt` — the durable-commit fact stays the `CoordFact`, the domain evidence stays with the AppHost owner that mints it.
- Packages: Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.Data.Sqlite, linq2db.EntityFrameworkCore, System.Text.Json, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new coordination concern is one `CoordKind` row (its `Singleton` flag and `Invariant` fragment) plus one `transition` fold the `Cas` gate runs, never a second table or a second CAS surface; a new read shape is one `CoordQuery` case; a new fault is one `CoordFault` case; a new evidence bucket is one `CoordFactKind` row; zero new surface — a per-concern coordination table, a second fencing-token column, a per-kind codec pair, a parallel read name, or a CAS loop in the caller is the deleted form because the gate is one store-side conditional upsert under one unique constraint.
- Boundary: `CoordStore` is the suite's only durable cross-process coordination owner — a per-tenant quota service, a workflow-instance table that bypasses the fence, a second transactional-message store, and a second distributed-lock store are the deleted forms; the store satisfies the three AppHost delegate records (`DistributedBudget`, `StepStateSeam`, `OutboxRelay.Runtime`) by implementing their `Func<…, Fin<…>>` slots over `CoordCell` (running each durable `IO<Fin<…>>` to the seam's `Fin` at the binding) and binds the durable lease CAS the static `LeaseElection.Runtime` consumes, never reversing the dependency — AppHost names the seam, owns the runtime policy, and OWNS `OutboxRow`/`DeadLetterRow`/`DispatchStatus`/`WorkflowStatus`/`StepStatus`; this page provides the durable CAS backing and the wire-stable projections only; the fence is the AppHost `FencingToken` checked store-side in the `DO UPDATE … WHERE` predicate so the correctness proof is the single conditional statement, never a held-lease-without-fence (the rejected form) and never a `SELECT … FOR UPDATE` then write (the read-then-write the prose must not dress as atomic); the ceiling check executes inside the `budget` `transition` fold and the SQL guard within the one statement so the multi-node ceiling overshoot a per-process gate would open is foreclosed; the `TenantId` RLS predicate is the `Store/tenancy#TENANCY_RLS` policy so coordination rows are tenant-scoped by construction and never a coarse table-level grant; the store mints no second changefeed — a relayed outbox row rides the one `Sync/collaboration#OPLOG_CHANGEFEED` op-log the `Sync/egress#EGRESS_PUMP` pump drains, so a second egress table or a re-minted `CdcEnvelope` is the drift defect the `[ONE_OUTBOX_EGRESS_SPINE]` branch forecloses; the database stays excluded from the AppHost hop law — `EnableRetryOnFailure` on the pg row reruns the serialization-failure-aborted `Cas` closure and is the only retry owner.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class CoordKind {
    public static readonly CoordKind Budget = new("budget", singleton: true);
    public static readonly CoordKind StepState = new("step-state", singleton: false);
    public static readonly CoordKind Outbox = new("outbox", singleton: false);
    public static readonly CoordKind DeadLetter = new("dead-letter", singleton: false);
    public static readonly CoordKind Lease = new("lease", singleton: true);

    public bool Singleton { get; }      // one cell per tenant (budget/lease role) vs one per key
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordQuery {
    private CoordQuery() { }

    public sealed record Point(string Key) : CoordQuery;                                 // one addressed cell, no lock
    public sealed record Page : CoordQuery;                                              // all rows of a kind past RLS
    public sealed record Sweep(ulong Watermark, LockMode Lock) : CoordQuery;             // outbox dequeue, SKIP LOCKED
}

// --- [MODELS] --------------------------------------------------------------------------

// One fenced-CAS row: (TenantId, CoordKind, CoordKey) identity, the FencingToken column, the xmin
// optimistic token, and the jsonb payload. The per-kind projections never re-encode — they ride
// CoordPayload.Write/Read, the one codec, so a malformed payload rails rather than null-forgives.
public sealed record CoordCell(
    TenantId Tenant,
    CoordKind Kind,
    string Key,
    FencingToken Token,
    uint Xmin,
    JsonElement Payload,
    Instant At) {
    public static CoordCell Empty(TenantId tenant, CoordKind kind, string key) =>
        new(tenant, kind, key, FencingToken.Zero, 0U, default, Instant.MinValue);

    public bool Admits(FencingToken incoming) => Token.Admits(incoming);

    public Fin<T> Read<T>() => CoordPayload.Read<T>(Payload);
    public CoordCell Write<T>(T value, FencingToken token, Instant at) =>
        this with { Payload = CoordPayload.Write(value), Token = token, At = at };
}

// The one jsonb payload codec the five kinds share. Every payload (QuotaLedger, StepStateRow,
// the AppHost OutboxRow/DeadLetterRow, LeaseRow) carries Thinktecture-generated owners —
// CostVector over the CostUnit [SmartEnum], FencingToken/[ValueObject], WorkflowStatus/DispatchStatus
// [SmartEnum] — so the factory partition is load-bearing: ThinktectureJsonConverterFactory at
// position 0 claims every value-object/smart-enum/keyed-union by its key, then ConfigureForNodaTime
// appends the semantic-time converters (Instant/Duration), the two factories partitioning the type
// space with no collision (api-thinktecture-json#CROSS_CODEC, api-nodatime-stj). A deserialize that
// returns null is the boundary reject, never propagated — BOUNDARY_ADMISSION admits the payload once
// or rails CoordFault.Corrupt.
public static class CoordPayload {
    static readonly JsonSerializerOptions Options = Configured();

    public static JsonElement Write<T>(T value) => JsonSerializer.SerializeToElement(value, Options);
    public static Fin<T> Read<T>(JsonElement payload) =>
        JsonSerializer.Deserialize<T>(payload, Options) is { } value
            ? Fin.Succ(value)
            : Fin.Fail<T>(new CoordFault.Corrupt(typeof(T).Name));

    static JsonSerializerOptions Configured() {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true));
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);   // appended after the factory at position 0
        return options;
    }
}

public readonly record struct CoordFact(CoordFactKind Kind, string Coord, string Key, ulong Token, Duration Elapsed, Instant At);

[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class CoordFactKind {
    public static readonly CoordFactKind Committed = new("committed");
    public static readonly CoordFactKind Fenced = new("fenced");
    public static readonly CoordFactKind Stale = new("stale");
    public static readonly CoordFactKind Ceiling = new("ceiling");
    public static readonly CoordFactKind DeadLetter = new("dead-letter");
    public static readonly CoordFactKind Acquired = new("acquired");
    public static readonly CoordFactKind Renewed = new("renewed");
    public static readonly CoordFactKind Released = new("released");
    public static readonly CoordFactKind Reclaimed = new("reclaimed");
    public static readonly CoordFactKind Evicted = new("evicted");
}

// --- [ERRORS] --------------------------------------------------------------------------

[Union]
public abstract partial record CoordFault : Expected, IValidationError<CoordFault> {
    private CoordFault(string detail, int code) : base(detail, code, None) { }
    public static CoordFault Create(string message) => new Text(message);

    public sealed record Text : CoordFault { public Text(string detail) : base(detail, 7100) { } }
    public sealed record Fenced : CoordFault { public Fenced(string detail) : base(detail, 7101) { } }
    public sealed record CeilingExceeded : CoordFault { public CeilingExceeded(CostUnit unit, long over) : base($"{unit.Key}:+{over}", 7102) { Unit = unit; Over = over; } public CostUnit Unit { get; } public long Over { get; } }
    public sealed record Stale : CoordFault { public Stale(string detail) : base(detail, 7103) { } }
    public sealed record Missing : CoordFault { public Missing(string detail) : base(detail, 7104) { } }
    public sealed record Corrupt : CoordFault { public Corrupt(string detail) : base(detail, 7105) { } }
    public sealed record Held : CoordFault { public Held(string holder, ulong epoch) : base($"{holder}@{epoch}", 7106) { Holder = holder; Epoch = epoch; } public string Holder { get; } public ulong Epoch { get; } }
    public sealed record Expired : CoordFault { public Expired(string detail) : base(detail, 7107) { } }
    public sealed record Exhausted : CoordFault { public Exhausted(int attempts) : base($"attempts:{attempts}", 7108) { Attempts = attempts; } public int Attempts { get; } }
}

// --- [SERVICES] ------------------------------------------------------------------------

public static class CoordStore {
    public sealed record Runtime(
        PooledDbContextFactory<StoreDb> Contexts,
        InterceptPolicy Policy,
        ClockPolicy Clocks,
        DeadlineClass Deadline);

    // --- [OPERATIONS] ------------------------------------------------------------------

    // The one atomic gate. The transition fold mints the next cell and rejects the kind invariant
    // (CeilingExceeded/Exhausted) in-process — and, for a read-modify-CAS caller, folds Stale when the
    // located cell's Xmin advanced past the snapshot it captured; the conditional upsert pushes the
    // fence reject-lower into DO UPDATE … WHERE, so a zero-row RETURNING IS the typed Fenced — and
    // because every advancing write mints a fresh token, that one fence guard is each kind's monotonicity
    // proof and the sole write-ordering control — one statement, never a SELECT … FOR UPDATE then write.
    public static IO<Fin<CoordCell>> Cas(
        Runtime runtime, TenantContext tenant, CoordKind kind, string key,
        FencingToken token, Func<Option<CoordCell>, Fin<CoordCell>> transition) =>
        Transactions.Scoped(
            runtime.Contexts,
            // The fenced conditional upsert is the write — its `(tenant_id, kind, coord_key)` unique
            // constraint plus the `fence_token <= excluded.fence_token` reject-lower IS the serializer,
            // so the scope needs no `SELECT … FOR UPDATE` row lock; the coarse `ROW EXCLUSIVE` table grant
            // is the declaration the bracket receipts. The CAS replays bit-stable under serialization retry
            // (a re-run reads the prior commit's accepted token and rejects-or-no-ops), so the tail is
            // `RetrySafety.Idempotent` and `verifySucceeded` is `null` — never the double-apply `NonRetryable`.
            TxnScope.Serializable(LockMode.On(TableLockMode.RowExclusiveTable, "coord_cell"), new RetrySafety.Idempotent()),
            // The Maintain arm carries the raw conditional-upsert statement: it runs the linq2db
            // MergeWithOutputAsync RETURNING write (UpdateWhenMatchedAnd carrying the fence reject-lower)
            // itself and never triggers the Upsert arm's SaveChangesAsync, because the fenced CAS is one
            // provider MERGE statement over the bridged DataConnection, not an EF tracked graph.
            new StoreOp<Fin<CoordCell>>.Maintain(async (db, ct) => {
                var current = await Locate(db, tenant.TenantId, kind, key, ct);
                // The transition fold (per-kind: QuotaLedger.Debit / LeaseRow.Acquire / row.Relayed) returns
                // the next cell with its jsonb payload already written; Cas stamps the fence token and clock.
                // ON CONFLICT … WHERE fence_token <= excluded RETURNING * — a zero-row return folds to Fenced.
                return await transition(current)
                    .Map(next => next with { Token = token, At = runtime.Clocks.Now })
                    .Match(
                        Succ: cell => Conditional(db, kind, cell, ct),
                        Fail: fault => new ValueTask<Fin<CoordCell>>(Fin.Fail<CoordCell>(fault)));
            }, $"coord.cas.{kind.Key}"),
            runtime.Policy, runtime.Clocks, runtime.Deadline);

    // The one polymorphic read; the CoordQuery case selects point / kind-page / watermark-sweep so
    // Spent, Rehydrate, InFlight, Pending, and the roster share one entry, never four read names.
    public static IO<T> Read<T>(
        Runtime runtime, TenantContext tenant, CoordKind kind, CoordQuery query, Func<Seq<CoordCell>, T> project) =>
        query switch {
            CoordQuery.Sweep s => Transactions.Scoped(
                runtime.Contexts, new TxnScope(IsolationPolicy.ReadCommitted, Some(s.Lock), Seq<Savepoint>(), new RetrySafety.Idempotent(), Prepared: false, None),
                new StoreOp<T>.Query(async (db, ct) => project(await SweepAfter(db, tenant.TenantId, kind, s.Watermark, ct))),
                runtime.Policy, runtime.Clocks, runtime.Deadline),
            _ => StoreRail.Run(
                runtime.Contexts,
                new StoreOp<T>.Query(async (db, ct) => project(await Select(db, tenant.TenantId, kind, query, ct))),
                runtime.Policy, runtime.Clocks, runtime.Deadline),
        };
}
```

The `Cas` fold and the `Read` family are the only two entrypoints; the conditional-upsert statement (`Conditional`, a `MergeWithOutputAsync` over the bridged `CreateLinqToDBConnection` data context) and the read shapes (`Locate` for the single cell, `Select` for point/page, `SweepAfter` for the watermark dequeue) are owner-local statement builders the `transition` and `project` delegates compose, never public surfaces. The `Conditional` builder always emits the one `UpdateWhenMatchedAnd(target => target.FenceToken <= source.FenceToken, ...)` guard arm, and because every advancing write mints a fresh fence token that one guard is every kind's monotonicity proof — so no per-kind SQL column guard and no per-call literal enter the statement, and a `MergeWithOutputAsync` that streams zero rows IS the `Fenced` reject without a second read.

## [03]-[BUDGET_LEDGER]

- Owner: `QuotaLedger` the per-tenant quota cell projected from the `budget`-kind `CoordCell` payload (the spent `CostVector` plus the resolved ceiling plus the window bound); `BudgetReceipt` the typed debit evidence; the `DistributedBudget` triple this row satisfies — the AppHost `Agent/capability#GRANT_BROKER` opt-in seam the broker debits a durable per-tenant budget through. The Persistence projection is named `QuotaLedger` because the AppHost `Budget` is the broker's per-process live-metering cell at `Agent/capability#GRANT_SCOPE` — one canonical name per concept, the durable projection distinct from the in-process cell, never a colliding second `Budget`.
- Cases: the `budget` cell is one row per `TenantId` (the `CoordKey` is the tenant slug, the kind `Singleton`) so the broker meters each tenant independently against one store; a `Debit` whose `spent + cost` exceeds the `ceiling` rejects `CeilingExceeded` with the offending `CostUnit` named, a debit carrying a stale token rejects `Fenced`, a debit crossing the window bound rolls the spent vector to zero before metering, and a fresh-token in-ceiling debit commits the advanced spent vector.
- Entry: the AppHost `GrantBroker` binds its settled `DistributedBudget(Spent, Debit, Token)` slots — `Func<TenantId, Fin<CostVector>>`, `Func<TenantId, FencingToken, CostVector, CostVector, Fin<CostVector>>`, `Func<TenantId, Fin<FencingToken>>` — from this surface, each slot running its `CoordStore` `IO` to the bare `Fin` the seam carries through `.Run(EnvIO.New(...))` at the binding seam (the seam slot is synchronous `Fin`, never `IO`, so the bridge runs here, never leaks `IO` to AppHost). `Spent` projects `CoordStore.Read` of the `budget` cell `Point` to the decoded spent vector (the dry-run pre-flight price, no lock); `Debit(TenantId, FencingToken token, CostVector cost, CostVector ceiling)` — the FOUR-slot seam shape, carrying NO `window` parameter — runs `CoordStore.Cas` supplying the `QuotaLedger.Debit` `transition` fold that rolls the spent vector when the broker-supplied `GrantScope.Window` bound stamped on the cell advances, adds the cost, and rejects `CeilingExceeded` when `next.Of(unit) > ceiling.Of(unit)` for any metered unit else returns the advanced cell so the ceiling gate executes inside the one statement; the window bound is the broker's `GrantScope.Window` already stamped on the prior cell, not a `Debit` argument, so the four-arg seam stays settled and the roll reads the cell's own window rather than a phantom signature; `Token` projects the cell's `FencingToken.Next()` so the broker presents the next monotone token on its debit. `QuotaLedger` and `BudgetReceipt` are the Persistence-owned projections; `CostVector`/`CostUnit`/`DistributedBudget`/`GrantScope.Window` are AppHost types this page reads, never builds.
- Auto: the ceiling check is the `transition` fold's own arm so the atomic conditional upsert decides `spent + cost <= ceiling` under the fence predicate — two nodes presenting fresh tokens cannot both overshoot because the store serializes the writes and the second reads the first's committed spent vector and rejects, the fleet-wide ceiling enforcement a per-process gate cannot give; the `CostVector` is the AppHost `HashMap<CostUnit, long>` so each metered resource caps independently and a command under the `Calls` ceiling but over the `BytesEgress` ceiling rejects on bytes-egress with the unit named; the metered set is the `CostUnit` smart-enum's own `Items` so a new `CostUnit` row meters automatically with no hand-kept `Seq`; the spent vector resets on the window roll the broker's `GrantScope.Window` carries (the fold rolls to zero when the incoming window bound advances past the cell's window) so a new window starts at zero spent without a second cell; the model-governance charge, the plugin `GrantHandle` charge, and the operator call all debit this one `budget` cell so a multi-node identity plane cannot let a tenant exceed its ceiling N-fold.
- Receipt: a debit rides the `CoordFact` `committed`/`ceiling` kind carrying the tenant, the token, and the elapsed; the typed `BudgetReceipt(Tenant, Charged, Spent, Ceiling, FencingToken, Instant)` carries the debit evidence the broker chains; an over-ceiling debit is the typed `CoordFault.CeilingExceeded` mapping to the AppHost `GrantFault.CeilingExceeded`, never a silent clamp.
- Packages: Npgsql, linq2db.EntityFrameworkCore, System.Text.Json, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new metered resource rides the AppHost `CostUnit` axis so the ledger absorbs it through `CostUnit.Items` without a schema change; a new quota concern is one field on the `QuotaLedger` payload; zero new surface — a second quota table or a hand-kept metered list is the deleted form.
- Boundary: the `budget` cell is the AppHost `GrantBroker`'s opt-in durable backing, never a second meter — with no seam bound the broker debits its per-process `Budget`, and this store is the cross-process upgrade the one broker entry consumes through the `DistributedBudget` triple; the ceiling gate is store-side inside the `Cas` conditional upsert so the AppHost gates the ceiling outside the fenced write ONLY for the dry-run pre-flight off `Spent`, foreclosing the read-then-write TOCTOU a multi-node per-process gate would open; the `budget` cell is the `ONE_FENCED_LEASE_STORE` leg the AppHost `DEEPEN_DISTRIBUTED_QUOTA_STORE_SEAM` and `STRUCTURE_SANDBOX_GRANT_MEDIATION` cards bind, consumed at the `Agent/capability ⇄ Rasm.AppHost/Runtime # [PORT]: fenced per-tenant Budget debit` seam — Persistence owns the durable ledger and the fenced CAS, AppHost owns the metering policy and the broker entry; the projection is `QuotaLedger`, never a second `Budget` colliding with the broker cell.

```csharp signature
// The Persistence-owned durable quota projection the `budget`-kind CoordCell payload encodes. Debit is
// the window-rolling, ceiling-gating transition fold the seam supplies to CoordStore.Cas — the `window`
// and `ceiling` are the broker's GrantScope.Window/ceiling resolved at the DistributedBudget binding (NOT
// Debit seam arguments, which carry only cost+ceiling), so the roll and `spent + cost <= ceiling` decide
// INSIDE the one fenced upsert against the cell's own stored window rather than a phantom signature slot.
public sealed record QuotaLedger(CostVector Spent, CostVector Ceiling, Instant Window) {
    public Fin<QuotaLedger> Debit(CostVector cost, Instant window) =>
        (window > Window ? this with { Spent = CostVector.Zero, Window = window } : this) is var rolled
            && rolled.Spent.Add(cost) is var next
            ? CostUnit.Items.Find(unit => next.Of(unit) > Ceiling.Of(unit)).Match(
                Some: unit => Fin.Fail<QuotaLedger>(new CoordFault.CeilingExceeded(unit, next.Of(unit) - Ceiling.Of(unit))),
                None: () => Fin.Succ(rolled with { Spent = next }))
            : Fin.Succ(rolled);

    public static QuotaLedger Seed(Option<CoordCell> cell, CostVector ceiling, Instant window) =>
        cell.Match(
            Some: static c => c.Read<QuotaLedger>().IfFail(_ => new QuotaLedger(CostVector.Zero, default, default)) with { Ceiling = ceiling, Window = window },
            None: () => new QuotaLedger(CostVector.Zero, ceiling, window));
}

public sealed record BudgetReceipt(TenantId Tenant, CostVector Charged, CostVector Spent, CostVector Ceiling, FencingToken Token, Instant At);
```

## [04]-[STEP_STATE]

- Owner: the `step-state`-kind `CoordCell` carrying the wire-stable `WorkflowInstance` projection (the `WorkflowStatus` key, the resume cursor, and each committed step's `StepKind` payload), satisfying the AppHost `Runtime/orchestration#STEP_STATE_SEAM` `StepStateSeam` triple; `StepStateRow` the durable projection record; `StepPayload` the wire-stable serialization of one `WorkflowStep`; the row commits same-transaction with the `outbox` row under one tenant-scoped transaction.
- Cases: one `step-state` cell per `WorkflowInstance` (the `CoordKey` is the instance id); a `Persist` carrying a `FencingToken` strictly below the instance's accepted token rejects `Fenced` so a resumed stale instance presenting a lower token fails rather than double-committing; the cell carries only wire-stable keys plus the resume cursor — the instance id, the workflow id, the `WorkflowStatus`, the cursor, and each committed step's `StepKind` payload (the `CommandIntent` descriptor + serialized arguments for an `Activity`/`Compensation`, the fire instant for a `Timer`, the channel for a `Signal`, the `ScheduleEntry` for a `PersistentJob`), never a live closure.
- Entry: the AppHost orchestrator binds its settled `StepStateSeam(Persist, Rehydrate, InFlight)` slots — `Func<WorkflowInstance, WorkflowStep, FencingToken, Fin<Unit>>`, `Func<string, Fin<WorkflowInstance>>`, `Func<TenantContext, Fin<Seq<string>>>` (each a bare `Fin`, never `IO`) — from this surface, running each `CoordStore` `IO` to the seam's `Fin` through `.Run(EnvIO.New(...))` at the binding so the `IO` boundary stays inside Persistence. `Persist(WorkflowInstance, WorkflowStep, FencingToken) => Fin<Unit>` (the AppHost `StepStateSeam.Persist` slot shape) projects the advanced instance plus its just-committed step to a `StepStateRow` and runs `CoordStore.Cas` upserting the `step-state` cell under the fenced token (the same transaction the `outbox` enqueue rides when the step publishes a domain event); `Rehydrate(string instanceId) => Fin<WorkflowInstance>` projects `CoordStore.Read` of the `step-state` cell `Point` to the decoded `StepStateRow` the orchestrator reconstructs into a `WorkflowInstance` from the wire-stable bytes so a crash-surviving process resumes mid-saga; `InFlight(TenantContext) => Fin<Seq<string>>` projects `CoordStore.Read` `Page` filtered to the non-terminal `step-state` rows (`WorkflowStatus` running/suspended/compensating) to their instance ids so the orchestrator drives the resume sweep. `StepStateRow`/`StepPayload` are the Persistence-owned wire-stable projections; `WorkflowInstance`/`WorkflowStep`/`StepKind`/`WorkflowStatus`/`StepStatus`/`StepStateSeam` are AppHost types this page reads, never builds.
- Auto: each `Persist` is one fenced `Cas` so a committed step persists by conditional upsert carrying only wire-stable keys plus the resume cursor, and a `StepKind.Activity` carries a `CommandIntent` (descriptor + serialized arguments + caller modality), not a `Func`, so a step rehydrates from durable bytes; the fence is the instance's `FencingToken` so a resumed stale instance presenting a lower token fails `Fenced` (mapping to the orchestrator's `OrchestrationFault.FenceStale`) rather than double-committing — the same Kleppmann reject-lower the budget cell enforces; the `step-state` cell and the `outbox` cell upsert under one `Transactions.Scoped` transaction when the step publishes a domain event so a step commit and its event enqueue ride one transaction boundary and crash recovery is re-delivery for both; `Rehydrate` reads the cursor and the committed-step payloads so the executor folds the remaining steps from the resume cursor, never replaying committed steps; the `WorkflowStatus`/`StepStatus` keys are the AppHost smart-enum keys serialized by value so the durable row speaks the one lifecycle vocabulary, never a stringly-typed status set.
- Receipt: a step persist rides the `CoordFact` `committed` kind carrying the instance id and the accepted token; the step's own `CommandReceipt` (the executor's) and the instance `SpineLog` event stay the AppHost orchestrator's evidence — this owner adds the durable-commit fact, never a second workflow receipt; a fence-stale persist is the typed `CoordFault.Fenced` the orchestrator maps to its `OrchestrationFault.FenceStale`.
- Packages: Npgsql, linq2db.EntityFrameworkCore, System.Text.Json, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new step shape is one AppHost `StepKind` case the wire-stable payload carries (the projection serializes the union by its key) so the cell absorbs it without a schema change; a new instance status is one AppHost `WorkflowStatus` row; zero new surface — a per-workflow state-machine table is the deleted form.
- Boundary: the step-state cell is the `ONE_FENCED_LEASE_STORE` leg the AppHost `DURABLE_ORCHESTRATION_SPINE` and `SEAM_OUTBOX_AND_WORKFLOW_PERSISTENCE_TABLE` cards bind, consumed at the `Runtime/orchestration ⇄ Rasm.AppHost/Runtime # [PORT]: workflow step-state CAS` seam — the AppHost orchestrator writes through the `Persist`/`Rehydrate`/`InFlight` contract and the durable CAS row, the fenced-token column, and the `TenantId` RLS predicate stay here; the cell persists only wire-stable keys plus the resume cursor by conditional upsert, never a live closure, so a per-process workflow table that bypasses the fenced store is the rejected form; the `step-state` row and the `outbox` row commit under one tenant-scoped transaction so crash-durable step resumption and exactly-once-effective delivery share one durable boundary — coupling to the CAS + outbox contract, never the store interior; the orchestrator's saga compensation and step dispatch stay AppHost runtime policy, durability stays here; the durable status keys are the AppHost `WorkflowStatus`/`StepStatus` smart-enums, never a duplicated string set.

```csharp signature
// The Persistence-owned wire-stable step-state projection the `step-state`-kind CoordCell carries —
// keys plus the resume cursor, never a live closure. StepPayload is the wire-stable serialization of
// one AppHost WorkflowStep (its StepKind key plus the kind's payload), so a step rehydrates from
// durable bytes; Status rides the AppHost WorkflowStatus/StepStatus smart-enums, never a string set.
public sealed record StepPayload(StepStatus Status, int Index, string Kind, JsonElement Body);

public sealed record StepStateRow(
    string InstanceId,
    string WorkflowId,
    WorkflowStatus Status,
    int Cursor,
    Seq<StepPayload> Steps,
    FencingToken Fence) {
    public bool NonTerminal => Status != WorkflowStatus.Completed && Status != WorkflowStatus.Faulted;
}
```

## [05]-[OUTBOX_TABLE]

- Owner: the `outbox`-kind `CoordCell` carrying the AppHost `OutboxRow` projection and the `dead-letter`-kind cell carrying the AppHost `DeadLetterRow`, providing the durable backing for the AppHost `Wire/outbox#DISPATCH_SWEEP` `OutboxRelay.Runtime` triple; the dispatch-sweep cursor is the `(ConsumerId, Hlc)` watermark the `OutboxRow.Logical` payload carries and the fresh-token fence guards on advance, the dedup-key index is the unique `(tenant_id, dedup_key)` constraint. `OutboxRow`/`DeadLetterRow`/`DispatchStatus`/`MaxAttempts` are AppHost-owned (`Wire/outbox`) — this page persists them as `CoordCell` payloads and never mints a parallel row shape.
- Cases: one `outbox` cell per pending `DomainEvent` (the `CoordKey` is the AppHost `OutboxRow.OutboxId` = `{topic}:{idempotency-key}`); a row relayed advances to `DispatchStatus.Dispatched`, a row exhausting its `OutboxRow.MaxAttempts` budget transitions to a `dead-letter`-kind cell carrying the last fault and the attempt history so a poison message leaves the dispatch lane rather than blocking it; the watermark advance fences through the fresh-token `fence_token <= excluded.fence_token` guard in the `DO UPDATE … WHERE` (the advancing relay mints a fresh token) so two nodes cannot both advance it past one row.
- Entry: the AppHost `OutboxRelay.Runtime` record binds its three durable slots — `Pending: Func<TenantContext, ulong, Fin<Seq<OutboxRow>>>`, `Advance: Func<OutboxRow, FencingToken, Fin<ulong>>`, `DeadLetter: Func<DeadLetterRow, Fin<Unit>>` (the seam slots carry bare `Fin`, never `IO`) — from this surface's `IO<Fin<…>>` operations, each slot running the durable `IO` to the seam's `Fin` through `.Run(EnvIO.New(...))` at the binding so the relay loop sees a synchronous `Fin` and the `IO` boundary stays inside Persistence. `Pending` projects `CoordStore.Read` `Sweep` of the tenant's `outbox` cells past the watermark cursor ordered by HLC `Logical` (the `LockMode.SkipLocked` dequeue so two nodes do not relay the same row simultaneously) to the decoded `OutboxRow`s; `Advance` runs `CoordStore.Cas` marking the row `OutboxRow.Relayed` under the fresh fenced token (the advance's own monotonicity proof), returning the row's `Logical` watermark the relay reads off the committed cell — `Relayed` flips `Status` to `Dispatched` and the `Logical` carries forward unchanged, so the returned watermark is the cell's settled cursor position, never a re-minted value; `DeadLetter` upserts the `dead-letter` cell for an exhausted row so the AppHost relay's `Replay(runtime, outboxId)` re-enqueues it for one more attempt. The relay loop, the `EventBus.Dispatch`, the `OutboundHop` registration, and `Replay` are the AppHost half; this page is the durable backing.
- Auto: the `outbox` cell writes same-transaction with the producing write (the `step-state` `Cas` when a workflow step publishes, or any entity write that enqueues) so a domain event and its source state commit atomically — a crash between the state write and the event publish cannot lose the event because both ride one transaction, the transactional-outbox guarantee; the dispatch sweep reads pending rows `FOR UPDATE SKIP LOCKED` so the fleet-spread sweep across nodes never double-relays a row, and a successful relay flips the row to `DispatchStatus.Dispatched` under the fresh-token fence guard while the consumer's `(ConsumerId, Hlc)` sweep cursor advances monotonically past the row's fixed enqueue `Logical` so the next sweep excludes it — the row's `Logical` is the immutable enqueue stamp, the watermark is the consumer cursor, never the row mutating its own `Logical` — and a relayed row never re-relays, the at-least-once-with-watermark guarantee that, with the consumer-side dedup, is exactly-once-effective; the dedup key is the event's idempotency key and the unique `(tenant_id, dedup_key)` constraint makes a re-enqueued identical event the upsert no-op so the outbox dedup and the AppHost `DeliveryFanout` dedup are one cell, never two; a relayed row rides the one `Sync/collaboration#OPLOG_CHANGEFEED` op-log the `Sync/egress#EGRESS_PUMP` pump drains as one keyed `OutboundHop` consumer, never a second egress table or a re-minted `CdcEnvelope`; the durable leg is the at-least-once backbone, so a subscription whose in-process `BroadcastBlock` buffer is full when the AppHost bus fan offers misses the in-process copy and re-receives the event on this outbox sweep — the store-side cursor plus the dedup-key index is the durability backbone the bounded in-process bus is not.
- Receipt: a relayed row mints one AppHost `DeliveryReceipt` carrying the topic and the dispatched flag (the relay's own); the watermark advance rides the `CoordFact` `committed` kind carrying the advanced `Logical`; a dead-letter transition rides the `CoordFact` `dead-letter` kind carrying the attempt count and maps to the AppHost `OutboxFault.Exhausted`; no parallel relay receipt — the typed `DeliveryReceipt` stays the relay's evidence.
- Packages: Npgsql, linq2db.EntityFrameworkCore, System.Text.Json, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new dispatch status is one AppHost `DispatchStatus` row; a new relay target is one AppHost `OutboundHop` the topic binds; zero new surface — a per-topic outbox table, a parallel `OutboxRow`/`DeadLetterRow` mirror, a second egress table, or a second dedup map is the deleted form.
- Boundary: the outbox backing is the `ONE_OUTBOX_EGRESS_SPINE` leg the AppHost `EVENT_BUS_OUTBOX_FABRIC`, `SEAM_OUTBOX_AND_WORKFLOW_PERSISTENCE_TABLE`, and `DEEPEN_EVENTBUS_DATAFLOW_TOPOLOGY` cards bind, consumed at the `Wire/outbox ⇄ Rasm.AppHost/Runtime # [PORT]: transactional outbox same-tx` seam — the AppHost names the seam, owns the `OutboxRow`/`DeadLetterRow`/`DispatchStatus` shapes and the relay, the outbox row writes atomically with the producing transaction here, and the relay registers as one keyed `OutboundHop` consumer advancing its own `(ConsumerId, Hlc)` watermark over the op-log rather than re-minting the `CdcEnvelopeWire` or a second egress table; this page persists the AppHost row as a `CoordCell` payload and never mints a parallel mirror — the prior `OutboxRow`/`DeadLetterRow`/`MaxAttempts = 8` re-declaration with a stringly-typed `Status` was the duplicate-across-seam defect; the at-least-once delivery guarantee is THIS durable outbox leg's (outbox row + watermark + consumer dedup), NOT the in-process `BroadcastBlock` bus (bounded best-effort latest-value-to-slow-target), so a subscription re-receives a missed event on this sweep and the store-side dispatch cursor plus the dedup-key index is the durability backbone, never an in-process retry; the `outbox` row and the `step-state` row commit under one tenant-scoped transaction so exactly-once-effective delivery and crash-durable step resumption share one durable boundary; the relayed payload reaches external consumers only through the settled `Sync/egress#EGRESS_PUMP` CloudEvents projection so the cross-language consumer reads the one envelope vocabulary, never a shape minted here.

```csharp signature
// No row shape is declared here — OutboxRow / DeadLetterRow / DispatchStatus / MaxAttempts are AppHost
// (Wire/outbox) types. This page persists them as CoordCell payloads through CoordPayload and exposes the
// three durable IO<Fin<…>> operations; the OutboxRelay.Runtime binding runs each to the seam's bare Fin via
// .Run(EnvIO.New(...)), never leaking IO to AppHost. The watermark advance mints a fresh fence token so the
// one fence guard proves it never re-relays.
public static class OutboxBacking {
    public static IO<Fin<Seq<OutboxRow>>> Pending(CoordStore.Runtime store, TenantContext tenant, ulong watermark) =>
        CoordStore.Read(store, tenant, CoordKind.Outbox,
            new CoordQuery.Sweep(watermark, LockMode.ForUpdate(WaitPolicy.SkipLocked)),
            static rows => rows.Traverse(static c => c.Read<OutboxRow>()).As());

    public static IO<Fin<ulong>> Advance(CoordStore.Runtime store, TenantContext tenant, OutboxRow row, FencingToken token) =>
        CoordStore.Cas(store, tenant, CoordKind.Outbox, row.OutboxId, token,
            current => current.Match(
                Some: cell => Fin.Succ(cell.Write(row.Relayed(store.Clocks.Now), token, store.Clocks.Now)),
                None: () => Fin.Fail<CoordCell>(new CoordFault.Missing(row.OutboxId))))
            .Map(static fin => fin.Bind(static cell => cell.Read<OutboxRow>().Map(static r => r.Logical)));

    public static IO<Fin<Unit>> DeadLetter(CoordStore.Runtime store, TenantContext tenant, DeadLetterRow dead) =>
        CoordStore.Cas(store, tenant, CoordKind.DeadLetter, dead.OutboxId, FencingToken.Zero,
            current => Fin.Succ(current.IfNone(CoordCell.Empty(tenant.TenantId, CoordKind.DeadLetter, dead.OutboxId)).Write(dead, FencingToken.Zero, store.Clocks.Now)))
            .Map(static fin => fin.Map(static _ => unit));
}
```

## [06]-[LEASE_MEMBERSHIP]

- Owner: `LeaseRow` the durable maintenance-lease/membership cell projected from the `lease`-kind `CoordCell` payload (the holder identity, the epoch = `FencingToken`, the TTL deadline, the heartbeat instant, and the self-compacting membership roster); `MembershipRoster` the live-member set the lease commits, each member carrying its own `Heartbeat` so any fleet node refreshes independently; `LeaseLink` the durable `Acquire`/`Renew`/`Beat`/`Release`/`Roster` lease surface this page owns — `Acquire` is the one seat-and-reclaim fold (a free, expired, or crash-stale cell seats under a fresh epoch; the `reclaimed` fact distinguishes a seat past a crashed holder), so a separate `Reclaim` entry is the collapsed form. The AppHost `Runtime/time#FENCING_TOKEN` `LeaseElection` is a static `Acquire(Runtime) => Fin<FencingToken>`/`Fence` surface over a `Runtime(Atom<FencingToken> Latest, Func<LeasePolicy, Fin<Unit>> AcquireLease, LeasePolicy Lease)` whose single inward slot is `AcquireLease: Func<LeasePolicy, Fin<Unit>>` — `LeaseElection` is NOT a multi-`Func` record, and its `Atom<FencingToken> Latest` is the in-process cache of the token; the durable cross-node seat is THIS page's `lease` CAS. The election binds `AcquireLease` to `LeaseLink.Acquire` adapted to `Func<LeasePolicy, Fin<Unit>>` (the role/holder/ttl arrive from the election composition, the durable seat success maps to `Fin<Unit>`, the durable epoch in `LeaseRow` IS the fence the AppHost `Atom` mirrors via `.Swap(Next)`), here durable so a fleet-wide role survives node loss and a paused holder is fenced by epoch, not by a timeout alone.
- Cases: the `lease` cell is one row per role key (the `CoordKey` is the role name, the kind `Singleton` per role) so a maintenance role (the persistence-maintenance sweep, the `FleetRoll` conductor, the sidecar write-forward) elects one fleet holder; an `Acquire` by a free, expired, or crash-stale cell mints the next epoch and seats the holder (a seat past a holder stale beyond the `LeasePolicy.Maintenance` `CrashStaleness` window stamps the `reclaimed` fact, so the crash-reclaim is `Acquire`'s own reclaim arm rather than a parallel entry), an `Acquire` against a live held cell rejects `Held` carrying the current holder and epoch, a `Renew` by the seated holder bumps the deadline and heartbeat under the same epoch and compacts the roster (a crashed peer's stale entry drops as the `evicted` fact), a `Renew` past the deadline (the holder paused beyond TTL) rejects `Expired` so the holder re-acquires through `Acquire` minting a fresh epoch, a `Beat` by a non-holder fleet member refreshes its own roster seat under the holder's epoch without contending the seat, and a `Release` by the holder frees the cell.
- Entry: `LeaseLink` is the durable lease surface the `LeaseElection.Runtime.AcquireLease: Func<LeasePolicy, Fin<Unit>>` slot binds `LeaseLink.Acquire` into (the durable seat success folds to `Fin<Unit>`), each operation running its `IO<Fin<…>>` to the seam's `Fin` through `.Run(EnvIO.New(...))` at the binding — `Acquire(TenantContext, string role, string holder, Duration ttl) => IO<Fin<FencingToken>>` runs `CoordStore.Cas` on the `lease` cell with the `LeaseRow.Acquire` `transition` fold that seats the holder and mints `Token.Next()` when the cell is free, expired, or crash-stale and rejects `Held` otherwise, returning the minted epoch as the durable `FencingToken` the AppHost `Atom<FencingToken> Latest` caches and every fenced write re-checks — a seat past a crash-stale prior stamps `reclaimed` (the prior holder named) rather than `acquired`, so the same entry owns acquire and reclaim; `Renew(TenantContext, string role, string holder, FencingToken epoch, Duration ttl) => IO<Fin<FencingToken>>` runs `CoordStore.Cas` with the `LeaseRow.Renew` fold that bumps the deadline and heartbeat under the held epoch, compacts the roster (dropped peers stamping `evicted`), and rejects `Expired`/`Held` on a paused or stolen lease; `Beat(TenantContext, string role, string member, FencingToken epoch, Duration staleness) => IO<Fin<Unit>>` runs `CoordStore.Cas` with the `LeaseRow.Beat` fold that refreshes one non-holder member's roster seat under the holder's epoch so each fleet node's liveness is independent of the holder's renew, never contending the seat; `Release(TenantContext, string role, FencingToken epoch) => IO<Fin<Unit>>` frees the cell; `Roster(TenantContext, string role) => IO<Fin<MembershipRoster>>` projects `CoordStore.Read` `Point` to the live-member set so a membership consumer reads the seated fleet. `LeaseRow`/`MembershipRoster`/`LeaseLink` are the Persistence-owned projections; `FencingToken`/`LeaseElection`/`LeasePolicy.Maintenance` are AppHost types this page reads.
- Auto: the epoch IS the `FencingToken` so the lease and the fence are one monotone identity — an `Acquire` mints `Token.Next()` and the seated resource re-checks every fenced write through `FencingToken.Admits`, so a paused holder that resumes presents a stale epoch the fenced write rejects, the Kleppmann reject-lower a timeout-only lease cannot give; the TTL deadline is the heartbeat-renewed liveness bound and the `CrashStaleness` window is the reclaim bound so a healthy holder stays seated by heartbeat while a crashed holder's cell reclaims past staleness without manual intervention; the `Acquire`/`Renew`/`Beat` are all the one `Cas` conditional upsert so the seat decision is store-side atomic — two nodes racing an `Acquire` against a free cell cannot both seat because the unique constraint plus the fence guard admit one and reject the other `Held`; the membership roster commits in the same `lease` cell so the seated fleet and the holder are one durable fact, never a second membership table, and each member refreshes its own `Heartbeat` through `Beat` (or, for the holder, through `Renew`) so liveness is genuinely per-member rather than holder-only, while every `Renew`/`Beat` runs `MembershipRoster.Compact(now, staleness)` so a crashed peer is durably pruned from the roster on the next live member's heartbeat (the dropped ids stamping the `evicted` fact) and `Live(now, staleness)` is the read-time projection of that already-pruned set — a stale member never lingers and no external membership-sweep owner is needed; this `lease` cell is the fleet-wide role election distinct from the embedded-sqlite `StoreLeaseRow` single-file writer epoch — the two leases never duplicated, one arbitrates a file, one a fleet role.
- Receipt: an `Acquire` seating a free or expired cell rides the `CoordFact` `acquired` kind carrying the role, the holder, and the minted epoch, while an `Acquire` seating past a crash-stale prior rides `reclaimed` carrying the prior and the new holder — one entry, two facts on the `LeaseRow.Reclaimed` discriminant; a renew rides `renewed` carrying the bumped heartbeat so a renew is its own evidence, never folded into `committed`; a release rides `released`; a member pruned by the `Renew`/`Beat` `Compact` past its staleness window rides `evicted` carrying the dropped member id (the durable roster transition, not a read-time hide); a contested acquire is the typed `CoordFault.Held`, a paused-renew the typed `CoordFault.Expired`, never a silent steal.
- Packages: Npgsql, linq2db.EntityFrameworkCore, System.Text.Json, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project).
- Growth: a new fenced role is one `lease` cell keyed on the role name, never a second lease table or a second token type; a new membership concern is one field on the `MembershipRoster`; a new lease disposition is one `CoordFault` case (`Held`/`Expired` already seat the contention vocabulary); zero new surface.
- Boundary: the `lease` cell is the AppHost `LeaseElection` durable backing the maintenance-lease election at `SchedulePort`, the `Sandbox/provisioning#ROLLOVER_DRAIN` `FleetRoll` conductor election, and the `Wire/companion#PROCESS_MODALITY` sidecar write-forward each acquire through, consumed at the `Sync/coordination ⇄ Rasm.AppHost/Runtime # [PORT]: fenced-CAS lease/membership election` seam — AppHost names the election driver and the `FencingToken` rail, this page owns the durable seat, the epoch, the TTL, and the reclaim; the epoch is the `FencingToken` the resource re-checks so a held lease without a fenced token is the rejected form — the lease alone is not the correctness proof, the token re-check is; the `lease` cell is the fleet-wide role distinct from the `Store/profiles#STORE_LIFECYCLE` `StoreLeaseRow` embedded single-writer epoch, the two leases never collapsed into one shape because one arbitrates an embedded file's writer and one a fleet-wide role across nodes; the crash-reclaim past `CrashStaleness` is the same window the embedded lease uses but the seat is store-side under the fence, never a client-side timeout poll.

```csharp signature
// The Persistence-owned durable lease/membership projection the `lease`-kind CoordCell carries. The
// epoch IS the FencingToken — Acquire mints Token.Next() (and subsumes crash-reclaim: a seat past a
// crash-stale prior is its own arm, the reclaimed fact distinguishing it), the seated resource re-checks
// every fenced write through Admits, so a paused holder is fenced by epoch, not by a timeout alone. The
// roster is self-compacting and genuinely multi-member: each node refreshes its own heartbeat through
// Beat (the holder through Renew), and every Renew/Beat Compacts the stale tail so a crashed peer is
// durably evicted on the next live heartbeat — Live is the read-time projection of the pruned set.
public sealed record Member(string Id, Instant Heartbeat) {
    public bool Live(Instant now, Duration staleness) => now <= Heartbeat + staleness;
}

// The seated fleet. Beat refreshes ANY member's own heartbeat (seating it if absent), so each fleet
// node refreshes independently — the roster is genuinely multi-member-live, not holder-only. Compact
// drops every member stale past the window and reports the dropped ids, so eviction is a real durable
// transition the `evicted` fact stamps, not a read-time filter alone.
public sealed record MembershipRoster(Seq<Member> Members) {
    public MembershipRoster Beat(string id, Instant now) =>
        new(Members.Filter(m => m.Id != id).Add(new Member(id, now)));
    public (MembershipRoster Roster, Seq<string> Dropped) Compact(Instant now, Duration staleness) =>
        (new MembershipRoster(Members.Filter(m => m.Live(now, staleness))),
         Members.Filter(m => !m.Live(now, staleness)).Map(static m => m.Id));
    public Seq<string> Live(Instant now, Duration staleness) =>
        Members.Filter(m => m.Live(now, staleness)).Map(static m => m.Id);
}

public sealed record LeaseRow(
    string Role,
    string Holder,
    FencingToken Epoch,
    Instant Deadline,
    Instant Heartbeat,
    MembershipRoster Roster) {
    public bool Expired(Instant now) => now > Deadline;
    public bool Reclaimable(Instant now, Duration crashStaleness) => now > Heartbeat + crashStaleness;

    // Seat a free / expired / crash-reclaimable cell under a fresh epoch; reject Held on a live lease.
    // This one fold IS the acquire-and-reclaim surface — a crash-stale prior seats a new holder, the
    // `acquired`-vs-`reclaimed` distinction is a CoordFact kind off `Reclaimed`, not a second entry.
    public static Fin<LeaseRow> Acquire(Option<LeaseRow> current, string role, string holder, FencingToken epoch, Instant now, Duration ttl, Duration crashStaleness) =>
        current.Match(
            None: () => Fin.Succ(new LeaseRow(role, holder, epoch, now + ttl, now, new MembershipRoster(Seq(new Member(holder, now))))),
            Some: held => held.Expired(now) || held.Reclaimable(now, crashStaleness) || held.Holder == holder
                ? Fin.Succ(held with { Holder = holder, Epoch = epoch, Deadline = now + ttl, Heartbeat = now, Roster = held.Roster.Beat(holder, now) })
                : Fin.Fail<LeaseRow>(new CoordFault.Held(held.Holder, (ulong)held.Epoch)));

    // Renew under the held epoch; reject Expired on a paused lease, Held on a stolen one. The renew
    // compacts the roster so a crashed peer's stale entry is durably evicted on the next holder beat —
    // the dropped ids ride the `evicted` fact rather than lingering until a read-time filter hides them.
    public bool Reclaimed(Option<LeaseRow> prior) => prior.Exists(p => p.Holder != Holder && p.Holder.Length > 0);

    public Fin<(LeaseRow Row, Seq<string> Evicted)> Renew(string holder, FencingToken epoch, Instant now, Duration ttl, Duration staleness) =>
        Holder != holder || (ulong)Epoch != (ulong)epoch ? Fin.Fail<(LeaseRow, Seq<string>)>(new CoordFault.Held(Holder, (ulong)Epoch))
        : Expired(now) ? Fin.Fail<(LeaseRow, Seq<string>)>(new CoordFault.Expired($"{Role}@{(ulong)Epoch}"))
        : Roster.Beat(holder, now).Compact(now, staleness) is var (roster, dropped)
            ? Fin.Succ((this with { Deadline = now + ttl, Heartbeat = now, Roster = roster }, dropped))
            : Fin.Fail<(LeaseRow, Seq<string>)>(new CoordFault.Text(Role));

    // A non-holder fleet member beats its own roster seat under the holder's epoch — independent liveness
    // without contending the seat; the beat compacts so a stale peer drops on any member's heartbeat.
    public Fin<(LeaseRow Row, Seq<string> Evicted)> Beat(string member, Instant now, Duration staleness) =>
        Roster.Beat(member, now).Compact(now, staleness) is var (roster, dropped)
            ? Fin.Succ((this with { Roster = roster }, dropped))
            : Fin.Fail<(LeaseRow, Seq<string>)>(new CoordFault.Text(Role));

    // Release frees the cell back to a holder-empty sentinel so the next Acquire seats from None-equivalent;
    // the released holder drops from the roster through one Compact-equivalent filter.
    public LeaseRow Release(string holder) =>
        this with { Holder = "", Deadline = Instant.MinValue, Roster = new MembershipRoster(Roster.Members.Filter(m => m.Id != holder)) };
}
```

| [INDEX] | [POLICY]              | [VALUE]                                                  | [BINDING]                                                                  |
| :-----: | :-------------------- | :------------------------------------------------------- | :------------------------------------------------------------------------- |
|  [01]   | atomic gate           | one `CoordStore.Cas` conditional `ON CONFLICT DO UPDATE … WHERE` | fence + invariant in the SQL predicate; one statement, never a read-then-write |
|  [02]   | fence                 | AppHost `FencingToken.Admits` reject-lower in `DO UPDATE … WHERE` | monotone token store-side; a held lease without fence rejected             |
|  [03]   | ceiling               | `budget` `transition` checks `spent+cost<=ceiling`       | inside the CAS upsert; AppHost gates outside only for dry-run              |
|  [04]   | same-transaction      | `step-state` + `outbox` upsert one `Transactions.Scoped` | crash-durable resume + exactly-once-effective delivery share one boundary  |
|  [05]   | dedup                 | unique `(tenant_id, dedup_key)` constraint               | re-enqueue is the upsert no-op; one cell with `DeliveryFanout`, never two  |
|  [06]   | watermark             | `Logical` advanced under the fresh-token `fence_token <= excluded.fence_token` guard | `SKIP LOCKED` dequeue; relayed row never re-relays; one op-log, not a second table |
|  [07]   | lease epoch           | epoch = `FencingToken`; `Acquire` mints `Token.Next()`   | paused holder fenced by epoch; crash-reclaim past `CrashStaleness`         |
|  [08]   | tenancy               | `Store/tenancy#TENANCY_RLS` `current_setting('rasm.tenant')::uuid` | coordination rows tenant-scoped by construction; cross-tenant read empty   |
|  [09]   | one row owner         | `OutboxRow`/`DeadLetterRow`/`WorkflowStatus` are AppHost  | persisted as `CoordCell` payloads; never a parallel mirror minted here     |

## [07]-[RESEARCH]

- [CONDITIONAL_UPSERT_FENCE]: the `INSERT … ON CONFLICT (tenant_id, kind, coord_key) DO UPDATE SET … WHERE coord_cell.fence_token <= excluded.fence_token RETURNING *` single-statement gate over the linq2db `MergeWithOutputAsync` `UpdateWhenMatchedAnd` conditional-update path (the `Query/rail#BULK_LANE` upsert owner, NOT `InsertOrUpdate` which carries neither the conflict `WHERE` nor the `RETURNING` image) — whether the fence reject-lower predicate and the upsert execute as one atomic store-side statement so a zero-row `RETURNING` stream folds to `Fenced`, whether the fresh-token-per-advance discipline makes that one guard each kind's monotonicity proof (the watermark, the spent vector, the epoch needing no separate column guard), and a concurrent debit on a lower token rejects under PG serializable WITHOUT a `SELECT … FOR UPDATE` read, confirmed against the Forge-provisioned PG18 before the coordination fence pins; the `xmin` the `Point` read surfaces is the `StoreProfile.ConcurrencyToken` value the read-modify-CAS `transition` snapshots and folds to `CoordFault.Stale` in-process — NOT the `Query/transaction#SQLSTATE_CLASSIFIER` `StoreFault.Concurrency` `SaveChangesAsync` path, which the fenced merge bypasses — so the fence stays the sole write-ordering guard.
- [SKIP_LOCKED_SWEEP]: the `outbox` dispatch sweep `SELECT … FOR UPDATE SKIP LOCKED` dequeue past the `(ConsumerId, Hlc)` watermark and the fleet-spread distribution — whether two nodes sweeping concurrently never relay the same row and the watermark advance fences through the fresh-token `fence_token <= excluded.fence_token` guard, confirmed against the live PG tier so the at-least-once-with-watermark guarantee holds across nodes; the `LockMode.SkipLocked` clause is the `Query/transaction#TXN_SCOPE` work-queue-dequeue mechanism already settled.
- [LEASE_EPOCH_RECLAIM]: the `lease`-cell `Acquire`/`Renew`/`Beat` conditional upsert minting the epoch as `FencingToken.Next()` under the unique role constraint — whether two nodes racing an `Acquire` against a free cell seat exactly one (the loser rejecting `Held`), a `Renew` past the TTL deadline rejects `Expired`, an `Acquire` past the `LeasePolicy.Maintenance` `CrashStaleness` window seats a fresh holder minting a fresh epoch and stamps `reclaimed` (the same entry reclaiming, not a parallel surface), and a non-holder `Beat` plus the `Renew`/`Beat` `Compact` durably prune a crashed peer (stamping `evicted`), confirmed against the live PG tier so the fleet-wide role election and the multi-member roster hold; the `FencingToken.Admits` re-check is the `Runtime/time#FENCING_TOKEN` rail already settled.
- [COORD_RLS_PREDICATE]: the `coord_cell` `current_setting('rasm.tenant')::uuid` RLS policy keyed on the `tenant_id` column — whether the `Store/tenancy#TENANCY_RLS` `CREATE POLICY` extends to the coordination table so a tenant reads and writes only its own budget/step-state/outbox/lease rows and a cross-tenant read returns the empty set, confirmed against the provisioned tenant GUC before the seam pins.
