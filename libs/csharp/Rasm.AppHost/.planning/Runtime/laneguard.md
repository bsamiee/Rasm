# [APPHOST_WORK_LANE_GOVERNOR]

`LaneGuard` is the work-lane vocabulary owner and in-process resilience governor: the `WorkLane` roster DECLARES here and one keyed Polly `ResiliencePipeline` composes per roster row, bracketing each lane with per-tenant admission, an adaptive bulkhead, circuit health, one allotment deadline, and a runtime-armed `Polly.Simmy` chaos block. Admission mints once as one per-`WorkLane` verdict over the atomic `DegradationReading` and the lane's own breaker evidence, then crosses to `Rasm.Compute/Runtime/admission` (the `ONE_DEGRADATION_SHED_VERDICT` ripple) rather than a Compute-side re-derivation.

Lanes run SINGLE-PASS and re-attempt nothing, so an allotment collapses to one span and in-process retry stays the `Schedule` policy the effect rail carries inside the work delegate. `LaneGuard` owns the lane roster, its deadline and pipeline-key projections, the registry and its closure proof, the adaptive and load-shed arms, the chaos fold, and the admission verdict; it consumes `Observability/health#DEGRADATION_RAIL` `DegradationReading`/`DegradationLevel`, `UtilizationCell`/`Utilization`, `DeadlineClass`/`ClockPolicy`, and `TenantContext` as settled vocabulary, minting no eighth port.

## [01]-[INDEX]

- [02]-[LANE_GUARD]: `WorkLane` roster and one keyed Polly `ResiliencePipeline` per row, proven closed at boot.
- [03]-[ADAPTIVE_ARMS]: ResourceMonitoring-fed resizable permit source under one cadence gate.
- [04]-[SHED_VERDICT]: Per-`WorkLane` admission verdict minted once and crossed to Compute admission.

## [02]-[LANE_GUARD]

