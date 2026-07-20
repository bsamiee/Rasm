# [APPHOST_DIAGNOSTICS_AND_TELEMETRY]

Telemetry identity, the correlation spine, log projection, signal governance, and the suite data-classification taxonomy form one diagnostics concern owned by Rasm.AppHost. Owned axes: the `TelemetrySource` vocabulary with its instrument registry, the four-row `TelemetrySignal` governance set (traces, metrics, logs, profiles), the `LogPipeline` arbitration column, and the nine-row `DataClassification` taxonomy enforced at every exporter seam. One spine composes Microsoft.Extensions telemetry policy over the OpenTelemetry SDK, `LoggerMessage` source generation at lib level, Serilog projection at desktop composition roots, and a continuous-profiling rail linking CPU profiles to spans; siblings emit through minted identities and never construct telemetry owners of their own. OTel GenAI semantic conventions (`gen_ai.*` attributes, MCP spans, `gen_ai.usage.input_tokens`/`output_tokens`) ride the trace and metric signals so AppHost telemetry aligns with the agentic surface the host serves.

## [01]-[INDEX]

- [01]-[TELEMETRY_IDENTITY]: Minted source and meter identity with the instrument registry.
- [02]-[CORRELATION_SPINE]: One boot-minted root id and W3C trace-context propagated across every hop.
- [03]-[LOG_PROJECTION]: Generated lib-level delegates and per-profile pipeline-owner arbitration.
- [04]-[SIGNAL_GOVERNANCE]: Per-signal sampling, buffering, enrichment, exporter placement, and drain flush.
- [05]-[REDACTION_TAXONOMY]: Nine classification rows binding redactor policy at every exporter seam.

## [02]-[TELEMETRY_IDENTITY]

- Owner: `TelemetrySource` `[SmartEnum<string>]` under the `ComparerAccessors.StringOrdinal` accessor; the `InstrumentRow` spine rows inside `TelemetryIdentity`.
- Cases: one source row per minted package meter and one per builtin meter; the spine instrument rows minted here merge with the Observability/instruments#INSTRUMENT_CATALOG domain roster onto the one `TelemetryContributorPort` row set `HostInstruments.Telemetry` carries.
- Entry: `TelemetryIdentity.Mint(IMeterFactory factory, TelemetrySource source, string version, string schemaUrl, CorrelationId root)` returning the DI-owned `(ActivitySource, Meter)` pair; each instrument materializes from its `InstrumentRow.Bind` delegate over the minted `Meter`.
- Auto: builtin rows feed GC, threadpool, exception-rate, and HttpClient duration streams through `AddMeter` with zero package; minted meters carry version and correlation tags at construction and the contributor's semconv schema coordinate as `MeterOptions.TelemetrySchemaUrl`, so a schema-aware backend reads every `rasm.*` scope with pinned semantics; instrument identity de-duplicates by name, so name, unit, and description are `InstrumentRow` declaration facts and a drifted unit forks the stream at its one registry row, never at a call site.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Microsoft.Extensions.Telemetry.Abstractions, BCL inbox.
- Growth: one source is one vocabulary row and one instrument is one `InstrumentRow`; zero new surface.
- Boundary: a process-static `Meter` field outliving its provider is the named defect — minted pairs are `IMeterFactory`-owned and unload with the host ALC; the host builder registers the metrics services on every path including the empty builder, so `IMeterFactory` arrives with zero registration row; every instrument enters through its `InstrumentRow` declaration on the contributor row set, so the minted pair is the registration payload `TelemetryContributorPort` carries inward, deleting handler-local `ActivitySource` and `Meter` owners.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TelemetrySource {
    public static readonly TelemetrySource AppHost = new("Rasm.AppHost", minted: true);
    public static readonly TelemetrySource Persistence = new("Rasm.Persistence", minted: true);
    public static readonly TelemetrySource Compute = new("Rasm.Compute", minted: true);
    public static readonly TelemetrySource AppUi = new("Rasm.AppUi", minted: true);
    public static readonly TelemetrySource Fabrication = new("Rasm.Fabrication", minted: true);
    public static readonly TelemetrySource SystemRuntime = new("System.Runtime", minted: false);
    public static readonly TelemetrySource SystemNetHttp = new("System.Net.Http", minted: false);

    public bool Minted { get; }
}

public sealed record InstrumentRow(
    TelemetrySource Source,
    string Name,
    string Unit,
    string Description,
    Func<Meter, string, string, string, Instrument> Bind);

