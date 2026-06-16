# [PERSISTENCE_QUERY_RAIL]

Every store operation in Rasm.Persistence executes through one typed dispatch: the eight-case `StoreOp<T>` union runs inside a pooled-context bracket, converts provider failure into the `StoreFault` rail exactly once, and self-emits cache invalidation on the paths SaveChanges interceptors never see. The page owns the operation algebra, the projection and keyset-page shapes, the linq2db bulk lane with its delta projection, and the four-hook interceptor spine; the `StoreProfile` row columns and the AppHost clock, deadline, receipt, cache, and telemetry ports arrive settled and compose as values.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                  |
| :-----: | ----------------- | ----------------------------------------------------------------------- |
|   [1]   | OPERATION_ALGEBRA | Eight-case StoreOp dispatch; pooled bracket; one fault conversion rail  |
|   [2]   | PROJECTION_SHAPES | Typed projection egress; keyset pages; filter keys; correlation stamps  |
|   [3]   | BULK_LANE         | linq2db movement; delta projection; self-emitted invalidation; receipts |
|   [4]   | INTERCEPTOR_SPINE | Four interception hooks; one fact stream; observability registration    |
|   [5]   | STANDING_QUERY    | Standing queries; tumbling/sliding/session windows; IVM delta; watermarks |
|   [6]   | ARROW_PLANE       | Columnar Arrow zero-copy carrier across transport/DuckDB/index/export    |

## [2]-[OPERATION_ALGEBRA]

- Owner: `StoreOp<T>` `[Union]` operation algebra; `StoreFault` `[Union]` fault vocabulary on the doctrine `Expected` shape with the dual-tier `Create` contract; `StoreRail` total dispatch surface.
- Cases: Get | Query | Stream | Aggregate | Upsert | Delete | Bulk | Maintain on `StoreOp<T>`; Text | Concurrency | Transient | Unsupported | ServerNotProvisioned | NewerSchema on `StoreFault`.
- Entry: `public static IO<T> Run<TDb, T>(PooledDbContextFactory<TDb> contexts, StoreOp<T> op, InterceptPolicy policy, ClockPolicy clocks, DeadlineClass deadline)` — `IO<T>` carries the store effect; every failure aborts through `StoreFault`.
- Auto: the bracket leases through `PooledDbContextFactory.CreateDbContextAsync` — one pooled factory per placement, built at composition from the profile row's `Configure` output — and `DisposeAsync` returns the lease, so the context never escapes the rail; every arm runs under `CreateExecutionStrategy`, binding pg `EnableRetryOnFailure` and sqlite busy-retry to the profile row while database retry stays excluded from the AppHost hop law; `Timeout` binds the caller's `DeadlineClass` row.
- Packages: Microsoft.EntityFrameworkCore.Sqlite, Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new operation posture is one case on `StoreOp<T>` and a new failure is one case row in the 7000 fault-code band; op composition is the `StoreOpCompose.Then` extension block on `StoreOp<T>`, not a new owner; zero new surface.
- Boundary: arity discriminates on payload shape — `Seq` carries one-or-many and `GetMany`/`UpsertMany` suffixes are the deleted spelling; repository-per-entity, generic repositories, lazy loading, and per-call-site tracking toggles are rejected forms — read posture is NoTracking from the profile factory options and write arms track explicitly attached graphs only; multi-statement round-trip collapse rides `NpgsqlBatch` and `NpgsqlBatchCommand` inside the `Query` and `Aggregate` arm shapes so a fan-out read executes as one server round-trip rather than N commands, while the EF batch geometry binds through the `MinBatchSize` and `MaxBatchSize` provider-option columns on the profile row — `NpgsqlBatch` is an execution vehicle inside the existing arms, never a ninth case; `Unsupported` materializes when a lane-by-profile capability row denies a shape; `ServerNotProvisioned` and `NewerSchema` construct at the provisioning probe and the open gate, and `From` is the single projection site — the schema-rail `SchemaFault` 5300-band evidence folds to `NewerSchema` 7005 before any provider-exception arm runs; SQLSTATE evidence rides `PostgresException.SqlState` matched against the `PostgresErrorCodes` constants because the `NpgsqlException` base carries no state code.

```csharp signature
[Union]
public abstract partial record StoreFault : Expected, IValidationError<StoreFault> {
    private StoreFault(string detail, int code) : base(detail, code, None) { }

    public static StoreFault Create(string message) => new Text(message);

    public static StoreFault From(Error error) =>
        error switch {
            SchemaFault schema => new NewerSchema(schema.Message),
            _ => error.Exception.Case switch {
                DbUpdateConcurrencyException ex => new Concurrency(ex.Message),
                PostgresException { SqlState: PostgresErrorCodes.UndefinedObject } ex => new ServerNotProvisioned(ex.Message),
                DbException { IsTransient: true } ex => new Transient(ex.Message),
                Exception ex => new Text(ex.Message),
                _ => new Text(error.Message),
            },
        };

    public sealed record Text : StoreFault { public Text(string detail) : base(detail, 7000) { } }

    public sealed record Concurrency : StoreFault, IValidationError<Concurrency> { public Concurrency(string detail) : base(detail, 7001) { } public static new Concurrency Create(string detail) => new(detail); }

    public sealed record Transient : StoreFault, IValidationError<Transient> { public Transient(string detail) : base(detail, 7002) { } public static new Transient Create(string detail) => new(detail); }

    public sealed record Unsupported : StoreFault, IValidationError<Unsupported> { public Unsupported(string detail) : base(detail, 7003) { } public static new Unsupported Create(string detail) => new(detail); }

    public sealed record ServerNotProvisioned : StoreFault, IValidationError<ServerNotProvisioned> { public ServerNotProvisioned(string detail) : base(detail, 7004) { } public static new ServerNotProvisioned Create(string detail) => new(detail); }

    public sealed record NewerSchema : StoreFault, IValidationError<NewerSchema> { public NewerSchema(string detail) : base(detail, 7005) { } public static new NewerSchema Create(string detail) => new(detail); }
}
```

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreOp<T> where T : notnull {
    private StoreOp() { }

    public sealed record Get(Func<DbContext, CancellationToken, ValueTask<T>> Shape) : StoreOp<T>;
    public sealed record Query(Func<DbContext, CancellationToken, ValueTask<T>> Shape) : StoreOp<T>;
    public sealed record Stream(Func<DbContext, CancellationToken, ValueTask<T>> Shape) : StoreOp<T>;
    public sealed record Aggregate(Func<DbContext, CancellationToken, ValueTask<T>> Shape) : StoreOp<T>;
    public sealed record Upsert(Func<DbContext, CancellationToken, ValueTask<T>> Shape) : StoreOp<T>;
    public sealed record Delete(Func<DbContext, CancellationToken, ValueTask<T>> Shape, Seq<string> Tags) : StoreOp<T>;
    public sealed record Bulk(Func<DbContext, CancellationToken, ValueTask<(T Value, BulkReceipt Receipt)>> Shape, Seq<string> Tags) : StoreOp<T>;
    public sealed record Maintain(Func<DbContext, CancellationToken, ValueTask<T>> Shape, string Kind) : StoreOp<T>;
}

