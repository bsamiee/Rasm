# [APPHOST_OUTBOUND_RESILIENCE]

Outbound boundary ownership for the runtime spine: nine `OutboundHop` cases bind to frozen `HopPolicy` rows, each row deriving its whole strategy posture from one `HopAllotment`, and every hop holds exactly one retry surface — the standard or hedging HTTP handler on `SocketsHttpHandler`-borne rows, one keyed result-typed Polly pipeline per non-HTTP row. Admission folds the degradation gate, the modality exclusion, and the retry-owner claim onto one `Fin` rail, every dispatch exits as a `HopReceipt`, and the `LocalIpc` peer attaches through the discovery manifest law.

This page owns the hop axis, both pipeline registries, the ownership law over the Polly, Http.Resilience, and Grpc.Net.Client spine, the transport chaos posture `Runtime/determinism#ADVERSARIAL_PROBE` records as `ChaosDecision` entries, and the discovery manifest, UDS attach, checksum gate, and companion-spawn lifecycle seating the `LocalIpc(DiscoveryManifest Peer)` hop case. Its boundary is the process seam rather than the dial — two store-rail cases hand their bytes to a provider SDK or a cluster driver, and cross exactly the seam the dialed cases cross.

## [01]-[INDEX]

- [02]-[HOP_AXIS]: Nine hop cases bound to frozen policy rows deriving one allotment, with total dispatch.
- [03]-[HTTP_PIPELINES]: Standard and hedging handlers for `SocketsHttpHandler`-borne rows.
- [04]-[KEYED_PIPELINES]: One keyed result-typed Polly registry, its strategy roster, and channel policy for non-HTTP hops.
- [05]-[OWNERSHIP_LAW]: One retry owner per hop claimed at boot, with the outcome fold, conflict evidence, and receipts.
- [06]-[DISCOVERY_ATTACH]: Manifest law, UDS attach, checksum gate, and companion child lifecycle.
- [07]-[DELIVERY_FANOUT]: Multi-channel notification fan-out, delivery receipts, and dedupe.
- [08]-[TS_PROJECTION]: Hop and delivery wire-evidence shapes the dashboard and the instrument fan consume.

## [02]-[HOP_AXIS]

- Owner: `OutboundHop` `[Union]` nine sealed hop cases; `HopPolicy` per-case row record; `HopAllotment` the derivation every strategy knob reads; `HopRows` frozen row set with the total dispatches; `HopIdempotency` keyless vocabulary; `HopTransport` keyless byte-mover vocabulary; `HopRateLimit` keyless admission-shape vocabulary; `HopDelivery` keyless delivery-honesty vocabulary — every row STATES its guarantee, never assumes one; `HopFault` `[Union]` fault family deriving its codes through `FaultBand.Hop`; `ReleaseIdentity` vehicle-free update identity.
- Cases: HttpApi, Grpc, ServerStream, CompanionSpawn, LocalIpc, WebhookPost, UpdateCheck, ObjectStore, WideColumn — the stream case is gRPC server-stream; UpdateCheck carries `ReleaseIdentity` and is structurally excluded where the host owns the process; the two store-rail cases name their object by its RENDERED key rather than a typed address, because `ContentAddress` seats at the element seam this spine does not reference.
- Entry: `HopPolicy Policy`, `Fin<Uri> Authority`, and `string IdempotencyKey` — extension properties; total state-free `Switch` from case to frozen row, dial target, and dedup identity.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one case plus one `HopPolicy` row absorbs a new outbound boundary; the update vehicle lands as one `UpdatePort` row on the UpdateCheck case; the admission shape lands as one `HopRateLimit` key plus one `Admission` column value, never a second limiter rail; a new resilience law lands as one `HopPolicy` column read by one `HopStrategy` arm; zero new surface.
- Boundary: every strategy knob DERIVES on `HopAllotment` from the row's two deadline classes and its `Trip`/`Floor` pair, so a numeric literal inside `AddRetry`, `AddCircuitBreaker`, or an admission row reconstructs what the column already carries and is the deleted form; a store whose callee owns transactional semantics is excluded from the hop law and retries on its own execution strategy — `[LAYER_SPLIT]` row `[01]` — while a remote object-store or wide-column call crosses a process seam and lands here on row `[02]`, so the two store-rail rows are the seam-crossing half of one split rather than a second store policy; the `Transport` column routes the row to its lane through `Piped`, so `HttpLane.Wire`, `KeyedLane.Register`, `KeyedLane.Proven`, and `OutboundSurface.Enforce` read ONE residence predicate and `StoreRail` joins the keyed lane through that same predicate with no arm added; the `Admission` column names the pipeline-head limiter shape on every row — `Concurrency` for the bounded-permit default, `SlidingWindow` for the webhook segment cap, `TokenBucket` for the redial-paced peer hop — read by `HopStrategy.Admission`, never a second limiter selector; `Idempotency` and `Replayable` are the two admission columns hedging demands together, so semantic repeat-safety alone admits retry and never concurrent duplication — `ObjectStore` is the sharpest instance: `Idempotent` earns it the retry, `Replayable: false` withholds the hedge, and a hedged object PUT uploads the body twice; the two store rows price against the classes they match rather than a fresh curve — a bulk, transient-tolerant object transfer at moderate concurrency takes the `WebhookPost` pair (`Trip: 0.3d`, `Floor: 32`), and a keyed, interactive wide-column read or upsert takes the `HttpApi` pair (`Trip: 0.2d`, `Floor: 64`); `Retries` folds `SingleShot` to a pipeline with no retry strategy at all, because `MaxRetryAttempts` refuses a zero and a spawn re-offered under a schedule forks a second child; the `Delivery` column is the honesty axis every row STATES — `BestEffort` (local-ipc, spawn, update-check: a lost frame is acceptable evidence loss), `AtLeastOnce` (webhook, http, server-stream: the retry schedule redelivers so consumers dedupe by delivery key), `ExactlyOnceEffective` (the wire-native gRPC hop the outbox drain rides: at-least-once transport + consumer dedupe by `id`=`ContentKey` per the Persistence CloudEvents law, the Persistence egress pump composing this exact column) — a hop whose guarantee is unstated and a claim stronger than the transport provides are both deleted forms; the row's `Needs` capability is the degradation gate, the breaker group, and the `ExcludedOn` reads one `ConsumptionProfile` axis value, never a product identity; `IdempotencyKey` mints ABOVE the pipeline on the hop case itself — the webhook `DeliveryKey`, the gRPC `ContentKey`, the peer's pid-and-start pair — because a key minted inside the retried callback changes per attempt and defeats itself.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutboundHop {
    private OutboundHop() { }

    public sealed record HttpApi(Uri Authority) : OutboundHop;
    public sealed record Grpc(Uri Address, string ContentKey) : OutboundHop;
    public sealed record ServerStream(Uri Address) : OutboundHop;
    public sealed record CompanionSpawn(ProcessStartInfo Spec) : OutboundHop;
    public sealed record LocalIpc(DiscoveryManifest Peer) : OutboundHop;
    public sealed record WebhookPost(Uri Target, string DeliveryKey) : OutboundHop;
    public sealed record UpdateCheck(ReleaseIdentity Installed) : OutboundHop;
    // Store-rail objects cross as RENDERED keys: `ContentAddress` and every Persistence element type seat below
    // this spine's one reference, so identity crossing here is a provider name beside a key string, and bucket,
    // region, and endpoint resolve inside the driver's own configured client.
    public sealed record ObjectStore(string Provider, string ObjectKey) : OutboundHop;
    public sealed record WideColumn(string Keyspace, string PartitionKey) : OutboundHop;
}

[Union]
public abstract partial record HopFault : Expected, IValidationError<HopFault> {
    private HopFault(string detail, int code) : base(detail, code, None) { }

    public static HopFault Create(string message) => new Text(message);

    public sealed record Text : HopFault { public Text(string detail) : base(detail, FaultBand.Hop.Code(0)) { } }
    public sealed record Excluded : HopFault { public Excluded(string detail) : base(detail, FaultBand.Hop.Code(1)) { } }
    public sealed record Fenced : HopFault { public Fenced(string detail) : base(detail, FaultBand.Hop.Code(2)) { } }
    // Conflict evidence carries BOTH declaration symbols and the discarded policy, because the losing claim
    // degrades to the incumbent and this arm is the only place that loser survives for repair comparison; a
    // key-only detail names the contested hop while erasing which two owners contested it.
    public sealed record OwnerConflict : HopFault {
        public OwnerConflict(string pipelineKey, string incumbent, string loser, string discarded)
            : base($"<owner-conflict:{pipelineKey}>", FaultBand.Hop.Code(3)) =>
            (PipelineKey, Incumbent, Loser, Discarded) = (pipelineKey, incumbent, loser, discarded);
        public string PipelineKey { get; }
        public string Incumbent { get; }
        public string Loser { get; }
        public string Discarded { get; }
    }
    public sealed record StaleManifest : HopFault { public StaleManifest(string detail) : base(detail, FaultBand.Hop.Code(4)) { } }
    public sealed record ChecksumBreaking : HopFault { public ChecksumBreaking(string detail) : base(detail, FaultBand.Hop.Code(5)) { } }
    public sealed record SpawnRejected : HopFault { public SpawnRejected(string detail) : base(detail, FaultBand.Hop.Code(6)) { } }

    // Rejection arms close the pipeline's own termination verbs, each binding its evidence as a typed field so
    // escalation matches the case and reads the field rather than re-parsing the wire detail the base carries.
    // `Foreign` passes the open tail through and `Empty` marks the no-exception-no-result sentinel, so this
    // family drops no fault and no caller re-opens the vocabulary the seam retired.
    public sealed record CallerLeft : HopFault { public CallerLeft() : base("<caller-left>", FaultBand.Hop.Code(7)) { } }

    public sealed record Deadline : HopFault {
        public Deadline(TimeSpan span) : base($"<deadline:{span}>", FaultBand.Hop.Code(8)) => Span = Duration.FromTimeSpan(span);
        public Duration Span { get; }
    }

    public sealed record ForcedDark : HopFault {
        public ForcedDark(string? pipeline) : base($"<forced-dark:{pipeline}>", FaultBand.Hop.Code(9)) => Pipeline = Optional(pipeline);
        public Option<string> Pipeline { get; }
    }

    public sealed record Open : HopFault {
        public Open(TimeSpan? retryAfter) : base($"<open:{retryAfter}>", FaultBand.Hop.Code(10)) =>
            RetryAfter = Optional(retryAfter).Map(Duration.FromTimeSpan);
        public Option<Duration> RetryAfter { get; }
    }

    public sealed record Shed : HopFault {
        public Shed(TimeSpan? retryAfter) : base($"<shed:{retryAfter}>", FaultBand.Hop.Code(11)) =>
            RetryAfter = Optional(retryAfter).Map(Duration.FromTimeSpan);
        public Option<Duration> RetryAfter { get; }
    }

    public sealed record Foreign : HopFault {
        public Foreign(Error cause) : base(cause.Message, FaultBand.Hop.Code(12)) => Cause = cause;
        public Error Cause { get; }
    }

