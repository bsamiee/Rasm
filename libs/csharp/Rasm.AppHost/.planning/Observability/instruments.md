# [APPHOST_DOMAIN_INSTRUMENTS]

One table-driven roster projects the receipt fan into `System.Diagnostics.Metrics` instruments, so every operational metric is a projection of a typed receipt, never a parallel truth minted at a call site. Owned axes: the AppHost `InstrumentRow` roster, the envelope-to-instrument projection fold over the kernel `ReceiptFan`, the contributed-port merge, the per-ALC provider-lifetime capsule for zero-host plugin processes, and the instrument observation rail — libraries emit through `Meter`, `ActivitySource`, and `ILogger` alone, and every OpenTelemetry SDK member on this page composes at a composition root.

Settled composition: `InstrumentRow`, `InstrumentSet`, `Buckets`, `LevelCells`, `InstrumentArm`/`ReceiptFan`, `TelemetryContributorPort`, and `TelemetryIdentity.Mint` arrive from the kernel signal capsule — this page composes instances and laces app identity, never re-declares the mechanism; `TelemetrySource` from Observability/telemetry#TELEMETRY_IDENTITY; `ReceiptEnvelope` from Runtime/ports#PORT_RECORDS; the observe tap that feeds the projection from Observability/hooks#HOOK_RAIL; the trace-based exemplar row and the `AddView` cardinality caps from Observability/telemetry#SIGNAL_GOVERNANCE. Metric names are dotted `rasm.<domain>.<measure>`, units are UCUM (`s`, `By`, `1`, `{thing}`), never pre-baked `_total`/unit suffixes; instrumentation scope name is the emitting package id, version-stamped and schema-pinned through the contributor port's `SchemaUrl` coordinate, identical across tracer, meter, and logger; Prometheus translation standardizes on `NoUTF8EscapingWithSuffixes` so dotted names survive byte-identical from every runtime.

## [01]-[INDEX]

- [02]-[INSTRUMENT_CATALOG]: AppHost instrument roster, cost-vector derivation, GenAI rows, level cells, and the contributor port.
- [03]-[RECEIPT_PROJECTION]: One projection fold from the receipt fan onto the mounted instruments, and the contributed-port merge.
- [04]-[PROVIDER_LIFETIME]: Per-ALC `IMeterFactory` capsule with unload-ordered flush and dispose.
- [05]-[OBSERVATION_RAIL]: `MetricCollector<T>` assertion rail and the out-of-process live-read boundary.

## [02]-[INSTRUMENT_CATALOG]

- Owner: `HostInstruments` — the ONE AppHost roster of `InstrumentRow` values: the spine rows (log flush, redaction, drain) and the domain rows in one declaration, with the contributor-port mint beside them.
- Cases: hop attempt and duration rows off `HopReceipt`; stale-binding and degradation-level rows read the composition's `LevelCells` scalars, the capability-roster row reading its keyed descriptor-admission family; broker delivery-outcome counts off `DeliveryReceipt`; command-admission counts and per-unit spend rows off `CommandReceipt`, the spend family derived from the `CostUnit` vocabulary; lifecycle-transition counts off the `Phase` hook tap; benchmark duration and regression rows off `BenchmarkReceipt`; live-wire rejection counts off the inbound coercion seam and write-back disposition counts off `WriteReceipt`; fleet-wave counts off `RollAnnotationWire`; machine-observation counts off the decode lane; the two GenAI semconv rows `gen_ai.client.token.usage` and `gen_ai.client.operation.duration` stamped by the governed model loop.
- Entry: `HostInstruments.Rows(LevelCells cells)` — the roster, gauge rows closing over the composition's cell readers; `HostInstruments.Telemetry(LevelCells cells, string version, string schemaUrl)` — the one `TelemetryContributorPort` carrying the row set with its semconv schema coordinate, mounted beside every sibling contributor port at `[03]-[RECEIPT_PROJECTION]`.
- Auto: the spend rows derive from `CostUnit.Items` through the `Ucum` policy map, and `UnitOf` turns an incomplete policy map into a cost-unit-named composition diagnostic instead of an unclassified dictionary miss; every histogram row ships `InstrumentAdvice<T>` explicit-bucket boundaries at creation through the kernel `Buckets` rows — the fallback a backend without exponential histograms reads — the default aggregation is base2 exponential per the `[04]` provider rows; a keyed level family binds through the cell's tagged-measurement `Reader`, so per-key cardinality rides one instrument and a per-key instrument mint is the deleted form.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: one domain metric is one `InstrumentRow` in this roster and one projection arm at `[03]-[RECEIPT_PROJECTION]`; a per-unit family derives from its owning vocabulary, never hand-enumerated rows; a level is one `cells.Level` write at its producing arm and one `Reader`-bound gauge row.
- Boundary: every row binds through the composition's minted meter, so no instrument outlives its `IMeterFactory` owner; the `LevelCells` instance is composition-owned and threaded — the degradation tap, the livewire binding fold, and the Agent/capability descriptor admission write the levels the gauges read, so a level is current at every collection and no process-static cell exists; the GenAI rows keep the semconv spelling rather than the `rasm.*` namespace because the convention owns the name, and their `gen_ai.token.type` dimension carries the input/output split the reasoning loop stamps; tenant fan-out on every row rides the `*`-wildcard `AddView` series cap at Observability/telemetry#SIGNAL_GOVERNANCE.