public static class StoreRail {
    public static IO<T> Run<TDb, T>(PooledDbContextFactory<TDb> contexts, StoreOp<T> op, InterceptPolicy policy, ClockPolicy clocks, DeadlineClass deadline) where TDb : DbContext where T : notnull =>
        IO.lift(clocks.Mark).Bind(mark =>
            IO.liftAsync(env => contexts.CreateDbContextAsync(env.Token)).Bracket(
                Use: db => Execute(op, policy, clocks, mark, db).Timeout(deadline.Allotted.ToTimeSpan()),
                Catch: static error => IO.fail<T>(StoreFault.From(error)),
                Fin: static db => IO.liftVAsync<Unit>(async _ => {
                    await db.DisposeAsync();
                    return unit;
                })));

    private static IO<T> Execute<T>(StoreOp<T> op, InterceptPolicy policy, ClockPolicy clocks, long mark, DbContext db) where T : notnull =>
        op.Switch(
            state: (Policy: policy, Clocks: clocks, Mark: mark, Db: db),
            get: static (s, c) => Strategic(s.Db, c.Shape),
            query: static (s, c) => Strategic(s.Db, c.Shape),
            stream: static (s, c) => Strategic(s.Db, c.Shape),
            aggregate: static (s, c) => Strategic(s.Db, c.Shape),
            upsert: static (s, c) => Strategic(s.Db, async (ctx, ct) => (await c.Shape(ctx, ct), await ctx.SaveChangesAsync(ct)).Item1),
            delete: static (s, c) => Strategic(s.Db, c.Shape).Bind(value => IO.liftVAsync<T>(async env => {
                await s.Policy.Invalidate(c.Tags, env.Token);
                return value;
            })),
            bulk: static (s, c) => IO.liftVAsync(env => c.Shape(s.Db, env.Token)).Bind(moved => IO.liftVAsync<T>(async env => {
                _ = s.Policy.Facts(moved.Receipt.Fact);
                await s.Policy.Invalidate(c.Tags, env.Token);
                return moved.Value;
            })),
            maintain: static (s, c) => Strategic(s.Db, c.Shape).Map(value =>
                (s.Policy.Facts(new StoreFact(StoreFact.Maintain, c.Kind, 1, s.Clocks.Elapsed(s.Mark), s.Clocks.Now)), value).Item2));

    private static IO<T> Strategic<T>(DbContext db, Func<DbContext, CancellationToken, ValueTask<T>> shape) =>
        IO.liftAsync(env => db.Database.CreateExecutionStrategy().ExecuteAsync(
            (Db: db, Shape: shape),
            static (state, ct) => state.Shape(state.Db, ct).AsTask(),
            env.Token));
}
```

The `StoreOp<T>` owner carries one self-composing Kleisli combinator — `Then` sequences a producing op with a `T`-keyed continuation through the generated total `Switch`, rewrapping into the same case the source carries so a receipt- or tag-bearing arm keeps its self-emitted invalidation: a `Get` read feeding a `Bulk` merge is one composed op the rail runs in a single bracket, a `Delete` source threads its `Tags` onto the composed `Delete`, a `Maintain` source threads its `Kind`, and a `Bulk` source re-projects its `BulkReceipt` onto the `U` result so the rail's `bulk` arm still fires `Facts(receipt.Fact)` and `Invalidate(tags)`; collapsing a receipt- or tag-bearing case to a bare `Query` is the rejected spelling. The combinator adds zero cases, dispatch stays total so a ninth case breaks composition at compile time, and the continuation runs inside the same execution strategy as the source arm.

```csharp signature
public static class StoreOpCompose {
    extension<T>(StoreOp<T> source) where T : notnull {
        public StoreOp<U> Then<U>(Func<T, DbContext, CancellationToken, ValueTask<U>> next) where U : notnull =>
            source.Switch<Func<T, DbContext, CancellationToken, ValueTask<U>>, StoreOp<U>>(
                state: next,
                get:       static (n, op) => new StoreOp<U>.Query(Sequenced(op.Shape, n)),
                query:     static (n, op) => new StoreOp<U>.Query(Sequenced(op.Shape, n)),
                stream:    static (n, op) => new StoreOp<U>.Stream(Sequenced(op.Shape, n)),
                aggregate: static (n, op) => new StoreOp<U>.Aggregate(Sequenced(op.Shape, n)),
                upsert:    static (n, op) => new StoreOp<U>.Upsert(Sequenced(op.Shape, n)),
                delete:    static (n, op) => new StoreOp<U>.Delete(Sequenced(op.Shape, n), op.Tags),
                maintain:  static (n, op) => new StoreOp<U>.Maintain(Sequenced(op.Shape, n), op.Kind),
                bulk:      static (n, op) => new StoreOp<U>.Bulk(
                    async (db, ct) => { var moved = await op.Shape(db, ct); return (await n(moved.Value, db, ct), moved.Receipt); },
                    op.Tags));

