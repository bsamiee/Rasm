# [APPHOST_OUTBOUND_RESILIENCE]

Outbound boundary ownership for the runtime spine: seven `OutboundHop` cases bind to frozen `HopPolicy` rows, every hop holds exactly one retry surface — the standard or hedging HTTP handler on SocketsHttpHandler-borne rows, one keyed Polly pipeline per non-HTTP row — admission folds the degradation gate, the modality exclusion, and the retry-owner claim into one `Fin` rail, every dispatch exits as a `HopReceipt`, and the `LocalIpc` peer attaches through the discovery manifest law. The page owns the hop axis, both pipeline registries, the ownership law over the Polly, Http.Resilience, and Grpc.Net.Client spine, and the discovery manifest, UDS attach, checksum gate, and companion-spawn lifecycle that seat the `LocalIpc(DiscoveryManifest Peer)` hop case.

## [01]-[INDEX]

- [01]-[HOP_AXIS]: Seven hop cases bound to frozen policy rows with total dispatch.
- [02]-[HTTP_PIPELINES]: Standard and hedging handlers for `SocketsHttpHandler`-borne rows.
- [03]-[KEYED_PIPELINES]: One keyed Polly registry and channel policy for non-HTTP hops.
- [04]-[OWNERSHIP_LAW]: One retry owner per hop with conflict evidence and receipts.
- [05]-[DISCOVERY_ATTACH]: Manifest law, UDS attach, checksum gate, and companion child lifecycle.
- [06]-[DELIVERY_FANOUT]: Multi-channel notification fan-out, delivery receipts, and dedupe.

## [02]-[HOP_AXIS]

- Owner: `OutboundHop` `[Union]` seven sealed hop cases; `HopPolicy` per-case row record; `HopRows` frozen row set with the total dispatch; `HopIdempotency` keyless vocabulary; `HopTransport` keyless byte-mover vocabulary; `HopRateLimit` keyless admission-shape vocabulary; `HopDelivery` keyless delivery-honesty vocabulary — every row STATES its guarantee, never assumes one; `HopFault` `[Union]` fault family deriving its codes through `FaultBand.Hop`; `ReleaseIdentity` vehicle-free update identity.
- Cases: HttpApi, Grpc, ServerStream, CompanionSpawn, LocalIpc, WebhookPost, UpdateCheck — the stream case is gRPC server-stream; UpdateCheck carries `ReleaseIdentity` and is structurally excluded on plugin rows.
- Entry: `HopPolicy Policy` — extension property; total state-free `Switch` from case to frozen row.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one case plus one `HopPolicy` row absorbs a new outbound boundary; the update vehicle lands as one `UpdatePort` row on the UpdateCheck case; the admission shape lands as one `HopRateLimit` key plus one `Admission` column value, never a second limiter rail; zero new surface.
- Boundary: deadline durations are the two hop deadline rows read by projection, never literals here; database retry is excluded from the hop law — the store execution strategy owns it; the `Transport` column names the byte mover on every row — `SocketsHttpHandler`, `GrpcChannel`, or process spawn — and resilience-event enrichment is the pipeline-key tag, never a second enrichment surface; the `Admission` column names the pipeline-head limiter shape on every row — `Concurrency` for the bounded-permit default, `SlidingWindow` for the webhook segment cap, `TokenBucket` for the redial-paced peer hop — read by `KeyedLane.Limiter`, never a second limiter selector; the `Delivery` column is the honesty axis every row STATES — `BestEffort` (local-ipc, spawn, update-check: a lost frame is acceptable evidence loss), `AtLeastOnce` (webhook, http, server-stream: the retry schedule redelivers so consumers dedupe by delivery key), `ExactlyOnceEffective` (the wire-native gRPC hop the outbox drain rides: at-least-once transport + consumer dedupe by `id`=`ContentKey` per the Persistence CloudEvents law, the Persistence egress pump composing this exact column) — a hop whose guarantee is unstated and a claim stronger than the transport provides are both deleted forms; the row's `Needs` capability is the degradation gate and `ExcludedOn` is the modality fence; webhook delivery identity is the case's `DeliveryKey` payload, never a header literal.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutboundHop {
    private OutboundHop() { }

    public sealed record HttpApi(Uri Authority) : OutboundHop;
    public sealed record Grpc(Uri Address) : OutboundHop;
    public sealed record ServerStream(Uri Address) : OutboundHop;
    public sealed record CompanionSpawn(ProcessStartInfo Spec) : OutboundHop;
    public sealed record LocalIpc(DiscoveryManifest Peer) : OutboundHop;
    public sealed record WebhookPost(Uri Target, Guid DeliveryKey) : OutboundHop;
    public sealed record UpdateCheck(ReleaseIdentity Installed) : OutboundHop;
}

[Union]
public abstract partial record HopFault : Expected, IValidationError<HopFault> {
    private HopFault(string detail, int code) : base(detail, code, None) { }

    public static HopFault Create(string message) => new Text(message);

    public sealed record Text : HopFault { public Text(string detail) : base(detail, FaultBand.Hop.Code(0)) { } }
    public sealed record Excluded : HopFault { public Excluded(string detail) : base(detail, FaultBand.Hop.Code(1)) { } }
    public sealed record Fenced : HopFault { public Fenced(string detail) : base(detail, FaultBand.Hop.Code(2)) { } }
    public sealed record OwnerConflict : HopFault { public OwnerConflict(string detail) : base(detail, FaultBand.Hop.Code(3)) { } }
    public sealed record StaleManifest : HopFault { public StaleManifest(string detail) : base(detail, FaultBand.Hop.Code(4)) { } }
    public sealed record ChecksumBreaking : HopFault { public ChecksumBreaking(string detail) : base(detail, FaultBand.Hop.Code(5)) { } }
    public sealed record SpawnRejected : HopFault { public SpawnRejected(string detail) : base(detail, FaultBand.Hop.Code(6)) { } }
}