```csharp signature
public static class HostInstruments {
    static readonly FrozenDictionary<CostUnit, string> Ucum = new Dictionary<CostUnit, string> {
        [CostUnit.CpuMillis] = "ms",
        [CostUnit.WallMillis] = "ms",
        [CostUnit.BytesEgress] = "By",
        [CostUnit.ModelTokens] = "{token}",
        [CostUnit.Calls] = "{call}",
    }.ToFrozenDictionary();

    public static Seq<InstrumentRow> Rows(LevelCells cells) => Seq(
        new InstrumentRow("rasm.apphost.logs.flushed", "{record}", "buffered log records replayed by the incident flush",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.redaction.tags", "{tag}", "classified tags redacted before any exporter observed them",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.drain.duration", "s", "drain-band fold duration per band",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow("rasm.apphost.hop.attempts", "{attempt}", "outbound hop attempts by hop kind and outcome",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.hop.duration", "s", "outbound hop wall duration per attempt",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.HopSeconds)),
        new InstrumentRow("rasm.apphost.binding.stale", "{binding}", "external bindings in stale or faulted state",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, cells.Reader(name), unit, text)),
        new InstrumentRow("rasm.apphost.delivery.outcomes", "{delivery}", "broker deliveries by channel and outcome",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.command.admissions", "{command}", "command dispatches by transaction disposition",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.capability.roster", "{descriptor}", "live capability descriptors by admitting surface",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, cells.Reader(name, "surface"), unit, text)),
        new InstrumentRow("rasm.apphost.lifecycle.transitions", "{transition}", "lifecycle phase commits by from, to, and trigger",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.health.level", "1", "derived degradation level rank, zero full through four suspended",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, cells.Reader(name), unit, text)),
        new InstrumentRow("rasm.apphost.benchmark.duration", "s", "gated benchmark median wall duration per case",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.BenchSeconds)),
        new InstrumentRow("rasm.apphost.benchmark.regressions", "{run}", "benchmark gate verdicts past budget by suite, case, and verdict",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.wire.rejections", "{rejection}", "live-wire inbound values rejected at coercion or staleness",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.write.dispositions", "{write}", "write-back transactions by binding",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.fleet.waves", "{wave}", "fleet rollout waves by strategy, channel, and verdict",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.apphost.machine.observations", "{observation}", "decoded machine-telemetry observations by machine",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("gen_ai.client.token.usage", "{token}", "model tokens consumed per operation by token type",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.TokenCounts)),
        new InstrumentRow("gen_ai.client.operation.duration", "s", "governed model operation duration",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.ModelSeconds)))
        + CostUnit.Items.AsIterable().Map(static unit => new InstrumentRow(
            $"rasm.apphost.grant.spend.{unit.Key}", UnitOf(unit), $"cost debited against the {unit.Key} ceiling",
            static (meter, name, unitCode, text) => meter.CreateCounter<long>(name, unitCode, text))).ToSeq();

    static string UnitOf(CostUnit unit) => Ucum.TryGetValue(unit, out string? code)
        ? code
        : throw new InvalidOperationException($"Cost unit '{unit.Key}' has no UCUM policy row.");

    public static TelemetryContributorPort Telemetry(LevelCells cells, string version, string schemaUrl) =>
        new(TelemetrySource.AppHost.Key, version, schemaUrl, Rows(cells));
}
```