        private static Func<DbContext, CancellationToken, ValueTask<U>> Sequenced<U>(Func<DbContext, CancellationToken, ValueTask<T>> producer, Func<T, DbContext, CancellationToken, ValueTask<U>> next) where U : notnull =>
            async (db, ct) => await next(await producer(db, ct), db, ct);
    }
}
```

## [3]-[PROJECTION_SHAPES]

- Owner: `KeysetPage<TRow>` page record; `ProjectionRail` filter-key vocabulary and query-stamping extensions.
- Cases: soft-delete | retention | sync-tombstone named filter keys.
- Entry: `public static async ValueTask<KeysetPage<TRow>> Materialize(IAsyncEnumerable<TRow> rows, Func<TRow, Guid> key, int take, CancellationToken token)` — pure materialization; the probe row beyond `take` decides cursor continuation.
- Auto: the `EF.CompileAsyncQuery` compiled-shape rows cache hot projections as static fields beside their projection records under the `[COMPILED_QUERY]` gate; `Correlated` stamps the `CorrelationId` through `TagWith` on every shape; `Stream` shapes fold a bounded `IAsyncEnumerable` inside the leased bracket, with the batch bound a per-shape policy value.
- Packages: Microsoft.EntityFrameworkCore.Sqlite, Npgsql.EntityFrameworkCore.PostgreSQL, linq2db.EntityFrameworkCore, LanguageExt.Core, BCL inbox.
- Growth: a new egress is one typed projection record beside its consumer plus one compiled-shape row, zero new surface; a non-uuid cursor is one shape row on `KeysetPage<TRow>`; a new bridge terminator is one method on the `ProjectionRail` extension block (`BridgeListAsync`, `BridgeArrayAsync`, `ReentryListAsync`, `FilterPredicate`), and a temporal rollup is one `DurationRollup`-shaped projection over the NodaTime `DbFunctions` aggregates; zero new surface.
- Boundary: typed projection records are the only egress and entity types never cross the package boundary; offset pagination is the rejected page form — keyset cursors ride the UuidV7Key identity order, and a compound `(tenant_id, created_at)` index serves both a keyset cursor and a `created_at`-only filter through the PG18 automatic B-tree skip scan (no SQL, no GUC) so a redundant single-column index leading with the tenant or partition discriminant is the deleted form; named filter predicates attach at the model under these keys and `Unfiltered` disables them per operation case; a caller-supplied `FilterPredicate(Expression<Func<TRow,bool>>)` composes one ad-hoc server-side `Where` onto the source before `Materialize` probes the cursor so a filtered keyset page re-queries the store under the same `take + 1` continuation rather than client-filtering a materialized page — a client-side `.Where` over a fetched page is the deleted form, the predicate stays an `Expression` so the EF translator pushes it into SQL, and it folds with the named model filters and the keyset cursor in one query shape; split-query is the per-shape `AsSplitQuery` policy value; JSON path predicates and `ExecuteUpdateAsync` into JSON paths ride document-lane shapes on the jsonb mapping; `LeftJoin`/`RightJoin` operators replace the `GroupJoin` flatten spelling; a window-function or set-based projection past the EF translator routes through `ToLinqToDB` and terminates on `ToListAsyncLinqToDB`, `ToArrayAsyncLinqToDB`, `FirstAsyncLinqToDB`, `CountAsyncLinqToDB`, or `SumAsyncLinqToDB` so the bridge materializes inside the same leased context rather than a second connection, while a leg re-entering EF translation after the bridge hand-off terminates on `ToListAsyncEF`, `FirstAsyncEF`, or `CountAsyncEF` so the two-door composition closes on the EF side without a second materialization pass; a `Duration` rollup or sync-lag statistic on the `Aggregate` arm projects through the `NpgsqlNodaTimeDbFunctionsExtensions` temporal aggregates `Sum`, `Average`, and `Distance` over `EF.Functions` so the duration math executes server-side rather than materializing rows for a client fold, with `Distance` measuring temporal spread; a client-side `Duration` aggregate over materialized rows is the deleted form; over a TimescaleDB hypertable rollup carrying timescaledb_toolkit, `DurationRollup` projects the time-series-product hyperfunctions `approx_percentile`/`percentile_agg`/`time_weight`/`counter_agg`/`state_agg`/`asof`/gapfill as raw-SQL `Aggregate`-arm shapes on `server-tier#TIMESCALE_PROVISIONING`, while the exact `percentile_cont(0.95) WITHIN GROUP (ORDER BY value)` form is the always-present zero-extra-extension fallback so a profile without the toolkit still answers the rollup.

