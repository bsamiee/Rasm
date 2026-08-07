# [APPHOST_WORK_LANE_GOVERNOR]

The work-lane vocabulary owner and the in-process resilience governor for the runtime spine: the `WorkLane` roster DECLARES here and one `LaneGuard` composes a keyed Polly `ResiliencePipeline` per row — mirroring the `Wire/outbound#KEYED_PIPELINES` `KeyedLane.Register` per-hop `AddResiliencePipeline` registry but for the in-process command/solve edge the transport-only Polly registry never reached — with bulkhead, adaptive concurrency, circuit breaker, load-shed, hedge, and `Polly.Simmy` chaos as roster-driven pipeline rows, the adaptive-concurrency arm reading `Microsoft.Extensions.Diagnostics.ResourceMonitoring` and the load-shed arm reading the collapsed `Observability/health#DEGRADATION_RAIL` `DegradationReading`, so the in-process lanes degrade gracefully, isolate per lane, tune concurrency at runtime, and carry a first-class chaos surface symmetric to the transport `KeyedLane`. The per-`WorkLane` shed verdict mints once here from the atomic `DegradationReading` and crosses to `Rasm.Compute/Runtime/admission` (the `ONE_DEGRADATION_SHED_VERDICT` ripple) rather than a Compute-side re-derivation. The page owns the lane roster, its deadline and pipeline-key projections, the lane-guard registry with its boot closure proof, the adaptive-concurrency and load-shed arms, the chaos axis, and the shed verdict; it consumes `KeyedLane`/`CircuitBreakerManualControl`/`CircuitBreakerStateProvider`, `DegradationCell`/`DegradationReading`/`DegradationLevel`, `UtilizationCell`/`ResourceQuota`, `DeadlineClass`/`ClockPolicy`, and `ReceiptSinkPort` as settled vocabulary and mints no eighth port.

## [01]-[INDEX]

- [02]-[LANE_GUARD]: The `WorkLane` roster and one keyed Polly `ResiliencePipeline` per row, proven closed at boot.
- [03]-[ADAPTIVE_ARMS]: The ResourceMonitoring-fed resizable permit source and the `DegradationReading` load-shed arm.
- [04]-[SHED_VERDICT]: The per-`WorkLane` shed verdict minted once and crossed to Compute admission.

## [02]-[LANE_GUARD]