## [03]-[RECEIPT_PROJECTION]

- Owner: `InstrumentFan` — the AppHost kind constants, the AppHost arm table, and the composition entries over the kernel `ReceiptFan`.
- Entry: `InstrumentFan.Mount(IMeterFactory factory, CorrelationId root, LevelCells cells, Seq<FrozenDictionary<string, InstrumentArm>> contributed, params ReadOnlySpan<TelemetryContributorPort> contributors)` mints each contributor's meter through the kernel `TelemetryIdentity.Mint` — the port's plain `Scope` string names the meter, its `SchemaUrl` coordinate stamps `MeterOptions.TelemetrySchemaUrl`, and this composition laces the boot correlation as the one meter tag — materializes every port's rows, and merges the contributed kind-arm tables beside the AppHost table through `ReceiptFan.Of`, so contribution is downward and self-identifying: a contributor never names a platform type, and this root maps the scope strings into its `AddMeter`/`AddSource` admission; `InstrumentFan.Project(ReceiptFan fan, ReceiptEnvelope envelope)` folds one envelope into instrument writes; `InstrumentFan.Tap(HookRail rail, ReceiptFan fan)` mounts the fan's three hook subscriptions at composition.
- Auto: `Tap` subscribes the Observability/hooks#HOOK_RAIL points — the `Receipt` tap projects every registered envelope kind through the arm table, the `Phase` tap counts `rasm.apphost.lifecycle.transitions` per commit, and the `Degradation` tap folds `DegradationReading.State.Level.Rank` into the health level the `[02]` gauge reads — so every settled projection rides the hook rail with zero call-site metering.
- Receipt: none — the fan is a projection of receipts; a metric minted beside the fan is a second truth.
- Packages: Rasm, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new projected receipt is one kind constant, one kind-keyed table row, and its instrument row at `[02]`; a sibling package's projected slot is one row in its contributed arm table; an unmapped kind projects nothing and stays receipt-only by declaration.
- Boundary: the kind constants are the canonical spellings — every emitting page passes its `InstrumentFan` constant to `ReceiptSinkPort.Send` (`HopKind` at Wire/outbound, `DeliveryKind` at the delivery fan, `SweepKind` at Wire/outbox, `CommandKind` at Agent/capability, `ModelKind` at Agent/reasoning, `BenchmarkKind` at Observability/benchmarks, `WireKind` at the Wire/livewire inbound rejection seam, `WriteKind` at the Wire/livewire write-back mint, `RollKind` at the Sandbox/provisioning fleet-wave annotation, `ObservationKind` at the Wire/livewire machine decode lane) — and the kind registry is per-package: the AppHost table owns AppHost kinds, a contributed table owns its own slot spellings, and sibling fans project their kinds typed pre-envelope (AppUi over its evidence union, Compute over `ComputeReceipt`, Grasshopper over `GhEvidence`, Element over `ElementFact`) — so one envelope kind projects in exactly one arm, a duplicate kind faults at the `ReceiptFan.Of` frozen merge, and a `Send` kind outside every package fan is receipt-only by declaration; payload field reads stay inside the table arms — the one place wire names meet instrument writes — and a projection arm never re-validates the payload the typed owner already admitted; a wire `Duration` field crosses as its NodaTime roundtrip text and `Seconds` is the one arm-side decode.

