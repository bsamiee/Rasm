# [APPHOST_WORK_LANE_GOVERNOR]

The in-process work-lane resilience governor for the runtime spine: one `LaneGuard` composes a keyed Polly `ResiliencePipeline` per `WorkLane` — mirroring the `Wire/outbound#KEYED_PIPELINES` `KeyedLane.Register` per-hop `AddResiliencePipeline` registry but for the in-process command/solve edge the transport-only Polly registry never reached — with bulkhead, adaptive concurrency, circuit breaker, load-shed, hedge, and `Polly.Simmy` chaos as pipeline rows, the adaptive-concurrency arm reading `Microsoft.Extensions.Diagnostics.ResourceMonitoring` and the load-shed arm reading the collapsed `Observability/health#DEGRADATION_RAIL` `DegradationReading`, so the in-process lanes degrade gracefully, isolate per lane, tune concurrency at runtime, and carry a first-class chaos surface symmetric to the transport `KeyedLane`. The per-`WorkLane` shed verdict mints once here from the atomic `DegradationReading` and crosses to `Rasm.Compute/Runtime/admission` (the `ONE_DEGRADATION_SHED_VERDICT` ripple) rather than a Compute-side re-derivation. The page owns the lane-guard registry, the adaptive-concurrency and load-shed arms, the chaos axis, and the shed verdict; it consumes `WorkLane` (the Compute solve-path name), `KeyedLane`/`CircuitBreakerManualControl`/`CircuitBreakerStateProvider`, `DegradationCell`/`DegradationReading`/`DegradationLevel`, `UtilizationCell`/`ResourceQuota`, `DeadlineClass`/`ClockPolicy`, and `ReceiptSinkPort` as settled vocabulary and mints no eighth port.

## [01]-[INDEX]

- [02]-[LANE_GUARD]: One keyed Polly `ResiliencePipeline` per `WorkLane` with bulkhead, breaker, hedge, and chaos rows.
- [03]-[ADAPTIVE_ARMS]: The ResourceMonitoring-fed concurrency arm and the `DegradationReading` load-shed arm.
- [04]-[SHED_VERDICT]: The per-`WorkLane` shed verdict minted once and crossed to Compute admission.

## [02]-[LANE_GUARD]

