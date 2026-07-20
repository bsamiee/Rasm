# [APPHOST_DOMAIN_INSTRUMENTS]

One table-driven roster projects the receipt fan into `System.Diagnostics.Metrics` instruments, so every operational metric is a projection of a typed receipt, never a parallel truth minted at a call site. Owned axes: the domain `InstrumentRow` roster with its bucket-advice policy, the receipt-to-instrument projection fold, the per-ALC provider-lifetime capsule for zero-host plugin processes, and the instrument observation rail — libraries emit through `Meter`, `ActivitySource`, and `ILogger` alone, and every OpenTelemetry SDK member on this page composes at a composition root.

Settled composition: `InstrumentRow`, `TelemetrySource`, and `TelemetryIdentity.Mint` arrive from Observability/telemetry#TELEMETRY_IDENTITY; `TelemetryContributorPort` and `ReceiptEnvelope` from Runtime/ports#PORT_RECORDS; the observe tap that feeds the projection from Observability/hooks#HOOK_REGISTRY; the trace-based exemplar row and the `AddView` cardinality caps from Observability/telemetry#SIGNAL_GOVERNANCE. Metric names are dotted `rasm.<domain>.<measure>`, units are UCUM (`s`, `By`, `1`, `{thing}`), never pre-baked `_total`/unit suffixes; instrumentation scope name is the emitting package id, version-stamped, identical across tracer, meter, and logger; Prometheus translation standardizes on `NoUTF8EscapingWithSuffixes` so dotted names survive byte-identical from every runtime.

## [01]-[INDEX]

- [01]-[INSTRUMENT_CATALOG]: Domain instrument roster, cost-vector derivation, GenAI rows, and bucket advice.
- [02]-[RECEIPT_PROJECTION]: One projection fold from the receipt fan onto the mounted instruments.
- [03]-[PROVIDER_LIFETIME]: Per-ALC `IMeterFactory` capsule with unload-ordered flush and dispose.
- [04]-[OBSERVATION_RAIL]: `MetricCollector<T>` assertion rail and the out-of-process live-read boundary.

## [02]-[INSTRUMENT_CATALOG]

- Owner: `HostInstruments` — the AppHost domain roster of `InstrumentRow` values; `Buckets` the explicit-bucket advice policy rows.
- Cases: hop attempt and duration rows off `HopReceipt`; outbox watermark-lag and oldest-undelivered-age level rows off the relay sweep; capability-roster, stale-binding, and degradation-level rows read off the `LevelCells` atoms; broker delivery-outcome counts off `DeliveryReceipt`; command-admission counts and per-unit spend rows off `CommandReceipt`, the spend family derived from the `CostUnit` vocabulary; lifecycle-transition counts off the `Phase` hook tap; benchmark duration and regression rows off `BenchmarkReceipt`; the two GenAI semconv rows `gen_ai.client.token.usage` and `gen_ai.client.operation.duration` stamped by the governed model loop.
- Entry: `HostInstruments.Telemetry(string version)` — the one `TelemetryContributorPort` carrying the merged spine and domain row set, mounted beside every sibling contributor port at `[02]-[RECEIPT_PROJECTION]`.
- Auto: the spend rows derive from `CostUnit.Items` through the `Ucum` policy map, so a new cost unit is one vocabulary row and the roster follows with zero edits here; every histogram row ships `InstrumentAdvice<double>` explicit-bucket boundaries at creation, the fallback a backend without exponential histograms reads — the default aggregation is base2 exponential per the `[03]` provider rows.
- Packages: Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: one domain metric is one `InstrumentRow` in this roster and one projection arm at `[02]-[RECEIPT_PROJECTION]`; a per-unit family derives from its owning vocabulary, never hand-enumerated rows.
- Boundary: every row binds through the `TelemetryIdentity.Mint` meter, so no instrument outlives its `IMeterFactory` owner; the GenAI rows keep the semconv spelling rather than the `rasm.*` namespace because the convention owns the name, and their `gen_ai.token.type` dimension carries the input/output split the reasoning loop stamps; level-shaped facts ride observable gauges reading the `LevelCells` atoms at collection cadence — a polled level through a synchronous gauge aliases; the `rasm.apphost.health.level` gauge reads the `Health` cell the `[02]-[RECEIPT_PROJECTION]` `Degradation` tap folds from `DegradationReading.State.Level.Rank`, and the `rasm.apphost.capability.roster` gauge reads the `Roster` cell `DescriptorSurface.Describe` folds at descriptor admission; tenant fan-out on every row rides the `*`-wildcard `AddView` series cap at Observability/telemetry#SIGNAL_GOVERNANCE.