- Owner: `WorkLane` `[SmartEnum<string>]` the six-row lane roster — identity and `Rank` — under the `ComparerAccessors.StringOrdinal` accessor; `LaneStrategy` `[SmartEnum<string>]` the pipeline-row vocabulary whose declaration order IS the strategy order and whose delegate column IS each strategy's arm; `LanePolicy` the per-lane allotment row every strategy knob derives from; `LaneClass` the rank-to-deadline and pipeline-key projections off the roster; `LaneGuard` the static keyed-pipeline registry over the in-process lanes; `LaneFault` `[Union]` fault family deriving its codes through `FaultBand.Lane` = Text | CallerLeft | Rejected | Shed | Deadline | Dark | Broken.
- Entry: `Register(IServiceCollection services, LaneGuard.Composition composition, params ReadOnlySpan<LanePolicy> rows)` returns `Fin<IServiceCollection>` — proves the roster closed against the supplied rows, then folds one `AddResiliencePipeline` entry per row keyed by `WorkLane.PipelineKey`, each pipeline composed by folding `LaneStrategy.Items` in declaration order; `Proven(ResiliencePipelineProvider<string> pipelines, Func<DegradationReading> pressure, params ReadOnlySpan<LanePolicy> rows)` returns `Fin<LaneGuard.Runtime>` — the built-provider half of the closure proof, MINTING the dispatch runtime it proved so a `Runtime` is unconstructible except through the proof covering it; `Run(LaneGuard.Runtime runtime, WorkLane lane, Func<CancellationToken, ValueTask<T>> work)` returns `IO<T>` — executes the in-process work outcome-first through the lane's resolved pipeline and folds every termination onto one `LaneFault` arm carrying its own typed evidence.
- Auto: each lane's pipeline is one keyed `ResiliencePipeline` registered through `AddResiliencePipeline(lane.PipelineKey, ...)` exactly as `KeyedLane.Register` registers per hop, so the lane and the hop share one resilience pattern; the tenant row partitions admission BY TENANT AND BY LANE — a `RateLimiterStrategyOptions` lease producer over the BCL's own `PartitionedRateLimiter` whose partition key IS the `(lane, tenant)` pair and whose bucket knobs derive from the lane's own row, the tenant half being the branch-settled `TenantContext.Entry` render threaded through `ResilienceContext.Properties`, `TenantId.Wire` being that render's format specifier and never its value; the bulkhead rides a second lease producer over the `ADAPTIVE_ARMS` `LanePermits` cell so the permit count is a live value the resize writes rather than a column frozen at build; the circuit breaker binds the composition's ONE group `CircuitBreakerManualControl` beside a per-lane `CircuitBreakerStateProvider`, so operator darkness acts on the whole in-process capability group as one verb while health evidence stays a per-seat read; the load-shed arm reads the atomic `DegradationReading` beside that evidence and refuses at the lane's degradation floor ahead of the pipeline entirely; the chaos row folds the `Runtime/determinism#ADVERSARIAL_PROBE` bands its lane declares straight onto the Simmy builder verbs, so injection gates per execution at that owner's seeded `EnabledGenerator` and this page writes no options body, the Simmy builder extensions shipping inside `Polly.Core` itself so no second package row admits them.
- Receipt: a lane execution's resilience events land under the lane key in the package meter and logger exactly as the keyed-pipeline events do — the lane key, the live degradation level, and the tenant slot are `MeteringEnricher` tags on the measurement itself, never the DI registration key a query cannot group by, and the severity map re-ranks only the events Polly already grades a problem; a refused admission fans the verdict (`SHED_VERDICT`); no parallel lane receipt.
- Packages: Polly.Core, Polly.Extensions, Polly.RateLimiting, System.Threading.RateLimiting, Microsoft.Extensions.Diagnostics.ResourceMonitoring, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new lane is one `WorkLane` roster row and the one `LanePolicy` the closure proof then demands, and NOTHING about its deadline — `Attempt` derives through `LaneClass` from the lane's own rank, `DeadlineClass.LaneAttempt` below the latency rank and `DeadlineClass.LaneFold` above it — so a row cannot spell a class its lane contradicts and a transport row can never reach the column to price an in-process fold on a socket's budget; a new resilience dimension is one `LaneStrategy` row carrying its arm and its severity, seated where its declaration order places it; a new posture edit is one `LanePolicy` column, every knob below it deriving; a new measurement dimension is one `LaneEnricher` tag beside its `LaneTags` const; zero new surface.
- Boundary: the lane roster DECLARES at the spine and crosses downward — `Rasm.Compute/Runtime/scheduling` keys its `LaneBound` channel table and its `readers(CpuBudget)` reader budgets on `WorkLane.Items` through its legal S3 reference, so a row added here fails Compute's keyed fold loudly, and an AppHost project reference to `Rasm.Compute` is the deleted form that closed an S1-to-S3 cycle; columns only the solve path's own domain decides stay at that consumer, so the roster carries identity and `Rank` alone. `LaneClass` is the ONE seat where a lane meets a deadline class — every consumer that dispatches onto a `WorkLane` reads the binding rather than naming a class beside the lane, so the `Agent/capability#COMMAND_ALGEBRA` `Spec` fold picks a lane and takes the class that lane's rank already fixes, and a literal `DeadlineClass` beside a literal `WorkLane` at any dispatch seat is the deleted form that let a whole-model fold ride an interactive lane under a transport hop's budget; the same block owns `PipelineKey`, so the registration, the closure proof, the resolved seat, and the dispatch all read ONE key spelling and a key-format edit cannot desynchronize the registry from its own proof — a `$"lane:{…}"` interpolation at any of those sites is the deleted form.
- Boundary: `LaneGuard` RE-ATTEMPTS NOTHING — the lane carries no retry row and no hedge row, so its allotment collapses to the one span a single-pass bracket admits and the in-process retry owner is the `Schedule` policy the effect rail runs INSIDE the work delegate, one bracket around one schedule; three refutations hold the split, and each is a named failure mode: a retry inside the lane sits below a limiter whose one lease spans every attempt, so a retrying lane holds a permit through N attempts and starves exactly the pool it rations; a re-attempted in-process solve re-spends CPU the bulkhead is rationing against the same deterministic input; and a lane-side loop above the delegate's own schedule multiplies attempts by m invisibly. Hedging is unreachable by construction beside all three — `AddHedging` binds `ResiliencePipelineBuilder<TResult>` alone and no non-generic `HedgingStrategyOptions` exists — the same result-typed foreclosure that puts `AddChaosOutcome` out of reach, so outcome substitution and concurrent duplication are both unavailable here rather than omitted, and a generic per-result-type lane registration to buy either is the rejected form. Allotment inheritance runs one way: the lane is the OUTERMOST in-process budget and inherits nothing, while a hop dialed from inside a lane takes the minimum of its own class and the lane's remainder at the hop's own owner.
- Boundary: declaration order is the policy and its derivation is stated at the roster, so a reorder that reads as a preference is a policy change with a named failure mode — the per-tenant bucket sits outside the lane pool because a tenant already over its own budget must refuse before it acquires and releases lane permits, and the inverse lets one tenant set the queue depth every other tenant waits behind; both limiters sit outside the breaker because admission counts logical calls while health statistics count attempts; the one deadline sits inside the breaker because the breaker's whole transient class IS the deadline it observes, and a deadline outside it leaves the breaker blind to the only infrastructural failure an in-process lane produces; chaos sits below everything it tests. Breaker health counts `TimeoutRejectedException` alone: a domain outcome rides `T` and never throws, so a caller-fault exception escaping the delegate fails that ONE call through the fold's open tail without moving the lane's health statistics — counting it darkens a whole lane for every tenant on one malformed input. Latency chaos therefore tests the breaker, fault chaos tests the fold's open tail, and behavior chaos tests what the delegate tolerates before it runs — one plane each.
- Boundary: chaos COMPOSES `Runtime/determinism#ADVERSARIAL_PROBE` and mints nothing — `ChaosArming` owns the per-execution conjunction, `ChaosBand`/`ChaosRow` carry each plane with its rate and weighted catalogue, and `ChaosPosture` is that owner's kill-and-scale cell, so an options body written here, a local posture record, and a second decision seat are three deleted forms; `Randomizer` is handed no context and therefore cannot be addressed at all, so the roll settles at `EnabledGenerator` — the one delegate the package hands a `ResilienceContext` on every execution — while `InjectionRate` pins open and `Randomizer` pins to a constant, because leaving the package's own thread-safe default in the chain reads correct and voids replay. Three planes reach this non-generic pipeline: latency spends the time plane, fault injects the exception rail, and behavior runs a side effect before the call, `AddChaosBehavior` constraining `ResiliencePipelineBuilderBase` so the lane reaches it, while result substitution stays foreclosed by its result-typed builder. Fault WEIGHTS declare as `ChaosRow` rows and the PICK rides `ChaosBand.Weighted` over the seeded draw, since the package's own `FaultGenerator` builds its selection through an internal helper no options member can substitute — a catalogue built through it picks a different row every run beneath a gate that reads perfectly deterministic; a band's rows resolve through `LaneGuard.ChaosFaults`, so an injected fault stays this page's own vocabulary and rides the fold's open tail.
- Boundary: `LaneGuard` is the spine owner for the in-process command/solve edge, distinct from the transport `KeyedLane`, and the keyed-pipeline registry mirrors `Wire/outbound#KEYED_PIPELINES` `KeyedLane.Register`'s `AddResiliencePipeline`/`CircuitBreakerManualControl`/`CircuitBreakerStateProvider` pattern verbatim so the in-process and transport resilience share one shape, never a second registry pattern; a kernel fold reached from inside `Run` takes the token the work delegate is HANDED and seats it through that fold's own governance column — the arrangement band's `ArrangementPolicy.Governed(progress, token)` is the landed instance, mints no source of its own, and reads the token at each declared stage head — so the lane's deadline, breaker, and shed all reach the native lane through one token and a kernel-boundary `CancellationTokenSource` is the second owner this discipline forbids; `WorkLane` names the solve-path lane distinct from the `Runtime/resources#DRAIN_QUEUES` `DrainQueue` process-queue name, one altitude per name; the resilience meter carries the lane key as a TAG through `ConfigureTelemetry(TelemetryOptions)` — the `ILoggerFactory` overload sets a logger alone, so a page claiming a per-lane series behind it publishes none and the deleted form is that overload beside the claim; `SeverityProvider` returns `ResilienceEventSeverity` because the Polly callback contract fixes that type, so the kernel `AlertSeverity` ladder rides the `LaneFault` family at the receipt seam and never this callback; the two limiter rows both raise `RateLimiterRejectedException`, so the fold resolves `ResilienceTelemetrySource.StrategyName` back through the roster and a lane-pool refusal stays distinguishable from a tenant-bucket refusal; a limiter reached through a lease producer is COMPOSITION-owned and the package disposes none of it — `RateLimiterResilienceStrategy` disposes only the wrapper it builds on the options' null-`RateLimiter` branch — so both admission cells release at the one `DrainBand.Telemetry` participant, the permit cell parking its retired instances and the tenant cell disposing its partitioned owner, and a bucket holding a replenishment timer for process lifetime under unbounded tenant cardinality is the deleted form; the composition registers `TimeProvider` in the container so every registry pipeline's sampling window, break duration, and injected latency ride the one `ClockPolicy` clock, and a per-builder assignment beside it is the second injection point; no `AddSingleton` spelling — the registry composes through `AddResiliencePipeline` exactly as the keyed transport registry does.

