# [APPHOST_DIAGNOSTICS_AND_TELEMETRY]

Telemetry identity, the correlation spine, log projection, signal governance, and the suite data-classification taxonomy form one diagnostics concern owned by Rasm.AppHost — the L3 lacing over the kernel signal capsule: this platform alone stacks the OTel SDK, correlation, tenancy, and host evidence onto the surface the strata contribute, referencing that composed surface upward as knowledge only. Owned axes: the `ForeignSource` admission vocabulary over the kernel `TelemetrySource` roster, the `TelemetryDomain` roster every branch-minted `rasm.*` name resolves against, the four-row `TelemetrySignal` governance set (traces, metrics, logs, profiles), the `LogPipeline` arbitration column, and the nine-row `DataClassification` taxonomy enforced at every exporter seam. One spine composes Microsoft.Extensions telemetry policy over the OpenTelemetry SDK, `LoggerMessage` source generation at lib level, Serilog projection at desktop composition roots, and a continuous-profiling rail linking CPU profiles to spans; siblings emit through minted identities and never construct telemetry owners of their own. OTel GenAI semantic conventions (`gen_ai.*` attributes, MCP spans, `gen_ai.usage.input_tokens`/`output_tokens`) ride the trace and metric signals so AppHost telemetry aligns with the agentic surface the host serves.

## [01]-[INDEX]

- [02]-[TELEMETRY_IDENTITY]: Foreign-source admission vocabulary and the app-identity lacing over the kernel mint.
- [03]-[CORRELATION_SPINE]: One boot-minted root id, the OTel tenancy mirror, and the adopted W3C trace-context every hop continues.
- [04]-[LOG_PROJECTION]: Generated lib-level delegates and provider-keyed pipeline-owner arbitration.
- [05]-[SIGNAL_GOVERNANCE]: Branch domain roster, per-signal sampling, per-signal exporter policy, durable OTLP buffering, enrichment, and drain flush.
- [06]-[REDACTION_TAXONOMY]: Nine classification rows binding redactor policy at every exporter seam.

## [02]-[TELEMETRY_IDENTITY]

- Owner: `ForeignSource` `[SmartEnum<string>]` under the `ComparerAccessors.StringOrdinal` accessor — the foreign instrumentation scopes this platform admits beside the kernel `TelemetrySource` roster, each row carrying the signal set it publishes.
- Cases: two builtin rows — `System.Runtime` publishing metrics alone, `System.Net.Http` publishing metrics and traces; every minted Rasm scope is a kernel `TelemetrySource` row this platform admits whole, so the vocabulary here holds only what a foreign library publishes on its own, and a package-mounted instrumentation whose instruments its registration verb alone constructs enters at `[05]-[SIGNAL_GOVERNANCE]` through that verb, never as a row here.
- Entry: `ForeignSource.Admitting(TelemetrySignal signal)` is the one admission projection — the kernel roster whole and every foreign row publishing that signal — feeding `AddSource` and `AddMeter` at `[05]-[SIGNAL_GOVERNANCE]`; mint mechanics are the kernel signal capsule's `TelemetryIdentity.Mint(factory, scope, version, schemaUrl, tags)`, and this platform's composition laces app identity at every mint call: the port's `Scope` string names the meter, the `SchemaUrl` coordinate stamps `MeterOptions.TelemetrySchemaUrl`, and the boot `CorrelationId` rides as the one meter tag.
- Auto: builtin rows feed GC, threadpool, exception-rate, and HttpClient duration streams through `AddMeter` with zero package, and a metric-only row never opens an empty `ActivitySource` because the projection filters on the published signal; instrument identity de-duplicates by name, so name, unit, and description are `InstrumentSpec` declaration facts and a drifted unit forks the stream at its one registry row, never at a call site; the AppHost spine and domain instruments live on the ONE `HostInstruments` roster at Observability/instruments#INSTRUMENT_CATALOG.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, Microsoft.Extensions.Telemetry.Abstractions, BCL inbox.
- Growth: one foreign instrumentation scope is one `ForeignSource` row with its signal set; a newly minted Rasm package is a kernel `TelemetrySource` row every admission here inherits with no edit; zero new surface.
- Boundary: a process-static `Meter` field outliving its provider is the named defect — minted pairs are `IMeterFactory`-owned and unload with the host ALC; the host builder registers the metrics services on every path including the empty builder, so `IMeterFactory` arrives with zero registration row; every instrument enters through its `InstrumentSpec` declaration on the contributor row set, so the minted pair is the registration payload `TelemetryContributorPort` carries inward, deleting handler-local `ActivitySource` and `Meter` owners; package self-identity is the kernel capsule's, so re-listing a Rasm scope here forks the vocabulary an emitter beyond this platform's reach already spells through the string-typed port seam.

```csharp signature
// Foreign scopes only: a Rasm-minted meter or source is a kernel TelemetrySource row, and a copy here
// forks the identity vocabulary the string-typed contributor seam already carries.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ForeignSource {
    public static readonly ForeignSource SystemRuntime = new("System.Runtime", FrozenSet.Create(TelemetrySignal.Metric));
    public static readonly ForeignSource SystemNetHttp = new("System.Net.Http", FrozenSet.Create(TelemetrySignal.Metric, TelemetrySignal.Trace));

    public FrozenSet<TelemetrySignal> Signals { get; }

    public static Seq<string> Admitting(TelemetrySignal signal) =>
        toSeq(TelemetrySource.Items).Map(static row => row.Key)
            + toSeq(Items).Filter(row => row.Signals.Contains(signal)).Map(static row => row.Key);
}
```

## [03]-[CORRELATION_SPINE]

- Owner: `Correlation` the ONE ambient causal-frame surface — the boot-minted `CorrelationId` stamp, the composition-supplied `OtelBaggage` `TenantMirror` row completing the kernel's ambient-store set, and the capture/restore pair over both halves; `TenantAdoption` the ingress trust axis every continuation names; `RootEnricher` `IStaticLogEnricher` stamps the resource-identity projection once per provider; `CausalEnricher` `ILogEnricher` stamps the request-scoped correlation per record; `TraceContext` the W3C distributed-trace propagation fold injecting and extracting `traceparent`/`tracestate` over every registered transport carrier so a remote span continues the parent trace.
- Cases: three ambient stores partition by owner and this platform supplies the third — the kernel `AsyncLocal` tenancy slot and the BCL `Activity` chain are the kernel's own rows, and `OtelBaggage` seats the SDK `Baggage.Current` store an OTel-free S0 assembly cannot name; two enrichment seats split by cost class — `RootEnricher` for the per-provider resource identity, `CausalEnricher` for the per-record ambient correlation key; two `TenantAdoption` rows close the ingress trust axis — a trusted intra-estate carrier adopts its wire tenancy and a foreign one refuses it; two propagation directions and the inbound continued-span start on `TraceContext` — the generic `Inject`/`Extract`/`Continue` members take any carrier with a getter/setter delegate pair, and `Continue` extracts, resolves tenancy through its carrier's adoption row, seeds `Baggage.Current`, and starts the inbound `Activity` from the extracted context; the gRPC `Metadata` pair and MQTT v5 publish builder are verified adapters, while MQTT receive, NATS headers, and CloudEvents attributes bind their getter/setter delegates at the transport owner until both catalog tiers carry their concrete member rows.
- Law: W3C trace-context is the identity wire — `ActivityContext` carries the 16-byte `ActivityTraceId` crossing every hop of one trace beside the 8-byte `ActivitySpanId` naming one hop's parent — and `Continue` seats the extracted context as the started `Activity`'s parent, so an inbound stamp is adopted whole and a fresh root mints only where extraction yields none. `CorrelationId` rides `Baggage` and the `Rasm/Domain/telemetry#CAUSAL_FRAME` two-half stamp rides `HlcStampWire` on the receipt envelope, each independent evidence beside the context; neither occupies a trace or span id slot, and the Serilog trace-id and span-id fields bind the live `Activity` ids alone.
- Entry: `Correlation.Stamp` is one entry discriminating on the value it scopes — a `CorrelationId` seats the boot root, a `TenantContext` seats tenancy across every registered store — each returning the restoring ambient scope; `Correlation.Capture()` snapshots the log, baggage, and tenancy triple for deferred work and `Correlation.Restore(value)` rehydrates all three at work entry; `TraceContext.Inject<TCarrier>(carrier, set)` writes the active context, `TraceContext.Extract<TCarrier>(carrier, get)` reads the parent context, and `TraceContext.Continue<TCarrier>(source, carrier, get, name, adoption, kind)` extracts the parent, resolves the carrier's tenancy under its `TenantAdoption` row, scopes `Baggage.Current`, and starts the continued `Activity` through the composition-owned source from `TelemetryIdentity.Mint`; the `Metadata` overloads are the gRPC adapter the Wire/companion#CONTROL_SERVICE handler reads, the `MqttApplicationMessageBuilder` overload the publish-edge adapter `MqttLane.Write` threads before `Build()`, and the `MqttApplicationMessage` overload the receive pump continues under a consumer kind.
- Auto: one boot mint stamps `LogContext` properties, `Baggage`, meter tags, receipts, and support manifests — deletes per-call-site correlation parameters across the suite; the two enrichers feed `IEnrichmentTagCollector` under one bounded prefix — the causal seat reads the scoped baggage value through `AddLogEnricher<CausalEnricher>`, the identity seat is the pre-constructed projection through `AddStaticLogEnricher(RootEnricher)` because the resolved record fixes it at composition, never from DI activation; pooled-callback, native-callback, and manual-thread ambient breaks share one repair — `CorrelationFrame` captures the log, baggage, and tenancy triple, and `Restore` scopes all three at deferred-work entry, so a receipt minted on a pooled thread carries the tenant its originating request admitted rather than reading single-tenant off an empty slot; `TraceContext` rides the same `Correlation.Spine` composite, so the W3C `traceparent`/`tracestate` carrier and the `Baggage` carrier inject and extract in one pass and a continued remote span shares the in-process correlation id automatically.
- Packages: Rasm, OpenTelemetry, Serilog, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core, Grpc.Core.Api, MQTTnet, BCL inbox.
- Growth: a new stamped carrier is one stamp row inside `Stamp` with one policy value; a new ambient store is one `TenantMirror` row beside `OtelBaggage` that every existing call site inherits with no edit; a new identity dimension is one `ProfileIdentity.ResourceAttributes` row both the resource and the log seat inherit, a new request dimension one `CausalEnricher` line; a new propagation carrier is one getter/setter adapter pair over the generic `Inject`/`Extract` on the same `Spine` composite beside the `TenantAdoption` row its trust class already carries, never a second tracer; zero new surface.
- Boundary: the composite registers as `Propagators.DefaultTextMapPropagator` and crosses every hop through `TextMapPropagator.Inject` and `TextMapPropagator.Extract`, riding gRPC metadata on the local-ipc leg; `TraceContext` is the seam owner of every crossing — the propagation mechanics live here while each transport boundary consumes its adapter pair, so a per-transport hand-rolled `traceparent` header write is the deleted form; MQTT publish writes v5 user properties through the catalogued non-obsolete `WithUserProperty(string name, ReadOnlyMemory<byte> value)` builder overload and receive reads them through the `MqttApplicationMessage` `Continue` overload whose ordinal-matched getter decodes `MqttUserProperty.ValueBuffer` — both legs on the buffer pair the package's own obsolescence notes point at, so the carrier adapter family closes in both directions on one transport and no leg hand-formats a header; NATS and CloudEvents adapters compose Persistence-side because NATS carries no OTel instrumentation by design — manual inject and extract are the contract — and their concrete setter/getter bodies land beside the egress legs, never a second spine; immutable `Baggage.Current` is the one ambient correlation owner and `OtelBaggage` is its ONE tenancy writer — a page reading tenancy off a raw store rather than the kernel `TenantContext.Current` accessor reads whichever of the three stores that page happens to know, which is the split-brain this row closes, and a second `TenantMirror` registration over the same store double-writes and double-restores one entry; ingress tenancy is ADMITTED, never inherited — `TenantAdoption` carries no default because trust is a property of the carrier a transport owner alone knows, an adopting leg seats the wire entry into the kernel slot so span promotion and the metric fold answer one tenant, and a refusing leg CLEARS that entry from the seated baggage so a foreign claim tags no span with a tenant every RLS predicate and receipt answers root for; a request value placed in `RootEnricher` is a bug and an identity constant placed in `CausalEnricher` is waste — the cost-class split is structural, and the captured `CorrelationFrame`, never ambient state read at execution time, seeds deferred children; every stamp, restore, and continuation scope restores prior baggage on dispose; every continuation receives its minted source, and a process-static source bypassing the factory scope is forbidden.