```csharp signature
public static class Buckets {
    public static readonly ImmutableArray<double> HopSeconds = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10];
    public static readonly ImmutableArray<double> ModelSeconds = [0.1, 0.25, 0.5, 1, 2, 5, 10, 30, 60];
    public static readonly ImmutableArray<double> BenchSeconds = [0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1, 1, 10];
    public static readonly ImmutableArray<double> TokenCounts = [16, 64, 256, 1024, 4096, 16384, 65536];

    public static Histogram<double> Advised(Meter meter, string name, string unit, string text, ImmutableArray<double> bounds) =>
        meter.CreateHistogram<double>(name, unit, text, tags: null, advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = bounds });
}

public sealed record LevelCells(Atom<long> OutboxLag, Atom<double> OldestAge, Atom<long> Roster, Atom<long> StaleBindings, Atom<long> Health) {
    public static readonly LevelCells Live = new(Atom(0L), Atom(0d), Atom(0L), Atom(0L), Atom(0L));
}

public static class HostInstruments {
    static readonly FrozenDictionary<CostUnit, string> Ucum = new Dictionary<CostUnit, string> {
        [CostUnit.CpuMillis] = "ms",
        [CostUnit.WallMillis] = "ms",
        [CostUnit.BytesEgress] = "By",
        [CostUnit.ModelTokens] = "{token}",
        [CostUnit.Calls] = "{call}",
    }.ToFrozenDictionary();

    public static readonly Seq<InstrumentRow> Rows = Seq(
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.hop.attempts", "{attempt}", "outbound hop attempts by hop kind and outcome",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.hop.duration", "s", "outbound hop wall duration per attempt",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.HopSeconds)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.outbox.lag", "{op}", "ops between the durable head and the slowest consumer watermark",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => LevelCells.Live.OutboxLag.Value, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.outbox.oldest.age", "s", "age of the oldest undelivered outbox op",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => LevelCells.Live.OldestAge.Value, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.capability.roster", "{descriptor}", "capability descriptors admitted into the live registry",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => LevelCells.Live.Roster.Value, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.binding.stale", "{binding}", "external bindings in stale or faulted state",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => LevelCells.Live.StaleBindings.Value, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.delivery.outcomes", "{delivery}", "broker deliveries by channel and outcome",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.command.admissions", "{command}", "command dispatches by transaction disposition",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.lifecycle.transitions", "{transition}", "lifecycle phase commits by from, to, and trigger",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.health.level", "1", "derived degradation level rank, zero full through four suspended",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => LevelCells.Live.Health.Value, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.benchmark.duration", "s", "gated benchmark median wall duration per case",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.BenchSeconds)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.benchmark.regressions", "{run}", "benchmark gate verdicts past budget by suite, case, and verdict",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "gen_ai.client.token.usage", "{token}", "model tokens consumed per operation by token type",
            static (meter, name, unit, text) => meter.CreateHistogram<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "gen_ai.client.operation.duration", "s", "governed model operation duration",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.ModelSeconds)))
        + CostUnit.Items.AsIterable().Map(static unit => new InstrumentRow(
            TelemetrySource.AppHost, $"rasm.apphost.grant.spend.{unit.Key}", Ucum[unit], $"cost debited against the {unit.Key} ceiling",
            static (meter, name, unitCode, text) => meter.CreateCounter<long>(name, unitCode, text))).ToSeq();

    public static TelemetryContributorPort Telemetry(string version) =>
        new(TelemetrySource.AppHost, version, TelemetryIdentity.SpineRows + Rows);
}
```