```csharp signature
public static class InstrumentFan {
    public const string HopKind = "hop";
    public const string DeliveryKind = "delivery";
    public const string SweepKind = "outbox-sweep";
    public const string CommandKind = "command";
    public const string ModelKind = "model-usage";
    public const string BenchmarkKind = "benchmark";
    public const string WireKind = "wire";
    public const string WriteKind = "write-back";
    public const string RollKind = "fleet-roll";
    public const string ObservationKind = "machine-observation";

    internal static readonly FrozenDictionary<string, InstrumentArm> Table =
        new Dictionary<string, InstrumentArm> {
            [HopKind] = static (set, _, payload) => {
                var tags = (KeyValuePair<string, object?>[])[new("hop", payload.GetProperty("hop").GetString()), new("outcome", payload.GetProperty("outcome").GetString())];
                ignore(set.Count("rasm.apphost.hop.attempts", payload.GetProperty("attempts").GetInt64(), tags));
                ignore(set.Record("rasm.apphost.hop.duration", payload.GetProperty("elapsedSeconds").GetDouble(), tags));
            },
            [DeliveryKind] = static (set, _, payload) =>
                ignore(set.Count("rasm.apphost.delivery.outcomes", 1L,
                    new KeyValuePair<string, object?>("channel", payload.GetProperty("channel").GetString()),
                    new KeyValuePair<string, object?>("outcome", payload.GetProperty("outcome").GetString()))),
            [CommandKind] = static (set, _, payload) => {
                ignore(set.Count("rasm.apphost.command.admissions", 1L,
                    new KeyValuePair<string, object?>("txn", payload.GetProperty("txn").GetProperty("kind").GetString())));
                foreach (var slot in payload.GetProperty("charged").EnumerateObject()) {
                    ignore(set.Count($"rasm.apphost.grant.spend.{slot.Name}", slot.Value.GetInt64()));
                }
            },
            [ModelKind] = static (set, _, payload) => {
                ignore(set.Record("gen_ai.client.token.usage", payload.GetProperty("inputTokens").GetInt64(), new KeyValuePair<string, object?>("gen_ai.token.type", "input")));
                ignore(set.Record("gen_ai.client.token.usage", payload.GetProperty("outputTokens").GetInt64(), new KeyValuePair<string, object?>("gen_ai.token.type", "output")));
                ignore(set.Record("gen_ai.client.operation.duration", Seconds(payload.GetProperty("elapsed"))));
            },
            [BenchmarkKind] = static (set, _, payload) => {
                var tags = (KeyValuePair<string, object?>[])[new("suite", payload.GetProperty("suite").GetString()), new("case", payload.GetProperty("case").GetString())];
                ignore(set.Record("rasm.apphost.benchmark.duration", Seconds(payload.GetProperty("median")), tags));
                if (payload.GetProperty("verdict").GetString() is { } verdict && verdict != BenchmarkVerdict.Pass.Key)
                    ignore(set.Count("rasm.apphost.benchmark.regressions", 1L, [.. tags, new("verdict", verdict)]));
            },
            // Wire rejection payloads carry fault text alone; the write,
            // roll, and observation arms read their receipt fields through the same camelCase wire names.
            [WireKind] = static (set, _, _) =>
                ignore(set.Count("rasm.apphost.wire.rejections", 1L)),
            [WriteKind] = static (set, _, payload) =>
                ignore(set.Count("rasm.apphost.write.dispositions", 1L,
                    new KeyValuePair<string, object?>("binding", payload.GetProperty("bindingId").GetString()))),
            [RollKind] = static (set, _, payload) =>
                ignore(set.Count("rasm.apphost.fleet.waves", 1L,
                    new KeyValuePair<string, object?>("strategy", payload.GetProperty("strategy").GetString()),
                    new KeyValuePair<string, object?>("channel", payload.GetProperty("channel").GetString()),
                    new KeyValuePair<string, object?>("verdict", payload.GetProperty("verdict").GetString()))),
            [ObservationKind] = static (set, _, payload) =>
                ignore(set.Count("rasm.apphost.machine.observations", 1L,
                    new KeyValuePair<string, object?>("machine", payload.GetProperty("machine").GetString()))),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // Duplicate kinds across the AppHost table and contributed tables throw at the ReceiptFan frozen merge,
    // so the one-fan-per-kind partition is composition-fatal.
    public static ReceiptFan Mount(IMeterFactory factory, CorrelationId root, LevelCells cells,
        Seq<FrozenDictionary<string, InstrumentArm>> contributed, params ReadOnlySpan<TelemetryContributorPort> contributors) =>
        ReceiptFan.Of(
            set: new InstrumentSet(Iterable<TelemetryContributorPort>.FromSpan(contributors)
                .Map(port => (Port: port, TelemetryIdentity.Mint(factory, port.Scope, port.Version, port.SchemaUrl,
                    new KeyValuePair<string, object?>(nameof(CorrelationId), root.ToString())).Meter))
                .Bind(bound => bound.Port.Instruments
                    .Map(row => KeyValuePair.Create(row.Name, row.Bind(bound.Meter, row.Name, row.Unit, row.Description)))
                    .AsIterable())
                .ToFrozenDictionary(StringComparer.Ordinal)),
            cells: cells,
            tables: [Table, .. contributed]);

    public static Unit Project(ReceiptFan fan, ReceiptEnvelope envelope) => fan.Project(envelope.Kind, envelope.Payload);

    public static IO<Seq<IDisposable>> Tap(HookRail rail, ReceiptFan fan) =>
        IO.lift(() => Seq<IDisposable>(
            rail.Receipt.Observe(envelope => IO.lift(() => Project(fan, envelope))),
            rail.Phase.Observe(value => IO.lift(() => fan.Set.Count("rasm.apphost.lifecycle.transitions", 1L,
                new KeyValuePair<string, object?>("from", value.From.Key),
                new KeyValuePair<string, object?>("to", value.To.Key),
                new KeyValuePair<string, object?>("trigger", value.Trigger)))),
            rail.Degradation.Observe(reading => IO.lift(() =>
                fan.Cells.Level("rasm.apphost.health.level", (double)reading.State.Level.Rank)))));

    // NodaTime Duration crosses the wire as JsonRoundtrip text; the one arm-side decode to seconds.
    static double Seconds(JsonElement element) =>
        DurationPattern.JsonRoundtrip.Parse(element.GetString()!).Value.TotalSeconds;
}
```