```csharp signature
// Ingress trust is a CARRIER property, so it rides a row the transport adapter names rather than a boolean a
// call site guesses. A gRPC hop between estate processes carries a tenancy this process already admitted; a
// public HTTP edge carries whatever a client typed.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TenantAdoption {
    public static readonly TenantAdoption Refused = new("refused", adopt: static _ => Option<TenantContext>.None);
    public static readonly TenantAdoption Adopted = new("adopted", adopt: Correlation.Tenanted);

    [UseDelegateFromConstructor]
    public partial Option<TenantContext> Adopt(Baggage extracted);
}

public sealed record CorrelationFrame(ILogEventEnricher Log, Baggage Baggage, TenantContext Tenant);

public static class Correlation {
    public static readonly TextMapPropagator Spine =
        new CompositeTextMapPropagator([new TraceContextPropagator(), new BaggagePropagator()]);

    // Composition supplies the one ambient store the kernel capsule cannot name: TenantContext.Stamp
    // threads its own AsyncLocal slot and prepends the BCL Activity chain, and this row adds the OTel store
    // that exporter-side promotion processors read. Null values REMOVE on SetBaggage, so restore and clear
    // are one call and an emptied entry never lingers as a zero-length tenant on a pooled continuation.
    public static readonly TenantMirror OtelBaggage = new(
        Store: nameof(Baggage),
        Read: static () => Optional(Baggage.GetBaggage(TenantContext.TenantSlot)),
        Write: static entry => ignore(Baggage.SetBaggage(
            TenantContext.TenantSlot,
            entry.Match<string?>(Some: static held => held, None: static () => null))));

    public static CorrelationId Mint() => CorrelationId.Create(Guid.CreateVersion7());

    public static IDisposable Stamp(CorrelationId root) {
        var prior = Baggage.Current;
        Baggage.Current = prior.SetBaggage(CorrelationId.Slot, root.ToString());
        return Scope(prior, LogContext.PushProperty(CorrelationId.Slot, root.ToString()));
    }

    // One tenancy entry serves this platform: every caller spells the value it scopes while the registered
    // mirror set rides here, so no call site threads a TenantMirror and no second registration seats a
    // rival store.
    public static IDisposable Stamp(TenantContext tenant) => tenant.Stamp(OtelBaggage);

    // Wire tenancy carries the kernel's invariant ID TEXT and no slug, so an adopted frame spells its slug from
    // that text and a slug-keyed configuration overlay resolves at the authorization boundary holding the
    // roster. Admission reads the Root row's own render for width and alphabet rather than re-spelling the
    // format, so a garbled carrier reads single-tenant instead of faulting the propagation seam on parse, and a
    // zero entry lands on Root rather than on a partitioning zero row — the sentinel the absent-tenant law
    // forecloses, which a reconstructed row would otherwise pass by carrying a slug Root does not.
    internal static Option<TenantContext> Tenanted(Baggage extracted) =>
        Optional(extracted.GetBaggage(TenantContext.TenantSlot))
            .Filter(static text => text.Length == TenantContext.Root.Entry.Length && text.All(char.IsAsciiHexDigit))
            .Map(static text => new TenantContext(TenantId.Of(text), text))
            .Filter(static held => held.TenantId != TenantContext.Root.TenantId);

    public static CorrelationFrame Capture() => new(LogContext.Clone(), Baggage.Current, TenantContext.Current);

    // Baggage seats FIRST so the tenancy stamp writes its entry into the restored value rather than into
    // whatever the pooled thread carried; capturing the tenant is what makes the restore total, because the
    // baggage snapshot carries the tenant TEXT while the kernel's AsyncLocal slot carries the tenant VALUE
    // every receipt, RLS predicate, and partition read resolves.
    public static IDisposable Restore(CorrelationFrame captured) {
        var prior = Baggage.Current;
        Baggage.Current = captured.Baggage;
        return Scope(prior, LogContext.Push(captured.Log), Stamp(captured.Tenant));
    }

    internal static IDisposable Scope(Baggage prior, params ReadOnlySpan<IDisposable?> held) =>
        new CorrelationScope(prior, [.. held]);
}

file sealed class CorrelationScope(Baggage prior, ImmutableArray<IDisposable?> held) : IDisposable {
    int disposed;

    // Reverse admission order releases an inner scope before its outer, and the baggage restore runs in the
    // finally whatever a held scope throws — a leaked ambient value outlives every request it then poisons.
    public void Dispose() {
        if (Interlocked.Exchange(ref disposed, 1) != 0) return;
        try {
            ignore(toSeq(held).Rev().Iter(static scope => scope?.Dispose()));
        } finally {
            Baggage.Current = prior;
        }
    }
}

public static class TraceContext {
    // One generic carrier spine: every transport adapter is a getter/setter delegate pair over
    // these three members, never a per-transport tracer or a hand-rolled header write.
    public static TCarrier Inject<TCarrier>(TCarrier carrier, Action<TCarrier, string, string> set) =>
        (fun(() => Correlation.Spine.Inject(
            new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current),
            carrier,
            set))(), carrier).Item2;

    public static PropagationContext Extract<TCarrier>(TCarrier carrier, Func<TCarrier, string, IEnumerable<string>> get) =>
        Correlation.Spine.Extract(default, carrier, get);

    // Adoption has NO default: a carrier's trust class is the one fact this seam cannot derive, and a defaulted
    // arm would make every new transport inherit whichever answer read safer the day it was written.
    public static IDisposable Continue<TCarrier>(ActivitySource source, TCarrier carrier, Func<TCarrier, string, IEnumerable<string>> get, string name, TenantAdoption adoption, ActivityKind kind = ActivityKind.Server) {
        var parent = Extract(carrier, get);
        Option<TenantContext> admitted = adoption.Adopt(parent.Baggage);
        var prior = Baggage.Current;
        // Refusal REMOVES the entry rather than leaving it riding: promotion processors read baggage where the
        // instrument fold reads the kernel slot, so an unadopted entry tags every span of this request with a
        // tenant its metrics, receipts, and RLS predicates all answer root for.
        Baggage.Current = admitted.IsSome ? parent.Baggage : parent.Baggage.RemoveBaggage(TenantContext.TenantSlot);
        // Activity starts FIRST so the kernel's own span mirror writes the adopted entry onto this span rather
        // than the caller's, and reverse-order release then restores tenancy while that span is still current.
        return Correlation.Scope(prior,
            source.StartActivity(name, kind, parent.ActivityContext),
            admitted.Match<IDisposable?>(Some: static tenant => Correlation.Stamp(tenant), None: static () => null));
    }

    // gRPC metadata adapter rows — the local-ipc control hop the Wire/companion handler reads.
    static IEnumerable<string> Get(Metadata carrier, string key) =>
        carrier.GetAll(key).Select(static entry => entry.Value);

    public static Metadata Inject(Metadata carrier) =>
        Inject(carrier, static (c, key, value) => c.Add(key, value));

    public static PropagationContext Extract(Metadata carrier) => Extract(carrier, Get);

    public static IDisposable Continue(ActivitySource source, Metadata carrier, string name, TenantAdoption adoption) =>
        Continue(source, carrier, Get, name, adoption);

    // Both v5 legs ride the non-obsolete buffer pair: publish writes `ReadOnlyMemory<byte>` and receive reads
    // `ValueBuffer` through `ReadValueAsString`, because `MqttUserProperty.Value` and the `(string, string)`
    // constructor both carry `[Obsolete]` pointing here. An absent collection or an unmatched name yields the
    // empty extraction the propagator already treats as a root, so neither leg guards a null before the fold.
    public static MqttApplicationMessageBuilder Inject(MqttApplicationMessageBuilder carrier) =>
        Inject(carrier, static (c, key, value) =>
            ignore(c.WithUserProperty(key, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(value)))));

    public static IDisposable Continue(ActivitySource source, MqttApplicationMessage carrier, string name, TenantAdoption adoption) =>
        Continue(source, carrier, Get, name, adoption, ActivityKind.Consumer);

    // Ordinal name match, one decode per hit: entries carry raw buffers, so a culture-sensitive compare or a
    // decode-then-compare fold pays UTF-8 work for every entry the propagator never asked for.
    static IEnumerable<string> Get(MqttApplicationMessage carrier, string key) =>
        (carrier.UserProperties ?? []).Where(entry => string.Equals(entry.Name, key, StringComparison.Ordinal))
            .Select(static entry => entry.ReadValueAsString());
}

// Log-record identity IS the resource projection: the shipped application enricher writes an unqualified
// `service.name` and the deprecated `deployment.environment` key, so leaving it seated puts two values of one
// dimension on two planes and a query joining a log record to a metric series compares neither. One projection
// feeds the detector and this seat, so both planes read byte-identical and a new identity fact moves both.
// Correlation is deliberately absent here — the boot mint rides `Baggage` from its own stamp and reaches every
// record through `CausalEnricher`, so a per-provider copy would be a third spelling of one process fact.
public sealed class RootEnricher(ImmutableArray<KeyValuePair<string, object>> identity) : IStaticLogEnricher {
    public void Enrich(IEnrichmentTagCollector collector) =>
        ignore(toSeq(identity).Iter(row => collector.Add(row.Key, row.Value)));
}

public sealed class CausalEnricher : ILogEnricher {
    public void Enrich(IEnrichmentTagCollector collector) =>
        Optional(Baggage.Current.GetBaggage(CorrelationId.Slot))
            .Iter(value => collector.Add($"{nameof(Correlation)}.causal", value));
}
```

## [04]-[LOG_PROJECTION]

