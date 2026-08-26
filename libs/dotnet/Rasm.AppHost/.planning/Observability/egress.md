# [APPHOST_DURABLE_EGRESS]

Durable OTLP egress owns branch TRANSPORT beneath the exporter: a failed export batch lands on disk, answers `Accepted` once it is there, and replays through the next request that proved the collector good. The page owns the disposition vocabulary every queue outcome projects, the policy row that arms a queue, the blob queue itself, the mutual-auth material a replaced client factory must re-mount, and the delegating handler that routes both of the exporter's send legs. It is one directed edge off `Observability/telemetry#SIGNAL_GOVERNANCE` — `Egress` hands the exporter a factory and reads the opened queue set — and nothing here reads back into signal governance.

Settled composition: `TelemetrySignal`, `TelemetryComposition`, and `TelemetryFault` arrive from Observability/telemetry#SIGNAL_GOVERNANCE; `AppHostMeasure.OtlpOffline`/`OtlpOfflineBytes` and the composition `InstrumentSet` from Observability/instruments; `DeadlineClass.OtlpDrain` and `ClockPolicy.Line` from Runtime/time; `MonotonicTimeline`, `GaugedSpan`, `Op`, and `Cell` from the kernel. Endpoint, headers, and mutual-auth material stay deploy-plane `OTEL_EXPORTER_OTLP_*` rows; protocol and compression ride `Observability/instruments#PROVIDER_LIFETIME` `ProviderProgram`.

## [01]-[INDEX]

- [02]-[DURABLE_EGRESS]: Disposition vocabulary, arming policy, the bounded drain fold, transport trust, and the two-leg persistent handler.

## [02]-[DURABLE_EGRESS]

