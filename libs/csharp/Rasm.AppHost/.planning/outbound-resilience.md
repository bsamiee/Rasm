# [APPHOST_OUTBOUND_RESILIENCE]

Outbound boundary ownership for the runtime spine: seven `OutboundHop` cases bind to frozen `HopPolicy` rows, every hop holds exactly one retry surface — the standard or hedging HTTP handler on SocketsHttpHandler-borne rows, one keyed Polly pipeline per non-HTTP row — admission folds the degradation gate, the modality exclusion, and the retry-owner claim into one `Fin` rail, the discovery manifest and UDS attach law carry the standalone-to-host and companion-spawn boundaries, and every dispatch exits as a `HopReceipt`. The page owns the hop axis, both pipeline registries, the discovery and spawn law, and the ownership law; the spine is Polly.Core, Polly.Extensions, Polly.RateLimiting, Microsoft.Extensions.Http.Resilience, Grpc.Net.Client, Thinktecture.Runtime.Extensions, LanguageExt.Core, and NodaTime.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                             |
| :-----: | ---------------- | ------------------------------------------------------------------ |
|   [1]   | HOP_AXIS         | Seven hop cases bound to frozen policy rows with total dispatch    |
|   [2]   | HTTP_PIPELINES   | Standard and hedging handlers for SocketsHttpHandler-borne rows    |
|   [3]   | KEYED_PIPELINES  | One keyed Polly registry and channel policy for non-HTTP hops      |
|   [4]   | DISCOVERY_ATTACH | Manifest law, UDS attach, checksum gate, companion child lifecycle |
|   [5]   | OWNERSHIP_LAW    | One retry owner per hop with conflict evidence and receipts        |

## [2]-[HOP_AXIS]

- Owner: `OutboundHop` `[Union]` seven sealed hop cases; `HopPolicy` per-case row record; `HopRows` frozen row set with the total dispatch; `HopIdempotency` keyless vocabulary; `HopTransport` keyless byte-mover vocabulary; `HopFault` `[Union]` fault family in the 4500 band; `ReleaseIdentity` vehicle-free update identity.
- Cases: HttpApi, Grpc, ServerStream, CompanionSpawn, LocalIpc, WebhookPost, UpdateCheck — the stream case is gRPC server-stream; UpdateCheck carries `ReleaseIdentity` and is structurally excluded on plugin rows.
- Entry: `HopPolicy Policy` — extension property; total state-free `Switch` from case to frozen row.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one case plus one `HopPolicy` row absorbs a new outbound boundary; the update vehicle lands in a future admission as one `UpdatePort` row on the UpdateCheck case; zero new surface.
- Boundary: deadline durations are the two hop deadline rows read by projection, never literals here; database retry is excluded from the hop law — the store execution strategy owns it; the `Transport` column names the byte mover on every row — `SocketsHttpHandler`, `GrpcChannel`, or process spawn — and resilience-event enrichment is the pipeline-key tag, never a second enrichment surface; the row's `Needs` capability is the degradation gate and `ExcludedOn` is the modality fence; webhook delivery identity is the case's `DeliveryKey` payload, never a header literal.

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

    public sealed record Text : HopFault { public Text(string detail) : base(detail, 4500) { } }
    public sealed record Excluded : HopFault { public Excluded(string detail) : base(detail, 4501) { } }
    public sealed record Fenced : HopFault { public Fenced(string detail) : base(detail, 4502) { } }
    public sealed record OwnerConflict : HopFault { public OwnerConflict(string detail) : base(detail, 4503) { } }
    public sealed record StaleManifest : HopFault { public StaleManifest(string detail) : base(detail, 4504) { } }
    public sealed record ChecksumBreaking : HopFault { public ChecksumBreaking(string detail) : base(detail, 4505) { } }
    public sealed record SpawnRejected : HopFault { public SpawnRejected(string detail) : base(detail, 4506) { } }
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

public sealed record ReleaseIdentity(string Product, string Channel, string Installed, Uri Feed);

public sealed record HopPolicy(
    string PipelineKey,
    HopTransport Transport,
    DeadlineClass Attempt,
    DeadlineClass Total,
    Capability Needs,
    HopIdempotency Idempotency,
    Func<HostProfile, bool> ExcludedOn);