[SmartEnum]
public sealed partial class HopIdempotency {
    public static readonly HopIdempotency Idempotent = new();
    public static readonly HopIdempotency MethodDerived = new();
    public static readonly HopIdempotency Keyed = new();
    public static readonly HopIdempotency SingleShot = new();
}

[SmartEnum]
public sealed partial class HopTransport {
    public static readonly HopTransport SocketsHttpHandler = new();
    public static readonly HopTransport GrpcChannel = new();
    public static readonly HopTransport ProcessSpawn = new();
}

[SmartEnum]
public sealed partial class HopRateLimit {
    public static readonly HopRateLimit Concurrency = new();
    public static readonly HopRateLimit SlidingWindow = new();
    public static readonly HopRateLimit TokenBucket = new();
}

// The delivery-honesty axis: policy DATA over the existing rows, zero new resilience surface.
// ExactlyOnceEffective = at-least-once transport + consumer dedupe by id=ContentKey (never magic).
[SmartEnum]
public sealed partial class HopDelivery {
    public static readonly HopDelivery BestEffort = new();
    public static readonly HopDelivery AtLeastOnce = new();
    public static readonly HopDelivery ExactlyOnceEffective = new();
}

public sealed record ReleaseIdentity(string Product, string Channel, string Installed, Uri Feed);

public sealed record HopPolicy(
    string PipelineKey,
    HopTransport Transport,
    DeadlineClass Attempt,
    DeadlineClass Total,
    Capability Needs,
    HopIdempotency Idempotency,
    HopRateLimit Admission,
    HopDelivery Delivery,
    Func<HostProfile, bool> ExcludedOn);

public static class HopRows {
    static readonly Func<HostProfile, bool> Never = static _ => false;
    static readonly Func<HostProfile, bool> PluginRows = static profile => profile == HostProfile.RhinoPlugin || profile == HostProfile.Gh2Plugin;

    public static readonly HopPolicy HttpApi = new(nameof(OutboundHop.HttpApi), HopTransport.SocketsHttpHandler, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.MethodDerived, HopRateLimit.Concurrency, HopDelivery.AtLeastOnce, Never);
    public static readonly HopPolicy Grpc = new(nameof(OutboundHop.Grpc), HopTransport.GrpcChannel, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Keyed, HopRateLimit.Concurrency, HopDelivery.ExactlyOnceEffective, Never);
    public static readonly HopPolicy ServerStream = new(nameof(OutboundHop.ServerStream), HopTransport.GrpcChannel, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Idempotent, HopRateLimit.Concurrency, HopDelivery.AtLeastOnce, Never);
    public static readonly HopPolicy CompanionSpawn = new(nameof(OutboundHop.CompanionSpawn), HopTransport.ProcessSpawn, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.LocalCompute, HopIdempotency.SingleShot, HopRateLimit.Concurrency, HopDelivery.BestEffort, Never);
    public static readonly HopPolicy LocalIpc = new(nameof(OutboundHop.LocalIpc), HopTransport.GrpcChannel, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.LocalCompute, HopIdempotency.Keyed, HopRateLimit.TokenBucket, HopDelivery.BestEffort, Never);
    public static readonly HopPolicy WebhookPost = new(nameof(OutboundHop.WebhookPost), HopTransport.SocketsHttpHandler, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Keyed, HopRateLimit.SlidingWindow, HopDelivery.AtLeastOnce, Never);
    public static readonly HopPolicy UpdateCheck = new(nameof(OutboundHop.UpdateCheck), HopTransport.SocketsHttpHandler, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Idempotent, HopRateLimit.Concurrency, HopDelivery.BestEffort, PluginRows);

    extension(OutboundHop hop) {
        public HopPolicy Policy => hop.Switch(
            httpApi: static _ => HttpApi,
            grpc: static _ => Grpc,
            serverStream: static _ => ServerStream,
            companionSpawn: static _ => CompanionSpawn,
            localIpc: static _ => LocalIpc,
            webhookPost: static _ => WebhookPost,
            updateCheck: static _ => UpdateCheck);
    }
}
```

## [03]-[HTTP_PIPELINES]

- Owner: `HttpLane` — one registration fold for the SocketsHttpHandler-borne rows (HttpApi, WebhookPost, UpdateCheck).
- Entry: `Wire(IServiceCollection services, IConfiguration configuration, OutboundHop hop, Uri authority)` — the idempotency row dispatch selects hedging versus standard and returns the graph.
- Auto: package-generated option validators prove the bound standard and hedging records at startup; `EnableReloads` keeps named policy live through the options monitor on the handler context route; `SetRequestMessage`/`GetRequestMessage` carry the per-request `RequestMetadata` route name into the Polly context so dependency-route metadata enriches the resilience meter without ambient state.
- Packages: Microsoft.Extensions.Http.Resilience, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one options row per pipeline key under the `Outbound` section root; a new HTTP-borne hop is one `Wire` call over its row; a multi-region target is one `:Routing` weighted-group subsection, never a second pipeline — zero new surface.
- Boundary: `AddStandardResilienceHandler` binds rate limiter, total timeout, retry, breaker, and attempt timeout as one options record from the `Outbound:{PipelineKey}` section; hedging admits only `Idempotent` rows, routing from the `:Routing` subsection — `ConfigureOrderedGroups` for a single-target row, `ConfigureWeightedGroups` over `WeightedGroupsRoutingOptions`/`WeightedUriEndpoint` with `WeightedGroupSelectionMode` as the row's routing policy value for a multi-region row; the standard transient surface is `HttpClientResiliencePredicates.IsTransientHttpFailure` and the hedging surface `HttpClientHedgingResiliencePredicates.IsTransient`, so a non-transient status never burns an attempt; `DisableForUnsafeHttpMethods` is the method-derived guard and `DisableFor(HttpMethod.Patch)` adds the explicit non-idempotent method that the unsafe-method set omits, `SelectPipelineByAuthority` keys pipeline identity by authority; one `IDownstreamDependencyMetadata` row (`DependencyName`, `UniqueHostNameSuffixes`, `ISet<RequestMetadata>`) declares the dependency route once through `HttpDiagnosticsServiceCollectionExtensions.AddDownstreamDependencyMetadata(IServiceCollection, IDownstreamDependencyMetadata)` so spans carry the route with no per-call literal, and the per-request route name rides a `RequestMetadata(MethodType, RequestRoute, RequestName)` value set through `IOutgoingRequestContext.SetRequestMetadata`; the named-client `AddHttpClient` arrives through the Microsoft.Extensions.Http transitive closure, never a direct pin; resilience context crosses through the request-message extensions, never ambient state.

```csharp signature
public static class HttpLane {
    public const string SectionRoot = "Outbound";