    public sealed record Empty : HopFault { public Empty() : base("<empty-outcome>", FaultBand.Hop.Code(13)) { } }
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
    public static readonly HopTransport StoreRail = new();
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
    string Owner,
    HopTransport Transport,
    DeadlineClass Attempt,
    DeadlineClass Total,
    Capability Needs,
    HopIdempotency Idempotency,
    bool Replayable,
    bool Redials,
    HopRateLimit Admission,
    HopDelivery Delivery,
    double Trip,
    int Floor,
    Func<ConsumptionProfile, bool> ExcludedOn);

// Every strategy knob projects HERE off the row's two deadline classes and its trip-and-floor pair, so a
// posture edit is one column and an incoherent knob pair is unconstructible. Each projection clamps against
// its own validator range — `MaxRetryAttempts` refuses below one, `MinimumThroughput` below two,
// `SamplingDuration` and `BreakDuration` below 500 ms — because an eager `Add*` validation turns an
// out-of-range derivation into an unconstructible pipeline rather than a first-request fault.
public readonly record struct HopAllotment(TimeSpan Total, TimeSpan Attempt, double Trip, int Floor) {
    public const int ThroughputFloor = 2;
    public const int HedgeCeiling = 2;
    public const int WindowSegments = 6;
    public const int BackoffDivisor = 20;

    public int Attempts => int.Max((int)(Total / Attempt) - 1, 1);
    public TimeSpan Sampling => 2 * Attempt;
    public TimeSpan Break => Sampling / 2;
    public TimeSpan Backoff => Attempt / BackoffDivisor;
    public TimeSpan HedgeDelay => Attempt / 4;
    public int Permits => Floor * 2;
    public int Throughput => int.Max(Floor / 2, ThroughputFloor);
    public int Hedges => int.Min(Attempts, HedgeCeiling);
}

public static class HopRows {
    static readonly Func<ConsumptionProfile, bool> Never = static _ => false;
    // Foreign attach means the host owns the process and its binaries, so this package can neither
    // replace nor relaunch itself; dispatch reads that axis value, never which product supplied it.
    static readonly Func<ConsumptionProfile, bool> HostOwnedProcess = static profile => profile.Attach == HostAttach.Foreign;

    public static readonly HopPolicy HttpApi = new(nameof(OutboundHop.HttpApi), nameof(HttpApi), HopTransport.SocketsHttpHandler, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.MethodDerived, Replayable: false, Redials: false, HopRateLimit.Concurrency, HopDelivery.AtLeastOnce, Trip: 0.2d, Floor: 64, Never);
    public static readonly HopPolicy Grpc = new(nameof(OutboundHop.Grpc), nameof(Grpc), HopTransport.GrpcChannel, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Keyed, Replayable: false, Redials: false, HopRateLimit.Concurrency, HopDelivery.ExactlyOnceEffective, Trip: 0.2d, Floor: 64, Never);
    public static readonly HopPolicy ServerStream = new(nameof(OutboundHop.ServerStream), nameof(ServerStream), HopTransport.GrpcChannel, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Idempotent, Replayable: false, Redials: false, HopRateLimit.Concurrency, HopDelivery.AtLeastOnce, Trip: 0.5d, Floor: 16, Never);
    public static readonly HopPolicy CompanionSpawn = new(nameof(OutboundHop.CompanionSpawn), nameof(CompanionSpawn), HopTransport.ProcessSpawn, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.LocalCompute, HopIdempotency.SingleShot, Replayable: false, Redials: false, HopRateLimit.Concurrency, HopDelivery.BestEffort, Trip: 0.5d, Floor: 4, Never);
    public static readonly HopPolicy LocalIpc = new(nameof(OutboundHop.LocalIpc), nameof(LocalIpc), HopTransport.GrpcChannel, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.LocalCompute, HopIdempotency.Keyed, Replayable: false, Redials: true, HopRateLimit.TokenBucket, HopDelivery.BestEffort, Trip: 0.5d, Floor: 8, Never);
    public static readonly HopPolicy WebhookPost = new(nameof(OutboundHop.WebhookPost), nameof(WebhookPost), HopTransport.SocketsHttpHandler, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Keyed, Replayable: false, Redials: false, HopRateLimit.SlidingWindow, HopDelivery.AtLeastOnce, Trip: 0.3d, Floor: 32, Never);
    public static readonly HopPolicy UpdateCheck = new(nameof(OutboundHop.UpdateCheck), nameof(UpdateCheck), HopTransport.SocketsHttpHandler, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Idempotent, Replayable: true, Redials: false, HopRateLimit.Concurrency, HopDelivery.BestEffort, Trip: 0.5d, Floor: 4, HostOwnedProcess);
    public static readonly HopPolicy ObjectStore = new(nameof(OutboundHop.ObjectStore), nameof(ObjectStore), HopTransport.StoreRail, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Idempotent, Replayable: false, Redials: false, HopRateLimit.Concurrency, HopDelivery.ExactlyOnceEffective, Trip: 0.3d, Floor: 32, Never);
    public static readonly HopPolicy WideColumn = new(nameof(OutboundHop.WideColumn), nameof(WideColumn), HopTransport.StoreRail, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Keyed, Replayable: false, Redials: false, HopRateLimit.Concurrency, HopDelivery.AtLeastOnce, Trip: 0.2d, Floor: 64, Never);

    extension(HopPolicy row) {
        public HopAllotment Allot(Func<DeadlineClass, TimeSpan> allotted) =>
            new(allotted(row.Total), allotted(row.Attempt), row.Trip, row.Floor);

        // Residence, retry admission, and hedging admission each read ONE predicate, so the four registration
        // and enforcement seats cannot disagree about which lane a row lives on or which strategies it earns.
        public bool Piped => row.Transport != HopTransport.SocketsHttpHandler;

        public bool Retries => row.Idempotency.Switch(
            idempotent: static () => true,
            methodDerived: static () => true,
            keyed: static () => true,
            singleShot: static () => false);

        public bool Hedges => row.Idempotency == HopIdempotency.Idempotent && row.Replayable;
    }

    extension(OutboundHop hop) {
        public HopPolicy Policy => hop.Switch(
            httpApi: static _ => HttpApi,
            grpc: static _ => Grpc,
            serverStream: static _ => ServerStream,
            companionSpawn: static _ => CompanionSpawn,
            localIpc: static _ => LocalIpc,
            webhookPost: static _ => WebhookPost,
            updateCheck: static _ => UpdateCheck,
            objectStore: static _ => ObjectStore,
            wideColumn: static _ => WideColumn);

        // Dial target reads off the CASE, never a caller-supplied argument beside it naming a host that case
        // contradicts. Cases whose bytes move under someone else's resolver rail typed — a spawn has no
        // authority at all, and a provider SDK or cluster driver resolves endpoint, region, and contact points
        // off its own configured client — so an HTTP registration cannot be handed one.
        public Fin<Uri> Authority => hop.Switch(
            httpApi: static hit => Fin.Succ(hit.Authority),
            grpc: static hit => Fin.Succ(hit.Address),
            serverStream: static hit => Fin.Succ(hit.Address),
            companionSpawn: static _ => Fin.Fail<Uri>(new HopFault.Excluded(nameof(OutboundHop.CompanionSpawn))),
            localIpc: static _ => Fin.Fail<Uri>(new HopFault.Excluded(nameof(OutboundHop.LocalIpc))),
            webhookPost: static hit => Fin.Succ(hit.Target),
            updateCheck: static hit => Fin.Succ(hit.Installed.Feed),
            objectStore: static _ => Fin.Fail<Uri>(new HopFault.Excluded(nameof(OutboundHop.ObjectStore))),
            wideColumn: static _ => Fin.Fail<Uri>(new HopFault.Excluded(nameof(OutboundHop.WideColumn))));

        // Dedup identity becomes the lease `OperationKey`, so it reaches every attempt and lands as
        // `operation.key` on every resilience event — key, attempts, and evidence correlate with no join.
        // Keyless rows answer their pipeline key, which is the same value a keyless lease would carry.
        public string IdempotencyKey => hop.Switch(
            httpApi: static _ => nameof(OutboundHop.HttpApi),
            grpc: static hit => hit.ContentKey,
            serverStream: static _ => nameof(OutboundHop.ServerStream),
            companionSpawn: static _ => nameof(OutboundHop.CompanionSpawn),
            localIpc: static hit => $"{hit.Peer.Pid}:{hit.Peer.StartInstant}",
            webhookPost: static hit => hit.DeliveryKey,
            updateCheck: static _ => nameof(OutboundHop.UpdateCheck),
            objectStore: static hit => hit.ObjectKey,
            wideColumn: static hit => hit.PartitionKey);
    }
}
```

## [03]-[HTTP_PIPELINES]

- Owner: `HttpLane` — one registration fold for the `SocketsHttpHandler`-borne rows (HttpApi, WebhookPost, UpdateCheck).
- Entry: `Wire(IServiceCollection services, IConfiguration configuration, OutboundHop hop, Func<DeadlineClass, TimeSpan> allotted, params ReadOnlySpan<WeightedUriEndpoint> routes)` returns `Fin<IServiceCollection>` — rails on the hop's own `Authority`, then the two admission columns select hedging over standard.
- Auto: `AddStandardResilienceHandler` and `AddStandardHedgingHandler` each call `EnableReloads` on their own options name inside the registration, so a section edit re-materializes the pipeline with in-flight executions finishing under the old generation and no reload member on this fence; package-generated validators prove attempt ≤ total, sampling ≥ 2× attempt, and the cumulative hedging plan at startup with the misconfigured client named; the handler sets `HttpClient.Timeout` infinite so the pipeline owns the seam's only deadline; registering any resilience handler installs the HTTP metrics enricher stamping `error.type` with the status code.
- Packages: Microsoft.Extensions.Http.Resilience, Microsoft.Extensions.Http.Diagnostics, Microsoft.Extensions.Telemetry.Abstractions, LanguageExt.Core, BCL inbox
- Growth: one options row per pipeline key under the `Outbound` section root; a new HTTP-borne hop is one `Wire` call over its row; a multi-region target is one weighted-endpoint span, never a second pipeline — zero new surface.
- Boundary: `AddStandardResilienceHandler` binds rate limiter, total timeout, retry, breaker, and attempt timeout as one options record, and every slot the row decides is set FROM the row's `HopAllotment` before the `Outbound:{PipelineKey}` section binds over it, so an operator edits a posture the derivation already made coherent; hedging admits only rows holding BOTH `Idempotent` and `Replayable` — semantic repeat-safety alone admits retry, since a hedged attempt replays the body concurrently and the request snapshot refuses a stream body at construction; routes ride the call as `WeightedUriEndpoint` values with `WeightedGroupSelectionMode.EveryAttempt` above one group and `InitialAttempt` at one, so load-spread against primary-with-failover is a route count, never a config toggle; the standard handler owns the closed transient set (408, 429, status ≥ 500, `HttpRequestException`, `TimeoutRejectedException`), so this lane declares no predicate of its own and `HttpClientResiliencePredicates.IsTransient` stays the reuse point a custom pipeline takes; `DisableForUnsafeHttpMethods` is the method-derived guard and already covers DELETE, POST, PUT, CONNECT, and PATCH, so a `DisableFor(HttpMethod.Patch)` beside it decorates a filter the set holds — its declaring `HttpRetryStrategyOptionsExtensions` carries `EXTEXP0001`, so the guard rides one centrally pinned acknowledgment and never a call-site pragma; `SelectPipelineByAuthority` mints one breaker, limiter, and deadline state per scheme-host-port from ONE declaration, so a flapping webhook target darkens only itself while `pipeline.instance` cardinality tracks the live target set, and the authority provider demands an absolute request URI so a webhook send crosses absolute; a custom `DelayGenerator` on the retry slot silently replaces the `Retry-After` header generator, so the header parse composes INSIDE any generator this lane later takes; `Configure(IConfigurationSection)` binds with unknown-key errors and refuses an empty section as a wiring defect, so the bind rides an existence gate; the hedging family binds under the client name with no suffix and carries a different options shape, so a posture flip rewrites the section rather than moving it; one `IDownstreamDependencyMetadata` row (`DependencyName`, `UniqueHostNameSuffixes`, `RequestMetadata` route set) declares each dependency route once through `HttpDiagnosticsServiceCollectionExtensions.AddDownstreamDependencyMetadata`, so spans carry the route with no per-call literal and the ambient `IOutgoingRequestContext.SetRequestMetadata` slot stays unused — a hop's route is its row, never per-call variance; the named-client `AddHttpClient` arrives through the Microsoft.Extensions.Http transitive closure, never a direct pin; a finite `client.Timeout` beside the `BaseAddress` assignment is the deleted form that re-mints a second untyped deadline surfacing as bare cancellation.

```csharp signature
public static class HttpLane {
    public const string SectionRoot = "Outbound";