- Owner: `WorkLane` `[SmartEnum<string>]` the six-row lane roster — identity and `Rank` — under the `ComparerAccessors.StringOrdinal` accessor; `LaneStrategy` `[SmartEnum<string>]` the pipeline-row vocabulary whose declaration order IS the strategy order and whose delegate column IS each strategy's arm; `LanePolicy` the per-lane resilience-row record; `LaneClass` the rank-to-deadline and pipeline-key projections off the roster; `LaneGuard` the static keyed-pipeline registry over the in-process lanes; `LaneFault` `[Union]` fault family deriving its codes through `FaultBand.Lane` = Text | BulkheadRejected | Shed | Deadline | LaneBroken.
- Entry: `Register(IServiceCollection services, LaneGuard.Composition composition, params ReadOnlySpan<LanePolicy> rows)` returns `Fin<IServiceCollection>` — proves the roster closed against the supplied rows, then folds one `AddResiliencePipeline` entry per row keyed by `WorkLane.PipelineKey`, each pipeline composed by folding `LaneStrategy.Items` in declaration order so admission sits outermost and the attempt deadline innermost; `Proven(ResiliencePipelineProvider<string> pipelines)` returns `Fin<Unit>` — the built-provider half of the closure proof, probing every roster row through `TryGetPipeline`; `Run(LaneGuard.Runtime runtime, WorkLane lane, Func<CancellationToken, ValueTask<T>> work)` returns `IO<T>` — executes the in-process work outcome-first through the lane's keyed pipeline and folds every termination onto one `LaneFault` arm carrying its own typed evidence.
- Auto: each lane's pipeline is one keyed `ResiliencePipeline` registered through `AddResiliencePipeline(lane.PipelineKey, ...)` exactly as `KeyedLane.Register` registers per hop, but for the in-process command/solve edge so the lane and the hop share one resilience pattern and one retry-owner discipline — exactly one retry owner per lane just as each hop has exactly one; the bulkhead rides a `RateLimiterStrategyOptions` lease producer over the `ADAPTIVE_ARMS` `LanePermits` cell so the permit count is a live value the resize writes rather than a column frozen at build, the tenant row partitions admission BY TENANT — a second `RateLimiterStrategyOptions.RateLimiter` lease producer over a per-tenant `TokenBucketRateLimiter` keyed on the branch-settled `TenantContext.Entry` render, `TenantId.Wire` being that render's format specifier and never its value — so one tenant's burst bounds at its own bucket instead of consuming the lane's whole pool, the circuit-breaker binds a `CircuitBreakerManualControl` and `CircuitBreakerStateProvider` keyed per lane so the breaker state reads from Polly's own observation surface (never a parallel state delegate), and the hedge admits only idempotent commands so a duplicated non-idempotent solve never double-applies; the load-shed arm reads the atomic `DegradationReading` and sheds at the lane's degradation floor ahead of the pipeline entirely; the chaos row arms on the test-host profile only — `AddChaosLatency` and `AddChaosFault` over `ChaosLatencyStrategyOptions`/`ChaosFaultStrategyOptions` with an `InjectionRate` column, the Simmy builder extensions shipping inside `Polly.Core` itself so no second package row admits them — so the work lanes carry a first-class chaos surface symmetric to the transport `KeyedLane` chaos, never in production.
- Receipt: a lane execution's resilience events land under the lane key in the package meter and logger exactly as the keyed-pipeline events do — the lane key, the live shed level, and the tenant slot are `MeteringEnricher` tags on the measurement itself, never the DI registration key a query cannot group by, and `SeverityProvider` resolves the emitting strategy back through `LaneStrategy.TryGet` and reads that row's own `Severity` column; a shed decision fans the shed verdict (`SHED_VERDICT`); no parallel lane receipt.
- Packages: Polly.Core, Polly.Extensions, Polly.RateLimiting, Microsoft.Extensions.Diagnostics.ResourceMonitoring, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new lane is one `WorkLane` roster row plus the one `LanePolicy` the closure proof then demands, and NOTHING about its deadline — `Attempt` derives through `LaneClass` from the lane's own rank, `DeadlineClass.LaneAttempt` below the latency rank and `DeadlineClass.LaneFold` above it — so a row cannot spell a class its lane contradicts and a transport row can never reach the column to price an in-process fold on a socket's budget; a new resilience dimension is one `LaneStrategy` row carrying its arm and its severity, seated where its declaration order places it; a new measurement dimension is one `LaneEnricher` tag beside its `LaneTags` const; zero new surface.
- Boundary: the lane roster DECLARES at the spine and crosses downward — `Rasm.Compute/Runtime/scheduling` keys its `LaneBound` channel table and its `readers(CpuBudget)` reader budgets on `WorkLane.Items` through its legal S3 reference, so a row added here fails Compute's keyed fold loudly, and an AppHost project reference to `Rasm.Compute` is the deleted form that closed an S1-to-S3 cycle; columns only the solve path's own domain decides stay at that consumer, so the roster carries identity and `Rank` alone. `LaneClass` is the ONE seat where a lane meets a deadline class — every consumer that dispatches onto a `WorkLane` reads the binding rather than naming a class beside the lane, so the `Agent/capability#COMMAND_ALGEBRA` `Spec` fold picks a lane and takes the class that lane's rank already fixes, and a literal `DeadlineClass` beside a literal `WorkLane` at any dispatch seat is the deleted form that let a whole-model fold ride an interactive lane under a transport hop's budget; the same block owns `PipelineKey`, so the registration, the closure proof, the provider probe, and the dispatch all read ONE key spelling and a key-format edit cannot desynchronize the registry from its own proof — a `$"lane:{…}"` interpolation at any of those four sites is the deleted form. `LaneGuard` is the spine owner for the in-process command/solve edge, distinct from the transport `KeyedLane` — it must not become a second retry on the transport seam, so in-process lanes get exactly one retry owner (`LaneGuard`) just as each hop has exactly one and a retry both on the lane and re-applied at the hop on one seam is the rejected form (the one-retry-owner-per-seam discipline); a kernel fold reached from inside `Run` takes the token the work delegate is HANDED and seats it through that fold's own governance column — the arrangement band's `ArrangementPolicy.Governed(progress, token)` is the landed instance, mints no source of its own, and reads the token at each declared stage head — so the lane's timeout, breaker, and shed all reach the native lane through one token and a kernel-boundary `CancellationTokenSource` is the second owner this discipline forbids; `WorkLane` names the solve-path lane distinct from the `Runtime/resources#DRAIN_QUEUES` `DrainQueue` process-queue name, one altitude per name; the keyed-pipeline registry mirrors `Wire/outbound#KEYED_PIPELINES` `KeyedLane.Register`'s `AddResiliencePipeline`/`CircuitBreakerManualControl`/`CircuitBreakerStateProvider` pattern verbatim so the in-process and transport resilience share one shape, never a second registry pattern; the resilience meter carries the lane key as a TAG through `ConfigureTelemetry(TelemetryOptions)` — the `ILoggerFactory` overload sets a logger alone, so a page claiming a per-lane series behind it publishes none and the deleted form is that overload beside the claim; `SeverityProvider` returns `ResilienceEventSeverity` because the Polly callback contract fixes that type, so the kernel `AlertSeverity` ladder rides the `LaneFault` family at the receipt seam and never this callback; the two limiter rows both raise `RateLimiterRejectedException`, so the fold resolves `ResilienceTelemetrySource.StrategyName` back through the roster and a lane-pool refusal stays distinguishable from a tenant-bucket refusal; `AddChaosOutcome` is a result-typed builder surface the non-generic lane pipeline cannot reach, so outcome substitution is unavailable here by construction rather than omitted, and a generic per-result-type lane registration to buy it is the rejected form; chaos arms on the test-host profile only so a production lane carries zero chaos; the adaptive-permit cadence reads `TimeProvider` through `ClockPolicy`, never a direct `Stopwatch` call site; no `AddSingleton` spelling — the registry composes through `AddResiliencePipeline` exactly as the keyed transport registry does.