```csharp signature
public sealed record KeysetPage<TRow>(Seq<TRow> Rows, Option<Guid> After, int Count) where TRow : notnull {
    public static async ValueTask<KeysetPage<TRow>> Materialize(IAsyncEnumerable<TRow> rows, Func<TRow, Guid> key, int take, CancellationToken token) {
        var probe = await rows.Take(take + 1).ToListAsync(token);
        return new KeysetPage<TRow>(
            toSeq(probe).Take(take),
            probe.Count > take ? Optional(key(probe[take - 1])) : None,
            int.Min(probe.Count, take));
    }
}

public static class ProjectionRail {
    public const string SoftDelete = "soft-delete";
    public const string Retention = "retention";
    public const string SyncTombstone = "sync-tombstone";

    extension<TRow>(IQueryable<TRow> source) where TRow : notnull {
        public IQueryable<TRow> Correlated(CorrelationId correlation) => source.TagWith(correlation.ToString());

        public IQueryable<TRow> Unfiltered(params ReadOnlySpan<string> filters) => source.IgnoreQueryFilters([.. filters]);

        public ValueTask<Seq<TRow>> BridgeListAsync(CancellationToken token) =>
            new(source.ToLinqToDB().ToListAsyncLinqToDB(token).Map(toSeq));

        public ValueTask<TRow[]> BridgeArrayAsync(CancellationToken token) =>
            new(source.ToLinqToDB().ToArrayAsyncLinqToDB(token));

        public ValueTask<Seq<TRow>> ReentryListAsync(CancellationToken token) =>
            new(source.ToListAsyncEF(token).Map(toSeq));

        public IQueryable<TRow> FilterPredicate(Expression<Func<TRow, bool>> predicate) => source.Where(predicate);
    }

    extension<TRow>(IQueryable<TRow> source) where TRow : class {
        public ValueTask<Duration> DurationRollup(Expression<Func<TRow, Duration>> selector, CancellationToken token) =>
            new(source.Select(selector).GroupBy(static _ => 1).Select(static g => EF.Functions.Sum(g.Select(static d => d))).FirstAsyncEF(token));
    }
}
```

## [4]-[BULK_LANE]

- Owner: `BulkRoute` route vocabulary; `BulkReceipt` typed movement receipt; `BulkDelta<TRow>` action-sentinel delta record.
- Cases: Copy | Merge | Set on `BulkRoute`.
- Entry: `public static BulkReceipt Of(ClockPolicy clocks, long mark, string entity, long rows, BulkRoute route, string provider)` — pure receipt mint from the clock seam.
- Auto: the `StoreOp<T>.Bulk` arm emits the receipt's fact projection and replays the tag transitions itself — the bulk path bypasses SaveChanges interceptors, so changefeed rows and invalidation are self-emitted inside the same transaction as the movement.
- Receipt: `BulkReceipt` — entity, rows, route, provider, elapsed `Duration`, `Instant` stamp.
- Packages: linq2db.EntityFrameworkCore, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core.
- Growth: a new movement form is one `BulkRoute` case carrying its receipt row; a transaction-opt-out lane is one `Detached` column on the bulk-options value; the OLD/NEW delta-emission path is the ReturningOldNew capability column on the profile row, never a parallel merge surface; a store-side backpressure shed is one `StoreFact.BulkShed` fact on the existing fact stream, never a second signal owner; zero new surface.
- Boundary: bridge activation is one `LinqToDBForEFTools.Initialize()` call at the composition root before the first bulk shape, and the bridge data-context binds `AddMappingSchema`, `AddCustomOptions`, `AddInterceptor`, and `EnableChangeTracker` as columns on the bulk-options value so the linq2db interceptor registers as one altitude on the spine rather than a parallel hook; a lane that must run outside the ambient `DbContext` transaction opens through `CreateLinqToDBConnectionDetached` so the bulk write commits independently of the enclosing save, and a bridge mapping-schema or interceptor swap at composition is followed by one `LinqToDBForEFTools.ClearCaches()` call so a stale cached mapping never survives the rebind; the `Set` route opens its target through `ToLinqToDBTable` or `GetTable` and starts a set-based insert with `Into` so a projection-sourced insert never materializes entities; `BulkCopyAsync` rides `BulkCopyOptions` on the `Copy` route; `MergeWithOutputAsync` consumes `Projection`, whose action string is a SQL-mapped sentinel and never caller input, and on a profile row carrying the ReturningOldNew capability column the merge emits old and new images straight into `BulkDelta<TRow>` so the changefeed reads the RETURNING image without a re-read — the PG18 engine half is the `RETURNING old.*, new.*` form (`BulkDelta.ReturningOldNewSql`) across INSERT/UPDATE/DELETE/MERGE that Npgsql EF v10 maps, so `MergeWithOutputAsync` materializes both images in one round-trip without a re-read, while a profile row without the column (sqlite) captures deltas through the SaveChanges interceptor hook instead; the binary-COPY route stays settled as `BulkCopyType.ProviderSpecific` for pg and `MultipleRows` for the sqlite downgrade over the raw `NpgsqlConnection.BeginBinaryImport`/`NpgsqlBinaryImporter` path; `ToLinqToDB` is the window-function escape inside one query shape; EFCore.BulkExtensions and a query-builder layer are the deleted forms; `Deleted`/`Inserted` sentinels project to `Option` and never travel inward; a bulk movement that exceeds the per-shape capacity bound under store-side pressure emits one `StoreFact.BulkShed` fact carrying the shed row count and the elapsed-at-shed `Duration` on the same `Func<StoreFact,Unit>` interceptor sink the movement already feeds, so a downstream lane reads backpressure off the existing fact stream and throttles its producer — a parallel backpressure channel, a thrown capacity exception, or a second shed signal owner is the deleted form, and the shed is observable as a route-degradation fact rather than a silent drop.

