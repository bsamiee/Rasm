# [APPHOST_DIAGNOSTICS_AND_TELEMETRY]

Telemetry identity, correlation, log projection, signal governance, latency, and data classification form one diagnostics concern owned by Rasm.AppHost. The platform stacks the OTel SDK, correlation, tenancy, and host evidence onto contributed surfaces while siblings emit through minted identities.

## [01]-[INDEX]

- [02]-[TELEMETRY_IDENTITY]: Foreign-source admission vocabulary and the app-identity lacing over the kernel mint.
- [03]-[CORRELATION_SPINE]: One boot-minted root id on the suite ambient slot, the OTel tenancy mirror, and the adopted W3C trace-context every hop continues.
- [04]-[LOG_PROJECTION]: Generated lib-level delegates, the per-entry volume verdict, and provider-keyed pipeline-owner arbitration.
- [05]-[SIGNAL_GOVERNANCE]: Branch domain roster, per-signal capability set, the composition capsule, and the registration folds every provider owner binds.
- [06]-[LATENCY_PLANE]: This root's in-flight phase vocabulary, every contributor's roster, and the one boot-strict name registration.
- [07]-[REDACTION_TAXONOMY]: Classification rows binding redactor policy at every exporter boundary, each branch-contributed federation value proved against the roster at boot.

## [02]-[TELEMETRY_IDENTITY]

- Owner: `ForeignSource` `[SmartEnum<string>]` under the `ComparerAccessors.StringOrdinal` accessor — the foreign instrumentation scopes this platform admits beside the kernel `TelemetrySource` roster, each row carrying the signal set it publishes.
- Cases: each row is a foreign scope carrying the signal set it publishes — BCL runtime and HTTP scopes beside the resilience meter — and every minted Rasm scope is a kernel `TelemetrySource` row this platform admits whole, so the vocabulary here holds only what a foreign library publishes on its own. Instrument CONSTRUCTION discriminates: a library minting from its own static at type init earns a row here, while a package-mounted instrumentation whose instruments its registration verb alone constructs enters at `[05]-[SIGNAL_GOVERNANCE]` through that verb, never as a row here.
- Entry: `ForeignSource.Admitting(TelemetrySignal signal)` is the one admission projection — the kernel roster whole and every foreign row publishing that signal — feeding `AddSource` and `AddMeter` at `[05]-[SIGNAL_GOVERNANCE]`; mint mechanics are the kernel signal capsule's `TelemetryIdentity.Mint(factory, scope, version, tags)`, and this platform's composition laces app identity at every mint call: the port's `Scope` row names the meter, the kernel `TelemetryIdentity.SchemaUrl` const stamps `MeterOptions.TelemetrySchemaUrl`, and the boot `CorrelationId` rides as the one meter tag.
- Auto: BCL rows feed GC, threadpool, exception-rate, and HttpClient duration streams through `AddMeter` with zero package, the resilience row feeds the strategy-event, attempt-duration, and pipeline-duration streams every `ConfigureTelemetry` pipeline already writes, and a metric-only row never opens an empty `ActivitySource` because the projection filters on the published signal; instrument identity de-duplicates by name, so name, unit, and description are `InstrumentSpec` declaration facts and a drifted unit forks the stream at its one registry row, never at a call site; the AppHost spine and domain instruments live on the ONE `AppHostMeasure` roster at Observability/instruments#INSTRUMENT_CATALOG.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, Microsoft.Extensions.Telemetry.Abstractions, BCL inbox.
- Growth: one foreign instrumentation scope is one `ForeignSource` row with its signal set; a newly minted Rasm package is a kernel `TelemetrySource` row every admission here inherits with no edit; zero new surface.
- Boundary: a process-static `Meter` field outliving its provider is the named defect — minted pairs are `IMeterFactory`-owned and unload with the host ALC; the host builder registers the metrics services on every path including the empty builder, so `IMeterFactory` arrives with zero registration row; every instrument enters through its `InstrumentSpec` declaration on the contributor row set, so the minted pair is the registration payload `TelemetryContributorPort` carries inward, deleting handler-local `ActivitySource` and `Meter` owners; package self-identity is the kernel capsule's, so re-listing a Rasm scope here forks the vocabulary an emitter beyond this platform's reach already spells through the string-typed port.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ForeignSource {
    public static readonly ForeignSource SystemRuntime = new("System.Runtime", FrozenSet.Create(TelemetrySignal.Metric));
    public static readonly ForeignSource SystemNetHttp = new("System.Net.Http", FrozenSet.Create(TelemetrySignal.Metric, TelemetrySignal.Trace));
    public static readonly ForeignSource Polly = new("Polly", FrozenSet.Create(TelemetrySignal.Metric));

    public FrozenSet<TelemetrySignal> Signals { get; }

    public static Seq<string> Admitting(TelemetrySignal signal) =>
        toSeq(TelemetrySource.Items).Map(static row => row.Key)
            + toSeq(Items).Filter(row => row.Signals.Contains(signal)).Map(static row => row.Key);
}
```

## [03]-[CORRELATION_SPINE]

- Owner: `Correlation` the ONE causal-frame surface — the boot-minted `CorrelationId` stamp, the composition-supplied `OtelBaggage` `TenantMirror` row completing the kernel's ambient-store set, the `AmbientSlot<CorrelationFrame>` carrier every deferred hand-off enters, and the capture/restore pair over all three halves; `TenantAdoption` the ingress trust axis every continuation names; `RootEnricher` `IStaticLogEnricher` stamps the resource-identity projection once per provider; `CausalEnricher` `ILogEnricher` stamps the request-scoped correlation per record; `TraceContext` the W3C distributed-trace propagation fold injecting and extracting `traceparent`/`tracestate` over every registered transport carrier so a remote span continues the parent trace.
- Cases: three ambient stores partition by owner and this platform supplies the third — the kernel `AsyncLocal` tenancy slot and the BCL `Activity` chain are the kernel's own rows, and `OtelBaggage` seats the SDK `Baggage.Current` store an OTel-free S0 assembly cannot name; two enrichment seats split by cost class — `RootEnricher` for the per-provider resource identity, `CausalEnricher` for the per-record ambient correlation key; two `TenantAdoption` rows close the ingress trust axis — a trusted intra-deployment carrier adopts its wire tenancy and a foreign one refuses it; two propagation directions and the inbound continued-span start on `TraceContext` — the generic `Inject`/`Extract`/`Continue` members take any carrier with a getter/setter delegate pair, and `Continue` extracts, resolves tenancy through its carrier's adoption row, seeds `Baggage.Current`, and starts the inbound `Activity` from the extracted context; the gRPC `Metadata` triple and both MQTT v5 legs are landed adapters, the CloudEvents pair binds `Rasm/Domain/event#ENVELOPE_MINT` `EventCarrier.Read`/`.Write` directly, and NATS headers bind their getter and setter beside the egress leg this section's Boundary routes them to; four causal slots close the stamp's carriage under `[CAUSAL_CARRIAGE]`, each naming the C# member that carries it rather than a header key this branch mints.
- Law: W3C trace-context is the identity wire — `ActivityContext` carries the 16-byte `ActivityTraceId` crossing every hop of one trace beside the 8-byte `ActivitySpanId` naming one hop's parent — and `Continue` seats the extracted context as the started `Activity`'s parent, so an inbound stamp is adopted whole and a fresh root mints only where extraction yields none. `CorrelationId` from `Rasm/Domain/frame#SOURCE` rides `Baggage` and the kernel `HlcStamp` (`#STAMP`) rides the CloudEvents `time`/`sequence` slots of every published fact and `clock.Hlc` on the fault detail, each independent evidence beside the context; neither occupies a trace or span id slot, and the Serilog trace-id and span-id fields bind the live `Activity` ids alone. `python:runtime/observability/telemetry` is the consuming counterpart — it continues traces this spine produces.
- Law: causal frames ride one `Runtime/resources#AMBIENT_SLOT` `AmbientSlot<CorrelationFrame>` value and nothing else — the branch's one ambient carrier, LIFO-restoring the frame it displaced and REFUSING past its declared nesting bound. A page-local `AsyncLocal` beside a hand-written scope class is the deleted form, and the bound is what turns a runaway re-restore into a typed refusal at the boundary that nested it rather than a shadowed frame every downstream read then trusts.
- Entry: `Correlation.Stamp` is one entry discriminating on the value it scopes — a `CorrelationId` seats the boot root, a `TenantContext` seats tenancy across every registered store — each returning the restoring ambient scope; `Correlation.Capture()` reads the innermost frame and falls back to the live triple; `Correlation.Restore(frame)` returns `Fin<IDisposable>` — it enters the slot and rehydrates the log, baggage, and tenancy halves, refusing past the slot's bound; `TraceContext.Inject<TCarrier>(carrier, set)` writes the active context, `TraceContext.Extract<TCarrier>(carrier, get)` reads the parent context, and `TraceContext.Continue<TCarrier>(source, carrier, get, name, adoption, kind)` extracts the parent, resolves the carrier's tenancy under its `TenantAdoption` row, scopes `Baggage.Current`, and starts the continued `Activity` through the composition-owned source from `TelemetryIdentity.Mint`; the `Metadata` overloads are the gRPC adapter the Wire/companion#CONTROL_SERVICE handler reads, the `MqttApplicationMessageBuilder` overload the publish-edge adapter `MqttLane.Write` threads before `Build()`, and the `MqttApplicationMessage` overload the receive pump continues under a consumer kind.
- Auto: one boot mint stamps `LogContext` properties, `Baggage`, meter tags, published facts, and support manifests — deletes per-call-site correlation parameters across the suite; the two enrichers feed `IEnrichmentTagCollector` under one bounded prefix — the causal seat reads the scoped baggage value through `AddLogEnricher<CausalEnricher>`, the identity seat is the pre-constructed projection through `AddStaticLogEnricher(RootEnricher)` because the resolved record fixes it at composition, never from DI activation; pooled-callback, native-callback, and manual-thread ambient breaks share one repair — `Capture` snapshots the log, baggage, and tenancy triple and `Restore` enters the slot with all three at deferred-work entry, so a fact published on a pooled thread carries the tenant its originating request admitted rather than reading single-tenant off an empty slot; `TraceContext` rides the same `Correlation.Spine` composite the provider program binds, so the W3C `traceparent`/`tracestate` carrier and the `Baggage` carrier inject and extract in one pass and a continued remote span shares the in-process correlation id automatically.
- Packages: Rasm, OpenTelemetry, Serilog, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core, Grpc.Core.Api, MQTTnet, BCL inbox.
- Growth: a new stamped carrier is one stamp row inside `Stamp` with one policy value; a new ambient store is one `TenantMirror` row beside `OtelBaggage` that every existing call site inherits with no edit; a new identity dimension is one `ProfileIdentity.ResourceAttributes` row both the resource and the log seat inherit, a new request dimension one `CausalEnricher` line; a new propagation carrier is one getter/setter adapter pair over the generic `Inject`/`Extract` on the same `Spine` composite beside the `TenantAdoption` row its trust class already carries, never a second tracer; a new causal slot is one `[CAUSAL_CARRIAGE]` row naming the member that carries it, which a peer runtime proves its own carriage against; zero new surface.
- Boundary: the composite registers as `Propagators.DefaultTextMapPropagator` and crosses every hop through `TextMapPropagator.Inject` and `TextMapPropagator.Extract`, riding gRPC metadata on the local-ipc leg; `TraceContext` is the boundary owner of every crossing — the propagation mechanics live here while each transport boundary consumes its adapter pair, so a per-transport hand-rolled `traceparent` header write is the deleted form; MQTT publish writes v5 user properties through the catalogued non-obsolete `WithUserProperty(string name, ReadOnlyMemory<byte> value)` builder overload and receive reads them through the `MqttApplicationMessage` `Continue` overload whose ordinal-matched getter decodes `MqttUserProperty.ValueBuffer` — both legs on the buffer pair the package's own obsolescence notes point at, so the carrier adapter family closes in both directions on one transport and no leg hand-formats a header; the CloudEvents carrier is the kernel's own `EventCarrier.Read`/`.Write` pair, so an envelope crossing any binding reaches this propagator through the ONE accessor the envelope owner publishes and no consuming folder re-spells a field name — an unrostered field DROPS on write there rather than minting an attribute every decode reads untyped; the NATS adapter alone composes egress-side because NATS carries no OTel instrumentation by design — manual inject and extract are the contract — and its concrete setter and getter land beside that leg, never a second spine; immutable `Baggage.Current` is the one ambient correlation owner and `OtelBaggage` is its ONE tenancy writer — a page reading tenancy off a raw store rather than the kernel `TenantContext.Current` accessor reads whichever of the three stores that page happens to know, which is the split-brain this row closes, and a second `TenantMirror` registration over the same store double-writes and double-restores one entry; ingress tenancy is ADMITTED, never inherited — `TenantAdoption` carries no default because trust is a property of the carrier a transport owner alone knows, an adopting leg seats the wire entry into the kernel slot so span promotion and the metric fold answer one tenant, and a refusing leg CLEARS that entry from the seated baggage so a foreign claim tags no span with a tenant every RLS predicate and every published fact answers root for; a request value placed in `RootEnricher` is a bug and an identity constant placed in `CausalEnricher` is waste — the cost-class split is structural, and the captured frame, never ambient state read at execution time, seeds deferred children; every stamp, restore, and continuation scope restores prior baggage on dispose and releases exactly once; every continuation receives its minted source, and a process-static source bypassing the factory scope is forbidden.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TenantAdoption {
    public static readonly TenantAdoption Refused = new("refused", adopt: static _ => Option<TenantContext>.None);
    public static readonly TenantAdoption Adopted = new("adopted", adopt: Correlation.Tenanted);

    [UseDelegateFromConstructor]
    public partial Option<TenantContext> Adopt(Baggage extracted);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CorrelationFrame(ILogEventEnricher Log, Baggage Baggage, TenantContext Tenant) {
    public static CorrelationFrame Live => new(LogContext.Clone(), Baggage.Current, TenantContext.Current);
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class Correlation {
    public static readonly TextMapPropagator Spine =
        new CompositeTextMapPropagator([new TraceContextPropagator(), new BaggagePropagator()]);

    public static readonly AmbientSlot<CorrelationFrame> Frame =
        AmbientSlot<CorrelationFrame>.Of(name: "rasm.correlation", depth: 8);

    public static readonly TenantMirror OtelBaggage = new(
        Store: nameof(Baggage),
        Read: static () => Optional(Baggage.GetBaggage(TenantContext.TenantSlot)),
        Write: static entry => ignore(Baggage.SetBaggage(
            TenantContext.TenantSlot,
            entry.Match<string?>(Some: static held => held, None: static () => null))));

    public static CorrelationId Mint() => CorrelationId.Create(Guid.CreateVersion7());

    public static IDisposable Stamp(CorrelationId root) =>
        Scope(Seated(Baggage.Current.SetBaggage(CorrelationId.Slot, root.ToString())),
            LogContext.PushProperty(CorrelationId.Slot, root.ToString()));

    public static IDisposable Stamp(TenantContext tenant) => tenant.Stamp(OtelBaggage);

    public static CorrelationFrame Capture() => Frame.Current.IfNone(static () => CorrelationFrame.Live);

    public static Fin<IDisposable> Restore(CorrelationFrame captured) =>
        Frame.Enter(captured).Map(seat => Scope(
            Seated(captured.Baggage), seat, LogContext.Push(captured.Log), Stamp(captured.Tenant)));

    internal static Option<TenantContext> Tenanted(Baggage extracted) =>
        Optional(extracted.GetBaggage(TenantContext.TenantSlot))
            .Bind(static text => TenantId.TryOf(text).Map(id => new TenantContext(id, text)))
            .Filter(static held => held.TenantId != TenantContext.Root.TenantId);

    internal static IDisposable Scope(Baggage prior, params ReadOnlySpan<IDisposable?> held) =>
        new CorrelationScope(prior, toSeq(held.ToArray()).Choose(Optional));

    internal static Baggage Seated(Baggage next) {
        Baggage prior = Baggage.Current;
        Baggage.Current = next;
        return prior;
    }
}

file sealed class CorrelationScope(Baggage prior, Seq<IDisposable> held) : IDisposable {
    readonly Atom<Option<(Baggage Prior, Seq<IDisposable> Held)>> release = Atom(Some((prior, held)));

    public void Dispose() => ignore(Cell.Take(release).Current.Iter(static seat => Released(seat)));

    static Unit Released((Baggage Prior, Seq<IDisposable> Held) seat) {
        try {
            ignore(seat.Held.Rev().Iter(static scope => scope.Dispose()));
        } finally {
            Baggage.Current = seat.Prior;
        }
        return unit;
    }
}

public static class TraceContext {
    public static TCarrier Inject<TCarrier>(TCarrier carrier, Action<TCarrier, string, string> set) =>
        (fun(() => Correlation.Spine.Inject(
            new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current),
            carrier,
            set))(), carrier).Item2;

    public static PropagationContext Extract<TCarrier>(TCarrier carrier, Func<TCarrier, string, IEnumerable<string>> get) =>
        Correlation.Spine.Extract(default, carrier, get);

    public static IDisposable Continue<TCarrier>(ActivitySource source, TCarrier carrier, Func<TCarrier, string, IEnumerable<string>> get, string name, TenantAdoption adoption, ActivityKind kind = ActivityKind.Server) =>
        Continued(source, Extract(carrier, get), name, adoption, kind);

    static IDisposable Continued(ActivitySource source, PropagationContext parent, string name, TenantAdoption adoption, ActivityKind kind) =>
        adoption.Adopt(parent.Baggage) switch {
            var admitted => Correlation.Scope(
                Correlation.Seated(admitted.IsSome
                    ? parent.Baggage
                    : parent.Baggage.RemoveBaggage(TenantContext.TenantSlot)),
                source.StartActivity(name, kind, parent.ActivityContext),
                admitted.Match<IDisposable?>(Some: static tenant => Correlation.Stamp(tenant), None: static () => null)),
        };

    static IEnumerable<string> Get(Metadata carrier, string key) =>
        carrier.GetAll(key).Select(static entry => entry.Value);

    public static Metadata Inject(Metadata carrier) =>
        Inject(carrier, static (c, key, value) => c.Add(key, value));

    public static PropagationContext Extract(Metadata carrier) => Extract(carrier, Get);

    public static IDisposable Continue(ActivitySource source, Metadata carrier, string name, TenantAdoption adoption) =>
        Continue(source, carrier, Get, name, adoption);

    public static MqttApplicationMessageBuilder Inject(MqttApplicationMessageBuilder carrier) =>
        Inject(carrier, static (c, key, value) =>
            ignore(c.WithUserProperty(key, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(value)))));

    public static IDisposable Continue(ActivitySource source, MqttApplicationMessage carrier, string name, TenantAdoption adoption) =>
        Continue(source, carrier, Get, name, adoption, ActivityKind.Consumer);

    static IEnumerable<string> Get(MqttApplicationMessage carrier, string key) =>
        (carrier.UserProperties ?? []).Where(entry => string.Equals(entry.Name, key, StringComparison.Ordinal))
            .Select(static entry => entry.ReadValueAsString());
}