public static class HopRows {
    static readonly Func<HostProfile, bool> Never = static _ => false;
    static readonly Func<HostProfile, bool> PluginRows = static profile => profile == HostProfile.RhinoPlugin || profile == HostProfile.Gh2Plugin;

    public static readonly HopPolicy HttpApi = new(nameof(OutboundHop.HttpApi), HopTransport.SocketsHttpHandler, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.MethodDerived, Never);
    public static readonly HopPolicy Grpc = new(nameof(OutboundHop.Grpc), HopTransport.GrpcChannel, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Keyed, Never);
    public static readonly HopPolicy ServerStream = new(nameof(OutboundHop.ServerStream), HopTransport.GrpcChannel, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Idempotent, Never);
    public static readonly HopPolicy CompanionSpawn = new(nameof(OutboundHop.CompanionSpawn), HopTransport.ProcessSpawn, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.LocalCompute, HopIdempotency.SingleShot, Never);
    public static readonly HopPolicy LocalIpc = new(nameof(OutboundHop.LocalIpc), HopTransport.GrpcChannel, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.LocalCompute, HopIdempotency.Keyed, Never);
    public static readonly HopPolicy WebhookPost = new(nameof(OutboundHop.WebhookPost), HopTransport.SocketsHttpHandler, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Keyed, Never);
    public static readonly HopPolicy UpdateCheck = new(nameof(OutboundHop.UpdateCheck), HopTransport.SocketsHttpHandler, DeadlineClass.HopAttempt, DeadlineClass.HopTotal, Capability.RemoteCompute, HopIdempotency.Idempotent, PluginRows);

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

## [3]-[HTTP_PIPELINES]

- Owner: `HttpLane` — one registration fold for the SocketsHttpHandler-borne rows (HttpApi, WebhookPost, UpdateCheck).
- Entry: `Wire(IServiceCollection services, IConfiguration configuration, OutboundHop hop, Uri authority)` — the idempotency row dispatch selects hedging versus standard and returns the graph.
- Auto: package-generated option validators prove the bound standard and hedging records at startup; `EnableReloads` keeps named policy live through the options monitor on the handler context route.
- Packages: Microsoft.Extensions.Http.Resilience, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one options row per pipeline key under the `Outbound` section root; a new HTTP-borne hop is one `Wire` call over its row — zero new surface.
- Boundary: `AddStandardResilienceHandler` binds rate limiter, total timeout, retry, breaker, and attempt timeout as one options record from the `Outbound:{PipelineKey}` section; hedging admits only rows whose idempotency is `Idempotent`, with ordered routing groups bound from the `:Routing` subsection; `DisableForUnsafeHttpMethods` is the method-derived guard; `SelectPipelineByAuthority` keys pipeline identity by authority; the named-client `AddHttpClient` registration arrives through the Microsoft.Extensions.Http transitive closure of the resilience package, never a direct pin; resilience context crosses through the request-message extensions, never ambient state.

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
            .AddStandardResilienceHandler(static options => options.Retry.DisableForUnsafeHttpMethods())
            .Configure(configuration.GetSection($"{SectionRoot}:{row.PipelineKey}"))
            .SelectPipelineByAuthority();