```csharp signature
// The lane roster DECLARES at the spine and crosses downward: `Rasm.Compute/Runtime/scheduling` keys its
// `LaneBound` channel table and its `readers(CpuBudget)` reader budgets on `WorkLane.Items` through its legal
// S3 reference, so a row added here breaks that keyed fold loudly and the spine never reaches upward for the
// vocabulary it governs. Identity and `Rank` are the whole roster — rank is the cross-lane precedence datum
// every consumer reads — and a column only the solve path's own domain decides stays at that consumer, so an
// external lane selector arriving as wire text admits through the generated `Validate`/`TryGet` key seam.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<Fault>]
public sealed partial class WorkLane {
    public static readonly WorkLane Interactive = new("interactive", rank: 1);
    public static readonly WorkLane Ranked = new("ranked", rank: 1);
    public static readonly WorkLane Background = new("background", rank: 2);
    public static readonly WorkLane Bulk = new("bulk", rank: 3);
    public static readonly WorkLane Benchmark = new("benchmark", rank: 4);
    public static readonly WorkLane CaptureIngest = new("capture-ingest", rank: 5);

    public int Rank { get; }
}

// The vocabulary IS the pipeline. Declaration order is the canonical strategy order the resilience law
// derives — admission counts logical calls so both limiters sit outermost, health statistics count attempts
// so the breaker sits above the deadline, each attempt earns a fresh deadline so the deadline sits inside,
// and chaos sits below everything it tests — and each row carries the arm that appends it plus the severity
// its events map to. `Register` folds `Items`, so a new resilience dimension is one row seated where its
// declaration places it and no builder body re-spells the order.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LaneStrategy {
    public static readonly LaneStrategy Bulkhead = new("bulkhead", ResilienceEventSeverity.Warning, ArmBulkhead);
    public static readonly LaneStrategy RateLimiter = new("rate-limiter", ResilienceEventSeverity.Warning, ArmTenant);
    public static readonly LaneStrategy CircuitBreaker = new("circuit-breaker", ResilienceEventSeverity.Error, ArmBreaker);
    public static readonly LaneStrategy Deadline = new("deadline", ResilienceEventSeverity.Warning, ArmDeadline);
    public static readonly LaneStrategy Hedge = new("hedge", ResilienceEventSeverity.Information, ArmHedge);
    public static readonly LaneStrategy Chaos = new("chaos", ResilienceEventSeverity.Information, ArmChaos);

    public ResilienceEventSeverity Severity { get; }

    // Each arm names its strategy with its own row key, so `(pipeline, strategy)` telemetry deduplicates per
    // row and the emitting row resolves back through `TryGet` at the severity callback and the outcome fold.
    [UseDelegateFromConstructor]
    public partial ResiliencePipelineBuilder Arm(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row);

    // The lane pool is a LIVE limiter the resize writes, reached per execution through the lease producer —
    // `AddConcurrencyLimiter` binds its permit count once at build, so a page claiming a runtime resize behind
    // it publishes none and that overload beside the claim is the deleted form.
    static ResiliencePipelineBuilder ArmBulkhead(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddRateLimiter(new RateLimiterStrategyOptions {
            Name = Bulkhead.Key,
            RateLimiter = args => composition.Permits.Of(row).AcquireAsync(1, args.Context.CancellationToken),
        });

    // The lane pool isolates BETWEEN lanes and never WITHIN one, so a second admission row partitions by
    // tenant: the lease producer keys a per-tenant TokenBucketRateLimiter on the branch-settled
    // TenantContext.Entry text — the fixed-width render itself, where TenantId.Wire is the FORMAT that
    // produces it — and a tenant's burst bounds at its own bucket instead of consuming the whole pool.
    static ResiliencePipelineBuilder ArmTenant(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddRateLimiter(new RateLimiterStrategyOptions {
            Name = RateLimiter.Key,
            RateLimiter = args => composition.Tenants.Limiter(TenantContext.Current.Entry).AcquireAsync(1, args.Context.CancellationToken),
        });

    static ResiliencePipelineBuilder ArmBreaker(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions {
            Name = CircuitBreaker.Key,
            ManualControl = LaneGuard.BreakerOf(row.Lane),
            StateProvider = LaneGuard.StateOf(row.Lane),
        });

    static ResiliencePipelineBuilder ArmDeadline(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddTimeout(new TimeoutStrategyOptions {
            Name = Deadline.Key,
            Timeout = composition.Allotted(row.Lane.Attempt),
        });

    static ResiliencePipelineBuilder ArmHedge(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        row.Hedges
            ? builder.AddHedging(new HedgingStrategyOptions { Name = Hedge.Key, MaxHedgedAttempts = LaneGuard.HedgeAttempts })
            : builder;

    // `AddChaosOutcome` is a result-typed builder surface the non-generic lane pipeline cannot reach, so the
    // lane's chaos plane is the exception rail and the time plane; buying outcome substitution would cost a
    // per-result-type registration the one-pipeline-per-lane law forecloses.
    static ResiliencePipelineBuilder ArmChaos(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        composition.Chaos
            ? builder
                .AddChaosLatency(new ChaosLatencyStrategyOptions {
                    Name = Chaos.Key,
                    InjectionRate = LaneGuard.ChaosInjectionRate,
                    Latency = composition.Allotted(row.Lane.Attempt),
                })
                .AddChaosFault(new ChaosFaultStrategyOptions {
                    Name = Chaos.Key,
                    InjectionRate = LaneGuard.ChaosInjectionRate,
                    FaultGenerator = static _ => new ValueTask<Exception?>(new LaneFault.LaneBroken("<chaos>").ToException()),
                })
            : builder;
}

[Union]
public abstract partial record LaneFault : Expected, IValidationError<LaneFault> {
    private LaneFault(string detail, int code) : base(detail, code, None) { }
    public static LaneFault Create(string message) => new Text(message);
    public sealed record Text : LaneFault { public Text(string detail) : base(detail, FaultBand.Lane.Code(0)) { } }

    // Both limiter rows raise one Polly exception type, so the refusing row rides as a typed field rather than
    // a message a caller re-parses, and the limiter's own retry-after hint rides beside it.
    public sealed record BulkheadRejected : LaneFault {
        public BulkheadRejected(string strategy, Option<Duration> retryAfter)
            : base($"<lane-rejected:{strategy}>", FaultBand.Lane.Code(1)) => (Strategy, RetryAfter) = (strategy, retryAfter);
        public string Strategy { get; }
        public Option<Duration> RetryAfter { get; }
    }

    public sealed record Shed : LaneFault { public Shed(string detail) : base(detail, FaultBand.Lane.Code(2)) { } }

    public sealed record Deadline : LaneFault {
        public Deadline(Duration span) : base($"<lane-deadline:{span}>", FaultBand.Lane.Code(3)) => Span = span;
        public Duration Span { get; }
    }

    public sealed record LaneBroken : LaneFault { public LaneBroken(string detail) : base(detail, FaultBand.Lane.Code(4)) { } }
}

// The two spine projections off the roster, seated together because neither is a datum the roster carries.
// The lane-to-deadline BINDING is executable rather than asserted: a latency-ranked lane is bounded by the
// attempt class and a throughput-ranked one by the fold class, and the discriminant is the lane's OWN rank,
// so no row spells its own class, no profile overrides it, and a transport row can never reach the column to
// price an in-process fold on a socket's budget. `PipelineKey` is the resilience-registry identity — ONE
// spelling the registration, the closure proof, the provider probe, and the dispatch all read, so a format
// edit cannot desynchronize the registry from the proof that covers it.
public static class LaneClass {
    public const int LatencyRank = 1;

    extension(WorkLane lane) {
        public DeadlineClass Attempt => lane.Rank <= LatencyRank ? DeadlineClass.LaneAttempt : DeadlineClass.LaneFold;

        public string PipelineKey => $"lane:{lane.Key}";
    }
}

public sealed record LanePolicy(
    WorkLane Lane,
    int BulkheadPermits,
    int QueueLimit,
    bool Hedges,
    DegradationLevel ShedFloor);

public static class LaneGuard {
    public const int BulkheadPermits = 32;
    public const int QueueLimit = 64;
    public const int HedgeAttempts = 2;
    public const double ChaosInjectionRate = 0.05d;
    static readonly ConcurrentDictionary<WorkLane, (CircuitBreakerManualControl Control, CircuitBreakerStateProvider State)> Breakers = new();
    static (CircuitBreakerManualControl Control, CircuitBreakerStateProvider State) Seat(WorkLane lane) =>
        Breakers.GetOrAdd(lane, static _ => (new CircuitBreakerManualControl(), new CircuitBreakerStateProvider()));
    public static CircuitBreakerManualControl BreakerOf(WorkLane lane) => Seat(lane).Control;
    public static CircuitBreakerStateProvider StateOf(WorkLane lane) => Seat(lane).State;

    // Two altitudes, two records: `Composition` is what the strategy arms read while the collection is still
    // editable, `Runtime` is what a dispatch reads off the built provider. Neither is reachable from the other,
    // which is what keeps a registration-time value from leaking into a per-execution decision.
    public sealed record Composition(
        ILoggerFactory Telemetry,
        Func<DeadlineClass, TimeSpan> Allotted,
        Func<DegradationReading> Pressure,
        TenantLimiters Tenants,
        LanePermits Permits,
        bool Chaos);

    public sealed record Runtime(
        ResiliencePipelineProvider<string> Pipelines,
        Func<DegradationReading> Pressure,
        Func<WorkLane, DegradationLevel> Floor);

    // The per-tenant lease producer: one TokenBucketRateLimiter per `TenantContext.Entry` text, minted once and
    // reused, so a lane's admission partitions on the SAME identity the meter tag, the RLS predicate, and the
    // receipt fold key on — never a second tenant alphabet. Admission keys the ROOT row too, since an
    // unpartitioned process still owns one bucket; absence is a telemetry arm, never a bucket the pool skips.
    public sealed class TenantLimiters(TokenBucketRateLimiterOptions options) {
        readonly ConcurrentDictionary<string, RateLimiter> byTenant = new(StringComparer.Ordinal);
        public RateLimiter Limiter(string tenant) => byTenant.GetOrAdd(tenant, _ => new TokenBucketRateLimiter(options));
    }

    // The lane's own dimensions on every resilience measurement: the lane key the page's Receipt clause
    // promises, the live shed level, and the frame's own tenancy projection — so a per-lane resilience series
    // is real and joins the rest of the estate's telemetry on one grammar. Tenancy SPREADS the kernel pair's
    // `Tags` rather than spelling key and value here: that projection is empty for the root row, so an
    // unpartitioned process tags nothing instead of stamping the zero-hex text as a tenant that exists, and
    // key and render both stay single-owned where the branch ruling seats them.
    public sealed class LaneEnricher(LanePolicy row, Func<DegradationReading> pressure) : MeteringEnricher {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context) {
            context.Tags.Add(new(LaneTags.Lane, row.Lane.Key));
            context.Tags.Add(new(LaneTags.Shed, ShedVerdict.Of(pressure(), row.Lane, row.ShedFloor).Level.Key));
            foreach (KeyValuePair<string, object?> tenancy in TenantContext.Current.Tags) {
                context.Tags.Add(tenancy);
            }
        }
    }

    public static class LaneTags {
        public const string Lane = "rasm.lane.key";
        public const string Shed = "rasm.lane.shed";
    }

    // Registration is boot-strict: the roster proves closed BEFORE a single pipeline registers, so a lane row
    // added at the roster fails composition with the lane named rather than throwing at its first dispatch —
    // the same discriminant the folder ruling already settled for unregistered latency names.
    public static Fin<IServiceCollection> Register(IServiceCollection services, Composition composition, params ReadOnlySpan<LanePolicy> rows) =>
        Closed(Iterable<LanePolicy>.FromSpan(rows).ToSeq())
            .Map(seated => seated.Fold(services, (graph, row) =>
                graph.AddResiliencePipeline(row.Lane.PipelineKey, builder =>
                    ignore(toSeq(LaneStrategy.Items).Fold(
                        // The ILoggerFactory overload sets a logger and NOTHING else, so the promised per-lane
                        // series would not exist as a metric dimension behind it. The options overload is where
                        // the estate's grammar reaches the meter — MeteringEnrichers append the lane key, the
                        // shed level, and the tenant slot per the branch dimension-key ruling, and the severity
                        // map resolves the emitting strategy back to its own roster row's declared column.
                        builder.ConfigureTelemetry(new TelemetryOptions {
                            LoggerFactory = composition.Telemetry,
                            MeteringEnrichers = { new LaneEnricher(row, composition.Pressure) },
                            SeverityProvider = static args =>
                                LaneStrategy.TryGet(args.Source.StrategyName ?? string.Empty, out LaneStrategy? strategy)
                                    ? strategy!.Severity
                                    : ResilienceEventSeverity.Information,
                        }),
                        (chain, strategy) => strategy.Arm(chain, composition, row))))));

    // The roster half of the closure proof, accumulating so every open lane reports at one boot rather than
    // one per attempt. The reverse direction needs no fold: `LanePolicy.Lane` is typed `WorkLane`, so a policy
    // naming a lane the roster does not carry is unconstructible. The refusal is `Fault.InvalidValue` on the
    // validation band and never a `LaneFault` arm — a missing composition row is a BOOT fault, and the lane
    // family carries dispatch faults alone.
    static Fin<Seq<LanePolicy>> Closed(Seq<LanePolicy> rows) =>
        toSeq(WorkLane.Items)
            .Traverse(lane => rows.Filter(row => row.Lane == lane).Count is 1
                ? Validation<Error, Unit>.Success(unit)
                : new Fault.InvalidValue(Label: lane.Key, Requirement: "<exactly-one-lane-policy>"))
            .As()
            .Map(_ => rows)
            .ToFin();

    // The provider half: a pipeline exists only after `BuildServiceProvider`, so the composition root's
    // built-provider gate runs this fold. `TryGetPipeline` is the non-throwing probe where `GetPipeline`
    // raises, so an unmaterialized lane refuses at the same boot band the roster fold does.
    public static Fin<Unit> Proven(ResiliencePipelineProvider<string> pipelines) =>
        toSeq(WorkLane.Items)
            .Traverse(lane => pipelines.TryGetPipeline(lane.PipelineKey, out _)
                ? Validation<Error, Unit>.Success(unit)
                : new Fault.InvalidValue(Label: lane.PipelineKey, Requirement: "<a built lane pipeline>"))
            .As()
            .Map(static _ => unit)
            .ToFin();

    // Dispatch sheds BEFORE the pipeline, then folds outcome-first: strategies never see an in-flight
    // exception, and every termination lands one arm carrying its own typed evidence — the refusing limiter
    // row and its retry-after hint, the deadline's span — so escalation matches the case and reads the field.
    public static IO<T> Run<T>(Runtime runtime, WorkLane lane, Func<CancellationToken, ValueTask<T>> work) =>
        ShedVerdict.Of(runtime.Pressure(), lane, runtime.Floor(lane)) is { Shed: true } verdict
            ? IO.fail<T>(new LaneFault.Shed($"{lane.PipelineKey}:{verdict.Level.Key}"))
            : runtime.Pipelines.TryGetPipeline(lane.PipelineKey, out ResiliencePipeline? pipeline)
                ? IO.liftAsync(env => Executed(pipeline!, lane, work, env.Token)).Bind(Lifted)
                : IO.fail<T>(new LaneFault.LaneBroken($"<unregistered-lane:{lane.PipelineKey}>"));

    // Named boundary capsule: the pooled-context lease `try`/`finally` and the outcome-capture kernel are the
    // platform-forced statement seam, and `OperationKey` fixes at lease so every strategy event and the
    // execution correlate on the lane key without a join.
    static async ValueTask<Outcome<T>> Executed<T>(
        ResiliencePipeline pipeline, WorkLane lane, Func<CancellationToken, ValueTask<T>> work, CancellationToken token) {
        ResilienceContext context = ResilienceContextPool.Shared.Get(lane.PipelineKey, token);
        try {
            return await pipeline.ExecuteOutcomeAsync(
                static async (ctx, state) => {
                    try { return Outcome.FromResult(await state(ctx.CancellationToken).ConfigureAwait(false)); }
                    catch (Exception ex) when (ex is not OutOfMemoryException) { return Outcome.FromException<T>(ex); }
                },
                context, work).ConfigureAwait(false);
        }
        finally { ResilienceContextPool.Shared.Return(context); }
    }

    static IO<T> Lifted<T>(Outcome<T> outcome) => outcome switch {
        { Exception: null, Result: { } value } => IO.pure(value),
        { Exception: RateLimiterRejectedException rejected } => IO.fail<T>(new LaneFault.BulkheadRejected(
            Emitting(rejected.TelemetrySource), Optional(rejected.RetryAfter).Map(Duration.FromTimeSpan))),
        { Exception: TimeoutRejectedException slow } => IO.fail<T>(new LaneFault.Deadline(Duration.FromTimeSpan(slow.Timeout))),
        { Exception: BrokenCircuitException open } => IO.fail<T>(new LaneFault.LaneBroken(Emitting(open.TelemetrySource))),
        { Exception: { } foreign } => IO.fail<T>(new LaneFault.Text(foreign.Message)),
        _ => IO.fail<T>(new LaneFault.Text("<empty-outcome>")),
    };

    // The refusing strategy resolves back to its own roster row, so a lane-pool refusal and a tenant-bucket
    // refusal stay distinguishable at the rail even though Polly raises one exception type for both.
    static string Emitting(ResilienceTelemetrySource? source) =>
        LaneStrategy.TryGet(source?.StrategyName ?? string.Empty, out LaneStrategy? strategy) ? strategy!.Key : "<unnamed-strategy>";
}
```