// --- [COMPOSITION] ---------------------------------------------------------------------
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

| [INDEX] | [SLOT]   | [DOTNET_CARRIAGE]                                        | [SHARED_LAW]                                                    |
| :-----: | :------- | :------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | physical | `HlcStamp.Physical` — CloudEvents `time`, `clock.Hlc`    | physical half first, `Instant` Unix-tick `long`-LE              |
|  [02]   | logical  | `HlcStamp.Logical` — CloudEvents `sequence`, `clock.Hlc` | monotone `ulong`-LE, zeroed on a physical advance               |
|  [03]   | tenant   | `TenantContext.TenantSlot` composed, never re-minted     | one GUC, baggage, meter-tag, and partition spelling             |
|  [04]   | packed   | `Rasm/Domain/frame#STAMP` `HlcStamp.Packed`              | `physical_ticks<<64 \| logical` as one `UInt128`, bit-identical |

- Stamps ride the CloudEvents envelope's standard and profile slots per this section's Law, so no slot occupies a trace or span id and no slot widens the promoted-baggage allowlist — the allowlist stays the tenancy and correlation pair, and a stamp promoted onto spans puts a per-event value on a dimension every series groups by.
- `[03]` COMPOSES the kernel constant rather than declaring a spelling: a second `"rasm.tenant"` literal here forks the one text the RLS predicate, the cache key, the object prefix, and the meter tag all read.
- `[04]` states the KEY and references the layout, which the kernel `HlcStamp.Packed` owns; restating the shift and mask arithmetic mints a second layout authority the seal already fixed.

## [04]-[LOG_PROJECTION]

