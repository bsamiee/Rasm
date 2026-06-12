# [APPHOST_DIAGNOSTICS_AND_TELEMETRY]

Telemetry identity, the correlation spine, log projection, signal governance, and the suite data-classification taxonomy form one diagnostics concern owned by Rasm.AppHost. Owned axes: the `TelemetrySource` vocabulary with its instrument registry, the `TelemetrySignal` governance rows, the `LogPipeline` arbitration column, and the seven-row `DataClassification` taxonomy enforced at every exporter seam. The spine composes Microsoft.Extensions telemetry policy over the OpenTelemetry SDK, `LoggerMessage` source generation at lib level, and Serilog projection at desktop composition roots; siblings emit through minted identities and never construct telemetry owners of their own.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                |
| :-----: | :----------------- | :-------------------------------------------------------------------- |
|   [1]   | TELEMETRY_IDENTITY | Minted source and meter identity plus the instrument registry         |
|   [2]   | CORRELATION_SPINE  | One boot-minted root id stamped across every signal and hop           |
|   [3]   | LOG_PROJECTION     | Generated lib-level delegates and per-profile pipeline-owner arbitration |
|   [4]   | SIGNAL_GOVERNANCE  | Per-signal sampling, buffering, enrichment, exporter placement, drain flush |
|   [5]   | REDACTION_TAXONOMY | Seven classification rows binding redactor policy at every exporter seam |

## [2]-[TELEMETRY_IDENTITY]

- Owner: `TelemetrySource` `[SmartEnum<string>]` under the `SignalKeyPolicy` ordinal accessor; `InstrumentRow` registry inside `TelemetryIdentity`.
- Cases: 6 source rows — 4 minted package rows, 2 builtin meter rows; 2 spine instrument rows.
- Entry: `TelemetryIdentity.Mint(IMeterFactory factory, TelemetrySource source, string version, CorrelationId root)` returning the DI-owned `(ActivitySource, Meter)` pair.
- Auto: builtin rows feed GC, threadpool, exception-rate, and HttpClient duration streams through `AddMeter` with zero package; minted meters carry version and correlation tags at construction.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: one source is one vocabulary row and one instrument is one registry row; zero new surface.
- Boundary: a process-static `Meter` field outliving its provider is the named defect — minted pairs are `IMeterFactory`-owned and unload with the host ALC; the minted pair is the registration payload TelemetryContributorPort carries inward, deleting handler-local `ActivitySource` and `Meter` owners.

```csharp signature
public sealed class SignalKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SignalKeyPolicy, string>]
[KeyMemberComparer<SignalKeyPolicy, string>]
public sealed partial class TelemetrySource {
    public static readonly TelemetrySource AppHost = new("Rasm.AppHost", minted: true);
    public static readonly TelemetrySource Persistence = new("Rasm.Persistence", minted: true);
    public static readonly TelemetrySource Compute = new("Rasm.Compute", minted: true);
    public static readonly TelemetrySource AppUi = new("Rasm.AppUi", minted: true);
    public static readonly TelemetrySource SystemRuntime = new("System.Runtime", minted: false);
    public static readonly TelemetrySource SystemNetHttp = new("System.Net.Http", minted: false);

    public bool Minted { get; }
}

public sealed record InstrumentRow(
    TelemetrySource Source,
    string Name,
    string Unit,
    string Description,
    Func<Meter, string, string, string, Instrument> Kind);

public static class TelemetryIdentity {
    public static readonly FrozenDictionary<string, InstrumentRow> Instruments =
        new[] {
            new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.logs.flushed", "{flush}", "fault-window buffer flushes",
                static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
            new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.redaction.tags", "{tag}", "classified tags redacted before export",
                static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        }.ToFrozenDictionary(static row => row.Name, StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, InstrumentRow>.AlternateLookup<ReadOnlySpan<char>> Probe =
        Instruments.GetAlternateLookup<ReadOnlySpan<char>>();

    public static (ActivitySource Source, Meter Meter) Mint(IMeterFactory factory, TelemetrySource source, string version, CorrelationId root) =>
        (new ActivitySource(source.Key, version),
         factory.Create(new MeterOptions(source.Key) {
             Version = version,
             Tags = [new(nameof(CorrelationId), root.ToString())],
         }));

    public static Option<InstrumentRow> Find(ReadOnlySpan<char> name) =>
        Probe.TryGetValue(name, out var row) ? Optional(row) : None;

    extension(InstrumentRow row) {
        public Instrument Materialize(Meter meter) => row.Kind(meter, row.Name, row.Unit, row.Description);
    }
}
```

