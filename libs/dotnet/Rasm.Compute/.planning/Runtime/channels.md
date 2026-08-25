# [COMPUTE_CHANNELS]

Rasm.Compute owns the gRPC channel MECHANICS the suite wire moves over: the `RemoteTransport` dial axis warmed through its row's own `WarmProbe` and observed as typed connectivity transitions where the channel reports them, the canonical `GrpcChannelPolicy` every `GrpcChannelOptions` site reads, the per-call credential and encoding policy one interceptor stamps, the 64 KiB `FrameEdge` artifact frame law over the frozen `ArtifactFrame` descriptor, and the `WireLimits` ceilings every hostile parse on this branch reads. One identity regime holds the page: a dialed channel, the calls that cross it, and the framed bytes those calls carry.

`Runtime/wire` owns the wire CONTRACT — proto vocabulary, `ParseGuard` admission, the served progress stream, fault projection, the TS posture — so this page owns how bytes MOVE and that page owns what they SAY, joined by prose anchor rather than a cross-split fence import (`RpcEdge.Rpc` folds a raised `RpcException` through the `Runtime/wire#FAULT_PROJECTION` `WireFault.Classify` arm by reference). `Runtime/ingest` owns the broker and REST ingest legs and composes `CallSpine.AwaitedHttp` from here; `Runtime/observation` owns the durable sensor lane. Channel policy values arrive settled on `GrpcChannelPolicy.Canonical`; discovery, retry ownership, deadline rows, correlation, degradation, and the mounted instrument set compose from the AppHost spine. Package spine: Google.Protobuf, Grpc.Core.Api, Grpc.Net.Client, Grpc.Net.Client.Web, Grpc.Net.Common, Microsoft.IO.RecyclableMemoryStream, CommunityToolkit.HighPerformance, Mapperly, Thinktecture.Runtime.Extensions, LanguageExt.Core, and NodaTime.

## [01]-[INDEX]

- [02]-[TRANSPORT_AXIS]: five transport rows under the canonical `GrpcChannelPolicy` tuning owner — RID-gated HTTP/3-forward posture, channel warm-up, typed connectivity fold, grpc-web binary framing, and the three-column dial admission.
- [03]-[CALL_POLICY]: five credential rows and three compression rows behind one stamping interceptor reading the `DeadlineClass.HopTotal` row's own bound.
- [04]-[ARTIFACT_FRAMES]: `FrameEdge` fixes the 64 KiB frame law over the descriptor's own `uint64` extent — whole-artifact SHA-256 in the 32-byte `sha256` field, arrival-order reassembly, typed Put/Fetch, and mask-driven partial update — beside `WireLimits`.

## [02]-[TRANSPORT_AXIS]

- Owner: `RemoteTransport` `[SmartEnum<string>]` rows with streaming, credential, affinity, warm-probe, and dial columns; `GrpcChannelPolicy` the canonical channel-tuning record centralizing send/receive caps, reconnect backoff, pooled-idle, keepalive, multiplexing, and the HTTP-version posture so a single literal-free policy value seeds every `GrpcChannelOptions` site; `HttpVersionPosture` `[Union]` the two-case HTTP-version family resolving the BCL `HttpVersion`/`HttpVersionPolicy` channel-option pair from the host QUIC verdict; `StreamShape` and `NodeSelection` row vocabularies; `EndpointLoad` the measured-or-absent load reading `NodeSelection.LeastLoaded` ranks on; `WireTransition` `[Union]` the typed prior→next connectivity-transition family carrying its own absorbing column; `WarmProbe` `[SmartEnum<string>]` the two-row warm-and-observe family every transport row seats; `ComputeEndpoint` endpoint identity record carrying the call shapes its composition intends; `RpcEdge` the ONE `RpcException` classifier this package's rails funnel through; `WireChannels` — attach, open, warm-through-the-row's-probe, observe, redial.
- Cases: Http2; Http3 (the QUIC byte path admitting unary/server/client/duplex over TLS only, dial-gated on `HttpVersionPosture.QuicCapable` so the row exists on every host but faults Excluded where `QuicConnection.IsSupported` answers false); GrpcWeb (unary and server-stream only, `GrpcWebMode.GrpcWeb` binary — the text mode is the rejected google-client-only spelling); UnixDomainSocket (discovery manifest consumption, peer-credential law, and the 0700 bind directory that IS the grant surface); InProcess (the composition-supplied `HttpMessageHandler` factory on `ComputeEndpoint.Handler`, dialing `GrpcChannel.ForAddress` against an in-host pipeline with no socket — the row names no handler source, so the proof estate binds `Microsoft.AspNetCore.TestHost` `TestServer.CreateHandler` onto that seam and a production in-host root binds its own).
- Entry: `Open(ComputeEndpoint endpoint, CallSpine spine)` — `IO<Fin<WireServices>>`. Admission ACCUMULATES three independent columns through the `Validation` applicative before one `ToFin` widen — credential-row membership, every intended `StreamShape` the row carries, and a client certificate present wherever the row's credential asserts `MutualAuth` — so a browser endpoint asking for duplex and an mTLS endpoint carrying no certificate report TOGETHER rather than one hiding the other. `NodeSelection.Select` ranks the admitted endpoint roster by rotation, validated load, or warm-fingerprint tier through one total row dispatch.
- Law: warm is universal and its MECHANISM is row data. `Open` warms every row before the first deadline-bearing call so connection latency never lands inside a budget; a cold channel dialed without the warm leg is the deleted form. Connectivity tracking is the mechanism only where the channel dials its own `SocketsHttpHandler`, because `ConnectAsync`, `State`, and `WaitForStateChangedAsync` all throw `InvalidOperationException` on a channel whose handler carries a `ConnectCallback` or arrives from the composition — the UDS, InProcess, and GrpcWeb rows are all that class, so they seat `WarmProbe.RoundTrip` and warm through one throwaway `grpc.health.v1` `Check`, and `Observe` answers unit on them rather than throwing. A `warms: false` skip and a bare `Observe` reachable on a handler-supplied row are the two deleted forms of one defect: the first pays connection latency inside the first budget, the second throws where a result was expected.
- Law: the round-trip probe CLASSIFIES its own refusal rather than swallowing it. `WireFault.Classify` is this estate's one `RpcException` reader, so a probe whose status classifies `Unreachable` proves the channel did NOT warm and rails, while every other arm — `Unimplemented` included — proves the round trip exactly as `Serving` does. NAMED LOSS: the discarded-status form was one line shorter. Witness: under the swallow an `Unavailable` and an `Unimplemented` erased alike and the caller was told a cold channel was warm.
- Law: channel pooling rides one `GrpcChannel` per `ComputeEndpoint` (`PooledConnectionIdleTimeout` Infinite, multiplexed) reused across redials until the storeEpoch re-handshake replaces it — a per-call channel is the deleted form. `DisableResolverServiceConfig` stays true and `GrpcChannelOptions.ServiceConfig` is never set, so a resolver-supplied service config can never override the root-declared no-retry posture and the whole retry/hedging/load-balancing config surface stays unadmitted. AppHost remains the one hop retry owner.
- Law: a redial ACQUIRES before it releases. The stale capsule disposes on the success arm alone, because a failed re-handshake leaves the caller a channel that still works and a disposed capsule beside a `Fail` is a caller holding nothing. NAMED LOSS: a redial that succeeds and then faults at composition leaks one capsule until its finalizer runs. Witness: the prior order disposed first, so every transient manifest miss cost the caller its live channel.
- Law: the UDS grant surface is the BIND DIRECTORY, never the socket file — the runtime exposes no chmod on a bound `UnixDomainSocketEndPoint`, so the 0700 parent directory is the whole access control and a per-socket permission call is a member that does not exist. A bind onto an existing path fails, so boot UNLINKS a stale socket before binding, under an exclusive advisory guard alone: unlink-then-bind is a real race two live hosts lose together, an `O_EXCL` sentinel or a lock file beside the socket serializes the pair, and an unguarded unlink is the deleted form that lets a second host delete a socket the first is already serving.
- Auto: the `ConnectivityState` fold projects `Idle`/`Connecting`/`Ready`/`TransientFailure`/`Shutdown` into typed `WireTransition` prior→next rows, and an unrecognized foreign state lands `Unknown` carrying the observed value rather than re-labelling itself a real state a recovery would act on. Pump termination is a TYPE fact: `Closed` publishes `Absorbing`, the pump reads that column, and a re-pump past the channel's absorbing state — which parks forever on a `WaitForStateChangedAsync` no channel can satisfy — is unspellable rather than comment-guarded.
- Law: channel-state transitions and redials are span events on the live dispatch span (`Activity.AddEvent` carrying the endpoint correlation and the transition's own `Label`); a round-trip row marks dial and redial alone because its channel reports no state; storeEpoch drift after redial is its own event.
- Packages: Grpc.Core.Api (`CallInvoker`, `ChannelCredentials`, `Metadata`, `RpcException`), Grpc.Net.Client, Grpc.Net.Client.Web, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project — `HopFault`, `DeadlineClass`, `ClockPolicy`), BCL inbox (`System.Net.Http.HttpClient`/`HttpVersion`/`HttpVersionPolicy`, `System.Net.Security.SslClientAuthenticationOptions`, `System.Security.Cryptography.X509Certificates.X509Certificate2`/`X509CertificateCollection`, `System.Net.Quic.QuicConnection`)
- Growth: one row absorbs a new byte path, and a byte path a host admits later enters carrying its own security law; one `HttpVersionPosture` case absorbs a new version negotiation posture; one `NodeSelection` row absorbs a new farm strategy; one `WireTransition` case absorbs a new connectivity-state pairing; one `WarmProbe` row absorbs a new warm mechanism and every transport row seats it by name; zero new surface.
- Boundary: `GrpcChannelPolicy` is the canonical channel-tuning owner and `WireChannels` the named boundary capsule consuming it — keepalive, pooled-idle, multiplexing, reconnect-backoff, the HTTP-version posture, and the send/receive caps read from `GrpcChannelPolicy.Canonical` and are never re-declared. `KeepAlivePingDelay`/`KeepAlivePingTimeout`/`EnableMultipleHttp2Connections` and `KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests` are BCL `SocketsHttpHandler` members, not `Grpc.Net.Client`, so idle-pool connections never burn pings without an in-flight request and a redeclared gRPC-package keepalive member is the deleted form. HTTP-version selection is the `HttpVersion`/`HttpVersionPolicy` `GrpcChannelOptions` pair projected from `GrpcChannelPolicy.Canonical.Version.Wire`, self-resolved through the ONE `QuicCapable` predicate — a static OS carve beside it restates the verdict in a second alphabet and drifts the moment a platform ships the asset; a per-call version knob, a handler-level `GrpcWebHandler.HttpVersion` override, and a forced `Version30` on a QUIC-absent host are the deleted forms. Client-side HTTP/2 flow-control windows are the app-root Kestrel `Http2Limits` SERVER leg, so the only client stream knob here is `EnableMultipleHttp2Connections`. The generated artifact fetch is server-streaming and therefore fits the GrpcWeb row; any later client- or duplex-stream call remains structurally excluded by `Needs` at endpoint admission. Reconnect on UnixDomainSocket is redial-only with the storeEpoch re-handshake; a failed attach folds to the LocalOnly consequence, substrate predicates reading the retained Capability set rather than a second health probe. `NodeSelection.ModelWarmupAffinity` populates the endpoint affinity column from the warm-start session fingerprint so a cold companion routes to the node holding the matching EP-context blob — this endpoint affinity is the single warm-start column `SubstrateSelection.Plan` reads, never a second affinity notion parallel to endpoint identity, never a rank override, never a `ServiceConfig` load-balancing policy. Absence in the load reading is a TIER, not a magnitude: an unmeasured endpoint ranks below every measured one and orders by rotation inside its tier, where the prior `double.PositiveInfinity` fill made every unmeasured roster tie at one score and silently collapsed `LeastLoaded` onto "always the first endpoint". [SPIKE]: dialing this axis from inside the live integrated-host ALC converges on running-plugin evidence alone; the deterministic floor is the landed row set, the `WarmProbe` mechanism law, and the redial fold, each standing without it.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HttpVersionPosture {
    private HttpVersionPosture() { }

    public sealed record Http2Default : HttpVersionPosture;
    public sealed record Http3Forward : HttpVersionPosture;

    public static readonly bool QuicCapable = QuicConnection.IsSupported && !OperatingSystem.IsBrowser();

    public static HttpVersionPosture ForHost() => QuicCapable ? new Http3Forward() : new Http2Default();

    public (Version Version, HttpVersionPolicy Policy) Wire => Switch(
        http2Default: static _ => (HttpVersion.Version20, HttpVersionPolicy.RequestVersionExact),
        http3Forward: static _ => (HttpVersion.Version30, HttpVersionPolicy.RequestVersionOrHigher));
}