    public static IServiceCollection Wire(IServiceCollection services, IConfiguration configuration, OutboundHop hop, Uri authority) =>
        hop.Policy.Idempotency.Switch(
            state: (Services: services, Configuration: configuration, Row: hop.Policy, Authority: authority),
            idempotent: static lane => (Hedged(lane.Services, lane.Configuration, lane.Row, lane.Authority), lane.Services).Item2,
            methodDerived: static lane => (Standard(lane.Services, lane.Configuration, lane.Row, lane.Authority), lane.Services).Item2,
            keyed: static lane => (Standard(lane.Services, lane.Configuration, lane.Row, lane.Authority), lane.Services).Item2,
            singleShot: static lane => (Standard(lane.Services, lane.Configuration, lane.Row, lane.Authority), lane.Services).Item2);

    static IHttpStandardResiliencePipelineBuilder Standard(IServiceCollection services, IConfiguration configuration, HopPolicy row, Uri authority) =>
        services.AddHttpClient(row.PipelineKey, client => client.BaseAddress = authority)
            .AddStandardResilienceHandler(static options => { options.Retry.DisableForUnsafeHttpMethods(); options.Retry.DisableFor(HttpMethod.Patch); })
            .Configure(configuration.GetSection($"{SectionRoot}:{row.PipelineKey}"))
            .SelectPipelineByAuthority();

    static IStandardHedgingHandlerBuilder Hedged(IServiceCollection services, IConfiguration configuration, HopPolicy row, Uri authority) =>
        services.AddHttpClient(row.PipelineKey, client => client.BaseAddress = authority)
            .AddStandardHedgingHandler(route => Route(route, configuration.GetSection($"{SectionRoot}:{row.PipelineKey}:Routing")))
            .Configure(configuration.GetSection($"{SectionRoot}:{row.PipelineKey}"))
            .SelectPipelineByAuthority();

    static void Route(IRoutingStrategyBuilder route, IConfigurationSection routing) =>
        ignore(routing.GetSection("Weighted").Exists()
            ? route.ConfigureWeightedGroups(routing.GetSection("Weighted"))
            : route.ConfigureOrderedGroups(routing));
}
```

## [04]-[KEYED_PIPELINES]

- Owner: `KeyedLane` — the one keyed registry registration fold for every non-HTTP hop; the channel-policy record is the canonical `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` `GrpcChannelPolicy` consumed by reference — this page reads `GrpcChannelPolicy.Canonical` and never re-declares the record.
- Entry: `Register(IServiceCollection services, ILoggerFactory telemetry, Func<DeadlineClass, TimeSpan> allotted, Func<HopPolicy, Func<FallbackActionArguments<HopOutcome>, ValueTask<Outcome<HopOutcome>>>> redial, Action<OnRateLimiterRejectedArguments> onRejected, params ReadOnlySpan<HopPolicy> rows)` — one `AddResiliencePipeline` entry per row, the `redial` projection seats the per-row fallback action and `onRejected` fans every rate-limiter rejection onto the meter.
- Auto: `ConfigureTelemetry` inserts the telemetry strategy at pipeline head; resilience events land under the pipeline key in the package meter and logger; each pipeline binds a `CircuitBreakerStateProvider` so the breaker state reads from Polly's own observation surface, never a parallel state delegate; the pipeline-head limiter is `AutoReplenishment` true on every replenishing shape so the sliding-window and token-bucket rows self-refill without a parallel timer.
- Packages: Polly.Core, Polly.Extensions, Polly.RateLimiting, System.Threading.RateLimiting, LanguageExt.Core, BCL inbox
- Growth: one strategy row inside one keyed pipeline; one `HopRateLimit` key plus one `Limiter` switch arm absorbs a new admission shape; chaos rows attach on the test-host profile row as one `AddChaosLatency`, `AddChaosFault`, or `AddChaosOutcome` strategy row each — zero new surface.
- Boundary: gRPC-native retry is rejected — the channel `ServiceConfig` retry and hedging are experimental and a second retry owner; admission is one `RateLimiterStrategyOptions` row at the pipeline head whose `RateLimiter` lease-producer delegate is the `KeyedLane.Limiter` projection over the row's `HopRateLimit Admission` column — `Concurrency` keeps the `DefaultRateLimiterOptions` typed `System.Threading.RateLimiting.ConcurrencyLimiterOptions` path (the `int PermitLimit`/`int QueueLimit` settable members bound to `RateLimitPermits` 128, `RateLimitQueue` 64), `SlidingWindow` leases from a `SlidingWindowRateLimiter` over `SlidingWindowRateLimiterOptions{Window,SegmentsPerWindow,PermitLimit,QueueLimit,QueueProcessingOrder}` and `TokenBucket` from a `TokenBucketRateLimiter` over `TokenBucketRateLimiterOptions{ReplenishmentPeriod,TokensPerPeriod,TokenLimit,QueueLimit}`, the generic `AddRateLimiter<TBuilder>(TBuilder, RateLimiterStrategyOptions)` of `Polly.RateLimiterResiliencePipelineBuilderExtensions` taking the builder by its own type; its `OnRejected` delegate fans the rejection onto the resilience meter, and a `RateLimiterRejectedException.RetryAfter` projection feeds the retry `DelayGenerator` so a server-advised back-pressure window is honored verbatim while every other fault keeps the exponential-jitter seed; the `LocalIpc` redial is one `AddFallback` strategy whose `FallbackStrategyOptions<HopOutcome>.FallbackAction` re-reads the peer manifest and reconnects, a hop-keyed policy value, never a coded retry loop — `FallbackAction` is typed `Func<FallbackActionArguments<HopOutcome>, ValueTask<Outcome<HopOutcome>>>`, and the `readonly struct FallbackActionArguments<HopOutcome>` carrier exposes `ResilienceContext Context` and the inbound `Outcome<HopOutcome> Outcome` the redial action reads to reconnect the peer; `CircuitBreakerManualControl` keyed per hop is the kill-switch write surface and `CircuitBreakerStateProvider` keyed per hop is the read-side state the receipt projects; the `allotted` projection is the deadline-row read so no duration literal lives in a strategy; the canonical channel record carries keepalive 60s/30s, infinite pooled-connection idle, multiplexed HTTP/2, and 4 MiB caps in both directions.

```csharp signature
// GrpcChannelPolicy is OWNED by Rasm.Compute/Runtime/transport#TRANSPORT_AXIS (the canonical 9-field
// record: PooledConnectionIdle, KeepAlivePingDelay, KeepAlivePingTimeout, EnableMultipleHttp2Connections,
// MaxSendBytes, MaxReceiveBytes, InitialReconnectBackoff, MaxReconnectBackoff, HttpVersionPosture Version).
// This page consumes GrpcChannelPolicy.Canonical by reference and never re-declares the record — a local
// re-declaration (especially the stale 6-field subset that drops the reconnect-backoff and HTTP-version-posture
// fields) is the rejected second-owner form.