## [3]-[CORRELATION_SPINE]

- Owner: `Correlation` spine surface stamping the boot-minted `CorrelationId` across every signal and hop.
- Entry: `Correlation.Stamp(CorrelationId root)` returning the `IDisposable` ambient scope.
- Auto: one boot mint stamps `LogContext` properties, `Baggage`, meter tags, receipts, and support manifests — deletes per-call-site correlation parameters across the suite.
- Packages: OpenTelemetry, Serilog, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new stamped carrier is one stamp row inside `Stamp` plus one policy value; zero new surface.
- Boundary: the composite registers as `Propagators.DefaultTextMapPropagator` and crosses every hop through `TextMapPropagator.Inject` and `TextMapPropagator.Extract`, riding gRPC metadata on the local-ipc leg; the Serilog event trace-id and span-id fields bind to the live `Activity` ids, never to a parallel identifier; `RuntimeContext` slots carry the ambient value where async flow demands it.

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
}
```

## [4]-[LOG_PROJECTION]

- Owner: `LogPipeline` `[SmartEnum<string>]` arbitration column; `SpineLog` generated delegates; `SerilogProjectionPolicy` shaping surface.
- Cases: 2 pipeline rows — serilog-projection on rhino-plugin, gh2-plugin, standalone-desktop, and test-host; otel-export on companion, sidecar, headless-service, and web-service.
- Entry: `LogPipeline.Owner(HostProfile profile)` — the total arbitration projection.
- Auto: generated delegates carry stable `EventId` and `EventName`; `[LogProperties]` expands typed payloads into bounded tags with classification intact.
- Packages: Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Telemetry.Abstractions, Serilog, Thinktecture.Runtime.Extensions.
- Growth: one spine event is one generated-delegate row inside the 1000-1999 band; a new profile is one `Owner` arm; zero new surface.
- Boundary: lib level emits `ILogger` only — zero Serilog types below composition roots, deleting static `Log` facade calls; the `AddSerilog` host bridge and every sink are app-root pins; `CloseAndFlush` is a ranked drain participant; exactly one pipeline owner per profile row, never both on one signal; the test row installs `AddFakeLogging` and asserts through `FakeLogCollector` snapshots, never sink text.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SignalKeyPolicy, string>]
[KeyMemberComparer<SignalKeyPolicy, string>]
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

public static partial class SpineLog {
    [LoggerMessage(EventId = 1000, EventName = nameof(ReloadApplied), Level = LogLevel.Information, Message = "configuration reload applied")]
    public static partial void ReloadApplied(ILogger logger, [LogProperties] ReloadReceipt receipt);

    [LoggerMessage(EventId = 1001, EventName = nameof(SignalDropped), Level = LogLevel.Warning, Message = "telemetry signal {Signal} dropped {Count} events", SkipEnabledCheck = true)]
    public static partial void SignalDropped(ILogger logger, string signal, long count);
}

public static class SerilogProjectionPolicy {
    public static readonly LoggingLevelSwitch Floor = new(LogEventLevel.Information);

    public static LoggerConfiguration Shape(LoggerConfiguration configuration) =>
        configuration
            .MinimumLevel.ControlledBy(Floor)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Destructure.ToMaximumDepth(4);
}
```

## [5]-[SIGNAL_GOVERNANCE]