public static class TelemetryIdentity {
    public static readonly Seq<InstrumentRow> SpineRows = Seq(
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.logs.flushed", "{record}", "buffered log records replayed by the incident flush",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.redaction.tags", "{tag}", "classified tags redacted before any exporter observed them",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.drain.duration", "s", "drain-band fold duration per band",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)));

    public static (ActivitySource Source, Meter Meter) Mint(IMeterFactory factory, TelemetrySource source, string version, string schemaUrl, CorrelationId root) =>
        (new ActivitySource(source.Key, version),
         factory.Create(new MeterOptions(source.Key) {
             Version = version,
             TelemetrySchemaUrl = schemaUrl,
             Tags = [new(nameof(CorrelationId), root.ToString())],
         }));
}
```

## [03]-[CORRELATION_SPINE]

- Owner: `Correlation` spine surface stamping the boot-minted `CorrelationId` across every signal and hop; `RootEnricher` `IStaticLogEnricher` stamps process constants once per provider; `CausalEnricher` `ILogEnricher` stamps the request-scoped correlation per record; `TraceContext` the W3C distributed-trace propagation fold injecting and extracting `traceparent`/`tracestate` over every registered transport carrier so a remote span continues the parent trace.
- Cases: two enrichment seats split by cost class — `RootEnricher` for the boot mint and resource instance id; `CausalEnricher` for the ambient correlation key; two propagation directions and the inbound continued-span start on `TraceContext` — the generic `Inject`/`Extract`/`Continue` members take any carrier with a getter/setter delegate pair, and `Continue` extracts then seeds `Baggage.Current` and starts the inbound `Activity` from the extracted context; four carrier adapter rows ride that generic spine — the gRPC `Metadata` pair for the local-ipc control hop, the MQTT v5 user-property pair mounted at the Wire/livewire `MqttLane` publish and receive edges, and the NATS-header and CloudEvents-attribute pairs composing at the Persistence egress legs, where the setter/getter bodies pin against the Persistence catalogs — `NatsHeaders` the `IDictionary<string, StringValues>` header carrier, and the CloudEvents `traceparent`/`tracestate` extension attributes through `SetAttributeFromString(name, value)` and the `this[string]` indexer.
- Entry: `Correlation.Stamp(CorrelationId root)` returning the `IDisposable` ambient scope; `Correlation.Capture()` snapshots the ambient context for deferred work, `Correlation.Restore(value)` rehydrates it at the work entry; `TraceContext.Inject<TCarrier>(carrier, set)` writes the active context, `TraceContext.Extract<TCarrier>(carrier, get)` reads the parent context, and `TraceContext.Continue<TCarrier>(carrier, get, name, kind)` extracts the parent, sets `Baggage.Current`, and starts the continued `Activity` from the parent `ActivityContext`; the `Metadata` overloads are the gRPC adapter the Wire/companion#CONTROL_SERVICE handler reads, and the `MqttApplicationMessageBuilder` overload the publish-edge adapter `MqttLane.Write` threads before `Build()`.
- Auto: one boot mint stamps `LogContext` properties, `Baggage`, meter tags, receipts, and support manifests — deletes per-call-site correlation parameters across the suite; the two enrichers feed `IEnrichmentTagCollector` under one bounded prefix — the causal seat through `AddLogEnricher<CausalEnricher>`, the root seat as the pre-constructed boot instance through `AddStaticLogEnricher(RootEnricher)` because the boot mint and instance id resolve at composition, never from DI activation; pooled-callback, native-callback, and manual-thread ambient breaks share one repair — `LogContext.Clone` captures the context as a value, `Push` restores it at deferred-work entry; `TraceContext` rides the same `Correlation.Spine` composite, so the W3C `traceparent`/`tracestate` carrier and the `Baggage` carrier inject and extract in one pass and a continued remote span shares the in-process correlation id automatically.
- Packages: OpenTelemetry, Serilog, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, Grpc.Core.Api, MQTTnet, BCL inbox.
- Growth: a new stamped carrier is one stamp row inside `Stamp` with one policy value; a new process-constant dimension is one `RootEnricher` line, a new request dimension one `CausalEnricher` line; a new propagation carrier is one getter/setter adapter pair over the generic `Inject`/`Extract` on the same `Spine` composite, never a second tracer; zero new surface.
- Boundary: the composite registers as `Propagators.DefaultTextMapPropagator` and crosses every hop through `TextMapPropagator.Inject` and `TextMapPropagator.Extract`, riding gRPC metadata on the local-ipc leg; `TraceContext` is the seam owner of every crossing — the propagation mechanics live here while each transport boundary consumes its adapter pair, so a per-transport hand-rolled `traceparent` header write is the deleted form; the MQTT adapter writes v5 user properties through the non-obsolete `WithUserProperty(name, value)` builder overload on publish and continues the trace at the receive pump through the `MqttRuntime.Properties` getter delegate, so a broker hop joins the same trace the gRPC and Kafka legs carry and TraceBased exemplars survive the hop; the NATS and CloudEvents adapters compose Persistence-side because NATS carries no OTel instrumentation by design — manual inject and extract are the contract — and their concrete setter/getter bodies land beside the egress legs, never a second spine; the Serilog event trace-id and span-id fields bind to the live `Activity` ids, never to a parallel identifier; `RuntimeContext` slots carry the ambient value where async flow demands it; a constant placed in `CausalEnricher` is waste and a request value placed in `RootEnricher` is a bug — the cost-class split is structural, and the captured `LogContext.Clone` value, never the ambient current at execution time, seeds deferred children; `ActivitySource.StartActivity(name, kind, parentContext)` seeds the continued span from the extracted `ActivityContext`, never a fresh root, so a flat per-process trace is the deleted form.

```csharp signature
public static class Correlation {
    public static readonly RuntimeContextSlot<CorrelationId> Ambient =
        RuntimeContext.RegisterSlot<CorrelationId>(nameof(CorrelationId));

    public static readonly TextMapPropagator Spine =
        new CompositeTextMapPropagator([new TraceContextPropagator(), new BaggagePropagator()]);

    public static CorrelationId Mint() => CorrelationId.Create(Guid.CreateVersion7());

    public static IDisposable Stamp(CorrelationId root) =>
        (Baggage.SetBaggage(nameof(CorrelationId), root.ToString()),
         LogContext.PushProperty(nameof(CorrelationId), root.ToString())).Item2;

    public static ILogEventEnricher Capture() => LogContext.Clone();

    public static IDisposable Restore(ILogEventEnricher captured) => LogContext.Push(captured);
}

public static class TraceContext {
    public static readonly ActivitySource Source = new(TelemetrySource.AppHost.Key);

    // One generic carrier spine: every transport adapter is a getter/setter delegate pair over
    // these three members, never a per-transport tracer or a hand-rolled header write.
    public static TCarrier Inject<TCarrier>(TCarrier carrier, Action<TCarrier, string, string> set) =>
        (fun(() => Correlation.Spine.Inject(
            new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current),
            carrier,
            set))(), carrier).Item2;

    public static PropagationContext Extract<TCarrier>(TCarrier carrier, Func<TCarrier, string, IEnumerable<string>> get) =>
        Correlation.Spine.Extract(default, carrier, get);

    public static Activity? Continue<TCarrier>(TCarrier carrier, Func<TCarrier, string, IEnumerable<string>> get, string name, ActivityKind kind = ActivityKind.Server) {
        var parent = Extract(carrier, get);
        Baggage.Current = parent.Baggage;
        return Source.StartActivity(name, kind, parent.ActivityContext);
    }

    // gRPC metadata adapter rows — the local-ipc control hop the Wire/companion handler reads.
    static IEnumerable<string> Get(Metadata carrier, string key) =>
        carrier.GetAll(key).Select(static entry => entry.Value);

    public static Metadata Inject(Metadata carrier) =>
        Inject(carrier, static (c, key, value) => c.Add(key, value));

    public static PropagationContext Extract(Metadata carrier) => Extract(carrier, Get);

    public static Activity? Continue(Metadata carrier, string name) => Continue(carrier, Get, name);

    // MQTT v5 user-property adapter rows — mounted at the Wire/livewire MqttLane publish and
    // receive edges; the receive-side getter is the MqttRuntime.Properties accessor delegate.
    public static MqttApplicationMessageBuilder Inject(MqttApplicationMessageBuilder carrier) =>
        Inject(carrier, static (c, key, value) => ignore(c.WithUserProperty(key, value)));
}

public sealed class RootEnricher(CorrelationId root, string instanceId) : IStaticLogEnricher {
    public void Enrich(IEnrichmentTagCollector collector) {
        collector.Add($"{nameof(Correlation)}.root", root.ToString());
        collector.Add($"{nameof(Correlation)}.instance", instanceId);
    }
}