- Owner: `LaneStrategy` `[SmartEnum<string>]` the per-lane pipeline-row vocabulary under the `ComparerAccessors.StringOrdinal` accessor; `LanePolicy` the per-`WorkLane` resilience-row record; `LaneGuard` the static keyed-pipeline registry over the in-process lanes; `LaneFault` `[Union]` fault family deriving its codes through `FaultBand.Lane` = Text | BulkheadRejected | Shed | LaneBroken.
- Entry: `Register(IServiceCollection services, ILoggerFactory telemetry, Func<DeadlineClass, TimeSpan> allotted, Func<DegradationReading> pressure, TenantLimiters tenants, bool chaos, params ReadOnlySpan<LanePolicy> rows)` — one `AddResiliencePipeline` entry per `WorkLane` row keyed by the lane key, composing bulkhead, tenant-partitioned rate-limiter, circuit-breaker, the `AddTimeout` deadline over the `allotted` attempt-class read, hedge, and (on the test-host profile) chaos strategies, with `TelemetryOptions` carrying the lane's own metering enricher and severity map; `Run(LaneGuard.Runtime runtime, WorkLane lane, Func<CancellationToken, ValueTask<T>> work)` returns `IO<T>` — executes the in-process work through the lane's keyed pipeline so the command/solve edge rides the lane's resilience.
- Auto: each lane's pipeline is one keyed `ResiliencePipeline` registered through `AddResiliencePipeline<T>(lane.Key, ...)` exactly as `KeyedLane.Register` registers per hop, but for the in-process command/solve edge so the lane and the hop share one resilience pattern and one retry-owner discipline — exactly one retry owner per lane just as each hop has exactly one; the bulkhead is a `ConcurrencyLimiterStrategyOptions` bounded-permit isolation per lane so a saturated lane cannot starve another lane's permits, the rate-limiter partitions that admission BY TENANT — a `RateLimiterStrategyOptions.RateLimiter` lease producer over a per-tenant `TokenBucketRateLimiter` keyed on the branch-settled `TenantId.Wire` text — so one tenant's burst bounds at its own bucket instead of consuming the lane's whole pool, and `OnRejected` carries `RateLimiterRejectedException.RetryAfter` onto the refusal so a rejected caller learns when to return, the circuit-breaker binds a `CircuitBreakerManualControl` and `CircuitBreakerStateProvider` keyed per lane so the breaker state reads from Polly's own observation surface (never a parallel state delegate), and the hedge admits only idempotent commands so a duplicated non-idempotent solve never double-applies; the adaptive-concurrency arm reads `ResourceMonitoring` and resizes the bulkhead permit count at runtime (`ADAPTIVE_ARMS`), and the load-shed arm reads the atomic `DegradationReading` and sheds at the lane's degradation floor; the chaos strategies arm on the test-host profile only — `AddChaosLatency`/`AddChaosFault`/`AddChaosOutcome` over `ChaosLatencyStrategyOptions`/`ChaosFaultStrategyOptions`/`ChaosOutcomeStrategyOptions` with an `InjectionRate` column, the Simmy builder extensions shipping inside `Polly.Core` itself so no second package row admits them — so the work lanes carry a first-class chaos surface symmetric to the transport `KeyedLane` chaos, never in production; adaptive concurrency reads `TimeProvider` through `ClockPolicy`, never `Stopwatch`.
- Receipt: a lane execution's resilience events land under the lane key in the package meter and logger exactly as the keyed-pipeline events do — the lane key, the live shed level, and the tenant slot are `MeteringEnricher` tags on the measurement itself, never the DI registration key a query cannot group by, and `SeverityProvider` maps the `LaneFault` family onto the incident ladder; a shed decision fans the shed verdict (`SHED_VERDICT`); no parallel lane receipt.
- Packages: Polly.Core, Polly.Extensions, Polly.RateLimiting, Microsoft.Extensions.Diagnostics.ResourceMonitoring, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one lane strategy is one pipeline row inside one keyed pipeline; one chaos arm is one `AddChaos*` strategy row on the test-host profile; a new lane is one `LanePolicy` row keyed by its `WorkLane`; a new resilience dimension is one `LaneEnricher` tag beside its `LaneTags` const; zero new surface.
- Boundary: `LaneGuard` is the new spine owner for the in-process command/solve edge, distinct from the transport `KeyedLane` — it must not become a second retry on the transport seam, so in-process lanes get exactly one retry owner (`LaneGuard`) just as each hop has exactly one and a retry both on the lane and re-applied at the hop on one seam is the rejected form (the one-retry-owner-per-seam discipline); `WorkLane` is the `Rasm.Compute` solve-path name distinct from the `Runtime/resources#DRAIN_QUEUES` `DrainQueue` process-queue name, so `LaneGuard` keys a pipeline per `WorkLane` but never owns the `WorkLane` vocabulary; the keyed-pipeline registry mirrors `Wire/outbound#KEYED_PIPELINES` `KeyedLane.Register`'s `AddResiliencePipeline`/`CircuitBreakerManualControl`/`CircuitBreakerStateProvider` pattern verbatim so the in-process and transport resilience share one shape, never a second registry pattern; the resilience meter carries the lane key as a TAG through `ConfigureTelemetry(TelemetryOptions)` — the `ILoggerFactory` overload sets a logger alone, so a page claiming a per-lane series behind it publishes none and the deleted form is that overload beside the claim; the rate-limiter partitions on tenant because the bulkhead's isolation is cross-lane only, so an unpartitioned lane pool is the intra-lane starvation the page's own isolation clause never covered; chaos arms on the test-host profile only so a production lane carries zero chaos; the adaptive-concurrency arm reads `TimeProvider` through `ClockPolicy`, never a direct `Stopwatch` call site; no `AddSingleton` spelling — the registry composes through `AddResiliencePipeline` exactly as the keyed transport registry does.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LaneStrategy {
    public static readonly LaneStrategy Bulkhead = new("bulkhead");
    public static readonly LaneStrategy RateLimiter = new("rate-limiter");
    public static readonly LaneStrategy CircuitBreaker = new("circuit-breaker");
    public static readonly LaneStrategy Hedge = new("hedge");
    public static readonly LaneStrategy Chaos = new("chaos");
}