```csharp signature
// Lane roster DECLARES at the spine and crosses downward: `Rasm.Compute/Runtime/scheduling` keys its
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

// This vocabulary IS the pipeline, and declaration order is the canonical strategy order the resilience law
// derives rather than a convention a reader could reshuffle. Each row carries its own arm beside the rank
// its refusal reports at, and `Register` folds `Items`, so a new resilience dimension is one row seated
// where its declaration places it and no builder body re-spells the order. Chaos is ONE row because its
// planes are `ChaosBand` data at the probe owner, which names each injected strategy by its own plane, so
// `(pipeline.name, strategy.name)` already separates the series a shared row would otherwise merge.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LaneStrategy {
    public static readonly LaneStrategy Tenant = new("tenant", ResilienceEventSeverity.Warning, ArmTenant);
    public static readonly LaneStrategy Bulkhead = new("bulkhead", ResilienceEventSeverity.Warning, ArmBulkhead);
    public static readonly LaneStrategy Breaker = new("breaker", ResilienceEventSeverity.Error, ArmBreaker);
    public static readonly LaneStrategy Deadline = new("deadline", ResilienceEventSeverity.Warning, ArmDeadline);
    public static readonly LaneStrategy Chaos = new("chaos", ResilienceEventSeverity.Information, ArmChaos);

    public ResilienceEventSeverity Severity { get; }

    // Each arm names its strategy with its own row key, so `(pipeline, strategy)` telemetry deduplicates per
    // row and the emitting row resolves back through `TryGet` at the severity callback and the outcome fold.
    [UseDelegateFromConstructor]
    public partial ResiliencePipelineBuilder Arm(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row);

    // Tenancy reaches the lease producer through the typed context property `Run` fixes at lease, never
    // through the ambient slot: a strategy runs on whichever execution context Polly hands it, so an ambient
    // read is correct only where the seam happens to be linear and silently answers root where it is not.
    static ResiliencePipelineBuilder ArmTenant(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddRateLimiter(new RateLimiterStrategyOptions {
            Name = Tenant.Key,
            RateLimiter = args => composition.Tenants.Lease(
                row,
                composition.Allotted(row.Lane.Attempt),
                args.Context.Properties.GetValue(LaneGuard.TenantKey, TenantContext.Root.Entry),
                args.Context.CancellationToken),
        });

    // Lane pool is a LIVE limiter the resize writes, reached per execution through the lease producer —
    // `AddConcurrencyLimiter` binds its permit count once at build, so a page claiming a runtime resize behind
    // it publishes none and that overload beside the claim is the deleted form.
    static ResiliencePipelineBuilder ArmBulkhead(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddRateLimiter(new RateLimiterStrategyOptions {
            Name = Bulkhead.Key,
            RateLimiter = args => composition.Permits.Of(row).AcquireAsync(permitCount: 1, args.Context.CancellationToken),
        });

    static ResiliencePipelineBuilder ArmBreaker(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddCircuitBreaker(row.Breaker(composition.Allotted(row.Lane.Attempt), composition.Dark, LaneGuard.EvidenceOf(row.Lane)));

    static ResiliencePipelineBuilder ArmDeadline(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddTimeout(new TimeoutStrategyOptions {
            Name = Deadline.Key,
            Timeout = composition.Allotted(row.Lane.Attempt),
        });

    // Chaos composes `Runtime/determinism#ADVERSARIAL_PROBE` whole: that owner's `ChaosArming` settles the
    // per-execution conjunction against a seeded address and STAMPS its decision, each `ChaosBand` carries one
    // plane with its rate and weighted catalogue, and this arm only folds bands onto builder verbs — an
    // options body written here would re-mint the gate and reintroduce the ambient randomizer that voids
    // replay. Injected latency expires the lane's own deadline so the breaker counts it, and an injected fault
    // rides the fold's open tail because the transient row excludes it, so the two planes prove two things.
    static ResiliencePipelineBuilder ArmChaos(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        composition.Chaos.Match(
            Some: arming => composition.Bands(row.Lane).Fold(builder, (chain, band) => band.Kind.Switch(
                state: (Chain: chain, Arming: arming, Band: band, Behaviors: composition.Behaviors),
                latency: static seat => seat.Chain.AddChaosLatency(seat.Arming.Latency(seat.Band)),
                fault: static seat => seat.Chain.AddChaosFault(seat.Arming.Fault(seat.Band, LaneGuard.ChaosFaults)),
                // Result substitution is the ONE plane a non-generic pipeline cannot reach, so its arm
                // returns its chain untouched rather than pretending a per-result-type registration exists.
                outcome: static seat => seat.Chain,
                behavior: static seat => seat.Chain.AddChaosBehavior(seat.Arming.Behavior(seat.Band, seat.Behaviors)))),
            None: () => builder);
}