    // Authority rails FIRST, so a process-borne row handed to this lane refuses by type rather than
    // registering a client whose base address nothing can fill.
    public static Fin<IServiceCollection> Wire(
        IServiceCollection services, IConfiguration configuration, OutboundHop hop,
        Func<DeadlineClass, TimeSpan> allotted, params ReadOnlySpan<WeightedUriEndpoint> routes) =>
        hop.Authority.Map(authority => Bind(
            services, configuration, hop.Policy, hop.Policy.Allot(allotted), authority, toSeq(routes.ToArray())));

    static IServiceCollection Bind(
        IServiceCollection services, IConfiguration configuration, HopPolicy row, HopAllotment allot,
        Uri authority, Seq<WeightedUriEndpoint> routes) =>
        row.Hedges
            ? (Hedged(services, configuration, row, allot, authority, routes), services).Item2
            : (Standard(services, configuration, row, allot, authority), services).Item2;

    // Slot edits land BEFORE the section bind, so config overrides a coherent derivation rather than the
    // package defaults; the order reverses and an operator's partial section reverts every unnamed slot.
    static IHttpStandardResiliencePipelineBuilder Standard(
        IServiceCollection services, IConfiguration configuration, HopPolicy row, HopAllotment allot, Uri authority) =>
        Sectioned(
            services.AddHttpClient(row.PipelineKey, client => client.BaseAddress = authority)
                .AddStandardResilienceHandler(options => {
                    options.TotalRequestTimeout.Timeout = allot.Total;
                    options.AttemptTimeout.Timeout = allot.Attempt;
                    options.Retry.MaxRetryAttempts = allot.Attempts;
                    options.Retry.Delay = allot.Backoff;
                    options.Retry.MaxDelay = allot.Attempt;
                    options.CircuitBreaker.FailureRatio = allot.Trip;
                    options.CircuitBreaker.MinimumThroughput = allot.Throughput;
                    options.CircuitBreaker.SamplingDuration = allot.Sampling;
                    options.CircuitBreaker.BreakDuration = allot.Break;
                    options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = allot.Permits;
                    options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = allot.Floor;
                    options.Retry.DisableForUnsafeHttpMethods();
                }),
            configuration, row)
            .SelectPipelineByAuthority();

    static IStandardHedgingHandlerBuilder Hedged(
        IServiceCollection services, IConfiguration configuration, HopPolicy row, HopAllotment allot,
        Uri authority, Seq<WeightedUriEndpoint> routes) =>
        Sectioned(
            services.AddHttpClient(row.PipelineKey, client => client.BaseAddress = authority)
                .AddStandardHedgingHandler(route => Route(route, routes))
                .Configure(options => {
                    options.TotalRequestTimeout.Timeout = allot.Total;
                    options.Hedging.MaxHedgedAttempts = allot.Hedges;
                    options.Hedging.Delay = allot.HedgeDelay;
                    options.Endpoint.Timeout.Timeout = allot.Attempt;
                    options.Endpoint.CircuitBreaker.FailureRatio = allot.Trip;
                    options.Endpoint.CircuitBreaker.MinimumThroughput = allot.Throughput;
                    options.Endpoint.CircuitBreaker.SamplingDuration = allot.Sampling;
                }),
            configuration, row)
            .SelectPipelineByAuthority();

    // Section bind refuses an empty section by construction, so an unconfigured hop keeps its derived
    // posture instead of failing composition on a section an operator never had to write.
    static IHttpStandardResiliencePipelineBuilder Sectioned(
        IHttpStandardResiliencePipelineBuilder builder, IConfiguration configuration, HopPolicy row) =>
        Section(configuration, row).Match(Some: builder.Configure, None: () => builder);

    static IStandardHedgingHandlerBuilder Sectioned(
        IStandardHedgingHandlerBuilder builder, IConfiguration configuration, HopPolicy row) =>
        Section(configuration, row).Match(Some: builder.Configure, None: () => builder);

    static Option<IConfigurationSection> Section(IConfiguration configuration, HopPolicy row) =>
        configuration.GetSection($"{SectionRoot}:{row.PipelineKey}") is var section && section.GetChildren().Any()
            ? Some(section)
            : None;

    // Route exhaustion stops hedging, so a single-group row draws its primary first and walks the rest in
    // declaration order while a multi-group row spreads by weight on every attempt.
    static void Route(IRoutingStrategyBuilder route, Seq<WeightedUriEndpoint> routes) =>
        ignore(route.ConfigureWeightedGroups(groups => {
            groups.SelectionMode = routes.Count > 1
                ? WeightedGroupSelectionMode.EveryAttempt
                : WeightedGroupSelectionMode.InitialAttempt;
            routes.Iter(endpoint => groups.Groups.Add(new WeightedUriEndpointGroup {
                Weight = endpoint.Weight,
                Endpoints = { endpoint },
            }));
        }));
}
```

## [04]-[KEYED_PIPELINES]

- Owner: `KeyedLane` — the one keyed registry registration fold for every non-HTTP hop, with its `Composition` seat record, its breaker seats, its transport chaos catalogue, and the closure proof; `HopStrategy` `[SmartEnum<string>]` the pipeline-row vocabulary whose declaration order IS the strategy order and whose delegate column IS each strategy's arm; `HopSeat` the per-row arm argument; `HopEnricher` the per-row measurement dimensions; the channel-policy record is the canonical `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` `GrpcChannelPolicy` consumed by reference — this page reads `GrpcChannelPolicy.Canonical` and never re-declares the record.
- Cases: 10 `HopStrategy` rows in canonical order — admission, budget, redial, retry, breaker, deadline, then the four chaos planes; the order is the derivation, not a convention.
- Entry: `Register(IServiceCollection services, Composition composition, params ReadOnlySpan<HopPolicy> rows)` folds one `AddResiliencePipeline<string, HopOutcome>` entry per piped row; `Proven(ResiliencePipelineProvider<string> pipelines, params ReadOnlySpan<HopPolicy> rows)` returns `Fin<Unit>` — the built-provider half of the closure proof, probing every piped row through `TryGetPipeline<HopOutcome>`.
- Auto: `ConfigureTelemetry(TelemetryOptions)` inserts the telemetry strategy at pipeline head and carries the estate's grammar to the meter — `MeteringEnrichers` append the transport, delivery, capability, and tenant dimensions, `ResultFormatter` projects the outcome to its wire key so the result dimension reads, and `SeverityProvider` resolves the emitting strategy back to its own roster row; every strategy row NAMES itself, so `(pipeline.name, strategy.name)` deduplicates the budget and attempt timeouts instead of merging them into one unattributed bucket; each pipeline binds a `CircuitBreakerStateProvider` so the breaker state reads from Polly's own observation surface, never a parallel state delegate; the pipeline-head limiter is `AutoReplenishment` true on every replenishing shape so the sliding-window and token-bucket rows self-refill without a parallel timer; the container `TimeProvider` flows into every registry pipeline, so one injection drives every delay, deadline, and sampling window.
- Packages: Polly.Core, Polly.Extensions, Polly.RateLimiting, System.Threading.RateLimiting, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new resilience dimension is one `HopStrategy` row carrying its arm and its severity, seated where its declaration order places it; one `HopRateLimit` key plus one `Admission` switch arm absorbs a new admission shape; a new measurement dimension is one `HopEnricher` tag beside its `HopTags` const; a new injected fault, substitution, or behavior is one weighted `ChaosRow` on its band beside one `HopChaos` key, and all four verbs — `AddChaosLatency`, `AddChaosFault`, `AddChaosOutcome`, `AddChaosBehavior` — arm here over all four planes; zero new surface.
- Boundary: strategy order is the derivation every reorder breaks with a named failure mode — admission counts logical calls so ONE limiter sits outermost and its single lease spans every attempt (a limiter inside retry converts a retry storm into permit starvation), the budget bounds the whole loop so the total deadline sits outside retry (inside, it re-arms per attempt and unbounds the loop), the redial sits ABOVE retry so it substitutes after every attempt fails (below, the retry predicate reads the substituted outcome and silently stops looping), health statistics count attempts so the breaker sits inside retry (outside, it reaches its throughput floor N times slower), each attempt earns a fresh deadline so the attempt timeout is innermost, and chaos sits below everything it tests since injection above a breaker proves nothing; a second `AddConcurrencyLimiter` beside the admission row is the deleted form that stacks two queues on one seam and makes the effective permit the smaller of two columns; retry and breaker BOTH bind the one `Transient` `PredicateBuilder<HopOutcome>` row, and that row is under-filled from either side: the default predicate handles every exception except `OperationCanceledException` and therefore never sees a `HopOutcome.Faulted` RESULT, so a hop stating its refusal as a result retries nothing and breaks no circuit — while the exception half must NAME the transports in use, since gRPC states every remote fault as a throw and an `RpcException` matching no exception row leaves the three `GrpcChannel` rows retrying nothing for the same reason from the opposite direction; `Unavailable` is the one status the row admits and `SocketException` covers the Unix-domain dial; `BrokenCircuitException` stays out of that row, since spending budget re-dialing a dead endpoint is the exact waste the breaker exists to stop, and the redial's own `Rejected` row handles the rejection family instead; `MaxDelay` caps the computed curve alone, so the `RetryAfter` projection a rejection or an open circuit advises bypasses the cap by design and any operator ceiling composes inside that generator; gRPC-native retry is rejected — the channel `ServiceConfig` retry and hedging fork a second retry owner; a SEPARATE store-rail pipeline family is rejected on the same ground and one rung lower — it duplicates the claim registry, the allotment derivation, the telemetry enricher, and the ownership law, while `[HOP_TOPOLOGY]` `[ONE_OWNER]` holds one registry per key type per container, so a store rail enters as two cases on the settled union at Tier-0 `[12]-[ADMISSION]` rung `[01]`; admission is one `RateLimiterStrategyOptions` row whose `RateLimiter` lease-producer delegate is the `Leased` projection over the row's `HopRateLimit Admission` column — `Concurrency` keeps the `DefaultRateLimiterOptions` typed `ConcurrencyLimiterOptions` path, `SlidingWindow` leases from a `SlidingWindowRateLimiter` and `TokenBucket` from a `TokenBucketRateLimiter`, every window, segment, permit, and replenishment value projecting off `HopAllotment`; a limiter handed through `AddRateLimiter` is NEVER disposed by the pipeline, so each minted limiter registers its release on `OnPipelineDisposed` — an unreleased replenishing limiter keeps a live refill timer for the process lifetime; the `LocalIpc` redial is one `AddFallback` strategy on the `Redials` column whose `FallbackStrategyOptions<HopOutcome>.FallbackAction` re-reads the peer manifest and reconnects — typed `Func<FallbackActionArguments<HopOutcome>, ValueTask<Outcome<HopOutcome>>>`, the `readonly struct FallbackActionArguments<HopOutcome>` carrier exposing `ResilienceContext Context` and the inbound `Outcome<HopOutcome> Outcome` — and a row without the column takes no fallback row at all, never a redial the composition must synthesize; `CircuitBreakerStateProvider` is SINGLE-ATTACH so one provider seats per pipeline key and a second pipeline reusing it throws at build, while `CircuitBreakerManualControl` seats per `Capability` group so isolate and close act on a capability's whole breaker set as one verb and a pipeline materializing lazily under a pinned control comes up dark; `AddHedging<TResult>` is reachable on this generic builder and stays UNARMED, because hedging admits only on a row holding both `Idempotent` and `Replayable` and no piped row does — the server-stream row is semantically repeat-safe yet cannot replay its body concurrently, and the object-store row is repeat-safe yet withholds `Replayable` because a hedged PUT uploads the body twice — so a hedge row here is a strategy nothing arms and hedging as a failure remedy is the rejected form; the four chaos planes compose `Runtime/determinism#ADVERSARIAL_PROBE` whole — `ChaosArming` writes the gate, the address, the seeded weighted pick, and the chain record, so this page declares its bands and its `HopChaos` row resolvers alone and mints no posture cell, no options body, and no decision record; the package's own `FaultGenerator` and `OutcomeGenerator<T>` catalogue constructions stay refused, since each builds its selection draw from an internal helper no options member substitutes and picks a different row every run beneath a gate that reads deterministic; an unarmed composition or an undeclared band appends no row at all, so a production pipeline carries zero chaos strategies rather than four disabled ones; per-pipeline options reload is declined — every knob derives from `HopAllotment`, so the reload unit is the deadline table and a per-pipeline options record forks the derivation it reloads; the canonical channel record carries keepalive 60s/30s, infinite pooled-connection idle, multiplexed HTTP/2, and 4 MiB caps in both directions.