    static IStandardHedgingHandlerBuilder Hedged(IServiceCollection services, IConfiguration configuration, HopPolicy row, Uri authority) =>
        services.AddHttpClient(row.PipelineKey, client => client.BaseAddress = authority)
            .AddStandardHedgingHandler(routing => routing.ConfigureOrderedGroups(configuration.GetSection($"{SectionRoot}:{row.PipelineKey}:Routing")))
            .Configure(configuration.GetSection($"{SectionRoot}:{row.PipelineKey}"))
            .SelectPipelineByAuthority();
}
```

## [4]-[KEYED_PIPELINES]

- Owner: `KeyedLane` — the one keyed registry registration fold for every non-HTTP hop; `GrpcChannelPolicy` canonical channel-policy record.
- Entry: `Register(IServiceCollection services, ILoggerFactory telemetry, Func<DeadlineClass, TimeSpan> allotted, params ReadOnlySpan<HopPolicy> rows)` — one `AddResiliencePipeline` entry per row.
- Auto: `ConfigureTelemetry` inserts the telemetry strategy at pipeline head; resilience events land under the pipeline key in the package meter and logger.
- Packages: Polly.Core, Polly.Extensions, Polly.RateLimiting, LanguageExt.Core, BCL inbox
- Growth: one strategy row inside one keyed pipeline; chaos rows attach on the test-host profile row as one `AddChaosLatency`, `AddChaosFault`, or `AddChaosOutcome` strategy row each — zero new surface.
- Boundary: gRPC-native retry is rejected — the channel `ServiceConfig` retry and hedging policies are experimental surfaces and a second retry owner; reconnect on the UDS hop is redial-only; `CircuitBreakerManualControl` keyed per hop is the kill-switch enforcement surface; the `allotted` projection is the deadline-row read so no duration literal lives in a strategy; the canonical channel record carries keepalive 60s/30s, infinite pooled-connection idle, multiplexed HTTP/2, and 4 MiB caps in both directions.

```csharp signature
public sealed record GrpcChannelPolicy(
    TimeSpan PooledConnectionIdle,
    TimeSpan KeepAlivePingDelay,
    TimeSpan KeepAlivePingTimeout,
    bool EnableMultipleHttp2Connections,
    int MaxSendBytes,
    int MaxReceiveBytes) {
    public static readonly GrpcChannelPolicy Canonical = new(
        PooledConnectionIdle: Timeout.InfiniteTimeSpan,
        KeepAlivePingDelay: TimeSpan.FromSeconds(60),
        KeepAlivePingTimeout: TimeSpan.FromSeconds(30),
        EnableMultipleHttp2Connections: true,
        MaxSendBytes: 4 * 1024 * 1024,
        MaxReceiveBytes: 4 * 1024 * 1024);
}

public static class KeyedLane {
    public const int PermitLimit = 64;
    public const int QueueLimit = 256;
    public const int RetryAttempts = 3;
    public static readonly TimeSpan RetrySeed = TimeSpan.FromMilliseconds(200);

    static readonly ConcurrentDictionary<string, CircuitBreakerManualControl> Breakers = new(StringComparer.Ordinal);

    public static CircuitBreakerManualControl BreakerOf(string pipelineKey) =>
        Breakers.GetOrAdd(pipelineKey, static _ => new CircuitBreakerManualControl());