- Owner: `LogPipeline` `[SmartEnum<string>]` arbitration column; `SpineLog` generated delegates; `SerilogProjectionPolicy` shaping surface; `SpineLossFold` failure listener.
- Cases: one pipeline row per delivery mandate — a bound OTLP-export provider takes otel-export, an unbound one projects through Serilog; the `Owner` arbitration is the total assignment.
- Entry: `LogPipeline.Owner(ConsumptionProfile profile)` — the total arbitration projection; `SerilogProjectionPolicy.Shape(LoggerConfiguration)` composes the six rails and freezes them on `CreateLogger`.
- Auto: generated delegates carry stable `EventId` and `EventName`; `[LogProperties]` expands typed payloads into bounded tags with classification intact, `[TagProvider]` projects a foreign type that carries no annotation, and `[TagName]`/`[LogPropertyIgnore]` rename and elide at the declaration; the wire sink rides one `BatchingOptions` latency/throughput square while `Fallible` wraps it in a `FailureListenerSink` that projects sink failure through the composition-injected receipt-backed `SpineLossFact` delegate, `AuditTo` propagates it to the caller, `FallbackChain` reroutes on synchronous throw, and `Conditional` forks the error-and-above tier to the hot sink.
- Packages: Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Telemetry.Abstractions, Serilog, Thinktecture.Runtime.Extensions.
- Growth: one spine event is one generated-delegate row inside the 1000-1099 EVENT stride (`FaultBand.SpineEvents`); one delivery mandate is one `LogPipeline` row the `Owner` fold selects off a capability column; one sink-loss class is one `SpineLossFold` fact row; zero new surface.
- Boundary: `Rasm.AppHost` IS the branch's telemetry composition owner and holds Serilog, exporter, and SDK types by charter — the no-exporter-below-composition law scopes to the S0-S2 library tiers, where a package emits `ILogger` and its minted `Meter` alone and a Serilog type, an exporter, or an ambient sink is the app-coupling defect that law forecloses; static `Log` facade calls are deleted at every tier; the host bridge is the service-aware `AddSerilog(IServiceCollection, Action<IServiceProvider, LoggerConfiguration>)` overload whose configuration action runs `SerilogProjectionPolicy.Shape`, every sink is an app-root pin, and the boot window logs through `CreateBootstrapLogger()`, frozen into the host pipeline when that bridge registers, so no startup fault predates the pipeline; destructuring pins all three caps — depth, string length, collection count — because a pipeline accepting foreign graphs is a payload-bomb seam; `CloseAndFlush` is a ranked drain participant; exactly one pipeline owner per profile row, never both on one signal; `Filter.ByExcluding` holds lifetime-noise categories out of the pipeline by `Matching.FromSource` construction, `Destructure.With` binds the redaction-preserving `IDestructuringPolicy` so a custom shaper never strips classification, and `ForContext` is the emission-side source-keyed derivation the generated delegates ride, never a second `Shape` call; `SpineLossFold` implements `ILoggingFailureListener.OnLoggingFailed(object sender, LoggingFailureKind kind, string message, IReadOnlyCollection<LogEvent>? events, Exception? exception)` and projects only the exception type, numeric code, and redacted bounded detail into `SpineLossFact`; the raw exception remains callback-local and never enters the receipt-backed fact stream; `SelfLog.Enable` is the never-throwing floor beneath the rail; `WriteTo.Fallible(configureSink, listener)` wraps the wire-sink fallback chain in a `FailureListenerSink`, and a sink outside `Fallible` is unobserved best-effort; the test row installs `AddFakeLogging` and asserts through `FakeLogCollector` snapshots, never sink text.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LogPipeline {
    public static readonly LogPipeline SerilogProjection = new("serilog-projection");
    public static readonly LogPipeline OtelExport = new("otel-export");

    // Arbitration reads one axis fact: a composition root binding an OTLP collector takes the export
    // pipeline, and a root binding none projects locally — the delivery mandate IS the bound provider.
    public static LogPipeline Owner(ConsumptionProfile profile) =>
        profile.OtlpExport ? OtelExport : SerilogProjection;
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

    [LoggerMessage(EventId = 1003, EventName = nameof(PeersRefused), Level = LogLevel.Warning, Message = "federation admitted no peer: {Count} server rows refused")]
    public static partial void PeersRefused(ILogger logger, long count, [TagName("federation.fault")] string fault);
}

public sealed record SpineLossFact(
    string Sink,
    LoggingFailureKind Kind,
    string Detail,
    int Count,
    Option<string> ExceptionType,
    Option<int> ExceptionCode);

public sealed class SpineLossFold(Action<SpineLossFact> emit, Redactor redactor) : ILoggingFailureListener {
    const int DetailCap = 512;

    public void OnLoggingFailed(object sender, LoggingFailureKind kind, string message, IReadOnlyCollection<LogEvent>? events, Exception? exception) =>
        emit(new SpineLossFact(
            sender.GetType().Name,
            kind,
            Bounded(redactor, exception is null ? message : $"{message}: {exception.Message}"),
            events?.Count ?? 0,
            Optional(exception?.GetType().FullName),
            exception is null ? None : Some(exception.HResult)));