```csharp signature
[SmartEnum]
public sealed partial class BulkRoute {
    public static readonly BulkRoute Copy = new();
    public static readonly BulkRoute Merge = new();
    public static readonly BulkRoute Set = new();
}

public readonly record struct BulkReceipt(string Entity, long Rows, BulkRoute Route, string Provider, Duration Elapsed, Instant At) {
    public static BulkReceipt Of(ClockPolicy clocks, long mark, string entity, long rows, BulkRoute route, string provider) =>
        new(entity, rows, route, provider, clocks.Elapsed(mark), clocks.Now);

    public StoreFact Fact => new(StoreFact.Bulk, Entity, Rows, Elapsed, At);
}

public sealed record BulkDelta<TRow>(string Action, TRow? Deleted, TRow? Inserted) where TRow : class {
    public const string ReturningOldNewSql = "RETURNING old.*, new.*";

    public static Expression<Func<string, TRow, TRow, BulkDelta<TRow>>> Projection { get; } =
        (action, deleted, inserted) => new BulkDelta<TRow>(action, deleted, inserted);

    public Option<TRow> Old => Optional(Deleted);

    public Option<TRow> New => Optional(Inserted);
}
```

## [5]-[INTERCEPTOR_SPINE]

- Owner: `StoreFact` operational fact stream; `InterceptPolicy` delegate-row policy; `StoreInterceptor` single interception capsule; `StoreObservability` registration rows.
- Entry: `public static Func<StoreFact, IO<ReceiptEnvelope>> Sink(ReceiptSinkPort port, JsonSerializerOptions wire, CorrelationId correlation)` — facts materialize as receipt envelopes at the sink edge, with the wire options arriving from the suite Strict merge.
- Auto: slow-query and burst sentinels fold per command with zero call-site code; plan capture re-issues the provider explain form only while `CapturePlans` is set, riding the `store.command.plan` kind; native `Activity` parameter destructuring caps at the `TraceDepth` policy value so deep object graphs never inflate span cardinality; the pg_stat_statements read view enters as one `Aggregate` raw-SQL shape gated on the store-profiles provisioning probe; savepoint evidence is one added delegate column when a profile row earns it.
- Receipt: `StoreFact` rows — kind, subject, count, elapsed `Duration`, `Instant`; bulk and maintain arms share the stream through their fact projections.
- Packages: Microsoft.EntityFrameworkCore.Sqlite, Npgsql.OpenTelemetry, OpenTelemetry, Microsoft.Extensions.Caching.Hybrid, NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new evidence bucket is one kind constant plus one emission row, a new cross-cutting concern is one delegate-column transformer on `InterceptPolicy` registered as an altitude in the stack, a continuous-aggregate refresh push is one `StoreFact.AggregateRefresh` kind plus one maintenance-lease emission, and a new hook binding is one delegate column on `InterceptPolicy`, zero new surface.
- Boundary: `StoreInterceptor` is the EF interception boundary capsule and its members carry language-owned statement forms; the four hooks compose as one ordered effect-transformer stack where registration order is execution order — the linq2db `AddInterceptor` altitude and the Npgsql `AddNpgsql`/`AddNpgsqlInstrumentation` admission roots register as further altitudes on the same stack, so a soft-delete audit or a trace-depth cap is a new transformer row, never code inside an operation body, and no second interceptor owner appears because AppHost owns the hop law; `Stamp` and `Changefeed` run inside `SavingChangesAsync` so op-log rows commit with entity rows in one transaction, while `Invalidate` and the save fact run after commit in `SavedChangesAsync`; `Reopen` re-applies the PRAGMA ladder on non-pooled opens and arrives bound from the native-sqlite policy table; `Invalidation` binds the tag delegate to the AppHost cache surface; `Traces` and `Meters` are the Npgsql registration rows — `Traces` rides `TracerProviderBuilderExtensions.AddNpgsql(TracerProviderBuilder)` and `Meters` rides `MeterProviderBuilderExtensions.AddNpgsqlInstrumentation(MeterProviderBuilder, Action<NpgsqlMetricsOptions>)`, so meter posture is one configuration column on the registration; `NpgsqlMetricsOptions` carries no settable property knobs at the admitted Npgsql, so the shape delegate is the registration seam and any histogram or cardinality posture rides the OpenTelemetry meter-view configuration rather than a provider-options knob, while `Contribution` carries the minted Persistence identity through `TelemetryContributorPort`; the pg_stat_statements slow-query read view is the durable query-observability surface entering as one `Aggregate` raw-SQL shape over the catalogued contrib view gated on the store-profiles provisioning probe, and the `auto_explain` GUC posture is a read-only `pg_settings` verification fact feeding the same `store.command.slow`/`store.command.plan` stream rather than a runtime `ALTER SYSTEM`; the OLD/NEW RETURNING bulk-delta count rides the existing `BulkReceipt.Fact`; the beta EF and gRPC instrumentation packages stay rejected — native `Activity` emission carries those spans; `InterceptPolicy.Default` is the axis row every spine literal traces to.

```csharp signature
public readonly record struct StoreFact(string Kind, string Subject, long Count, Duration Elapsed, Instant At) {
    public const string CommandSlow = "store.command.slow";
    public const string CommandBurst = "store.command.burst";
    public const string CommandPlan = "store.command.plan";
    public const string Transaction = "store.transaction";
    public const string Save = "store.save";
    public const string Bulk = "store.bulk";
    public const string BulkShed = "store.bulk.shed";
    public const string Maintain = "store.maintain";
    public const string AggregateRefresh = "store.aggregate.refresh";
}

public sealed record InterceptPolicy(
    Duration SlowQuery,
    int Burst,
    Duration BurstSpan,
    bool CapturePlans,
    int TraceDepth,
    Func<DbConnection, CancellationToken, Task> Reopen,
    Func<DbContext, Instant, Unit> Stamp,
    Func<DbContext, Instant, Unit> Changefeed,
    Func<DbContext, Seq<string>> Tags,
    Func<Seq<string>, CancellationToken, ValueTask> Invalidate,
    Func<StoreFact, Unit> Facts) {
    public static readonly InterceptPolicy Default = new(
        SlowQuery: Duration.FromMilliseconds(250),
        Burst: 16,
        BurstSpan: Duration.FromMilliseconds(100),
        CapturePlans: false,
        TraceDepth: 3,
        Reopen: static (_, _) => Task.CompletedTask,
        Stamp: static (_, _) => unit,
        Changefeed: static (_, _) => unit,
        Tags: static _ => Seq<string>(),
        Invalidate: static (_, _) => ValueTask.CompletedTask,
        Facts: static _ => unit);
}
```