[SmartEnum]
public sealed partial class StreamShape {
    public static readonly StreamShape Unary = new();
    public static readonly StreamShape ServerStream = new();
    public static readonly StreamShape ClientStream = new();
    public static readonly StreamShape Bidi = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WireTransition {
    private WireTransition() { }

    public sealed record Connecting(ConnectivityState Prior) : WireTransition;
    public sealed record Ready(ConnectivityState Prior) : WireTransition;
    public sealed record Degraded(ConnectivityState Prior) : WireTransition;
    public sealed record Closed(ConnectivityState Prior) : WireTransition;
    public sealed record Idle(ConnectivityState Prior) : WireTransition;
    public sealed record Unknown(ConnectivityState Prior, ConnectivityState Observed) : WireTransition;

    public static WireTransition Of(ConnectivityState prior, ConnectivityState next) => next switch {
        ConnectivityState.Idle => new Idle(prior),
        ConnectivityState.Connecting => new Connecting(prior),
        ConnectivityState.Ready => new Ready(prior),
        ConnectivityState.TransientFailure => new Degraded(prior),
        ConnectivityState.Shutdown => new Closed(prior),
        _ => new Unknown(prior, next),
    };

    public bool Absorbing => this is Closed;

    public string Label => Switch(
        connecting: static c => $"<connecting:{c.Prior}>",
        ready: static r => $"<ready:{r.Prior}>",
        degraded: static d => $"<transient-failure:{d.Prior}>",
        closed: static s => $"<shutdown:{s.Prior}>",
        idle: static i => $"<idle:{i.Prior}>",
        unknown: static u => $"<unknown:{u.Prior}:{u.Observed}>");
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record GrpcChannelPolicy(
    TimeSpan PooledConnectionIdle,
    TimeSpan KeepAlivePingDelay,
    TimeSpan KeepAlivePingTimeout,
    bool EnableMultipleHttp2Connections,
    int MaxSendBytes,
    int MaxReceiveBytes,
    TimeSpan InitialReconnectBackoff,
    TimeSpan MaxReconnectBackoff,
    HttpVersionPosture Version) {
    public static readonly GrpcChannelPolicy Canonical = new(
        PooledConnectionIdle: Timeout.InfiniteTimeSpan,
        KeepAlivePingDelay: TimeSpan.FromSeconds(60),
        KeepAlivePingTimeout: TimeSpan.FromSeconds(30),
        EnableMultipleHttp2Connections: true,
        MaxSendBytes: 4 * 1024 * 1024,
        MaxReceiveBytes: 4 * 1024 * 1024,
        InitialReconnectBackoff: TimeSpan.FromSeconds(1),
        MaxReconnectBackoff: TimeSpan.FromSeconds(120),
        Version: HttpVersionPosture.ForHost());
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ComputeEndpoint(
    Uri Address, RemoteTransport Transport, CredentialPolicy Credential, CorrelationId Correlation,
    Seq<StreamShape> Needs = default,
    Option<DiscoveryManifest> Peer = default, Option<string> WarmFingerprint = default, Option<Func<HttpMessageHandler>> Handler = default,
    Seq<AsyncAuthInterceptor> Mints = default, Option<X509Certificate2> ClientCertificate = default);

public readonly record struct EndpointLoad(HashMap<Uri, double> Measured) {
    public static readonly EndpointLoad Unmeasured = new(HashMap<Uri, double>());

    public Option<double> At(Uri address) =>
        Measured.Find(address).Filter(static value => double.IsFinite(value) && value >= 0d);
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class RpcEdge {
    public static Error Rpc(Error raised) =>
        raised.Exception.Bind(static held => held is RpcException call ? Some(call) : None)
            .Match(
                Some: call => WireFault.Decode(call, raised).Match(
                    Succ: decoded => decoded.Match(
                        Some: static fault => (Error)fault,
                        None: () => (Error)WireFault.Classify(call, raised)),
                    Fail: static fault => fault),
                None: () => raised);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WarmProbe {
    private static readonly Op WarmEdge = Op.Of(name: "wire.warm");

    public static readonly WarmProbe Connectivity = new("connectivity", observable: true, warm: static (services, _) =>
        IO.liftAsync(async envIO => await WarmEdge.Catch(
            async token => {
                await services.Channel.ConnectAsync(token).ConfigureAwait(false);
                return Fin.Succ(services);
            },
            envIO.Token).ConfigureAwait(false)));

    public static readonly WarmProbe RoundTrip = new("round-trip", observable: false, warm: static (services, call) =>
        IO.liftAsync(async envIO => {
            Fin<HealthCheckResponse> outcome = (await WarmEdge.Catch(
                async token => Fin.Succ(await call.Health.CheckAsync(
                    new HealthCheckRequest(), cancellationToken: token).ResponseAsync.ConfigureAwait(false)),
                envIO.Token).ConfigureAwait(false)).MapFail(RpcEdge.Rpc);
            return outcome.Match(
                Succ: _ => Fin.Succ(services),
                Fail: error => error is WireFault.Unreachable
                    ? Fin.Fail<WireServices>(error)
                    : Fin.Succ(services));
        }));

    public Func<WireServices, WireCall, IO<Fin<WireServices>>> Warm { get; }

    public bool Observable { get; }
}

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RemoteTransport {
    public static readonly RemoteTransport Http2 = new("http2", streams: [StreamShape.Unary, StreamShape.ServerStream, StreamShape.ClientStream, StreamShape.Bidi], credentials: Seq(CredentialPolicy.Tls, CredentialPolicy.Mtls, CredentialPolicy.Bearer, CredentialPolicy.Composed), affinity: true, probe: WarmProbe.Connectivity, dial: static endpoint => Fin.Succ(GrpcChannel.ForAddress(endpoint.Address, WireChannels.Canonical(endpoint))));
    public static readonly RemoteTransport Http3 = new("http3", streams: [StreamShape.Unary, StreamShape.ServerStream, StreamShape.ClientStream, StreamShape.Bidi], credentials: Seq(CredentialPolicy.Tls, CredentialPolicy.Mtls, CredentialPolicy.Composed), affinity: true, probe: WarmProbe.Connectivity, dial: static endpoint => HttpVersionPosture.QuicCapable ? Fin.Succ(GrpcChannel.ForAddress(endpoint.Address, WireChannels.Canonical(endpoint))) : Fin.Fail<GrpcChannel>(new HopFault.Excluded(nameof(Http3))));
    public static readonly RemoteTransport GrpcWeb = new("grpc-web", streams: [StreamShape.Unary, StreamShape.ServerStream], credentials: Seq(CredentialPolicy.Bearer, CredentialPolicy.Tls), affinity: false, probe: WarmProbe.RoundTrip, dial: static endpoint => Fin.Succ(GrpcChannel.ForAddress(endpoint.Address, WireChannels.Web(endpoint))));
    public static readonly RemoteTransport UnixDomainSocket = new("uds", streams: [StreamShape.Unary, StreamShape.ServerStream, StreamShape.ClientStream, StreamShape.Bidi], credentials: Seq(CredentialPolicy.InsecureLoopback), affinity: false, probe: WarmProbe.RoundTrip, dial: static endpoint => endpoint.Peer.ToFin(new HopFault.StaleManifest(endpoint.Address.AbsoluteUri)).Map(static peer => Discovery.Connect(peer, GrpcChannelPolicy.Canonical)));
    public static readonly RemoteTransport InProcess = new("in-process", streams: [StreamShape.Unary, StreamShape.ServerStream, StreamShape.ClientStream, StreamShape.Bidi], credentials: Seq(CredentialPolicy.InsecureLoopback), affinity: false, probe: WarmProbe.RoundTrip, dial: static endpoint => endpoint.Handler.ToFin(new HopFault.Excluded(nameof(InProcess))).Map(static handler => GrpcChannel.ForAddress(endpoint.Address, new GrpcChannelOptions { HttpHandler = handler() })));

    public Seq<StreamShape> Streams { get; }
    public Seq<CredentialPolicy> Credentials { get; }
    public bool Affinity { get; }
    public WarmProbe Probe { get; }
    public Func<ComputeEndpoint, Fin<GrpcChannel>> Dial { get; }

    public bool Carries(StreamShape shape) => Streams.Contains(shape);

    public Seq<StreamShape> Uncarried(Seq<StreamShape> needs) => needs.Filter(shape => !Carries(shape));
}

[SmartEnum]
public sealed partial class NodeSelection {
    public static readonly NodeSelection RoundRobin = new();
    public static readonly NodeSelection LeastLoaded = new();
    public static readonly NodeSelection ModelWarmupAffinity = new();

    public Fin<ComputeEndpoint> Select(Seq<ComputeEndpoint> endpoints, EndpointLoad loads, int rotation) =>
        endpoints.IsEmpty
            ? Fin.Fail<ComputeEndpoint>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)))
            : Fin.Succ(toSeq(endpoints.Zip(Enumerable.Range(0, endpoints.Count)))
                .Map(candidate => (Endpoint: candidate.First, Score: Score(candidate.First, candidate.Second, endpoints.Count, rotation, loads)))
                .OrderBy(static ranked => ranked.Score.Tier)
                .ThenBy(static ranked => ranked.Score.Load)
                .ThenBy(static ranked => ranked.Score.Rotation)
                .First().Endpoint);

    private (int Tier, double Load, int Rotation) Score(
        ComputeEndpoint endpoint, int ordinal, int count, int rotation, EndpointLoad loads) {
        int turn = (int)((((long)ordinal - rotation) % count + count) % count);
        return Switch(
            state: (Endpoint: endpoint, Turn: turn, Load: loads.At(endpoint.Address)),
            roundRobin: static state => (0, 0d, state.Turn),
            leastLoaded: static state => state.Load.Match(
                Some: measured => (0, measured, state.Turn),
                None: () => (1, 0d, state.Turn)),
            modelWarmupAffinity: static state => (
                state.Endpoint.WarmFingerprint.IsSome ? 0 : 1,
                state.Load.IfNone(0d),
                state.Turn));
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class WireChannels {
    public static GrpcChannelOptions Canonical(ComputeEndpoint endpoint) => new() {
        Credentials = endpoint.Credential.Channel(endpoint.Mints),
        CompressionProviders = CompressionProviders.Register,
        MaxSendMessageSize = GrpcChannelPolicy.Canonical.MaxSendBytes, MaxReceiveMessageSize = GrpcChannelPolicy.Canonical.MaxReceiveBytes,
        DisableResolverServiceConfig = true,
        InitialReconnectBackoff = GrpcChannelPolicy.Canonical.InitialReconnectBackoff,
        MaxReconnectBackoff = GrpcChannelPolicy.Canonical.MaxReconnectBackoff,
        HttpVersion = GrpcChannelPolicy.Canonical.Version.Wire.Version, HttpVersionPolicy = GrpcChannelPolicy.Canonical.Version.Wire.Policy,
        HttpHandler = new SocketsHttpHandler {
            PooledConnectionIdleTimeout = GrpcChannelPolicy.Canonical.PooledConnectionIdle,
            KeepAlivePingDelay = GrpcChannelPolicy.Canonical.KeepAlivePingDelay,
            KeepAlivePingTimeout = GrpcChannelPolicy.Canonical.KeepAlivePingTimeout,
            KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests,
            EnableMultipleHttp2Connections = GrpcChannelPolicy.Canonical.EnableMultipleHttp2Connections,
            SslOptions = {
                ClientCertificates = endpoint.ClientCertificate.Match(
                    Some: static cert => new X509CertificateCollection { cert },
                    None: static () => new X509CertificateCollection()),
            },
        },
    };

    public static GrpcChannelOptions Web(ComputeEndpoint endpoint) => new() {
        Credentials = endpoint.Credential.Channel(endpoint.Mints),
        HttpVersion = HttpVersion.Version11, HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact,
        MaxSendMessageSize = GrpcChannelPolicy.Canonical.MaxSendBytes, MaxReceiveMessageSize = GrpcChannelPolicy.Canonical.MaxReceiveBytes,
        HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, endpoint.Handler.IfNone(static () => new HttpClientHandler())()),
    };

    public static Fin<ComputeEndpoint> Attach(ProfileRoots roots, int pid, JsonTypeInfo<DiscoveryManifest> contract, CorrelationId correlation) =>
        Discovery.Read(roots, pid, contract)
            .Bind(peer => ContractGeneration.Compute.Bind(local => Discovery.Compatible(peer, local)))
            .Map(peer => new ComputeEndpoint(
                new UriBuilder(Uri.UriSchemeHttp, "localhost").Uri, RemoteTransport.UnixDomainSocket, CredentialPolicy.InsecureLoopback,
                correlation, Needs: Seq(StreamShape.Unary, StreamShape.ServerStream, StreamShape.ClientStream, StreamShape.Bidi), Peer: peer));

    public static ComputeEndpoint InMemory(Func<HttpMessageHandler> handler, CorrelationId correlation, Seq<StreamShape> needs) =>
        new(new UriBuilder(Uri.UriSchemeHttp, "localhost").Uri, RemoteTransport.InProcess, CredentialPolicy.InsecureLoopback,
            correlation, Needs: needs, Handler: Some(handler));

    public static ComputeEndpoint WarmAffinity(ComputeEndpoint endpoint, FrozenSet<string> nodeWarmBlobs, string warmStartFingerprint) =>
        endpoint.Transport.Affinity && nodeWarmBlobs.Contains(warmStartFingerprint)
            ? endpoint with { WarmFingerprint = Some(warmStartFingerprint) }
            : endpoint;

    public static Fin<ComputeEndpoint> Admit(ComputeEndpoint endpoint) =>
        (Credentialed(endpoint), Shaped(endpoint), Certified(endpoint))
            .Apply((_, _, _) => endpoint)
            .As().ToFin();

    private static Validation<Error, Unit> Credentialed(ComputeEndpoint endpoint) =>
        endpoint.Transport.Credentials.Contains(endpoint.Credential)
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new HopFault.Excluded($"<credential-unadmitted:{endpoint.Transport.Key}:{endpoint.Credential.Key}>"));

    private static Validation<Error, Unit> Shaped(ComputeEndpoint endpoint) =>
        endpoint.Transport.Uncarried(endpoint.Needs) is { IsEmpty: false } missing
            ? Validation<Error, Unit>.Fail(new HopFault.Excluded($"<shape-uncarried:{endpoint.Transport.Key}:{missing.Count}>"))
            : Validation<Error, Unit>.Success(unit);

    private static Validation<Error, Unit> Certified(ComputeEndpoint endpoint) =>
        !endpoint.Credential.MutualAuth || endpoint.ClientCertificate.IsSome
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new HopFault.Excluded($"<mutual-auth-uncertified:{endpoint.Address.AbsoluteUri}>"));

    public static IO<Fin<WireServices>> Open(ComputeEndpoint endpoint, CallSpine spine) =>
        Admit(endpoint).Bind(admitted => admitted.Transport.Dial(admitted)).Match(
            Succ: channel => {
                WireServices services = WireServices.Of(channel.CreateCallInvoker(), channel);
                return endpoint.Transport.Probe.Warm(services, services.Bind(spine));
            },
            Fail: error => IO.pure(Fin.Fail<WireServices>(error)));

    public static IO<Unit> Observe(ComputeEndpoint endpoint, GrpcChannel channel, Func<WireTransition, IO<Unit>> record) =>
        endpoint.Transport.Probe.Observable ? Pump(channel, channel.State, record) : IO.pure(unit);

    public static IO<Fin<WireServices>> Redial(ComputeEndpoint endpoint, WireServices stale, CallSpine spine, Func<DiscoveryManifest, Fin<DiscoveryManifest>> rehandshake) =>
        endpoint.Peer.ToFin(new HopFault.StaleManifest(endpoint.Address.AbsoluteUri))
            .Bind(rehandshake)
            .Match(
                Succ: peer => Open(endpoint with { Peer = peer }, spine)
                    .Bind(opened => opened.Match(
                        Succ: _ => IO.lift(fun(stale.Dispose)).Map(_ => opened),
                        Fail: _ => IO.pure(opened))),
                Fail: error => IO.pure(Fin.Fail<WireServices>(error)));

    private static IO<Unit> Pump(GrpcChannel channel, ConnectivityState prior, Func<WireTransition, IO<Unit>> record) =>
        IO.liftAsync(async () => { await channel.WaitForStateChangedAsync(prior).ConfigureAwait(false); return channel.State; })
            .Bind(next => record(WireTransition.Of(prior, next)).Map(_ => WireTransition.Of(prior, next)))
            .Bind(transition => transition.Absorbing
                ? IO.pure(unit)
                : Pump(channel, channel.State, record));
}
```

```mermaid
sequenceDiagram
    accTitle: Wire channel discovery, contract guard, and call-spine interception
    accDescr: Wire channels read the discovery manifest, prove the peer advertises the local contract generation, accumulate the three admission columns, connect a warmed channel, and intercept the call spine.
    participant WireChannels
    participant Discovery
    participant WireServices
    participant CallSpine
    WireChannels->>Discovery: Read
    Discovery-->>WireChannels: DiscoveryManifest
    WireChannels->>Discovery: Compatible
    Discovery-->>WireChannels: generation match
    WireChannels->>WireChannels: Admit accumulate
    WireChannels->>Discovery: Connect
    Discovery-->>WireChannels: GrpcChannel
    WireChannels->>WireServices: Of raw invoker and channel
    WireServices->>CallSpine: Bind boot correlation
    CallSpine-->>WireServices: WireCall
    WireChannels->>WireChannels: WarmProbe warm
```

## [03]-[CALL_POLICY]

- Owner: `CredentialPolicy` `[SmartEnum<string>]` rows projecting `ChannelCredentials` and minting per-call identity through `AsyncAuthInterceptor`; `CompressionProviders` `[SmartEnum<string>]` the claim-gated encoding axis projecting inbox `ICompressionProvider` rows; `CallSpineFactory` — the composition-safe mint over the settled `ClockPolicy`; `CallSpine` — the one logical-call interceptor stamping its explicit correlation, the `DeadlineClass.HopTotal` budget, and the per-call compression and credential edges across all five client call shapes, plus the deadline, payload, and awaited-fault edges; the distributed-trace carrier is stamped by the spine's own propagation owner through `TraceContext.Inject`, never by a key this interceptor spells.
- Cases: InsecureLoopback (UnixDomainSocket-scoped), Tls, Mtls (the `MutualAuth` row whose `ComputeEndpoint.ClientCertificate` threads onto the handler `SslOptions.ClientCertificates` so the channel presents a client certificate at the TLS layer while `Channel` stays `ChannelCredentials.SecureSsl`), Bearer (browser; per-call token minted through `CallCredentials.FromInterceptor(AsyncAuthInterceptor)` reading the `AuthInterceptorContext.ServiceUrl`/`MethodName` and composed onto the channel through `ChannelCredentials.Create`), Composed (farm node dialing a hub; ≥2 per-call identity mints stacked through `CallCredentials.Compose(params CallCredentials[])` and bound to the TLS channel through `ChannelCredentials.Create`, a single-mint sequence collapsing to the bare `FromInterceptor` bind and an empty sequence to the plain `SecureSsl` channel). `CompressionProviders` rows: Identity (the default no-op `"identity"` accept-encoding), Gzip (`GzipCompressionProvider`), Deflate (`DeflateCompressionProvider` wrapping `ZLibStream` for zlib framing). `CallSpine` interceptor overrides: `BlockingUnaryCall`, `AsyncUnaryCall`, `AsyncServerStreamingCall`, `AsyncClientStreamingCall`, `AsyncDuplexStreamingCall` — the full `Grpc.Core.Interceptors.Interceptor` client family, one `Stamped` projection feeding every shape.
- Entry: `CallSpineFactory.Create(CorrelationId)` mints exactly one spine per logical call from the root-settled clock policy; `Options(AdmittedIntent intent, CancellationToken token)` projects an admitted compute deadline onto `CallOptions`; `Options(CancellationToken token)` carries caller cancellation for typed service calls outside the compute-dispatch algebra and lets the interceptor apply the canonical `DeadlineClass.HopTotal` bound; `Bounded` checks `CalculateSize` before serialization; token-aware `Awaited(Func<Task<T>>, CancellationToken)` captures through `Op.Catch` before folding a raised `RpcException` through `RpcEdge.Rpc`; `AwaitedHttp` owns the same budget and linked token for the REST leg `Runtime/ingest#REST_INGEST` composes; `WithIdentity` binds a fresh per-call credential.
- Law: the hop budget is the ROW's own bound. `DeadlineClass.HopTotal` carries `Bound` as the gauge lane it declares, so a `Func<DeadlineClass, TimeSpan> allotted` parameter reconstructs from the row it is handed and is the knob the removal test deletes. NAMED LOSS: a composition can no longer widen one call's budget without a row edit. Witness: two call sites resolving one deadline through two supplied delegates could disagree about `hop-total` while both read correct in isolation.
- Law: temporal capability arrives as the one `ClockPolicy` record. The semantic instant the deadline projects from is NodaTime's `IClock`, the elapsed evidence is the kernel `MonotonicTimeline` this record already minted off the provider at the app root, and a raw `TimeProvider.GetTimestamp`/`GetElapsedTime` pair below that root is the deleted form because a second timeline's stamps order against nothing the first produced.
- Law: cancellation and expiry are DISTINCT refusals. gRPC status maps `Cancelled` and `DeadlineExceeded` separately; the REST leg keeps a dedicated budget source beside caller/runtime cancellation, so only the budget-only trip refines captured `KernelFault.Cancelled` into cause-bearing `WireFault.DeadlineExpired`. NAMED LOSS: one arm became three. Witness: the single-arm form reported a user cancel as a deadline expiry, which is the one classification the re-drive rail reads in reverse.
- Auto: every generated stub call crosses the interceptor — correlation metadata, the injected W3C carrier, the budgeted deadline, and the per-call `RemoteReply` capture stamp without hand-threaded Metadata; the same `Stamped` projection runs for blocking unary, async unary, server-stream, client-stream, and duplex because the four request-and-context arities all route through one context rewrite.
- Output: `RemoteReply` — transport row, method, the peer's status name, request and response byte sizes, deadline outcome, and the measured wall — the remote arm's own settled value the dispatch spine returns as `ComputeOutput.Remote`.
- Packages: Grpc.Core.Api (`Interceptor`, `CallOptions`, `CallCredentials`, `AsyncAuthInterceptor`, `Metadata`, `RpcException`), Grpc.Net.Client, Grpc.Net.Common (inbox `Grpc.Net.Compression.ICompressionProvider`/`GzipCompressionProvider`/`DeflateCompressionProvider`), Google.Protobuf, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project — `DeadlineClass`, `ClockPolicy`, `TraceContext`), BCL inbox (`System.IO.Compression.CompressionLevel`)
- Growth: one credential row per new trust shape (Composed stacks N identity mints, never a new surface); one `CompressionProviders` row per new wire encoding; a custom zstd/brotli codec is one `CompressionProviders` row whose `Provider` returns a host-implemented `ICompressionProvider` projecting the new `EncodingName`, never a package admission; the compression flip resolves through `CompressionProviders.Winning(payloadBytes, substrate, host, claims)` which folds the `BenchmarkClaim` rows of the `wire-compression` family, matches the running `HostFingerprint` and the payload `Band`, reads the winning `Route`-keyed row, and drops the `Identity` no-op, then `CallSpine.Compressed` stamps the per-call `grpc-internal-encoding-request` metadata key with the winning `Key` against the channel-side registration `CompressionProviders.Register` materializes from the axis rows — the winning encoding is a claim-gated `Option<CompressionProviders>`, so an absent or stale claim leaves the call uncompressed and a per-call default-on knob is the deleted form; zero new surface.
- Boundary: `Options` reads the admitted `DeadlineAt`; raw deadline parameters never cross the client edge. `CallSpineFactory` is singleton-safe because it retains only the immutable composition `ClockPolicy`; each call supplies its own `CorrelationId` to `Create`, so neither dependency injection nor an `Op`-derived surrogate can freeze correlation across calls. `Budgeted` applies the `DeadlineClass.HopTotal` row bound only to interceptor calls that lack admitted intent evidence. `Awaited` classifies once through the ONE `RpcEdge.Rpc` funnel, while `CredentialPolicy.Mint` creates each bearer token per call. `DisableResolverServiceConfig` excludes resolver retry, hedging, and load balancing, and AppHost remains the one hop retry owner. `grpc-internal-encoding-request` selects only a provider registered on `GrpcChannelOptions.CompressionProviders`. Propagation is the spine's seam whole: `TraceContext.Inject(Metadata)` writes every field the registered composite propagator declares, so a `traceparent` const, a `tracestate` twin, and a `Func<string>` handing this interceptor a pre-rendered header are all deleted forms — each stamps one propagator's shape at one moment and silently drops whatever the composite gains afterward. Every protocol header name this interceptor spells is a `const` beside its siblings, because a default-parameter string literal is the same coordinate spelled where no reader can find it. [SPIKE]: the `Composed` row's dial through a running plugin channel converges on live-ALC evidence alone; the deterministic floor is that row's stacked `CallCredentials.Compose` shape and its `ChannelCredentials.Create` composition seam, both settled here.