// Every terminal binds its evidence as a typed field, and the taxonomy runs child-before-parent so operator
// darkness never masquerades as an organic break. `CallerLeft` and `Deadline` are structurally distinct: the
// caller's intent and an attempt that was too slow read as different cases, never as message text.
[Union]
public abstract partial record LaneFault : Expected, IValidationError<LaneFault> {
    private LaneFault(string detail, int code) : base(detail, code, None) { }
    public static LaneFault Create(string message) => new Text(message);
    public sealed record Text : LaneFault { public Text(string detail) : base(detail, FaultBand.Lane.Code(0)) { } }

    public sealed record CallerLeft : LaneFault { public CallerLeft() : base("<caller-left>", FaultBand.Lane.Code(1)) { } }

    // Both limiter rows raise one Polly exception type, so the refusing row rides as a typed field rather than
    // a message a caller re-parses, and the limiter's own retry-after hint rides beside it.
    public sealed record Rejected : LaneFault {
        public Rejected(string strategy, Option<Duration> retryAfter)
            : base($"<lane-rejected:{strategy}>", FaultBand.Lane.Code(2)) => (Strategy, RetryAfter) = (strategy, retryAfter);
        public string Strategy { get; }
        public Option<Duration> RetryAfter { get; }
    }

    public sealed record Shed : LaneFault {
        public Shed(ShedVerdict verdict)
            : base($"<lane-shed:{verdict.Lane.Key}:{verdict.Level.Key}:{verdict.Breaker}>", FaultBand.Lane.Code(3)) => Verdict = verdict;
        public ShedVerdict Verdict { get; }
    }

    public sealed record Deadline : LaneFault {
        public Deadline(Duration span) : base($"<lane-deadline:{span}>", FaultBand.Lane.Code(4)) => Span = span;
        public Duration Span { get; }
    }

    public sealed record Dark : LaneFault {
        public Dark(string pipeline) : base($"<lane-dark:{pipeline}>", FaultBand.Lane.Code(5)) => Pipeline = pipeline;
        public string Pipeline { get; }
    }

    public sealed record Broken : LaneFault {
        public Broken(Option<Duration> retryAfter) : base($"<lane-broken:{retryAfter}>", FaultBand.Lane.Code(6)) => RetryAfter = retryAfter;
        public Option<Duration> RetryAfter { get; }
    }
}

// Two spine projections off the roster, seated together because neither is a datum the roster carries.
// Lane-to-deadline BINDING is executable rather than asserted: a latency-ranked lane is bounded by the
// attempt class and a throughput-ranked one by the fold class, and the discriminant is the lane's OWN rank,
// so no row spells its own class, no profile overrides it, and a transport row can never reach the column to
// price an in-process fold on a socket's budget. `PipelineKey` is the resilience-registry identity — ONE
// spelling the registration, the closure proof, the resolved seat, and the dispatch all read, so a format
// edit cannot desynchronize the registry from the proof that covers it.
public static class LaneClass {
    public const int LatencyRank = 1;

    extension(WorkLane lane) {
        public DeadlineClass Attempt => lane.Rank <= LatencyRank ? DeadlineClass.LaneAttempt : DeadlineClass.LaneFold;

        public string PipelineKey => $"lane:{lane.Key}";
    }
}

// Whole lane allotment: four DECLARED columns and every strategy knob derived from them, so a posture edit
// moves one column, an incoherent knob pair is unconstructible, and a numeric literal inside an arm is one
// rejected form reconstructing what this row already carries. `Floor` is the one concurrency floor both
// admission shapes and breaker health read; `Narrowed` bounds where the adaptive resize tunes down to, so a
// pressured lane narrows and never starves. Sampling is twice the attempt span and the break is half that
// window, so a breaker can never observe fewer attempts than the window it grades over.
public sealed record LanePolicy(WorkLane Lane, int Floor, double Trip, DegradationLevel ShedFloor) {
    public int Permits => Floor * 2;

    public int Queue => Permits * 2;

    public int Throughput => int.Max(Floor / 2, 2);

    public int Narrowed => int.Max(Floor / 4, 1);

    public CircuitBreakerStrategyOptions Breaker(TimeSpan attempt, CircuitBreakerManualControl dark, CircuitBreakerStateProvider evidence) =>
        new() {
            Name = LaneStrategy.Breaker.Key,
            FailureRatio = Trip,
            MinimumThroughput = Throughput,
            SamplingDuration = 2 * attempt,
            BreakDuration = attempt,
            ShouldHandle = LaneGuard.Transient,
            ManualControl = dark,
            StateProvider = evidence,
        };

    // Replenishment is OFF because the bucket lives inside the partitioned limiter, whose ONE heartbeat calls
    // `TryReplenish` on every partition it holds — the period still governs, a per-tenant timer does not exist
    // to leak, and the partition factory strips the flag anyway at the cost of a duplicated options object.
    public TokenBucketRateLimiterOptions Bucket(TimeSpan attempt) => new() {
        ReplenishmentPeriod = attempt,
        TokensPerPeriod = Floor,
        TokenLimit = Permits,
        QueueLimit = Queue,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        AutoReplenishment = false,
    };

    public ConcurrencyLimiterOptions Pool(int permits) => new() {
        PermitLimit = permits,
        QueueLimit = Queue,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    };
}

public static class LaneGuard {
    // One transient row, converted implicitly into the breaker's `ShouldHandle` slot: an expired attempt
    // deadline is the ONLY infrastructural failure an in-process lane produces, so every other escaping
    // exception is a property of one call's input and moves no health statistic.
    public static readonly PredicateBuilder<object> Transient = new PredicateBuilder<object>().Handle<TimeoutRejectedException>();