    static string Bounded(Redactor redactor, string detail) {
        var sanitized = redactor.Redact(detail);
        return sanitized.Length <= DetailCap ? sanitized : sanitized[..DetailCap];
    }
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

- Owner: `TelemetryDomain` `[SmartEnum<string>]` the branch domain roster under Tier-0 `[08]-[OBSERVABILITY_CONFORMANCE]` — each row a capability subject carrying its `Head`/`Measure` name projections, so every `rasm.*` instrument name and rasm-owned dimension key in this branch resolves a row or refuses at admission; `TelemetrySignal` `[SmartEnum<string>]` governance rows and the `SignalGovernance` registration fold; `LatencyCheckpoint` `[SmartEnum<string>]` the in-flight phase vocabulary; `LatencySpine` the checkpoint recorder; the admitted `PyroscopeSpanProcessor` the profile-to-span linking `BaseProcessor<Activity>`; `ResourceIdentity` the one detector-composed `Action<ResourceBuilder>` every provider owner consumes; `TelemetryComposition` the disposable composition capsule every governance entry folds — resolved row, boot correlation, contributed roster, offline policy, receipt delegate, clock, the process `SpanBand` folded from the contributed trace planes, and the opened per-signal queue and transport sets; `OtlpOfflinePolicy`/`OtlpOfflineQueue`/`PersistentOtlpHandler`/`OfflineDisposition` the branch-owned durable OTLP egress and its disposition vocabulary; `OtlpTrust` the mutual-auth material a replaced transport re-reads from the same environment rows the shipped factory owned.
- Cases: one governance row per signal — trace, metric, log, profile — each binding ratio, buffering, redaction, and OTLP-egress policy, the export column selecting which three open a durable queue; one latency checkpoint row per measured phase — drain, hop, capture; six offline dispositions covering every outcome the queue can observe — accept, capacity refusal, replay, deferral, corruption, and the bounded-drain exit.
- Entry: `TelemetryComposition.Of(ResolvedProfile resolved, CorrelationId root, Action<OtlpOfflineFact> emit, TimeProvider time, params ReadOnlySpan<TelemetryContributorPort> contributors)` opens the composition, its queue set, and the one `SpanBand`, and `Dispose` releases all three at the telemetry drain band; `SignalGovernance.Rostered(TelemetryContributorPort)` gates every contributed port before it mounts, returning the port on the typed rail or a refusal naming the unrostered spelling; `SignalGovernance.Views(Seq<TelemetryContributorPort>)` projects the one per-instrument view function both provider owners bind; `SignalGovernance.Govern(IServiceCollection services, TelemetryComposition composition)` returning the host-owned `OpenTelemetryBuilder`, `SignalGovernance.GovernLogs(ILoggingBuilder, TelemetryComposition)` binding the sampler, redaction, and incident-buffer rails on the `ILogger` floor, and `SignalGovernance.EnrichContext(IServiceCollection, TelemetryComposition)` seating the two enricher rows, the latency ledger, and the outbound client-log taxonomy; `ResourceIdentity.Compose(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra)` the one identity delegate — the resolved-record detector ahead of the contrib chain and the deployment override behind it, with composition facts riding as data; `SignalGovernance.StoreDriver(OpenTelemetryBuilder)` and `SignalGovernance.StoreWire<TKey, TValue>(OpenTelemetryBuilder)` the store-composing service-root rows — driver subscription shape-free and once, wire instrumentation once per message shape; `LatencySpine.Mark(ILatencyContext context, CheckpointToken phase)` records one checkpoint, `LatencySpine.Seal` freezes the context for export at drain.
- Auto: provider `ForceFlush` and `Shutdown` ride the telemetry drain band; the fault transition lands the `GlobalLogBuffer.Flush` window inside support capture; `AddRandomProbabilisticSampler` carries a `RandomProbabilisticSamplerFilterRule` row keyed by maximum level so it thins the chatty floor and never the error ceiling, while a `LogBufferingFilterRule` row holds the verbose tiers until an incident flushes them, bounded by the `GlobalLogBufferingOptions` caps — record size, buffer size, auto-flush window — so the incident buffer never runs unbounded; `AddHttpClientInstrumentation` binds `HttpClientTraceInstrumentationOptions` — `FilterHttpRequestMessage` drops the loopback leg, `EnrichWithException` records through `Activity.AddException` so the exception event carries the runtime's own `exception.type`/`message`/`stacktrace` grammar and the listener-installed `ExceptionRecorder` shapes it in one place, and URL-query redaction stays the package default; `Views` binds ONE `AddView` predicate resolving each published instrument against the contributed roster — a rostered stream takes its declaring row's `Dimensions` beside the one tenancy key as `TagKeys` under the `SeriesCap` budget, and a foreign stream keeps its semconv vocabulary under the same budget — so an undeclared tag reaches no exporter, the per-tenant `TenantContext.Tags` dimension stays inside a bounded series budget on every minted meter, and one instrument yields exactly one stream; the service-app-root metric exemplar policy rides `SetExemplarFilter` per the trace-based governance row; egress binds per signal rather than through one cross-signal call, so each signal carries its own batch square, its own temporality and reader cadence, and its own durable transport, and the three `AddOtlpExporter` rows are what make those knobs reachable at all — the cross-signal `UseOtlpExporter` seat exposes exporter options, reader options, and processor options through `internal` types alone, so the cross-signal seat reaches no per-signal policy value this page pins; `Egress` binds every exporter arm — the `WireProtocol`/`WireCompression` estate pins stamp first because the exporter parses its `OTEL_EXPORTER_OTLP_*` wire keys at options construction and the deploy plane publishes the endpoint row alone, so a pin left to those keys resolves the shipped gRPC-and-uncompressed defaults — and an `OtlpOfflinePolicy` armed by durable disk BESIDE a bound OTLP provider opens one queue per exported signal at composition, where `Egress` then swaps that signal's transport for `PersistentOtlpHandler`, which answers `Accepted` once a failed batch is on disk and opportunistically replays the tail through the next request that proved the endpoint good, bounded by drain window and batch so an export call never stalls on a deep queue, and it carries both message-handler legs because the http+protobuf exporter drives the SYNCHRONOUS one; `EnableEnrichment` activates the `RootEnricher`/`CausalEnricher` seats and binds `LoggerEnrichmentOptions` — `CaptureStackTraces` and `IncludeExceptionMessage` admit exception frames onto the log signal behind the redaction seam, and `UseFileInfoForStackTraces` stays off because file and line are leak-bearing; the serilog-projection rows add `AddConsoleLatencyDataExporter` so a desktop or test host reads latency spans live with zero wire cost; the latency vocabulary registers once through `RegisterCheckpointNames` at composition, `ILatencyContextTokenIssuer.GetCheckpointToken(string)` resolves each name to a `CheckpointToken`, and runtime code records through those resolved handles only — durations never derive from stamp differences; a value-bearing `MeasureToken` recording from `GetMeasureToken(string)` is a forward row admitted only when a measure consumer exists; `ResourceIdentity.Compose` runs the minted `service.namespace`/`service.name`/`service.instance.id` triple delegate first, then chains `AddHostDetector`/`AddOperatingSystemDetector`/`AddProcessDetector`/`AddProcessRuntimeDetector` always-on and `AddContainerDetector` on the OCI-vehicle rows alone — detectors ENRICH and never replace the mint, each contributing only the semconv attributes it resolves (`host.*`, `os.*`, `process.*`, `process.runtime.*`, `container.id`), placement dimensions no backend derives from the triple, and `AddEnvironmentVariableDetector` tails the chain so the deploy plane's `OTEL_RESOURCE_ATTRIBUTES` outranks every row ahead of it; `AddBaggageActivityProcessor(SignalGovernance.PromotedBaggage)` promotes the allowlisted `rasm.tenant` and `CorrelationId` baggage entries onto every span at start, so a backend groups spend, latency, and traces by tenant with zero per-call-site tagging; `AddHttpClientLatencyTelemetry()` installs the per-phase checkpoint handler over every named `HttpClient` — name-resolution versus connection versus server time at checkpoint cost, `EnableDetailedLatencyBreakdown` the package-default breakdown — and `AddExtendedHttpClientLogging` replaces the built-in client logger with the redaction-aware form whose four `*DataClasses` maps bind the `[06]` taxonomy through `DataClassification.Marker`, bespoke tags entering as `AddHttpClientLogEnricher<T>` rows.
- Receipt: `LatencyData` — the frozen checkpoint spans `ILatencyDataExporter` exports at the drain band, one span per drain, hop, and capture phase; `OtlpOfflineFact` — the durable-egress evidence every queue outcome projects through the composition-injected receipt delegate onto the `Observability/instruments#RECEIPT_PROJECTION` offline arm.
- Packages: OpenTelemetry.Extensions.Hosting, OpenTelemetry, OpenTelemetry.Extensions, OpenTelemetry.PersistentStorage.FileSystem, OpenTelemetry.Resources.Host, OpenTelemetry.Resources.OperatingSystem, OpenTelemetry.Resources.Process, OpenTelemetry.Resources.ProcessRuntime, OpenTelemetry.Resources.Container, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime, OpenTelemetry.Instrumentation.GrpcNetClient, OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Instrumentation.ConfluentKafka, OpenTelemetry.Instrumentation.EntityFrameworkCore, Npgsql.OpenTelemetry, Microsoft.Extensions.Telemetry, Microsoft.Extensions.Telemetry.Abstractions, Microsoft.Extensions.Http.Diagnostics, OpenTelemetry.Exporter.OpenTelemetryProtocol, Pyroscope.OpenTelemetry, Microsoft.Extensions.Diagnostics.Testing.
- Growth: a new capability subject is one `TelemetryDomain` row and a second package emitting under a standing subject adds none — the roster grows on subjects, never on emitters; a contributor minting on its own load-context meter is one `Published` roster on its port that the standing gate and the standing view predicate already read; a new transport-trust coordinate is one `OtlpTrust` row beside its governance variable; one governance decision is one policy value row; one stream reshaping is one `Dimensions` edit at the roster that declares the instrument, which the one view predicate already reads — a second `AddView` row is the shape that mints a duplicate stream; one measured phase is one `LatencyCheckpoint` row recorded by one `LatencySpine.Mark` call; a new store message shape is one `StoreWire<TKey, TValue>` closure at the composing root; a new resource dimension is one detector row inside `ResourceIdentity.Compose` and a new composition fact one `TelemetryComposition` column; a signal crossing onto the OTLP wire is one `Exported` value the queue set and the exporter roster both fold; a new promoted baggage key is one `PromotedBaggage` pattern row; a new offline outcome is one `OfflineDisposition` row the one counter already partitions on; zero new surface.
- Boundary: this platform admits all span custody — one `SpanBand` over contributed `Planes`, registered beside `ForeignSource`.
- Boundary: the domain roster is this branch's projection of one estate vocabulary — the corpus `TELEMETRY_CONVENTION` entry carries it, so a segment this branch and a peer both admit spells one subject byte-identical or fails there rather than surviving as two unjoinable series; estate identity dimensions ride the resource projection and carry no segment, which is why `Rostered` reads `PromotedBaggage` as its carve rather than minting a second allowlist, and hook-point ids stay a package-keyed space this grammar never reaches; the OTLP exporter package enters only at service app roots — the otelExport arm binds one `AddOtlpExporter` per signal through the one `Egress` seat, which stamps the estate wire pins — HTTP+protobuf the one egress protocol, gzip the one payload encoding — while endpoint, headers, and mTLS material stay deploy-plane rows the `OTEL_EXPORTER_OTLP_*` keys carry; that egress boundary is the `OtelExport` seam; the per-signal rows and the cross-signal `UseOtlpExporter` seat are mutually exclusive by package law and the per-signal form is the branch's, because the cross-signal seat routes every knob this page pins through `internal` option types and a composition behind it can set no batch square, no temporality, no reader cadence, and no transport; EF Core emits no `ActivitySource` of its own, so the ORM-layer command span is the `AddEntityFrameworkCoreInstrumentation` row `StoreDriver` registers — nesting over the Npgsql ADO driver span, complementary never redundant — while gRPC client spans ride `AddGrpcClientInstrumentation` with `SuppressDownstreamInstrumentation` so the underlying HTTP/2 leg never double-traces, superseding the client's native `Grpc.Net.Client` source and its single `Grpc.Net.Client.GrpcOut` per-call activity, and neither surface adds an `AddSource` row; the otelExport arm carries `AddAspNetCoreInstrumentation` beside the wire host for inbound request spans; store telemetry is the PORT-peer arbitration — Persistence owns the driver and the instrumented builders while the app root alone registers, so no downward reference forms: `StoreDriver` subscribes `AddNpgsql` tracing, `AddEntityFrameworkCoreInstrumentation` ORM tracing, and `AddNpgsqlInstrumentation` metrics once at the store-composing root, with `NpgsqlDataSourceBuilder.Name` (`string?`, get/set) assigned Persistence-side per logical database so `db.client.connection.pool.name` keys stable pool dimensions, and `StoreWire<TKey, TValue>` registers `AddKafkaProducerInstrumentation`/`AddKafkaConsumerInstrumentation` on both providers once per message shape over the Persistence egress `AsInstrumentedProducerBuilder` and CDC ingress `InstrumentedConsumerBuilder` legs, closing the producer-only Kafka asymmetry; the builtin rows delete the meter-side `AddRuntimeInstrumentation` registration because the runtime publishes `System.Runtime` inbox and a meter-name row is that whole admission, while `AddProcessInstrumentation` survives as a verb on the always-on metrics arm because its absolute process series exist only on the meter its registered instrumentation instance constructs, so a `ForeignSource` row subscribes an empty scope; the trace-side `AddHttpClientInstrumentation` row keeps URL-query redaction; `LatencySpine.Mark` is the single checkpoint recorder, and the `ILatencyContext` payload is carried into `DrainConductor.Drain`, `OutboundSurface.Run`, and `SupportCapture.Capture` — those folds thread the context so each phase boundary has its recording seat, deleting per-fold `Stopwatch` timing; the recorder is cheaper than child spans and free of sampling coupling; name-to-`CheckpointToken`/`MeasureToken` issuance rides `ILatencyContextTokenIssuer.GetCheckpointToken`/`GetMeasureToken`, the frozen spans read through the `ILatencyContext.LatencyData` accessor, and `ILatencyDataExporter.ExportAsync(LatencyData, CancellationToken)` exports at the telemetry drain band; `AddLatencyContext` registers the context once and the consuming folds thread it so each phase boundary records through its issued token; test-row trace assertions ride one `BaseProcessor<Activity>` through `AddProcessor` and metric assertions ride `MetricCollector<T>` — no in-memory exporter package enters a production tier, and the proof estate's own in-memory exporter row stays its own admission; the `Profile` signal is the continuous-profiling rail — the admitted `PyroscopeSpanProcessor` registers as `AddProcessor<PyroscopeSpanProcessor>()` through the same seat the test-row processor uses, gated to service app roots where the profiler endpoint resolves — `Profiler.Instance` carries no address member, so the ingest address is the agent `PYROSCOPE_SERVER_ADDRESS` environment row bound from the deploy-plane profiles endpoint beside `PYROSCOPE_APPLICATION_NAME` and the CLR-profiler enablement rows, with `SetAuthToken`/`SetBasicAuth` the credential seam and `PYROSCOPE_TENANT_ID` the tenancy row — and stamps the `pyroscope.profile.id` tag on each root span so a flame graph scopes to the exact trace that showed a regression; the GenAI semantic conventions ride the trace and metric signals — an MCP-served tool span carries `gen_ai.operation.name` and the `gen_ai.provider.name` provider discriminant beside the `gen_ai.usage.input_tokens`/`output_tokens` counts, and the token-usage instruments ride the minted `Rasm.AppHost` meter, so the agentic surface the host serves shares one telemetry taxonomy with the runtime, never a parallel agent-metrics owner; durable egress has exactly ONE owner per exporter — the branch-typed queue installs through `OtlpExporterOptions.HttpClientFactory`, so the exporter's own `OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY=disk` pair stays unset wherever the handler leg is selected, and arming both gives one batch two independent persistence owners writing two directories with no shared accounting; a replaced factory DISPLACES the shipped one whole, and the shipped one is the sole application point for both the option timeout and the mutual-auth client the `OTEL_EXPORTER_OTLP_*` trust rows arm — `OtlpMtlsOptions` is internal at this pin — so `Egress` carries both halves and a durable profile exporting unauthenticated against a mutual-auth collector is the defect that row forecloses; durable-transport LIFETIME is the composition's, never the exporter's — the SDK hands its export client an `HttpClient` it never disposes and its shutdown only cancels pending requests, so neither the handler chain nor the provider directory reaches a release seat of its own and `TelemetryComposition.Dispose` at the telemetry drain band is the one seat closing both, which is also why the set opens at composition rather than inside an options delegate the SDK invokes past a sealed service collection; credential material never reaches disk because a stored blob carries the request BODY alone and the replay copies its headers off the live request that just succeeded, so a rotated ingest token applies to the whole tail and a stolen queue directory yields payloads and no key; queue DEPTH is the disposition ledger's own arithmetic — neither storage tier publishes a count or size accessor and its directory field is internal, so a depth level costs an O(n) directory walk per collection while the `queued`-minus-`replayed`-minus-`corrupt` gap answers the same question off counters already mounted; retention-expiry reclamation is the provider's own maintenance timer surfacing on the package `EventSource` alone, so an aged-out tail widens that same gap rather than minting a disposition row; plugin ALC capsules open no disk queue, so an unloaded capsule's failed batches die with it and never outlive the load context that minted them; baggage promotion is allowlist-only — `PromotedBaggage` names `TenantContext.TenantSlot` and `CorrelationId.Slot`, one predicate serving the span processor and the log processor alike, and an unlisted key reaches neither, so free-form baggage flood is structurally impossible; log-record identity is the resource projection and the shipped `AddApplicationLogEnricher` row is DELETED, not configured off — it writes an unqualified `service.name` and the deprecated `deployment.environment` spelling as package-owned tags, so leaving it seated puts two values of one dimension on two planes and a query joining a log record to a metric series compares neither, while `AddProcessLogEnricher` survives on its `ThreadId` column alone because the resource already carries `process.pid`; the latency breakdown and the `AddHttpClientInstrumentation` span are two projections of one hop — the checkpoint handler never mints a second trace, extended logging supersedes the built-in client logger so both active is the double-log defect, and `wrapHandlersPipeline` decides whether logging observes pre- or post-retry attempts on the resilience chain.

```csharp signature
// Branch domain roster: the `<domain>` segment of every rasm.<domain>.<measure> instrument name and every
// rasm-owned metric dimension this branch mints. A row names the capability SUBJECT a query joins on, so two
// packages serving one subject share the row while `service.name` separates their series and a package spanning
// two subjects emits under both; a host-boundary row names that host's own surface because a document census and
// a canvas solution answer different questions, never one subject split by emitter.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TelemetryDomain {
    public static readonly TelemetryDomain AppHost = new("apphost", "application-platform lifecycle, admission, and brokered capability");
    public static readonly TelemetryDomain AppUi = new("appui", "desktop shell interaction, charting, and collaboration surfaces");
    public static readonly TelemetryDomain Bim = new("bim", "building-model exchange, energy, and semantic-graph work");
    public static readonly TelemetryDomain Compute = new("compute", "solver execution, monitoring, and the numerical residual per graduation");
    public static readonly TelemetryDomain Deploy = new("deploy", "the consumption-profile axes a signal groups on");
    public static readonly TelemetryDomain Element = new("element", "element-seam projection and wire volume");
    public static readonly TelemetryDomain Fabrication = new("fabrication", "fabrication engines, delivery programs, and process objectives");
    public static readonly TelemetryDomain Fault = new("fault", "the fault-triple discriminants a signal groups on");
    public static readonly TelemetryDomain Grasshopper = new("grasshopper", "canvas solution, paint, and interaction surfaces");
    public static readonly TelemetryDomain Host = new("host", "the host-boundary discriminant a signal groups on");
    public static readonly TelemetryDomain Kernel = new("kernel", "kernel op cost, receipts, and fault counts");
    public static readonly TelemetryDomain Materials = new("materials", "material projection and impact assemblies");
    public static readonly TelemetryDomain Outbound = new("outbound", "the outbound-route discriminant a signal groups on");
    public static readonly TelemetryDomain Persistence = new("persistence", "store usage, census, and durable-work health");
    public static readonly TelemetryDomain Rhino = new("rhino", "document, display, and bench surfaces of the modeling host");
    public static readonly TelemetryDomain Slo = new("slo", "objective burn and severity axes");

    // Estate namespace carries ONE spelling: the resource triple's `service.namespace`, the `service.name`
    // qualifier, every instrument head, every rasm-owned dimension key, and the conformance gate's own carve
    // all read it, so a rename moves every surface together and none carries a stale prefix the roster admits.
    public const string Namespace = "rasm";

    public const string Prefix = Namespace + ".";

    public string Subject { get; }

    // Declaring rosters concatenate their head at compile time, so the head stays a const there and `Rostered`
    // proves it against these rows; `Measure` is the mint wherever a name assembles at runtime.
    public string Head => Prefix + Key + ".";

    public string Measure(string measure) => Head + measure;

    // ONE qualifier for a service row: the name spells the namespace once and in the roster's own casing, so a
    // composition supplying an already-qualified application id never doubles the prefix, and a PascalCase
    // assembly id never lands a `service.name` no dotted-grammar query matches its own domain segments against.
    public static string Qualify(string service) => Qualified(service.ToLowerInvariant());

    static string Qualified(string lowered) => lowered.StartsWith(Prefix, StringComparison.Ordinal) ? lowered : Prefix + lowered;

    public static Fin<TelemetryDomain> Resolve(string name) =>
        toSeq(Items).Find(row => name.StartsWith(row.Head, StringComparison.Ordinal)).Match(
            Some: static row => Fin.Succ(row),
            None: () => Fin.Fail<TelemetryDomain>(new Fault.InvalidValue(Label: name, Requirement: "a rostered rasm.<domain> segment")));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TelemetrySignal {
    public static readonly TelemetrySignal Log = new("log", buffered: true, redacted: true, exported: true, SampleRatio);
    public static readonly TelemetrySignal Trace = new("trace", buffered: false, redacted: true, exported: true, SampleRatio);
    public static readonly TelemetrySignal Metric = new("metric", buffered: false, redacted: false, exported: true, static _ => 1d);
    public static readonly TelemetrySignal Profile = new("profile", buffered: false, redacted: false, exported: false, SampleRatio);

    public bool Buffered { get; }

    public bool Redacted { get; }

    // OTLP carries three signals and the profile rail rides the Pyroscope agent's own push, so the durable
    // queue set folds over this column instead of a hand-written three; the Tier-0 profiles swap replaces an
    // agent push with an OTLP exporter by flipping ONE value, and every per-signal wire policy follows.
    public bool Exported { get; }

    [UseDelegateFromConstructor]
    public partial double Ratio(ConsumptionProfile profile);

    // Two axis facts carry the thinned floor: sampling pays only where an OTLP-export provider ships
    // spans off the process, and only an unattended long-lived topology carries the volume worth
    // thinning — an in-host or cli run samples whole on either side of the provider question.
    private static double SampleRatio(ConsumptionProfile profile) =>
        profile.OtlpExport
            ? profile.Topology.Map(inHost: 1d, sidecar: 0.1d, companion: 0.1d, service: 0.1d, edge: 0.1d, cli: 1d)
            : 1d;
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

// --- [OFFLINE_EGRESS] -----------------------------------------------------------------------
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

public sealed record OtlpOfflineFact(string Signal, OfflineDisposition Disposition, long Bytes);

public sealed record OtlpOfflinePolicy(
    Option<string> Root,
    long CapBytes,
    Duration Maintenance,
    Duration Retention,
    Duration WriteBound,
    Duration LeaseWindow,
    Duration DrainBound,
    int DrainBatch) {
    // Absent root is the DEFAULT, so every unanswered composition refuses structurally rather than by omission.
    public static readonly OtlpOfflinePolicy None =
        new(Option<string>.None, 0L, Duration.Zero, Duration.Zero, Duration.Zero, Duration.Zero, Duration.Zero, 0);

    // TWO facts arm the queue and each is necessary: durable disk to hold a tail, and a bound OTLP provider to
    // produce one. Disk alone opens a directory and a maintenance timer per exported signal on every desktop
    // launch of a locally-projecting composition and never writes a byte; a provider alone would write signal
    // bytes into whatever path the process happens to reach and report a durability it never had.
    public static OtlpOfflinePolicy For(ResolvedProfile resolved) =>
        (resolved.Profile.OtlpExport ? resolved.Roots.QueueRoot : Option<string>.None).Match(
            Some: static root => new OtlpOfflinePolicy(
                Root: Some(root),
                CapBytes: 512L * 1024 * 1024,
                Maintenance: Duration.FromMinutes(2),
                Retention: Duration.FromHours(48),
                WriteBound: Duration.FromSeconds(60),
                LeaseWindow: Duration.FromSeconds(30),
                DrainBound: Duration.FromSeconds(5),
                DrainBatch: 32),
            None: static () => None);

    public bool Armed => Root.IsSome;

    // ONE mint for the whole exported set, taken at composition: a per-signal Open called from inside an
    // exporter's options delegate runs AFTER the service provider is built, so nothing outside that closure
    // could reach the queue to drain or dispose it and an unwritable root would surface at the first dropped
    // batch instead of at admission. One directory per signal, because the drain replays a blob through the
    // live request that just proved the endpoint and a shared directory posts a metrics batch at /v1/traces.
    public FrozenDictionary<string, OtlpOfflineQueue> Open(Action<OtlpOfflineFact> emit, TimeProvider time) =>
        Root.Match(
            Some: root => toSeq(TelemetrySignal.Items)
                .Filter(static signal => signal.Exported)
                .Map(signal => KeyValuePair.Create(signal.Key, new OtlpOfflineQueue(
                    signal,
                    new FileBlobProvider(
                        Path.Join(root, signal.Key),
                        CapBytes,
                        (int)Maintenance.TotalMilliseconds,
                        (long)Retention.TotalMilliseconds,
                        (int)WriteBound.TotalMilliseconds),
                    this,
                    emit,
                    time)))
                .ToFrozenDictionary(StringComparer.Ordinal),
            None: static () => FrozenDictionary<string, OtlpOfflineQueue>.Empty);
}

public sealed class TelemetryComposition : IDisposable {
    // Exporters never dispose an export client they were handed, so a handler chain and its connection pool
    // reach no release seat of their own. Registering each minted transport here makes the drain band the ONE
    // seat that closes durable egress whole — queue directory and socket pool together.
    readonly ConcurrentBag<HttpClient> transports = [];
    int disposed;

    TelemetryComposition(ResolvedProfile resolved, CorrelationId root, Seq<TelemetryContributorPort> contributors,
        OtlpOfflinePolicy offline, Action<OtlpOfflineFact> emit, TimeProvider time,
        FrozenDictionary<string, OtlpOfflineQueue> queues, SpanBand band) =>
        (Resolved, Root, Contributors, Offline, Emit, Time, Queues, Band) =
            (resolved, root, contributors, offline, emit, time, queues, band);

    public ResolvedProfile Resolved { get; }

    public CorrelationId Root { get; }

    // Contributed rows are a composition fact BOTH governance and the fan read: the view projection resolves
    // each stream against the row that declared it while `InstrumentFan.Mount` binds the same rows against
    // their meters, so one supplied set feeds both and neither governs a roster the other never mounted.
    public Seq<TelemetryContributorPort> Contributors { get; }

    // Span custody for the whole process, folded from the SAME contributed set the meter side reads: every
    // kernel domain rides in by construction and each port's `Planes` roster enters beside them, so one band
    // answers every emitting package and a folder minting its own `ActivitySource` has no reason to exist.
    // This platform references no emitting package, so the port column is the only carriage that reaches the
    // rosters at all — a hand-listed roster here would name types this assembly cannot resolve.
    public SpanBand Band { get; }

    public OtlpOfflinePolicy Offline { get; }

    public Action<OtlpOfflineFact> Emit { get; }

    public TimeProvider Time { get; }

    public FrozenDictionary<string, OtlpOfflineQueue> Queues { get; }

    public static TelemetryComposition Of(ResolvedProfile resolved, CorrelationId root, Action<OtlpOfflineFact> emit,
        TimeProvider time, params ReadOnlySpan<TelemetryContributorPort> contributors) =>
        Opened(resolved, root, [.. contributors], OtlpOfflinePolicy.For(resolved), emit, time);

    static TelemetryComposition Opened(ResolvedProfile resolved, CorrelationId root, Seq<TelemetryContributorPort> contributors,
        OtlpOfflinePolicy offline, Action<OtlpOfflineFact> emit, TimeProvider time) =>
        new(resolved, root, contributors, offline, emit, time, offline.Open(emit, time),
            SpanBand.Of(resolved.ServiceVersion, [.. contributors.Bind(static port => port.Planes)]));

    internal HttpClient Transport(Func<HttpClient> mint) {
        HttpClient client = mint();
        transports.Add(client);
        return client;
    }

    // Release runs at the telemetry drain band AFTER provider ForceFlush and Shutdown: a queue closed while
    // an exporter still holds its handler drops the very tail the flush just handed it. Transports close
    // before stores, so no live client reaches a disposed provider with one last failed batch, and the band
    // closes between them — its sources outlive the flush that drained their spans and die before the queues
    // holding that flush, so no emitting package's bracket writes into a source the drain already released.
    public void Dispose() {
        if (Interlocked.Exchange(ref disposed, 1) != 0) return;
        ignore(toSeq(transports).Iter(static transport => transport.Dispose()));
        Band.Dispose();
        ignore(toSeq(Queues.Values).Iter(static queue => queue.Dispose()));
    }
}

public sealed class OtlpOfflineQueue(
    TelemetrySignal signal,
    FileBlobProvider store,
    OtlpOfflinePolicy policy,
    Action<OtlpOfflineFact> emit,
    TimeProvider time) : IDisposable {
    // Queued batches answer ACCEPTED because durability is the contract this handler sells: the bytes are
    // on disk and replay is owed. A refusal answers a real transport failure instead, so the exporter's own
    // drop path runs and its self-diagnostics record the loss rather than a synthetic success hiding it.
    public HttpResponseMessage Accept(HttpRequestMessage request, CancellationToken token) {
        byte[] body = Body(request, token);
        // Bodiless requests store nothing and replay nothing, so they take the transport's own failure rather
        // than a durability receipt over zero bytes and a blob the drain then pays a lease to discard.
        bool stored = body.Length > 0 && store.TryCreateBlob(body.AsSpan(), out _);
        emit(Fact(stored ? OfflineDisposition.Queued : OfflineDisposition.Refused, body.LongLength));
        return new HttpResponseMessage(stored ? HttpStatusCode.Accepted : HttpStatusCode.ServiceUnavailable);
    }

    // Drain rides the proven-good live request as its replay template, so a rotated ingest credential and a
    // moved endpoint both apply to the tail the moment the next export proves them — neither has ever been
    // written to disk. Both bounds hold the export thread: a deep queue drains across successive exports.
    // Replay order is the provider's, NEWEST blob first, so a queue held past its retention window sheds its
    // OLDEST batches — the honest durability claim is a bounded recent tail, never a complete one.
    public Unit Drain(HttpRequestMessage template, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken token) {
        DateTimeOffset deadline = time.GetUtcNow() + policy.DrainBound.ToTimeSpan();
        for (int replayed = 0; replayed < policy.DrainBatch; replayed++) {
            if (time.GetUtcNow() >= deadline) {
                emit(Fact(OfflineDisposition.DrainTimeout, 0L));
                return unit;
            }
            if (!store.TryGetBlob(out PersistentBlob? blob) || blob is null) { return unit; }
            if (!blob.TryLease((int)policy.LeaseWindow.TotalMilliseconds)) { return unit; }
            if (!blob.TryRead(out byte[]? body) || body is null) {
                // Unreadable blobs DELETE rather than release: a leased-and-failed head blob the
                // maintenance timer keeps promoting back wedges every later batch behind it forever.
                ignore(blob.TryDelete());
                emit(Fact(OfflineDisposition.Corrupt, 0L));
                continue;
            }
            using HttpRequestMessage replay = Rebuild(template, body);
            // Replay rides the SAME transient classifier the live send rides: a raise from a replay forward
            // escapes into the export call that just SUCCEEDED and fails a landed batch on the tail's behalf, so
            // a transport that dies mid-drain defers its leased body exactly as a non-success status does.
            using HttpResponseMessage? response = Attempt(replay, forward, token);
            if (response is not { IsSuccessStatusCode: true }) {
                emit(Fact(OfflineDisposition.Deferred, body.LongLength));
                return unit;
            }
            ignore(blob.TryDelete());
            emit(Fact(OfflineDisposition.Replayed, body.LongLength));
        }
        return unit;
    }

    public void Dispose() => store.Dispose();

    // ONE transient classifier for both directions of this transport: a transport fault and a non-success status
    // are one condition — the batch did not land — and the queue owns the difference between what is worth
    // storing and what the exporter should see, so the live send and the replay cannot drift on that verdict.
    // A token the caller tripped stays a raise, because a cancelled export is the caller's own decision.
    internal static HttpResponseMessage? Attempt(HttpRequestMessage request, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken token) {
        try {
            return forward(request, token);
        } catch (HttpRequestException) {
            return null;
        } catch (TaskCanceledException) when (!token.IsCancellationRequested) {
            return null;
        }
    }

    // Body re-materializes SYNCHRONOUSLY because the exporter's own http+protobuf leg is synchronous and both
    // shipped content shapes override the synchronous serializer — the raw memory content and the gzip
    // wrapper alike — so a failed send's payload replays with no async hop and no second whole-batch buffer.
    static byte[] Body(HttpRequestMessage request, CancellationToken token) {
        if (request.Content is not { } content) { return []; }
        using var buffer = new MemoryStream();
        content.CopyTo(buffer, context: null, token);
        return buffer.ToArray();
    }

    // FRAMING is the replay content's own fact and never the template's: the live request's Content-Length
    // measures the batch that just went out, and a transfer-encoding declared on it contradicts a fixed-length
    // array body — copying either truncates or over-declares the replay at the collector. The carve is a
    // DENYLIST so a header the exporter adds next release survives it, where an allowlist silently drops every
    // authentication and content coordinate it never anticipated.
    static readonly FrozenSet<string> FramingHeaders =
        FrozenSet.Create(StringComparer.OrdinalIgnoreCase, ["Content-Length", "Transfer-Encoding"]);

    // Content headers carry the exporter's own Content-Encoding, so the stored bytes replay byte-identically
    // under whatever compression the live request already applied; the exporter's content is memory-backed and
    // therefore re-readable, which is what lets Accept read a body a failed send already streamed.
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

    OtlpOfflineFact Fact(OfflineDisposition disposition, long bytes) => new(signal.Key, disposition, bytes);
}

// Transport TRUST, the second half the shipped exporter factory owned. `OtlpMtlsOptions` is internal at this
// pin and the exporter mounts its client certificate inside that factory alone, so the moment a composition
// seats `HttpClientFactory` for the durable queue every deployment configuring mutual auth through the
// `OTEL_EXPORTER_OTLP_*` rows the governance table pins loses it — silently, because an unauthenticated
// connection a collector accepts reports nothing and one it refuses reads as an endpoint outage the queue then
// buffers. This row re-reads those same three variables, so arming durability never disarms trust and the
// governance table stays the one place both are pinned.
public readonly record struct OtlpTrust(Option<string> Authority, Option<string> Certificate, Option<string> Key) {
    public const string AuthorityVariable = "OTEL_EXPORTER_OTLP_CERTIFICATE";
    public const string CertificateVariable = "OTEL_EXPORTER_OTLP_CLIENT_CERTIFICATE";
    public const string KeyVariable = "OTEL_EXPORTER_OTLP_CLIENT_KEY";

    public static OtlpTrust Read() =>
        new(Path(AuthorityVariable), Path(CertificateVariable), Path(KeyVariable));

    static Option<string> Path(string variable) =>
        Optional(Environment.GetEnvironmentVariable(variable)).Filter(static value => value.Length > 0);

    // Client identity mounts BOTH halves or neither: a certificate without its key presents nothing, a key
    // without its certificate names nothing, and a half-configured deployment therefore mounts no identity
    // rather than exporting unauthenticated under a configuration that reads as armed. A CA row NARROWS the
    // trust anchor to a custom store — that private collector chain a public root list does not carry — and
    // never widens it, so no validation callback returning true unconditionally has a spelling here and
    // revocation stays ONLINE with the custom root excluded: `NoCheck` beside a narrowed anchor waives every
    // revocation that anchor's own chain publishes, which is the one widening spelled as a hardening, and a
    // collector chain publishing no distribution point then fails the build closed rather than silently
    // exempting every certificate the custom store admits.
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

public sealed class PersistentOtlpHandler(OtlpOfflineQueue queue) : DelegatingHandler {
    // BOTH overrides are load-bearing: the exporter's http+protobuf leg reaches HttpClient.Send, which
    // dispatches to HttpMessageHandler.Send and NEVER to SendAsync, so a queue installed on the async
    // override alone is dead on every desktop and server host and the durable charter silently buys nothing;
    // async stays for the http/2 and browser transports and blocks exactly where the exporter's own client
    // already blocks. One core carries both, so a queue decision has one body.
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken token) =>
        Routed(request, base.Send, token);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) =>
        Task.FromResult(Routed(request, Awaited, token));

    // Platform-forced handler boundary — the out-parameter blob protocol and the message-handler seam are the
    // page's named statement carve-out, and every branch here resolves to one queue call.
    HttpResponseMessage Routed(HttpRequestMessage request, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> forward, CancellationToken token) {
        HttpResponseMessage? live = OtlpOfflineQueue.Attempt(request, forward, token);
        if (live is { IsSuccessStatusCode: true }) {
            ignore(queue.Drain(request, forward, token));
            return live;
        }
        live?.Dispose();
        return queue.Accept(request, token);
    }

    HttpResponseMessage Awaited(HttpRequestMessage message, CancellationToken token) =>
        base.SendAsync(message, token).GetAwaiter().GetResult();
}

// --- [COMPOSITION] --------------------------------------------------------------------------
public static class ResourceIdentity {
    // Containerization is the ship vehicle's own fact: an OCI image runs inside the container whose id
    // AddContainerDetector resolves, and every other vehicle resolves none.
    static readonly Func<ConsumptionProfile, bool> Containerized = static profile => profile.Vehicle == ShipVehicle.Oci;

    // ONE Action<ResourceBuilder> for every provider owner: the resolved-record detector mints the triple
    // first, then the contrib rows ENRICH the same builder — never a SetResourceBuilder replacement. Extra
    // facts ride the detector as data, so no root hand-builds an identity delegate and no second producer
    // of that delegate exists to drift from this one.
    public static Action<ResourceBuilder> Compose(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra) =>
        Composed(resolved, [.. extra]);

    // Merge order IS precedence — Build folds detectors in registration order and the LAST contributor wins a
    // shared key — so the deployment override TAILS the chain: the default builder seats its environment
    // detector ahead of every ConfigureResource row, which would let the minted triple outrank an
    // OTEL_RESOURCE_ATTRIBUTES value the deploy plane set, inverting the Tier-0 rule. Re-chaining it last
    // costs one re-read of the same variable and restores deployment-wins.
    static Action<ResourceBuilder> Composed(ResolvedProfile resolved, ImmutableArray<KeyValuePair<string, object>> extra) =>
        resource => ignore((Containerized(resolved.Profile)
            ? Detected(resource, resolved, extra).AddContainerDetector()
            : Detected(resource, resolved, extra))
            .AddEnvironmentVariableDetector());

    static ResourceBuilder Detected(ResourceBuilder resource, ResolvedProfile resolved, ImmutableArray<KeyValuePair<string, object>> extra) =>
        resource
            .AddDetector(new ProfileIdentity.HostResourceDetector(resolved, extra))
            .AddHostDetector()
            .AddOperatingSystemDetector()
            .AddProcessDetector()
            .AddProcessRuntimeDetector();
}

public static class SignalGovernance {
    public static readonly Predicate<string> PromotedBaggage =
        static key => key is TenantContext.TenantSlot or CorrelationId.Slot;

    // Conformance gate: every contributed port mounts only once its rasm-owned instrument names and rasm-owned
    // dimension keys each resolve a rostered segment, so an unjoinable series refuses at admission carrying the
    // offending name. It reads the port's WHOLE declared surface — a contributor minting on a load-context
    // meter of its own publishes rows this root binds no handle for, and gating the mounted half alone leaves
    // every per-ALC host boundary exporting names no roster ever proved. Two carves are structural — promoted
    // keys ride the causal frame and estate identity dimensions ride the resource projection, and a
    // semconv-owned name (`gen_ai.*`, `http.*`) belongs to the convention that mints it, so gating an
    // unprefixed name would refuse the ports that spell it correctly.
    public static Fin<TelemetryContributorPort> Rostered(TelemetryContributorPort port) =>
        port.Declared.Fold(Fin.Succ(port), static (rail, spec) =>
            spec.Name.Cons(spec.Dimensions)
                .Filter(static name => name.StartsWith(TelemetryDomain.Prefix, StringComparison.Ordinal) && !PromotedBaggage(name))
                .Fold(rail, static (held, name) => held.Bind(carried => TelemetryDomain.Resolve(name).Map(_ => carried))));

    // Series budget is ONE number for every stream because the tenant dimension dominates every row's product:
    // a per-row literal sized off the declared dimensions alone prices a nine-value classification key at
    // thirty-two and drops every tenant past the third.
    public const int SeriesCap = 256;

    // ONE view, resolved per instrument. The SDK mints a stream per MATCHING view, so a named row beside a
    // trailing wildcard exports the same instrument twice — once projected, once whole — and the projection
    // then guarantees nothing; the predicate form is the only shape carrying per-instrument resolution.
    // Each rostered stream derives from the row that DECLARED the instrument, so a new dimension is one
    // `Dimensions` edit at its roster and no governance row moves. The index reads the whole declared surface,
    // because a per-ALC contributor's self-minted streams reach this predicate on the FOREIGN arm and would
    // otherwise keep an unprojected tag set the classification seam never bounded.
    public static Func<Instrument, MetricStreamConfiguration?> Views(Seq<TelemetryContributorPort> contributors) =>
        Projected(contributors.Bind(static port => port.Declared)
            .ToFrozenDictionary(static spec => spec.Name, static spec => spec, StringComparer.Ordinal));

    static Func<Instrument, MetricStreamConfiguration?> Projected(FrozenDictionary<string, InstrumentSpec> rostered) =>
        instrument => rostered.TryGetValue(instrument.Name, out InstrumentSpec? spec)
            // Rostered streams carry their declared dimensions beside the ONE estate tenancy key and nothing
            // else, so a contributor evidence string — asset key, document path, media source — reaches no
            // exporter and the `[06]` classification seam holds on the metric plane by construction. Tenancy
            // appends rather than declaring per row because `InstrumentSet.Tags` stamps it on every write.
            ? Shaped(spec.Kind == InstrumentKind.Distribution && spec.Bounds.IsNone, [.. spec.Dimensions, TenantContext.TenantSlot])
            // Foreign instrumentation carries semconv dimensions this roster never declares, so its streams keep
            // their own convention-owned vocabulary under the same budget — projecting them would erase the
            // route, method, and pool-name keys every server and store panel breaks on.
            : Shaped(Distributed(instrument), null);

    // Base2 exponential is the estate histogram wire default and a view row is the ONLY seat this SDK exposes for
    // it: no provider-wide aggregation default is published and neither assembly at this pin carries the
    // `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION` key other SDK trains read, so a pin spelled as
    // that variable governs nothing and every distribution exports the shipped explicit ladder instead. A rostered
    // row whose mint declared `Bounds` keeps the explicit form, because those boundaries are finished at the
    // instrument's own advice and a view recomputing them is the deleted form; every other distribution — rostered
    // without advice, or foreign — takes the exponential configuration under the same series budget, so one
    // configuration mint answers both arms and neither can seat a budget the other misses.
    static MetricStreamConfiguration Shaped(bool exponential, string[]? tags) =>
        exponential
            ? new Base2ExponentialBucketHistogramConfiguration { TagKeys = tags, CardinalityLimit = SeriesCap }
            : new MetricStreamConfiguration { TagKeys = tags, CardinalityLimit = SeriesCap };

    // Generic DEFINITION is the whole test: `Histogram<T>` closes over every numeric argument the runtime
    // admits and foreign instrumentation picks any of them, so a roster of closed forms drops a stream to the
    // explicit ladder the moment a package mints one this branch never enumerated.
    static bool Distributed(Instrument instrument) =>
        instrument.GetType() is { IsGenericType: true } shape && shape.GetGenericTypeDefinition() == typeof(Histogram<>);

    // Every push-signal batch square is a declared policy value, never a package default: the queue depth an
    // export burst survives and the delay a drop appears after are the two numbers an incident reads first.
    public static readonly BatchExportProcessorOptions<Activity> SpanBatch = new() {
        MaxQueueSize = 4096, MaxExportBatchSize = 512, ScheduledDelayMilliseconds = 5_000, ExporterTimeoutMilliseconds = 30_000,
    };

    public static readonly BatchExportLogRecordProcessorOptions LogBatch = new() {
        MaxQueueSize = 8192, MaxExportBatchSize = 1024, ScheduledDelayMilliseconds = 2_000, ExporterTimeoutMilliseconds = 30_000,
    };

    public static readonly PeriodicExportingMetricReaderOptions ReaderCadence = new() {
        ExportIntervalMilliseconds = 60_000, ExportTimeoutMilliseconds = 30_000,
    };

    public static OpenTelemetryBuilder Govern(IServiceCollection services, TelemetryComposition composition) =>
        LogPipeline.Owner(composition.Resolved.Profile).Switch(
            state: Propagated(services).AddOpenTelemetry()
                .ConfigureResource(ResourceIdentity.Compose(composition.Resolved))
                .WithTracing(tracing => tracing
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(TelemetrySignal.Trace.Ratio(composition.Resolved.Profile))))
                    // Two admissions, two grammars: `ForeignSource` carries the PascalCase package identities a
                    // foreign library and a contributed meter publish under, while the band's own frozen scope
                    // names spell the dotted `rasm.<package>.<plane>` roster every kernel and contributed
                    // bracket opens against. Neither derives from the other, so registering one alone strands
                    // every span of the other: an unlistened source takes the same null-span arm an untraced
                    // composition legitimately takes, and no bracket reports the loss.
                    .AddSource([.. ForeignSource.Admitting(TelemetrySignal.Trace)])
                    .AddSource([.. composition.Band.Names])
                    .AddHttpClientInstrumentation(static http => {
                        http.FilterHttpRequestMessage = static request => request.RequestUri is { IsLoopback: false };
                        // RecordException stays OFF: AddException is the one exception path and both active
                        // writes two exception events per failed hop under two attribute grammars.
                        http.EnrichWithException = static (activity, exception) =>
                            ignore(activity.AddException(exception));
                    })
                    .AddGrpcClientInstrumentation(static grpc => grpc.SuppressDownstreamInstrumentation = true)
                    .AddBaggageActivityProcessor(PromotedBaggage))
                .WithMetrics(metrics => metrics
                    .AddMeter([.. ForeignSource.Admitting(TelemetrySignal.Metric)])
                    // Process series mount on the verb, never on a meter-name row: the package's instruments
                    // exist only on the meter its registered instrumentation instance constructs, so a
                    // ForeignSource row subscribes an empty scope and double-admits the name besides.
                    .AddProcessInstrumentation()
                    .AddView(Views(composition.Contributors))),
            serilogProjection: static builder => builder,
            otelExport: builder =>
                (builder
                    .WithTracing(tracing => tracing
                        .AddAspNetCoreInstrumentation()
                        .AddProcessor<PyroscopeSpanProcessor>()
                        .AddOtlpExporter(otlp => {
                            otlp.BatchExportProcessorOptions = SpanBatch;
                            ignore(Egress(composition, TelemetrySignal.Trace, otlp));
                        }))
                    // Metric-side AspNetCore mounts the server RED family (`http.server.request.duration`,
                    // Kestrel connection and queue streams); the trace-side row alone leaves an inbound request
                    // with spans and no latency histogram, which is the half every board tile reads.
                    .WithMetrics(metrics => metrics
                        .AddAspNetCoreInstrumentation()
                        .SetExemplarFilter(ExemplarFilterType.TraceBased)
                        .AddOtlpExporter((otlp, reader) => {
                            reader.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                            reader.PeriodicExportingMetricReaderOptions = ReaderCadence;
                            ignore(Egress(composition, TelemetrySignal.Metric, otlp));
                        }))
                    .WithLogging(
                        logs => logs
                            .AddBaggageProcessor(PromotedBaggage)
                            .AddOtlpExporter((otlp, processor) => {
                                processor.ExportProcessorType = ExportProcessorType.Batch;
                                processor.BatchExportProcessorOptions = LogBatch;
                                ignore(Egress(composition, TelemetrySignal.Log, otlp));
                            }),
                        static options => {
                            options.IncludeScopes = true;
                            options.IncludeFormattedMessage = true;
                            options.ParseStateValues = true;
                            ignore(options.AttachLogsToActivityEvent());
                        }), builder).Item2);

    // SDK static construction already installs an equivalent composite, so registration exists to make the
    // injected composite and the global one the SAME value — an equivalent-but-distinct pair drifts the moment
    // a leg is added here, and a foreign library reaching Propagators.DefaultTextMapPropagator gets that drift.
    // Registration returns void, so it crosses this expression through the same `fun` lift every void SDK call
    // on this page takes; a bare void call in a tuple slot does not compile.
    static IServiceCollection Propagated(IServiceCollection services) =>
        (fun(() => Sdk.SetDefaultTextMapPropagator(Correlation.Spine))(), services).Item2;

    // Estate wire pins, stamped as VALUES on every arm. The exporter's own env parse runs at options
    // CONSTRUCTION and the deploy plane publishes ONE row — `OTEL_EXPORTER_OTLP_ENDPOINT` at
    // `typescript:iac/kube/workload#_KEYS` — so a protocol or compression pin left to `_PROTOCOL` and
    // `_COMPRESSION` binds against keys nothing in this estate writes and falls to the SHIPPED defaults:
    // gRPC at 4317 and no compression, against a collector door the deploy plane hands in for http+protobuf
    // and against the conformance compression row. Endpoint stays deploy-plane data; the wire shape is
    // estate law, so it homes here and a deployment needing another wire composes its own root.
    public const OtlpExportProtocol WireProtocol = OtlpExportProtocol.HttpProtobuf;
    public const OtlpExportCompression WireCompression = OtlpExportCompression.GZip;