[CONTRIBUTED_ARMS]: every emitting package contributes through the kernel port shape — a wire-borne contributor mounts one kind-arm table beside its port, a typed-fold contributor projects pre-envelope and contributes rows alone; a duplicate kind across any two tables faults at the frozen merge, and each port carries its own semconv schema coordinate. Host custody names where the contributor's meters live — hosted roots ride `SignalGovernance.Govern`, plugin-hosted processes ride the `PluginTelemetryHost` per-ALC capsule.

| [INDEX] | [CONTRIBUTOR]      | [PROJECTION]                            | [PORT_MINT]                        | [CUSTODY]        |
| :-----: | :----------------- | :-------------------------------------- | :--------------------------------- | :--------------- |
|  [01]   | `Rasm` kernel      | typed fold — `TelemetrySink.Tap`        | `KernelInstruments.Telemetry`      | composing root   |
|  [02]   | `Rasm.Element`     | typed fold — `GraphInstrument`          | `ElementInstruments.Telemetry`     | composing root   |
|  [03]   | `Rasm.Bim`         | typed fold — `BimTelemetry.Tap`         | `BimTelemetry.Telemetry`           | composing root   |
|  [04]   | `Rasm.Materials`   | receipt arms at this fan                | `MaterialsInstruments.Telemetry`   | composing root   |
|  [05]   | `Rasm.Fabrication` | `FabricationInstruments.Arms`           | `FabricationInstruments.Telemetry` | host root        |
|  [06]   | `Rasm.Persistence` | `StoreInstruments.Arms`                 | `StoreInstruments.Telemetry`       | host root        |
|  [07]   | `Rasm.Compute`     | typed fold — `ComputeInstrumentFan`     | `ReceiptSurface.Telemetry`         | host root        |
|  [08]   | `Rasm.AppUi`       | typed fold — `EvidenceFan`              | `AppUiTelemetry.Contribute`        | host root        |
|  [09]   | `Rasm.Rhino`       | observe taps on the mount registry      | `RhinoInstruments.Telemetry`       | plugin ALC       |
|  [10]   | `Rasm.Grasshopper` | typed fold — `GhInstruments.Project`    | scope admission by name            | plugin ALC       |