- Owner: `OfflineDisposition` `[SmartEnum<string>]` the six outcomes a queue can observe; `OtlpOfflinePolicy` the arming row and its queue-set mint; `DrainPass` `[Union]` one drain pass's outcome; `OtlpOfflineQueue` the per-signal blob queue with its accept and drain legs; `OtlpTrust` the mutual-auth material a replaced transport re-reads; `PersistentOtlpHandler` the delegating handler routing both exporter send legs.
- Cases: six dispositions cover every outcome the queue can observe — accept, capacity refusal, replay, deferral, corruption, and the bounded-drain exit; two drain-pass cases split a tail that moved from one that settled, and the settled case carries its disposition as an `Option` because an empty queue and a head another export holds are the ABSENCE of an outcome rather than an outcome.
- Entry: `OtlpOfflinePolicy.For(ResolvedProfile resolved)` returns the armed or absent policy; `OtlpOfflinePolicy.Open(InstrumentSet signals, MonotonicTimeline line)` mints one queue per exported signal at composition over the mounted set it writes through; `OtlpOfflineQueue.Accept(HttpRequestMessage, CancellationToken)` stores a failed batch and answers `Accepted`; `OtlpOfflineQueue.Drain(HttpRequestMessage template, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken)` replays the tail under the gauged drain bound; `OtlpTrust.Read()` reads the three environment rows and `OtlpTrust.Mount(SocketsHttpHandler)` mounts them; `PersistentOtlpHandler` overrides both `Send` and `SendAsync`.
- Law: draining is BOUNDED twice and neither bound is wall arithmetic. Batch bound is fold length, and time bound is the kernel `MonotonicTimeline` gauge over `DeadlineClass.OtlpDrain` — the lane row owns the duration, the timeline owns the cut, and the measured span owns the timeout disposition. A `time.GetUtcNow() + bound` deadline is the deleted form: it moves in either direction under an NTP correction on the one thread an export call is holding, and it left the bound spelled at a call site instead of on the deadline roster every other duration in the suite traces to.
- Auto: queued batches answer `Accepted` because durability is the contract this handler sells — the bytes are on disk and replay is owed — while a refusal answers a real transport failure so the exporter's own drop path runs and its self-diagnostics record the loss rather than a synthetic success hiding it; bodiless requests store nothing and replay nothing; the drain rides the proven-good live request as its replay template, so a rotated ingest credential and a moved endpoint both apply to the tail the moment the next export proves them and neither has ever been written to disk; replay order is the provider's, NEWEST blob first, so a queue held past its retention window sheds its OLDEST batches and the honest durability claim is a bounded recent tail, never a complete one; unreadable blobs DELETE rather than release, because a leased-and-failed head the maintenance timer keeps promoting back wedges every later batch behind it forever; one transient classifier serves both directions of the transport, so the live send and the replay cannot drift on whether a batch landed, and a token the caller tripped stays a raise because a cancelled export is the caller's own decision.
- Law: every disposition is COUNTED where it happens — the queue writes `AppHostMeasure.OtlpOffline` and `AppHostMeasure.OtlpOfflineBytes` under the signal and disposition dimensions at the accept and drain legs, so queue depth, replay rate, and corruption read off one population and no fact record stands beside the counter waiting for a projection.
- Packages: Rasm (kernel `MonotonicTimeline`, `Op`), OpenTelemetry.Extensions.PersistentStorage.FileSystem, OpenTelemetry.Exporter.OpenTelemetryProtocol, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new offline outcome is one `OfflineDisposition` row the one counter already partitions on and one arm on the drain fold's own pass carrier; a new queue bound is one `OtlpOfflinePolicy` column or one `DeadlineClass` row; a new transport-trust coordinate is one `OtlpTrust` row beside its governance variable; zero new surface.
- Boundary: durable egress has exactly ONE owner per exporter — the branch-typed queue installs through `OtlpExporterOptions.HttpClientFactory`, so the exporter's own `OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY=disk` pair stays unset wherever the handler leg is selected, and arming both gives one batch two independent persistence owners writing two directories with no shared accounting; a replaced factory DISPLACES the shipped one whole, and the shipped one is the sole application point for both the option timeout and the mutual-auth client the `OTEL_EXPORTER_OTLP_*` trust rows arm — `OtlpMtlsOptions` is internal at this pin — so the egress seat carries both halves and a durable profile exporting unauthenticated against a mutual-auth collector is the defect that row forecloses; durable-transport LIFETIME is the composition's, never the exporter's — the SDK hands its export client an `HttpClient` it never disposes and its shutdown only cancels pending requests, so neither the handler chain nor the provider directory reaches a release seat of its own and `TelemetryComposition.Dispose` at the telemetry drain band is the one seat closing both, which is also why the set opens at composition rather than inside an options delegate the SDK invokes past a sealed service collection; credential material never reaches disk because a stored blob carries the request BODY alone and the replay copies its headers off the live request that just succeeded, so a rotated ingest token applies to the whole tail and a stolen queue directory yields payloads and no key; queue DEPTH is the disposition ledger's own arithmetic — neither storage tier publishes a count or size accessor and its directory field is internal, so a depth level costs an O(n) directory walk per collection while the `queued`-minus-`replayed`-minus-`corrupt` gap answers the same question off counters already mounted; retention-expiry reclamation is the provider's own maintenance timer surfacing on the package `EventSource` alone, so an aged-out tail widens that same gap rather than minting a disposition row; plugin ALC capsules open no disk queue, so an unloaded capsule's failed batches die with it and never outlive the load context that minted them; one directory per signal, because the drain replays a blob through the live request that just proved the endpoint and a shared directory posts a metrics batch at `/v1/traces`; queue location reads the deploy-declared volume rather than the local document store, since that column answers document location and is false where export runs.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Rasm.AppHost.Observability;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OfflineDisposition {
    public static readonly OfflineDisposition Queued = new("queued");
    public static readonly OfflineDisposition Refused = new("refused");
    public static readonly OfflineDisposition Replayed = new("replayed");
    public static readonly OfflineDisposition Deferred = new("deferred");
    public static readonly OfflineDisposition Corrupt = new("corrupt");
    public static readonly OfflineDisposition DrainTimeout = new("drain-timeout");
}