    // ONE egress binding per exporter arm: the wire pins land first, then durable transport where the resolved
    // profile armed a queue. Armed policy replaces the transport on that signal alone; an unarmed one leaves the
    // exporter's own factory, which is what binds its timeout AND its mutual-auth client, so a composition
    // without durable disk loses nothing and one WITH durable disk must carry both halves the shipped factory
    // owned. This body runs inside the exporter's options delegate, which the SDK invokes from its processor
    // factory AFTER the provider is built — the composition's queue set is therefore READ here and never minted,
    // because a service registration or a directory create at this point is past the sealed collection, and the
    // same lateness is what lets the wire stamps outrank whatever the construction-time env parse resolved.
    static Unit Egress(TelemetryComposition composition, TelemetrySignal signal, OtlpExporterOptions otlp) =>
        (fun(() => (otlp.Protocol, otlp.Compression) = (WireProtocol, WireCompression))(),
            Optional(composition.Queues.GetValueOrDefault(signal.Key)).Match(
                Some: queue => {
                    otlp.HttpClientFactory = () => composition.Transport(() =>
                        new HttpClient(new PersistentOtlpHandler(queue) { InnerHandler = OtlpTrust.Read().Mount(new SocketsHttpHandler()) }) {
                            Timeout = TimeSpan.FromMilliseconds(otlp.TimeoutMilliseconds),
                        });
                    return unit;
                },
                None: static () => unit)).Item2;