## [03]-[ADAPTIVE_ARMS]

- Owner: `AdaptiveConcurrency` the static ResourceMonitoring-fed permit-resize projection; `LanePermits` the live per-lane limiter cell the lane's admission row leases from and the resize is the sole writer of; `LoadShed` the static `DegradationReading`-fed shed arm.
- Entry: `Resize(LanePolicy policy, Utilization utilization)` returns `int` — projects the lane's permit count from the live CPU/memory utilization so a pressured host narrows the lane and an idle host widens it; `Of(LanePolicy row)` returns `RateLimiter` — the lease producer the `LaneStrategy.Bulkhead` arm binds, re-seating the lane's `ConcurrencyLimiter` when a resize moves the count and returning the seated one otherwise; `Drain()` returns `IO<Unit>` — the `DrainParticipantPort` body releasing every retired limiter; `Shed(LanePolicy policy, DegradationReading reading)` returns `bool` — the load-shed verdict reads the atomic `DegradationReading` and sheds when the derived level meets or exceeds the lane's `ShedFloor`.
- Auto: the resize reads the `Observability/health#HEALTH_FOLD` `UtilizationCell` CPU and memory ratios graded against the `ResourceQuota` container limit — the composition root supplies `UtilizationCell.Read` as the cell's utilization function — so the permit resize rides the same observable-instrument-and-quota path the host pressure grade reads, never a parallel meter, and a lane under cgroup throttling narrows on the limit it actually runs under; the resize decision is cadence-gated on a `ClockPolicy` mark rather than a timer, so the lease producer runs per execution while the utilization read and the limiter mint run once per interval; a moved count seats a fresh `ConcurrencyLimiter` and PARKS the retired instance instead of disposing it, because a `RateLimitLease` releases against the instance that issued it and disposing under an outstanding lease strands the permit; the load-shed arm reads the one `DegradationReading` (the collapsed `(snapshot, level)` cell) so the shed decision reads a coherent pressure value and a lane never sheds on a stale snapshot against a fresh level — the prior two-surface read is the collapsed form the shed arm depends on; the resize is bounded between a floor and the lane's configured permits so adaptive concurrency tunes within a band, never to zero; the shed floor is a `DegradationLevel` row so a lane sheds at its own floor and a `Suspended` host sheds every non-critical lane through the existing degradation rail, never a parallel throttle.
- Receipt: the parked limiters release inside the `DrainBand.Telemetry` participant row so a retirement is drain evidence, never a finalizer-thread release; the resize itself mints no receipt — the live permit count reaches the meter as the lane's own resilience series.
- Packages: Microsoft.Extensions.Diagnostics.ResourceMonitoring, LanguageExt.Core, BCL inbox
- Growth: a new utilization signal is one enabled instrument the `UtilizationCell` reads; a new shed input is one column on the `DegradationReading` read; zero new surface.
- Boundary: the adaptive arms read the existing health owners — the `UtilizationCell` for utilization and the `DegradationReading` for the shed level — never a second resource meter or a second pressure cell; the resize projection is pure and the cell is its ONLY writer, so a permit count computed and applied nowhere is the deleted form this seam exists to foreclose; `Utilization` is the value the cell's supplied function yields and the projection never reaches the cell type itself, so the health page owns the read and this page owns the response; the load-shed arm reads the atomic `DegradationReading` so its decision is race-free, the exact reason the `COLLAPSE_HEALTH_DEGRADATION_CELL` collapse exists; the resize tunes within a band so the lane never starves or floods; the shed floor is a `DegradationLevel` row so the shed and the degradation rail share one level vocabulary, never two.