[Union]
public abstract partial record DrainPass {
    private DrainPass() { }
    public sealed record Moved(OfflineDisposition Disposition, long Bytes) : DrainPass;
    public sealed record Settled(Option<OfflineDisposition> Disposition, long Bytes) : DrainPass;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OtlpOfflinePolicy(
    Option<string> Root,
    long CapBytes,
    Duration Maintenance,
    Duration Retention,
    Duration WriteBound,
    Duration LeaseWindow,
    int DrainBatch) {
    public static readonly OtlpOfflinePolicy None =
        new(Option<string>.None, 0L, Duration.Zero, Duration.Zero, Duration.Zero, Duration.Zero, 0);

    public static OtlpOfflinePolicy For(ResolvedProfile resolved) =>
        (resolved.Profile.OtlpExport ? resolved.Roots.QueueRoot : Option<string>.None).Match(
            Some: static root => new OtlpOfflinePolicy(
                Root: Some(root),
                CapBytes: 512L * 1024 * 1024,
                Maintenance: Duration.FromMinutes(2),
                Retention: Duration.FromHours(48),
                WriteBound: Duration.FromSeconds(60),
                LeaseWindow: Duration.FromSeconds(30),
                DrainBatch: 32),
            None: static () => None);

    public bool Armed => Root.IsSome;

    public string Bounds => string.Create(CultureInfo.InvariantCulture,
        $"{CapBytes}/{Retention:c}/{Maintenance:c}/{WriteBound:c}/{LeaseWindow:c}/{DrainBatch}");

    public FrozenDictionary<string, OtlpOfflineQueue> Open(InstrumentSet signals, MonotonicTimeline line) =>
        Root.Match(
            Some: root => toSeq(TelemetrySignal.Items)
                .Filter(static signal => signal.Capabilities.Admits(SignalCapability.Exported))
                .Map(signal => KeyValuePair.Create(signal.Key, new OtlpOfflineQueue(
                    signal,
                    new FileBlobProvider(
                        Path.Join(root, signal.Key),
                        CapBytes,
                        (int)Maintenance.TotalMilliseconds,
                        (long)Retention.TotalMilliseconds,
                        (int)WriteBound.TotalMilliseconds),
                    this,
                    signals,
                    line)))
                .ToFrozenDictionary(StringComparer.Ordinal),
            None: static () => FrozenDictionary<string, OtlpOfflineQueue>.Empty);
}

public readonly record struct OtlpTrust(Option<string> Authority, Option<string> Certificate, Option<string> Key) {
    public const string AuthorityVariable = "OTEL_EXPORTER_OTLP_CERTIFICATE";
    public const string CertificateVariable = "OTEL_EXPORTER_OTLP_CLIENT_CERTIFICATE";
    public const string KeyVariable = "OTEL_EXPORTER_OTLP_CLIENT_KEY";

    public static OtlpTrust Read() =>
        new(Path(AuthorityVariable), Path(CertificateVariable), Path(KeyVariable));

    static Option<string> Path(string variable) =>
        Optional(Environment.GetEnvironmentVariable(variable)).Filter(static value => value.Length > 0);

    public SocketsHttpHandler Mount(SocketsHttpHandler handler) {
        SslClientAuthenticationOptions ssl = handler.SslOptions;
        ignore((Certificate, Key)
            .Apply(static (certificate, key) => X509Certificate2.CreateFromPemFile(certificate, key))
            .Map(identity => ssl.ClientCertificates = [identity]));
        ignore(Authority.Map(chain => ssl.CertificateChainPolicy = new X509ChainPolicy {
            TrustMode = X509ChainTrustMode.CustomRootTrust,
            CustomTrustStore = { X509CertificateLoader.LoadCertificateFromFile(chain) },
            RevocationMode = X509RevocationMode.Online,
            RevocationFlag = X509RevocationFlag.ExcludeRoot,
        }));
        return handler;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class OtlpOfflineQueue(
    TelemetrySignal signal,
    FileBlobProvider store,
    OtlpOfflinePolicy policy,
    InstrumentSet signals,
    MonotonicTimeline line) : IDisposable {
    static readonly Op DrainWork = Op.Of(nameof(Drain));

    static readonly FrozenSet<string> FramingHeaders =
        FrozenSet.Create(StringComparer.OrdinalIgnoreCase, ["Content-Length", "Transfer-Encoding"]);

    public HttpResponseMessage Accept(HttpRequestMessage request, CancellationToken token) {
        byte[] body = Body(request, token);
        bool stored = body.Length > 0 && store.TryCreateBlob(body.AsSpan(), out _);
        ignore(Counted(stored ? OfflineDisposition.Queued : OfflineDisposition.Refused, body.LongLength));
        return new HttpResponseMessage(stored ? HttpStatusCode.Accepted : HttpStatusCode.ServiceUnavailable);
    }

    public Unit Drain(HttpRequestMessage template, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken token) =>
        line.Gauged<Unit, DeadlineClass>(
                lane: DeadlineClass.OtlpDrain,
                work: DrainWork,
                body: () => Fin.Succ(Passes(Spent(), template, forward, token)),
                key: DrainWork)
            .Match(
                Succ: measured => measured.Span.Breached ? ignore(Counted(OfflineDisposition.DrainTimeout, 0L)) : unit,
                Fail: static _ => unit);

    Func<bool> Spent() =>
        line.Capture(DrainWork).Match(
            Succ: opened => () => line.Capture(DrainWork).Bind(now => line.Elapsed(opened, now, DrainWork))
                .Match(Succ: elapsed => elapsed >= DeadlineClass.OtlpDrain.Bound, Fail: static _ => true),
            Fail: static _ => static () => true);

    Unit Passes(Func<bool> spent, HttpRequestMessage template, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken token) =>
        ignore(Range(0, policy.DrainBatch).AsIterable().FoldUntil(
            Option<DrainPass>.None,
            (_, _) => Some(Emitted(spent() ? Stalled : Pass(template, forward, token))),
            static step => step.Item1 is { Case: DrainPass.Settled }));

    static DrainPass Stalled => new DrainPass.Settled(Option<OfflineDisposition>.None, 0L);

    DrainPass Emitted(DrainPass pass) =>
        (pass.Switch(
            moved: row => ignore(Counted(row.Disposition, row.Bytes)),
            settled: row => row.Disposition.Match(Some: held => ignore(Counted(held, row.Bytes)), None: static () => unit)),
         pass).Item2;

    DrainPass Pass(HttpRequestMessage template, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken token) =>
        Leased() switch {
            { Case: PersistentBlob blob } => Read(blob) switch {
                { Case: byte[] body } => Forwarded(blob, body, template, forward, token),
                _ => (fun(() => ignore(blob.TryDelete()))(), (DrainPass)new DrainPass.Moved(OfflineDisposition.Corrupt, 0L)).Item2,
            },
            _ => Stalled,
        };

    Option<PersistentBlob> Leased() =>
        store.TryGetBlob(out PersistentBlob? blob) && blob is not null
        && blob.TryLease((int)policy.LeaseWindow.TotalMilliseconds)
            ? Some(blob)
            : None;

    static Option<byte[]> Read(PersistentBlob blob) =>
        blob.TryRead(out byte[]? body) && body is not null ? Some(body) : None;

    DrainPass Forwarded(PersistentBlob blob, byte[] body, HttpRequestMessage template, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken token) {
        using HttpRequestMessage replay = Rebuild(template, body);
        Option<HttpResponseMessage> response = Attempt(replay, forward, token);
        DrainPass pass = response.Filter(static held => held.IsSuccessStatusCode).Match(
            Some: _ => (fun(() => ignore(blob.TryDelete()))(),
                (DrainPass)new DrainPass.Moved(OfflineDisposition.Replayed, body.LongLength)).Item2,
            None: () => new DrainPass.Settled(Some(OfflineDisposition.Deferred), body.LongLength));
        ignore(response.Iter(static held => held.Dispose()));
        return pass;
    }

    public void Dispose() => store.Dispose();

    internal static Option<HttpResponseMessage> Attempt(HttpRequestMessage request, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken token) {
        try {
            return Some(forward(request, token));
        } catch (HttpRequestException) {
            return None;
        } catch (TaskCanceledException) when (!token.IsCancellationRequested) {
            return None;
        }
    }

    static byte[] Body(HttpRequestMessage request, CancellationToken token) {
        if (request.Content is not { } content) { return []; }
        using var buffer = new MemoryStream();
        content.CopyTo(buffer, context: null, token);
        return buffer.ToArray();
    }

    static HttpRequestMessage Rebuild(HttpRequestMessage template, byte[] body) {
        var content = new ByteArrayContent(body);
        ignore(Optional(template.Content).Iter(source => ignore(Carried(toSeq(source.Headers)).Iter(
            header => ignore(content.Headers.TryAddWithoutValidation(header.Key, header.Value))))));
        var replay = new HttpRequestMessage(template.Method, template.RequestUri) { Content = content };
        ignore(Carried(toSeq(template.Headers)).Iter(
            header => ignore(replay.Headers.TryAddWithoutValidation(header.Key, header.Value))));
        return replay;
    }

    static Seq<KeyValuePair<string, IEnumerable<string>>> Carried(Seq<KeyValuePair<string, IEnumerable<string>>> headers) =>
        headers.Filter(static header => !FramingHeaders.Contains(header.Key));

    Fin<Unit> Counted(OfflineDisposition disposition, long bytes) =>
        InstrumentSet.Tags((AppHostSlot.Signal, signal.Key), (AppHostSlot.Disposition, disposition.Key)) is var tags
            ? signals.Write(AppHostMeasure.OtlpOffline.Row, 1d, in tags)
                .Bind(_ => signals.Write(AppHostMeasure.OtlpOfflineBytes.Row, bytes, in tags))
            : Fin.Succ(unit);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class PersistentOtlpHandler(OtlpOfflineQueue queue) : DelegatingHandler {
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken token) =>
        Routed(request, base.Send, token);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) =>
        Task.FromResult(Routed(request, Awaited, token));

    HttpResponseMessage Routed(HttpRequestMessage request, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken token) {
        Option<HttpResponseMessage> live = OtlpOfflineQueue.Attempt(request, forward, token);
        return live.Filter(static held => held.IsSuccessStatusCode).Match(
            Some: held => (queue.Drain(request, forward, token), held).Item2,
            None: () => (ignore(live.Iter(static held => held.Dispose())), queue.Accept(request, token)).Item2);
    }

    HttpResponseMessage Awaited(HttpRequestMessage message, CancellationToken token) =>
        base.SendAsync(message, token).GetAwaiter().GetResult();
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