public sealed class CausalEnricher : ILogEnricher {
    public void Enrich(IEnrichmentTagCollector collector) =>
        Optional(Correlation.Ambient.Get()).Iter(value => collector.Add($"{nameof(Correlation)}.causal", value.ToString()));
}
```

## [04]-[LOG_PROJECTION]

- Owner: `LogPipeline` `[SmartEnum<string>]` arbitration column; `SpineLog` generated delegates; `SerilogProjectionPolicy` shaping surface; `SpineLossFold` failure listener.
- Cases: one pipeline row per delivery mandate — serilog-projection on the desktop-family profile rows, otel-export on the service-family profile rows; the `Owner` arbitration is the total assignment.
- Entry: `LogPipeline.Owner(HostProfile profile)` — the total arbitration projection; `SerilogProjectionPolicy.Shape(LoggerConfiguration)` composes the six rails and freezes them on `CreateLogger`.
- Auto: generated delegates carry stable `EventId` and `EventName`; `[LogProperties]` expands typed payloads into bounded tags with classification intact, `[TagProvider]` projects a foreign type that carries no annotation, and `[TagName]`/`[LogPropertyIgnore]` rename and elide at the declaration; the wire sink rides one `BatchingOptions` latency/throughput square while `Fallible` wraps it in a `FailureListenerSink` that folds sink failure into the typed loss rail and `AuditTo` propagates it to the caller, `FallbackChain` reroutes on synchronous throw, `Conditional` forks the error-and-above tier to the hot sink, and the registered `ILoggingFailureListener` folds every reported `LoggingFailureKind` into the loss stream.
- Packages: Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Telemetry.Abstractions, Serilog, Thinktecture.Runtime.Extensions.
- Growth: one spine event is one generated-delegate row inside the 1000-1099 EVENT stride (`FaultBand.SpineEvents`); a new profile is one `Owner` arm; one sink-loss class is one `SpineLossFold` fact row; zero new surface.
- Boundary: lib level emits `ILogger` only — zero Serilog types below composition roots, deleting static `Log` facade calls; the host bridge is the service-aware `AddSerilog(IServiceCollection, Action<IServiceProvider, LoggerConfiguration>)` overload whose configuration action runs `SerilogProjectionPolicy.Shape`, every sink is an app-root pin, and the boot window logs through `CreateBootstrapLogger()`, frozen into the host pipeline when that bridge registers, so no startup fault predates the pipeline; destructuring pins all three caps — depth, string length, collection count — because a pipeline accepting foreign graphs is a payload-bomb seam; `CloseAndFlush` is a ranked drain participant; exactly one pipeline owner per profile row, never both on one signal; `Filter.ByExcluding` holds lifetime-noise categories out of the pipeline by `Matching.FromSource` construction, `Destructure.With` binds the redaction-preserving `IDestructuringPolicy` so a custom shaper never strips classification, and `ForContext` is the emission-side source-keyed derivation the generated delegates ride, never a second `Shape` call; `SpineLossFold` implements `ILoggingFailureListener.OnLoggingFailed(object sender, LoggingFailureKind kind, string message, IReadOnlyCollection<LogEvent>? events, Exception? exception)` and folds every sink failure into one evidence stream keyed by `LoggingFailureKind` (`Temporary`, `Permanent`, `Final`), with `SelfLog.Enable` the never-throwing floor beneath the rail; `WriteTo.Fallible(configureSink, listener)` is the binding that wraps the wire-sink fallback chain in a `FailureListenerSink` so the listener observes every reported failure, and a sink outside `Fallible` is unobserved best-effort, the named defect; the test row installs `AddFakeLogging` and asserts through `FakeLogCollector` snapshots, never sink text.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LogPipeline {
    public static readonly LogPipeline SerilogProjection = new("serilog-projection");
    public static readonly LogPipeline OtelExport = new("otel-export");

    public static LogPipeline Owner(HostProfile profile) =>
        profile.Map(
            rhinoPlugin: SerilogProjection,
            gh2Plugin: SerilogProjection,
            standaloneDesktop: SerilogProjection,
            companionProcess: OtelExport,
            sidecar: OtelExport,
            headlessService: OtelExport,
            webService: OtelExport,
            testHost: SerilogProjection);
}

public static class HostTags {
    public static void Collect(ITagCollector collector, Version value) =>
        collector.Add("host.generation", value.Major);
}

public static partial class SpineLog {
    [LoggerMessage(EventId = 1000, EventName = nameof(ReloadApplied), Level = LogLevel.Information, Message = "configuration reload applied")]
    public static partial void ReloadApplied(ILogger logger, [LogProperties(OmitReferenceName = true, SkipNullProperties = true)] ReloadReceipt receipt);

    [LoggerMessage(EventId = 1001, EventName = nameof(SignalDropped), Level = LogLevel.Warning, Message = "telemetry signal {Signal} dropped {Count} events", SkipEnabledCheck = true)]
    public static partial void SignalDropped(ILogger logger, [TagName("signal.kind")] string signal, long count);

    [LoggerMessage(EventId = 1002, EventName = nameof(DrainSettled), Level = LogLevel.Information, Message = "drain settled on host {Host}")]
    public static partial void DrainSettled(ILogger logger, [TagProvider(typeof(HostTags), nameof(HostTags.Collect))] Version host, [LogPropertyIgnore] string trace);
}

public sealed class SpineLossFold : ILoggingFailureListener {
    public static readonly Atom<Seq<(string Sink, LoggingFailureKind Kind, int Count)>> Facts = Atom(Seq<(string, LoggingFailureKind, int)>());

    public void OnLoggingFailed(object sender, LoggingFailureKind kind, string message, IReadOnlyCollection<LogEvent>? events, Exception? exception) =>
        ignore(Facts.Swap(facts => facts.Add((sender.GetType().Name, kind, events?.Count ?? 0))));
}

public static class SerilogProjectionPolicy {
    public static readonly LoggingLevelSwitch Floor = new(LogEventLevel.Information);

    public static readonly BatchingOptions Batch = new() {
        EagerlyEmitFirstEvent = true,
        BatchSizeLimit = 500,
        BufferingTimeLimit = TimeSpan.FromSeconds(2),
        QueueLimit = 10_000,
    };

    public static LoggerConfiguration Shape(LoggerConfiguration configuration, IBatchedLogEventSink wire, ILogEventSink fallback, ILogEventSink audit, ILogEventSink hot, IDestructuringPolicy classification, ILoggingFailureListener loss) {
        ArgumentNullException.ThrowIfNull(configuration);
        SelfLog.Enable(Console.Error);
        return configuration
            .MinimumLevel.ControlledBy(Floor)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Destructure.With(classification)
            .Destructure.ToMaximumDepth(4)
            .Destructure.ToMaximumStringLength(1024)
            .Destructure.ToMaximumCollectionCount(64)
            .Filter.ByExcluding(Matching.FromSource("Microsoft.Hosting.Lifetime"))
            .WriteTo.Fallible(
                observed => observed.FallbackChain(
                    write => write.Sink(wire, Batch),
                    rescue => rescue.Sink(fallback)),
                loss)
            .WriteTo.Conditional(
                static log => log.Level >= LogEventLevel.Error,
                static into => into.Sink(hot))
            .AuditTo.Sink(audit);
    }
}
```