public static class KeyedLane {
    public const int PermitLimit = 64;
    public const int QueueLimit = 256;
    public const int RetryAttempts = 3;
    public const int RateLimitPermits = 128;
    public const int RateLimitQueue = 64;
    public const int WindowSegments = 6;
    public const int WindowPermits = 256;
    public const int TokensPerPeriod = 32;
    public const int TokenBucketCapacity = 128;
    public static readonly TimeSpan RetrySeed = TimeSpan.FromMilliseconds(200);
    public static readonly TimeSpan SlidingWindowSpan = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan TokenReplenishment = TimeSpan.FromSeconds(1);
    static readonly ConcurrentDictionary<string, (CircuitBreakerManualControl Control, CircuitBreakerStateProvider State)> Breakers = new(StringComparer.Ordinal);
    static (CircuitBreakerManualControl Control, CircuitBreakerStateProvider State) Of(string key) =>
        Breakers.GetOrAdd(key, static _ => (new CircuitBreakerManualControl(), new CircuitBreakerStateProvider()));
    public static CircuitBreakerManualControl BreakerOf(string pipelineKey) => Of(pipelineKey).Control;
    public static CircuitBreakerStateProvider StateOf(string pipelineKey) => Of(pipelineKey).State;

    static RateLimiterStrategyOptions Limiter(HopPolicy row, Action<OnRateLimiterRejectedArguments> onRejected) =>
        row.Admission.Switch<RateLimiter?>(
            concurrency: static () => null,
            slidingWindow: static () => new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions {
                Window = SlidingWindowSpan,
                SegmentsPerWindow = WindowSegments,
                AutoReplenishment = true,
                PermitLimit = WindowPermits,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = RateLimitQueue,
            }),
            tokenBucket: static () => new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions {
                ReplenishmentPeriod = TokenReplenishment,
                TokensPerPeriod = TokensPerPeriod,
                AutoReplenishment = true,
                TokenLimit = TokenBucketCapacity,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = RateLimitQueue,
            })) is { } limiter
            ? new RateLimiterStrategyOptions {
                RateLimiter = args => limiter.AcquireAsync(permitCount: 1, args.Context.CancellationToken),
                OnRejected = args => { onRejected(args); return default; },
            }
            : new RateLimiterStrategyOptions {
                DefaultRateLimiterOptions = new ConcurrencyLimiterOptions { PermitLimit = RateLimitPermits, QueueLimit = RateLimitQueue },
                OnRejected = args => { onRejected(args); return default; },
            };

    public static IServiceCollection Register(IServiceCollection services, ILoggerFactory telemetry, Func<DeadlineClass, TimeSpan> allotted, Func<HopPolicy, Func<FallbackActionArguments<HopOutcome>, ValueTask<Outcome<HopOutcome>>>> redial, Action<OnRateLimiterRejectedArguments> onRejected, params ReadOnlySpan<HopPolicy> rows) =>
        rows.ToArray().ToSeq().Fold(services, (graph, row) =>
            graph.AddResiliencePipeline<HopOutcome>(row.PipelineKey, builder => builder
                .ConfigureTelemetry(telemetry)
                .AddRateLimiter(Limiter(row, onRejected))
                .AddConcurrencyLimiter(permitLimit: PermitLimit, queueLimit: QueueLimit)
                .AddRetry(new RetryStrategyOptions<HopOutcome> {
                    MaxRetryAttempts = RetryAttempts,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = RetrySeed,
                    DelayGenerator = static args => new ValueTask<TimeSpan?>(
                        args.Outcome.Exception is RateLimiterRejectedException rejected && rejected.RetryAfter is { } after ? after : (TimeSpan?)null),
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HopOutcome> {
                    ManualControl = BreakerOf(row.PipelineKey),
                    StateProvider = StateOf(row.PipelineKey),
                })
                .AddFallback(new FallbackStrategyOptions<HopOutcome> {
                    ShouldHandle = new PredicateBuilder<HopOutcome>().Handle<ExecutionRejectedException>(),
                    FallbackAction = redial(row),
                })
                .AddTimeout(allotted(row.Attempt))));
}
```

Every keyed-pipeline literal is a named frozen `const` or `static readonly` on `KeyedLane` — `PermitLimit` 64, `QueueLimit` 256, `RetryAttempts` 3, `RateLimitPermits` 128, `RateLimitQueue` 64, `WindowSegments` 6, `WindowPermits` 256, `TokensPerPeriod` 32, `TokenBucketCapacity` 128, `RetrySeed` 200 ms, `SlidingWindowSpan` 60 s, `TokenReplenishment` 1 s — so each strategy value traces to its declaring field, never an inline literal; `permitCount` 1 on `AcquireAsync` is the single-call lease grain and `QueueProcessingOrder.OldestFirst` is the fairness fact on both replenishing shapes.

## [05]-[OWNERSHIP_LAW]

- Owner: `OutboundSurface` — admission, dispatch, conflict evidence, and enforcement over one runtime record; `OutboundRuntime` capability record; `HopOutcome` `[Union]`; `HopReceipt` receipt struct.
- Cases: Delivered, Refused, Faulted — Refused carries pre-flight admission faults, Faulted carries in-flight pipeline rejection.
- Entry: `Run(OutboundRuntime runtime, OutboundHop hop, Func<CancellationToken, Task<HopOutcome>> send)` — `IO<HopReceipt>` carries the boundary effect; every exit is a receipt, never a throw.
- Receipt: `HopReceipt` — pipeline key, attempts, outcome, elapsed `Duration`, consumed deadline class, breaker state.
- Packages: Polly.Core, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one outcome case per new terminal kind; zero new surface.
- Boundary: exactly one retry owner per hop held at the composition root — domain rails retry through `Schedule`, transport hops retry through the keyed or HTTP pipeline, never both on one seam; the `RetryOwners` claim cell is the structural proof and `Guarded` is the schedule-side guard that degrades to a single pass and emits the conflict receipt instead of stacking a loop; `Execute` runs the hop through `ExecuteOutcomeAsync` over a pooled `ResilienceContext` leased from `ResilienceContextPool.Shared` and returned in `finally`, so a pipeline rejection surfaces as a captured `Outcome<HopOutcome>.Exception` folded to `Faulted` with no exception-as-control-flow round-trip, and `RouteKey`, a `readonly struct ResiliencePropertyKey<string>` (single `string key` ctor, `string Key` accessor), carries the pipeline key into the meter through `context.Properties`, never ambient state; `Enforce` sweeps the keyed manual breakers from the effective degradation level — the kill-switch consequence; `BreakerState` reads the receipt's breaker field from the native `CircuitBreakerStateProvider` keyed per hop on `KeyedLane`, deleting the hand-held state delegate so the runtime record carries no parallel breaker-state function; the runtime record is constructed once at the composition root.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HopOutcome {
    private HopOutcome() { }

    public sealed record Delivered : HopOutcome;
    public sealed record Refused(Error Reason) : HopOutcome;
    public sealed record Faulted(Error Reason) : HopOutcome;
}

public readonly record struct HopReceipt(
    string PipelineKey,
    int Attempts,
    HopOutcome Outcome,
    Duration Elapsed,
    DeadlineClass Consumed,
    CircuitState Breaker);

public sealed record OutboundRuntime(
    ResiliencePipelineProvider<string> Pipelines,
    HostProfile Profile,
    ClockPolicy Clocks,
    Func<DegradationLevel> Level,
    Atom<HashMap<string, string>> RetryOwners) {
    public CircuitState BreakerState(string pipelineKey) => KeyedLane.StateOf(pipelineKey).CircuitState;
}

public static class OutboundSurface {
    public const string PipelineOwner = "transport-pipeline";
    public const string ScheduleOwner = "schedule";

    public static Fin<Unit> Claim(OutboundRuntime runtime, string pipelineKey, string owner) =>
        runtime.RetryOwners.Swap(owners => owners.TryAdd(pipelineKey, owner)).Find(pipelineKey)
            .Filter(holder => holder == owner)
            .Map(static _ => unit)
            .ToFin(new HopFault.OwnerConflict(pipelineKey));

    public static Fin<HopPolicy> Admit(OutboundRuntime runtime, OutboundHop hop) =>
        from row in Fin.Succ(hop.Policy)
        from _excluded in guardnot(row.ExcludedOn(runtime.Profile), new HopFault.Excluded(row.PipelineKey))
        from _fenced in guard(runtime.Level().Permits(row.Needs), new HopFault.Fenced(row.PipelineKey))
        from _owner in Claim(runtime, row.PipelineKey, PipelineOwner)
        select row;

    public static IO<HopReceipt> Run(OutboundRuntime runtime, OutboundHop hop, Func<CancellationToken, Task<HopOutcome>> send) =>
        Admit(runtime, hop).Match(
            Succ: row => Execute(runtime, row, send),
            Fail: error => IO.pure(Conflicted(runtime, hop.Policy, error)));

    public static IO<HopReceipt> Guarded(OutboundRuntime runtime, HopPolicy row, Schedule retry, IO<HopReceipt> work, Action<HopReceipt> evidence) =>
        Claim(runtime, row.PipelineKey, ScheduleOwner).Match(
            Succ: _ => work.Retry(retry),
            Fail: conflict => IO.lift(fun(() => evidence(Conflicted(runtime, row, conflict)))).Bind(_ => work));

    public static IO<Unit> Enforce(DegradationLevel effective, params ReadOnlySpan<HopPolicy> rows) =>
        rows.ToArray().ToSeq()
            .TraverseM(row => IO.liftAsync(async () => {
                await (effective.Permits(row.Needs)
                    ? KeyedLane.BreakerOf(row.PipelineKey).CloseAsync()
                    : KeyedLane.BreakerOf(row.PipelineKey).IsolateAsync());
                return unit;
            }))
            .As()
            .Map(static _ => unit);

    static readonly ResiliencePropertyKey<string> RouteKey = new("rasm.outbound.route");

    static IO<HopReceipt> Execute(OutboundRuntime runtime, HopPolicy row, Func<CancellationToken, Task<HopOutcome>> send) =>
        IO.liftAsync(async envIO => {
            var mark = runtime.Clocks.Mark();
            var attempts = 0;
            var context = ResilienceContextPool.Shared.Get(row.PipelineKey, envIO.Token);
            context.Properties.Set(RouteKey, row.PipelineKey);
            try {
                Outcome<HopOutcome> captured = await runtime.Pipelines.GetPipeline<HopOutcome>(row.PipelineKey)
                    .ExecuteOutcomeAsync(async (ctx, state) => { state.Bump(); return Outcome.FromResult(await state.Send(ctx.CancellationToken)); },
                        context, (Bump: new Action(() => attempts++), Send: send));
                return captured.Exception is ExecutionRejectedException rejected
                    ? Receipt(runtime, row, attempts, new HopOutcome.Faulted(Error.New(rejected)), mark)
                    : Receipt(runtime, row, attempts, captured.Result ?? new HopOutcome.Faulted(Error.New(captured.Exception!)), mark);
            }
            finally { ResilienceContextPool.Shared.Return(context); }
        });

    static HopReceipt Receipt(OutboundRuntime runtime, HopPolicy row, int attempts, HopOutcome outcome, long mark) =>
        new(row.PipelineKey, attempts, outcome, runtime.Clocks.Elapsed(mark), row.Attempt, runtime.BreakerState(row.PipelineKey));

    static HopReceipt Conflicted(OutboundRuntime runtime, HopPolicy row, Error reason) =>
        new(row.PipelineKey, Attempts: 0, new HopOutcome.Refused(reason), Duration.Zero, row.Total, runtime.BreakerState(row.PipelineKey));
}
```