## [03]-[RECEIPT_PROJECTION]

- Owner: `InstrumentFan` — the one receipt-to-instrument projection; `InstrumentSet` the mounted-instrument capsule.
- Entry: `InstrumentFan.Mount(IMeterFactory factory, CorrelationId root, Seq<FrozenDictionary<string, Action<InstrumentSet, JsonElement>>> contributed, params ReadOnlySpan<TelemetryContributorPort> contributors)` mints each contributor's meter through `TelemetryIdentity.Mint`, materializes its rows over it, and merges the contributed kind-arm tables beside the AppHost table onto `InstrumentSet.Arms` — the port is the one inward instrument seam, AppHost's own roster entering as the `HostInstruments.Telemetry` port beside every sibling contribution, and the Persistence `StoreInstruments.Arms` table entering as one contributed element; `InstrumentFan.Project(InstrumentSet set, ReceiptEnvelope envelope)` folds one envelope into instrument writes through the mounted arm table; `InstrumentFan.Tap(HookRail rail, InstrumentSet set)` mounts the fan's three hook subscriptions at composition.
- Auto: `Tap` subscribes the Observability/hooks#HOOK_REGISTRY points — the `Receipt` tap projects every envelope through the kind table, the `Phase` tap counts `rasm.apphost.lifecycle.transitions` per commit, and the `Degradation` tap folds `DegradationReading.State.Level.Rank` into the `Health` cell — so every projection rides the hook rail with zero call-site metering; counter and histogram writes ride the envelope payload fields, the sweep arm folds watermark evidence into the `LevelCells.Live` lag and age cells the observable gauges read, and `DescriptorSurface.Describe` folds the descriptor census into the `Roster` cell at admission, so the gauges read a current level, never a re-derived scan.
- Receipt: none — the fan is a projection of receipts; a metric minted beside the fan is a second truth.
- Packages: LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new projected receipt is one kind constant, one kind-keyed table row, and its instrument row at `[02]`; a sibling package's projected slot is one row in its contributed arm table; an unmapped kind projects nothing and stays receipt-only by declaration.
- Boundary: the kind constants are the canonical spellings — every emitting page passes its `InstrumentFan` constant to `ReceiptSinkPort.Send` (`HopKind` at Wire/outbound, `DeliveryKind` at the delivery fan, `SweepKind` at Wire/outbox, `CommandKind` at Agent/capability, `ModelKind` at Agent/reasoning, `BenchmarkKind` at Observability/benchmarks) — and the kind registry is per-package: the AppHost table owns AppHost kinds, a contributed table owns its own slot spellings (Persistence `StoreInstruments.Arms` over the `store.*` census), and sibling fans project their kinds without an envelope hop — AppUi `EvidenceFan` over its evidence union, Compute `ComputeInstrumentFan` over the typed union pre-envelope — so one envelope kind projects in exactly one arm, a duplicate kind faults at the `Mount` merge, and a `Send` kind outside every package fan is receipt-only by declaration; payload field reads stay inside the table arms — the one place wire names meet instrument writes — and a projection arm never re-validates the payload the typed owner already admitted; a wire `Duration` field crosses as its NodaTime roundtrip text and `Seconds` is the one arm-side decode.