```csharp signature
public sealed class StoreInterceptor(InterceptPolicy policy, ClockPolicy clocks) :
    IDbConnectionInterceptor, IDbCommandInterceptor, ISaveChangesInterceptor, IDbTransactionInterceptor {
    private readonly Atom<(Instant At, int Count)> burst = Atom((Instant.MinValue, 0));

    public Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default) =>
        policy.Reopen(connection, cancellationToken);

    public ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default) {
        var now = clocks.Now;
        var window = burst.Swap(last => now - last.At > policy.BurstSpan ? (now, 1) : (last.At, last.Count + 1));
        _ = eventData.Duration >= policy.SlowQuery.ToTimeSpan()
            ? policy.Facts(new StoreFact(StoreFact.CommandSlow, command.CommandText, 1, eventData.Duration.ToDuration(), now))
            : unit;
        _ = window.Count == policy.Burst
            ? policy.Facts(new StoreFact(StoreFact.CommandBurst, command.CommandText, window.Count, now - window.At, now))
            : unit;
        return ValueTask.FromResult(result);
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult((eventData.Context is { } db
            ? (policy.Stamp(db, clocks.Now), policy.Changefeed(db, clocks.Now)).Item2
            : unit, result).Item2);

    public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default) {
        var tags = eventData.Context is { } db ? policy.Tags(db) : Seq<string>();
        await policy.Invalidate(tags, cancellationToken);
        _ = policy.Facts(new StoreFact(StoreFact.Save, eventData.Context?.GetType().Name ?? string.Empty, result, Duration.Zero, clocks.Now));
        return result;
    }

    public Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default) =>
        Task.FromResult(policy.Facts(new StoreFact(StoreFact.Transaction, transaction.IsolationLevel.ToString(), 1, eventData.Duration.ToDuration(), clocks.Now)));
}

public static class StoreObservability {
    public static TracerProviderBuilder Traces(TracerProviderBuilder tracing) => tracing.AddNpgsql();

    public static MeterProviderBuilder Meters(MeterProviderBuilder metrics, Action<NpgsqlMetricsOptions> shape) => metrics.AddNpgsqlInstrumentation(shape);

    public static TelemetryContributorPort Contribution(string version, Seq<InstrumentRow> instruments) =>
        new(TelemetrySource.Persistence, version, instruments);

    public static Func<Seq<string>, CancellationToken, ValueTask> Invalidation(HybridCache cache) =>
        (tags, token) => cache.Invalidate(tags, token);

    public static Func<StoreFact, IO<ReceiptEnvelope>> Sink(ReceiptSinkPort port, JsonSerializerOptions wire, CorrelationId correlation) =>
        fact => port.Send(correlation, TenantContext.Current, TelemetrySource.Persistence.Key, fact.Kind, JsonSerializer.SerializeToElement(fact, wire));
}
```

```mermaid
flowchart LR
    SavingChangesAsync --> Stamp
    Stamp --> Changefeed
    Changefeed --> SavedChangesAsync
    SavedChangesAsync --> Invalidate
    Invalidate --> HybridCache
    SavedChangesAsync --> Facts
    Facts --> Sink
    Sink --> ReceiptSinkPort
```

## [6]-[STANDING_QUERY]

- Owner: `StandingQuery<TRow>` the registered continuous-query record; `WindowSpec` the window vocabulary; `QueryDelta<TRow>` the incremental-view-maintenance delta-in/delta-out carrier; `Watermark` the event-time progress mark; `StandingQueries` the static surface owning the standing-query registration, the windowed fold, the IVM delta application, and the watermark/late-arrival policy.
- Cases: `Tumbling | Sliding | Session` on `WindowSpec`; a standing query is a registered `IQueryable`-shaped predicate plus a window plus an IVM fold so a new op-log row produces a delta-out without re-running the whole query.
- Entry: `public static StandingQuery<TRow> Register(string id, Func<IQueryable<TRow>, IQueryable<TRow>> shape, WindowSpec window, Func<QueryDelta<TRow>, QueryDelta<TRow>> fold)` — registers a standing query; `public static (Seq<TRow> Out, Watermark Advanced) Step(StandingQuery<TRow> query, QueryDelta<TRow> deltaIn, Watermark watermark)` applies an incoming delta and emits the delta-out plus the advanced watermark.
- Auto: the standing query rides the op-log changefeed as its delta-in source so a continuous query never polls — the changefeed cursor advances and each new `OpLogEntry` is a `QueryDelta` insert/delete the IVM fold applies, emitting only the result rows that changed; the window vocabulary folds over the event-time `OpLogEntry.Physical` — a tumbling window buckets by fixed interval, a sliding window by interval-plus-slide, a session window by inactivity gap — so a windowed aggregate (count-per-minute, moving-average, session-rollup) maintains incrementally; the watermark is the event-time progress mark so a late-arriving row (a row whose `Physical` precedes the watermark) folds into a retraction-plus-restated delta rather than a dropped row, and the late-arrival allowed-lateness bound is a policy value; the standing query result rides the DynamicData change-set surface (`AppUi/live-data`) and the TimescaleDB continuous aggregate (`server-tier#TIMESCALE_PROVISIONING`) is the server-side persisted analogue.
- Receipt: a step rides `store.standing.step` carrying the delta-in and delta-out cardinalities and the advanced watermark; a late-arrival retraction rides `store.standing.late`.
- Packages: linq2db.EntityFrameworkCore, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new window kind is one `WindowSpec` case; a new aggregation is one IVM fold; a new late-arrival policy is one column on `WindowSpec`; zero new surface — a polling query loop, a per-query materialized-view trigger, or a second streaming engine is the deleted form because the standing query rides the op-log changefeed as its delta source, the window folds over the HLC event time, and the IVM fold emits delta-out, and the building blocks (DynamicData change-sets, TimescaleDB continuous aggregates, the DuckDB analytical lane) compose under this one watermark/window owner.
- Boundary: the standing query is incremental-view-maintenance over the changefeed so it never polls and never re-runs the full query — a new op-log row is a `QueryDelta` the IVM fold applies, emitting only the changed result rows, so a poll loop or a full-query-on-every-change is the deleted form; the window folds over event time (`OpLogEntry.Physical`), never wall-clock arrival time, so a window aggregate is reproducible and replays identically — a processing-time window is the deleted form; the watermark bounds out-of-order tolerance — a row whose event time precedes the watermark by less than the allowed-lateness emits a retraction-plus-restated delta so the windowed result corrects, and a row later than the bound folds into the next window with a late-arrival fact, so a silently-dropped late row is the deleted form; the standing query is the unified watermark/window owner the existing building blocks compose under — DynamicData change-sets carry the delta-out to the UI, TimescaleDB continuous aggregates are the server-side persisted standing query, and the DuckDB analytical lane materializes a windowed rollup — so a second streaming framework is the deleted form; the IVM delta is signed (insert positive, delete negative) so a sliding-window aggregate maintains by adding the entering rows and subtracting the leaving rows, never re-scanning the window.