    public static readonly ResiliencePropertyKey<string> TenantKey = new("rasm.lane.tenant");

    // Catalogue rows resolve onto the lane's OWN fault family, so a realistic fault mix declares as weighted
    // `ChaosRow` rows at its band while the exception each becomes stays this page's vocabulary and rides the
    // fold's open tail exactly as any escaping delegate exception does. Weight lives on the row and the PICK
    // lives on the probe owner's seeded draw, because the package's own generator builds its selection from an
    // internal helper no options member can substitute — a catalogue built through it picks a different row
    // every run beneath a gate that reads perfectly deterministic.
    public static Exception ChaosFaults(string row) => LaneFault.Create($"<chaos-fault:{row}>").ToException();

    static readonly ConcurrentDictionary<WorkLane, CircuitBreakerStateProvider> Evidence = new();

    // State providers are SINGLE-ATTACH — a second pipeline reusing one throws at build — so evidence seats
    // per lane while the operator control seats once for the whole group.
    public static CircuitBreakerStateProvider EvidenceOf(WorkLane lane) =>
        Evidence.GetOrAdd(lane, static _ => new CircuitBreakerStateProvider());

    // Two altitudes, two records: `Composition` is what the strategy arms read while the collection is still
    // editable, `Runtime` is what a dispatch reads off the built provider. Neither is reachable from the other,
    // which is what keeps a registration-time value from leaking into a per-execution decision. `Dark` is ONE
    // control across every lane breaker — isolate and close act on the in-process capability group as one
    // verb, and `isIsolated: true` at construction boots that group dark so a degraded boot serves no
    // undegraded solve; a per-lane control makes darkness N verbs an operator can half-apply. `Chaos` is the
    // probe owner's arming SEAT rather than a flag, so a recording campaign and a driven replay swap one
    // column, and an absent seat leaves every pipeline chaos-free without a second build shape.
    public sealed record Composition(
        ILoggerFactory Telemetry,
        Func<DeadlineClass, TimeSpan> Allotted,
        Func<DegradationReading> Pressure,
        TenantLimiters Tenants,
        LanePermits Permits,
        CircuitBreakerManualControl Dark,
        Option<ChaosArming> Chaos,
        Func<WorkLane, Seq<ChaosBand>> Bands,
        Func<string, ValueTask> Behaviors);

    // Dispatch reads one frozen seat per lane: the row it sheds against, the pipeline it executes, and the
    // breaker evidence its verdict grades. `Proven` is the only mint, so the lookup below is total by
    // construction rather than by a probe every dispatch re-runs.
    public sealed record Runtime(FrozenDictionary<WorkLane, Runtime.Seat> Lanes, Func<DegradationReading> Pressure) {
        public readonly record struct Seat(LanePolicy Row, ResiliencePipeline Pipeline, CircuitBreakerStateProvider Evidence);
    }

    // Per-tenant admission is the BCL's OWN partitioned limiter, which already owns every property a
    // hand-rolled cache would restate: one bucket per `(lane, tenant)` pair minted on first reach and reused,
    // one heartbeat replenishing all of them, eviction of a partition idle past its ten-second limit, and
    // disposal of every partition still held when the owner drains — so live buckets cost one per ACTIVE
    // tenant rather than one per tenant the process has ever seen. Keying the pair rather than the tenant
    // alone is what stops a tenant's bulk work from spending the budget its own interactive work needs, and
    // admission keys the ROOT row too, since an unpartitioned process still owns one bucket per lane.
    public sealed class TenantLimiters {
        // Knobs ride the resource and identity rides the key, so the factory — reached only on a partition
        // miss — derives its options through `LanePolicy.Bucket` and no call site spells a limiter literal.
        // Pair equality already folds the roster row's generated ordinal key comparer beside ordinal string
        // equality, so the optional comparer argument restates what the tuple compares on.
        public readonly record struct Admission(LanePolicy Row, TimeSpan Attempt, string Tenant);

        readonly PartitionedRateLimiter<Admission> buckets =
            PartitionedRateLimiter.Create<Admission, (WorkLane Lane, string Tenant)>(static seat =>
                RateLimitPartition.GetTokenBucketLimiter(
                    (seat.Row.Lane, seat.Tenant), _ => seat.Row.Bucket(seat.Attempt)));

        public ValueTask<RateLimitLease> Lease(LanePolicy row, TimeSpan attempt, string tenant, CancellationToken token) =>
            buckets.AcquireAsync(new Admission(row, attempt, tenant), permitCount: 1, token);

        // Composition owns the lifetime because nothing else can: a limiter handed to the strategy as a lease
        // producer is never disposed by the package. Release rides the drain participant the retired permit
        // cells already ride, so admission teardown is drain evidence and never a finalizer-thread release.
        public IO<Unit> Drain() =>
            IO.liftAsync(async () => { await buckets.DisposeAsync(); return unit; });
    }