## [06]-[DISCOVERY_ATTACH]

- Owner: `DiscoveryManifest` attach record; `CompanionChild` spawn capsule; `Discovery` static surface — path law, atomic publish, staleness probe, checksum gate, UDS connect, spawn, drain-fan producer.
- Entry: `Read(ProfileRoots roots, int pid, JsonTypeInfo<DiscoveryManifest> contract)` — `Fin` aborts on missing, empty, or dead-pid manifests.
- Packages: Grpc.Net.Client, LanguageExt.Core, NodaTime, BCL inbox
- Growth: the connect dispatch is the single Unix-domain-socket route; socket-file mode and the accept-side peer-credential read are the access boundary, never a transport-level ACL; zero new surface.
- Boundary: `Publish` and `Connect` are the named boundary capsules carrying statement bodies — atomic temp-write-then-move publication and the UDS connect callback; the socket lives at the temp-root `rasm-{pid}.sock` path under the 104-byte `sun_path` cap; the manifest directory is created 0700 and directory mode is the credential boundary; `Compatible` takes the additive-versus-breaking classifier as a delegate — checksum equality or additive drift admits, breaking drift is a typed rejection; the spawn is single-shot and the post-spawn manifest read rides the CompanionSpawn keyed pipeline registered at KEYED_PIPELINES; `DrainFan` is the parent-to-child drain-fan producer — it dials the peer over the `LocalIpc` hop case through `OutboundSurface.Run` with the drain control message, returning the `IO<Unit>` that `Spawn` threads into `CompanionChild.FanDrain` as the `drainFan` arg, so the drain conductor fans onto a real owner instead of a dangling delegate field; the accepted-socket peer-credential read moves to `Wire/companion#PEER_ADMISSION` (the serving side reads the connecting peer's uid and pid once at accept and never trusts the manifest) — a seam-split, not an owner here.

```csharp signature
public sealed record DiscoveryManifest(
    int Pid,
    string SocketPath,
    Instant StartInstant,
    string ContractChecksum,
    long StoreEpoch);

public sealed record CompanionChild(
    Process Child,
    DiscoveryManifest Manifest,
    Func<CancellationToken, IO<Unit>> FanDrain);

public static class Discovery {
    public const int SunPathMax = 104;
    public const UnixFileMode SocketDirMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

    public static string ManifestPath(ProfileRoots roots, int pid) =>
        Path.Join(roots.AppRoot, "discovery", $"rasm-{pid}.json");

    public static Fin<string> SocketPath(int pid) =>
        Path.Join(Path.GetTempPath(), $"rasm-{pid}.sock") is var path && Encoding.UTF8.GetByteCount(path) <= SunPathMax
            ? Fin.Succ(path)
            : Fin.Fail<string>(new HopFault.Text($"sun_path over {SunPathMax} bytes: {path}"));

    public static IO<DiscoveryManifest> Publish(DiscoveryManifest manifest, ProfileRoots roots, JsonTypeInfo<DiscoveryManifest> contract) =>
        IO.lift(() => {
            var path = ManifestPath(roots, manifest.Pid);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!, SocketDirMode);
            File.WriteAllBytes($"{path}.tmp", JsonSerializer.SerializeToUtf8Bytes(manifest, contract));
            File.Move($"{path}.tmp", path, overwrite: true);
            return manifest;
        });

    public static Fin<DiscoveryManifest> Read(ProfileRoots roots, int pid, JsonTypeInfo<DiscoveryManifest> contract) =>
        Try.lift(() => Optional(JsonSerializer.Deserialize(File.ReadAllBytes(ManifestPath(roots, pid)), contract)))
            .Run()
            .MapFail(static error => new HopFault.StaleManifest(error.Message))
            .Bind(manifest => manifest.ToFin(new HopFault.StaleManifest($"empty manifest: {pid}")))
            .Bind(static manifest => Alive(manifest));

    public static Fin<DiscoveryManifest> Compatible(DiscoveryManifest peer, string localChecksum, Func<string, string, bool> additiveOnly) =>
        peer.ContractChecksum == localChecksum || additiveOnly(localChecksum, peer.ContractChecksum)
            ? Fin.Succ(peer)
            : Fin.Fail<DiscoveryManifest>(new HopFault.ChecksumBreaking(peer.ContractChecksum));

    public static GrpcChannel Connect(DiscoveryManifest peer, GrpcChannelPolicy policy) =>
        GrpcChannel.ForAddress(new UriBuilder(Uri.UriSchemeHttp, "localhost").Uri, new GrpcChannelOptions {
            MaxSendMessageSize = policy.MaxSendBytes,
            MaxReceiveMessageSize = policy.MaxReceiveBytes,
            HttpHandler = new SocketsHttpHandler {
                PooledConnectionIdleTimeout = policy.PooledConnectionIdle,
                KeepAlivePingDelay = policy.KeepAlivePingDelay,
                KeepAlivePingTimeout = policy.KeepAlivePingTimeout,
                EnableMultipleHttp2Connections = policy.EnableMultipleHttp2Connections,
                ConnectCallback = async (_, cancel) => {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    try {
                        await socket.ConnectAsync(new UnixDomainSocketEndPoint(peer.SocketPath), cancel);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch {
                        socket.Dispose();
                        throw;
                    }
                },
            },
        });

    public static Fin<CompanionChild> Spawn(ProcessStartInfo spec, Func<int, Fin<DiscoveryManifest>> manifestOf, Func<DiscoveryManifest, CancellationToken, IO<Unit>> drainFan) =>
        Try.lift(() => Optional(Process.Start(spec)))
            .Run()
            .MapFail(static error => new HopFault.SpawnRejected(error.Message))
            .Bind(child => child.ToFin(new HopFault.SpawnRejected(spec.FileName)))
            .Bind(child => manifestOf(child.Id).Map(manifest =>
                new CompanionChild(child, manifest, cancel => drainFan(manifest, cancel))));

    public static Func<DiscoveryManifest, CancellationToken, IO<Unit>> FanOf(OutboundRuntime runtime) =>
        (peer, token) => DrainFan(peer, runtime, token);

    public static IO<Unit> DrainFan(DiscoveryManifest peer, OutboundRuntime runtime, CancellationToken token) =>
        OutboundSurface.Run(runtime, new OutboundHop.LocalIpc(peer), inner =>
            Drain(Connect(peer, GrpcChannelPolicy.Canonical), CancellationTokenSource.CreateLinkedTokenSource(token, inner).Token))
            .Map(static _ => unit);

    static async Task<HopOutcome> Drain(GrpcChannel channel, CancellationToken token) {
        await using (channel) {
            await channel.ConnectAsync(token);
            return new HopOutcome.Delivered();
        }
    }

    static Fin<DiscoveryManifest> Alive(DiscoveryManifest manifest) =>
        Try.lift(() => Process.GetProcessById(manifest.Pid))
            .Run()
            .MapFail(_ => new HopFault.StaleManifest($"pid absent: {manifest.Pid}"))
            .Map(_ => manifest);
}
```

## [07]-[DELIVERY_FANOUT]

- Owner: `DeliveryTarget` `[Union]` the channel-target carrier (endpoint `Uri` versus discovered peer manifest); `DeliveryChannel` `[SmartEnum<string>]` the outbound notification-channel axis under the hop key policy; `DeliveryMessage` the channel-agnostic notification payload; `DeliveryReceipt` the per-channel delivery evidence; `DeliveryFanout` the static multi-channel fan surface with the dedupe cell.
- Cases: 2 target cases — `Endpoint(Uri)` for the network-borne channels, `Peer(DiscoveryManifest)` for the in-app companion channel; 4 channel rows — push, webhook, email, in-app — each binding the `OutboundHop` its bytes ride through a target-discriminating `Hop(DeliveryTarget)` returning `Fin<OutboundHop>`: push and webhook on `WebhookPost` over an `Endpoint`, email on `HttpApi` over an `Endpoint` (the transactional-mail API), in-app on `LocalIpc` over a `Peer` manifest; a channel fed the wrong target shape returns `HopFault.Excluded` so the in-app channel can never forge a null manifest and a network channel can never dial a peer; delivery dispositions ride `HopOutcome`.
- Entry: `Fan(DeliveryRuntime runtime, DeliveryMessage message, params ReadOnlySpan<DeliveryChannel> channels)` returns `IO<Seq<DeliveryReceipt>>` — deduplicates the message by its idempotency key, then fans it to each channel through the channel's `OutboundHop` so one notification reaches every configured channel under one dedupe guard.
- Auto: every channel rides its `OutboundHop` so delivery inherits the hop's retry, breaker, rate-limit, and deadline — a flapping webhook endpoint breaks on the existing circuit breaker and a rate-capped push channel admits through the existing sliding-window limiter, never a per-channel retry loop; the dedupe fold is read-modify-write INSIDE `Swap(current => ...)` — the prune and the add fold over the LIVE cell value and the verdict derives from the swap itself (first sight within the window delivers, a re-fanned identical message folds every channel to a `DeliveryReceipt` carrying the deduped flag), so concurrent fans never lose each other's records — a stale-read `.Value` snapshot beside the swap is the deleted form; each channel's delivery mints one `DeliveryReceipt` carrying the channel, the `HopReceipt`, and the delivery disposition so a partial fan (push delivered, email faulted) records every channel's outcome independently; the fan runs the channels concurrently under one `CancelScope` so a slow channel does not block the others.
- Receipt: `DeliveryReceipt` — channel key, idempotency key, `HopOutcome`, deduped flag, attempt count, elapsed `Duration`, the advanced watermark (`Some` only on a fenced outbox-relay advance — the fan-out legs answer `None`), correlation id.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one channel row absorbs a new delivery medium — a new SMS or chat channel is one `DeliveryChannel` row binding its `OutboundHop` over the matching `DeliveryTarget` case, never a parallel sender; a new target shape is one `DeliveryTarget` case breaking every channel's `Hop` switch; a new delivery disposition rides the existing `HopOutcome`; zero new surface.
- Boundary: the delivery fan-out is the only multi-channel notification owner — a per-channel sender, a notification service wrapper, and a parallel delivery queue are the deleted forms, so all channels ride one fan and one dedupe; delivery never owns its own resilience — each channel composes its `OutboundHop` so the retry-owner, breaker, and rate-limit are the existing hop policy, and the delivery fan is purely the fan-and-dedupe layer above the hops; the dedupe is bounded — the idempotency-key cell evicts past its window so a long-lived process does not accumulate unbounded dedup state, mirroring the cache tag-cut bound; the fan is the scheduled-delivery consumer — a `ScheduleEntry` row fires the fan on its cadence so scheduled multi-channel delivery is one schedule row plus one fan call, never a second scheduler; the in-app channel rides the `LocalIpc` hop over a `DeliveryTarget.Peer` carrying the attached companion's `DiscoveryManifest` so an in-app notification reaches the companion over the control hop with a real peer manifest, never a `default!` placeholder and never a separate transport — the `Hop(DeliveryTarget)` switch is total and a target/channel shape mismatch is a typed `HopFault.Excluded`, never an unsound construction.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliveryTarget {
    private DeliveryTarget() { }

    public sealed record Endpoint(Uri Authority) : DeliveryTarget;
    public sealed record Peer(DiscoveryManifest Manifest) : DeliveryTarget;
}

