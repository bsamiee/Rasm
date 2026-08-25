# [APPHOST_WORK_LANE_GOVERNOR]

`LaneGuard` is the work-lane vocabulary owner and in-process resilience governor: the `WorkLane` roster DECLARES here and one keyed Polly `ResiliencePipeline` composes per roster row, bracketing each lane with per-tenant admission, an adaptive bulkhead, circuit health, one allotment deadline, and a runtime-armed `Polly.Simmy` chaos block. Admission mints once as one per-`WorkLane` `Admission` case over the atomic `DegradationReading` and the lane's own breaker evidence, then crosses to `Rasm.Compute/Runtime/admission` (the `ONE_DEGRADATION_SHED_VERDICT` ripple) rather than a Compute-side re-derivation.

Lanes run SINGLE-PASS at the pipeline: the bracket admits once, and the in-process re-drive is the `RedrivePolicy` the lane's own row carries, composed INSIDE the work delegate where the kernel `Redrive` owner runs it. `LaneGuard` owns the lane roster, its deadline and pipeline-key projections, the registry and its closure proof, the adaptive and load-shed arms, the chaos fold, and the admission verdict; it consumes `Observability/health#DEGRADATION_RAIL` `DegradationReading`/`DegradationLevel`, `UtilizationCell`/`Utilization`, `Runtime/time#DEADLINE_TAXONOMY` `DeadlineClass`/`ClockPolicy`, `Runtime/determinism#ADVERSARIAL_PROBE` `ChaosArming`/`ChaosBand`, kernel `RedrivePolicy`/`Redrive`/`Retriability`, `Transition`/`Cell`, `MonotonicTimeline`, and `TenantContext` as settled vocabulary, minting no eighth port.

## [01]-[INDEX]

- [02]-[LANE_GUARD]: `WorkLane` roster, its deadline and `PipelineKey` projections, the per-lane allotment row, and the lane fault family.
- [03]-[LANE_REGISTRY]: One keyed Polly `ResiliencePipeline` per roster row, its two-altitude composition, the closure proof, and the dispatch fold.
- [04]-[ADAPTIVE_ARMS]: ResourceMonitoring-fed resizable permit source under one cadence gate, its retirement transition, and its idle reclaim.
- [05]-[ADMISSION]: Per-`WorkLane` admission case minted once and crossed to Compute admission.

## [02]-[LANE_GUARD]