- Owner: `TelemetrySignal` `[SmartEnum<string>]` governance rows and the `SignalGovernance` registration fold.
- Cases: 3 signal rows — log, trace, metric — each binding ratio, buffering, and redaction policy.
- Entry: `SignalGovernance.Govern(IServiceCollection services, HostProfile profile, Action<ResourceBuilder> identity)` returning the host-owned `OpenTelemetryBuilder`.
- Auto: provider `ForceFlush` and `Shutdown` ride the telemetry drain band; the fault transition lands the `GlobalLogBuffer.Flush` window inside support capture; `AuditTo` and `FallbackChain` carry receipt-grade events with `SelfLog` as the failure listener at desktop roots.
- Packages: OpenTelemetry.Extensions.Hosting, OpenTelemetry, OpenTelemetry.Instrumentation.Http, Microsoft.Extensions.Telemetry, OpenTelemetry.Exporter.OpenTelemetryProtocol, Microsoft.Extensions.Diagnostics.Testing.
- Growth: one governance decision is one policy value row; one stream reshaping is one `AddView` row over `MetricStreamConfiguration`; zero new surface.
- Boundary: the OTLP exporter package enters only at service app roots with endpoint from `OTEL_EXPORTER_OTLP_*`; the EF and gRPC instrumentation packages stay rejected — native `Activity` emission carries those spans; on net10 the builtin rows delete the meter-side `AddRuntimeInstrumentation` registration while the trace-side `AddHttpClientInstrumentation` row keeps URL-query redaction; test-row trace assertions ride one `BaseProcessor<Activity>` through `AddProcessor` and metric assertions ride `MetricCollector<T>` — no in-memory exporter package enters.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SignalKeyPolicy, string>]
[KeyMemberComparer<SignalKeyPolicy, string>]
public sealed partial class TelemetrySignal {
    public static readonly TelemetrySignal Log = new("log", buffered: true, redacted: true, SampleRatio);
    public static readonly TelemetrySignal Trace = new("trace", buffered: false, redacted: true, SampleRatio);
    public static readonly TelemetrySignal Metric = new("metric", buffered: false, redacted: false, static _ => 1d);

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

public static class SignalGovernance {
    public static OpenTelemetryBuilder Govern(IServiceCollection services, HostProfile profile, Action<ResourceBuilder> identity) =>
        LogPipeline.Owner(profile).Switch(
            state: services.AddOpenTelemetry()
                .ConfigureResource(identity)
                .WithTracing(static tracing => tracing
                    .AddSource([.. TelemetrySource.Items.Where(static row => row.Minted).Select(static row => row.Key)])
                    .AddHttpClientInstrumentation())
                .WithMetrics(static metrics => metrics
                    .AddMeter([.. TelemetrySource.Items.Select(static row => row.Key)])),
            serilogProjection: static builder => builder,
            otelExport: static builder => builder.WithLogging());

    public static ILoggingBuilder GovernLogs(ILoggingBuilder logging) =>
        logging.AddTraceBasedSampler().EnableRedaction().EnableEnrichment().AddGlobalBuffer(LogLevel.Warning);