```csharp
// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CredentialPolicy {
    public static readonly CredentialPolicy InsecureLoopback = new("insecure-loopback", channel: static _ => ChannelCredentials.Insecure, mutualAuth: false);
    public static readonly CredentialPolicy Tls = new("tls", channel: static _ => ChannelCredentials.SecureSsl, mutualAuth: false);
    public static readonly CredentialPolicy Mtls = new("mtls", channel: static _ => ChannelCredentials.SecureSsl, mutualAuth: true);
    public static readonly CredentialPolicy Bearer = new("bearer", mutualAuth: false, channel: static mints => mints.Head.Match(
        Some: static mint => ChannelCredentials.Create(ChannelCredentials.SecureSsl, CallCredentials.FromInterceptor(mint)),
        None: static () => ChannelCredentials.SecureSsl));
    public static readonly CredentialPolicy Composed = new("composed", mutualAuth: false, channel: static mints => mints.Match(
        Empty: static () => ChannelCredentials.SecureSsl,
        Head: static mint => ChannelCredentials.Create(ChannelCredentials.SecureSsl, CallCredentials.FromInterceptor(mint)),
        Tail: static (head, tail) => ChannelCredentials.Create(
            ChannelCredentials.SecureSsl,
            CallCredentials.Compose(head.Cons(tail).Map(CallCredentials.FromInterceptor).ToArray()))));

    public Func<Seq<AsyncAuthInterceptor>, ChannelCredentials> Channel { get; }
    public bool MutualAuth { get; }

    public static AsyncAuthInterceptor Mint(Func<AuthInterceptorContext, CancellationToken, ValueTask<string>> token) =>
        async (context, metadata) => metadata.Add(CallSpine.AuthorizationKey, $"Bearer {await token(context, context.CancellationToken).ConfigureAwait(false)}");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CompressionProviders {
    public static readonly CompressionProviders Identity = new("identity", provider: static () => Option<ICompressionProvider>.None);
    public static readonly CompressionProviders Gzip = new("gzip", provider: static () => Some<ICompressionProvider>(new GzipCompressionProvider(CompressionLevel.Fastest)));
    public static readonly CompressionProviders Deflate = new("deflate", provider: static () => Some<ICompressionProvider>(new DeflateCompressionProvider(CompressionLevel.Fastest)));

    public Func<Option<ICompressionProvider>> Provider { get; }

    public string EncodingName => Provider().Match(Some: static p => p.EncodingName, None: static () => Key);

    public static IList<ICompressionProvider> Register => toSeq(Items).Choose(static row => row.Provider()).ToList();

    public const string ClaimFamily = "wire-compression";

    public static Option<CompressionProviders> Winning(long payloadBytes, Substrate substrate, HostFingerprint host, Seq<BenchmarkClaim> claims) =>
        claims.Find(claim =>
                claim.Family == ClaimFamily && claim.Substrate == substrate && !claim.Stale(host) &&
                claim.Input.Band == BenchmarkClaim.BandOf(payloadBytes))
            .Bind(static claim => TryGet(claim.Route, out CompressionProviders? row) && row is not null ? Some(row) : None)
            .Filter(static row => row != Identity);
}

// --- [MIDDLEWARE] ----------------------------------------------------------------------
public sealed record RemoteReply(
    RemoteTransport Transport, string Method, string Status, long RequestBytes, long ResponseBytes, DeadlineOutcome Outcome, Duration Elapsed);

public sealed class CallSpineFactory(ClockPolicy clocks) {
    public CallSpine Create(CorrelationId correlation) => new(correlation, clocks);
}

public sealed class CallSpine(CorrelationId correlation, ClockPolicy clocks) : Interceptor {
    public const string CorrelationKey = "rasm-correlation";
    public const string RequestEncodingKey = "grpc-internal-encoding-request";
    public const string AuthorizationKey = "authorization";
    private static readonly Op GrpcAwait = Op.Of(name: "wire.grpc-await");
    private static readonly Op HttpAwait = Op.Of(name: "wire.http-await");

    public CorrelationId Correlation => correlation;

    public CallOptions Options(AdmittedIntent intent, CancellationToken token) =>
        new CallOptions()
            .WithDeadline(intent.DeadlineAt.ToDateTimeUtc())
            .WithCancellationToken(token);

    public CallOptions Options(CancellationToken token) =>
        new CallOptions().WithCancellationToken(token);

    public static CallOptions WithIdentity(CallOptions options, AsyncAuthInterceptor mint) =>
        options.WithCredentials(CallCredentials.FromInterceptor(mint));

    public static CallOptions Compressed(CallOptions options, Option<CompressionProviders> winningEncoding) =>
        winningEncoding.Match(
            Some: encoding => options.WithHeaders(Merge(options.Headers, new Metadata { { RequestEncodingKey, encoding.Key } })),
            None: () => options);

    public static Fin<T> Bounded<T>(T message) where T : IMessage<T> {
        int bytes = message.CalculateSize();
        return bytes <= GrpcChannelPolicy.Canonical.MaxSendBytes
            ? Fin.Succ(message)
            : Fin.Fail<T>(new ComputeFault.PayloadOverBounds($"<payload-over-bounds:{bytes}:{GrpcChannelPolicy.Canonical.MaxSendBytes}>"));
    }

    public static IO<Fin<T>> Awaited<T>(Func<Task<T>> call, CancellationToken token) =>
        IO.liftAsync(async _ => (await GrpcAwait.Catch(
            async _ => Fin.Succ(await call().ConfigureAwait(false)),
            token).ConfigureAwait(false)).MapFail(RpcEdge.Rpc));

    public IO<Fin<T>> AwaitedHttp<T>(string subject, CancellationToken token, Func<string, CancellationToken, Task<Fin<T>>> exchange) =>
        IO.liftAsync(async envIO => {
            using CancellationTokenSource budget = new(DeadlineClass.HopTotal.Bound);
            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(token, envIO.Token, budget.Token);
            Fin<T> outcome = await HttpAwait.Catch(
                async active => await exchange(subject, active).ConfigureAwait(false),
                linked.Token).ConfigureAwait(false);
            return outcome.MapFail(error =>
                budget.IsCancellationRequested && !token.IsCancellationRequested && !envIO.Token.IsCancellationRequested
                    && error is KernelFault.Cancelled cancelled
                    ? new WireFault.DeadlineExpired(subject, cancelled.Cause)
                    : error);
        });

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation) => continuation(request, Stamped(context));
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation) => continuation(request, Stamped(context));
    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation) => continuation(request, Stamped(context));
    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation) => continuation(Stamped(context));
    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation) => continuation(Stamped(context));

    private ClientInterceptorContext<TRequest, TResponse> Stamped<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context) where TRequest : class where TResponse : class =>
        new(context.Method, context.Host,
            Budgeted(context.Options)
                .WithHeaders(TraceContext.Inject(Merge(context.Options.Headers, new Metadata { { CorrelationKey, correlation.ToString() } }))));

    private CallOptions Budgeted(CallOptions options) =>
        options.Deadline is not null
            ? options
            : options.WithDeadline(clocks.Now.ToDateTimeUtc() + DeadlineClass.HopTotal.Bound);

    private static Metadata Merge(Metadata? existing, Metadata stamped) =>
        toSeq(existing ?? Metadata.Empty).Fold(stamped, static (acc, entry) => { acc.Add(entry); return acc; });
}
```