    public static IServiceCollection Register(IServiceCollection services, ILoggerFactory telemetry, Func<DeadlineClass, TimeSpan> allotted, params ReadOnlySpan<HopPolicy> rows) =>
        rows.ToArray().ToSeq().Fold(services, (graph, row) =>
            graph.AddResiliencePipeline(row.PipelineKey, builder => builder
                .ConfigureTelemetry(telemetry)
                .AddConcurrencyLimiter(permitLimit: PermitLimit, queueLimit: QueueLimit)
                .AddRetry(new RetryStrategyOptions {
                    MaxRetryAttempts = RetryAttempts,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = RetrySeed,
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions { ManualControl = BreakerOf(row.PipelineKey) })
                .AddTimeout(allotted(row.Attempt))));
}
```

Strategy literals are frozen policy rows held on `KeyedLane`; every keyed-pipeline literal traces to this table:

| [INDEX] | [POLICY]                 | [VALUE] | [RELOAD_CLASS] |
| :-----: | :----------------------- | ------: | :------------- |
|   [1]   | concurrency-permit-limit |      64 | frozen         |
|   [2]   | concurrency-queue-limit  |     256 | frozen         |
|   [3]   | retry-attempts           |       3 | frozen         |
|   [4]   | retry-seed-delay         |  200 ms | frozen         |

## [5]-[DISCOVERY_ATTACH]

- Owner: `DiscoveryManifest` attach record; `CompanionChild` spawn capsule; `Discovery` static surface — path law, atomic publish, staleness probe, checksum gate, UDS connect, spawn.
- Entry: `Read(ProfileRoots roots, int pid, JsonTypeInfo<DiscoveryManifest> contract)` — `Fin` aborts on missing, empty, or dead-pid manifests.
- Packages: Grpc.Net.Client, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one named-pipe hardening row on the connect dispatch covers the Windows surface; the peer-credential read lands as one admission-probe row on the manifest read; zero new surface.
- Boundary: `Publish` and `Connect` are the named boundary capsules carrying statement bodies — atomic temp-write-then-move publication and the UDS connect callback; the socket lives at the temp-root `rasm-{pid}.sock` path under the 104-byte `sun_path` cap; the manifest directory is created 0700 and directory mode is the credential boundary; `Compatible` takes the additive-versus-breaking classifier as a delegate — checksum equality or additive drift admits, breaking drift is a typed rejection; the spawn is single-shot and the post-spawn manifest read rides the CompanionSpawn keyed pipeline; `FanDrain` is the parent-to-child drain fan over the LocalIpc hop, consumed by the parent's drain registration row.

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

    static Fin<DiscoveryManifest> Alive(DiscoveryManifest manifest) =>
        Try.lift(() => Process.GetProcessById(manifest.Pid))
            .Run()
            .MapFail(_ => new HopFault.StaleManifest($"pid absent: {manifest.Pid}"))
            .Map(_ => manifest);
}
```

## [6]-[OWNERSHIP_LAW]

- Owner: `OutboundSurface` — admission, dispatch, conflict evidence, and enforcement over one runtime record; `OutboundRuntime` capability record; `HopOutcome` `[Union]`; `HopReceipt` receipt struct.
- Cases: Delivered, Refused, Faulted — Refused carries pre-flight admission faults, Faulted carries in-flight pipeline rejection.
- Entry: `Run(OutboundRuntime runtime, OutboundHop hop, Func<CancellationToken, Task<HopOutcome>> send)` — `IO<HopReceipt>` carries the boundary effect; every exit is a receipt, never a throw.
- Receipt: `HopReceipt` — pipeline key, attempts, outcome, elapsed `Duration`, consumed deadline class, breaker state.
- Packages: Polly.Core, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one outcome case per new terminal kind; zero new surface.
- Boundary: exactly one retry owner per hop held at the composition root — domain rails retry through `Schedule`, transport hops retry through the keyed or HTTP pipeline, never both on one seam; the `RetryOwners` claim cell is the structural proof and `Guarded` is the schedule-side guard that degrades to a single pass and emits the conflict receipt instead of stacking a loop; `Execute` is the named boundary capsule converting pipeline rejection exceptions to `Faulted`; `Enforce` sweeps the keyed manual breakers from the effective degradation level — the kill-switch consequence; the runtime record is constructed once at the composition root.

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
    Atom<HashMap<string, string>> RetryOwners,
    Func<string, CircuitState> BreakerStateOf);

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

    static IO<HopReceipt> Execute(OutboundRuntime runtime, HopPolicy row, Func<CancellationToken, Task<HopOutcome>> send) =>
        IO.liftAsync(async envIO => {
            var mark = runtime.Clocks.Mark();
            var attempts = 0;
            try {
                var outcome = await runtime.Pipelines.GetPipeline(row.PipelineKey)
                    .ExecuteAsync(token => { attempts++; return new ValueTask<HopOutcome>(send(token)); }, envIO.Token);
                return Receipt(runtime, row, attempts, outcome, mark);
            }
            catch (ExecutionRejectedException rejected) {
                return Receipt(runtime, row, attempts, new HopOutcome.Faulted(Error.New(rejected)), mark);
            }
        });

    static HopReceipt Receipt(OutboundRuntime runtime, HopPolicy row, int attempts, HopOutcome outcome, long mark) =>
        new(row.PipelineKey, attempts, outcome, runtime.Clocks.Elapsed(mark), row.Attempt, runtime.BreakerStateOf(row.PipelineKey));

    static HopReceipt Conflicted(OutboundRuntime runtime, HopPolicy row, Error reason) =>
        new(row.PipelineKey, Attempts: 0, new HopOutcome.Refused(reason), Duration.Zero, row.Total, runtime.BreakerStateOf(row.PipelineKey));
}
```

## [7]-[RESEARCH]

| [INDEX] | [ITEM]                                                                                                   | [PROOF]                                                                                                 | [GATE]           |
| :-----: | -------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- | ---------------- |
|   [1]   | Peer-credential projection on accepted UDS sockets through the raw socket-option read on macOS and Linux | dotnet run on a scratch UDS accept spike asserting the LOCAL_PEERCRED raw-option read on both platforms | DISCOVERY_ATTACH |