[SmartEnum<string>]
public sealed partial class DeliveryChannel {
    public static readonly DeliveryChannel Push = new("push", static target => target.Switch(
        endpoint: static e => Fin<OutboundHop>.Succ(new OutboundHop.WebhookPost(e.Authority, Guid.CreateVersion7())),
        peer: static _ => Fin<OutboundHop>.Fail(new HopFault.Excluded("push:requires-endpoint"))));
    public static readonly DeliveryChannel Webhook = new("webhook", static target => target.Switch(
        endpoint: static e => Fin<OutboundHop>.Succ(new OutboundHop.WebhookPost(e.Authority, Guid.CreateVersion7())),
        peer: static _ => Fin<OutboundHop>.Fail(new HopFault.Excluded("webhook:requires-endpoint"))));
    public static readonly DeliveryChannel Email = new("email", static target => target.Switch(
        endpoint: static e => Fin<OutboundHop>.Succ(new OutboundHop.HttpApi(e.Authority)),
        peer: static _ => Fin<OutboundHop>.Fail(new HopFault.Excluded("email:requires-endpoint"))));
    public static readonly DeliveryChannel InApp = new("in-app", static target => target.Switch(
        endpoint: static _ => Fin<OutboundHop>.Fail(new HopFault.Excluded("in-app:requires-peer")),
        peer: static p => Fin<OutboundHop>.Succ(new OutboundHop.LocalIpc(p.Manifest))));