```csharp signature
public static class AdaptiveConcurrency {
    public const int MinPermits = 4;
    // Named policy consts per the const discipline — an inline CPU threshold is the deleted literal.
    public const double CpuSaturated = 0.90d;
    public const double CpuPressured = 0.75d;

    public static int Resize(LanePolicy policy, Utilization utilization) =>
        utilization.CpuRatio is var cpu && cpu >= CpuSaturated ? MinPermits
        : cpu >= CpuPressured ? int.Max(policy.BulkheadPermits / 2, MinPermits)
        : policy.BulkheadPermits;
}

// The runtime-resizable permit source. The lease producer runs PER EXECUTION, so the live limiter is a value
// this cell publishes and `Resize` is its only writer — which is what makes the page's runtime-resize claim a
// seam rather than a sentence. The cadence gate keeps the cost honest: an unelapsed lane returns its seated
// limiter with no utilization read and no mint.
public sealed class LanePermits(ClockPolicy clocks, Func<Utilization> utilization) {
    public static readonly Duration ResizeInterval = Duration.FromSeconds(5);

    readonly ConcurrentDictionary<WorkLane, Seat> seats = new();
    readonly Atom<Seq<ConcurrencyLimiter>> parked = Atom(Seq<ConcurrencyLimiter>());

    readonly record struct Seat(int Permits, long Mark, ConcurrencyLimiter Limiter);

    public RateLimiter Of(LanePolicy row) =>
        seats.AddOrUpdate(row.Lane, _ => Seated(row, row.BulkheadPermits), (_, held) => Resized(row, held)).Limiter;

    Seat Resized(LanePolicy row, Seat held) =>
        clocks.Elapsed(held.Mark) < ResizeInterval
            ? held
            : Resize(row, utilization()) is var sized && sized == held.Permits
                ? held with { Mark = clocks.Mark() }
                : (parked.Swap(rows => rows.Add(held.Limiter)), Seated(row, sized)).Item2;

    Seat Seated(LanePolicy row, int permits) =>
        new(permits, clocks.Mark(), new ConcurrencyLimiter(new ConcurrencyLimiterOptions {
            PermitLimit = permits,
            QueueLimit = row.QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        }));

    // Retired limiters release at the drain band and never at the swap: a lease returns to the instance that
    // issued it, so disposing under an outstanding lease is the stranded permit this parking avoids.
    public IO<Unit> Drain() =>
        IO.lift(() => parked.Swap(static _ => Seq<ConcurrencyLimiter>()))
            .Bind(static rows => rows.TraverseM(static limiter =>
                IO.liftAsync(async () => { await limiter.DisposeAsync(); return unit; })).As())
            .Map(static _ => unit);
}

public static class LoadShed {
    // One shed-decision owner: the floor comparison lives on ShedVerdict.Of so the per-lane arm and the
    // cross-Compute verdict can never disagree — LoadShed reads the policy's lane+floor onto the one verdict.
    public static bool Shed(LanePolicy policy, DegradationReading reading) =>
        ShedVerdict.Of(reading, policy.Lane, policy.ShedFloor).Shed;
}
```