## [04]-[ARTIFACT_FRAMES]

- Owner: `FrameEdge` owns frame size, whole-artifact SHA-256 identity, buffer parsing, arrival-order reassembly admission, the `Put`/`Fetch` drivers, the explicit copy-granted raw-artifact projection, partial updates, and the pool hand-off over the settled `Tensor/memory#STREAM_POOL` singleton; `WireLimits` the ONE hostile-parse ceiling record per consumer folder, its rows the bounds every `CreateWithLimits` parse on this branch reads; `FrameSeed` the one typed source `WireFrames.Frame` transcribes onto `ArtifactFrame`; `FramePartition` the zero-copy outbound slices and `FrameCopy` the admitted inbound flatten; `WireFrames` that mapper.
- Cases: `WireLimits` rows — `Artifact` (the reassembled-artifact ceiling the pool rents, recursion 100) and `Inbound` (the channel receive cap, recursion 100).
- Law: `ArtifactFrame` carries `payload=3 bytes(1..65536)` and required `artifact=4 ArtifactRef`; `FetchRequest` binds its `sha256=1 bytes(32)` directly, while `FetchResponse.frame`, `PutRequest.frame`, and `PutResponse.artifact` are the exact RPC-direction envelopes over that one payload law. Every extent this owner carries is `ulong`; a `long` extent and an `int64` prose spelling are two ways of not compiling against the descriptor.
- Law: artifact identity is exactly the 32 raw bytes of SHA-256 over the ordered payload. Streaming reads append each admitted chunk to one `IncrementalHash`; an already-held memory payload uses `SHA256.HashData`. No byte-order projection, algorithm tag, or semantic `ContentHash` enters the artifact identity. XXH3 remains the 16-byte semantic/cache identity used by request content keys and never addresses artifact bodies.
- Law: a frame carries NO offset and NO CRC. The Connect and gRPC server stream preserves message boundaries and arrival order; the whole-artifact digest alone proves application integrity because the complete artifact is the retry unit. A per-frame offset repeats stream order, while a per-frame `Crc32` adds no acceptance outcome. `Advance` proves id and extent agreement, caps every payload at 64 KiB, requires that width for every nonterminal frame, and accumulates exactly `artifact_bytes` in arrival order, bounding frame-object cardinality as well as bytes; live `Fetch` and local `Drain` each re-verify the whole-artifact digest. NAMED LOSS: a frame delivered out of order or with one corrupted byte is caught only at the whole-artifact digest, never at the frame. Witness: the stream orders, so the out-of-order case cannot arise, and the digest refusal names the artifact the peer must resend — exactly what the per-frame check had ultimately done.
- Exemption: the `Fetch` drain kernel is the platform-forced stream statement seam — the `await foreach` over the response stream holds the call open across `yield`, the pooled lease and hash accumulator ride one `try`/`finally`, and the per-frame refusal cell exists because a rail abort cannot leave the iterator any other way; every other body on this page is expression flow.
- Entry: `Frames` derives the artifact identity and partitions a staged stream into 64 KiB payloads, returning the partition copy's evidence with the frames; `Fetch` validates one generated `FetchRequest`, unwraps and validates each generated `FetchResponse`, writes its frame incrementally, proves terminal extent and hash, then returns the one copy-granted `FrameCopy`; `Put` validates the shared frames, wraps each in `PutRequest`, and admits the nested `PutResponse.artifact`; `Reassemble` admits and parses an already-held protobuf frame run; `Bytes` admits and hash-verifies an already-held raw frame run, then returns its exact-size `ReadOnlyMemory<byte>` with the copy grant its caller settles through `Tensor/memory#STREAM_POOL` `StreamPool.Stamp`; `Patch` unions, normalizes, validates, and merges field masks; `WireFrames.Frame` transcribes one `FrameSeed`.
- Law: inbound flattening returns its allocation evidence to the caller that settles it; outbound framing wraps the caller-owned memory and allocates no second artifact body; `StreamPool` alone owns recyclable-manager events, typed `AllocationEvidence`, and subscription detachers.
- Growth: a new wire column on either message is one field on its seed record and one generated member — the mapper breaks until both agree, where the hand initializer landed the column twice or silently once; a new parse ceiling is one `WireLimits` row; artifact identity remains fixed SHA-256 and gains no policy surface; zero new surface.
- Boundary: `Fetch`, `Frames`, `Reassemble`, and `Bytes` drive buffer custody through the one `StreamPool`; direct `RecyclableMemoryStreamManager` construction and duplicate event wiring never enter this owner. Live `Fetch` retains no frame roster: the first admitted frame fixes an extent under `WireLimits.Artifact`, rents one right-sized stream, and every subsequent frame validates before its bytes append. `Advance` is the one incremental admission both live fetch and local reassembly fold, rejecting invalid generated messages, mixed identity or extent, fragmented nonterminal frames, and over-length runs before the offending frame is copied; terminal admission proves exact length and whole-artifact digest. The protobuf arm parses under `WireLimits.Artifact` — the ONE ceiling over a reassembled artifact, so `Runtime/wire#PROTO_VOCABULARY` `ParseGuard` (whose row is `WireLimits.Inbound`, the channel receive cap) does NOT gate this parse: frames exist because the artifact exceeds that cap, and composing it here would refuse every artifact the frame law was written for. A raw artifact flattens only after `AllocationClass.EdgeCopy.Grant` admits the exact extent and copy reason; the returned `AllocationEvidence` is that allocation grant, not a second copy record. `WireLimits.Artifact.SizeLimit` IS the pool's `StreamPoolPolicy.Canonical.MaximumStreamCapacity` because every drain rents from that pool — one authority, never a second literal. `Fetch` runs whole under `CallSpine.Awaited`, so a raised `RpcException` on the server stream classifies through the one `RpcEdge.Rpc` funnel and both call and pool lease dispose on every exit. `Patch` validates the normalized `FieldMask` before `Merge`, and `Normalize` dedupes and prunes subpaths without sorting, so no consumer reads an order off the mask. `WireFrames` is the ONE construction of the frame message, and a hand field-by-field initializer beside it is the deleted twin.