    [UseDelegateFromConstructor]
    public partial Fin<OutboundHop> Hop(DeliveryTarget target);
}

public sealed record DeliveryMessage(
    string IdempotencyKey,
    string Subject,
    JsonElement Body,
    DataClassification Classification,
    HashMap<DeliveryChannel, DeliveryTarget> Targets);

public readonly record struct DeliveryReceipt(
    string Channel,
    string IdempotencyKey,
    HopOutcome Outcome,
    bool Deduped,
    int Attempts,
    Duration Elapsed,
    Option<ulong> Watermark,
    CorrelationId Correlation);

public sealed record DeliveryRuntime(
    OutboundRuntime Outbound,
    Func<OutboundHop, DeliveryMessage, Func<CancellationToken, Task<HopOutcome>>> Send,
    Atom<HashMap<string, Instant>> Dedupe,
    Duration DedupeWindow,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    JsonSerializerOptions Wire);

public static class DeliveryFanout {
    public static IO<Seq<DeliveryReceipt>> Fan(DeliveryRuntime runtime, DeliveryMessage message, params ReadOnlySpan<DeliveryChannel> channels) =>
        IO.lift(() => runtime.Clocks.Now).Bind(now => {
            var correlation = Correlation.Mint();
            // Read-modify-write INSIDE the swap: prune and add fold over the LIVE cell value and the
            // dedupe verdict derives from the swap result — a Swap closing over a stale .Value read
            // loses concurrent fans' records and is the deleted form.
            var deduped = false;
            ignore(runtime.Dedupe.Swap(current => {
                var pruned = current.Filter(stamp => now - stamp < runtime.DedupeWindow);
                deduped = pruned.ContainsKey(message.IdempotencyKey);
                return pruned.AddOrUpdate(message.IdempotencyKey, now, now);
            }));
            return channels.ToArray().ToSeq()
                .TraverseM(channel => deduped
                    ? IO.pure(new DeliveryReceipt(channel.Key, message.IdempotencyKey, new HopOutcome.Delivered(), Deduped: true, 0, Duration.Zero, None, correlation))
                    : Deliver(runtime, channel, message, correlation))
                .As();
        });