```csharp signature
// GrpcChannelPolicy is OWNED by Rasm.Compute/Runtime/transport#TRANSPORT_AXIS (the canonical 9-field
// record: PooledConnectionIdle, KeepAlivePingDelay, KeepAlivePingTimeout, EnableMultipleHttp2Connections,
// MaxSendBytes, MaxReceiveBytes, InitialReconnectBackoff, MaxReconnectBackoff, HttpVersionPosture Version).
// This page consumes GrpcChannelPolicy.Canonical by reference and never re-declares the record — a local
// re-declaration (especially the stale 6-field subset that drops the reconnect-backoff and HTTP-version-posture
// fields) is the rejected second-owner form.

public sealed record HopSeat(
    HopPolicy Row,
    HopAllotment Allot,
    KeyedLane.Composition Composition,
    Func<Action, Unit> Reclaim);

// Vocabulary IS the pipeline. Declaration order is the canonical strategy order the resilience law derives,
// and each row carries the arm that appends it beside the severity its events map to. `Register` folds
// `Items`, so a new resilience dimension is one row seated where its declaration places it and no builder
// body re-spells the order.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HopStrategy {
    public static readonly HopStrategy Admission = new("admission", ResilienceEventSeverity.Warning, ArmAdmission);
    public static readonly HopStrategy Budget = new("budget", ResilienceEventSeverity.Error, ArmBudget);
    public static readonly HopStrategy Redial = new("redial", ResilienceEventSeverity.Warning, ArmRedial);
    public static readonly HopStrategy Retry = new("retry", ResilienceEventSeverity.Information, ArmRetry);
    public static readonly HopStrategy Breaker = new("breaker", ResilienceEventSeverity.Error, ArmBreaker);
    public static readonly HopStrategy Deadline = new("deadline", ResilienceEventSeverity.Warning, ArmDeadline);
    public static readonly HopStrategy Slow = new("chaos-latency", ResilienceEventSeverity.Information, ArmSlow);
    public static readonly HopStrategy Injection = new("chaos-fault", ResilienceEventSeverity.Information, ArmInjection);
    public static readonly HopStrategy Substitute = new("chaos-outcome", ResilienceEventSeverity.Information, ArmSubstitute);
    public static readonly HopStrategy Perturb = new("chaos-behavior", ResilienceEventSeverity.Information, ArmPerturb);

    public ResilienceEventSeverity Severity { get; }

    // Each arm names its strategy with its own row key, so `(pipeline, strategy)` telemetry deduplicates per
    // row and the emitting row resolves back through `TryGet` at the severity callback.
    [UseDelegateFromConstructor]
    public partial ResiliencePipelineBuilder<HopOutcome> Arm(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat);

    // One transient row converts implicitly into every `ShouldHandle` slot that reads it, so a hop's transient
    // class is declared once. `HandleResult` is what makes a stated refusal retriable at all: the package
    // default reads exceptions alone and a hop whose body ANSWERS its outcome loops zero times under it.
    //
    // Both halves are load-bearing, and the exception half is the one a result-shaped rail quietly under-fills.
    // Three rows ride `HopTransport.GrpcChannel`, and gRPC states every remote fault as a THROW —
    // `RpcException` never reaches `HandleResult` and matches neither shipped exception row, so a gRPC row whose
    // body raises rather than converts retries nothing and opens no circuit, the mirror image of the missing
    // `HandleResult`. Both `HopTransport.StoreRail` rows sit at the opposite pole and need no exception row of
    // their own: a provider SDK or cluster driver refusal converts to `HopOutcome.Faulted` at the hop body, which
    // is exactly what `HandleResult` reads. `Unavailable` alone earns the retry: it is the one code the protocol defines as a transient
    // condition a backed-off retry corrects, while `DeadlineExceeded` belongs to the budget the pipeline already
    // owns and every remaining code names a request the same request cannot fix. `SocketException` covers the
    // Unix-domain dial, whose connect fault surfaces below `HttpRequestException` on a `ConnectCallback` channel.
    static readonly PredicateBuilder<HopOutcome> Transient = new PredicateBuilder<HopOutcome>()
        .Handle<TimeoutRejectedException>()
        .Handle<HttpRequestException>()
        .Handle<SocketException>()
        .Handle<RpcException>(static refused => refused.StatusCode == StatusCode.Unavailable)
        .HandleResult(static outcome => outcome is HopOutcome.Faulted);

    static readonly PredicateBuilder<HopOutcome> Rejected = new PredicateBuilder<HopOutcome>()
        .Handle<ExecutionRejectedException>();

    static ResiliencePipelineBuilder<HopOutcome> ArmAdmission(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        KeyedLane.Leased(seat) is { } limiter
            ? (seat.Reclaim(limiter.Dispose), builder.AddRateLimiter(new RateLimiterStrategyOptions {
                Name = Admission.Key,
                RateLimiter = args => limiter.AcquireAsync(permitCount: 1, args.Context.CancellationToken),
            })).Item2
            : builder.AddRateLimiter(new RateLimiterStrategyOptions {
                Name = Admission.Key,
                DefaultRateLimiterOptions = new ConcurrencyLimiterOptions {
                    PermitLimit = seat.Allot.Permits,
                    QueueLimit = seat.Allot.Floor,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                },
            });

    static ResiliencePipelineBuilder<HopOutcome> ArmBudget(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        builder.AddTimeout(new TimeoutStrategyOptions { Name = Budget.Key, Timeout = seat.Allot.Total });

    static ResiliencePipelineBuilder<HopOutcome> ArmRedial(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        seat.Row.Redials
            ? builder.AddFallback(new FallbackStrategyOptions<HopOutcome> {
                Name = Redial.Key,
                ShouldHandle = Rejected,
                FallbackAction = seat.Composition.Redial(seat.Row),
            })
            : builder;

    // `MaxRetryAttempts` refuses a zero, so a single-shot row takes NO retry row rather than a retry bounded
    // at nothing; a re-offered spawn forks a second child the manifest read then races.
    static ResiliencePipelineBuilder<HopOutcome> ArmRetry(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        seat.Row.Retries
            ? builder.AddRetry(new RetryStrategyOptions<HopOutcome> {
                Name = Retry.Key,
                MaxRetryAttempts = seat.Allot.Attempts,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = seat.Allot.Backoff,
                MaxDelay = seat.Allot.Attempt,
                ShouldHandle = Transient,
                DelayGenerator = static args => new ValueTask<TimeSpan?>(Advised(args.Outcome.Exception)),
            })
            : builder;

    // Server-advised windows honor verbatim and escape `MaxDelay` by construction; a null return falls back
    // to the computed curve, which is the whole opt-out channel for every unadvised fault. Advice reads the
    // EXCEPTION alone, so a store-rail refusal — stated as a result, never thrown — carries none and rides the
    // computed curve: a provider's own `Retry-After` cannot reach this generator, and that bound is the known
    // shape rather than an invitation to open a second advice channel beside it.
    static TimeSpan? Advised(Exception? cause) => cause switch {
        RateLimiterRejectedException shed => shed.RetryAfter,
        BrokenCircuitException open => open.RetryAfter,
        _ => null,
    };

    static ResiliencePipelineBuilder<HopOutcome> ArmBreaker(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HopOutcome> {
            Name = Breaker.Key,
            FailureRatio = seat.Allot.Trip,
            MinimumThroughput = seat.Allot.Throughput,
            SamplingDuration = seat.Allot.Sampling,
            BreakDuration = seat.Allot.Break,
            ShouldHandle = Transient,
            ManualControl = KeyedLane.BreakerOf(seat.Row.Needs),
            StateProvider = KeyedLane.StateOf(seat.Row.PipelineKey),
        });

    static ResiliencePipelineBuilder<HopOutcome> ArmDeadline(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        builder.AddTimeout(new TimeoutStrategyOptions { Name = Deadline.Key, Timeout = seat.Allot.Attempt });

    // Four chaos planes seat BELOW every strategy under test, each declaring its band and its row resolver and
    // handing the bound options straight to its builder verb — `ChaosArming` writes the gate, the address, the
    // record, and the weighted pick, so no options body, no posture cell, and no decision record live here;
    // an unarmed composition or an undeclared band answers `None` and appends nothing.
    static ResiliencePipelineBuilder<HopOutcome> ArmSlow(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        Banded(seat, ChaosKind.Latency).Match(
            Some: seated => builder.AddChaosLatency(seated.Arming.Latency(seated.Band)),
            None: () => builder);

    static ResiliencePipelineBuilder<HopOutcome> ArmInjection(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        Banded(seat, ChaosKind.Fault).Match(
            Some: seated => builder.AddChaosFault(seated.Arming.Fault(seated.Band, KeyedLane.HopChaos.Thrown)),
            None: () => builder);

    // Result substitution needs the result type, so this plane is reachable on the branch's ONE generic keyed
    // pipeline and nowhere else — the transport rail is where a refusal is faked without a throw, which is the
    // exact shape a hop body states as its own outcome.
    static ResiliencePipelineBuilder<HopOutcome> ArmSubstitute(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        Banded(seat, ChaosKind.Outcome).Match(
            Some: seated => builder.AddChaosOutcome(seated.Arming.Substitution<HopOutcome>(seated.Band, KeyedLane.HopChaos.Substituted)),
            None: () => builder);

    static ResiliencePipelineBuilder<HopOutcome> ArmPerturb(ResiliencePipelineBuilder<HopOutcome> builder, HopSeat seat) =>
        Banded(seat, ChaosKind.Behavior).Match(
            Some: seated => builder.AddChaosBehavior(seated.Arming.Behavior(seated.Band, row => KeyedLane.HopChaos.Perturbed(row, seat.Row.Needs))),
            None: () => builder);

    static Option<(ChaosArming Arming, ChaosBand Band)> Banded(HopSeat seat, ChaosKind kind) =>
        from arming in seat.Composition.Chaos
        from band in arming.BandOf(seat.Row.PipelineKey, kind)
        select (arming, band);
}

public static class KeyedLane {
    public static class HopTags {
        public const string Transport = "rasm.hop.transport";
        public const string Delivery = "rasm.hop.delivery";
        public const string Capability = "rasm.hop.capability";
    }

    // Two seats, two lifetimes: state reads are per-pipeline because a provider is SINGLE-ATTACH and a second
    // pipeline reusing one throws at build, while the manual control is per-capability so isolate and close act
    // on a whole group as one verb and a lazily materialized pipeline inherits a pinned isolation.
    static readonly ConcurrentDictionary<string, CircuitBreakerStateProvider> States = new(StringComparer.Ordinal);
    static readonly ConcurrentDictionary<Capability, CircuitBreakerManualControl> Controls = new();
    public static CircuitBreakerStateProvider StateOf(string pipelineKey) =>
        States.GetOrAdd(pipelineKey, static _ => new CircuitBreakerStateProvider());
    public static CircuitBreakerManualControl BreakerOf(Capability group) =>
        Controls.GetOrAdd(group, static _ => new CircuitBreakerManualControl());

    public sealed record Composition(
        ILoggerFactory Telemetry,
        Func<DeadlineClass, TimeSpan> Allotted,
        Func<HopPolicy, Func<FallbackActionArguments<HopOutcome>, ValueTask<Outcome<HopOutcome>>>> Redial,
        Option<ChaosArming> Chaos);

    // Transport-plane catalogue ROWS this page resolves — weights and rates stay the band's declaration, so a
    // realistic mix is a row set an operator retargets rather than a branch here. Each key maps to a real
    // transport perturbation: a connect failure, a stalled attempt, a stated refusal, and operator darkness
    // over the row's own capability group, so an injection exercises the same path a live fault takes.
    public static class HopChaos {
        public const string Connect = "connect";
        public const string Stall = "stall";
        public const string Refuse = "refuse";
        public const string Isolate = "isolate";

        public static Exception Thrown(string row) => row switch {
            Connect => new HttpRequestException("<injected-connect>"),
            _ => new TimeoutRejectedException("<injected-stall>"),
        };

        public static Outcome<HopOutcome> Substituted(string row) =>
            Outcome.FromResult<HopOutcome>(new HopOutcome.Faulted(new HopFault.Text($"<injected:{row}>")));

        public static ValueTask Perturbed(string row, Capability group) =>
            row == Isolate ? new ValueTask(BreakerOf(group).IsolateAsync()) : ValueTask.CompletedTask;
    }

    // Hop dimensions on every resilience measurement, so a per-transport or per-delivery-class resilience
    // series is real and joins the estate's telemetry on one grammar. Tenancy SPREADS the kernel pair's own
    // projection, which is empty for the root row, so an unpartitioned process tags nothing.
    public sealed class HopEnricher(HopPolicy row) : MeteringEnricher {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context) {
            context.Tags.Add(new(HopTags.Transport, row.Transport.ToString()));
            context.Tags.Add(new(HopTags.Delivery, row.Delivery.ToString()));
            context.Tags.Add(new(HopTags.Capability, row.Needs.ToString()));
            foreach (KeyValuePair<string, object?> tenancy in TenantContext.Current.Tags) {
                context.Tags.Add(tenancy);
            }
        }
    }

    // Every window, segment, permit, and replenishment value projects off the allotment; a null answer takes
    // Polly's own concurrency limiter, the one shape it disposes on the caller's behalf.
    public static RateLimiter? Leased(HopSeat seat) =>
        seat.Row.Admission.Switch<RateLimiter?>(
            concurrency: static () => null,
            slidingWindow: () => new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions {
                Window = seat.Allot.Total,
                SegmentsPerWindow = HopAllotment.WindowSegments,
                AutoReplenishment = true,
                PermitLimit = seat.Allot.Permits,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = seat.Allot.Floor,
            }),
            tokenBucket: () => new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions {
                ReplenishmentPeriod = seat.Allot.Backoff,
                TokensPerPeriod = seat.Allot.Throughput,
                AutoReplenishment = true,
                TokenLimit = seat.Allot.Permits,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = seat.Allot.Floor,
            }));

    // Registration takes the two-arg configure overload for ONE reason: `OnPipelineDisposed` is the reclaim
    // hook a supplied limiter needs, since a limiter handed through `AddRateLimiter` never disposes with the
    // pipeline that leases from it. Statement bodies here are the platform-forced registry seam.
    public static IServiceCollection Register(IServiceCollection services, Composition composition, params ReadOnlySpan<HopPolicy> rows) =>
        Iterable<HopPolicy>.FromSpan(rows).ToSeq().Filter(static row => row.Piped).Fold(services, (graph, row) =>
            graph.AddResiliencePipeline<string, HopOutcome>(row.PipelineKey, (builder, context) => {
                HopSeat seat = new(row, row.Allot(composition.Allotted), composition, release => {
                    context.OnPipelineDisposed(release);
                    return unit;
                });
                ignore(toSeq(HopStrategy.Items).Fold(
                    builder.ConfigureTelemetry(new TelemetryOptions {
                        LoggerFactory = composition.Telemetry,
                        MeteringEnrichers = { new HopEnricher(row) },
                        ResultFormatter = static (_, result) => result is HopOutcome outcome ? outcome.OutcomeKey : result,
                        SeverityProvider = static args =>
                            HopStrategy.TryGet(args.Source.StrategyName ?? string.Empty, out HopStrategy? strategy)
                                ? strategy!.Severity
                                : ResilienceEventSeverity.Information,
                    }),
                    (chain, strategy) => strategy.Arm(chain, seat)));
            }));

    // Closure proof completes here on the built provider, because a pipeline exists only after the provider
    // builds and `TryGetPipeline` probes without throwing where `GetPipeline` raises — so an unregistered hop refuses
    // at boot with its key named instead of throwing inside the first dispatch that dials it.
    public static Fin<Unit> Proven(ResiliencePipelineProvider<string> pipelines, params ReadOnlySpan<HopPolicy> rows) =>
        Iterable<HopPolicy>.FromSpan(rows).ToSeq().Filter(static row => row.Piped)
            .Traverse(row => pipelines.TryGetPipeline<HopOutcome>(row.PipelineKey, out _)
                ? Validation<Error, Unit>.Success(unit)
                : new Fault.InvalidValue(Label: row.PipelineKey, Requirement: "<a built hop pipeline>"))
            .As()
            .Map(static _ => unit)
            .ToFin();
}
```