[Union]
public abstract partial record LaneFault : Expected, IValidationError<LaneFault> {
    private LaneFault(string detail, int code) : base(detail, code, None) { }
    public static LaneFault Create(string message) => new Text(message);
    public sealed record Text : LaneFault { public Text(string detail) : base(detail, FaultBand.Lane.Code(0)) { } }
    public sealed record BulkheadRejected : LaneFault { public BulkheadRejected(string detail) : base(detail, FaultBand.Lane.Code(1)) { } }
    public sealed record Shed : LaneFault { public Shed(string detail) : base(detail, FaultBand.Lane.Code(2)) { } }
    public sealed record LaneBroken : LaneFault { public LaneBroken(string detail) : base(detail, FaultBand.Lane.Code(3)) { } }
}

public sealed record LanePolicy(
    WorkLane Lane,
    int BulkheadPermits,
    int QueueLimit,
    bool Hedges,
    DegradationLevel ShedFloor,
    DeadlineClass Attempt) {
    public string Key => $"lane:{Lane}";
}

public static class LaneGuard {
    public const int BulkheadPermits = 32;
    public const int QueueLimit = 64;
    public const int HedgeAttempts = 2;
    public const double ChaosInjectionRate = 0.05d;
    static readonly ConcurrentDictionary<string, (CircuitBreakerManualControl Control, CircuitBreakerStateProvider State)> Breakers = new(StringComparer.Ordinal);
    static (CircuitBreakerManualControl Control, CircuitBreakerStateProvider State) Of(string key) =>
        Breakers.GetOrAdd(key, static _ => (new CircuitBreakerManualControl(), new CircuitBreakerStateProvider()));
    public static CircuitBreakerManualControl BreakerOf(string laneKey) => Of(laneKey).Control;
    public static CircuitBreakerStateProvider StateOf(string laneKey) => Of(laneKey).State;

    public sealed record Runtime(
        ResiliencePipelineProvider<string> Pipelines,
        Func<DegradationReading> Pressure,
        Func<WorkLane, DegradationLevel> Floor,
        ClockPolicy Clocks);

    // Non-generic per-lane pipelines: bulkhead, breaker, timeout, optional hedge, and (test-host only) chaos.
    // The non-generic ResiliencePipeline executes heterogeneous work generically per call, so one lane
    // pipeline guards every solve on that lane without a per-result-type registration.
    static readonly ResiliencePropertyKey<TimeSpan?> RetryAfterKey = new("rasm.lane.retry-after");

    // The per-tenant lease producer: one TokenBucketRateLimiter per tenant wire text, minted once and reused,
    // so a lane's admission partitions on the SAME identity the meter tag, the RLS predicate, and the receipt
    // fold key on — never a second tenant alphabet.
    public sealed class TenantLimiters(TokenBucketRateLimiterOptions options) {
        readonly ConcurrentDictionary<string, RateLimiter> byTenant = new(StringComparer.Ordinal);
        public RateLimiter Limiter(string tenant) => byTenant.GetOrAdd(tenant, _ => new TokenBucketRateLimiter(options));
    }