```csharp signature
[SmartEnum]
public sealed partial class WindowKind {
    public static readonly WindowKind Tumbling = new();
    public static readonly WindowKind Sliding = new();
    public static readonly WindowKind Session = new();
}

public readonly record struct WindowSpec(WindowKind Kind, Duration Size, Duration Slide, Duration Gap, Duration AllowedLateness) {
    public static WindowSpec Of(Duration size) => new(WindowKind.Tumbling, size, size, Duration.Zero, Duration.FromSeconds(30));

    public Instant BucketStart(Instant at) =>
        Kind.Switch(
            state: (At: at, Size: Size),
            tumbling: static s => Instant.FromUnixTimeTicks(s.At.ToUnixTimeTicks() - s.At.ToUnixTimeTicks() % s.Size.BclCompatibleTicks),
            sliding: static s => Instant.FromUnixTimeTicks(s.At.ToUnixTimeTicks() - s.At.ToUnixTimeTicks() % s.Size.BclCompatibleTicks),
            session: static s => s.At);
}

public readonly record struct QueryDelta<TRow>(Seq<TRow> Inserted, Seq<TRow> Deleted, Instant EventTime) where TRow : notnull {
    public int Net => Inserted.Count - Deleted.Count;
}

public readonly record struct Watermark(Instant EventTime, long Processed) {
    public bool IsLate(Instant rowTime, Duration allowed) => rowTime < EventTime - allowed;

    public Watermark Advance(Instant rowTime, int rows) =>
        new(Instant.Max(EventTime, rowTime), Processed + rows);
}

public sealed record StandingQuery<TRow>(
    string Id,
    Func<IQueryable<TRow>, IQueryable<TRow>> Shape,
    WindowSpec Window,
    Func<QueryDelta<TRow>, QueryDelta<TRow>> Fold) where TRow : notnull;

public static class StandingQueries {
    public static StandingQuery<TRow> Register<TRow>(string id, Func<IQueryable<TRow>, IQueryable<TRow>> shape, WindowSpec window, Func<QueryDelta<TRow>, QueryDelta<TRow>> fold) where TRow : notnull =>
        new(id, shape, window, fold);

    public static (QueryDelta<TRow> Out, Watermark Advanced) Step<TRow>(StandingQuery<TRow> query, QueryDelta<TRow> deltaIn, Watermark watermark) where TRow : notnull {
        var late = deltaIn.Inserted.Filter(_ => watermark.IsLate(deltaIn.EventTime, query.Window.AllowedLateness));
        var folded = query.Fold(deltaIn);
        return (folded, watermark.Advance(deltaIn.EventTime, deltaIn.Inserted.Count + deltaIn.Deleted.Count));
    }
}
```

## [7]-[ARROW_PLANE]