Frame-admission vectors use `W = FrameBytes`, `L = WireLimits.Artifact.SizeLimit`, `A = hash(body)`, and `B != A`:

| [INDEX] | [VECTOR]  | [ARRIVAL]                                                              | [TYPED_RESULT]                                    |
| :-----: | :-------- | :--------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | empty     | no frames                                                              | `fetch-frame-empty`                               |
|  [02]   | oversize  | `(A, L + 1, one byte)`                                                 | `frame-artifact-extent` before pool rent          |
|  [03]   | truncated | `(A, W + 1, W bytes)`, then stream close                               | `fetch-frame-length`                              |
|  [04]   | overrun   | `(A, W, W bytes)`, then `(A, W, one byte)`                             | `frame-overrun` before the extra byte writes      |
|  [05]   | bad hash  | `(A, body.Length, body)` where `SHA-256(body) = B`                     | `artifact-identity`                               |
|  [06]   | bad width | `(A, W + 2, W - 1 bytes)`, then `(A, W + 2, three bytes)`              | `frame-nonterminal-width` before frame two writes |
|  [07]   | admitted  | `W`-byte frames, one `1..W`-byte terminal, exact extent, matching hash | one `FrameCopy` and its `EdgeCopy` evidence       |

```csharp
using System.Security.Cryptography;
using Google.Protobuf;
// Contracts are retired from this logic.

// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record WireLimits(int SizeLimit, int RecursionLimit) {
    public const int Recursion = 100;

    public static readonly WireLimits Artifact = new(checked((int)StreamPoolPolicy.Canonical.MaximumStreamCapacity), Recursion);
    public static readonly WireLimits Inbound = new(GrpcChannelPolicy.Canonical.MaxReceiveBytes, Recursion);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FrameSeed(ArtifactRef Artifact, ReadOnlyMemory<byte> Body);

public sealed record FramePartition(Seq<ArtifactFrame> Frames);

public sealed record FrameCopy(ReadOnlyMemory<byte> Payload, AllocationEvidence Evidence);

internal readonly record struct FrameState(ByteString Sha256, ulong Extent, ulong Received, ulong Previous);

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper]
public static partial class WireFrames {
    [MapProperty(nameof(FrameSeed.Body), nameof(ArtifactFrame.Payload), Use = nameof(Wrapped))]
    public static partial ArtifactFrame Frame(FrameSeed seed);

    private static ByteString Wrapped(ReadOnlyMemory<byte> body) => UnsafeByteOperations.UnsafeWrap(body);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameEdge {
    public const int FrameBytes = 64 * 1024;

    public static readonly FieldMask.MergeOptions MergeReplace = new() { ReplaceMessageFields = true, ReplaceRepeatedFields = true };

    public static Fin<T> Patch<T>(T live, T update, params ReadOnlySpan<FieldMask> tiles) where T : class, IMessage<T> {
        if (tiles.IsEmpty) { return Fin.Fail<T>(new ComputeFault.PayloadOverBounds("<viewport-mask-empty>")); }
        FieldMask mask = tiles[0].Union(tiles[1..].ToArray()).Normalize();
        if (!FieldMask.IsValid(live.Descriptor, mask)) { return Fin.Fail<T>(new ComputeFault.PayloadOverBounds($"<patch-path-unknown:{string.Join(',', mask.Paths)}>")); }
        mask.Merge(update, live, MergeReplace);
        return Fin.Succ(live);
    }

    public static IO<Fin<FrameCopy>> Fetch(
        WireCall calls, CallSpine spine, StreamPool pool,
        ArtifactRef artifact, CancellationToken token) =>
        ParseGuard.Validated(new FetchRequest { Sha256 = artifact.Sha256 })
            .Bind(CallSpine.Bounded)
            .Match(
            Succ: request => CallSpine.Awaited(async () => {
                    using AsyncServerStreamingCall<FetchResponse> call =
                        calls.Artifacts.Fetch(request, spine.Options(token));
                    RecyclableMemoryStream? staged = null;
                    using IncrementalHash digest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                    try {
                        FrameState state = new(artifact.Sha256, artifact.ArtifactBytes, Received: 0UL, Previous: 0UL);
                        await foreach (FetchResponse response in call.ResponseStream.ReadAllAsync(token).ConfigureAwait(false)) {
                            Error? rejected = null;
                            ParseGuard.Validated(response)
                                .Bind(admitted => Advance(state, admitted.Frame)
                                    .Bind(next => (staged is RecyclableMemoryStream held
                                            ? Fin.Succ(held)
                                            : pool.Get(spine.Correlation, new StreamGrant.Sized((long)next.Extent)))
                                        .Map(buffer => {
                                            staged = buffer;
                                            buffer.Write(admitted.Frame.Payload.Span);
                                            digest.AppendData(admitted.Frame.Payload.Span);
                                            state = next;
                                            return unit;
                                        })))
                                .Match(
                                    Succ: static _ => unit,
                                    Fail: error => { rejected = error; return unit; });
                            if (rejected is not null) { return Fin.Fail<FrameCopy>(rejected); }
                        }
                        if (staged is null) {
                            return Fin.Fail<FrameCopy>(new ComputeFault.WireDecodeRejected("<fetch-frame-empty>"));
                        }
                        if (state.Received != state.Extent) {
                            return Fin.Fail<FrameCopy>(new ComputeFault.WireDecodeRejected(
                                $"<fetch-frame-length:{state.Received}:{state.Extent}>"));
                        }
                        byte[] rebuilt = digest.GetHashAndReset();
                        staged.Position = 0;
                        return artifact.Sha256.Span.SequenceEqual(rebuilt)
                            ? Copy(staged, spine.Correlation)
                            : Fin.Fail<FrameCopy>(new ComputeFault.WireDecodeRejected("<artifact-identity>"));
                    }
                    finally {
                        staged?.Dispose();
                    }
                }, token).Map(result => result.Bind(static admitted => admitted)),
            Fail: error => IO.pure(Fin.Fail<FrameCopy>(error)));

    public static IO<Fin<ArtifactRef>> Put(
        WireCall calls, CallSpine spine, FramePartition partition, CancellationToken token) =>
        Admit(partition.Frames).Match(
            Succ: admitted => CallSpine.Awaited(async () => {
                    using AsyncClientStreamingCall<PutRequest, PutResponse> call =
                        calls.Artifacts.Put(spine.Options(token));
                    foreach (ArtifactFrame frame in admitted) {
                        await call.RequestStream.WriteAsync(new PutRequest { Frame = frame }).ConfigureAwait(false);
                    }
                    await call.RequestStream.CompleteAsync().ConfigureAwait(false);
                    PutResponse response = await call.ResponseAsync.ConfigureAwait(false);
                    ArtifactRef expected = admitted.Head.Artifact;
                    return ParseGuard.Validated(response).Bind(valid =>
                        valid.Artifact.Sha256 == expected.Sha256
                        && valid.Artifact.ArtifactBytes == expected.ArtifactBytes
                            ? Fin.Succ(valid.Artifact)
                            : Fin.Fail<ArtifactRef>(new ComputeFault.WireDecodeRejected("<artifact-put-response>")));
                }, token).Map(result => result.Bind(static admitted => admitted)),
            Fail: error => IO.pure(Fin.Fail<ArtifactRef>(error)));

    public static Fin<T> Reassemble<T>(StreamPool pool, CorrelationId correlation, MessageParser<T> parser, Seq<ArtifactFrame> frames) where T : class, IMessage<T> =>
        Admit(frames).Bind(ordered => Drain(
            pool, correlation, ordered.Head.Artifact.Sha256, ordered,
            staged => pool.Read(staged, parser, WireLimits.Artifact)));

    public static Fin<FrameCopy> Bytes(StreamPool pool, CorrelationId correlation, Seq<ArtifactFrame> frames) =>
        Admit(frames).Bind(ordered => Drain(
            pool, correlation, ordered.Head.Artifact.Sha256, ordered,
            staged => Copy(staged, correlation)));

    public static Fin<FramePartition> Frames(ReadOnlyMemory<byte> payload) {
        if (payload.IsEmpty || payload.Length > WireLimits.Artifact.SizeLimit) {
            return Fin.Fail<FramePartition>(new ComputeFault.PayloadOverBounds($"<frame-bytes:{payload.Length}>"));
        }
        int segments = 1 + ((payload.Length - 1) / FrameBytes);
        ArtifactRef artifact = new() {
            Sha256 = ByteString.CopyFrom(SHA256.HashData(payload.Span)),
            ArtifactBytes = checked((ulong)payload.Length),
        };
        return Fin.Succ(new FramePartition(
            toSeq(Enumerable.Range(0, segments))
                .Map(static index => index * FrameBytes)
                .Map(offset => WireFrames.Frame(new FrameSeed(
                    artifact,
                    payload.Slice(offset, Math.Min(FrameBytes, payload.Length - offset))))))));
    }

    private static Fin<Seq<ArtifactFrame>> Admit(Seq<ArtifactFrame> frames) =>
        frames.Head.ToFin(new ComputeFault.WireDecodeRejected("<reassemble-empty>"))
            .Bind(head => frames.Fold(
                Fin.Succ(new FrameState(head.Artifact.Sha256, head.Artifact.ArtifactBytes, Received: 0UL, Previous: 0UL)),
                static (state, frame) => state.Bind(admitted => Advance(admitted, frame)))
                .Bind(state => state.Received == state.Extent
                    ? Fin.Succ(frames)
                    : Fin.Fail<Seq<ArtifactFrame>>(new ComputeFault.WireDecodeRejected(
                        $"<frame-length:{state.Received}:{state.Extent}>"))));

    private static Fin<FrameState> Advance(FrameState state, ArtifactFrame frame) =>
        ParseGuard.Validated(frame).Bind(admitted => {
            ulong payloadBytes = (ulong)admitted.Payload.Length;
            return admitted.Artifact.Sha256 != state.Sha256
                ? Fin.Fail<FrameState>(new ComputeFault.WireDecodeRejected("<frame-artifact-sha256>"))
                : admitted.Artifact.ArtifactBytes > (ulong)WireLimits.Artifact.SizeLimit
                    ? Fin.Fail<FrameState>(new ComputeFault.WireDecodeRejected(
                        $"<frame-artifact-extent:{admitted.Artifact.ArtifactBytes}:{WireLimits.Artifact.SizeLimit}>"))
                    : admitted.Artifact.ArtifactBytes != state.Extent
                        ? Fin.Fail<FrameState>(new ComputeFault.WireDecodeRejected(
                            $"<frame-extent-change:{state.Extent}:{admitted.Artifact.ArtifactBytes}>"))
                        : state.Received != 0UL && state.Previous != FrameBytes
                            ? Fin.Fail<FrameState>(new ComputeFault.WireDecodeRejected(
                                $"<frame-nonterminal-width:{state.Previous}:{FrameBytes}>"))
                            : payloadBytes > admitted.Artifact.ArtifactBytes - state.Received
                                ? Fin.Fail<FrameState>(new ComputeFault.WireDecodeRejected(
                                    $"<frame-overrun:{state.Received}:{admitted.Artifact.ArtifactBytes}:{payloadBytes}>"))
                                : Fin.Succ(new FrameState(
                                    state.Sha256, admitted.Artifact.ArtifactBytes,
                                    state.Received + payloadBytes, payloadBytes));
        });

    static Fin<FrameCopy> Copy(RecyclableMemoryStream staged, CorrelationId correlation) =>
        AllocationClass.EdgeCopy.Grant(new AllocationRequest(
                correlation,
                RequestedBytes: staged.Length,
                PayloadBound: WireLimits.Artifact.SizeLimit,
                Async: false,
                Mode: AllocationMode.Default,
                CopyReason: Some("artifact-frame-boundary"),
                NativeAllocator: None,
                NativeReservedBytes: None))
            .Bind(evidence => Op.Of(name: "frame.artifact-copy").Catch(() =>
                Fin.Succ(new FrameCopy(new ReadOnlyMemory<byte>(staged.ToArray()), evidence))));

    private static Fin<T> Drain<T>(
        StreamPool pool,
        CorrelationId correlation,
        ByteString sha256,
        Seq<ArtifactFrame> ordered,
        Func<RecyclableMemoryStream, Fin<T>> project) =>
        pool.Get(correlation, new StreamGrant.Sized((long)ordered.Head.Artifact.ArtifactBytes)).Bind(staged => {
            using (staged) {
                using IncrementalHash digest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                ordered.Iter(frame => {
                    staged.Write(frame.Payload.Span);
                    digest.AppendData(frame.Payload.Span);
                });
                byte[] rebuilt = digest.GetHashAndReset();
                staged.Position = 0;
                return sha256.Span.SequenceEqual(rebuilt)
                    ? project(staged)
                    : Fin.Fail<T>(new ComputeFault.WireDecodeRejected("<artifact-identity>"));
            }
        });
}
```