    // The lane's own dimensions on every resilience measurement: the lane key the page's Receipt clause
    // promises, the live shed level, and the tenant slot the branch dimension-key ruling fixes — so a per-lane
    // resilience series is real and joins the rest of the estate's telemetry on one grammar.
    public sealed class LaneEnricher(LanePolicy row, Func<DegradationReading> pressure) : MeteringEnricher {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context) {
            context.Tags.Add(new(LaneTags.Lane, row.Lane.ToString()));
            context.Tags.Add(new(LaneTags.Shed, ShedVerdict.Of(pressure(), row.Lane, row.ShedFloor).Level.ToString()));
            context.Tags.Add(new(TenantContext.TenantSlot, TenantContext.Current.Id.Wire));
        }
    }

    public static class LaneTags {
        public const string Lane = "rasm.lane.key";
        public const string Shed = "rasm.lane.shed";
    }

    public static IServiceCollection Register(IServiceCollection services, ILoggerFactory telemetry, Func<DeadlineClass, TimeSpan> allotted, Func<DegradationReading> pressure, TenantLimiters tenants, bool chaos, params ReadOnlySpan<LanePolicy> rows) =>
        rows.ToArray().ToSeq().Fold(services, (graph, row) =>
            graph.AddResiliencePipeline(row.Key, builder => {
                builder
                    // The ILoggerFactory overload sets a logger and NOTHING else: the lane key is only the DI
                    // registration key, so the promised per-lane resilience series does not exist as a metric
                    // dimension. The options overload is where the estate's own grammar reaches the meter —
                    // MeteringEnrichers append the lane key, the shed level, and the tenant slot per the branch
                    // dimension-key ruling, and SeverityProvider maps the lane fault family onto the kernel
                    // AlertSeverity ladder rather than Polly's flat package default.
                    .ConfigureTelemetry(new TelemetryOptions {
                        LoggerFactory = telemetry,
                        MeteringEnrichers = { new LaneEnricher(row, pressure) },
                        SeverityProvider = static args => args.Event.EventName switch {
                            nameof(LaneFault.LaneBroken) => ResilienceEventSeverity.Error,
                            nameof(LaneFault.Shed) or nameof(LaneFault.BulkheadRejected) => ResilienceEventSeverity.Warning,
                            _ => ResilienceEventSeverity.Information,
                        },
                    })
                    .AddConcurrencyLimiter(permitLimit: row.BulkheadPermits, queueLimit: row.QueueLimit)
                    // The concurrency limiter is ONE global permit pool per lane, so one tenant's burst consumes
                    // every permit and starves the rest at the same WorkLane — the page's cross-lane isolation
                    // clause forecloses starvation BETWEEN lanes and never WITHIN one. The rate-limiter strategy
                    // is the partitioning seam LaneStrategy.RateLimiter names: the lease producer keys a
                    // per-tenant TokenBucketRateLimiter on the branch-settled TenantId.Wire text, so a tenant's
                    // burst bounds at its own bucket, and RetryAfter projects into the refusal so a rejected
                    // caller learns when to return rather than re-arriving blind.
                    .AddRateLimiter(new RateLimiterStrategyOptions {
                        RateLimiter = args => tenants.Limiter(TenantContext.Current.Id.Wire).AcquireAsync(1, args.Context.CancellationToken),
                        OnRejected = args => {
                            args.Context.Properties.Set(RetryAfterKey,
                                args.Outcome.Exception is RateLimiterRejectedException rejected ? rejected.RetryAfter : null);
                            return default;
                        },
                    })
                    .AddCircuitBreaker(new CircuitBreakerStrategyOptions {
                        ManualControl = BreakerOf(row.Key),
                        StateProvider = StateOf(row.Key),
                    })
                    .AddTimeout(allotted(row.Attempt));
                if (row.Hedges)
                    builder.AddHedging(new HedgingStrategyOptions { MaxHedgedAttempts = HedgeAttempts });
                if (chaos)
                    builder
                        .AddChaosLatency(new ChaosLatencyStrategyOptions { InjectionRate = ChaosInjectionRate, Latency = allotted(row.Attempt) })
                        .AddChaosFault(new ChaosFaultStrategyOptions { InjectionRate = ChaosInjectionRate, FaultGenerator = static _ => new ValueTask<Exception?>(new LaneFault.LaneBroken("chaos").ToException()) });
            }));

    public static IO<T> Run<T>(Runtime runtime, WorkLane lane, Func<CancellationToken, ValueTask<T>> work) =>
        ShedVerdict.Of(runtime.Pressure(), lane, runtime.Floor(lane)).Shed
            ? IO.fail<T>(new LaneFault.Shed($"lane:{lane}"))
            : IO.liftAsync(async env => await runtime.Pipelines.GetPipeline($"lane:{lane}").ExecuteAsync(work, env.Token));
}
```

## [03]-[ADAPTIVE_ARMS]

- Owner: `AdaptiveConcurrency` the static ResourceMonitoring-fed permit-resize arm; `LoadShed` the static `DegradationReading`-fed shed arm.
- Entry: `Resize(LanePolicy policy, Utilization utilization)` returns `int` — projects the lane's bulkhead permit count from the live CPU/memory utilization so a pressured host narrows the lane and an idle host widens it; `Shed(LanePolicy policy, DegradationReading reading)` returns `bool` — the load-shed verdict reads the atomic `DegradationReading` and sheds when the derived level meets or exceeds the lane's `ShedFloor`.
- Auto: the adaptive-concurrency arm reads the `Observability/health#HEALTH_FOLD` `UtilizationCell` CPU and memory ratios graded against the `ResourceQuota` container limit, so the permit resize rides the same observable-instrument-and-quota path the host pressure grade reads, never a parallel meter — a lane under cgroup throttling narrows on the limit it actually runs under; the load-shed arm reads the one `DegradationReading` (the collapsed `(snapshot, level)` cell) so the shed decision reads a coherent pressure value and a lane never sheds on a stale snapshot against a fresh level — the prior two-surface read is the collapsed form the shed arm depends on; the resize is bounded between a floor and the lane's configured permits so adaptive concurrency tunes within a band, never to zero; the shed floor is a `DegradationLevel` row so a lane sheds at its own floor and a `Suspended` host sheds every non-critical lane through the existing degradation rail, never a parallel throttle.
- Packages: Microsoft.Extensions.Diagnostics.ResourceMonitoring, LanguageExt.Core, BCL inbox
- Growth: a new utilization signal is one enabled instrument the `UtilizationCell` reads; a new shed input is one column on the `DegradationReading` read; zero new surface.
- Boundary: the adaptive arms read the existing health owners — the `UtilizationCell` for utilization and the `DegradationReading` for the shed level — never a second resource meter or a second pressure cell; the load-shed arm reads the atomic `DegradationReading` so its decision is race-free, the exact reason the `COLLAPSE_HEALTH_DEGRADATION_CELL` collapse exists; the resize tunes the bulkhead permit count within a band so the lane never starves or floods; the shed floor is a `DegradationLevel` row so the shed and the degradation rail share one level vocabulary, never two.

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
- Auto: the verdict reads the one atomic `DegradationReading` so the shed flag and the level it derives from are coherent, never a stale-snapshot-against-fresh-level race; the verdict is minted once at the in-process governor edge and crosses to `Rasm.Compute/Runtime/admission` as the one shed verdict the Compute `SubstrateSelection` fold consumes on its admission decision, so the Compute side never re-derives the shed from raw saturation — one verdict, one mint, two consumers (the in-process `LaneGuard.Run` shed and the Compute admission shed); a lane below its shed floor admits, at or above sheds.
- Receipt: the shed verdict is the cross-package fact the Compute admission consumes; no parallel verdict receipt.
- Packages: LanguageExt.Core, BCL inbox
- Growth: a new shed input is one column on the verdict; zero new surface.
- Boundary: the shed verdict is the one per-`WorkLane` shed fact — the per-lane shed verdict `LaneGuard` mints from the atomic `DegradationReading` is the one verdict `Rasm.Compute/Runtime/admission` consumes on its `SubstrateSelection` fold rather than a Compute-side re-derivation (the `ONE_DEGRADATION_SHED_VERDICT` ripple), so the in-process lane shed and the Compute admission shed read one verdict and a Compute-side re-derivation from raw saturation is the rejected form; the verdict reads the atomic `DegradationReading` so it is race-free, the reason the health-cell collapse exists.

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