    // Lane dimensions ride every resilience measurement: lane key, live degradation level, and this frame's
    // own tenancy projection — so a per-lane resilience series is real and joins the rest of the estate's
    // telemetry on one grammar. Level reads its reading DIRECTLY because a verdict minted here answers
    // `reading.Level` verbatim, and a shed execution never reaches an enricher at all. Tenancy SPREADS the
    // kernel pair's `Tags` rather than spelling key and value here: that projection is empty for the root
    // row, so an unpartitioned process tags nothing instead of stamping zero-hex text as a tenant.
    public sealed class LaneEnricher(LanePolicy row, Func<DegradationReading> pressure) : MeteringEnricher {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context) {
            context.Tags.Add(new(LaneTags.Lane, row.Lane.Key));
            context.Tags.Add(new(LaneTags.Level, pressure().Level.Key));
            foreach (KeyValuePair<string, object?> tenancy in TenantContext.Current.Tags) {
                context.Tags.Add(tenancy);
            }
        }
    }

    public static class LaneTags {
        public const string Lane = "rasm.lane.key";
        public const string Level = "rasm.lane.level";
    }

    // Polly grades every event and the estate RE-RANKS only the events Polly already calls a problem, lifting
    // each to the emitting row's own rank and passing benign events through untouched. A flat per-row constant
    // reports a circuit CLOSING at the breaker row's `Error` rank, which pages an operator on a recovery.
    static ResilienceEventSeverity Ranked(SeverityProviderArguments args) =>
        args.Event.Severity >= ResilienceEventSeverity.Warning
        && LaneStrategy.TryGet(args.Source.StrategyName ?? string.Empty, out LaneStrategy? row)
            ? row!.Severity
            : args.Event.Severity;

    // Registration is boot-strict: the roster proves closed BEFORE a single pipeline registers, so a lane row
    // added at the roster fails composition with the lane named rather than throwing at its first dispatch,
    // on the same discriminant the folder ruling already settled for unregistered latency names.
    public static Fin<IServiceCollection> Register(IServiceCollection services, Composition composition, params ReadOnlySpan<LanePolicy> rows) =>
        Closed(Iterable<LanePolicy>.FromSpan(rows).ToSeq())
            .Map(seated => seated.Fold(services, (graph, row) =>
                graph.AddResiliencePipeline(row.Lane.PipelineKey, builder =>
                    ignore(toSeq(LaneStrategy.Items).Fold(
                        // Polly's ILoggerFactory overload sets a logger and NOTHING else, so a promised
                        // per-lane series would not exist as a metric dimension behind it. Its options
                        // overload is where estate grammar reaches the meter — MeteringEnrichers append lane
                        // key, degradation level, and tenant slot per the branch dimension-key ruling.
                        builder.ConfigureTelemetry(new TelemetryOptions {
                            LoggerFactory = composition.Telemetry,
                            MeteringEnrichers = { new LaneEnricher(row, composition.Pressure) },
                            SeverityProvider = static args => Ranked(args),
                        }),
                        (chain, strategy) => strategy.Arm(chain, composition, row))))));

    // Roster half of the closure proof, accumulating so every open lane reports at one boot rather than one
    // per attempt. The reverse direction needs no fold: `LanePolicy.Lane` is typed `WorkLane`, so a policy
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

    // Provider half: a pipeline exists only after `BuildServiceProvider`, so the composition root's
    // built-provider gate runs this fold, and the fold MINTS the runtime it proved rather than handing a
    // caller a receipt it then re-derives a record from. `TryGetPipeline` is the non-throwing probe where
    // `GetPipeline` raises, so an unmaterialized lane refuses at the same boot band the roster fold does.
    // Resolution is this fold's WHOLE claim — a lane whose `LaneStrategy` arms dropped a strategy still
    // resolves, so its composed CHAIN proves in the suite off the built pipeline rather than at boot, keeping
    // that inspection dependency on the test plane where it belongs.
    public static Fin<Runtime> Proven(
        ResiliencePipelineProvider<string> pipelines, Func<DegradationReading> pressure, params ReadOnlySpan<LanePolicy> rows) =>
        Closed(Iterable<LanePolicy>.FromSpan(rows).ToSeq())
            .Bind(seated => seated
                .Traverse(row => pipelines.TryGetPipeline(row.Lane.PipelineKey, out ResiliencePipeline? pipeline)
                    ? Validation<Error, Runtime.Seat>.Success(new(row, pipeline!, EvidenceOf(row.Lane)))
                    : new Fault.InvalidValue(Label: row.Lane.PipelineKey, Requirement: "<a built lane pipeline>"))
                .As()
                .Map(seats => new Runtime(seats.ToFrozenDictionary(static seat => seat.Row.Lane), pressure))
                .ToFin());

    // Dispatch refuses BEFORE the pipeline, then folds outcome-first: a refused lane leases no permit, no
    // pooled context, and no telemetry frame, which is exactly why the refusal is not a strategy row. The
    // seat lookup is total because `Proven` covered `WorkLane.Items` and is the only mint of a `Runtime`.
    public static IO<T> Run<T>(Runtime runtime, WorkLane lane, Func<CancellationToken, ValueTask<T>> work) =>
        runtime.Lanes[lane] is var seat
        && ShedVerdict.Of(runtime.Pressure(), seat.Row, seat.Evidence.CircuitState) is { Shed: true } verdict
            ? IO.fail<T>(new LaneFault.Shed(verdict))
            : IO.liftAsync(env => Executed(seat.Pipeline, lane, work, env.Token)).Bind(Lifted);

    // Named boundary capsule: the pooled-context lease `try`/`finally` and the outcome-capture kernel are the
    // platform-forced statement seam. `OperationKey` fixes at lease so every strategy event and the execution
    // correlate on the lane key without a join, tenant render fixes beside it as one typed side channel each
    // limiter arm reads, and a caller's cancellation verdict rides out beside the outcome so this fold
    // separates a caller who left from an attempt that ran long.
    static async ValueTask<(Outcome<T> Outcome, bool Left)> Executed<T>(
        ResiliencePipeline pipeline, WorkLane lane, Func<CancellationToken, ValueTask<T>> work, CancellationToken token) {
        ResilienceContext context = ResilienceContextPool.Shared.Get(lane.PipelineKey, token);
        context.Properties.Set(TenantKey, TenantContext.Current.Entry);
        try {
            Outcome<T> outcome = await pipeline.ExecuteOutcomeAsync(
                static async (ctx, state) => {
                    try { return Outcome.FromResult(await state(ctx.CancellationToken).ConfigureAwait(false)); }
                    catch (Exception ex) when (ex is not OutOfMemoryException) { return Outcome.FromException<T>(ex); }
                },
                context, work).ConfigureAwait(false);
            return (outcome, token.IsCancellationRequested);
        }
        finally { ResilienceContextPool.Shared.Return(context); }
    }

    // Closed taxonomy ordered child-before-parent: `IsolatedCircuitException` derives from its
    // `BrokenCircuitException` base, so operator-forced darkness folds to `Dark` and never reads as a
    // dependency break an on-call rotation would chase. Every arm binds its own evidence as a typed field, so
    // escalation matches the case and reads the field rather than re-parsing the detail text.
    static IO<T> Lifted<T>((Outcome<T> Outcome, bool Left) captured) => (captured.Outcome, captured.Left) switch {
        ({ Exception: null, Result: { } value }, _) => IO.pure(value),
        ({ Exception: OperationCanceledException }, true) => IO.fail<T>(new LaneFault.CallerLeft()),
        ({ Exception: TimeoutRejectedException slow }, _) => IO.fail<T>(new LaneFault.Deadline(Duration.FromTimeSpan(slow.Timeout))),
        ({ Exception: IsolatedCircuitException dark }, _) => IO.fail<T>(new LaneFault.Dark(dark.TelemetrySource?.PipelineName ?? "<unnamed-pipeline>")),
        ({ Exception: BrokenCircuitException open }, _) => IO.fail<T>(new LaneFault.Broken(Optional(open.RetryAfter).Map(Duration.FromTimeSpan))),
        ({ Exception: RateLimiterRejectedException rejected }, _) => IO.fail<T>(new LaneFault.Rejected(
            Emitting(rejected.TelemetrySource), Optional(rejected.RetryAfter).Map(Duration.FromTimeSpan))),
        ({ Exception: { } foreign }, _) => IO.fail<T>(new LaneFault.Text(foreign.Message)),
        _ => IO.fail<T>(new LaneFault.Text("<empty-outcome>")),
    };

    // Refusing strategy resolves back to its own roster row, so a lane-pool refusal and a tenant-bucket
    // refusal stay distinguishable at the rail even though Polly raises one exception type for both.
    static string Emitting(ResilienceTelemetrySource? source) =>
        LaneStrategy.TryGet(source?.StrategyName ?? string.Empty, out LaneStrategy? strategy) ? strategy!.Key : "<unnamed-strategy>";
}
```

## [03]-[ADAPTIVE_ARMS]

- Owner: `AdaptiveConcurrency` the static ResourceMonitoring-fed permit-resize projection; `LanePermits` the live per-lane limiter cell the lane's admission row leases from and the resize is the sole writer of.
- Entry: `Resize(LanePolicy row, Utilization utilization)` returns `int` — tapers the lane's permit count between its full pool and its narrowed floor from the live CPU and memory utilization, so a pressured host narrows the lane and an idle host widens it; `Of(LanePolicy row)` returns `RateLimiter` — the lease producer the `LaneStrategy.Bulkhead` arm binds, re-seating the lane's `ConcurrencyLimiter` when a resize moves the count and returning the seated one otherwise; `Drain()` returns `IO<Unit>` — the `DrainParticipantPort` body releasing every retired limiter.
- Auto: the resize reads the `Observability/health#HEALTH_FOLD` `UtilizationCell` CPU and memory ratios graded against the `ResourceQuota` container limit — the composition root supplies `UtilizationCell.Read` as the cell's utilization function — so the permit resize rides the same observable-instrument-and-quota path the host pressure grade reads, never a parallel meter, and a lane under cgroup throttling narrows on the limit it runs under; the resize decision is cadence-gated on a `ClockPolicy` mark rather than a timer, so the lease producer runs per execution while the utilization read and the limiter mint run once per interval; a moved count seats a fresh `ConcurrencyLimiter` and PARKS the retired instance instead of disposing it, because a `RateLimitLease` releases against the instance that issued it and disposing under an outstanding lease strands the permit; the resize is bounded between the row's derived floor and its full pool so adaptive concurrency tunes within a band, never to zero.
- Receipt: the parked limiters release inside the `DrainBand.Telemetry` participant row so a retirement is drain evidence, never a finalizer-thread release; the resize itself mints no receipt — the live permit count reaches the meter as the lane's own resilience series.
- Packages: Microsoft.Extensions.Diagnostics.ResourceMonitoring, Polly.RateLimiting, LanguageExt.Core, BCL inbox
- Growth: a new utilization signal is one enabled instrument the `UtilizationCell` reads; a new grading input is one `Utilization` column the taper folds; zero new surface.
- Boundary: the adaptive arms read the existing health owners — the `UtilizationCell` for utilization and the `DegradationReading` for the shed level — never a second resource meter or a second pressure cell; the resize projection is pure and the cell is its ONLY writer, so a permit count computed and applied nowhere is the deleted form this seam exists to foreclose; `Utilization` is the value the cell's supplied function yields and the projection never reaches the cell type itself, so the health page owns the read and this page owns the response; utilization grades on the BINDING constraint — the higher of the CPU and memory ratios — so a lane pressured by allocation narrows exactly as one pressured by compute, and a CPU-only grade leaves the memory ratio a column the page reads nowhere; the taper is continuous between the two named ratios because a two-step ladder gives every intermediate pressure one of two widths and re-decides the space an interpolation already generates; the retired-limiter park happens only on the swap that WON, since a compare-exchange loser must never hand the drain an instance still seated and still leasing.