## [05]-[OWNERSHIP_LAW]

- Owner: `OutboundSurface` — admission, dispatch, conflict evidence, and enforcement over one runtime record; `OutboundRuntime` capability record; `HopClaim` the retry-owner cell value; `HopOutcome` `[Union]`; `HopReceipt` receipt struct.
- Cases: Delivered, Refused, Faulted — Refused carries pre-flight admission faults, Faulted carries in-flight pipeline rejection.
- Entry: `Seat(OutboundRuntime runtime, params ReadOnlySpan<HopPolicy> rows)` returns `Fin<Unit>` — the boot gate claiming every row for the pipeline owner and proving every piped row materialized; `Carry<T>(OutboundRuntime runtime, OutboundHop hop, Func<CancellationToken, Task<(HopOutcome, T)>> send, Option<ILatencyContext> latency = default)` is the ONE hop run — `IO<T>` railing the body's produced value on a delivered outcome and the outcome's own `Error` otherwise; `Run(...)` is its outcome-only instantiation returning `IO<HopReceipt>`, the shape a write-only hop takes. Both take the composition root's `ILatencyContext` and both dispatch through the one `Execute` seat that marks the hop phase. Every exit is a receipt, never a throw.
- Receipt: `HopReceipt` — pipeline key, attempts, outcome, elapsed `Duration`, consumed deadline class, observed breaker state; every hop exit fans one `HopReceiptWire` envelope under `InstrumentFan.HopKind` through the one `Fan` projection both entries compose, so hop attempts and durations project off the receipt fan with zero call-site metering.
- Packages: Polly.Core, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one outcome case per new terminal kind; one `HopFault` rejection arm per new pipeline termination verb, seated ahead of `Foreign`; zero new surface.
- Boundary: a hop body that produces a value carries it out through `Carry<T>` on the run that timed it, so the reported value and the receipt describe ONE transport call — a hop run for its outcome followed by a second raw call for its value is the deleted form, since that second frame rides no pipeline, no retry, and no breaker while the receipt attributes its timing to the first; exactly one retry owner per hop, claimed at COMPOSITION through `Seat` and read by `Admit` — a per-dispatch claim swap re-decides a boot fact on every frame and leaves an unregistered key admitting until its first dial throws; domain rails retry through `Schedule`, transport hops retry through the keyed or HTTP pipeline, never both on one seam, since a schedule of m over a pipeline of n multiplies attempts invisibly and inflates the idempotency window by m; the remote object store and the wide-column store seat on THIS owner because `[LAYER_SPLIT]` row `[02]` reads the process seam alone and a `HopOutcome` is transport-neutral by construction, while the embedded and coordination bands answer rows `[03]` and `[01]` at the Persistence side, so a store call has exactly one retry owner and which one is a seam fact rather than a package preference; `Guarded` is the schedule-side guard that degrades to a single pass and emits the conflict receipt instead of stacking a loop, and `HopFault.OwnerConflict` carries both declaration symbols beside the discarded policy so a duplicate declaration is rankable rather than silent; `Execute` runs the hop through `ExecuteOutcomeAsync` over a pooled `ResilienceContext` leased from `ResilienceContextPool.Shared` and returned in `finally`, so a pipeline rejection surfaces as a captured `Outcome<HopOutcome>.Exception` with no exception-as-control-flow round-trip; the lease fixes `OperationKey` to the hop's own `IdempotencyKey`, so the dedup identity reaches every attempt and lands as `operation.key` on every resilience event, and `Window`, a `readonly struct ResiliencePropertyKey<Duration>`, carries the allotment span through `context.Properties` BEFORE the pipeline runs, never ambient state — a key or window minted inside the retried callback changes per attempt and defeats itself; the total-outcome fold happens exactly ONCE at this seam over a taxonomy ordered child-before-parent, `IsolatedCircuitException` ahead of its `BrokenCircuitException` base so operator-forced darkness never masquerades as a dependency open, and a caller re-folding the rail re-opens the vocabulary the seam retired; `Enforce` sweeps the per-capability manual breakers from the effective degradation level and runs once at boot against the resolved level, so a pipeline materializing lazily under a pinned isolation comes up dark rather than serving one undegraded call; `BreakerState` answers `None` for a row whose breaker lives inside the HTTP handler's per-authority instances, because a receipt claiming `Closed` from an unattached provider reads as a measurement to every consumer; the hop's latency checkpoint records through `LatencySpine.Mark(ILatencyContext, CheckpointToken)` in the same `finally` that returns the pooled context, so a faulted or rejected hop is measured exactly like a delivered one and the `Duration` on the receipt is never the recorder's only source; the runtime record and its issued `CheckpointToken` are constructed once at the composition root.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HopOutcome {
    private HopOutcome() { }

    public sealed record Delivered : HopOutcome;
    public sealed record Refused(Error Reason) : HopOutcome;
    public sealed record Faulted(Error Reason) : HopOutcome;
}