```csharp signature
public sealed record InstrumentSet(FrozenDictionary<string, Instrument> ByName) {
    public FrozenDictionary<string, Action<InstrumentSet, JsonElement>> Arms { get; init; } = InstrumentFan.Table;

    // Statement seam: the params span cannot cross a lambda, so each write branches in place.
    public Unit Count(string name, long value, params ReadOnlySpan<KeyValuePair<string, object?>> tags) {
        if (ByName[name] is Counter<long> counter) counter.Add(value, tags);
        return unit;
    }

    public Unit Record(string name, double value, params ReadOnlySpan<KeyValuePair<string, object?>> tags) {
        if (ByName[name] is Histogram<double> histogram) histogram.Record(value, tags);
        return unit;
    }

    public Unit Record(string name, long value, params ReadOnlySpan<KeyValuePair<string, object?>> tags) {
        if (ByName[name] is Histogram<long> histogram) histogram.Record(value, tags);
        return unit;
    }
}

public static class InstrumentFan {
    public const string HopKind = "hop";
    public const string DeliveryKind = "delivery";
    public const string SweepKind = "outbox-sweep";
    public const string CommandKind = "command";
    public const string ModelKind = "model-usage";
    public const string BenchmarkKind = "benchmark";

    internal static readonly FrozenDictionary<string, Action<InstrumentSet, JsonElement>> Table =
        new Dictionary<string, Action<InstrumentSet, JsonElement>> {
            [HopKind] = static (set, payload) => {
                var tags = (KeyValuePair<string, object?>[])[new("hop", payload.GetProperty("hop").GetString()), new("outcome", payload.GetProperty("outcome").GetString())];
                ignore(set.Count("rasm.apphost.hop.attempts", payload.GetProperty("attempts").GetInt64(), tags));
                ignore(set.Record("rasm.apphost.hop.duration", payload.GetProperty("elapsedSeconds").GetDouble(), tags));
            },
            [DeliveryKind] = static (set, payload) =>
                ignore(set.Count("rasm.apphost.delivery.outcomes", 1L,
                    new KeyValuePair<string, object?>("channel", payload.GetProperty("channel").GetString()),
                    new KeyValuePair<string, object?>("outcome", payload.GetProperty("outcome").GetString()))),
            // Level arms write the one live cell the observable gauges bind at declaration; a cells
            // parameter beside a Live-bound gauge is the split-truth knob this table refuses.
            [SweepKind] = static (_, payload) => {
                ignore(LevelCells.Live.OutboxLag.Swap(_ => payload.GetProperty("lag").GetInt64()));
                ignore(LevelCells.Live.OldestAge.Swap(_ => payload.GetProperty("oldestAgeSeconds").GetDouble()));
            },
            [CommandKind] = static (set, payload) => {
                ignore(set.Count("rasm.apphost.command.admissions", 1L,
                    new KeyValuePair<string, object?>("txn", payload.GetProperty("txn").GetProperty("kind").GetString())));
                foreach (var slot in payload.GetProperty("charged").EnumerateObject()) {
                    ignore(set.Count($"rasm.apphost.grant.spend.{slot.Name}", slot.Value.GetInt64()));
                }
            },
            [ModelKind] = static (set, payload) => {
                ignore(set.Record("gen_ai.client.token.usage", payload.GetProperty("inputTokens").GetInt64(), new KeyValuePair<string, object?>("gen_ai.token.type", "input")));
                ignore(set.Record("gen_ai.client.token.usage", payload.GetProperty("outputTokens").GetInt64(), new KeyValuePair<string, object?>("gen_ai.token.type", "output")));
                ignore(set.Record("gen_ai.client.operation.duration", Seconds(payload.GetProperty("elapsed"))));
            },
            [BenchmarkKind] = static (set, payload) => {
                var tags = (KeyValuePair<string, object?>[])[new("suite", payload.GetProperty("suite").GetString()), new("case", payload.GetProperty("case").GetString())];
                ignore(set.Record("rasm.apphost.benchmark.duration", Seconds(payload.GetProperty("median")), tags));
                if (payload.GetProperty("verdict").GetString() is { } verdict && verdict != BenchmarkVerdict.Pass.Key)
                    ignore(set.Count("rasm.apphost.benchmark.regressions", 1L, [.. tags, new("verdict", verdict)]));
            },
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // Duplicate kinds across the AppHost table and contributed tables throw at the frozen merge, so the one-fan-per-kind partition is composition-fatal.
    public static InstrumentSet Mount(IMeterFactory factory, CorrelationId root, Seq<FrozenDictionary<string, Action<InstrumentSet, JsonElement>>> contributed, params ReadOnlySpan<TelemetryContributorPort> contributors) =>
        new(Iterable<TelemetryContributorPort>.FromSpan(contributors)
            .Map(port => (Port: port, TelemetryIdentity.Mint(factory, port.Source, port.Version, root).Meter))
            .Bind(bound => bound.Port.Instruments
                .Map(row => KeyValuePair.Create(row.Name, row.Bind(bound.Meter, row.Name, row.Unit, row.Description)))
                .AsIterable())
            .ToFrozenDictionary(StringComparer.Ordinal)) {
            Arms = contributed.Fold(Table.AsEnumerable(), static (merged, table) => merged.Concat(table)).ToFrozenDictionary(StringComparer.Ordinal),
        };

    public static Unit Project(InstrumentSet set, ReceiptEnvelope envelope) =>
        set.Arms.TryGetValue(envelope.Kind, out var arm) ? fun(() => arm(set, envelope.Payload))() : unit;

    public static Seq<IDisposable> Tap(HookRail rail, InstrumentSet set) => Seq(
        rail.Receipt.Observe(envelope => IO.lift(() => Project(set, envelope))),
        rail.Phase.Observe(receipt => IO.lift(() => set.Count("rasm.apphost.lifecycle.transitions", 1L,
            new KeyValuePair<string, object?>("from", receipt.From.Key),
            new KeyValuePair<string, object?>("to", receipt.To.Key),
            new KeyValuePair<string, object?>("trigger", receipt.Trigger)))),
        rail.Degradation.Observe(reading => IO.lift(() => ignore(LevelCells.Live.Health.Swap(_ => (long)reading.State.Level.Rank)))));

    // NodaTime Duration crosses the wire as JsonRoundtrip text; the one arm-side decode to seconds.
    static double Seconds(JsonElement element) =>
        DurationPattern.JsonRoundtrip.Parse(element.GetString()!).Value.TotalSeconds;
}
```