- Owner: `ArrowCarrier` the columnar zero-copy buffer carrier; `ArrowSchema` the column-layout descriptor; `ArrowPlane` the static surface owning the DuckDB-to-Arrow zero-copy export, the Arrow-to-index ingest, and the Arrow-to-transport/GPU/export hand-off.
- Cases: a carrier wraps a DuckDB result chunk as a borrowed columnar buffer so the same memory threads transport → DuckDB → index → GPU → export without a managed-array copy.
- Entry: `public static IO<ArrowCarrier> FromDuckDb(DuckDBConnection lane, string sql, Seq<UInt128> parameters)` — projects a DuckDB query result as a zero-copy Arrow carrier over the vector-chunk readers; `public static IO<Unit> ToExport(ArrowCarrier carrier, Func<ReadOnlyMemory<byte>, IO<Unit>> sink)` streams the columnar buffer to a sink chunk-wide.
- Auto: the Arrow plane is the columnar in-memory analytics carrier the DuckDB lane already produces — DuckDB's native vector-chunk readers (`data-lanes#ANALYTICAL_LANE` vector chunk transfer) expose columnar buffers at the engine's vector quantum, so the carrier borrows those buffers rather than copying into a managed array, and the same columnar memory threads from the analytical query into the index ingest, the GPU tensor encode (`Compute/tensor-lane`), and the parquet export; the carrier is one chunk wide so peak managed memory stays bounded regardless of result size; the hand-off to the index is column-oriented so a vector-index bulk ingest reads the embedding column directly, and the hand-off to export is the parquet `COPY` the analytical lane owns.
- Receipt: a carrier projection rides `store.arrow.carrier` carrying the column count and the chunk row count; an export rides the analytical-lane parquet receipt.
- Packages: DuckDB.NET.Data.Full, System.Buffers, LanguageExt.Core, BCL inbox.
- Growth: a new carrier sink is one `ArrowPlane` hand-off method; a new column type is one Arrow-schema row; zero new surface — a managed-array result buffer, a row-oriented carrier, or a per-sink copy is the deleted form because the carrier borrows the DuckDB vector-chunk buffers at the vector quantum and threads them zero-copy across the analytics surfaces; the DuckDB analytics lane stays the columnar engine and this owner models the zero-copy carrier the lane's chunk readers already expose, replacing the DynamicData change-set hand-off for the bulk analytical path.
- Boundary: the Arrow plane borrows DuckDB's columnar vector-chunk buffers zero-copy so the analytical carrier never copies into a managed array — a managed-array result buffer or a row-oriented carrier is the deleted form, and the carrier wraps the engine's native chunk at the vector quantum so peak memory is one chunk wide; the same columnar memory threads transport → DuckDB → index → GPU → export so a vector-index ingest reads the embedding column directly off the carrier, a GPU tensor encode reads the numeric columns (`Compute/tensor-lane` geometry encoding), and the parquet export streams the carrier through the analytical lane's `COPY` — so a per-sink re-serialization is the deleted form; the carrier is a borrowed buffer with a bounded lifetime tied to the DuckDB chunk reader's scope so it never escapes the read scope or enters a closure, the same ref-struct discipline the Sep reader holds; the DuckDB lane stays the columnar engine and this owner is the zero-copy carrier model over its existing vector-chunk readers, never a second analytics engine; the bulk analytical hand-off uses this carrier while the row-granular UI hand-off stays the DynamicData change-set, so the two hand-offs are altitude-split by granularity, never duplicated.

```csharp signature
public readonly record struct ArrowSchema(Seq<(string Name, string ArrowType, bool Nullable)> Columns);

public sealed record ArrowCarrier(ArrowSchema Schema, int RowCount, ReadOnlyMemory<byte> ColumnBuffers, long ChunkOffset);

public static class ArrowPlane {
    public static IO<ArrowCarrier> FromDuckDb(DuckDBConnection lane, string sql, Func<DuckDBConnection, string, IO<(ArrowSchema Schema, int Rows, ReadOnlyMemory<byte> Buffers)>> readChunk) =>
        readChunk(lane, sql).Map(chunk => new ArrowCarrier(chunk.Schema, chunk.Rows, chunk.Buffers, 0L));

    public static IO<Unit> ToExport(ArrowCarrier carrier, Func<ReadOnlyMemory<byte>, IO<Unit>> sink) =>
        sink(carrier.ColumnBuffers);

    public static IO<Unit> ToIndex(ArrowCarrier carrier, string column, Func<ReadOnlyMemory<byte>, IO<Unit>> ingest) =>
        carrier.Schema.Columns.Find(c => c.Name == column).Match(
            Some: _ => ingest(carrier.ColumnBuffers),
            None: () => IO.fail<Unit>(Error.New($"<arrow-column-absent:{column}>")));
}
```

## [8]-[RESEARCH]

- [STANDING_QUERY_IVM]: the incremental-view-maintenance delta-fold over the op-log changefeed for a windowed aggregate — whether a sliding-window count maintains by signed delta (add entering rows, subtract leaving rows) without re-scanning the window, and the late-arrival retraction-plus-restate emission against the watermark allowed-lateness on a live changefeed, beside the TimescaleDB continuous-aggregate server-side analogue.
- [ARROW_ZERO_COPY]: the DuckDB.NET vector-chunk-reader buffer the `ArrowCarrier` borrows zero-copy — whether the chunk reader exposes a columnar buffer with a lifetime safe to thread to an index ingest and a parquet export without a managed copy, and the column-type-to-Arrow-type mapping the schema descriptor carries, proven against the live in-process engine.
- [BULK_RETURNING_PROBE]: the live `MergeWithOutputAsync` round-trip emitting the PG18 `RETURNING old.*, new.*` image into `BulkDelta<TRow>` against a live PG18 server behind the ReturningOldNew capability column — the engine spelling and the binary-COPY route (`BulkCopyType.ProviderSpecific` for pg, `MultipleRows` for the sqlite downgrade, raw `NpgsqlConnection.BeginBinaryImport`/`NpgsqlBinaryImporter`) are settled.
- [TRACE_DEPTH]: EF Core 10 native `Activity` emission depth beside `AddNpgsql` spans on a live EF context.
- [COMPILED_QUERY]: the cached `EF.CompileAsyncQuery` plan lifetime against the pooled-context factory on a live context — whether a compiled async query rebinds the leased context's execution strategy per invocation, beyond the confirmed `Func<TContext, IAsyncEnumerable<TResult>>` delegate shape and `TParam1`..`TParam15` arity.
- [TEMPORAL_AGGREGATE]: the `NpgsqlNodaTimeDbFunctionsExtensions` aggregate arity for `Sum`, `Average`, and `Distance` over `Duration` and `Period` sequences on a live pg translate — the grouping shape the EF translator requires for the `DurationRollup` projection and whether `Distance` takes a single-column or two-column temporal span.