// Breaker state is an OPTION because only a keyed row seats an observable provider — an HTTP row's breaker
// lives inside the handler's per-authority instances, so a `Closed` stamped from an unattached provider would
// report health nobody measured.
public readonly record struct HopReceipt(
    string PipelineKey,
    int Attempts,
    HopOutcome Outcome,
    Duration Elapsed,
    DeadlineClass Consumed,
    Option<CircuitState> Breaker);

public sealed record HopClaim(string Owner, string PolicyRef);

// One spelling per wire key: the projection below and every reliability objective partitioning the hop and
// delivery counters on their outcome dimension read the same const, so a renamed case cannot leave an
// objective selecting a value no receipt ever writes.
public static class HopOutcomeWire {
    public const string Delivered = "delivered";
    public const string Refused = "refused";
    public const string Faulted = "faulted";

    extension(HopOutcome outcome) {
        public string OutcomeKey => outcome.Switch(
            delivered: static _ => Delivered,
            refused: static _ => Refused,
            faulted: static _ => Faulted);
    }
}

public sealed record HopReceiptWire(string Hop, string Outcome, int Attempts, double ElapsedSeconds, string? Breaker = null) {
    public static HopReceiptWire From(HopReceipt receipt) =>
        new(receipt.PipelineKey, receipt.Outcome.OutcomeKey, receipt.Attempts, receipt.Elapsed.TotalSeconds,
            receipt.Breaker.Match(Some: static state => state.ToString(), None: static () => null));
}

public sealed record OutboundRuntime(
    ResiliencePipelineProvider<string> Pipelines,
    ConsumptionProfile Profile,
    ClockPolicy Clocks,
    Func<DeadlineClass, TimeSpan> Allotted,
    Func<DegradationLevel> Level,
    Atom<HashMap<string, HopClaim>> RetryOwners,
    CheckpointToken Checkpoint,
    ReceiptSinkPort Sink,
    JsonSerializerOptions Wire) {
    // Checkpoint is the hop phase's token, issued ONCE at composition through
    // ILatencyContextTokenIssuer.GetCheckpointToken against the folded LatencyCheckpoint roster — a name
    // resolved per call against an unfolded roster answers a positionless token whose writes drop with
    // nothing raised.
    public Option<CircuitState> BreakerState(HopPolicy row) =>
        row.Piped ? Some(KeyedLane.StateOf(row.PipelineKey).CircuitState) : None;
}

public static class OutboundSurface {
    public const string PipelineOwner = "transport-pipeline";
    public const string ScheduleOwner = "schedule";

    // Claim identity is the HOP, never the policy or the instance, so per-authority pipeline instances stay
    // one owner with N isolated states. A losing claim answers the conflict carrying both symbols and the
    // discarded policy — the only place the loser survives for repair comparison.
    public static Fin<HopClaim> Claim(OutboundRuntime runtime, HopPolicy row, string owner) =>
        runtime.RetryOwners.Swap(owners => owners.TryAdd(row.PipelineKey, new HopClaim(owner, row.Owner)))
            .Find(row.PipelineKey)
            .Filter(held => held.Owner == owner)
            .ToFin(new HopFault.OwnerConflict(
                row.PipelineKey,
                runtime.RetryOwners.Value.Find(row.PipelineKey).Map(static held => held.PolicyRef).IfNone("<unrowed>"),
                owner,
                row.Owner));

    // Boot gate: every row claims its pipeline owner and every piped row proves materialized BEFORE a dispatch
    // reaches the registry, so a missing registration names its key at composition rather than throwing inside
    // whichever frame dials it first; this sweep also fixes the boot darkness the degradation level implies.
    public static Fin<Unit> Seat(OutboundRuntime runtime, params ReadOnlySpan<HopPolicy> rows) =>
        Iterable<HopPolicy>.FromSpan(rows).ToSeq() switch {
            var seated => seated
                .Traverse(row => Claim(runtime, row, PipelineOwner).Match(
                    Succ: static _ => Validation<Error, Unit>.Success(unit),
                    Fail: static conflict => Validation<Error, Unit>.Fail(conflict)))
                .As()
                .Map(static _ => unit)
                .ToFin()
                .Bind(_ => KeyedLane.Proven(runtime.Pipelines, seated.ToArray().AsSpan())),
        };

    public static Fin<HopPolicy> Admit(OutboundRuntime runtime, OutboundHop hop) =>
        from row in Fin.Succ(hop.Policy)
        from _excluded in guardnot(row.ExcludedOn(runtime.Profile), new HopFault.Excluded(row.PipelineKey))
        from _fenced in guard(runtime.Level().Permits(row.Needs), new HopFault.Fenced(row.PipelineKey))
        from _owner in runtime.RetryOwners.Value.Find(row.PipelineKey)
            .Filter(static held => held.Owner == PipelineOwner)
            .ToFin(new HopFault.OwnerConflict(row.PipelineKey, "<unseated>", PipelineOwner, row.Owner))
        select row;

    // Carry is the ONE hop run and Run its Unit instantiation: a hop whose body PRODUCES a value (a decoded
    // register window, a parsed frame) carries it out on the same rail that fans the receipt, so the value
    // and the timing describe ONE transport call. The rejected form is a hop run for its outcome followed by
    // a second raw call for its value — two frames on the wire, the reported value taken from the untimed,
    // unretried, unbroken-circuit one. The body states its own HopOutcome because a protocol-level refusal (a
    // 404, a false confirmed-request ack) is a hop fact the rail cannot infer from a non-throwing return.
    public static IO<T> Carry<T>(OutboundRuntime runtime, OutboundHop hop, Func<CancellationToken, Task<(HopOutcome Outcome, T Value)>> send, Option<ILatencyContext> latency = default) {
        Atom<Option<T>> carried = Atom(Option<T>.None);
        return Admit(runtime, hop).Match(
                Succ: row => Execute(runtime, row, hop, async ct => {
                    (HopOutcome outcome, T value) = await send(ct).ConfigureAwait(false);
                    ignore(carried.Swap(_ => Some(value)));
                    return outcome;
                }, latency),
                Fail: error => IO.pure(Conflicted(runtime, hop.Policy, error)))
            .Bind(receipt => Fan(runtime, receipt))
            .Bind(receipt => (receipt.Outcome, carried.Value) switch {
                (HopOutcome.Delivered, { IsSome: true, Case: T value }) => IO.pure(value),
                (HopOutcome.Refused r, _) => IO.fail<T>(r.Reason),
                (HopOutcome.Faulted f, _) => IO.fail<T>(f.Reason),
                _ => IO.fail<T>(new HopFault.Excluded(hop.Policy.PipelineKey)),
            });
    }

    // The latency context is the P14 threading half: the composition root's factory hands it in and the hop
    // boundary records ONE checkpoint through LatencySpine.Mark, deleting the per-fold Stopwatch. A caller
    // with no telemetry composition passes none and the hop runs unrecorded rather than minting a second
    // recorder — the same honesty the untraced-band arm takes.
    public static IO<HopReceipt> Run(OutboundRuntime runtime, OutboundHop hop, Func<CancellationToken, Task<HopOutcome>> send, Option<ILatencyContext> latency = default) =>
        Admit(runtime, hop).Match(
            Succ: row => Execute(runtime, row, hop, send, latency),
            Fail: error => IO.pure(Conflicted(runtime, hop.Policy, error)))
        .Bind(receipt => Fan(runtime, receipt));

    static IO<HopReceipt> Fan(OutboundRuntime runtime, HopReceipt receipt) =>
        runtime.Sink.Send(Correlation.Mint(), TenantContext.Current, TelemetrySource.AppHost.Key, InstrumentFan.HopKind,
            JsonSerializer.SerializeToElement(HopReceiptWire.From(receipt), runtime.Wire)).Map(_ => receipt);

    public static IO<HopReceipt> Guarded(OutboundRuntime runtime, HopPolicy row, Schedule retry, IO<HopReceipt> work, Action<HopReceipt> evidence) =>
        Claim(runtime, row, ScheduleOwner).Match(
            Succ: _ => work.Retry(retry),
            Fail: conflict => IO.lift(fun(() => evidence(Conflicted(runtime, row, conflict)))).Bind(_ => work));

    // Enforcement folds the CAPABILITY set, never the row set: one control governs a capability's whole
    // breaker set as one verb, so a hop registered after the sweep inherits the pinned isolation instead of
    // materializing light. Both verbs are idempotent, so a repeated command cannot stack.
    public static IO<Unit> Enforce(DegradationLevel effective, params ReadOnlySpan<HopPolicy> rows) =>
        Iterable<HopPolicy>.FromSpan(rows).ToSeq().Map(static row => row.Needs).Distinct()
            .TraverseM(group => IO.liftAsync(async () => {
                await (effective.Permits(group)
                    ? KeyedLane.BreakerOf(group).CloseAsync()
                    : KeyedLane.BreakerOf(group).IsolateAsync());
                return unit;
            }))
            .As()
            .Map(static _ => unit);

    // Allotment span rides the typed side channel, fixed BEFORE the pipeline runs, so a dedup window derives
    // from the budget rather than from a backoff parameter no strategy publishes.
    static readonly ResiliencePropertyKey<Duration> Window = new("rasm.hop.window");

    // ONE recording seat for both entries: Carry and Run funnel here, so the hop phase is marked once per hop
    // whatever shape the caller took, and a mark placed at each entry would double-count a carried hop.
    // `TryGetPipeline` keeps an unregistered key a typed refusal rather than a throw in the request path.
    static IO<HopReceipt> Execute(OutboundRuntime runtime, HopPolicy row, OutboundHop hop, Func<CancellationToken, Task<HopOutcome>> send, Option<ILatencyContext> latency) =>
        IO.liftAsync(async envIO => {
            var mark = runtime.Clocks.Mark();
            var attempts = 0;
            var context = ResilienceContextPool.Shared.Get(hop.IdempotencyKey, envIO.Token);
            context.Properties.Set(Window, Duration.FromTimeSpan(runtime.Allotted(row.Total)));
            try {
                return runtime.Pipelines.TryGetPipeline<HopOutcome>(row.PipelineKey, out ResiliencePipeline<HopOutcome>? pipeline)
                    ? Receipt(runtime, row, attempts, Fold(await pipeline!.ExecuteOutcomeAsync(
                        async (ctx, state) => { state.Bump(); return Outcome.FromResult(await state.Send(ctx.CancellationToken)); },
                        context, (Bump: new Action(() => attempts++), Send: send)), envIO.Token), mark)
                    : Receipt(runtime, row, attempts, new HopOutcome.Refused(new HopFault.Text($"<unregistered-hop:{row.PipelineKey}>")), mark);
            }
            finally {
                ResilienceContextPool.Shared.Return(context);
                latency.Iter(ctx => ignore(LatencySpine.Mark(ctx, runtime.Checkpoint)));
            }
        });