## [04]-[PROVIDER_LIFETIME]

- Owner: `PluginTelemetryHost` — the zero-host telemetry capsule for Rhino/GH plugin processes; one instance per plugin `AssemblyLoadContext`.
- Entry: `PluginTelemetryHost.Open(AssemblyLoadContext alc, HostProfile profile, Action<ResourceBuilder> identity)` — builds the minimal per-ALC service provider, the explicit tracer and meter providers, and arms the unload hook.
- Auto: `new ServiceCollection().AddMetrics()` mints the per-ALC `IMeterFactory`, so two co-resident plugins minting a `Rasm.Compute` meter in one `Rhino.exe` stay isolated by provider scope; `AssemblyLoadContext.Unloading` drives `ForceFlush` then `Dispose` on both providers, then disposes the mini provider, so a plugin unload never drops the tail of an export batch; the exemplar filter pins `ExemplarFilterType.TraceBased` so any measurement recorded inside an active span carries its trace and span id with zero wiring.
- Packages: OpenTelemetry, OpenTelemetry.Exporter.OpenTelemetryProtocol, Microsoft.Extensions.DependencyInjection, LanguageExt.Core, BCL inbox.
- Growth: a new plugin-visible meter or source is one `AddMeter`/`AddSource` row in `Open`; a new resource dimension is one `identity` line at the caller; `FlushBound` is the one unload-flush policy value.
- Boundary: the provider — never the `Meter` or `ActivitySource` — is the disposable owner, and the capsule is the enforcing structure behind the process-static-meter prohibition: every meter reaches the process through a factory whose lifetime is the ALC; service-modality processes take the host-owned `SignalGovernance.Govern` path instead, so exactly one provider owner exists per process shape; OTLP egress is HTTP+protobuf with endpoint, headers, and protocol bound from `OTEL_EXPORTER_OTLP_*`, and the histogram default aggregation rides `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION=base2_exponential_bucket_histogram` with the `[02]` bucket advice as the explicit-bucket fallback.