## [05]-[SIGNAL_GOVERNANCE]

- Owner: `TelemetrySignal` `[SmartEnum<string>]` governance rows and the `SignalGovernance` registration fold; `LatencyCheckpoint` `[SmartEnum<string>]` the in-flight phase vocabulary; `LatencySpine` the checkpoint recorder; the admitted `PyroscopeSpanProcessor` the profile-to-span linking `BaseProcessor<Activity>`; `ResourceIdentity` the detector-enriched identity delegate every provider owner consumes; `OfflineQueue` the file-system-backed persistent OTLP transmission store.
- Cases: one governance row per signal — trace, metric, log, profile — each binding ratio, buffering, and redaction policy; one latency checkpoint row per measured phase — drain, hop, capture.
- Entry: `SignalGovernance.Govern(IServiceCollection services, HostProfile profile, Action<ResourceBuilder> identity)` returning the host-owned `OpenTelemetryBuilder`; `ResourceIdentity.Compose(HostProfile profile, Action<ResourceBuilder> identity)` wrapping the minted-triple identity delegate with the detector rows — the one `Action<ResourceBuilder>` every provider owner consumes; `OfflineQueue.Open(string path, OfflineQueuePolicy policy)` constructing the bounded on-disk queue one transmission owner holds and disposes; `SignalGovernance.StoreDriver(OpenTelemetryBuilder)` and `SignalGovernance.StoreWire<TKey, TValue>(OpenTelemetryBuilder)` the store-composing service-root rows — driver subscription shape-free and once, wire instrumentation once per message shape; `LatencySpine.Mark(ILatencyContext context, CheckpointToken phase)` records one checkpoint, `LatencySpine.Seal` freezes the context for export at drain.
- Auto: provider `ForceFlush` and `Shutdown` ride the telemetry drain band; the fault transition lands the `GlobalLogBuffer.Flush` window inside support capture; `AddRandomProbabilisticSampler` carries a `RandomProbabilisticSamplerFilterRule` row keyed by maximum level so it thins the chatty floor and never the error ceiling, while a `LogBufferingFilterRule` row holds the verbose tiers until an incident flushes them, bounded by the `GlobalLogBufferingOptions` caps — record size, buffer size, auto-flush window — so the incident buffer never runs unbounded; `AddHttpClientInstrumentation` binds `HttpClientTraceInstrumentationOptions` — `RecordException` projects exceptions as `ActivityEvent`, `FilterHttpRequestMessage` drops the loopback leg, `EnrichWithException` stamps the exception type, and URL-query redaction stays the package default; `Views` folds one `MetricStreamConfiguration` per instrument through `AddView` — the named-instrument rows project their tag keys and cap the drain-band and classification dimensions, and the trailing `*` wildcard row pins `CardinalityLimit` 256 on every remaining stream so the per-tenant `TenantContext.Tag` dimension stays inside a bounded series budget on every minted meter; a wildcard `TagKeys` projection erases every unlisted tag, so the wildcard row carries the series cap alone; the service-app-root metric exemplar policy rides `SetExemplarFilter` per the trace-based governance row; `EnableEnrichment` activates the `RootEnricher`/`CausalEnricher` seats and binds `LoggerEnrichmentOptions` — `CaptureStackTraces` and `IncludeExceptionMessage` admit exception frames onto the log signal behind the redaction seam, and `UseFileInfoForStackTraces` stays off because file and line are leak-bearing; the serilog-projection rows add `AddConsoleLatencyDataExporter` so a desktop or test host reads latency spans live with zero wire cost; the latency vocabulary registers once through `RegisterCheckpointNames` at composition, `ILatencyContextTokenIssuer.GetCheckpointToken(string)` resolves each name to a `CheckpointToken`, and runtime code records through those resolved handles only — durations never derive from stamp differences; a value-bearing `MeasureToken` recording from `GetMeasureToken(string)` is a forward row admitted only when a measure consumer exists; `ResourceIdentity.Compose` runs the minted `service.namespace`/`service.name`/`service.instance.id` triple delegate first, then chains `AddHostDetector`/`AddOperatingSystemDetector`/`AddProcessDetector`/`AddProcessRuntimeDetector` always-on and `AddContainerDetector` on the containerized profile rows alone — detectors ENRICH and never replace the mint, each contributing only the semconv attributes it resolves (`host.*`, `os.*`, `process.*`, `process.runtime.*`, `container.id`), placement dimensions no backend derives from the triple; `AddBaggageActivityProcessor(SignalGovernance.PromotedBaggage)` promotes the allowlisted `rasm.tenant` and `CorrelationId` baggage entries onto every span at start, so a backend groups spend, latency, and traces by tenant with zero per-call-site tagging; `AddHttpClientLatencyTelemetry()` installs the per-phase checkpoint handler over every named `HttpClient` — name-resolution versus connection versus server time at checkpoint cost, `EnableDetailedLatencyBreakdown` the package-default breakdown — and `AddExtendedHttpClientLogging` replaces the built-in client logger with the redaction-aware form whose four `*DataClasses` maps bind the `[06]` taxonomy through `DataClassification.Marker`, bespoke tags entering as `AddHttpClientLogEnricher<T>` rows.
- Receipt: `LatencyData` — the frozen checkpoint spans `ILatencyDataExporter` exports at the drain band; one span per drain, hop, and capture phase.
- Packages: OpenTelemetry.Extensions.Hosting, OpenTelemetry, OpenTelemetry.Extensions, OpenTelemetry.PersistentStorage.FileSystem, OpenTelemetry.Resources.Host, OpenTelemetry.Resources.OperatingSystem, OpenTelemetry.Resources.Process, OpenTelemetry.Resources.ProcessRuntime, OpenTelemetry.Resources.Container, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime, OpenTelemetry.Instrumentation.GrpcNetClient, OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Instrumentation.ConfluentKafka, Npgsql.OpenTelemetry, Microsoft.Extensions.Telemetry, Microsoft.Extensions.Telemetry.Abstractions, Microsoft.Extensions.Http.Diagnostics, OpenTelemetry.Exporter.OpenTelemetryProtocol, Pyroscope.OpenTelemetry, Microsoft.Extensions.Diagnostics.Testing.
- Growth: one governance decision is one policy value row; one stream reshaping is one `AddView` row over `MetricStreamConfiguration`; one measured phase is one `LatencyCheckpoint` row recorded by one `LatencySpine.Mark` call; a new store message shape is one `StoreWire<TKey, TValue>` closure at the composing root; a new resource dimension is one detector row inside `ResourceIdentity.Compose`; a new promoted baggage key is one `PromotedBaggage` pattern row; zero new surface.
- Boundary: the OTLP exporter package enters only at service app roots — the otelExport arm binds `UseOtlpExporter` once after all four signals, and endpoint, protocol, temporality, compression, and mTLS material bind from the `OTEL_EXPORTER_OTLP_*` rows the governance table pins, HTTP+protobuf the one estate egress protocol — that egress boundary is the `OtelExport` seam; EF spans stay native `Activity` emission, gRPC client spans ride `AddGrpcClientInstrumentation` with `SuppressDownstreamInstrumentation` so the underlying HTTP/2 leg never double-traces, and the otelExport arm carries `AddAspNetCoreInstrumentation` beside the wire host for inbound request spans; store telemetry is the PORT-peer arbitration — Persistence owns the driver and the instrumented builders while the app root alone registers, so no downward reference forms: `StoreDriver` subscribes `AddNpgsql` tracing and `AddNpgsqlInstrumentation` metrics once at the store-composing root, with `NpgsqlDataSourceBuilder.Name` set Persistence-side per logical database so `db.client.connection.pool.name` keys stable pool dimensions, and `StoreWire<TKey, TValue>` registers `AddKafkaProducerInstrumentation`/`AddKafkaConsumerInstrumentation` on both providers once per message shape over the Persistence egress `AsInstrumentedProducerBuilder` and CDC ingress `InstrumentedConsumerBuilder` legs, closing the producer-only Kafka asymmetry; the builtin rows delete the meter-side `AddRuntimeInstrumentation` registration while the trace-side `AddHttpClientInstrumentation` row keeps URL-query redaction; `LatencySpine.Mark` is the single checkpoint recorder, and the `ILatencyContext` payload is carried into `DrainConductor.Drain`, `OutboundSurface.Run`, and `SupportCapture.Capture` — those folds thread the context so each phase boundary has its recording seat, deleting per-fold `Stopwatch` timing; the recorder is cheaper than child spans and free of sampling coupling; name-to-`CheckpointToken`/`MeasureToken` issuance rides `ILatencyContextTokenIssuer.GetCheckpointToken`/`GetMeasureToken`, the frozen spans read through the `ILatencyContext.LatencyData` accessor, and `ILatencyDataExporter.ExportAsync(LatencyData, CancellationToken)` exports at the telemetry drain band; `AddLatencyContext` registers the context once and the consuming folds thread it so each phase boundary records through its issued token; test-row trace assertions ride one `BaseProcessor<Activity>` through `AddProcessor` and metric assertions ride `MetricCollector<T>` — no in-memory exporter package enters; the `Profile` signal is the continuous-profiling rail — the admitted `PyroscopeSpanProcessor` registers as `AddProcessor<PyroscopeSpanProcessor>()` through the same seat the test-row processor uses, gated to service app roots where the profiler endpoint resolves — `Profiler.Instance` carries no address member, so the ingest address is the agent `PYROSCOPE_SERVER_ADDRESS` environment row bound from the deploy-plane profiles endpoint beside `PYROSCOPE_APPLICATION_NAME` and the CLR-profiler enablement rows, with `SetAuthToken`/`SetBasicAuth` the credential seam and `PYROSCOPE_TENANT_ID` the tenancy row — and stamps the `pyroscope.profile.id` tag on each root span so a flame graph scopes to the exact trace that showed a regression; the GenAI semantic conventions ride the trace and metric signals — an MCP-served tool span carries `gen_ai.operation.name` and the `gen_ai.provider.name` provider discriminant beside the `gen_ai.usage.input_tokens`/`output_tokens` counts, and the token-usage instruments ride the minted `Rasm.AppHost` meter, so the agentic surface the host serves shares one telemetry taxonomy with the runtime, never a parallel agent-metrics owner; the offline spine is composition-root custody — one `OfflineQueue` per OTLP transmission owner (the hosted service root registering it as a DI-owned singleton beside `Govern`, `PluginTelemetryHost.Open` holding it as the capsule queue field), rooted at a per-owner writable directory, disposed with its owner — `Hold` persists a failed export batch through the span `TryCreateBlob`, `Replay` drains in the catalog order `TryGetBlob`/`GetBlobs` then `TryLease` then `TryRead` then successful export then `TryDelete`, an over-cap create drops silently onto the provider `EventSource`, stored bytes are already-redacted wire bytes because redaction runs before export, and the queue directory inherits the support-bundle retention law; baggage promotion is allowlist-only — `PromotedBaggage` names `TenantContext.TenantSlot` and `nameof(CorrelationId)` and an unlisted key never reaches a span, so free-form baggage flood is structurally impossible; the latency breakdown and the `AddHttpClientInstrumentation` span are two projections of one hop — the checkpoint handler never mints a second trace, extended logging supersedes the built-in client logger so both active is the double-log defect, and `wrapHandlersPipeline` decides whether logging observes pre- or post-retry attempts on the resilience chain.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TelemetrySignal {
    public static readonly TelemetrySignal Log = new("log", buffered: true, redacted: true, SampleRatio);
    public static readonly TelemetrySignal Trace = new("trace", buffered: false, redacted: true, SampleRatio);
    public static readonly TelemetrySignal Metric = new("metric", buffered: false, redacted: false, static _ => 1d);
    public static readonly TelemetrySignal Profile = new("profile", buffered: false, redacted: false, SampleRatio);

    public bool Buffered { get; }

    public bool Redacted { get; }

    [UseDelegateFromConstructor]
    public partial double Ratio(HostProfile profile);

    private static double SampleRatio(HostProfile profile) =>
        profile.Map(
            rhinoPlugin: 1d,
            gh2Plugin: 1d,
            standaloneDesktop: 1d,
            companionProcess: 0.1d,
            sidecar: 0.1d,
            headlessService: 0.1d,
            webService: 0.1d,
            testHost: 1d);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LatencyCheckpoint {
    public static readonly LatencyCheckpoint Drain = new("drain");
    public static readonly LatencyCheckpoint Hop = new("hop");
    public static readonly LatencyCheckpoint Capture = new("capture");
}

public static class LatencySpine {
    public static IServiceCollection Register(IServiceCollection services) =>
        services.RegisterCheckpointNames([.. LatencyCheckpoint.Items.Select(static row => row.Key)]);

    public static ILatencyContext Mark(ILatencyContext context, CheckpointToken phase) =>
        (context.AddCheckpoint(phase), context).Item2;

    public static ILatencyContext Seal(ILatencyContext context) =>
        (context.Freeze(), context).Item2;
}

public static class ResourceIdentity {
    static readonly Func<HostProfile, bool> Containerized = static profile => profile.Map(
        rhinoPlugin: false,
        gh2Plugin: false,
        standaloneDesktop: false,
        companionProcess: false,
        sidecar: true,
        headlessService: true,
        webService: true,
        testHost: false);

    // Wraps the minted-triple identity delegate: the triple mints first, then the detector rows
    // ENRICH the same ResourceBuilder — never a SetResourceBuilder replacement.
    public static Action<ResourceBuilder> Compose(HostProfile profile, Action<ResourceBuilder> identity) =>
        resource => ignore((fun(() => identity(resource))(), Containerized(profile)
            ? Detected(resource).AddContainerDetector()
            : Detected(resource)).Item2);

    static ResourceBuilder Detected(ResourceBuilder resource) =>
        resource
            .AddHostDetector()
            .AddOperatingSystemDetector()
            .AddProcessDetector()
            .AddProcessRuntimeDetector();
}

public sealed record OfflineQueuePolicy(
    long SizeCapBytes,
    int MaintenanceMillis,
    long RetentionMillis,
    int WriteTimeoutMillis,
    int LeaseMillis) {
    public static readonly OfflineQueuePolicy Canonical = new(
        SizeCapBytes: 52_428_800,
        MaintenanceMillis: 120_000,
        RetentionMillis: 172_800_000,
        WriteTimeoutMillis: 60_000,
        LeaseMillis: 60_000);
}

public sealed record OfflineQueue(FileBlobProvider Store, OfflineQueuePolicy Policy) : IDisposable {
    public static OfflineQueue Open(string path, OfflineQueuePolicy policy) =>
        new(new FileBlobProvider(path, policy.SizeCapBytes, policy.MaintenanceMillis, policy.RetentionMillis, policy.WriteTimeoutMillis), policy);

    public Unit Hold(ReadOnlySpan<byte> redactedBatch) =>
        ignore(Store.TryCreateBlob(redactedBatch, out _));

    public Unit Replay(Func<byte[], bool> export) =>
        Store.GetBlobs().AsIterable().Fold(unit, (state, blob) =>
            blob.TryLease(Policy.LeaseMillis) && blob.TryRead(out var bytes) && bytes is not null && export(bytes)
                ? (ignore(blob.TryDelete()), state).Item2
                : state);

    public void Dispose() => Store.Dispose();
}

public static class SignalGovernance {
    public static readonly Predicate<string> PromotedBaggage =
        static key => key is TenantContext.TenantSlot or nameof(CorrelationId);

    public static readonly Seq<(string Instrument, MetricStreamConfiguration Shape)> Views = [
        ("rasm.apphost.drain.duration", new MetricStreamConfiguration { TagKeys = ["drain.band"], CardinalityLimit = 64 }),
        ("rasm.apphost.redaction.tags", new MetricStreamConfiguration { TagKeys = [nameof(DataClassification)], CardinalityLimit = 32 }),
        ("*", new MetricStreamConfiguration { CardinalityLimit = 256 }),
    ];

    public static OpenTelemetryBuilder Govern(IServiceCollection services, HostProfile profile, Action<ResourceBuilder> identity) =>
        LogPipeline.Owner(profile).Switch(
            state: services.AddOpenTelemetry()
                .ConfigureResource(identity)
                .WithTracing(tracing => tracing
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(TelemetrySignal.Trace.Ratio(profile))))
                    .AddSource([.. TelemetrySource.Items.Where(static row => row.Minted).Select(static row => row.Key)])
                    .AddHttpClientInstrumentation(static http => {
                        http.RecordException = true;
                        http.FilterHttpRequestMessage = static request => request.RequestUri is { IsLoopback: false };
                        http.EnrichWithException = static (activity, exception) => activity.SetTag("exception.type", exception.GetType().Name);
                    })
                    .AddGrpcClientInstrumentation(static grpc => grpc.SuppressDownstreamInstrumentation = true)
                    .AddBaggageActivityProcessor(PromotedBaggage))
                .WithMetrics(metrics => Views.Fold(
                    metrics.AddMeter([.. TelemetrySource.Items.Select(static row => row.Key)]),
                    static (admitted, view) => admitted.AddView(view.Instrument, view.Shape))),
            serilogProjection: static builder => builder,
            otelExport: static builder =>
                (builder
                    .WithTracing(static tracing => tracing
                        .AddAspNetCoreInstrumentation()
                        .AddProcessor<PyroscopeSpanProcessor>())
                    .WithMetrics(static metrics => metrics.SetExemplarFilter(ExemplarFilterType.TraceBased))
                    .WithLogging()
                    .UseOtlpExporter(), builder).Item2);

    public static OpenTelemetryBuilder StoreDriver(OpenTelemetryBuilder governed) =>
        governed
            .WithTracing(static tracing => tracing.AddNpgsql())
            .WithMetrics(static metrics => metrics.AddNpgsqlInstrumentation());

    public static OpenTelemetryBuilder StoreWire<TKey, TValue>(OpenTelemetryBuilder governed) =>
        governed
            .WithTracing(static tracing => tracing
                .AddKafkaProducerInstrumentation<TKey, TValue>()
                .AddKafkaConsumerInstrumentation<TKey, TValue>())
            .WithMetrics(static metrics => metrics
                .AddKafkaProducerInstrumentation<TKey, TValue>()
                .AddKafkaConsumerInstrumentation<TKey, TValue>());

    public const int BufferRecordCapBytes = 128 * 1024;
    public const int BufferCapBytes = 64 * 1024 * 1024;
    public static readonly TimeSpan BufferFlushWindow = TimeSpan.FromSeconds(30);

    public static ILoggingBuilder GovernLogs(ILoggingBuilder logging, HostProfile profile) =>
        logging
            .AddTraceBasedSampler()
            .AddRandomProbabilisticSampler(TelemetrySignal.Log.Ratio(profile))
            .EnableRedaction()
            .EnableEnrichment(static enrich => {
                enrich.CaptureStackTraces = true;
                enrich.IncludeExceptionMessage = true;
            })
            .AddGlobalBuffer(static buffer => {
                buffer.AutoFlushDuration = BufferFlushWindow;
                buffer.MaxLogRecordSizeInBytes = BufferRecordCapBytes;
                buffer.MaxBufferSizeInBytes = BufferCapBytes;
                buffer.Rules.Add(new LogBufferingFilterRule(logLevel: LogLevel.Warning));
            });

    public static IServiceCollection EnrichContext(IServiceCollection services, HostProfile profile, RootEnricher boot) =>
        LatencySpine.Register(LogPipeline.Owner(profile).Switch(
            state: services
                .AddLogEnricher<CausalEnricher>()
                .AddStaticLogEnricher(boot)
                .AddApplicationLogEnricher()
                .AddProcessLogEnricher()
                .AddLatencyContext()
                .AddHttpClientLatencyTelemetry()
                .AddExtendedHttpClientLogging(static logging => {
                    logging.RequestHeadersDataClasses["Authorization"] = DataClassification.Credential.Marker;
                    logging.RequestHeadersDataClasses["Cookie"] = DataClassification.Credential.Marker;
                    logging.ResponseHeadersDataClasses["Set-Cookie"] = DataClassification.Credential.Marker;
                    logging.RequestQueryParametersDataClasses["token"] = DataClassification.Secret.Marker;
                    logging.RouteParameterDataClasses["tenant"] = DataClassification.HostIdentity.Marker;
                }),
            serilogProjection: static enriched => enriched.AddConsoleLatencyDataExporter(),
            otelExport: static enriched => enriched));
}
```

[GOVERNANCE_VALUES]:

| [INDEX] | [POLICY]                             | [VALUE]                            | [BINDING]                                                   |
| :-----: | :----------------------------------- | :--------------------------------- | :---------------------------------------------------------- |
|  [01]   | trace ratio, serilog-projection rows | 1.0                                | `Ratio` row, parent-based sampler                           |
|  [02]   | trace ratio, otel-export rows        | 0.1                                | `Ratio` row, parent-based sampler                           |
|  [03]   | log sampling, trace-coupled floor    | trace-coupled                      | `AddTraceBasedSampler`                                      |
|  [04]   | service-root log chatty floor        | `Log` `Ratio` column               | `AddRandomProbabilisticSampler`                             |
|  [05]   | buffered-event selection             | Warning and below                  | `LogBufferingFilterRule` row                                |
|  [06]   | metric exemplar policy               | trace-based                        | `SetExemplarFilter` at service app roots                    |
|  [07]   | metric reader cadence                | 60 s                               | `PeriodicExportingMetricReader`                             |
|  [08]   | global buffer admission              | Warning and below                  | `AddGlobalBuffer`                                           |
|  [09]   | buffer flush window                  | support-window deadline row        | `GlobalLogBuffer.Flush` on the fault transition             |
|  [10]   | destructuring caps                   | 4 deep / 1024 chars / 64 items     | `Destructure.ToMaximum{Depth,StringLength,CollectionCount}` |
|  [11]   | desktop level floor                  | Information                        | `LoggingLevelSwitch`/`MinimumLevel.ControlledBy`            |
|  [12]   | spine event-id band                  | 1000-1099                          | `LoggerMessage` `EventId` values                            |
|  [13]   | latency checkpoint vocabulary        | drain, hop, capture                | `LatencyCheckpoint`/`RegisterCheckpointNames`               |
|  [14]   | latency span export                  | drain-band flush                   | `ILatencyDataExporter` on `LatencySpine.Seal`               |
|  [15]   | serilog wire-sink batch square       | 500/2 s/10 000                     | `BatchingOptions` on `FallbackChain` sink                   |
|  [16]   | otlp span batch square               | package defaults                   | `BatchExportProcessorOptions<Activity>`                     |
|  [17]   | http route-parameter redaction       | erase                              | `HttpRouteParameterRedactionMode`/`RequestMetadata`         |
|  [18]   | otel processor admission             | test-row `BaseProcessor<Activity>` | `AddProcessor` over `CompositeProcessor<Activity>`          |
|  [19]   | tenant meter-series cap              | 256                                | `*` `AddView` `CardinalityLimit` row                        |
|  [20]   | profile signal admission             | service-root profiler endpoint     | `AddProcessor<PyroscopeSpanProcessor>()`                    |
|  [21]   | GenAI span/metric conventions        | `gen_ai.*` attributes, token usage | MCP spans on the `Rasm.AppHost` meter                       |
|  [22]   | exception log enrichment             | stack + message, file info off     | `LoggerEnrichmentOptions` on `EnableEnrichment`             |
|  [23]   | incident buffer caps                 | 64 MiB / 128 KiB / 30 s            | `GlobalLogBufferingOptions` on `AddGlobalBuffer`            |
|  [24]   | latency console export               | serilog-projection rows            | `AddConsoleLatencyDataExporter`                             |
|  [25]   | otlp egress protocol                 | `http/protobuf`                    | `OTEL_EXPORTER_OTLP_PROTOCOL`                               |
|  [26]   | otlp metric temporality              | cumulative                         | `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE`         |
|  [27]   | otlp payload compression             | gzip                               | `OTEL_EXPORTER_OTLP_COMPRESSION`                            |
|  [28]   | otlp mTLS material                   | CA, client cert, client key paths  | `OTEL_EXPORTER_OTLP_CERTIFICATE`/`_CLIENT_*` rows           |
|  [29]   | kafka wire spans and metrics         | producer + consumer per shape      | `SignalGovernance.StoreWire<TKey,TValue>`                   |
|  [30]   | resource identity triple             | `rasm`/`rasm.<svc>`/instance id    | `ConfigureResource` identity delegate at every root         |
|  [31]   | npgsql driver spans and pool metrics | store-composing service roots      | `SignalGovernance.StoreDriver`                              |
|  [32]   | profiler agent ingest address        | deploy-plane profiles endpoint     | `PYROSCOPE_SERVER_ADDRESS` row at service app roots         |
|  [33]   | resource detector rows               | host/os/process/runtime; container | `ResourceIdentity.Compose` detector chain                   |
|  [34]   | container detector gate              | containerized profile rows only    | `Containerized` column on `HostProfile.Map`                 |
|  [35]   | baggage promotion allowlist          | `rasm.tenant` + `CorrelationId`    | `AddBaggageActivityProcessor(PromotedBaggage)`              |
|  [36]   | offline queue caps                   | 50 MiB / 2 min / 48 h / 60 s lease | `OfflineQueuePolicy.Canonical` on `FileBlobProvider`        |
|  [37]   | offline queue directory              | per-owner writable queue root      | `OfflineQueue.Open(path, policy)` at each provider owner    |
|  [38]   | outbound latency breakdown           | detailed checkpoints, package on   | `AddHttpClientLatencyTelemetry`                             |
|  [39]   | outbound client-log redaction        | four `*DataClasses` taxonomy maps  | `AddExtendedHttpClientLogging` over `Marker`                |
|  [40]   | meter schema pin                     | semconv coordinate per contributor | `MeterOptions.TelemetrySchemaUrl` at `TelemetryIdentity.Mint` |

## [06]-[REDACTION_TAXONOMY]

- Owner: `DataClassification` `[SmartEnum<string>]` taxonomy with the `RedactorKind` keyless vocabulary as its redactor column; `RedactionRegistration` the binding fold.
- Cases: classification rows in escalating sensitivity order — `Internal` the non-PII internal-data tier, `Confidential` the protected business tier the durable-store retention and blob-catalog lanes classify against; one redactor kind per disclosure treatment — none, hmac, erase.
- Entry: `RedactionRegistration.Bind(ILoggingBuilder logging, IConfigurationSection hmacKeys)` returning the redaction-enabled builder; the `AddRedaction` fold maps each `RedactorKind` to its classification set and `EnableRedaction` seals the seam.
- Auto: classification flows through `[LogProperties]` and `[TagProvider]` generated methods as `LoggerMessageState.ClassifiedTag`; `EnableRedaction` applies the bound redactor before any sink or exporter observes the tag, and the `rasm.apphost.redaction.tags` count rises per redacted tag.
- Packages: Microsoft.Extensions.Compliance.Redaction, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions.
- Growth: one classification row with one redactor binding; one redactor kind is one case; zero new surface.
- Boundary: an unredacted classified value reaching any exporter is a seam violation; classification attributes annotate shapes at definition time as `DataClassificationAttribute` subclasses through the transitively arriving compliance-abstractions surface; redactor binding rides `AddRedaction(Action<IRedactionBuilder>)` with one `SetHmacRedactor(IConfigurationSection, params DataClassificationSet[])` row, one `SetRedactor<ErasingRedactor>(params DataClassificationSet[])` row, and `SetFallbackRedactor<ErasingRedactor>()` as the fail-closed default for unmapped classifications, and the fold registers with no suppression; hmac rows pseudonymize while preserving cross-event correlation, erase rows destroy the value, and credential and secret material never persists in any signal; the log seam governs the log path while the HTTP route-parameter path is a prevention row at the instrumentation root — `RequestMetadata` declares route-template parameters and `HttpRouteParameterRedactionMode` erases them so an outgoing-request span never carries an unredacted route segment, crossing to Persistence as VALUE fields on the landed rows (`Element/codec` `SnapshotCatalogRow.Classification`, `Element/identity`) — never a guard symbol and never a second registration; one redaction policy serves logs, traces, support capture, and the route-parameter prevention row, deleting call-site string scrubbing; metric tags ride the `[05]` view seam instead — the wildcard `AddView` `TagKeys` projection erases every unlisted tag, so a contributor evidence-string tag (asset key, document key, media source path) reaches an exporter only through an allow-listed view row naming its classification.

```csharp signature
[SmartEnum]
public sealed partial class RedactorKind {
    public static readonly RedactorKind None = new();
    public static readonly RedactorKind Hmac = new();
    public static readonly RedactorKind Erase = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DataClassification {
    public static readonly DataClassification None = new("none", redactor: RedactorKind.None);
    public static readonly DataClassification Operational = new("operational", redactor: RedactorKind.None);
    public static readonly DataClassification Internal = new("internal", redactor: RedactorKind.None);
    public static readonly DataClassification HostIdentity = new("host-identity", redactor: RedactorKind.Hmac);
    public static readonly DataClassification UserContent = new("user-content", redactor: RedactorKind.Erase);
    public static readonly DataClassification Personal = new("personal", redactor: RedactorKind.Hmac);
    public static readonly DataClassification Confidential = new("confidential", redactor: RedactorKind.Hmac);
    public static readonly DataClassification Credential = new("credential", redactor: RedactorKind.Erase);
    public static readonly DataClassification Secret = new("secret", redactor: RedactorKind.Erase);

    public RedactorKind Redactor { get; }

    // One projection from the taxonomy row to the compliance marker every classification-keyed
    // map consumes — the redaction sets, the HTTP-diagnostics *DataClasses maps — never a second
    // hand-built (taxonomy, value) pair at a call site.
    public Microsoft.Extensions.Compliance.Classification.DataClassification Marker => new(nameof(DataClassification), Key);
}

public static class RedactionRegistration {
    private static DataClassificationSet SetOf(DataClassification row) => new(row.Marker);