```csharp signature
public static class AdaptiveConcurrency {
    // Named policy ratios per the const discipline — an inline CPU threshold is the deleted literal.
    public const double Pressured = 0.75d;
    public const double Saturated = 0.90d;

    // Permits taper linearly from the row's full pool at the pressured ratio to its narrowed floor at the
    // saturated one, so every intermediate load has its own width and the band's ends stay the row's own.
    public static int Resize(LanePolicy row, Utilization utilization) =>
        double.Max(utilization.CpuRatio, utilization.MemoryRatio) switch {
            <= Pressured => row.Permits,
            >= Saturated => row.Narrowed,
            var load => row.Narrowed + (int)double.Round((row.Permits - row.Narrowed) * ((Saturated - load) / (Saturated - Pressured))),
        };
}

// Runtime-resizable permit source. The lease producer runs PER EXECUTION, so the live limiter is a value this
// cell publishes and `Resize` is its only writer — which is what makes the page's runtime-resize claim a seam
// rather than a sentence. Cadence gating keeps the cost honest: an unelapsed lane returns its seated limiter
// with no utilization read and no mint.
public sealed class LanePermits(ClockPolicy clocks, Func<Utilization> utilization) {
    public static readonly Duration ResizeInterval = Duration.FromSeconds(5);

    readonly ConcurrentDictionary<WorkLane, Seat> seats = new();
    readonly Atom<Seq<ConcurrencyLimiter>> parked = Atom(Seq<ConcurrencyLimiter>());

    readonly record struct Seat(int Permits, long Mark, ConcurrencyLimiter Limiter);

    public RateLimiter Of(LanePolicy row) =>
        seats.GetOrAdd(row.Lane, _ => Seated(row, row.Permits)) is var held && clocks.Elapsed(held.Mark) >= ResizeInterval
            ? Resized(row, held)
            : held.Limiter;

    // Retirement parks only on the compare-exchange that WON: an update delegate re-run under contention
    // would otherwise hand the drain a limiter still seated and still issuing leases, and a losing mint has
    // never leased so disposing it strands nothing.
    RateLimiter Resized(LanePolicy row, Seat held) =>
        AdaptiveConcurrency.Resize(row, utilization()) is var sized && sized == held.Permits
            ? (ignore(seats.TryUpdate(row.Lane, held with { Mark = clocks.Mark() }, held)), held.Limiter).Item2
            : Seated(row, sized) is var next && seats.TryUpdate(row.Lane, next, held)
                ? (parked.Swap(rows => rows.Add(held.Limiter)), next.Limiter).Item2
                : (next.Limiter.Dispose(), seats[row.Lane].Limiter).Item2;

    Seat Seated(LanePolicy row, int permits) => new(permits, clocks.Mark(), new ConcurrencyLimiter(row.Pool(permits)));

    // Retired limiters release at the drain band and never at the swap: a lease returns to the instance that
    // issued it, so disposing under an outstanding lease is the stranded permit this parking avoids.
    public IO<Unit> Drain() =>
        IO.lift(() => parked.Swap(static _ => Seq<ConcurrencyLimiter>()))
            .Bind(static rows => rows.TraverseM(static limiter =>
                IO.liftAsync(async () => { await limiter.DisposeAsync(); return unit; })).As())
            .Map(static _ => unit);
}
```