```csharp signature
public sealed class PluginTelemetryHost : IDisposable {
    static readonly TimeSpan FlushBound = TimeSpan.FromSeconds(5);

    readonly ServiceProvider services;
    readonly TracerProvider tracing;
    readonly MeterProvider metrics;

    PluginTelemetryHost(ServiceProvider services, TracerProvider tracing, MeterProvider metrics) =>
        (this.services, this.tracing, this.metrics) = (services, tracing, metrics);

    public IMeterFactory Meters => services.GetRequiredService<IMeterFactory>();

    public static PluginTelemetryHost Open(AssemblyLoadContext alc, HostProfile profile, Action<ResourceBuilder> identity) {
        var services = new ServiceCollection().AddMetrics().BuildServiceProvider();
        var tracing = Sdk.CreateTracerProviderBuilder()
            .ConfigureResource(identity)
            .AddSource([.. TelemetrySource.Items.Where(static row => row.Minted).Select(static row => row.Key)])
            .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(TelemetrySignal.Trace.Ratio(profile))))
            .AddOtlpExporter()
            .Build();
        var metrics = Sdk.CreateMeterProviderBuilder()
            .ConfigureResource(identity)
            .AddMeter([.. TelemetrySource.Items.Select(static row => row.Key)])
            .SetExemplarFilter(ExemplarFilterType.TraceBased)
            .AddOtlpExporter()
            .Build();
        var host = new PluginTelemetryHost(services, tracing, metrics);
        alc.Unloading += _ => host.Dispose();
        return host;
    }

    public void Dispose() {
        ignore(tracing.ForceFlush((int)FlushBound.TotalMilliseconds));
        ignore(metrics.ForceFlush((int)FlushBound.TotalMilliseconds));
        tracing.Dispose();
        metrics.Dispose();
        services.Dispose();
    }
}
```

## [05]-[OBSERVATION_RAIL]

- Owner: `MetricCollector<T>` — the assertion tap composed directly at test sites; the package surface is the rail, and construction or snapshot forwarding around it is the rename adapter the prohibitions delete.
- Entry: `new MetricCollector<T>(factory, TelemetrySource.AppHost.Key, instrument)` — one collector per asserted instrument, scoped to the test's own `IMeterFactory`; `GetMeasurementSnapshot()` yields the indexable measurement list assertions fold over.
- Auto: a factory-scoped collector isolates parallel tests observing one meter name.
- Packages: Microsoft.Extensions.Diagnostics.Testing, BCL inbox.
- Growth: one collector per asserted instrument; a multi-instrument assertion is one collector row per instrument, never a shared listener.
- Boundary: a null-factory collector binds the process-global meter and its test runs non-parallel by declaration; `dotnet-counters` attaches by PID and live-reads every `rasm.*` meter with no exporter — a free out-of-process debugging surface over the identical instruments, a tool boundary and never a code dependency; deep EventPipe capture stays the Observability/bundles support-capture lane.

## [06]-[RESEARCH]

- [GENAI_METRIC_STABILITY]-[OPEN]: `gen_ai.client.token.usage`, `gen_ai.client.operation.duration`, and the `gen_ai.token.type` dimension against the pinned semconv registry — stability level and spelling drift; verify against the semconv gen-ai metrics page before the reasoning loop stamps them.