- Owner: `WorkLane` `[SmartEnum<string>]` the six-row lane roster — identity and `Rank` — under the `ComparerAccessors.StringOrdinal` accessor; `PipelineKey` `[ValueObject<string>]` the resilience-registry identity whose namespace head is unspellable outside it; `LaneClass` the rank-to-deadline projection off the roster; `LanePolicy` the per-lane allotment row every strategy knob and the lane's re-drive law derive from; `Departure` `[Union]` the two-case cancellation verdict; `LaneFault` `[Union]` the direct `FaultBand.LaneGuard` family: CallerLeft | Rejected | Shed | Deadline | Dark | Broken.
- Cases: `Departure` = `Caller` (the caller's own token was pulled) | `Attempt` (the attempt itself ended) — a tuple-position bool answering the same question makes the two indistinguishable at the fold that must separate them.
- Entry: `LaneClass.Attempt` on `WorkLane` binds the lane's rank to its deadline class; `PipelineKey.Of(WorkLane)` is the ONE mint of a registry key and `PipelineKey.Named(string?)` the admission arm resolving a Polly-reported pipeline name back onto the vocabulary; `LanePolicy` derives `Permits`/`Queue`/`Throughput`/`Narrowed` and projects the breaker, bucket, and pool option bodies its strategies bind.
- Auto: identity and `Rank` are the whole roster because rank is the cross-lane precedence datum every consumer reads, and a column only the solve path's own domain decides stays at that consumer — `Rasm.Compute/Runtime/scheduling` keys `LaneBound` and its reader budgets on `WorkLane.Items`, so `Interactive` and `Ranked` share rank 1 and separate at the CHANNEL column that stratum owns; four DECLARED `LanePolicy` columns carry the whole posture and every strategy knob derives from them, so an incoherent knob pair is unconstructible and a numeric literal inside an arm reconstructs what the row already holds; the re-drive law rides the row beside them, so a lane that re-drives declares its curve once and no dispatch seat threads a policy argument.
- Auto: the fault family IS the dispatch evidence — each arm binds its own typed field, so escalation matches the case and reads the field rather than re-parsing detail text; a refused admission carries the `Admission.Shed` case whole (`[05]`).
- Packages: Polly.Core, Polly.RateLimiting, System.Threading.RateLimiting, Rasm (kernel `FaultBand`/`RedrivePolicy`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new lane is one `WorkLane` roster row and the one `LanePolicy` the closure proof then demands, and NOTHING about its deadline — `Attempt` derives through `LaneClass` from the lane's own rank, `DeadlineClass.LaneAttempt` below the latency rank and `DeadlineClass.LaneFold` above it — so a row cannot spell a class its lane contradicts and a transport row can never reach the column to price an in-process fold on a socket's budget; a new posture edit is one `LanePolicy` column, every knob below it deriving; a new re-drive curve is one `Schedule` composition at the row's own policy mint; zero new surface.
- Boundary: the lane roster DECLARES at the spine and crosses downward — `Rasm.Compute/Runtime/scheduling` keys its `LaneBound` channel table and its `readers(CpuBudget)` reader budgets on `WorkLane.Items` through its legal S3 reference, so a row added here fails Compute's keyed fold loudly, and an AppHost project reference to `Rasm.Compute` is the deleted form that closed an S1-to-S3 cycle. `LaneClass` is the ONE seat where a lane meets a deadline class — every consumer that dispatches onto a `WorkLane` reads the binding rather than naming a class beside the lane, so the `Agent/capability#COMMAND_ALGEBRA` `Spec` fold picks a lane and takes the class that lane's rank already fixes, and a literal `DeadlineClass` beside a literal `WorkLane` at any dispatch seat is the deleted form that let a whole-model fold ride an interactive lane under a transport hop's budget. `PipelineKey` is a VALUE OBJECT rather than an interpolation, so the registration, the closure proof, the resolved seat, the pooled context, and the Polly-reported refusal all read one spelling and the `lane:` head has exactly one author — the generalized keyed-registry key law every namespaced registry on the spine reads, and a `$"lane:{…}"` at any of those sites is unspellable rather than merely discouraged.
- Boundary: the lane BRACKETS, it does not loop — the pipeline admits one logical call and the in-process re-drive is the kernel `RedrivePolicy` the row carries, run by `Redrive.Run` INSIDE the work delegate, so there is exactly one retry owner and it is the domain rail's. Three named consequences hold the arrangement: the lane's one lease spans the whole re-drive stream, so the bound is priced against the lane's own deadline and a lane whose row declares `RedrivePolicy.None` holds a permit for one attempt; a re-driven in-process solve re-spends CPU the bulkhead is rationing against the same deterministic input, so a solve lane declares no curve; and a lane-side loop ABOVE the delegate multiplies attempts by m invisibly and remains the deleted form. Hedging is unreachable by construction beside all three — `AddHedging` binds `ResiliencePipelineBuilder<TResult>` alone and no non-generic `HedgingStrategyOptions` exists — the same result-typed foreclosure that puts `AddChaosOutcome` out of reach, so outcome substitution and concurrent duplication are both unavailable here rather than omitted, and a generic per-result-type lane registration to buy either is the rejected form. Allotment inheritance runs one way: the lane is the OUTERMOST in-process budget and inherits nothing, while a hop dialled from inside a lane takes the minimum of its own class and the lane's remainder at the hop's own owner.
- Boundary: `Interactive` and `Ranked` hold ONE rank by declaration and the folder does not treat that as a defect — rank orders lanes against each other and these two are equally urgent, while the datum that separates them is `Rasm.Compute/Runtime/scheduling`'s own `LaneBound` column (`Parked(16)` against `Ranked(256)` with an earliest-deadline comparer), which is exactly the column the boundary above leaves at its deciding stratum; a rank column split to make the two differ here would encode a Compute channel shape in a spine roster.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using Thinktecture;

namespace Rasm.AppHost.Runtime;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public sealed partial class WorkLane {
    public static readonly WorkLane Interactive = new("interactive", rank: 1);
    public static readonly WorkLane Ranked = new("ranked", rank: 1);
    public static readonly WorkLane Background = new("background", rank: 2);
    public static readonly WorkLane Bulk = new("bulk", rank: 3);
    public static readonly WorkLane Benchmark = new("benchmark", rank: 4);
    public static readonly WorkLane CaptureIngest = new("capture-ingest", rank: 5);

    public int Rank { get; }
}

[ValueObject<string>(KeyMemberName = "Value")]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PipelineKey {
    private const string Head = "lane:";

    public static PipelineKey Of(WorkLane lane) => Create(Head + lane.Key);

    public static Option<PipelineKey> Named(string? reported) =>
        Validate(reported, out PipelineKey? key) is null ? Optional(key) : None;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (!(value ?? string.Empty).StartsWith(Head, StringComparison.Ordinal)) {
            validationError = new ValidationError($"a '{Head}' registry key");
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Departure {
    private Departure() { }
    public sealed record CallerCase : Departure;
    public sealed record AttemptCase : Departure;
    public static Departure Caller { get; } = new CallerCase();
    public static Departure Attempt { get; } = new AttemptCase();
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LanePolicy(WorkLane Lane, int Floor, double Trip, DegradationLevel ShedFloor, RedrivePolicy Redrive) {
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
            BreakDurationGenerator = args => new ValueTask<TimeSpan>(attempt * (1 + args.HalfOpenAttempts)),
            ShouldHandle = LaneGuard.Transient,
            ManualControl = dark,
            StateProvider = evidence,
        };

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



[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LaneFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.LaneGuard;
    private LaneFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;


    [FaultCase(0)]
    public sealed partial record CallerLeft : LaneFault, ICausedFault {
        public CallerLeft(Error cause) : base("<caller-left>") => Cause = cause;
        public Error Cause { get; }
    }

    [FaultCase(1)]
    public sealed partial record Rejected : LaneFault, ICausedFault {
        public Rejected(Option<LaneStrategy> strategy, Option<Duration> retryAfter, Error cause)
            : base(strategy.Match(Some: row => $"<lane-rejected:{row.Key}>", None: () => "<lane-rejected>"))
            => (Strategy, RetryAfter, Cause) = (strategy, retryAfter, cause);
        public Option<LaneStrategy> Strategy { get; }
        public Option<Duration> RetryAfter { get; }
        public Error Cause { get; }
        public override Retriability Retriability => RetryAfter.Match(Some: Retriability.Throttled, None: () => Retriability.Transient);
    }

    [FaultCase(2)]
    public sealed partial record Shed : LaneFault {
        public Shed(Admission.ShedCase refused)
            : base($"<lane-shed:{refused.Reading.Lane.Key}:{refused.Reading.Level.Key}:{refused.Cause.Key}>")
            => Refused = refused;
        public Admission.ShedCase Refused { get; }
        public override Retriability Retriability => Retriability.Transient;
    }

    [FaultCase(3)]
    public sealed partial record Deadline : LaneFault, ICausedFault {
        public Deadline(Duration span, Error cause) : base($"<lane-deadline:{span}>") => (Span, Cause) = (span, cause);
        public Duration Span { get; }
        public Error Cause { get; }
    }

    [FaultCase(4)]
    public sealed partial record Dark : LaneFault, ICausedFault {
        public Dark(Option<PipelineKey> pipeline, Error cause)
            : base(pipeline.Match(Some: key => $"<lane-dark:{key.Value}>", None: () => "<lane-dark>"))
            => (Pipeline, Cause) = (pipeline, cause);
        public Option<PipelineKey> Pipeline { get; }
        public Error Cause { get; }
    }

    [FaultCase(5)]
    public sealed partial record Broken : LaneFault, ICausedFault {
        public Broken(Option<Duration> retryAfter, Error cause) : base("<lane-broken>") => (RetryAfter, Cause) = (retryAfter, cause);
        public Option<Duration> RetryAfter { get; }
        public Error Cause { get; }
        public override Retriability Retriability => RetryAfter.Match(Some: Retriability.Throttled, None: () => Retriability.Transient);
    }

}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LaneClass {
    public const int LatencyRank = 1;

    extension(WorkLane lane) {
        public DeadlineClass Attempt => lane.Rank <= LatencyRank ? DeadlineClass.LaneAttempt : DeadlineClass.LaneFold;
    }
}
```

## [03]-[LANE_REGISTRY]

- Owner: `LaneStrategy` `[SmartEnum<string>]` the pipeline-row vocabulary whose declaration order IS the strategy order and whose delegate column IS each strategy's arm; `LaneEvidence` the per-composition breaker-state cell set; `LaneGuard` the static keyed-pipeline registry over the in-process lanes with its `Composition` and `Runtime` altitudes.
- Entry: `Register(IServiceCollection services, LaneGuard.Composition composition, params ReadOnlySpan<LanePolicy> rows)` returns `Fin<IServiceCollection>` — proves the roster closed against the supplied rows, then folds one `AddResiliencePipeline` entry per row keyed by `PipelineKey.Of(row.Lane)`, each pipeline composed by folding `LaneStrategy.Items` in declaration order; `Proven(ResiliencePipelineProvider<string> pipelines, LaneGuard.Composition composition, params ReadOnlySpan<LanePolicy> rows)` returns `Fin<LaneGuard.Runtime>` — the built-provider half of the closure proof, MINTING the dispatch runtime it proved so a `Runtime` is unconstructible except through the proof covering it; `Run(LaneGuard.Runtime runtime, WorkLane lane, Func<CancellationToken, IO<T>> work)` returns `IO<T>` — executes the in-process work outcome-first through the lane's resolved pipeline, re-drives the delegate under the lane row's own `RedrivePolicy`, and folds every termination onto one `LaneFault` arm or back onto the caller's own typed rail.
- Auto: each lane's pipeline is one keyed `ResiliencePipeline` registered through `AddResiliencePipeline(PipelineKey.Of(lane).Value, …)` exactly as `KeyedLane.Register` registers per hop, so the lane and the hop share one resilience pattern; the tenant row partitions admission BY TENANT AND BY LANE — a `RateLimiterStrategyOptions` lease producer over the BCL's own `PartitionedRateLimiter` whose partition key IS the `(lane, tenant)` pair and whose bucket knobs derive from the lane's own row, the tenant half being the branch-settled `TenantContext.Entry` render threaded through `ResilienceContext.Properties`; the bulkhead rides a second lease producer over the `ADAPTIVE_ARMS` `LanePermits` cell so the permit count is a live value the resize writes rather than a column frozen at build; the circuit breaker binds the composition's ONE group `CircuitBreakerManualControl` beside the composition's own per-lane `CircuitBreakerStateProvider`, so operator darkness acts on the whole in-process capability group as one verb while health evidence stays a per-seat read; the load-shed arm reads the atomic `DegradationReading` beside that evidence and refuses at the lane's degradation floor ahead of the pipeline entirely; the deadline arm binds the lane's own class allotment as a constant because no per-execution datum reaches this seat, while the breaker's dwell DOES take a generator over the one datum that is per-execution — the count of consecutive failed half-open probes — so a lane whose probe keeps failing backs off in multiples of its attempt span; the chaos row folds the `Runtime/determinism#ADVERSARIAL_PROBE` bands its lane declares straight onto the Simmy builder verbs, so injection gates per execution at that owner's seeded `EnabledGenerator` and this page writes no options body.
- Auto: a lane execution's resilience events land under the lane key in the package meter and logger exactly as the keyed-pipeline events do — the lane key, the live degradation level, and the tenant slot are `MeteringEnricher` tags on the measurement itself, never the DI registration key a query cannot group by, and the severity map re-ranks only the events Polly already grades a problem; a refused admission carries the case (`[05]`).
- Packages: Polly.Core, Polly.Extensions, Polly.RateLimiting, System.Threading.RateLimiting, Rasm (kernel `Redrive`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new resilience dimension is one `LaneStrategy` row carrying its arm and its severity, seated where its declaration order places it; a new measurement dimension is one `LaneEnricher` tag beside its `LaneTags` const; zero new surface.
- Boundary: declaration order is the policy and its derivation is stated at the roster, so a reorder that reads as a preference is a policy change with a named failure mode — the per-tenant bucket sits outside the lane pool because a tenant already over its own budget must refuse before it acquires and releases lane permits, and the inverse lets one tenant set the queue depth every other tenant waits behind; both limiters sit outside the breaker because admission counts logical calls while health statistics count attempts; the one deadline sits inside the breaker because the breaker's whole transient class IS the deadline it observes, and a deadline outside it leaves the breaker blind to the only infrastructural failure an in-process lane produces; chaos sits below everything it tests. Breaker health counts `TimeoutRejectedException` alone: a domain outcome rides the caller's own rail and a caller-fault exception escaping the delegate fails that ONE call through the fold's typed tail without moving the lane's health statistics — counting it darkens a whole lane for every tenant on one malformed input. Latency chaos therefore tests the breaker, fault chaos tests the fold's tail, and behavior chaos tests what the delegate tolerates before it runs — one plane each.
- Boundary: the two altitudes are ONE record and one projection off it — `Composition` is what the strategy arms read while the collection is still editable and what `Proven` reads to mint the built-lane seats, `Runtime` carries the four columns a dispatch reads and nothing else, so no registration-time value reaches a per-execution decision and the two halves cannot disagree about which breaker cell a lane's evidence lives in. NAMED LOSS: the earlier pair was mutually unreachable and the evidence cells lived in a process-static dictionary keyed by roster row, which could not reset between compositions and handed a second composition the first one's breaker state; the projection replaces that unreachability with a one-directional derivation and the static dies with it. `Dark` is ONE control across every lane breaker — isolate and close act on the in-process capability group as one verb, and `isIsolated: true` at construction boots that group dark so a degraded boot serves no undegraded solve; a per-lane control makes darkness N verbs an operator can half-apply. `Chaos` is the probe owner's arming SEAT rather than a flag, so a recording campaign and a driven replay swap one column.
- Boundary: chaos COMPOSES `Runtime/determinism#ADVERSARIAL_PROBE` and mints nothing — `ChaosArming` owns the per-execution conjunction, `ChaosBand`/`ChaosRow` carry each plane with its rate and weighted catalogue, and `ChaosPosture` is that owner's kill-and-scale cell, so an options body written here, a local posture record, and a second decision seat are three deleted forms; `Randomizer` is handed no context and therefore cannot be addressed at all, so the roll settles at `EnabledGenerator` — the one delegate the package hands a `ResilienceContext` on every execution — while `InjectionRate` pins open and `Randomizer` pins to a constant, because leaving the package's own thread-safe default in the chain reads correct and voids replay. Three planes reach this non-generic pipeline: latency spends the time plane, fault injects the exception rail, and behavior runs a side effect before the call, `AddChaosBehavior` constraining `ResiliencePipelineBuilderBase` so the lane reaches it, while result substitution stays foreclosed by its result-typed builder. Fault WEIGHTS declare as `ChaosRow` rows and the PICK rides the probe owner's seeded draw, since the package's own `FaultGenerator` builds its selection through an internal helper no options member can substitute; a band's rows resolve through `LaneGuard.ChaosFaults`, so an injected fault stays this page's own vocabulary and rides the fold's tail.
- Boundary: `LaneGuard` is the spine owner for the in-process command/solve edge, distinct from the transport `KeyedLane`, and the keyed-pipeline registry mirrors `Wire/outbound#KEYED_PIPELINES` `KeyedLane.Register`'s `AddResiliencePipeline`/`CircuitBreakerManualControl`/`CircuitBreakerStateProvider` pattern verbatim so the in-process and transport resilience share one shape, never a second registry pattern; a kernel fold reached from inside `Run` takes the token the work delegate is HANDED and seats it through that fold's own governance column — the arrangement band's `ArrangementPolicy.Governed(progress, token)` is the landed instance — so the lane's deadline, breaker, and shed all reach the native lane through one token and a kernel-boundary `CancellationTokenSource` is the second owner this discipline forbids; `WorkLane` names the solve-path lane distinct from the `Runtime/resources#DRAIN_QUEUES` `DrainQueue` process-queue name, one altitude per name; the resilience meter carries the lane key as a TAG through `ConfigureTelemetry(TelemetryOptions)` — the `ILoggerFactory` overload sets a logger alone, so a page claiming a per-lane series behind it publishes none; `SeverityProvider` returns `ResilienceEventSeverity` because the Polly callback contract fixes that type, so the kernel `AlertSeverity` ladder rides the `LaneFault` family and never this callback; the two limiter rows both raise `RateLimiterRejectedException`, so the fold resolves `ResilienceTelemetrySource.StrategyName` back through the roster and a lane-pool refusal stays distinguishable from a tenant-bucket refusal; a limiter reached through a lease producer is COMPOSITION-owned and the package disposes none of it, so both admission cells release at the one `DrainBand.Telemetry` participant; the composition registers `TimeProvider` in the container so every registry pipeline's sampling window, break duration, and injected latency ride the one `ClockPolicy` clock; the COMPOSED chain is proved off the built pipeline in the suite through `Polly.Testing` `pipeline.GetPipelineDescriptor().Strategies` — an ordered roster whose `Options` type and `Name` are the assertable identity — because resolution alone admits a lane whose arms silently dropped a strategy, and that inspection dependency belongs on the test plane; no `AddSingleton` spelling — the registry composes through `AddResiliencePipeline` exactly as the keyed transport registry does.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

    [UseDelegateFromConstructor]
    public partial ResiliencePipelineBuilder Arm(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row);

    static ResiliencePipelineBuilder ArmTenant(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddRateLimiter(new RateLimiterStrategyOptions {
            Name = Tenant.Key,
            RateLimiter = args => composition.Tenants.Lease(
                row,
                composition.Allotted(row.Lane.Attempt),
                args.Context.Properties.GetValue(LaneGuard.TenantKey, TenantContext.Root.Entry),
                args.Context.CancellationToken),
        });

    static ResiliencePipelineBuilder ArmBulkhead(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddRateLimiter(new RateLimiterStrategyOptions {
            Name = Bulkhead.Key,
            RateLimiter = args => composition.Permits.Of(row).AcquireAsync(permitCount: 1, args.Context.CancellationToken),
        });

    static ResiliencePipelineBuilder ArmBreaker(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddCircuitBreaker(row.Breaker(composition.Allotted(row.Lane.Attempt), composition.Dark, composition.Evidence.Of(row.Lane)));

    static ResiliencePipelineBuilder ArmDeadline(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        builder.AddTimeout(new TimeoutStrategyOptions {
            Name = Deadline.Key,
            Timeout = composition.Allotted(row.Lane.Attempt),
        });

    static ResiliencePipelineBuilder ArmChaos(ResiliencePipelineBuilder builder, LaneGuard.Composition composition, LanePolicy row) =>
        composition.Chaos.Match(
            Some: arming => composition.Bands(row.Lane).Fold(builder, (chain, band) => band.Kind.Switch(
                state: (Chain: chain, Arming: arming, Band: band, Behaviors: composition.Behaviors),
                latency: static seat => seat.Chain.AddChaosLatency(seat.Arming.Latency(seat.Band)),
                fault: static seat => seat.Chain.AddChaosFault(seat.Arming.Fault(seat.Band, LaneGuard.ChaosFaults)),
                outcome: static seat => seat.Chain,
                behavior: static seat => seat.Chain.AddChaosBehavior(seat.Arming.Behavior(seat.Band, seat.Behaviors)))),
            None: () => builder);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class LaneEvidence {
    readonly FrozenDictionary<WorkLane, CircuitBreakerStateProvider> cells;

    LaneEvidence(FrozenDictionary<WorkLane, CircuitBreakerStateProvider> cells) => this.cells = cells;

    public static LaneEvidence Of() =>
        new(WorkLane.Items.ToFrozenDictionary(static lane => lane, static _ => new CircuitBreakerStateProvider()));

    public CircuitBreakerStateProvider Of(WorkLane lane) => cells[lane];
}

public static class LaneGuard {
    // --- [CONSTANTS]
    public static readonly PredicateBuilder<object> Transient = new PredicateBuilder<object>().Handle<TimeoutRejectedException>();

    public static readonly ResiliencePropertyKey<string> TenantKey = new("rasm.lane.tenant");

    public static class LaneTags {
        public const string Lane = "rasm.lane.key";
        public const string Level = "rasm.lane.level";
    }

    // --- [MODELS]
    public sealed record Composition(
        ILoggerFactory Telemetry,
        Func<DeadlineClass, TimeSpan> Allotted,
        Func<DegradationReading> Pressure,
        TenantLimiters Tenants,
        LanePermits Permits,
        LaneEvidence Evidence,
        CircuitBreakerManualControl Dark,
        Option<ChaosArming> Chaos,
        Func<WorkLane, Seq<ChaosBand>> Bands,
        Func<string, ValueTask> Behaviors);

    public sealed record Runtime(FrozenDictionary<WorkLane, Runtime.Seat> Lanes, Func<DegradationReading> Pressure) {
        public readonly record struct Seat(LanePolicy Row, ResiliencePipeline Pipeline, CircuitBreakerStateProvider Evidence);
    }

    public sealed class TenantLimiters {
        public readonly record struct Claim(LanePolicy Row, TimeSpan Attempt, string Tenant);

        readonly PartitionedRateLimiter<Claim> buckets =
            PartitionedRateLimiter.Create<Claim, (WorkLane Lane, string Tenant)>(static seat =>
                RateLimitPartition.GetTokenBucketLimiter(
                    (seat.Row.Lane, seat.Tenant), _ => seat.Row.Bucket(seat.Attempt)));

        public ValueTask<RateLimitLease> Lease(LanePolicy row, TimeSpan attempt, string tenant, CancellationToken token) =>
            buckets.AcquireAsync(new Claim(row, attempt, tenant), permitCount: 1, token);

        public IO<Unit> Drain() =>
            IO.liftAsync(async () => { await buckets.DisposeAsync(); return unit; });
    }

    public sealed class LaneEnricher(LanePolicy row, Func<DegradationReading> pressure) : MeteringEnricher {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context) {
            context.Tags.Add(new(LaneTags.Lane, row.Lane.Key));
            context.Tags.Add(new(LaneTags.Level, pressure().Level.Key));
            foreach (KeyValuePair<string, object?> tenancy in TenantContext.Current.Tags) {
                context.Tags.Add(tenancy);
            }
        }
    }

    // --- [OPERATIONS]
    public static Exception ChaosFaults(string row) => new InvalidOperationException($"<chaos-fault:{row}>");

    static ResilienceEventSeverity Ranked(SeverityProviderArguments args) =>
        args.Event.Severity >= ResilienceEventSeverity.Warning
        && LaneStrategy.TryGet(args.Source.StrategyName ?? string.Empty, out LaneStrategy? row)
            ? row!.Severity
            : args.Event.Severity;

    public static Fin<IServiceCollection> Register(IServiceCollection services, Composition composition, params ReadOnlySpan<LanePolicy> rows) =>
        Closed(Iterable<LanePolicy>.FromSpan(rows).ToSeq())
            .Map(seated => seated.Fold(services, (graph, row) =>
                graph.AddResiliencePipeline(PipelineKey.Of(row.Lane).Value, builder =>
                    ignore(toSeq(LaneStrategy.Items).Fold(
                        builder.ConfigureTelemetry(new TelemetryOptions {
                            LoggerFactory = composition.Telemetry,
                            MeteringEnrichers = { new LaneEnricher(row, composition.Pressure) },
                            SeverityProvider = static args => Ranked(args),
                        }),
                        (chain, strategy) => strategy.Arm(chain, composition, row))))));

    static Fin<Seq<LanePolicy>> Closed(Seq<LanePolicy> rows) =>
        toSeq(WorkLane.Items)
            .Traverse(lane => rows.Filter(row => row.Lane == lane).Count is 1
                ? Validation<Error, Unit>.Success(unit)
                : new KernelFault.InvalidValue(Label: lane.Key, Requirement: "<exactly-one-lane-policy>"))
            .As()
            .Map(_ => rows)
            .ToFin();

    public static Fin<Runtime> Proven(
        ResiliencePipelineProvider<string> pipelines, Composition composition, params ReadOnlySpan<LanePolicy> rows) =>
        Closed(Iterable<LanePolicy>.FromSpan(rows).ToSeq())
            .Bind(seated => seated
                .Traverse(row => pipelines.TryGetPipeline(PipelineKey.Of(row.Lane).Value, out ResiliencePipeline? pipeline)
                    ? Validation<Error, Runtime.Seat>.Success(new(row, pipeline!, composition.Evidence.Of(row.Lane)))
                    : new KernelFault.InvalidValue(Label: PipelineKey.Of(row.Lane).Value, Requirement: "<a built lane pipeline>"))
                .As()
                .Map(seats => new Runtime(seats.ToFrozenDictionary(static seat => seat.Row.Lane), composition.Pressure))
                .ToFin());

    public static IO<T> Run<T>(Runtime runtime, WorkLane lane, Func<CancellationToken, IO<T>> work) =>
        Dispatched(runtime.Lanes[lane], runtime.Pressure(), lane, work);

    static IO<T> Dispatched<T>(Runtime.Seat seat, DegradationReading reading, WorkLane lane, Func<CancellationToken, IO<T>> work) =>
        Admission.Of(reading, seat.Row, seat.Evidence.CircuitState).Switch(
            state: (Seat: seat, Lane: lane, Work: work),
            admittedCase: static (frame, _) => IO.liftAsync(env => Executed(frame.Seat, frame.Lane, frame.Work, env.Token)).Bind(Lifted),
            shedCase: static (_, refused) => IO.fail<T>(new LaneFault.Shed(refused)));

    static async ValueTask<(Outcome<T> Outcome, Departure From)> Executed<T>(
        Runtime.Seat seat, WorkLane lane, Func<CancellationToken, IO<T>> work, CancellationToken token) {
        ResilienceContext context = ResilienceContextPool.Shared.Get(PipelineKey.Of(lane).Value, token);
        context.Properties.Set(TenantKey, TenantContext.Current.Entry);
        try {
            Outcome<T> outcome = await seat.Pipeline.ExecuteOutcomeAsync(
                static async (ctx, state) => {
                    try {
                        return Outcome.FromResult(
                            await Redrive.Run(state.Law, state.Work(ctx.CancellationToken)).RunAsync().ConfigureAwait(false));
                    }
                    catch (Exception ex) when (ex is not OutOfMemoryException) { return Outcome.FromException<T>(ex); }
                },
                context, (Law: seat.Row.Redrive, Work: work)).ConfigureAwait(false);
            return (outcome, token.IsCancellationRequested ? Departure.Caller : Departure.Attempt);
        }
        finally { ResilienceContextPool.Shared.Return(context); }
    }

    static IO<T> Lifted<T>((Outcome<T> Outcome, Departure From) captured) => (captured.Outcome, captured.From) switch {
        ({ Exception: null, Result: { } value }, _) => IO.pure(value),
        ({ Exception: OperationCanceledException cancelled }, Departure.CallerCase) => IO.fail<T>(
            new LaneFault.CallerLeft(Captured(cancelled))),
        ({ Exception: TimeoutRejectedException slow }, _) => IO.fail<T>(new LaneFault.Deadline(
            Duration.FromTimeSpan(slow.Timeout), Captured(slow))),
        ({ Exception: IsolatedCircuitException dark }, _) => IO.fail<T>(new LaneFault.Dark(
            PipelineKey.Named(dark.TelemetrySource?.PipelineName), Captured(dark))),
        ({ Exception: BrokenCircuitException open }, _) => IO.fail<T>(new LaneFault.Broken(
            Optional(open.RetryAfter).Map(Duration.FromTimeSpan), Captured(open))),
        ({ Exception: RateLimiterRejectedException rejected }, _) => IO.fail<T>(new LaneFault.Rejected(
            Emitting(rejected.TelemetrySource), Optional(rejected.RetryAfter).Map(Duration.FromTimeSpan),
            Captured(rejected))),
        ({ Exception: ErrorException domain }, _) => IO.fail<T>(domain.ToError()),
        ({ Exception: { } foreign }, _) => IO.fail<T>(Captured(foreign)),
        _ => IO.fail<T>(new KernelFault.InvalidResult(Op.Of(), Some("<polly-outcome-empty>"))),
    };

    static Error Captured(Exception raised) => Error.New(raised.Message, raised);

    static Option<LaneStrategy> Emitting(ResilienceTelemetrySource? source) =>
        LaneStrategy.TryGet(source?.StrategyName ?? string.Empty, out LaneStrategy? strategy) ? Optional(strategy) : None;
}
```

## [04]-[ADAPTIVE_ARMS]

- Owner: `AdaptiveConcurrency` the static ResourceMonitoring-fed permit-resize projection; `LanePermits` the live per-lane limiter cell the lane's admission row leases from and the resize is the sole writer of.
- Entry: `Resize(LanePolicy row, Utilization utilization)` returns `int` — tapers the lane's permit count between its full pool and its narrowed floor from the live CPU and memory utilization; `Of(LanePolicy row)` returns `RateLimiter` — the lease producer the `LaneStrategy.Bulkhead` arm binds, re-seating the lane's `ConcurrencyLimiter` when a resize moves the count and returning the seated one otherwise; `Drain()` returns `IO<Unit>` — the `DrainParticipantPort` body releasing every retired limiter.
- Auto: the resize reads the `Observability/health#HEALTH_FOLD` `UtilizationCell` CPU and memory ratios graded against the `ResourceQuota` container limit — the composition root supplies `UtilizationCell.Read` as the cell's utilization function — so the permit resize rides the same observable-instrument-and-quota path the host pressure grade reads, never a parallel meter, and a lane under cgroup throttling narrows on the limit it runs under; the resize decision is cadence-gated on a `MonotonicTimeline` span rather than a timer, so the lease producer runs per execution while the utilization read and the limiter mint run once per interval; a moved count seats a fresh `ConcurrencyLimiter` and PARKS the retired instance instead of disposing it, because a `RateLimitLease` releases against the instance that issued it and disposing under an outstanding lease strands the permit; the resize is bounded between the row's derived floor and its full pool so adaptive concurrency tunes within a band, never to zero.
- Auto: the parked limiters release inside the `DrainBand.Telemetry` participant row; the live permit count reaches the meter as the lane's own resilience series.
- Packages: Microsoft.Extensions.Diagnostics.ResourceMonitoring, Polly.RateLimiting, System.Threading.RateLimiting, Rasm (kernel `Cell`/`Transition`/`MonotonicTimeline`), LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new utilization signal is one enabled instrument the `UtilizationCell` reads; a new grading input is one `Utilization` column the taper folds; zero new surface.
- Boundary: the adaptive arms read the existing health owners — the `UtilizationCell` for utilization and the `DegradationReading` for the shed level — never a second resource meter or a second pressure cell; the resize projection is pure and the cell is its ONLY writer, so a permit count computed and applied nowhere is the deleted form this seam exists to foreclose; utilization grades on the BINDING constraint — the higher of the CPU and memory ratios — so a lane pressured by allocation narrows exactly as one pressured by compute; the taper is continuous between the two named ratios because a two-step ladder gives every intermediate pressure one of two widths and re-decides the space an interpolation already generates.
- Boundary: every seat transition answers a VERDICT — the first reach is `Cell.Claim` whose `Ceded` case tells the losing minter to dispose the limiter it built, and each resize is `Cell.Step` whose guard re-reads the seat it saw so only the winner parks the retired instance; a swap that answers the post-state alone cannot tell those two callers apart, which is why the retired limiter used to be handed to a drain that read an already-emptied list. The cadence read is `MonotonicTimeline`, so a clock that refuses to measure leaves the lane at its SEATED width rather than resizing on an unmeasured span — an unresized lane is correct-but-stale, while a resize on a broken span is a fabricated width.
- Boundary: this cell is a keyed limiter SET and the branch ruling that keyed limiter sets ride `PartitionedRateLimiter.Create` does not reach it, because that ruling is about unbounded key cardinality (one bucket per tenant the process ever saw) while this key space is the six-row `WorkLane` roster and its whole purpose is REPLACING an instance the partition factory would only ever mint once; the tenant cell above IS the partitioned form, so the two admission shapes sit on opposite sides of that discriminant by declaration. Retirement is bounded on both ends: the reclaim sweep releases every parked limiter whose `IdleDuration` reads present — the package's own evidence that every permit returned — so a long-lived process does not accumulate one retired limiter per resize interval, and the drain releases whatever the sweeps left.

```csharp
// --- [CONSTANTS] -----------------------------------------------------------------------
public static class AdaptiveConcurrency {
    public const double Pressured = 0.75d;
    public const double Saturated = 0.90d;

    // --- [OPERATIONS]
    public static int Resize(LanePolicy row, Utilization utilization) =>
        double.Max(utilization.CpuRatio, utilization.MemoryRatio) switch {
            <= Pressured => row.Permits,
            >= Saturated => row.Narrowed,
            var load => row.Narrowed + (int)double.Round((row.Permits - row.Narrowed) * ((Saturated - load) / (Saturated - Pressured))),
        };
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class LanePermits(ClockPolicy clocks, Func<Utilization> utilization) {
    public static readonly Duration ResizeInterval = Duration.FromSeconds(5);

    readonly Atom<HashMap<WorkLane, Seat>> seats = Atom(HashMap<WorkLane, Seat>());
    readonly Atom<Seq<ConcurrencyLimiter>> parked = Atom(Seq<ConcurrencyLimiter>());

    readonly record struct Seat(int Permits, Option<MonotonicStamp> Mark, ConcurrencyLimiter Limiter);

    readonly record struct Retirement(Seq<ConcurrencyLimiter> Idle, Seq<ConcurrencyLimiter> Held) {
        public static readonly Retirement Empty = new(Seq<ConcurrencyLimiter>(), Seq<ConcurrencyLimiter>());
    }

    public RateLimiter Of(LanePolicy row) =>
        seats.Value.Find(row.Lane).Match(
            Some: held => Cadenced(row, held),
            None: () => Contested(row, Seated(row, row.Permits)));

    RateLimiter Contested(LanePolicy row, Seat candidate) =>
        Cell.Claim(seats, row.Lane, () => candidate) switch {
            Transition<HashMap<WorkLane, Seat>>.Committed committed => committed.State[row.Lane].Limiter,
            var ceded => (candidate.Limiter.Dispose(), ceded.Current[row.Lane].Limiter).Item2,
        };

    Seat Seated(LanePolicy row, int permits) =>
        new(permits, clocks.Line.Capture().ToOption(), new ConcurrencyLimiter(row.Pool(permits)));

    RateLimiter Cadenced(LanePolicy row, Seat held) =>
        Measured(held).Match(
            Some: read => read.Span >= ResizeInterval.ToTimeSpan() ? Resized(row, held, read.Now) : held.Limiter,
            None: () => held.Limiter);

    Option<(MonotonicStamp Now, TimeSpan Span)> Measured(Seat held) =>
        from mark in held.Mark
        from now in clocks.Line.Capture().ToOption()
        from span in clocks.Line.Elapsed(mark, now).ToOption()
        select (now, span);

    RateLimiter Resized(LanePolicy row, Seat held, MonotonicStamp now) =>
        Sized(row, held, now, AdaptiveConcurrency.Resize(row, utilization()));

    RateLimiter Sized(LanePolicy row, Seat held, MonotonicStamp now, int permits) =>
        permits == held.Permits
            ? Stepped(row, held, held with { Mark = Some(now) }).Current[row.Lane].Limiter
            : Swapped(row, held, Seated(row, permits));

    RateLimiter Swapped(LanePolicy row, Seat held, Seat next) =>
        Stepped(row, held, next) switch {
            Transition<HashMap<WorkLane, Seat>>.Committed committed =>
                (parked.Swap(rows => rows.Add(held.Limiter)), committed.State[row.Lane].Limiter).Item2,
            var declined => (next.Limiter.Dispose(), declined.Current[row.Lane].Limiter).Item2,
        };

    Transition<HashMap<WorkLane, Seat>> Stepped(LanePolicy row, Seat held, Seat next) =>
        Cell.Step(
            seats,
            map => map.Find(row.Lane).Filter(seat => ReferenceEquals(seat.Limiter, held.Limiter)).Map(_ => map.SetItem(row.Lane, next)),
            new KernelFault.InvalidValue(Label: row.Lane.Key, Requirement: "<the seat this caller read>"));

    public IO<Unit> Reclaim() =>
        IO.lift(() => Cell.Take(parked).Current.Fold(Retirement.Empty, static (split, limiter) =>
                limiter.IdleDuration is null
                    ? split with { Held = split.Held.Add(limiter) }
                    : split with { Idle = split.Idle.Add(limiter) }))
            .Bind(split => IO.lift(() => ignore(parked.Swap(rows => split.Held + rows)))
                .Bind(_ => split.Idle.TraverseM(Released).As()))
            .Map(static _ => unit);

    public IO<Unit> Drain() =>
        IO.lift(() => Cell.Take(parked).Current)
            .Bind(static rows => rows.TraverseM(Released).As())
            .Map(static _ => unit);

    static IO<Unit> Released(ConcurrencyLimiter limiter) =>
        IO.liftAsync(async () => { await limiter.DisposeAsync(); return unit; });
}
```

## [05]-[ADMISSION]

- Owner: `Admission` `[Union]` the per-`WorkLane` admission verdict minted once here and crossed to Compute admission; `ShedCause` `[SmartEnum<string>]` the refusal's own reason vocabulary; `LaneReading` the evidence triple both cases carry.
- Cases: `Admission` = `Admitted(LaneReading)` | `Shed(LaneReading, ShedCause)`; `ShedCause` = `Pressure` (the host reading reached the lane's declared floor) | `Dark` (an operator isolated the capability group) | `Broken` (the lane's own breaker is open) — three causes with three different operator responses, which a single refusal bit cannot carry.
- Entry: `Of(DegradationReading reading, LanePolicy row, CircuitState breaker)` returns `Admission` — mints the per-lane verdict from the atomic `DegradationReading` against the lane's own configured `ShedFloor` and the lane breaker's own evidence, so admission is computed once at the in-process governor and consumed downstream rather than re-derived from raw saturation.
- Auto: the verdict reads the one atomic `DegradationReading` so the refusal and the level it derives from are coherent, never a stale-snapshot-against-fresh-level race; host pressure is global and the per-lane axis is the FLOOR the lane's own `LanePolicy` declares; the breaker column folds the lane's own health into the same verdict, so a dark or broken lane refuses admission at the one seat pressure already refuses at; the cause is decided child-before-parent — operator darkness, then an organic break, then host pressure — so an isolated group never reports as a dependency failure an on-call rotation would chase; the verdict is minted once at the in-process governor edge and crosses to `Rasm.Compute/Runtime/admission` as the one verdict the Compute `SubstrateSelection` fold consumes.
- Auto: the case IS the cross-package fact the Compute admission consumes and the typed evidence `LaneFault.Shed` carries.
- Packages: Polly.Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new admission input is one column on `LaneReading`; a new refusal reason is one `ShedCause` row breaking every consumer that switches on cause; zero new surface.
- Boundary: the verdict is the one per-`WorkLane` admission fact — the case `LaneGuard` mints from the atomic `DegradationReading` is the one verdict `Rasm.Compute/Runtime/admission` consumes on its `SubstrateSelection` fold rather than a Compute-side re-derivation (the `ONE_DEGRADATION_SHED_VERDICT` ripple), so the in-process refusal and the Compute veto read one value and a Compute-side re-derivation from raw saturation is the rejected form; the seam couples to the CASE, so a consumer reads `is Admission.ShedCase { Cause: … }` and carries lane, level, and cause into its own hop reason — the earlier `bool Shed` column ORed pressure and breaker into one bit, so a Compute reason string could not say which refusal it was degrading around, and that erasure is what the union retires; the breaker column refuses on `Open` and `Isolated` and ADMITS on `HalfOpen`, because a half-open breaker is waiting for exactly one probe and a verdict refusing that probe leaves the lane dark until something else dispatches — a recovery deadlock the two-state read forecloses; `CircuitBreakerStateProvider` answers `Closed` while unattached, so a verdict minted before the provider binds admits rather than inventing darkness; the consumer count stays TWO under a kernel fold, because `Run` refuses BEFORE it invokes the work delegate — a refused lane never enters the fold, so the kernel governance band inherits the decision by absence.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShedCause {
    public static readonly ShedCause Pressure = new("pressure");
    public static readonly ShedCause Dark = new("dark");
    public static readonly ShedCause Broken = new("broken");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct LaneReading(WorkLane Lane, DegradationLevel Level, CircuitState Breaker);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Admission {
    private Admission() { }

    public sealed record AdmittedCase(LaneReading Reading) : Admission;
    public sealed record ShedCase(LaneReading Reading, ShedCause Cause) : Admission;

    public LaneReading Reading => Switch(
        admittedCase: static row => row.Reading,
        shedCase: static row => row.Reading);

    public static Admission Of(DegradationReading reading, LanePolicy row, CircuitState breaker) =>
        Cased(new LaneReading(row.Lane, reading.Level, breaker), row.ShedFloor);

    static Admission Cased(LaneReading seen, DegradationLevel floor) => seen.Breaker switch {
        CircuitState.Isolated => new ShedCase(seen, ShedCause.Dark),
        CircuitState.Open => new ShedCase(seen, ShedCause.Broken),
        _ => seen.Level.Rank >= floor.Rank ? new ShedCase(seen, ShedCause.Pressure) : new AdmittedCase(seen),
    };
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