## [04]-[SHED_VERDICT]

- Owner: `ShedVerdict` the per-`WorkLane` shed-decision record minted once here and crossed to Compute admission.
- Entry: `Of(DegradationReading reading, WorkLane lane, DegradationLevel floor)` returns `ShedVerdict` — mints the per-lane shed verdict from the atomic `DegradationReading` against the lane's own configured `ShedFloor`, carrying the lane, the derived level, and the shed flag, so the shed decision is computed once at the in-process governor (the one floor-comparison owner the per-lane `LoadShed.Shed` arm and `LaneGuard.Run` both fold through) and consumed downstream rather than re-derived from raw saturation.
- Auto: the verdict reads the one atomic `DegradationReading` so the shed flag and the level it derives from are coherent, never a stale-snapshot-against-fresh-level race; host pressure is global and the per-lane axis is the FLOOR the lane's own `LanePolicy` declares, so the verdict compares one reading against one lane's threshold and the lane it names is the row that threshold belongs to — a per-lane pressure read would grade a resource no lane owns; the verdict is minted once at the in-process governor edge and crosses to `Rasm.Compute/Runtime/admission` as the one shed verdict the Compute `SubstrateSelection` fold consumes on its admission decision, so the Compute side never re-derives the shed from raw saturation — one verdict, one mint, two consumers (the in-process `LaneGuard.Run` shed and the Compute admission shed); a lane below its shed floor admits, at or above sheds.
- Receipt: the shed verdict is the cross-package fact the Compute admission consumes; no parallel verdict receipt.
- Packages: LanguageExt.Core, BCL inbox
- Growth: a new shed input is one column on the verdict; zero new surface.
- Boundary: the shed verdict is the one per-`WorkLane` shed fact — the per-lane shed verdict `LaneGuard` mints from the atomic `DegradationReading` is the one verdict `Rasm.Compute/Runtime/admission` consumes on its `SubstrateSelection` fold rather than a Compute-side re-derivation (the `ONE_DEGRADATION_SHED_VERDICT` ripple), so the in-process lane shed and the Compute admission shed read one verdict and a Compute-side re-derivation from raw saturation is the rejected form; the verdict reads the atomic `DegradationReading` so it is race-free, the reason the health-cell collapse exists; the consumer count stays TWO under a kernel fold, because `Run` sheds BEFORE it invokes the work delegate — a shed lane never enters the fold, so the kernel governance band inherits the decision by absence and a third verdict read seated below the lane grades pressure the caller already refused on.

```csharp signature
// The one per-WorkLane shed verdict minted from the atomic DegradationReading against the lane's own
// configured ShedFloor — consumed by both the in-process LaneGuard.Run shed and Rasm.Compute/Runtime/
// admission's SubstrateSelection fold, never re-derived Compute-side. The shed decision IS this verdict
// (the LoadShed floor comparison folds in here), so a lane sheds at its own floor and the Compute admission
// reads the identical per-lane verdict. The seam couples to this verdict shape, not the DegradationCell interior.
public readonly record struct ShedVerdict(WorkLane Lane, DegradationLevel Level, bool Shed) {
    public static ShedVerdict Of(DegradationReading reading, WorkLane lane, DegradationLevel floor) =>
        new(lane, reading.Level, reading.Level.Rank >= floor.Rank);
}
```

## [05]-[RESEARCH]

(none)