    // One total fold, ordered child-before-parent so operator-forced darkness never reads as a dependency
    // open, and cancelled-versus-rejected stays structural: the caller's token decides which of the two this
    // cancellation was. Every arm binds its own typed evidence, so escalation matches the case.
    static HopOutcome Fold(Outcome<HopOutcome> captured, CancellationToken caller) => captured switch {
        { Exception: null, Result: { } outcome } => outcome,
        { Exception: OperationCanceledException } when caller.IsCancellationRequested => new HopOutcome.Faulted(new HopFault.CallerLeft()),
        { Exception: TimeoutRejectedException slow } => new HopOutcome.Faulted(new HopFault.Deadline(slow.Timeout)),
        { Exception: IsolatedCircuitException dark } => new HopOutcome.Faulted(new HopFault.ForcedDark(dark.TelemetrySource?.PipelineName)),
        { Exception: BrokenCircuitException open } => new HopOutcome.Faulted(new HopFault.Open(open.RetryAfter)),
        { Exception: RateLimiterRejectedException shed } => new HopOutcome.Faulted(new HopFault.Shed(shed.RetryAfter)),
        { Exception: { } foreign } => new HopOutcome.Faulted(new HopFault.Foreign(Error.New(foreign))),
        _ => new HopOutcome.Faulted(new HopFault.Empty()),
    };

    static HopReceipt Receipt(OutboundRuntime runtime, HopPolicy row, int attempts, HopOutcome outcome, long mark) =>
        new(row.PipelineKey, attempts, outcome, runtime.Clocks.Elapsed(mark), row.Total, runtime.BreakerState(row));