## [04]-[PROVIDER_LIFETIME]

- Owner: `PluginTelemetryHost` — the zero-host trace-and-metric capsule for Rhino/GH plugin processes; one instance per plugin `AssemblyLoadContext` — the Grasshopper canvas ALC and the Rhino plugin ALC each open their own capsule, the custody the `[03]` contributor roster names.
- Entry: `PluginTelemetryHost.Open(AssemblyLoadContext alc, HostProfile profile, Action<ResourceBuilder> identity)` — builds the minimal per-ALC service provider, the explicit tracer and meter providers, and the unload hook; `identity` arrives as the `ResourceIdentity.Compose` product, so the detector rows ride the capsule resource exactly as the hosted root.
- Auto: `new ServiceCollection().AddMetrics()` mints the per-ALC `IMeterFactory`, so two co-resident plugins minting a `Rasm.Compute` meter in one `Rhino.exe` stay isolated by provider scope; `AssemblyLoadContext.Unloading` drives bounded `ForceFlush` then `Dispose` on both providers before the mini provider; the exemplar filter pins `ExemplarFilterType.TraceBased` so any measurement recorded inside an active span carries its trace and span id with zero wiring; the tracer builder carries the same `AddBaggageActivityProcessor(SignalGovernance.PromotedBaggage)` promotion row the hosted root binds, so tenant attribution holds on plugin spans.
- Packages: OpenTelemetry, OpenTelemetry.Extensions, OpenTelemetry.Exporter.OpenTelemetryProtocol, Microsoft.Extensions.DependencyInjection, LanguageExt.Core, BCL inbox.
- Growth: a new plugin-visible meter or source is one `AddMeter`/`AddSource` row in `Open`; a new resource dimension is one detector or identity line inside `ResourceIdentity.Compose`; `FlushBound` is the one unload-flush policy value.
- Boundary: the provider — never the `Meter` or `ActivitySource` — is the disposable owner, and the capsule is the enforcing structure behind the process-static-meter prohibition: every meter reaches the process through a factory whose lifetime is the ALC; service-modality processes take the host-owned `SignalGovernance.Govern` path instead, so exactly one provider owner exists per process shape; OTLP egress is HTTP+protobuf with endpoint, headers, and protocol bound from `OTEL_EXPORTER_OTLP_*`, and the histogram default aggregation rides `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION=base2_exponential_bucket_histogram` with the kernel `Buckets` advice as the explicit-bucket fallback; disk retry remains outside capsule fence code under `Observability/telemetry#[OTLP_DISK_RETRY_BINDING]`; logs remain on the host `ILogger` projection and continuous profiling remains the process-wide Pyroscope agent, so this capsule claims only the two providers it owns.

```csharp signature
public sealed class PluginTelemetryHost : IDisposable {
    static readonly TimeSpan FlushBound = TimeSpan.FromSeconds(5);

    readonly ServiceProvider services;
    readonly TracerProvider tracing;
    readonly MeterProvider metrics;
    int disposed;

    PluginTelemetryHost(ServiceProvider services, TracerProvider tracing, MeterProvider metrics) =>
        (this.services, this.tracing, this.metrics) = (services, tracing, metrics);

    public IMeterFactory Meters => services.GetRequiredService<IMeterFactory>();

    public static PluginTelemetryHost Open(AssemblyLoadContext alc, HostProfile profile, Action<ResourceBuilder> identity) {
        var services = new ServiceCollection().AddMetrics().BuildServiceProvider();
        var tracing = Sdk.CreateTracerProviderBuilder()
            .ConfigureResource(identity)
            .AddSource([.. TelemetrySource.Items.Where(static row => row.Minted).Select(static row => row.Key)])
            .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(TelemetrySignal.Trace.Ratio(profile))))
            .AddBaggageActivityProcessor(SignalGovernance.PromotedBaggage)
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
        if (Interlocked.Exchange(ref disposed, 1) != 0) {
            return;
        }
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

(none)