    public static OpenTelemetryBuilder StoreDriver(OpenTelemetryBuilder governed) =>
        governed
            .WithTracing(static tracing => tracing
                .AddNpgsql()
                .AddEntityFrameworkCoreInstrumentation())
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

    public static ILoggingBuilder GovernLogs(ILoggingBuilder logging, TelemetryComposition composition) =>
        logging
            .AddTraceBasedSampler()
            // Level IS the sampler's filter row: rule selection matches at and BELOW the level it names, so the
            // chatty floor thins and the error ceiling passes whole. Omitting it builds a rule matching every
            // level, which ships one error in ten off an unattended host.
            .AddRandomProbabilisticSampler(TelemetrySignal.Log.Ratio(composition.Resolved.Profile), LogLevel.Warning)
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

    // Identity is a composition fact, so the static enricher constructs HERE off the ONE resource projection;
    // a caller-supplied enricher instead forces every root to hand-spell the triple a second time and drift it
    // from the resource on the next edit. The shipped application enricher is DELETED rather than configured
    // off: with every switch cleared it contributes nothing, and left on it writes an unqualified service.name
    // and the deprecated deployment.environment key beside the resource's own — one dimension, two values.
    public static IServiceCollection EnrichContext(IServiceCollection services, TelemetryComposition composition) =>
        LatencySpine.Register(LogPipeline.Owner(composition.Resolved.Profile).Switch(
            state: services
                .AddLogEnricher<CausalEnricher>()
                .AddStaticLogEnricher(new RootEnricher(ProfileIdentity.ResourceAttributes(composition.Resolved)))
                // Process id already rides the resource projection; THREAD id is the per-record dimension no
                // resource can carry, which is the whole reason this row survives beside the identity seat.
                .AddProcessLogEnricher(static process => {
                    process.ProcessId = false;
                    process.ThreadId = true;
                })
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

| [INDEX] | [POLICY]                       | [VALUE]                                   | [OWNER]                                                     |
| :-----: | :----------------------------- | :---------------------------------------- | :---------------------------------------------------------- |
|  [01]   | attended trace ratio           | 1.0                                       | `SampleRatio` in-host and cli arms, parent-based sampler    |
|  [02]   | unattended export trace ratio  | 0.1                                       | `SampleRatio` sidecar/companion/service/edge arms           |
|  [03]   | log sampling floor             | trace-coupled                             | `AddTraceBasedSampler`                                      |
|  [04]   | service-root log chatty floor  | `Log` `Ratio` at Warning and below        | `AddRandomProbabilisticSampler(double, LogLevel?)`          |
|  [05]   | buffered-event selection       | Warning and below                         | `LogBufferingFilterRule` row                                |
|  [06]   | metric exemplar policy         | trace-based                               | `SetExemplarFilter` at service app roots                    |
|  [07]   | metric reader cadence          | 60 s export / 30 s timeout                | `ReaderCadence` on the metric `AddOtlpExporter` leg         |
|  [08]   | global buffer admission        | Warning and below                         | `AddGlobalBuffer`                                           |
|  [09]   | buffer flush window            | support-window deadline row               | `GlobalLogBuffer.Flush` on the fault transition             |
|  [10]   | destructuring caps             | 4 deep / 1024 chars / 64 items            | `SerilogProjectionPolicy.Shape` destructuring caps          |
|  [11]   | desktop level floor            | Information                               | `LoggingLevelSwitch`/`MinimumLevel.ControlledBy`            |
|  [12]   | spine event-id band            | 1000-1099                                 | `LoggerMessage` `EventId` values                            |
|  [13]   | latency checkpoint vocabulary  | drain, hop, capture                       | `LatencyCheckpoint`/`RegisterCheckpointNames`               |
|  [14]   | latency span export            | drain-band flush                          | `ILatencyDataExporter` on `LatencySpine.Seal`               |
|  [15]   | serilog wire-sink batch square | 500/2 s/10 000                            | `BatchingOptions` on `FallbackChain` sink                   |
|  [16]   | otlp span batch square         | 4096 queue / 512 batch / 5 s / 30 s       | `BatchExportProcessorOptions<Activity>` on `SpanBatch`      |
|  [17]   | otlp log-record batch square   | 8192 queue / 1024 batch / 2 s / 30 s      | `BatchExportLogRecordProcessorOptions` on `LogBatch`        |
|  [18]   | http route-parameter redaction | erase                                     | `HttpRouteParameterRedactionMode`/`RequestMetadata`         |
|  [19]   | otel processor admission       | test-row `BaseProcessor<Activity>`        | `AddProcessor` over `CompositeProcessor<Activity>`          |
|  [20]   | meter series budget            | 256 per stream                            | `SignalGovernance.SeriesCap` on every view row              |
|  [21]   | profile signal admission       | service-root profiler endpoint            | `AddProcessor<PyroscopeSpanProcessor>()`                    |
|  [22]   | GenAI span/metric conventions  | `gen_ai.*` attributes, token usage        | MCP spans on the `Rasm.AppHost` meter                       |
|  [23]   | exception log enrichment       | stack + message, file info off            | `LoggerEnrichmentOptions` on `EnableEnrichment`             |
|  [24]   | incident buffer caps           | 64 MiB / 128 KiB / 30 s                   | `GlobalLogBufferingOptions` on `AddGlobalBuffer`            |
|  [25]   | latency console export         | serilog-projection rows                   | `AddConsoleLatencyDataExporter`                             |
|  [26]   | otlp egress protocol           | `http/protobuf`                           | `SignalGovernance.WireProtocol` on the `Egress` seat        |
|  [27]   | otlp metric temporality        | delta                                     | `MetricReaderTemporalityPreference.Delta`                   |
|  [28]   | otlp histogram aggregation     | base2 exponential; advice re-arms         | `Base2ExponentialBucketHistogramConfiguration` on `Views`   |
|  [29]   | otlp payload compression       | gzip                                      | `SignalGovernance.WireCompression` on the `Egress` seat     |
|  [30]   | otlp mTLS material             | CA, client cert, client key paths         | `OTEL_EXPORTER_OTLP_CERTIFICATE`/`_CLIENT_*` rows           |
|  [31]   | otlp durable egress queue      | 512 MiB / 48 h / 2 min / 60 s write       | `OtlpOfflinePolicy.For` over `ProfileRoots.QueueRoot`       |
|  [32]   | otlp queue arming              | durable root beside a bound provider      | `OtlpExport` gate on `OtlpOfflinePolicy.For`                |
|  [33]   | otlp queue drain bound         | 32 blobs / 5 s drain / 30 s lease         | `OtlpOfflineQueue.Drain` per successful export              |
|  [34]   | otlp queue disposition roster  | six outcomes the queue can observe        | `OfflineDisposition` on `OtlpOfflineFact`                   |
|  [35]   | kafka wire spans and metrics   | producer + consumer per shape             | `SignalGovernance.StoreWire<TKey,TValue>`                   |
|  [36]   | fan-in causal links            | drain brackets link every relayed row     | kernel `TraceCarrier.Link` on `Wire/outbox#DISPATCH_SWEEP`  |
|  [37]   | resource identity triple       | `rasm`/`rasm.<svc>`/instance id           | `ProfileIdentity.ResourceAttributes`                        |
|  [38]   | semconv schema coordinate      | `https://opentelemetry.io/schemas/1.43.0` | kernel `TelemetryIdentity.SchemaUrl`                        |
|  [39]   | meter schema pin               | semconv coordinate per contributor        | `MeterOptions.TelemetrySchemaUrl` at every mint             |
|  [40]   | npgsql spans and pool metrics  | store-composing service roots             | `SignalGovernance.StoreDriver`                              |
|  [41]   | orm command spans              | store-composing service roots             | `AddEntityFrameworkCoreInstrumentation` on `StoreDriver`    |
|  [42]   | aspnet server request metrics  | duration histogram + kestrel streams      | `AddAspNetCoreInstrumentation` on the meter builder         |
|  [43]   | hop exception recording        | runtime exception event, one grammar      | `Activity.AddException` on `EnrichWithException`            |
|  [44]   | profiler agent ingest address  | deploy-plane profiles endpoint            | `PYROSCOPE_SERVER_ADDRESS` row at service app roots         |
|  [45]   | resource detector rows         | host/os/process/runtime; container        | `ResourceIdentity.Compose` detector chain                   |
|  [46]   | container detector gate        | `Vehicle == ShipVehicle.Oci`              | `Containerized` column on `ConsumptionProfile.Vehicle`      |
|  [47]   | global propagator registration | one `Correlation.Spine` composite         | `Sdk.SetDefaultTextMapPropagator`                           |
|  [48]   | baggage promotion allowlist    | `rasm.tenant` + `CorrelationId`           | `AddBaggageActivityProcessor`/`AddBaggageProcessor`         |
|  [49]   | otlp log record capture        | scopes, formatted body, parsed state      | `OpenTelemetryLoggerOptions` on `WithLogging`               |
|  [50]   | log-to-span attachment         | records land as span events               | `AttachLogsToActivityEvent`                                 |
|  [51]   | log severity mapping           | `(int)LogLevel * 4 + 1`                   | SDK `LogRecord.LogLevel` to `LogRecordSeverity`             |
|  [52]   | outbound latency breakdown     | detailed checkpoints, package on          | `AddHttpClientLatencyTelemetry`                             |
|  [53]   | outbound client-log redaction  | four `*DataClasses` taxonomy maps         | `AddExtendedHttpClientLogging` over `Marker`                |
|  [54]   | otlp exported signal set       | trace, metric, log                        | `TelemetrySignal.Exported` column                           |
|  [55]   | otlp durable transport leg     | synchronous and async handler legs        | `PersistentOtlpHandler.Send`/`SendAsync`                    |
|  [56]   | resource merge precedence      | deployment override wins the fold         | `AddEnvironmentVariableDetector` chain tail                 |
|  [57]   | metric view derivation         | declared dimensions + tenancy key         | `SignalGovernance.Views` one `AddView` predicate            |
|  [58]   | log identity dimensions        | the resource projection, verbatim         | `RootEnricher` over `ProfileIdentity.ResourceAttributes`    |
|  [59]   | log per-record process tags    | thread id; process id off                 | `AddProcessLogEnricher` beside the resource `process.pid`   |
|  [60]   | ingress tenancy adoption       | per-carrier trust, no default             | `TenantAdoption` on `TraceContext.Continue`                 |
|  [61]   | durable transport release      | client set closed before its queues       | `TelemetryComposition.Dispose` over `Transport`             |
|  [62]   | otlp replay framing carve      | length and transfer-encoding re-derived   | `OtlpOfflineQueue.FramingHeaders` denylist on `Rebuild`     |
|  [63]   | durable queue residence        | deploy volume over local disk, host-keyed | `ProfileIdentity.QueueRootVariable` ahead of the base root  |
|  [64]   | service name qualification     | one dotted lowercase `rasm.<svc>` render  | `TelemetryDomain.Qualify` on the resource projection        |
|  [65]   | process-level metrics          | memory, virtual, cpu, threads, uptime     | `AddProcessInstrumentation` on the always-on metrics arm    |
|  [66]   | span band admission            | kernel domains + contributed planes       | `SpanBand.Of` over `TelemetryContributorPort.Planes`        |
|  [67]   | trace scope registration       | band names beside the foreign roster      | `AddSource` over `TelemetryComposition.Band.Names`          |
|  [68]   | otlp transport trust custody   | replaced factory re-mounts row [30]       | `OtlpTrust.Mount` on the durable inner handler              |
|  [69]   | contributor naming gate reach  | mounted rows beside self-minted rows      | `Rostered`/`Views` over `TelemetryContributorPort.Declared` |

## [06]-[REDACTION_TAXONOMY]

- Owner: `DataClassification` `[SmartEnum<string>]` taxonomy with the `RedactorKind` keyless vocabulary as its redactor column; `RedactionRegistration` the binding fold.
- Cases: classification rows in escalating sensitivity order — `Internal` the non-PII internal-data tier, `Confidential` the protected business tier the durable-store retention and blob-catalog lanes classify against; one redactor kind per disclosure treatment — none, hmac, erase.
- Entry: `RedactionRegistration.Bind(ILoggingBuilder logging, IConfigurationSection hmacKeys)` returning the redaction-enabled builder; the `AddRedaction` fold maps each `RedactorKind` to its classification set and `EnableRedaction` seals the seam.
- Auto: classification flows through `[LogProperties]` and `[TagProvider]` generated methods as `LoggerMessageState.ClassifiedTag`; `EnableRedaction` applies the bound redactor before any sink or exporter observes the tag, and the `rasm.apphost.redaction.tags` count rises per redacted tag.
- Packages: Microsoft.Extensions.Compliance.Redaction, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions.
- Growth: one classification row with one redactor binding; one redactor kind is one case; zero new surface.
- Boundary: an unredacted classified value reaching any exporter is a seam violation; classification attributes annotate shapes at definition time as `DataClassificationAttribute` subclasses through the transitively arriving compliance-abstractions surface; redactor binding rides `AddRedaction(Action<IRedactionBuilder>)` with one `SetHmacRedactor(IConfigurationSection, params DataClassificationSet[])` row, one `SetRedactor<ErasingRedactor>(params DataClassificationSet[])` row, and `SetFallbackRedactor<ErasingRedactor>()` as the fail-closed default for unmapped classifications, and the fold registers with no suppression; hmac rows pseudonymize while preserving cross-event correlation, erase rows destroy the value, and credential and secret material never persists in any signal; the log seam governs the log path while the HTTP route-parameter path is a prevention row at the instrumentation root — `RequestMetadata` declares route-template parameters and `HttpRouteParameterRedactionMode` erases them so an outgoing-request span never carries an unredacted route segment, crossing to Persistence as VALUE fields on the landed rows (`Element/codec` `SnapshotCatalogRow.Classification`, `Element/identity`) — never a guard symbol and never a second registration; one redaction policy serves logs, traces, support capture, and the route-parameter prevention row, deleting call-site string scrubbing; metric tags ride the `[05]` view seam instead — the one `AddView` predicate projects each rostered stream onto its declaring row's `Dimensions` beside the tenancy key alone, so a contributor evidence-string tag (asset key, document key, media source path) reaches an exporter only by being DECLARED on the row that mints it, where this taxonomy grades it; a foreign instrumentation stream keeps its convention-owned tags because no row of this branch declares them and none carries branch evidence.

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

(none)