    static HopReceipt Conflicted(OutboundRuntime runtime, HopPolicy row, Error reason) =>
        new(row.PipelineKey, Attempts: 0, new HopOutcome.Refused(reason), Duration.Zero, row.Total, runtime.BreakerState(row));
}
```

## [06]-[DISCOVERY_ATTACH]

- Owner: `DiscoveryManifest` attach record; `CompanionChild` spawn capsule; `Discovery` static surface — path law, atomic publish, staleness probe, checksum gate, UDS connect, spawn, the named drain-verb arrow, and the drain-fan producer.
- Entry: `Read(ProfileRoots roots, int pid, JsonTypeInfo<DiscoveryManifest> contract)` — `Fin` aborts on missing, empty, or dead-pid manifests; `FanOf(OutboundRuntime runtime, ILatencyContext latency, Duration cooperative, string reason)` seats the drain-fan producer, taking the parent's remaining cooperative allotment as the budget the child inherits.
- Packages: Grpc.Net.Client (`GrpcChannel.ForAddress`; a `ConnectCallback` handler resolves `HttpHandlerType.Custom`, which forfeits the balancer surface WHOLE yet reports only half that forfeit — `ConnectAsync`, `State`, and `WaitForStateChangedAsync` raise `InvalidOperationException` loudly, while `CallOptions.WithWaitForReady(true)` accepts and does nothing, because `Custom` takes the passive subchannel transport whose connect reports Ready without connecting and leaves the one `PickResultType.Fail` arm that reads the flag unreachable), Grpc.Core.Api (`RpcException.StatusCode`/`Status.Detail`, `Metadata` — every remote fault leaves as a throw), Grpc.Tools (build-only `<Protobuf>` codegen emitting `ControlService` into this assembly), NodaTime.Serialization.Protobuf (`ToProtobufDuration` carries the inherited remainder onto the drain request), LanguageExt.Core, NodaTime, BCL inbox
- Growth: the connect dispatch is the single Unix-domain-socket route; socket-file mode and the accept-side peer-credential read are the access boundary, never a transport-level ACL; zero new surface.
- Boundary: `Publish` and `Connect` are the named boundary capsules carrying statement bodies — atomic temp-write-then-move publication and the UDS connect callback; the socket lives at the temp-root `rasm-{pid}.sock` path under the 104-byte `sun_path` cap; the manifest directory is created 0700 and directory mode is the credential boundary; `Compatible` takes the additive-versus-breaking classifier as a delegate — checksum equality or additive drift admits, breaking drift is a typed rejection; the spawn is single-shot and the post-spawn manifest read rides the CompanionSpawn keyed pipeline registered at KEYED_PIPELINES; `DrainFan` is the parent-to-child drain-fan producer — it dials the peer over the `LocalIpc` hop case through `OutboundSurface.Run` and invokes `DrainVerb`, the ONE named control arrow, returning the `IO<Unit>` that `Spawn` threads into `CompanionChild.FanDrain` as the `drainFan` arg, so the drain conductor fans onto a named verb rather than a delegate any caller fills; liveness over this transport is an actual CALL, since the channel forfeits every connectivity member — a connect-probe drain raises `InvalidOperationException` on every fan and proves nothing about the peer, and wait-for-ready is the silent face of that one forfeit rather than a second remedy beside it; the redial and the retry cover DISJOINT fault classes and never stack, so nothing here is missing a third wait — `HopStrategy.ArmRedial` handles `ExecutionRejectedException` alone (a shed limiter, an open circuit) while a dead-peer connect arrives as `HttpRequestException`/`SocketException` on the retry row's `Transient` predicate, and wait-for-ready contributes its queueing only where the handler type admits the flag at all, which this channel never does; the accepted-socket peer-credential read moves to `Wire/companion#PEER_ADMISSION` (the serving side reads the connecting peer's uid and pid once at accept and never trusts the manifest) — a seam-split, not an owner here.

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

    public static Fin<DiscoveryManifest> Compatible(DiscoveryManifest peer, string localChecksum, Func<string, string, Fin<bool>> additiveOnly) =>
        peer.ContractChecksum == localChecksum
            ? Fin.Succ(peer)
            : additiveOnly(localChecksum, peer.ContractChecksum)
                .Bind(compatible => compatible
                    ? Fin.Succ(peer)
                    : Fin.Fail<DiscoveryManifest>(new HopFault.ChecksumBreaking(peer.ContractChecksum)));

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

    public static Func<DiscoveryManifest, CancellationToken, IO<Unit>> FanOf(
        OutboundRuntime runtime, ILatencyContext latency, Duration cooperative, string reason) =>
        (peer, token) => DrainFan(peer, runtime, latency, DrainVerb(cooperative, reason), token);

    // Naming the verb is what closes the arrow: `Wire/companion#CONTROL_SERVICE` owns the serving half, and
    // `ControlService` reaches this assembly through build-time `Grpc.Tools` codegen off the repo's own
    // `.proto` — a compile-time emission, never a project reference, so a generated client named on this spine
    // crosses no strata edge. Cooperative REMAINDER rides the request rather than a fresh child budget: the
    // child folds the minimum of its own drain class and what arrives, so a child re-arming its full allotment
    // cannot outlive the parent drain that asked for it. Trace context injects on this same call, which is what
    // lets the child's drain span descend from the parent's.
    public static Func<GrpcChannel, CancellationToken, Task<Unit>> DrainVerb(Duration cooperative, string reason) =>
        async (channel, token) => {
            ignore(await new ControlService.ControlServiceClient(channel).DrainRuntimeAsync(
                new DrainRuntimeRequest { Reason = reason, Cooperative = cooperative.ToProtobufDuration() },
                TraceContext.Inject(new Metadata()), cancellationToken: token));
            return unit;
        };

    // Unix-domain channels forfeit the connectivity surface entirely: a `SocketsHttpHandler` carrying a
    // `ConnectCallback` resolves to `HttpHandlerType.Custom`, and `ConnectAsync`, `State`, and
    // `WaitForStateChangedAsync` each raise `InvalidOperationException` on that type — so a connect probe here
    // is not a weak liveness signal, it is an unconditional throw the hop reports as a foreign fault on every
    // fan. Liveness over this transport is an actual CALL, which is why the fan rides `DrainVerb` while the
    // parameter stays a delegate: one seam names the verb and this fold still folds against a stub peer.
    // Converting `RpcException` HERE is what keeps the fan a receipt rather than a raise, since the retry row
    // admits only `Unavailable` and every other status is a settled refusal this body states as its own outcome.
    public static IO<Unit> DrainFan(DiscoveryManifest peer, OutboundRuntime runtime, ILatencyContext latency,
        Func<GrpcChannel, CancellationToken, Task<Unit>> control, CancellationToken token) =>
        OutboundSurface.Run(runtime, new OutboundHop.LocalIpc(peer), inner =>
            Drain(Connect(peer, GrpcChannelPolicy.Canonical), control,
                CancellationTokenSource.CreateLinkedTokenSource(token, inner).Token), latency)
            .Map(static _ => unit);

    static async Task<HopOutcome> Drain(
        GrpcChannel channel, Func<GrpcChannel, CancellationToken, Task<Unit>> control, CancellationToken token) {
        await using (channel) {
            try {
                ignore(await control(channel, token));
                return new HopOutcome.Delivered();
            }
            catch (RpcException refused) when (refused.StatusCode != StatusCode.Unavailable) {
                return new HopOutcome.Faulted(new HopFault.Text($"<drain-fan:{refused.StatusCode}>:{refused.Status.Detail}"));
            }
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
- Cases: 2 target cases — `Endpoint(Uri)` for the network-borne channels, `Peer(DiscoveryManifest)` for the in-app companion channel; 4 channel rows — push, webhook, email, in-app — each binding the `OutboundHop` its bytes ride through a target-discriminating `Hop(DeliveryTarget, string)` returning `Fin<OutboundHop>`: push and webhook on `WebhookPost` over an `Endpoint`, email on `HttpApi` over an `Endpoint` (the transactional-mail API), in-app on `LocalIpc` over a `Peer` manifest; a channel fed the wrong target shape returns `HopFault.Excluded` so the in-app channel can never forge a null manifest and a network channel can never dial a peer; delivery dispositions ride `HopOutcome`.
- Entry: `Fan(DeliveryRuntime runtime, DeliveryMessage message, params ReadOnlySpan<DeliveryChannel> channels)` returns `IO<Seq<DeliveryReceipt>>` — deduplicates the message by its idempotency key, then fans it to each channel through the channel's `OutboundHop` so one notification reaches every configured channel under one dedupe guard.
- Auto: every channel rides its `OutboundHop` so delivery inherits the hop's retry, breaker, rate-limit, and deadline — a flapping webhook endpoint breaks on the existing circuit breaker and a rate-capped push channel admits through the existing sliding-window limiter, never a per-channel retry loop; the dedupe verdict is one `DedupeWindow.Admit(key, now)` call against the `Runtime/resources#DEDUPE_WINDOW` bounded seen-key window — a `true` is the first admission and a `false` is a key still holding an unexpired deadline, folding every channel to a `DeliveryReceipt` carrying the deduped flag — so the expiry prune, the capacity ceiling, and the admit-record race are the primitive's while the instant stays this composition's own `ClockPolicy` read, and this page carries no cell and no window column; each channel's delivery mints one `DeliveryReceipt` carrying the channel, the `HopReceipt`, and the delivery disposition so a partial fan (push delivered, email faulted) records every channel's outcome independently; the fan SCHEDULES every channel leg through `IO.Fork` before it awaits any of them, so the channels genuinely overlap and one slow channel never holds the others, while the evidence leg stays sequential after the join because the receipt stream is the one ordered record of what the fan did; a bare traverse over the deliveries is the deleted form that would leave the partial-fan claim as prose over a serial loop; the evidence leg fires the `Observability/hooks#HOOK_RAIL` `Delivery` row through the rail's own `Settled` member, so a per-channel subscriber reads the typed receipt while the envelope keeps carrying the instrument projection.
- Receipt: `DeliveryReceipt` — channel key, idempotency key, `HopOutcome`, deduped flag, attempt count, elapsed `Duration`, the advanced watermark (`Some` only on a fenced outbox-relay advance — the fan-out legs answer `None`), correlation id; every fanned receipt sends one `DeliveryReceiptWire` envelope under `InstrumentFan.DeliveryKind` through the runtime `Sink`, so per-channel outcomes project off the receipt fan.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one channel row absorbs a new delivery medium — a new SMS or chat channel is one `DeliveryChannel` row binding its `OutboundHop` over the matching `DeliveryTarget` case, never a parallel sender; a new target shape is one `DeliveryTarget` case breaking every channel's `Hop` switch; a new delivery disposition rides the existing `HopOutcome`; zero new surface.
- Boundary: the delivery fan-out is the only multi-channel notification owner — a per-channel sender, a notification service wrapper, and a parallel delivery queue are the deleted forms, so all channels ride one fan and one dedupe; delivery never owns its own resilience — each channel composes its `OutboundHop` so the retry-owner, breaker, and rate-limit are the existing hop policy, and the delivery fan is purely the fan-and-dedupe layer above the hops; the dedupe is bounded and NOT owned here — `DedupeWindow` at `Runtime/resources#DEDUPE_WINDOW` is the one TTL-and-capacity seen-key window, composed by this fan and by `Wire/topics#SUBSCRIPTION_FABRIC` alike, so a long-lived process accumulates no unbounded dedup state under either bound and a local idempotency-key map beside it is the twin that primitive deleted; the fan is the scheduled-delivery consumer — a `ScheduleEntry` row fires the fan on its cadence so scheduled multi-channel delivery is one schedule row plus one fan call, never a second scheduler; the in-app channel rides the `LocalIpc` hop over a `DeliveryTarget.Peer` carrying the attached companion's `DiscoveryManifest` so an in-app notification reaches the companion over the control hop with a real peer manifest, never a `default!` placeholder and never a separate transport — the `Hop(DeliveryTarget, string)` switch is total and a target/channel shape mismatch is a typed `HopFault.Excluded`, never an unsound construction; the message's idempotency key threads INTO the hop case, so the fan's dedupe key, the pipeline's `OperationKey`, and the receiver's dedup key are one value rather than three mints of one intent.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliveryTarget {
    private DeliveryTarget() { }

    public sealed record Endpoint(Uri Authority) : DeliveryTarget;
    public sealed record Peer(DiscoveryManifest Manifest) : DeliveryTarget;
}

// Delivery identity threads the MESSAGE's own idempotency key into the hop case, so the key the fan dedupes
// on, the key the pipeline leases as its `OperationKey`, and the key the receiver dedupes on are ONE value; a
// freshly minted per-call identity here re-keys every replay and defeats the dedup it advertises.
[SmartEnum<string>]
public sealed partial class DeliveryChannel {
    public static readonly DeliveryChannel Push = new("push", static (target, key) => target.Switch(
        state: key,
        endpoint: static (e, k) => Fin<OutboundHop>.Succ(new OutboundHop.WebhookPost(e.Authority, k)),
        peer: static (_, _) => Fin<OutboundHop>.Fail(new HopFault.Excluded("push:requires-endpoint"))));
    public static readonly DeliveryChannel Webhook = new("webhook", static (target, key) => target.Switch(
        state: key,
        endpoint: static (e, k) => Fin<OutboundHop>.Succ(new OutboundHop.WebhookPost(e.Authority, k)),
        peer: static (_, _) => Fin<OutboundHop>.Fail(new HopFault.Excluded("webhook:requires-endpoint"))));
    public static readonly DeliveryChannel Email = new("email", static (target, _) => target.Switch(
        endpoint: static e => Fin<OutboundHop>.Succ(new OutboundHop.HttpApi(e.Authority)),
        peer: static _ => Fin<OutboundHop>.Fail(new HopFault.Excluded("email:requires-endpoint"))));
    public static readonly DeliveryChannel InApp = new("in-app", static (target, _) => target.Switch(
        endpoint: static _ => Fin<OutboundHop>.Fail(new HopFault.Excluded("in-app:requires-peer")),
        peer: static p => Fin<OutboundHop>.Succ(new OutboundHop.LocalIpc(p.Manifest))));

    [UseDelegateFromConstructor]
    public partial Fin<OutboundHop> Hop(DeliveryTarget target, string idempotencyKey);
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

public sealed record DeliveryReceiptWire(string Channel, string Outcome, bool Deduped, int Attempts, double ElapsedSeconds, ulong? Watermark = null) {
    public static DeliveryReceiptWire From(DeliveryReceipt receipt) =>
        new(receipt.Channel, receipt.Outcome.OutcomeKey, receipt.Deduped, receipt.Attempts, receipt.Elapsed.TotalSeconds,
            receipt.Watermark.Match(Some: static cursor => (ulong?)cursor, None: static () => null));
}

public sealed record DeliveryRuntime(
    OutboundRuntime Outbound,
    Func<OutboundHop, DeliveryMessage, Func<CancellationToken, Task<HopOutcome>>> Send,
    DedupeWindow Dedupe,
    ILatencyContext Latency,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    HookRail Rail,
    JsonSerializerOptions Wire);

public static class DeliveryFanout {
    public static IO<Seq<DeliveryReceipt>> Fan(DeliveryRuntime runtime, DeliveryMessage message, params ReadOnlySpan<DeliveryChannel> channels) =>
        IO.lift(() => (Now: runtime.Clocks.Now, Correlation: Correlation.Mint())).Bind(frame => {
            var correlation = frame.Correlation;
            // One admission against the shared bounded window: TRUE is first sight, FALSE is a replay still
            // holding an unexpired deadline, so the false branch is the suppress arm. The prune, the ceiling,
            // and the admit-record race are the primitive's; the instant is the composition's own ClockPolicy
            // read, which is what lets a spec expire rows against a fake clock. This fan carries no cell and
            // no window column — a second seen-key map here is the twin that primitive exists to delete.
            var deduped = !runtime.Dedupe.Admit(message.IdempotencyKey, frame.Now);
            // Fork-before-await fan-out: every channel's leg SCHEDULES before any await, so a slow webhook never
            // holds the push leg and each channel settles its own outcome — a bare traverse over the deliveries
            // sequences them and makes the partial-fan claim prose over a serial loop. Evidence stays sequential
            // after the join, because the receipt stream is the one ordered record of what the fan did.
            return toSeq(channels.ToArray())
                .TraverseM(channel => (deduped
                    ? IO.pure(new DeliveryReceipt(channel.Key, message.IdempotencyKey, new HopOutcome.Delivered(), Deduped: true, 0, Duration.Zero, None, correlation))
                    : Deliver(runtime, channel, message, correlation)).Fork())
                .As()
                .Bind(handles => handles.TraverseM(static handle => handle.Await).As())
                .Bind(receipts => receipts.TraverseM(receipt => Evidence(runtime, receipt)).As());
        });

    // Every leg — delivered, refused, deduped — fans its evidence envelope; the arm counts outcomes. The typed
    // Delivery row fires beside that envelope because a subscriber reading channel, outcome, and attempt count
    // off the serialized wire re-derives what the receipt already carries.
    static IO<DeliveryReceipt> Evidence(DeliveryRuntime runtime, DeliveryReceipt receipt) =>
        IO.lift(() => runtime.Rail.Settled(receipt)).Bind(_ =>
            runtime.Sink.Send(receipt.Correlation, TenantContext.Current, TelemetrySource.AppHost.Key, InstrumentFan.DeliveryKind,
                JsonSerializer.SerializeToElement(DeliveryReceiptWire.From(receipt), runtime.Wire))).Map(_ => receipt);

    static IO<DeliveryReceipt> Deliver(DeliveryRuntime runtime, DeliveryChannel channel, DeliveryMessage message, CorrelationId correlation) =>
        (from target in message.Targets.Find(channel).ToFin(new HopFault.Text($"no-target:{channel.Key}"))
         from hop in channel.Hop(target, message.IdempotencyKey)
         select (Target: target, Hop: hop)).Match(
            Succ: bound => OutboundSurface.Run(runtime.Outbound, bound.Hop, runtime.Send(bound.Hop, message), runtime.Latency)
                .Map(receipt => new DeliveryReceipt(channel.Key, message.IdempotencyKey, receipt.Outcome, Deduped: false, receipt.Attempts, receipt.Elapsed, None, correlation)),
            Fail: error => IO.pure(new DeliveryReceipt(channel.Key, message.IdempotencyKey, new HopOutcome.Refused(error), Deduped: false, 0, Duration.Zero, None, correlation)));
}
```

## [08]-[TS_PROJECTION]

- Owner: `HopReceiptWire`, `DeliveryReceiptWire` — the hop and delivery evidence shapes riding the receipt envelope; the outbox relay's per-row receipts reuse `DeliveryReceiptWire`, bound at Wire/outbox, never re-authored.
- Packages: BCL inbox
- Growth: one wire-member row per new hop or delivery evidence field; the outcome crosses as its case key; zero new surface.
- Boundary: elapsed time crosses as seconds so the instrument arm reads a number, never a parsed duration; breaker state crosses absent on an HTTP-borne hop whose breaker lives inside the handler's per-authority instances, so a dashboard reads a missing observation rather than a fabricated `Closed`; the watermark crosses `null` on every fan-out leg and carries the fenced cursor only on an outbox-relay advance; the envelope kind is the `InstrumentFan` constant the emitting fence passes.

```ts signature
type HopOutcomeKey = "delivered" | "refused" | "faulted";

interface HopReceiptWire {
  readonly hop: string;
  readonly outcome: HopOutcomeKey;
  readonly attempts: number;
  readonly elapsedSeconds: number;
  readonly breaker?: string;
}

interface DeliveryReceiptWire {
  readonly channel: string;
  readonly outcome: HopOutcomeKey;
  readonly deduped: boolean;
  readonly attempts: number;
  readonly elapsedSeconds: number;
  readonly watermark?: number;
}
```


Outbound carries no open research. Accepted-socket peer-credential projection moves to the serving owner at `Wire/companion#PEER_ADMISSION`, where the P/Invoke `getsockopt` route and the `ucred`/`xucred` blittable layout seat the admission fence. Transactional-mail channel target API resolves at app-root creation behind the same app-root pin the OTLP exporter rides; the channel rows bind the `OutboundHop` only, never the provider client, so a mail provider is one channel target Uri, never a delivery-page client.

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