    public static IServiceCollection EnrichContext(IServiceCollection services) =>
        services.AddApplicationLogEnricher().AddProcessLogEnricher().AddLatencyContext();
}
```

[GOVERNANCE_VALUES]:

| [INDEX] | [POLICY]                                        | [VALUE]                       | [BINDING]                                      |
| :-----: | :---------------------------------------------- | :---------------------------- | :--------------------------------------------- |
|   [1]   | trace admission ratio, serilog-projection rows  | 1.0                           | `Ratio` column, parent-based trace-id-ratio sampler |
|   [2]   | trace admission ratio, otel-export rows         | 0.1                           | `Ratio` column, parent-based trace-id-ratio sampler |
|   [3]   | log sampling                                    | trace-coupled                 | `AddTraceBasedSampler`                          |
|   [4]   | metric exemplar policy                          | trace-based                   | `SetExemplarFilter` at service app roots        |
|   [5]   | metric reader cadence                           | 60 s                          | `PeriodicExportingMetricReader` at service app roots |
|   [6]   | global buffer admission                         | Warning and below             | `AddGlobalBuffer`                               |
|   [7]   | buffer flush window                             | support-window deadline row   | `GlobalLogBuffer.Flush` on the fault transition |
|   [8]   | destructuring depth cap                         | 4                             | `Destructure.ToMaximumDepth`                    |
|   [9]   | desktop level floor                             | Information                   | `LoggingLevelSwitch` under `MinimumLevel.ControlledBy` |
|  [10]   | spine event-id band                             | 1000-1999                     | `LoggerMessage` `EventId` values                |

## [6]-[REDACTION_TAXONOMY]

- Owner: `DataClassification` `[SmartEnum<string>]` taxonomy with the `RedactorKind` keyless vocabulary as its redactor column.
- Cases: 7 classification rows in escalating sensitivity order; 3 redactor kinds — none, hmac, erase.
- Auto: classification flows through `[LogProperties]` and `[TagProvider]` generated methods as `LoggerMessageState.ClassifiedTag`; `EnableRedaction` applies the bound redactor before any sink or exporter observes the tag, and the `rasm.apphost.redaction.tags` count rises per redacted tag.
- Packages: Microsoft.Extensions.Compliance.Redaction, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions.
- Growth: one classification row plus one redactor binding; one redactor kind is one case; zero new surface.
- Boundary: an unredacted classified value reaching any exporter is a seam violation; classification attributes annotate shapes at definition time through the transitively arriving compliance-abstractions surface; hmac rows pseudonymize while preserving cross-event correlation, erase rows destroy the value, and credential plus secret material never persists in any signal; one redaction policy serves logs, traces, and support capture, deleting call-site string scrubbing.

```csharp signature
[SmartEnum]
public sealed partial class RedactorKind {
    public static readonly RedactorKind None = new();
    public static readonly RedactorKind Hmac = new();
    public static readonly RedactorKind Erase = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SignalKeyPolicy, string>]
[KeyMemberComparer<SignalKeyPolicy, string>]
public sealed partial class DataClassification {
    public static readonly DataClassification None = new("none", redactor: RedactorKind.None);
    public static readonly DataClassification Operational = new("operational", redactor: RedactorKind.None);
    public static readonly DataClassification HostIdentity = new("host-identity", redactor: RedactorKind.Hmac);
    public static readonly DataClassification UserContent = new("user-content", redactor: RedactorKind.Erase);
    public static readonly DataClassification Personal = new("personal", redactor: RedactorKind.Hmac);
    public static readonly DataClassification Credential = new("credential", redactor: RedactorKind.Erase);
    public static readonly DataClassification Secret = new("secret", redactor: RedactorKind.Erase);

    public RedactorKind Redactor { get; }
}
```

## [7]-[RESEARCH]

| [INDEX] | [ITEM]                                                                                                   | [PROOF]                                                                                            | [GATE]             |
| :-----: | :-------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------- | :----------------- |
|   [1]   | `IMeterFactory` registration route, `MeterOptions` knob set, and `Instrument` mint overloads on net10      | `uv run python -m tools.assay api query microsoft.extensions.diagnostics.abstractions IMeterFactory` | TELEMETRY_IDENTITY |
|   [2]   | `AddSerilog` host-bridge registration shape for serilog-projection app roots                                | `uv run python -m tools.assay api resolve serilog.extensions.hosting`                                | LOG_PROJECTION     |
|   [3]   | Parent-based trace-id-ratio sampler and trace-based exemplar-filter argument spellings                      | `uv run python -m tools.assay api query opentelemetry TracerProviderBuilder`                         | SIGNAL_GOVERNANCE  |
|   [4]   | `AddGlobalBuffer`, `EnableRedaction`, and `EnableEnrichment` configurator overloads plus `LogBufferingFilterRule` fields | `uv run python -m tools.assay api query microsoft.extensions.telemetry AddGlobalBuffer`              | SIGNAL_GOVERNANCE  |
|   [5]   | Erasing and HMAC redactor provider registration spellings plus the classification attribute bridge          | `uv run python -m tools.assay api resolve compliance.redaction`                                      | REDACTION_TAXONOMY |
|   [6]   | EF Core 10 and grpc-dotnet native `Activity` emission depth replacing the rejected beta instrumentation packages | `uv run python -m tools.assay test run --target Rasm.AppHost` with one `BaseProcessor<Activity>` capture spec | SIGNAL_GOVERNANCE  |