    public static ILoggingBuilder Bind(ILoggingBuilder logging, IConfigurationSection hmacKeys) {
        ArgumentNullException.ThrowIfNull(logging);
        logging.Services.AddRedaction(redaction => DataClassification.Items.Fold(redaction,
            (seam, row) => row.Redactor.Switch(
                none: seam,
                hmac: seam.SetHmacRedactor(hmacKeys, SetOf(row)),
                erase: seam.SetRedactor<ErasingRedactor>(SetOf(row)))
            ).SetFallbackRedactor<ErasingRedactor>());
        return logging.EnableRedaction(static options => options.ApplyDiscriminator = true);
    }
}
```

## [07]-[RESEARCH]

- [NATIVE_ACTIVITY]-[BLOCKED]: the exact `ActivitySource` names EF Core and grpc-dotnet emit natively and their span depth against the rejected instrumentation packages — which `AddSource` rows the service roots admit; route: `tools.assay api query` over the `Microsoft.EntityFrameworkCore` and `Grpc.Net.Client` assemblies at spec-compile, from the owning app roots.
- [MQTT_PROPERTY_READ]-[OPEN]: the exact receive-side MQTT v5 user-property member — the collection member on the received application message and its entry name/value accessors — the `MqttRuntime.Properties` getter delegate binds; route: `.api/api-mqtt.md` extension via `tools.assay api query` over `MQTTnet` at the central pin.
- [OTLP_RETRY_HOOK]-[OPEN]: the exporter-side member or environment row that binds `OfflineQueue` to the OTLP transmission-retry path — the exporter's disk-retry seat versus an explicit export-failure callback; route: `tools.assay api query` over `OpenTelemetry.Exporter.OpenTelemetryProtocol` at the central pin, `.api/api-otel-exporter.md` extension on resolution.