    static IO<DeliveryReceipt> Deliver(DeliveryRuntime runtime, DeliveryChannel channel, DeliveryMessage message, CorrelationId correlation) =>
        (from target in message.Targets.Find(channel).ToFin(new HopFault.Text($"no-target:{channel.Key}"))
         from hop in channel.Hop(target)
         select (Target: target, Hop: hop)).Match(
            Succ: bound => OutboundSurface.Run(runtime.Outbound, bound.Hop, runtime.Send(bound.Hop, message))
                .Map(receipt => new DeliveryReceipt(channel.Key, message.IdempotencyKey, receipt.Outcome, Deduped: false, receipt.Attempts, receipt.Elapsed, None, correlation)),
            Fail: error => IO.pure(new DeliveryReceipt(channel.Key, message.IdempotencyKey, new HopOutcome.Refused(error), Deduped: false, 0, Duration.Zero, None, correlation)));
}
```

## [08]-[RESEARCH]

The dial-out boundary carries no open research. The accepted-socket peer-credential projection moves to the serving owner at `Wire/companion#PEER_ADMISSION`, where the P/Invoke `getsockopt` route and the `ucred`/`xucred` blittable layout seat the admission fence. The transactional-mail channel target API resolves at app-root creation behind the same app-root pin the OTLP exporter rides; the channel rows bind the `OutboundHop` only, never the provider client, so a mail provider is one channel target Uri, never a delivery-page client.