## [04]-[SHED_VERDICT]

- Owner: `ShedVerdict` the per-`WorkLane` admission verdict minted once here and crossed to Compute admission.
- Entry: `Of(DegradationReading reading, LanePolicy row, CircuitState breaker)` returns `ShedVerdict` — mints the per-lane admission verdict from the atomic `DegradationReading` against the lane's own configured `ShedFloor` and the lane breaker's own evidence, carrying the lane, the derived level, the refusal flag, and the breaker state, so admission is computed once at the in-process governor and consumed downstream rather than re-derived from raw saturation.
- Auto: the verdict reads the one atomic `DegradationReading` so the refusal flag and the level it derives from are coherent, never a stale-snapshot-against-fresh-level race; host pressure is global and the per-lane axis is the FLOOR the lane's own `LanePolicy` declares, so the verdict compares one reading against one lane's threshold and the lane it names is the row that threshold belongs to; the breaker column folds the lane's own health into the same verdict, so a dark or broken lane refuses admission at the one seat pressure already refuses at; the verdict is minted once at the in-process governor edge and crosses to `Rasm.Compute/Runtime/admission` as the one verdict the Compute `SubstrateSelection` fold consumes on its admission decision — one mint, two consumers, the in-process `LaneGuard.Run` refusal and the Compute admission veto.
- Receipt: the verdict is the cross-package fact the Compute admission consumes and the typed evidence `LaneFault.Shed` carries; no parallel verdict receipt.
- Packages: Polly.Core, LanguageExt.Core, BCL inbox
- Growth: a new admission input is one column on the verdict; zero new surface.
- Boundary: the verdict is the one per-`WorkLane` admission fact — the verdict `LaneGuard` mints from the atomic `DegradationReading` is the one verdict `Rasm.Compute/Runtime/admission` consumes on its `SubstrateSelection` fold rather than a Compute-side re-derivation (the `ONE_DEGRADATION_SHED_VERDICT` ripple), so the in-process refusal and the Compute veto read one verdict and a Compute-side re-derivation from raw saturation is the rejected form; the breaker column refuses on `Open` and `Isolated` and ADMITS on `HalfOpen`, because a half-open breaker is waiting for exactly one probe and a verdict refusing that probe leaves the lane dark until something else dispatches — a recovery deadlock the two-state read forecloses; `CircuitBreakerStateProvider` answers `Closed` while unattached, so a verdict minted before the provider binds admits rather than inventing darkness; the verdict reads the atomic `DegradationReading` so it is race-free, the reason the health-cell collapse exists; the consumer count stays TWO under a kernel fold, because `Run` refuses BEFORE it invokes the work delegate — a refused lane never enters the fold, so the kernel governance band inherits the decision by absence and a third read seated below the lane grades pressure the caller already refused on.

```csharp signature
// One per-WorkLane admission verdict, minted from the atomic DegradationReading against the lane's own
// configured ShedFloor and its own breaker evidence — consumed by both the in-process LaneGuard.Run refusal
// and Rasm.Compute/Runtime/admission's SubstrateSelection fold, never re-derived Compute-side. Breaker state
// rides the verdict as a column rather than a second query, so a dark lane and a pressured host refuse at one
// seat. The seam couples to this verdict shape, not the DegradationCell interior.
public readonly record struct ShedVerdict(WorkLane Lane, DegradationLevel Level, bool Shed, CircuitState Breaker) {
    public static ShedVerdict Of(DegradationReading reading, LanePolicy row, CircuitState breaker) =>
        new(row.Lane,
            reading.Level,
            reading.Level.Rank >= row.ShedFloor.Rank || breaker is CircuitState.Open or CircuitState.Isolated,
            breaker);
}
```

## [05]-[RESEARCH]

(none)