- Owner: `LogPipeline` `[SmartEnum<string>]` arbitration column; `SpineLog` generated delegates; `SpineSampler` the branch's per-entry volume verdict; `BufferScope` the hold vocabulary and `IncidentBuffers` the two-scope hold-and-replay seat; `SerilogProjectionPolicy` the shaping surface; `SerilogSinks` the concrete app-root sink set and sink-failure observer behind the pipeline arbitration; `SerilogHost` the service-aware bridge.
- Cases: one pipeline row per delivery mandate — a bound OTLP-export provider takes otel-export, an unbound one projects through Serilog; the `Owner` arbitration is the total assignment; two buffer scopes — the process-wide `GlobalLogBuffer` and the operation-scoped `PerRequestLogBuffer` — each a hold whose flush is an OUTCOME rather than a timer, and each a `BufferScope` row the flush counts itself under; four sink mandates on the serilog arm — batched wire, durable fallback, hot error tier, synchronous audit.
- Entry: `LogPipeline.Owner(ConsumptionProfile profile)` — the total arbitration projection; `SpineSampler.ShouldSample<TState>(in LogEntry<TState> entry)` — the per-entry verdict; `IncidentBuffers.Flush(InstrumentSet signals)` returns `Fin<Unit>` and writes one `AppHostMeasure.LogsFlushed` point per flushed scope — the outcome-driven replay; `SerilogProjectionPolicy.Shape(LoggerConfiguration configuration, SerilogSinks sinks)` returns the shaped configuration off the ONE sink record; `SerilogSinks.For(ConsumptionProfile profile, IBatchedLogEventSink wire, string durableRoot, string hostKey, long fileCapBytes, IDestructuringPolicy classification, InstrumentSet signals)` returns `Fin<Option<SerilogSinks>>` — the arbitration, the storage admission, the mounted observation set, and the sink-arrow set in one, absent on an export root; `SerilogHost.Boot(Option<SerilogSinks> sinks)` mints the reloadable boot logger through `CreateBootstrapLogger`; `SerilogHost.Compose(IServiceCollection services, Option<SerilogSinks> sinks)` runs the shape through `AddSerilog(IServiceCollection, Action<IServiceProvider, LoggerConfiguration>)`.
- Auto: volume governance spans THREE planes of disjoint jurisdiction and this owner seats all three, because a plane seated per emitter is a policy every new emitter re-decides. Rule selection stays `[05]`'s — category, level, and event-identity selectors a `RandomProbabilisticSamplerFilterRule` and a `LogBufferingFilterRule` express. `SpineSampler` owns the VERDICT plane as the one `LoggingSampler` subclass: a rule row cannot read the entry's own state, so the branch's real discriminants — the EVENT-kind band stride that must never thin, and the live `DegradationLevel` under which the chatty floor must thin harder — land here as one `ShouldSample` fold over `LogEntry<TState>`, registered through `AddSampler(LoggingSampler)`; a probability literal spread across rule rows re-derives that verdict per category and drifts on the first rank the fold learns. The draw arrives as a seeded `Deterministic.Supplier` off the composition's own `DeterminismContext`, so a recorded run reproduces its retained set exactly and a spec drives the verdict from a known sequence rather than asserting against a process-random one. Two scopes carry the HOLD plane and `IncidentBuffers` is their ONE flush owner, so the fault transition and the support capture drive one replay rather than each reaching whichever ring it happens to hold: `AddGlobalBuffer` seats the process ring, while `PerRequestLogBuffer` is RESOLVED rather than registered — its activation verb ships with the ASP.NET middleware this package does not admit, so the seat is an `Option<PerRequestLogBuffer>` a hosting root may fill and the interior flushes on a failed command or hop outcome, holding the verbose tiers of a succeeding operation and replaying exactly the one that failed; with no seat bound the global ring alone carries and nothing degrades. `LogBuffer.TryEnqueue<TState>(IBufferedLogger, in LogEntry<TState>)` is the admission both scopes share and `IBufferedLogger` the replay contract the pipeline's own provider satisfies, so a held record replays through the same path it took live rather than a second formatting path. Generated delegates carry stable `EventId` and `EventName`; `[LogProperties]` expands typed payloads into bounded tags with classification intact — `Transitive` opening the nested-member expansion where a payload's own members carry annotations a flat expansion strands — `[TagProvider]` projects a foreign type that carries no annotation, and `[TagName]`/`[LogPropertyIgnore]` rename and elide at the declaration; a payload whose classification is resolved at runtime rather than declared writes through `LoggerMessageHelper.ThreadLocalState`, `ReserveTagSpace(int)` sizing the pooled array once and `AddClassifiedTag(string, object?, DataClassificationSet)` carrying each tag's own set, so the one dynamic emitter stays allocation-free and its tags reach the redaction boundary classified exactly as a generated one's do; the wire sink rides one `BatchingOptions` latency/throughput square while `Fallible` wraps it in a `FailureListenerSink` that reports sink failure to `SerilogSinks`, which writes `AppHostMeasure.LogsLost` by sink and failure kind through the mounted set, `AuditTo` propagates it to the caller, `FallbackChain` reroutes on synchronous throw, and `Conditional` forks the error-and-above tier to the hot sink.
- Packages: Rasm (kernel `Deterministic`, `FaultBand`, `InstrumentSet`), Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Telemetry, Microsoft.Extensions.Telemetry.Abstractions, Serilog, Thinktecture.Runtime.Extensions.
- Growth: one spine event is one generated-delegate row inside the `FaultBand.SpineEvents` stride; one delivery mandate is one `LogPipeline` row the `Owner` fold selects off a capability column; one volume discriminant is one arm on the `SpineSampler` fold; one buffer scope is one `BufferScope` row both `IncidentBuffers` and its flush counter read; one sink mandate is one `SerilogSinks` column the one `Shape` call already threads; one sink-loss kind is one `LoggingFailureKind` value the `AppHostMeasure.LogsLost` dimension already partitions; one draw lane is one `TelemetryLane` row; zero new surface.
- Boundary: `Rasm.AppHost` IS the branch's telemetry composition owner and holds Serilog, exporter, and SDK types by charter — the no-exporter-below-composition law scopes to the S0-S2 library tiers, where a package emits `ILogger` and its minted `Meter` alone and a Serilog type, an exporter, or an ambient sink is the app-coupling defect that law forecloses; static `Log` facade CALLS are deleted at every tier while the `Log.Logger` SLOT is written exactly once, by `SerilogHost.Boot`, because the bridge reads that slot to find the reloadable logger it reconfigures; the host bridge is the service-aware `AddSerilog(IServiceCollection, Action<IServiceProvider, LoggerConfiguration>)` overload whose configuration action runs `SerilogProjectionPolicy.Shape`, and `UseSerilog` is the `IHostBuilder`-era spelling no fence here composes; `Shape` takes the ONE `SerilogSinks` record rather than its six legs spread as parameters, so the record's own construction is the only place a leg can be omitted and the shaping surface cannot be called with a set the arbitration never minted — the seven-argument twin and its `SerilogHost.Shaped` relay both delete; every sink is an app-root pin carried on that record — `WriteTo.Console(ITextFormatter)` under a display template on the hot error tier, `WriteTo.File(ITextFormatter, path, shared: true, flushToDiskInterval, rollingInterval)` on the fallback leg because co-resident processes under one mount must both append and an exclusive handle loses every record of whichever opened second, and `AuditTo.Console(ITextFormatter)` on the audit leg because a batched sink is structurally incompatible with an audit guarantee — with the durable file scoped by HOST KEY under the same rule the durable OTLP queue's storage row carries, so two hosts on one volume never append into each other's file; that storage is ADMITTED at the mint rather than guarded at the shaping call, because an empty root or host key resolves a path under the process working directory, which is the collision the host-keyed rule exists to foreclose, and a null-argument guard on a parameter the SDK itself supplies proved nothing; every leg is a sink ARROW rather than a constructed instance, so the record holds no handle and owns no disposal — `SharedFileSink` is the constructed spelling of the same sharing contract and carries `[Obsolete]` pointing back at this arrow, which returns no sink, so `flushToDiskInterval` and the ranked `CloseAndFlush` participant are the durability seats and a caller-held `IFlushableFileSink.FlushToDisk` is unreachable at this pin; the boot window logs through `CreateBootstrapLogger()`, frozen into the host pipeline when that bridge registers, so no startup fault predates the pipeline; the loss reaches the `LogsLost` counter and `SelfLog` alone — a sink failure logged through the pipeline whose sink just failed reports on the leg it names as down; destructuring pins all three caps — depth, string length, collection count — because a pipeline accepting foreign graphs is a payload-bomb vector; `CloseAndFlush` is a ranked drain participant; exactly one pipeline owner per profile row, never both on one signal; `Filter.ByExcluding` holds lifetime-noise categories out of the pipeline by `Matching.FromSource` construction, `Destructure.With` binds the redaction-preserving `IDestructuringPolicy` so a custom shaper never strips classification, and `ForContext` is the emission-side source-keyed derivation the generated delegates ride, never a second `Shape` call; `SerilogSinks` implements `ILoggingFailureListener.OnLoggingFailed(object sender, LoggingFailureKind kind, string message, IReadOnlyCollection<LogEvent>? events, Exception? exception)` and writes the dropped count by sink and kind through its mounted `InstrumentSet`; the message and exception stay callback-local, `SelfLog` being the floor that already renders them; `SelfLog.Enable` is the never-throwing floor beneath the pipeline; the two producing arms on this page write through the `AppHostMeasure` rows and their `AppHostSlot` keys rather than re-spelling a dimension key — one spelling serves the declaring roster row and the arm that stamps it, and a telemetry-local literal beside it is the fork those rosters exist to delete; `WriteTo.Fallible(configureSink, listener)` wraps the wire-sink fallback chain in a `FailureListenerSink`, and a sink outside `Fallible` is unobserved best-effort; the test row installs `AddFakeLogging` and asserts through `FakeLogCollector` snapshots, never sink text.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LogPipeline {
    public static readonly LogPipeline SerilogProjection = new("serilog-projection");
    public static readonly LogPipeline OtelExport = new("otel-export");

    public static LogPipeline Owner(ConsumptionProfile profile) =>
        profile.OtlpExport ? OtelExport : SerilogProjection;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BufferScope {
    public static readonly BufferScope Process = new("process");
    public static readonly BufferScope Operation = new("operation");
}

[SmartEnum<int>]
public sealed partial class TelemetryLane : IDrawLane<TelemetryLane> {
    public static readonly TelemetryLane Sampler = new(key: 0);

    static IReadOnlyList<TelemetryLane> IDrawLane<TelemetryLane>.Items => Items;
    public long Lane => Key;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class SpineLog {
    [LoggerMessage(EventId = 1000, EventName = nameof(ReloadApplied), Level = LogLevel.Information, Message = "configuration reload applied")]
    public static partial void ReloadApplied(ILogger logger, [LogProperties(OmitReferenceName = true, SkipNullProperties = true)] ReloadOutcome outcome);

    [LoggerMessage(EventId = 1001, EventName = nameof(SignalDropped), Level = LogLevel.Warning, Message = "telemetry signal {Signal} dropped {Count} events", SkipEnabledCheck = true)]
    public static partial void SignalDropped(ILogger logger, [TagName("signal.kind")] string signal, long count);

    [LoggerMessage(EventId = 1002, EventName = nameof(DrainSettled), Level = LogLevel.Information, Message = "drain settled on host {Host}")]
    public static partial void DrainSettled(ILogger logger, [TagProvider(typeof(HostTags), nameof(HostTags.Collect))] Version host, [LogPropertyIgnore] string trace);

    [LoggerMessage(EventId = 1003, EventName = nameof(PeersRefused), Level = LogLevel.Warning, Message = "federation admitted no peer: {Count} server rows refused")]
    public static partial void PeersRefused(ILogger logger, long count, [TagName("federation.fault")] string fault);

    [LoggerMessage(EventId = 1004, EventName = nameof(FlagsChanged), Level = LogLevel.Information, Message = "feature domain {Domain} re-folded {Flags}")]
    public static partial void FlagsChanged(ILogger logger, [TagName("feature.domain")] string domain, [TagName("feature.flags")] string flags);

    [LoggerMessage(EventId = 1005, EventName = nameof(ProviderReady), Level = LogLevel.Information, Message = "feature provider {Provider} ready")]
    public static partial void ProviderReady(ILogger logger, [TagName("feature.provider")] string provider);
}

public static class HostTags {
    public static void Collect(ITagCollector collector, Version value) =>
        collector.Add("host.generation", value.Major);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SpineSampler(Func<DegradationLevel> level, Func<double> draw) : LoggingSampler {
    public override bool ShouldSample<TState>(in LogEntry<TState> entry) =>
        (Audited(entry.EventId.Id), entry.LogLevel) switch {
            (true, _) => true,
            (_, >= LogLevel.Warning) => true,
            _ => draw() < Floor(level()),
        };

    static bool Audited(int eventId) => FaultBand.OwnerOf(kind: BandKind.Event, code: eventId).IsSome;

    static double Floor(DegradationLevel current) => 1d / (1 << current.Rank);
}

public sealed record IncidentBuffers(GlobalLogBuffer Process, Option<PerRequestLogBuffer> Operation) {
    public Fin<Unit> Flush(InstrumentSet signals) =>
        Replayed().TraverseM(scope => signals.Write(
                AppHostMeasure.LogsFlushed.Row, 1L, InstrumentSet.Tags((AppHostSlot.Scope, scope.Key))))
            .As().Map(static _ => unit);

    Seq<BufferScope> Replayed() =>
        (Seq((Scope: BufferScope.Process, Flush: (Action)Process.Flush))
            + Operation.Map(static held => (Scope: BufferScope.Operation, Flush: (Action)held.Flush)).ToSeq())
            .Map(static row => (fun(row.Flush)(), row.Scope).Item2);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class SerilogProjectionPolicy {
    public static readonly LoggingLevelSwitch Floor = new(LogEventLevel.Information);

    public static readonly BatchingOptions Batch = new() {
        EagerlyEmitFirstEvent = true,
        BatchSizeLimit = 500,
        BufferingTimeLimit = TimeSpan.FromSeconds(2),
        QueueLimit = 10_000,
    };

    public static LoggerConfiguration Shape(LoggerConfiguration configuration, SerilogSinks sinks) =>
        sinks.Audit((fun(() => SelfLog.Enable(Console.Error))(), configuration).Item2
            .MinimumLevel.ControlledBy(Floor)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Destructure.With(sinks.Classification)
            .Destructure.ToMaximumDepth(4)
            .Destructure.ToMaximumStringLength(1024)
            .Destructure.ToMaximumCollectionCount(64)
            .Filter.ByExcluding(Matching.FromSource("Microsoft.Hosting.Lifetime"))
            .WriteTo.Fallible(
                observed => observed.FallbackChain(
                    write => write.Sink(sinks.Wire, Batch),
                    rescue => sinks.Durable(rescue)),
                sinks)
            .WriteTo.Conditional(static log => log.Level >= LogEventLevel.Error, sinks.Hot)
            .AuditTo);
}

public sealed record SerilogSinks(
    IBatchedLogEventSink Wire,
    Action<LoggerSinkConfiguration> Durable,
    Func<LoggerAuditSinkConfiguration, LoggerConfiguration> Audit,
    Action<LoggerSinkConfiguration> Hot,
    IDestructuringPolicy Classification,
    InstrumentSet Signals) : ILoggingFailureListener {
    const string Display = "[{Timestamp:O} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    static readonly MessageTemplateTextFormatter Rendered = new(Display, CultureInfo.InvariantCulture);

    static readonly TimeSpan FileFlush = TimeSpan.FromSeconds(2);

    public static Fin<Option<SerilogSinks>> For(ConsumptionProfile profile, IBatchedLogEventSink wire, string durableRoot,
        string hostKey, long fileCapBytes, IDestructuringPolicy classification, InstrumentSet signals) =>
        LogPipeline.Owner(profile).Switch(
            state: (Wire: wire, Root: durableRoot, Host: hostKey, Cap: fileCapBytes, Shaper: classification, Signals: signals),
            serilogProjection: static row => Rooted(row).Map(Some),
            otelExport: static _ => Fin.Succ(Option<SerilogSinks>.None));

    static Fin<SerilogSinks> Rooted(
        (IBatchedLogEventSink Wire, string Root, string Host, long Cap, IDestructuringPolicy Shaper, InstrumentSet Signals) row) =>
        row.Root.Length > 0 && row.Host.Length > 0
            ? Fin.Succ(new SerilogSinks(
                Wire: row.Wire,
                Durable: into => ignore(into.File(
                    new JsonFormatter(),
                    Path.Join(row.Root, row.Host, "spine.jsonl"),
                    fileSizeLimitBytes: row.Cap,
                    shared: true,
                    flushToDiskInterval: FileFlush,
                    rollingInterval: RollingInterval.Day)),
                Audit: static sink => sink.Console(Rendered),
                Hot: static into => into.Console(Rendered),
                Classification: row.Shaper,
                Signals: row.Signals))
            : Fin.Fail<SerilogSinks>(new TelemetryFault.Composition($"serilog-durable-storage:{row.Host}"));

    void ILoggingFailureListener.OnLoggingFailed(object sender, LoggingFailureKind kind, string message,
        IReadOnlyCollection<LogEvent>? events, Exception? exception) =>
        ignore(Signals.Write(
            AppHostMeasure.LogsLost.Row, events?.Count ?? 0,
            InstrumentSet.Tags((AppHostSlot.Sink, sender.GetType().Name), (AppHostSlot.Loss, kind.ToString()))));
}

public static class SerilogHost {
    public static Option<ReloadableLogger> Boot(Option<SerilogSinks> sinks) =>
        sinks.Map(static held => Seated(
            SerilogProjectionPolicy.Shape(new LoggerConfiguration(), held).CreateBootstrapLogger()));

    static ReloadableLogger Seated(ReloadableLogger boot) => (fun(() => Log.Logger = boot)(), boot).Item2;

    public static IServiceCollection Compose(IServiceCollection services, Option<SerilogSinks> sinks) =>
        sinks.Match(
            Some: held => services.AddSerilog((_, configuration) =>
                ignore(SerilogProjectionPolicy.Shape(configuration, held))),
            None: () => services);
}
```

## [05]-[SIGNAL_GOVERNANCE]

- Owner: `TelemetryDomain` `[SmartEnum<string>]` the branch domain roster under Tier-0 `[06]-[OBSERVABILITY_CONFORMANCE]` — each row a capability subject carrying its `Head`/`Measure` name projections, so every `rasm.*` instrument name and rasm-owned dimension key in this branch resolves a row or refuses at admission; `SignalCapability` `[SmartEnum<string>] : ICapability<SignalCapability>` the per-signal posture vocabulary and `TelemetrySignal` `[SmartEnum<string>]` the four governance rows carrying that set beside their ratio column; `TelemetryFault` `[Union]` the page's banded refusal family; `ResourceIdentity` the one detector-composed `Action<ResourceBuilder>` every provider owner consumes; `InstrumentMount` from Observability/instruments#INSTRUMENT_MOUNT the mount this capsule folds; `TelemetryComposition` the disposable composition capsule every governance entry folds — resolved row, boot correlation, determinism context, contributed roster, offline policy, clock, timeline, its own meter factory, the one `LevelCells`, the mounted `InstrumentSet` every AppHost producer writes through, the process `SpanBand` folded from the contributed trace planes, and the opened per-signal queue set; `SignalGovernance` the registration fold binding the `Observability/instruments#PROVIDER_LIFETIME` `ProviderProgram` onto the hosted root.
- Cases: one governance row per signal — trace, metric, log, profile — each holding the capability set that decides buffering, redaction, and OTLP egress beside its own ratio projection, the `Exported` capability selecting which three open a durable queue; five refusal cases close the page's fault family — an unrostered name, a foreign taxonomy, an undeclared federation value, a short redaction sink, and a composition input the admission refused.
- Entry: `TelemetryComposition.Of(ResolvedProfile resolved, CorrelationId root, DeterminismContext determinism, ClockPolicy clocks, IConfigurationSection hmacKeys, Func<DegradationLevel> level, Seq<LatencyRoster> latency, params ReadOnlySpan<TelemetryContributorPort> contributors)` returns `Fin<TelemetryComposition>` — it mints the meter factory, mounts every contributor through `Observability/instruments#INSTRUMENT_MOUNT` `InstrumentMount.Mount`, opens the queue set over that set and the one `SpanBand`, and derives the federation `ClassificationRoster` set from each contributed port's `Classifications` column; `Dispose` releases transports, band, queues, and the meter provider at the telemetry drain band; `SignalGovernance.Rostered(TelemetryContributorPort)` returns `Validation<Error, TelemetryContributorPort>` accumulating every unrostered spelling on the port's whole declared surface, and `SignalGovernance.Rostered(EventType)` returns `Fin<EventType>` because one name carries one refusal; `SignalGovernance.Views(Seq<TelemetryContributorPort>)` projects the one per-instrument view function both provider owners bind; `SignalGovernance.Govern(IServiceCollection services, TelemetryComposition composition)` returns the host-owned `OpenTelemetryBuilder` with the `ProviderProgram` bound on both provider builders, `SignalGovernance.GovernLogs(ILoggingBuilder, TelemetryComposition)` binds the sampler, redaction, and incident-buffer stages on the `ILogger` floor, and `SignalGovernance.EnrichContext(IServiceCollection, TelemetryComposition)` seats the two enricher rows, the latency ledger, and the outbound client-log taxonomy; `ResourceIdentity.Compose(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra)` the one identity delegate — the resolved-record detector ahead of the contrib chain and the deployment override behind it, with composition facts riding as data; `SignalGovernance.StoreDriver(OpenTelemetryBuilder)` and `SignalGovernance.StoreWire<TKey, TValue>(OpenTelemetryBuilder)` the store-composing service-root rows — driver subscription shape-free and once, wire instrumentation once per message shape.
- Law: provider POLICY is not this owner's. `ProviderProgram.Canonical` at Observability/instruments#PROVIDER_LIFETIME carries the batch squares, reader cadence, temporality, exemplar filter, propagator, promoted-baggage predicate, and the two OTLP wire pins as VALUES, and both provider owners — the hosted root here and the per-ALC plugin capsule there — bind the SAME value. Eight identical rows spelled at each owner beside a prose claim that they agree is the deleted form.
- Auto: provider `ForceFlush` and `Shutdown` ride the telemetry drain band; the fault transition lands the `IncidentBuffers.Flush` window inside support capture, so both held scopes replay under one owner rather than a capture reaching the process ring alone; `AddRandomProbabilisticSampler` carries a `RandomProbabilisticSamplerFilterRule` row keyed by maximum level so it thins the chatty floor and never the error ceiling, while a `LogBufferingFilterRule` row holds the verbose tiers until an incident flushes them, bounded by the `GlobalLogBufferingOptions` caps — record size, buffer size, auto-flush window — so the incident buffer never runs unbounded; `AddHttpClientInstrumentation` binds `HttpClientTraceInstrumentationOptions` — `FilterHttpRequestMessage` drops the loopback leg, `EnrichWithException` records through `Activity.AddException` so the exception event carries the runtime's own `exception.type`/`message`/`stacktrace` grammar and the listener-installed `ExceptionRecorder` shapes it in one place, and URL-query redaction stays the package default; `Views` binds ONE `AddView` predicate resolving each published instrument against the contributed roster — a rostered stream takes its declaring row's `Dimensions` beside the one tenancy key as `TagKeys` under the `SeriesCap` budget, and a foreign stream keeps its semconv vocabulary under the same budget — so an undeclared tag reaches no exporter, the per-tenant `TenantContext.Tags` dimension stays inside a bounded series budget on every minted meter, and one instrument yields exactly one stream — the key set is a SET, so a keyed level family declaring the tenancy slot as its own leading dimension takes it once, and it ALLOWLISTS rather than requires, so an entry omitting a declared key is shaped by that same row with the key absent and a family carrying its key on some entries alone stays one stream under one budget; egress binds per signal rather than through one cross-signal call, so each signal carries its own batch square, its own temporality and reader cadence, and its own durable transport, and the three `AddOtlpExporter` rows are what make those knobs reachable at all — the cross-signal `UseOtlpExporter` seat exposes exporter options, reader options, and processor options through `internal` types alone; `Egress` binds every exporter arm — the program's wire pins stamp first because the exporter parses its `OTEL_EXPORTER_OTLP_*` wire keys at options construction and the deploy plane publishes the endpoint row alone, so a pin left to those keys resolves the shipped gRPC-and-uncompressed defaults — and an `OtlpOfflinePolicy` armed by durable disk BESIDE a bound OTLP provider opens one queue per exported signal at composition, where `Egress` then swaps that signal's transport for `PersistentOtlpHandler`; `EnableEnrichment` activates the `RootEnricher`/`CausalEnricher` seats and binds `LoggerEnrichmentOptions` — `CaptureStackTraces` and `IncludeExceptionMessage` admit exception frames onto the log signal behind the redaction boundary, and `UseFileInfoForStackTraces` stays off because file and line are leak-bearing; the serilog-projection rows add `AddConsoleLatencyDataExporter` so a desktop or test host reads latency spans live with zero wire cost; `ResourceIdentity.Compose` runs the minted `service.namespace`/`service.name`/`service.instance.id` triple delegate first, then chains `AddHostDetector`/`AddOperatingSystemDetector`/`AddProcessDetector`/`AddProcessRuntimeDetector` always-on and `AddContainerDetector` on the OCI-vehicle rows alone — detectors ENRICH and never replace the mint, each contributing only the semconv attributes it resolves (`host.*`, `os.*`, `process.*`, `process.runtime.*`, `container.id`), placement dimensions no backend derives from the triple, and `AddEnvironmentVariableDetector` tails the chain so the deploy plane's `OTEL_RESOURCE_ATTRIBUTES` outranks every row ahead of it; the program's baggage predicate promotes the allowlisted `rasm.tenant` and `CorrelationId` entries onto every span at start, so a backend groups spend, latency, and traces by tenant with zero per-call-site tagging; `AddHttpClientLatencyTelemetry()` installs the per-phase checkpoint handler over every named `HttpClient` — name-resolution versus connection versus server time at checkpoint cost, `EnableDetailedLatencyBreakdown` the package-default breakdown — and `AddExtendedHttpClientLogging` replaces the built-in client logger with the redaction-aware form whose four `*DataClasses` maps bind the `[08]` taxonomy through `DataClassification.Marker`, bespoke tags entering as `AddHttpClientLogEnricher<T>` rows.
- Growth: a new capability subject is one `TelemetryDomain` row and a second package emitting under a standing subject adds none — the roster grows on subjects, never on emitters; a contributor minting on its own load-context meter is one `Published` roster on its port that the standing gate and the standing view predicate already read; one governance decision is one `SignalCapability` row or one policy value on the row that holds it; one stream reshaping is one `Dimensions` edit at the roster that declares the instrument, which the one view predicate already reads — a second `AddView` row is the shape that mints a duplicate stream; a new store message shape is one `StoreWire<TKey, TValue>` closure at the composing root; a new resource dimension is one detector row inside `ResourceIdentity.Compose` and a new composition fact one `TelemetryComposition` column; a signal crossing onto the OTLP wire is one `Exported` capability the queue set and the exporter roster both fold; a new refusal is one `TelemetryFault` case on the page's own band; zero new surface.
- Boundary: this platform admits all span custody — one `SpanBand` over contributed `Planes`, registered beside `ForeignSource`.
- Boundary: a foreign meter admitted through `ForeignSource` publishes under the LIBRARY's grammar rather than this branch's, so `Rostered` never reaches its names and the view predicate's foreign arm keeps that vocabulary whole under the shared budget — the resilience family is the case pricing that decision. `resilience.polly.strategy.events` counts events while `resilience.polly.strategy.attempt.duration` and `resilience.polly.pipeline.duration` time attempts and pipelines in `ms`, every one partitioning on `event.name`, `event.severity`, `pipeline.name`, `pipeline.instance`, `strategy.name`, `operation.key`, and `exception.type` beside each `MeteringEnricher` tag a composing pipeline appends, with `attempt.number` and `attempt.handled` on the attempt histogram alone; `exception.type` renders a type's full name and `operation.key` fixes per execution, so this family carries the branch's widest foreign dimension product, it overflows at `SeriesCap` rather than at an exporter, and a lane, hop, or tenant enricher row spends that same budget. Both histograms mint with NO bucket advice, so the exponential foreign arm is their only ladder and the producer-fixed-ladder carve never engages here, and both instrument kinds are synchronous and additive, so the delta temporality pin loses nothing across the reader cadence. Absence is per dimension rather than per stream — an unnamed pipeline, an unset instance formatter, and a context leased without an operation key each drop their own key and leave the rest.
- Boundary: the domain roster is branch-owned and every instrument reads that one vocabulary; deployment identity dimensions ride the resource projection and carry no segment, which is why `Rostered` reads the program's promoted-baggage predicate as its carve rather than minting a second allowlist, and hook-point ids stay a package-keyed space this grammar never reaches; the OTLP exporter package enters only at service app roots — the otelExport arm binds one `AddOtlpExporter` per signal through the one `Egress` seat, which stamps the program's solution wire pins — HTTP+protobuf the one egress protocol, gzip the one payload encoding — while endpoint, headers, and mTLS material stay deploy-plane rows the `OTEL_EXPORTER_OTLP_*` keys carry; that egress boundary is `OtelExport`; the per-signal rows and the cross-signal `UseOtlpExporter` seat are mutually exclusive by package law and the per-signal form is the branch's, because the cross-signal seat routes every knob the program pins through `internal` option types and a composition behind it can set no batch square, no temporality, no reader cadence, and no transport; EF Core emits no `ActivitySource` of its own, so the ORM-layer command span is the `AddEntityFrameworkCoreInstrumentation` row `StoreDriver` registers — nesting over the Npgsql ADO driver span, complementary never redundant — while gRPC client spans ride `AddGrpcClientInstrumentation` with `SuppressDownstreamInstrumentation` so the underlying HTTP/2 leg never double-traces, superseding the client's native `Grpc.Net.Client` source and its single `Grpc.Net.Client.GrpcOut` per-call activity, and neither surface adds an `AddSource` row; the otelExport arm carries `AddAspNetCoreInstrumentation` beside the wire host for inbound request spans; store telemetry is the PORT-peer arbitration — Persistence owns the driver and the instrumented builders while the app root alone registers, so no downward reference forms: `StoreDriver` subscribes `AddNpgsql` tracing, `AddEntityFrameworkCoreInstrumentation` ORM tracing, and `AddNpgsqlInstrumentation` metrics once at the store-composing root, with `NpgsqlDataSourceBuilder.Name` (`string?`, get/set) assigned Persistence-side per logical database so `db.client.connection.pool.name` keys stable pool dimensions, and `StoreWire<TKey, TValue>` registers `AddKafkaProducerInstrumentation`/`AddKafkaConsumerInstrumentation` on both providers once per message shape over the Persistence egress `AsInstrumentedProducerBuilder` and CDC ingress `InstrumentedConsumerBuilder` legs, closing the producer-only Kafka asymmetry; the builtin rows delete the meter-side `AddRuntimeInstrumentation` registration because the runtime publishes `System.Runtime` inbox and a meter-name row is that whole admission, while `AddProcessInstrumentation` survives as a verb on the always-on metrics arm because its absolute process series exist only on the meter its registered instrumentation instance constructs, so a `ForeignSource` row subscribes an empty scope; the trace-side `AddHttpClientInstrumentation` row keeps URL-query redaction; test-row trace assertions ride one `BaseProcessor<Activity>` through `AddProcessor` and metric assertions ride `MetricCollector<T>` — no in-memory exporter package enters a production tier, and the test suite's own in-memory exporter row stays its own admission; the `Profile` signal is the continuous-profiling path — the admitted `PyroscopeSpanProcessor` registers as `AddProcessor<PyroscopeSpanProcessor>()` through the same seat the test-row processor uses, gated to service app roots where the profiler endpoint resolves — `Profiler.Instance` carries no address member, so the ingest address is the agent `PYROSCOPE_SERVER_ADDRESS` environment row bound from the deploy-plane profiles endpoint beside `PYROSCOPE_APPLICATION_NAME` and the CLR-profiler enablement rows, with `SetAuthToken`/`SetBasicAuth` the credential boundary and `PYROSCOPE_TENANT_ID` the tenancy row — and stamps the `pyroscope.profile.id` tag on each root span so a flame graph scopes to the exact trace that showed a regression; the GenAI semantic conventions ride the trace and metric signals — an MCP-served tool span carries `gen_ai.operation.name` and the `gen_ai.provider.name` provider discriminant beside the `gen_ai.usage.input_tokens`/`output_tokens` counts, and the token-usage instruments ride the minted `Rasm.AppHost` meter, so the agentic surface the host serves shares one telemetry taxonomy with the runtime, never a parallel agent-metrics owner; log-record identity is the resource projection and the shipped `AddApplicationLogEnricher` row is DELETED, not configured off — it writes an unqualified `service.name` and the deprecated `deployment.environment` spelling as package-owned tags, so leaving it seated puts two values of one dimension on two planes and a query joining a log record to a metric series compares neither, while `AddProcessLogEnricher` survives on its `ThreadId` column alone because the resource already carries `process.pid`; the latency breakdown and the `AddHttpClientInstrumentation` span are two projections of one hop — the checkpoint handler never mints a second trace, extended logging supersedes the built-in client logger so both active is the double-log defect, and `wrapHandlersPipeline` decides whether logging observes pre- or post-retry attempts on the resilience chain.
- Swap: this owner holds the branch's profiles swap point, so the swap off vendor push onto the OTLP profiles signal replaces rows rather than redesigning a lane — the `AddProcessor<PyroscopeSpanProcessor>()` seat and the agent environment rows give way to one OTLP profiles exporter row on the same otelExport arm, armed only once that signal reaches stable across the SDK trains; the span-profile stamp, tenant and phase projections, and every flamegraph query survive untouched.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TelemetryDomain {
    public static readonly TelemetryDomain AppHost = new("apphost", "application-platform lifecycle, admission, and brokered capability");
    public static readonly TelemetryDomain AppUi = new("appui", "desktop shell interaction, charting, and collaboration surfaces");
    public static readonly TelemetryDomain Bim = new("bim", "building-model exchange, energy, and semantic-graph work");
    public static readonly TelemetryDomain Compute = new("compute", "solver execution, monitoring, and the numerical residual per graduation");
    public static readonly TelemetryDomain Deploy = new("deploy", "the consumption-profile axes a signal groups on");
    public static readonly TelemetryDomain Element = new("element", "element-contract projection and wire volume");
    public static readonly TelemetryDomain Fabrication = new("fabrication", "fabrication engines, delivery programs, and process objectives");
    public static readonly TelemetryDomain Fault = new("fault", "the owner, posture, code, and case discriminants a signal groups on");
    public static readonly TelemetryDomain Grasshopper = new("grasshopper", "canvas solution, paint, and interaction surfaces");
    public static readonly TelemetryDomain Host = new("host", "the host-boundary discriminant a signal groups on");
    public static readonly TelemetryDomain Kernel = new("kernel", "kernel op cost and fault counts");
    public static readonly TelemetryDomain Materials = new("materials", "material projection and impact assemblies");
    public static readonly TelemetryDomain Outbound = new("outbound", "the outbound-route discriminant a signal groups on");
    public static readonly TelemetryDomain Persistence = new("persistence", "store usage, census, and durable-work health");
    public static readonly TelemetryDomain Rhino = new("rhino", "document, display, and bench surfaces of the modeling host");
    public static readonly TelemetryDomain Slo = new("slo", "objective burn and severity axes");

    public const string Namespace = "rasm";

    public const string Prefix = Namespace + ".";

    public string Subject { get; }

    public string Head => Prefix + Key + ".";

    public string Measure(string measure) => Head + measure;

    public static string Qualify(string service) => Qualified(service.ToLowerInvariant());

    static string Qualified(string lowered) => lowered.StartsWith(Prefix, StringComparison.Ordinal) ? lowered : Prefix + lowered;

    public static Fin<TelemetryDomain> Resolve(string name) =>
        toSeq(Items)
            .Find(row => name.StartsWith(row.Head, StringComparison.Ordinal))
            .ToFin(new TelemetryFault.Unrostered(name));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SignalCapability : ICapability<SignalCapability> {
    public static readonly SignalCapability Buffered = new("buffered", rank: 0);
    public static readonly SignalCapability Redacted = new("redacted", rank: 1);
    public static readonly SignalCapability Exported = new("exported", rank: 2);

    public int Rank { get; }

    static IReadOnlyList<SignalCapability> ICapability<SignalCapability>.Items => Items;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TelemetrySignal {
    public static readonly TelemetrySignal Log = new("log",
        CapabilitySet<SignalCapability>.Of(SignalCapability.Buffered, SignalCapability.Redacted, SignalCapability.Exported), SampleRatio);
    public static readonly TelemetrySignal Trace = new("trace",
        CapabilitySet<SignalCapability>.Of(SignalCapability.Redacted, SignalCapability.Exported), SampleRatio);
    public static readonly TelemetrySignal Metric = new("metric",
        CapabilitySet<SignalCapability>.Of(SignalCapability.Exported), static _ => 1d);
    public static readonly TelemetrySignal Profile = new("profile", CapabilitySet<SignalCapability>.None, SampleRatio);

    public CapabilitySet<SignalCapability> Capabilities { get; }

    [UseDelegateFromConstructor]
    public partial double Ratio(ConsumptionProfile profile);

    private static double SampleRatio(ConsumptionProfile profile) =>
        profile.OtlpExport
            ? profile.Topology.Map(inHost: 1d, sidecar: 0.1d, companion: 0.1d, service: 0.1d, edge: 0.1d, cli: 1d)
            : 1d;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TelemetryFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Telemetry;
    private TelemetryFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record Unrostered : TelemetryFault { public Unrostered(string name) : base($"{name}: a rostered rasm.<domain> segment") { } }
    [FaultCase(1)]
    public sealed partial record Taxonomy : TelemetryFault { public Taxonomy(string pair) : base($"{pair}: the suite classification taxonomy") { } }
    [FaultCase(2)]
    public sealed partial record Unclassified : TelemetryFault { public Unclassified(string package, string pair) : base($"{package}:{pair}") { } }
    [FaultCase(3)]
    public sealed partial record Sink : TelemetryFault { public Sink(int capacity) : base($"<redaction-sink:{capacity}>") { } }
    [FaultCase(4)]
    public sealed partial record Composition : TelemetryFault { public Composition(string detail) : base(detail) { } }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class TelemetryComposition : IDisposable {
    readonly ConcurrentBag<HttpClient> transports = [];
    readonly Atom<Option<Unit>> released = Atom(Option<Unit>.None);

    TelemetryComposition(ResolvedProfile resolved, CorrelationId root, DeterminismContext determinism,
        Seq<TelemetryContributorPort> contributors, Seq<LatencyRoster> latency, Seq<ClassificationRoster> classifications,
        OtlpOfflinePolicy offline, ClockPolicy clocks, IConfigurationSection hmacKeys, Func<DegradationLevel> level,
        ServiceProvider meters, LevelCells cells, InstrumentSet signals, FrozenDictionary<string, OtlpOfflineQueue> queues, SpanBand band) =>
        (Resolved, Root, Determinism, Contributors, Latency, Classifications, Offline, Clocks, HmacKeys, Level, this.meters, Cells, Signals, Queues, Band) =
            (resolved, root, determinism, contributors, latency, classifications, offline, clocks, hmacKeys, level, meters, cells, signals, queues, band);

    readonly ServiceProvider meters;

    public ResolvedProfile Resolved { get; }

    public CorrelationId Root { get; }

    public DeterminismContext Determinism { get; }

    public Seq<TelemetryContributorPort> Contributors { get; }

    public SpanBand Band { get; }

    public Seq<LatencyRoster> Latency { get; }

    public Seq<ClassificationRoster> Classifications { get; }

    public OtlpOfflinePolicy Offline { get; }

    public ClockPolicy Clocks { get; }

    public IConfigurationSection HmacKeys { get; }

    public Func<DegradationLevel> Level { get; }

    public IMeterFactory Meters => meters.GetRequiredService<IMeterFactory>();

    public LevelCells Cells { get; }

    public InstrumentSet Signals { get; }

    public FrozenDictionary<string, OtlpOfflineQueue> Queues { get; }

    public static Fin<TelemetryComposition> Of(ResolvedProfile resolved, CorrelationId root, DeterminismContext determinism,
        ClockPolicy clocks, IConfigurationSection hmacKeys, Func<DegradationLevel> level,
        Seq<LatencyRoster> latency, params ReadOnlySpan<TelemetryContributorPort> contributors) =>
        Opened(resolved, root, determinism, [.. contributors], latency,
            OtlpOfflinePolicy.For(resolved), clocks, hmacKeys, level, new ServiceCollection().AddMetrics().BuildServiceProvider(), new LevelCells());

    static Fin<TelemetryComposition> Opened(ResolvedProfile resolved, CorrelationId root, DeterminismContext determinism,
        Seq<TelemetryContributorPort> contributors, Seq<LatencyRoster> latency, OtlpOfflinePolicy offline,
        ClockPolicy clocks, IConfigurationSection hmacKeys, Func<DegradationLevel> level, ServiceProvider meters, LevelCells cells) =>
        InstrumentMount.Mount(meters.GetRequiredService<IMeterFactory>(), root, cells, [.. contributors])
            .Map(signals => new TelemetryComposition(resolved, root, determinism, contributors, latency,
                contributors.Filter(static port => !port.Classifications.IsEmpty)
                    .Map(static port => new ClassificationRoster(port.Scope, port.Classifications.Map(static row => (row.Taxonomy, row.Value)))),
                offline, clocks, hmacKeys, level, meters, cells, signals,
                offline.Open(signals, clocks.Line),
                SpanBand.Of(resolved.ServiceVersion, [.. contributors.Bind(static port => port.Planes)])))
            .MapFail(refused => (fun(meters.Dispose)(), refused).Item2);

    internal HttpClient Transport(Func<HttpClient> mint) {
        HttpClient client = mint();
        transports.Add(client);
        return client;
    }

    public void Dispose() =>
        ignore(Cell.Seat(released, static () => unit) is Transition<Option<Unit>>.Committed ? Released() : unit);

    Unit Released() {
        ignore(toSeq(transports).Iter(static transport => transport.Dispose()));
        Band.Dispose();
        ignore(toSeq(Queues.Values).Iter(static queue => queue.Dispose()));
        meters.Dispose();
        return unit;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class ResourceIdentity {
    static readonly Func<ConsumptionProfile, bool> Containerized = static profile => profile.Vehicle == ShipVehicle.Oci;

    public static Action<ResourceBuilder> Compose(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra) =>
        Composed(resolved, [.. extra]);

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
    public const int SeriesCap = 256;

    public const int BufferRecordCapBytes = 128 * 1024;
    public const int BufferCapBytes = 64 * 1024 * 1024;
    public static readonly TimeSpan BufferFlushWindow = TimeSpan.FromSeconds(30);

    public static Validation<Error, TelemetryContributorPort> Rostered(TelemetryContributorPort port) =>
        port.Declared.Bind(static spec => spec.Name.Cons(spec.Dimensions))
            .Filter(static name => name.StartsWith(TelemetryDomain.Prefix, StringComparison.Ordinal)
                && !ProviderProgram.Canonical.Baggage(name))
            .Traverse(static name => TelemetryDomain.Resolve(name).ToValidation()).As()
            .Map(_ => port);

    public static Fin<EventType> Rostered(EventType type) => TelemetryDomain.Resolve(type.Domain).Map(_ => type);

    public static Func<Instrument, MetricStreamConfiguration?> Views(Seq<TelemetryContributorPort> contributors) =>
        Projected(contributors.Bind(static port => port.Declared)
            .ToFrozenDictionary(static spec => spec.Name, static spec => spec, StringComparer.Ordinal));

    static Func<Instrument, MetricStreamConfiguration?> Projected(FrozenDictionary<string, InstrumentSpec> rostered) =>
        instrument => rostered.TryGetValue(instrument.Name, out InstrumentSpec? spec)
            ? Shaped(spec.Kind == InstrumentKind.Distribution && spec.Bounds.IsNone,
                [.. spec.Dimensions.Add(TenantContext.TenantSlot).Distinct()])
            : Shaped(Distributed(instrument), null);

    static MetricStreamConfiguration Shaped(bool exponential, string[]? tags) =>
        exponential
            ? new Base2ExponentialBucketHistogramConfiguration { TagKeys = tags, CardinalityLimit = SeriesCap }
            : new MetricStreamConfiguration { TagKeys = tags, CardinalityLimit = SeriesCap };

    static bool Distributed(Instrument instrument) =>
        instrument.GetType() is { IsGenericType: true } shape && shape.GetGenericTypeDefinition() == typeof(Histogram<>);

    public static OpenTelemetryBuilder Govern(IServiceCollection services, TelemetryComposition composition) =>
        Governed(services, composition, ProviderProgram.Canonical);

    static OpenTelemetryBuilder Governed(IServiceCollection services, TelemetryComposition composition, ProviderProgram program) =>
        LogPipeline.Owner(composition.Resolved.Profile).Switch(
            state: Propagated(services, program).AddOpenTelemetry()
                .ConfigureResource(ResourceIdentity.Compose(composition.Resolved))
                .WithTracing(tracing => program.Bind(tracing
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(TelemetrySignal.Trace.Ratio(composition.Resolved.Profile))))
                    .AddSource([.. ForeignSource.Admitting(TelemetrySignal.Trace)])
                    .AddSource([.. composition.Band.Names])
                    .AddHttpClientInstrumentation(static http => {
                        http.FilterHttpRequestMessage = static request => request.RequestUri is { IsLoopback: false };
                        http.EnrichWithException = static (activity, exception) =>
                            ignore(activity.AddException(exception));
                    })
                    .AddGrpcClientInstrumentation(static grpc => grpc.SuppressDownstreamInstrumentation = true)))
                .WithMetrics(metrics => program.Bind(metrics
                    .AddMeter([.. ForeignSource.Admitting(TelemetrySignal.Metric)])
                    .AddProcessInstrumentation(), Views(composition.Contributors))),
            serilogProjection: static builder => builder,
            otelExport: builder =>
                (builder
                    .WithTracing(tracing => tracing
                        .AddAspNetCoreInstrumentation()
                        .AddProcessor<PyroscopeSpanProcessor>()
                        .AddOtlpExporter(otlp => {
                            otlp.BatchExportProcessorOptions = program.SpanBatch;
                            ignore(Egress(composition, program, TelemetrySignal.Trace, otlp));
                        }))
                    .WithMetrics(metrics => metrics
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter((otlp, reader) => {
                            reader.TemporalityPreference = program.Temporality;
                            reader.PeriodicExportingMetricReaderOptions = program.ReaderCadence;
                            ignore(Egress(composition, program, TelemetrySignal.Metric, otlp));
                        }))
                    .WithLogging(
                        logs => logs
                            .AddBaggageProcessor(program.Baggage)
                            .AddOtlpExporter((otlp, processor) => {
                                processor.ExportProcessorType = ExportProcessorType.Batch;
                                processor.BatchExportProcessorOptions = program.LogBatch;
                                ignore(Egress(composition, program, TelemetrySignal.Log, otlp));
                            }),
                        static options => {
                            options.IncludeScopes = true;
                            options.IncludeFormattedMessage = true;
                            options.ParseStateValues = true;
                            ignore(options.AttachLogsToActivityEvent());
                        }), builder).Item2);

    static IServiceCollection Propagated(IServiceCollection services, ProviderProgram program) =>
        (fun(() => Sdk.SetDefaultTextMapPropagator(program.Propagator))(), services).Item2;

    static Unit Egress(TelemetryComposition composition, ProviderProgram program, TelemetrySignal signal, OtlpExporterOptions otlp) =>
        (program.Egress(otlp),
            Optional(composition.Queues.GetValueOrDefault(signal.Key)).Match(
                Some: queue => (fun(() => otlp.HttpClientFactory = () => composition.Transport(() =>
                    new HttpClient(new PersistentOtlpHandler(queue) { InnerHandler = OtlpTrust.Read().Mount(new SocketsHttpHandler()) }) {
                        Timeout = TimeSpan.FromMilliseconds(otlp.TimeoutMilliseconds),
                    }))(), unit).Item2,
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

    public static ILoggingBuilder GovernLogs(ILoggingBuilder logging, TelemetryComposition composition) =>
        RedactionRegistration.Bind(logging, composition.HmacKeys)
            .AddTraceBasedSampler()
            .AddRandomProbabilisticSampler(TelemetrySignal.Log.Ratio(composition.Resolved.Profile), LogLevel.Warning)
            .AddSampler(new SpineSampler(composition.Level, Deterministic.Supplier(
                seed: unchecked((long)composition.Determinism.Seed), purpose: TelemetryLane.Sampler.Lane)))
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

    public static IServiceCollection EnrichContext(IServiceCollection services, TelemetryComposition composition) =>
        LatencySpine.Register(LogPipeline.Owner(composition.Resolved.Profile).Switch(
            state: services
                .AddLogEnricher<CausalEnricher>()
                .AddStaticLogEnricher(new RootEnricher(ProfileIdentity.ResourceAttributes(composition.Resolved)))
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
            otelExport: static enriched => enriched),
            [.. composition.Latency]);
}
```

## [06]-[LATENCY_PLANE]

- Owner: `LatencyCheckpoint` `[SmartEnum<string>]` this root's own in-flight phase vocabulary, each row carrying the pivot dimension a reader groups the phase on; `LatencyRoster` one emitting package's contributed checkpoint/measure/tag names; `LatencySpine` the checkpoint recorder and the one folded name registration.
- Cases: one checkpoint row per measured phase — drain, hop, capture — each naming the `AppHostSlot` its own instrument row already declares, so one column answers both planes and no second tag vocabulary mints beside the instrument slots.
- Entry: `LatencySpine.Register(IServiceCollection, params ReadOnlySpan<LatencyRoster>)` folds this root's own vocabulary with every contributed roster into ONE registration across all three name axes under `ThrowOnUnregisteredNames`; `LatencySpine.Open(ILatencyContextProvider provider, ILatencyContextTokenIssuer issuer, LatencyCheckpoint phase)` is the composition-root factory minting one context beside its resolved token so a threaded fold never resolves a name on its own; `LatencySpine.Mark(ILatencyContext context, CheckpointToken phase)` records one checkpoint; `LatencySpine.Seal(ILatencyDataExporter exporter, ILatencyContext context)` returns `IO<Unit>` — it freezes the context and hands its `LatencyData` to the exporter under the drain band's own token.
- Auto: the latency vocabulary registers once through `RegisterCheckpointNames` at composition, `ILatencyContextTokenIssuer.GetCheckpointToken(string)` resolves each name to a `CheckpointToken`, and runtime code records through those resolved handles only — durations never derive from stamp differences; a value-bearing `MeasureToken` recording from `GetMeasureToken(string)` is a forward row admitted only when a measure consumer exists; the three phase folds each take ONE `ILatencyContext` parameter threaded from `Open` at the composition root.
- Output: `LatencyData` — the frozen checkpoint spans `ILatencyDataExporter` exports at the drain band, one span per drain, hop, and capture phase.
- Packages: Microsoft.Extensions.Telemetry.Abstractions, Microsoft.Extensions.DependencyInjection, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: one measured phase is one `LatencyCheckpoint` row recorded by one `Mark` call, and an emitting package's whole phase vocabulary is one `LatencyRoster` value on the composition the one registration already folds; zero new surface.
- Boundary: `Mark` is the single checkpoint recorder and the three phase folds thread ONE `ILatencyContext` parameter — `DrainConductor.Drain(...)` at `Runtime/lifecycle#DRAIN_CONDUCTOR`, `OutboundSurface.Run(...)` at `Wire/outbound#OWNERSHIP_LAW`, and `SupportCapture.Capture(...)` at Observability/bundles#CAPTURE_PIPELINE, which opens its OWN ledger because it IS the operation rather than a fold inside one — so a phase boundary records through a resolved token rather than a per-fold `Stopwatch`, and `Seal` exports each frozen ledger at the telemetry drain band; a context threaded as a runtime-record COLUMN instead is the rejected placement, because the record outlives the operation while the ledger is one operation's and a pooled context returned at a fold's own `using` cannot be a field of a value that survives it; the recorder is cheaper than child spans and free of sampling coupling; the frozen spans read through the `ILatencyContext.LatencyData` accessor and `ILatencyDataExporter.ExportAsync(LatencyData, CancellationToken)` exports at the telemetry drain band; `AddLatencyContext` registers the context once and the consuming folds thread it; the NAME registration is one folded table over this root's roster and every contributor's, across checkpoints, measures, and tags alike, because an unregistered name resolves to a positionless token whose writes drop with nothing raised — a contributor recording under its own unfolded roster is instrumented in prose and silent on the wire, which is why `ThrowOnUnregisteredNames` makes the omission a boot failure and why no contributor reaches its own `RegisterCheckpointNames` call to split the table.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LatencyCheckpoint {
    public static readonly LatencyCheckpoint Drain = new("drain", AppHostSlot.Band);
    public static readonly LatencyCheckpoint Hop = new("hop", AppHostSlot.Hop);
    public static readonly LatencyCheckpoint Capture = new("capture", AppHostSlot.Trigger);

    public AppHostSlot Pivot { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct LatencyRoster(Seq<string> Checkpoints, Seq<string> Measures, Seq<string> Tags) {
    public static readonly LatencyRoster Empty = new(Seq<string>(), Seq<string>(), Seq<string>());

    public static LatencyRoster operator +(LatencyRoster left, LatencyRoster right) =>
        new(left.Checkpoints + right.Checkpoints, left.Measures + right.Measures, left.Tags + right.Tags);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LatencySpine {
    public static IServiceCollection Register(IServiceCollection services, params ReadOnlySpan<LatencyRoster> contributed) {
        LatencyRoster folded = Iterable<LatencyRoster>.FromSpan(contributed).Fold(Own, static (all, next) => all + next);
        return services
            .Configure<LatencyContextOptions>(static options => options.ThrowOnUnregisteredNames = true)
            .RegisterCheckpointNames([.. folded.Checkpoints.Distinct()])
            .RegisterMeasureNames([.. folded.Measures.Distinct()])
            .RegisterTagNames([.. folded.Tags.Distinct()]);
    }

    static LatencyRoster Own =>
        toSeq(LatencyCheckpoint.Items) is var phases
            ? new(phases.Map(static row => row.Key), Seq<string>(), phases.Map(static row => row.Pivot.Key))
            : LatencyRoster.Empty;

    public static (ILatencyContext Context, CheckpointToken Phase) Open(
        ILatencyContextProvider provider, ILatencyContextTokenIssuer issuer, LatencyCheckpoint phase) =>
        (provider.CreateContext(), issuer.GetCheckpointToken(phase.Key));

    public static ILatencyContext Mark(ILatencyContext context, CheckpointToken phase) =>
        (context.AddCheckpoint(phase), context).Item2;

    public static IO<Unit> Seal(ILatencyDataExporter exporter, ILatencyContext context) =>
        IO.liftAsync(async envIO => {
            context.Freeze();
            await exporter.ExportAsync(context.LatencyData, envIO.Token).ConfigureAwait(false);
            return unit;
        });
}
```

## [07]-[REDACTION_TAXONOMY]

- Owner: `DataClassification` `[SmartEnum<string>]` taxonomy with the `RedactorKind` keyless vocabulary as its redactor column, `Marker` its projection onto the compliance key and `Resolve` the federation inverse reading a `(taxonomy, value)` pair back to an owned row; `ClassificationRoster` one branch's declared federation values; `RedactionRegistration` the binding fold, the subset-lattice closure, and the boot federation proof; `RedactedText` the allocation-free egress read boundary.
- Cases: classification rows partition by DISCLOSURE CONSEQUENCE, never by sensitivity order — the roster's declaration order is the framework pair first (`None` the reviewed-public seal, `Unknown` the never-reviewed one) and the reviewed tiers between them, while treatment is the `RedactorKind` column alone, so a reader takes the verdict off the row and never off its position; `None`/`Operational`/`Internal` pass, `HostIdentity`/`HostPath`/`Personal`/`Confidential` pseudonymize, and `UserContent`/`Credential`/`Secret`/`Unknown` erase; four redactor kinds — none, hmac, erase, and the never-reviewed arm that erases under its own row — each carrying the `Rank` that decides a composite set's treatment, and four `DisclosureGrade` rungs every classification names as the ONE narrowing authority a durable row, an outbox envelope, and a retention tier all read; two federation refusals close the boot proof — a pair naming a taxonomy this suite does not own, and a value the roster never declared.
- Entry: `RedactionRegistration.Bind(ILoggingBuilder logging, IConfigurationSection hmacKeys)` returning the redaction-enabled builder, reached from `[05]`'s `GovernLogs` as the ONE redaction boundary; `RedactionRegistration.Federated(params ReadOnlySpan<ClassificationRoster> contributed)` returns `Validation<Error, Unit>` — the boot proof that every branch-declared `(taxonomy, value)` pair resolves an owned row, accumulating so one boot names every undeclared spelling; the `AddRedaction` fold maps EVERY producible classification set — each row's own singleton and every composite the taxonomy admits — onto its redactor, and `EnableRedaction` seals the boundary; `RedactedText.Into(Redactor redactor, Span<char> sink, string value)` returns `Fin<int>` — the sized non-throwing write; `RedactedText.Appended(StringBuilder into, Redactor redactor, ReadOnlySpan<char> value)` returns `StringBuilder` — the multi-part compose; `RedactedText.Mask(Redactor redactor, string value)` returns the kernel `Masked` case every redaction-tallying column reads through.
- Law: masked values report their own verdict as a CASE — `Rasm/Domain/validation#VERDICT_CARRIERS` `Masked {Unchanged, Redacted}` is that carrier, and the `(string, bool)` tuple this member used to return is the deleted form — a length compare answers the question wrongly for every length-preserving redactor, an HMAC token and a fixed-width fill both replacing a value byte-for-byte at its own width, so a fully masked bundle reported zero. The case makes the question unaskable at the call site because the transform authored the verdict where it ran.
- Auto: redactor resolution keys on WHOLE-SET equality, so a set composed through `Union` resolves only against a row registered for that same composite and NEVER against its member classifications — which makes every producible union a required row and a missing one a silent erasure with a compliance-shaped cause, so the fold DERIVES the composite rows rather than enumerating them: every non-empty subset of `Items` builds its set from its members' markers and takes the STRONGEST member treatment by `RedactorKind.Rank`, so a member annotated both `Personal` and `Confidential` hmacs, one annotated `Internal` and `Credential` erases, a member carrying three annotations resolves its own row rather than the fallback, and a new taxonomy row extends the whole lattice with no fold edit; the proof runs the other direction at boot — `Federated` resolves every branch-declared `(taxonomy, value)` pair against `Items` and refuses composition naming every offending package and spelling at once, so a sibling's unrostered value cannot reach egress and erase there; a `RedactorKind.None` row binds `NullRedactor` EXPLICITLY rather than falling through, because the fail-closed fallback is erasure and an unbound `Operational` tag therefore erases every operational dimension the dashboards join on — an availability bug the fallback's own correctness manufactures; `HmacRedactorOptions.KeyId` is the declared rotation epoch stamped ahead of every pseudonym, so values hashed under different generations never correlate and a rotation cuts correlation exactly at the rotation rather than silently re-tokenizing history — the same epoch the `Agent/reasoning#MODEL_GOVERNANCE` cache key folds in, so a rotation cannot replay a body redacted under the retired generation; `NoDataClassificationAttribute` seals a member as reviewed-public and `UnknownDataClassificationAttribute` seals a never-reviewed one, so the difference between "reviewed and cleared" and "nobody looked" is a declaration rather than an absence, and the `Unknown` row's erase treatment makes the never-reviewed arm fail closed by DECLARATION where the fallback made it fail closed by accident; classification flows through `[LogProperties]` and `[TagProvider]` generated methods as `LoggerMessageState.ClassifiedTag`; `EnableRedaction` applies the bound redactor before any sink or exporter observes the tag, and `AppHostMeasure.RedactionTags` counts the values THIS branch's own egress boundary masked, partitioned on the classification each carried — the generated tag path redacts inside the provider and publishes no count, so a roster-wide claim there is a measurement no surface takes; a sized egress read goes through `Redactor.TryRedact` into a caller span and a multi-part compose through `RedactionStringBuilderExtensions.AppendRedacted`, so a bounded detail string is redacted once into its own buffer rather than materialized, cut, and discarded.
- Packages: Rasm (kernel `Masked`), Microsoft.Extensions.Compliance.Redaction, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Law: disclosure GRADE has one owner, the `Grade` column on the classification row. Deriving a grade from the redactor column beside an is-none probe is the deleted form — treatment answers what happens at egress and grade answers where a value rests, so two consumers narrowing off one column disagreed the moment a row's treatment moved for a reason its grade did not share.
- Growth: one classification row extends the whole subset lattice through the same derivation and doubles its registration count, which is the declared cost a disclosure-consequence partition is sized against; one redactor kind is one case with its rank the treatment fold already groups, and one disclosure rung is one `DisclosureGrade` row every classification names; one federating branch is one `Classifications` roster on its own contributor port, which the composition projects into the `ClassificationRoster` set the one federation proof reads; one key generation is one `KeyId` value; zero new surface.
- Boundary: an unredacted classified value reaching any exporter is a boundary violation; classification attributes annotate shapes at definition time as `DataClassificationAttribute` subclasses through the transitively arriving compliance-abstractions surface, with `NoDataClassificationAttribute` and `UnknownDataClassificationAttribute` the two shipped seals that make a reviewed-public member and a never-reviewed member declare their status instead of sharing the absence of an annotation; redactor binding rides `AddRedaction(Action<IRedactionBuilder>)` over the DERIVED closure — every non-empty subset of the roster, grouped into one `SetRedactor<NullRedactor>` call on the pass sets, one `SetHmacRedactor(IConfigurationSection, params DataClassificationSet[])` on the pseudonym sets, and one `SetRedactor<ErasingRedactor>(params DataClassificationSet[])` on the erase sets — closing on `SetFallbackRedactor<ErasingRedactor>()`, which stays the fail-closed default for a set outside the closure rather than the working path for sets inside it, and the fold registers with no suppression; that bind is reached from `[05]`'s `GovernLogs` and from nowhere else, because a chain sealing the boundary without it resolves EVERY set through the fallback and erases the pass rows this fold exists to spare — the failure reads as a working redaction plane and surfaces only when a dashboard's operational dimension is missed; hmac rows pseudonymize while preserving cross-event correlation, erase rows destroy the value, and credential and secret material never persists in any signal; the log boundary governs the log path while the HTTP route-parameter path is a prevention row at the instrumentation root — `RequestMetadata` declares route-template parameters and `HttpRouteParameterRedactionMode` erases them so an outgoing-request span never carries an unredacted route segment, crossing to Persistence as VALUE fields on the landed rows (`Element/codec` `SnapshotCatalogRow.Classification`, `Element/identity`) — never a guard symbol and never a second registration; one redaction policy serves logs, traces, support capture, and the route-parameter prevention row, deleting call-site string scrubbing; metric tags ride the `[05]` view boundary instead — the one `AddView` predicate projects each rostered stream onto its declaring row's `Dimensions` beside the tenancy key alone, so a contributor evidence-string tag reaches an exporter only by being DECLARED on the row that mints it, where this taxonomy grades it; a foreign instrumentation stream keeps its convention-owned tags because no row of this branch declares them and none carries branch evidence.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class RedactorKind {
    public static readonly RedactorKind None = new(rank: 0);
    public static readonly RedactorKind Hmac = new(rank: 1);
    public static readonly RedactorKind Erase = new(rank: 2);
    public static readonly RedactorKind Unknown = new(rank: 3);

    public int Rank { get; }

    public static RedactorKind Strongest(RedactorKind left, RedactorKind right) => left.Rank >= right.Rank ? left : right;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DisclosureGrade {
    public static readonly DisclosureGrade Public = new("public", rank: 0);
    public static readonly DisclosureGrade Operational = new("operational", rank: 1);
    public static readonly DisclosureGrade Restricted = new("restricted", rank: 2);
    public static readonly DisclosureGrade Secret = new("secret", rank: 3);

    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public sealed partial class DataClassification {
    public static readonly DataClassification None = new("none", redactor: RedactorKind.None, grade: DisclosureGrade.Public);
    public static readonly DataClassification Operational = new("operational", redactor: RedactorKind.None, grade: DisclosureGrade.Operational);
    public static readonly DataClassification Internal = new("internal", redactor: RedactorKind.None, grade: DisclosureGrade.Operational);
    public static readonly DataClassification HostIdentity = new("host-identity", redactor: RedactorKind.Hmac, grade: DisclosureGrade.Restricted);
    public static readonly DataClassification HostPath = new("host-path", redactor: RedactorKind.Hmac, grade: DisclosureGrade.Restricted);
    public static readonly DataClassification UserContent = new("user-content", redactor: RedactorKind.Erase, grade: DisclosureGrade.Secret);
    public static readonly DataClassification Personal = new("personal", redactor: RedactorKind.Hmac, grade: DisclosureGrade.Restricted);
    public static readonly DataClassification Confidential = new("confidential", redactor: RedactorKind.Hmac, grade: DisclosureGrade.Restricted);
    public static readonly DataClassification Credential = new("credential", redactor: RedactorKind.Erase, grade: DisclosureGrade.Secret);
    public static readonly DataClassification Secret = new("secret", redactor: RedactorKind.Erase, grade: DisclosureGrade.Secret);
    public static readonly DataClassification Unknown = new("unknown", redactor: RedactorKind.Unknown, grade: DisclosureGrade.Secret);

    public RedactorKind Redactor { get; }

    public DisclosureGrade Grade { get; }

    public Microsoft.Extensions.Compliance.Classification.DataClassification Marker => new(nameof(DataClassification), Key);

    public static Fin<DataClassification> Resolve(string taxonomy, string value) =>
        string.Equals(taxonomy, nameof(DataClassification), StringComparison.Ordinal)
            ? Op.Of().AcceptValidated<DataClassification>(
                fault: Validate(value, null, out DataClassification? row), admitted: row)
            : Fin.Fail<DataClassification>(new TelemetryFault.Taxonomy($"{taxonomy}:{value}"));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ClassificationRoster(string Package, Seq<(string Taxonomy, string Value)> Values) {
    public static readonly ClassificationRoster Empty = new(string.Empty, Seq<(string, string)>());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RedactedText {
    public static Masked Mask(Redactor redactor, string value) =>
        redactor.Redact(value) switch {
            var masked when string.Equals(masked, value, StringComparison.Ordinal) => new Masked.Unchanged(masked),
            var masked => new Masked.Redacted(masked),
        };

    public static Fin<int> Into(Redactor redactor, Span<char> sink, string value) =>
        redactor.TryRedact(value, sink, out var written, format: default)
            ? Fin.Succ(written)
            : Fin.Fail<int>(new TelemetryFault.Sink(sink.Length));

    public static StringBuilder Appended(StringBuilder into, Redactor redactor, ReadOnlySpan<char> value) =>
        into.AppendRedacted(redactor, value);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class RedactionRegistration {
    public static ILoggingBuilder Bind(ILoggingBuilder logging, IConfigurationSection hmacKeys) {
        logging.Services.AddRedaction(redaction => ignore(
            toSeq(Closure().GroupBy(static row => row.Kind))
                .Fold(redaction, (builder, treatment) =>
                    treatment.Key.Switch(
                        state: (Builder: builder, Keys: hmacKeys, Sets: treatment.Select(static row => row.Set).ToArray()),
                        none: static bound => bound.Builder.SetRedactor<NullRedactor>(bound.Sets),
                        hmac: static bound => bound.Builder.SetHmacRedactor(bound.Keys, bound.Sets),
                        erase: static bound => bound.Builder.SetRedactor<ErasingRedactor>(bound.Sets),
                        unknown: static bound => bound.Builder.SetRedactor<ErasingRedactor>(bound.Sets)))
                .SetFallbackRedactor<ErasingRedactor>()));
        return logging.EnableRedaction(static options => options.ApplyDiscriminator = true);
    }

    public static Validation<Error, Unit> Federated(params ReadOnlySpan<ClassificationRoster> contributed) =>
        Iterable<ClassificationRoster>.FromSpan(contributed).ToSeq()
            .Bind(static roster => roster.Values.Map(pair => (roster.Package, pair.Taxonomy, pair.Value)))
            .Traverse(static claim => DataClassification.Resolve(claim.Taxonomy, claim.Value)
                .MapFail(claim, static (claim, _) => (Error)new TelemetryFault.Unclassified(
                    claim.Package, $"{claim.Taxonomy}:{claim.Value}"))
                .ToValidation()).As()
            .Map(static _ => unit);

    static Seq<(DataClassificationSet Set, RedactorKind Kind)> Closure() =>
        DataClassification.Items.AsIterable().ToSeq() is var rows
            ? toSeq(Range(1, (1 << rows.Count) - 1)).Map(mask => Composed(rows, mask))
            : Seq<(DataClassificationSet, RedactorKind)>();

    static (DataClassificationSet Set, RedactorKind Kind) Composed(Seq<DataClassification> rows, int mask) =>
        Sealed(rows.Map(static (row, index) => (Row: row, Index: index))
            .Filter(pair => (mask & (1 << pair.Index)) != 0)
            .Map(static pair => pair.Row));

    static (DataClassificationSet Set, RedactorKind Kind) Sealed(Seq<DataClassification> members) =>
        (new DataClassificationSet(members.Map(static row => row.Marker)),
         members.Fold(RedactorKind.None, static (kind, row) => RedactorKind.Strongest(kind, row.Redactor)));
}
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
