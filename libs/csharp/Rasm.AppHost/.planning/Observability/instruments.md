# [APPHOST_DOMAIN_INSTRUMENTS]

One table-driven roster projects the receipt fan into `System.Diagnostics.Metrics` instruments, so every operational metric is a projection of a typed receipt, never a parallel truth minted at a call site. Owned axes: the domain `InstrumentRow` roster with its bucket-advice policy, the receipt-to-instrument projection fold, the per-ALC provider-lifetime capsule for zero-host plugin processes, and the instrument observation rail — libraries emit through `Meter`, `ActivitySource`, and `ILogger` alone, and every OpenTelemetry SDK member on this page composes at a composition root.

Settled composition: `InstrumentRow`, `TelemetrySource`, and `TelemetryIdentity.Mint` arrive from Observability/telemetry#TELEMETRY_IDENTITY; `TelemetryContributorPort` and `ReceiptEnvelope` from Runtime/ports#PORT_RECORDS; the observe tap that feeds the projection from Observability/hooks#HOOK_REGISTRY; the trace-based exemplar row and the `AddView` cardinality caps from Observability/telemetry#SIGNAL_GOVERNANCE. Metric names are dotted `rasm.<domain>.<measure>`, units are UCUM (`s`, `By`, `1`, `{thing}`), never pre-baked `_total`/unit suffixes; instrumentation scope name is the emitting package id, version-stamped and schema-pinned through the contributor port's `SchemaUrl` coordinate, identical across tracer, meter, and logger; Prometheus translation standardizes on `NoUTF8EscapingWithSuffixes` so dotted names survive byte-identical from every runtime.

## [01]-[INDEX]

- [01]-[INSTRUMENT_CATALOG]: Domain instrument roster, cost-vector derivation, GenAI rows, and bucket advice.
- [02]-[RECEIPT_PROJECTION]: One projection fold from the receipt fan onto the mounted instruments.
- [03]-[PROVIDER_LIFETIME]: Per-ALC `IMeterFactory` capsule with unload-ordered flush and dispose.
- [04]-[OBSERVATION_RAIL]: `MetricCollector<T>` assertion rail and the out-of-process live-read boundary.

## [02]-[INSTRUMENT_CATALOG]

- Owner: `HostInstruments` — the AppHost domain roster of `InstrumentRow` values; `Buckets` the explicit-bucket advice policy rows.
- Cases: hop attempt and duration rows off `HopReceipt`; outbox watermark-lag and oldest-undelivered-age level rows off the relay sweep, partitioned by topic; capability-roster (partitioned by surface family), per-probe status, stale-binding, and degradation-level rows read off the `LevelCells` atoms; broker delivery-outcome counts off `DeliveryReceipt`; command-admission counts and per-unit spend rows off `CommandReceipt`, the spend family derived from the `CostUnit` vocabulary; lifecycle-transition counts off the `Phase` hook tap; benchmark duration and regression rows off `BenchmarkReceipt`; the two GenAI semconv rows `gen_ai.client.token.usage` and `gen_ai.client.operation.duration` stamped by the governed model loop.
- Entry: `HostInstruments.Telemetry(string version, string schemaUrl)` — the one `TelemetryContributorPort` carrying the merged spine and domain row set with its semconv schema coordinate, mounted beside every sibling contributor port at `[02]-[RECEIPT_PROJECTION]`.
- Auto: the spend rows derive from `CostUnit.Items` through the `Ucum` policy map, so a new cost unit is one vocabulary row and the roster follows with zero edits here; every histogram row ships `InstrumentAdvice<T>` explicit-bucket boundaries at creation — the one generic `Buckets.Advised<T>` bind serves the seconds and token-count carriers alike — the fallback a backend without exponential histograms reads — the default aggregation is base2 exponential per the `[03]` provider rows.
- Packages: Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: one domain metric is one `InstrumentRow` in this roster and one projection arm at `[02]-[RECEIPT_PROJECTION]`; a per-unit family derives from its owning vocabulary, never hand-enumerated rows.
- Boundary: every row binds through the `TelemetryIdentity.Mint` meter, so no instrument outlives its `IMeterFactory` owner; the GenAI rows keep the semconv spelling rather than the `rasm.*` namespace because the convention owns the name, and their `gen_ai.token.type` dimension carries the input/output split the reasoning loop stamps; level-shaped facts ride observable gauges reading the `LevelCells` atoms at collection cadence — a polled level through a synchronous gauge aliases; a keyed atom is the partitioned form — one `Measurement<T>` multi-value callback projects the whole partition map in one collection pass through `LevelCells.Series`, a scalar atom stays for an un-partitioned level, and every partition vocabulary is bounded by its owning row set (`topic` by the outbox topic roster, `family` by `CapabilityDescriptor.Surface`, `probe` by the `DriverProbe` keys), so the series budget stays inside the `*`-wildcard `AddView` cap; the `rasm.apphost.health.level` gauge reads the `Health` cell the `[02]-[RECEIPT_PROJECTION]` `Degradation` tap folds from `DegradationReading.State.Level.Rank`, the `rasm.apphost.probe.status` gauge reads the `Probes` cell the same tap folds from the snapshot entries, and the `rasm.apphost.capability.roster` gauge reads the `Roster` cell `DescriptorSurface.Describe` folds at descriptor admission; tenant fan-out on every row rides the `*`-wildcard `AddView` series cap at Observability/telemetry#SIGNAL_GOVERNANCE.

```csharp signature
public static class Buckets {
    public static readonly ImmutableArray<double> HopSeconds = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10];
    public static readonly ImmutableArray<double> ModelSeconds = [0.1, 0.25, 0.5, 1, 2, 5, 10, 30, 60];
    public static readonly ImmutableArray<double> BenchSeconds = [0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1, 1, 10];
    public static readonly ImmutableArray<long> TokenCounts = [16, 64, 256, 1024, 4096, 16384, 65536];

    public static Histogram<T> Advised<T>(Meter meter, string name, string unit, string text, ImmutableArray<T> bounds) where T : struct =>
        meter.CreateHistogram<T>(name, unit, text, tags: null, advice: new InstrumentAdvice<T> { HistogramBucketBoundaries = bounds });
}

public sealed record LevelCells(
    Atom<HashMap<string, long>> OutboxLag,
    Atom<HashMap<string, double>> OldestAge,
    Atom<HashMap<string, long>> Roster,
    Atom<HashMap<string, long>> Probes,
    Atom<long> StaleBindings,
    Atom<long> Health) {
    public static readonly LevelCells Live = new(
        Atom(HashMap<string, long>()),
        Atom(HashMap<string, double>()),
        Atom(HashMap<string, long>()),
        Atom(HashMap<string, long>()),
        Atom(0L),
        Atom(0L));

    // One partition projection serves every keyed cell: a keyed atom reads as a tagged
    // Measurement<T> set from one callback, so a partitioned level is one collection pass,
    // never a per-key gauge family.
    public static IEnumerable<Measurement<T>> Series<T>(Atom<HashMap<string, T>> cell, string tag) where T : struct =>
        cell.Value.AsIterable().Map(pair => new Measurement<T>(pair.Value, new KeyValuePair<string, object?>(tag, pair.Key)));
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
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.outbox.lag", "{op}", "ops between the durable head and the slowest consumer watermark, by topic",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => LevelCells.Series(LevelCells.Live.OutboxLag, "topic"), unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.outbox.oldest.age", "s", "age of the oldest undelivered outbox op, by topic",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => LevelCells.Series(LevelCells.Live.OldestAge, "topic"), unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.capability.roster", "{descriptor}", "capability descriptors admitted into the live registry, by surface family",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => LevelCells.Series(LevelCells.Live.Roster, "family"), unit, text)),
        new InstrumentRow(TelemetrySource.AppHost, "rasm.apphost.probe.status", "1", "backing-service probe status by probe, zero unhealthy through two healthy",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => LevelCells.Series(LevelCells.Live.Probes, "probe"), unit, text)),
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
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.TokenCounts)),
        new InstrumentRow(TelemetrySource.AppHost, "gen_ai.client.operation.duration", "s", "governed model operation duration",
            static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.ModelSeconds)))
        + CostUnit.Items.AsIterable().Map(static unit => new InstrumentRow(
            TelemetrySource.AppHost, $"rasm.apphost.grant.spend.{unit.Key}", Ucum[unit], $"cost debited against the {unit.Key} ceiling",
            static (meter, name, unitCode, text) => meter.CreateCounter<long>(name, unitCode, text))).ToSeq();

    public static TelemetryContributorPort Telemetry(string version, string schemaUrl) =>
        new(TelemetrySource.AppHost, version, schemaUrl, TelemetryIdentity.SpineRows + Rows);
}
```

## [03]-[RECEIPT_PROJECTION]

- Owner: `InstrumentFan` — the one receipt-to-instrument projection; `InstrumentSet` the mounted-instrument capsule.
- Entry: `InstrumentFan.Mount(IMeterFactory factory, CorrelationId root, Seq<FrozenDictionary<string, Action<InstrumentSet, JsonElement>>> contributed, params ReadOnlySpan<TelemetryContributorPort> contributors)` mints each contributor's meter through `TelemetryIdentity.Mint` — stamping each port's `SchemaUrl` coordinate as `MeterOptions.TelemetrySchemaUrl` so every contributed roster satisfies the wire law's schema pin — materializes its rows over it, and merges the contributed kind-arm tables beside the AppHost table onto `InstrumentSet.Arms` — the port is the one inward instrument seam, AppHost's own roster entering as the `HostInstruments.Telemetry` port beside every sibling contribution, and the Persistence `StoreInstruments.Arms` table entering as one contributed element; `InstrumentFan.Project(InstrumentSet set, ReceiptEnvelope envelope)` folds one envelope into instrument writes through the mounted arm table; `InstrumentFan.Tap(HookRail rail, InstrumentSet set)` mounts the fan's three hook subscriptions at composition, and the kernel hook-tap subscriber registers on the same hook rail at the same composition point.
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
            // parameter beside a Live-bound gauge is the split-truth knob this table refuses. The
            // sweep arm folds the receipt's per-topic lane rows into the keyed cells the partitioned
            // gauges read, so a sweep replaces its whole partition map, never a stale-key residue.
            [SweepKind] = static (_, payload) => {
                var lanes = payload.GetProperty("lanes").EnumerateArray().ToSeq();
                ignore(LevelCells.Live.OutboxLag.Swap(_ => lanes.Fold(HashMap<string, long>(), static (map, lane) =>
                    map.AddOrUpdate(lane.GetProperty("topic").GetString() ?? "", lane.GetProperty("lag").GetInt64()))));
                ignore(LevelCells.Live.OldestAge.Swap(_ => lanes.Fold(HashMap<string, double>(), static (map, lane) =>
                    map.AddOrUpdate(lane.GetProperty("topic").GetString() ?? "", lane.GetProperty("oldestAgeSeconds").GetDouble()))));
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
            .Map(port => (Port: port, TelemetryIdentity.Mint(factory, port.Source, port.Version, port.SchemaUrl, root).Meter))
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
        rail.Degradation.Observe(reading => IO.lift(() => {
            ignore(LevelCells.Live.Health.Swap(_ => (long)reading.State.Level.Rank));
            ignore(LevelCells.Live.Probes.Swap(probes => reading.Snapshot.Entries.Fold(probes,
                static (folded, entry) => folded.AddOrUpdate(entry.Name, (long)entry.Status))));
            return unit;
        })));

    // NodaTime Duration crosses the wire as JsonRoundtrip text; the one arm-side decode to seconds.
    static double Seconds(JsonElement element) =>
        DurationPattern.JsonRoundtrip.Parse(element.GetString()!).Value.TotalSeconds;
}
```

[CONTRIBUTED_ARMS]: every emitting package mounts through one contributed kind-arm table and one `TelemetryContributorPort` mint, merged at `Mount` beside the Persistence `StoreInstruments.Arms` precedent; a duplicate kind across any two tables faults at the frozen merge, and each port carries its own semconv schema coordinate. Host custody names where the contributor's meters live — hosted roots ride `SignalGovernance.Govern`, plugin-hosted processes ride the `PluginTelemetryHost` per-ALC capsule.

| [INDEX] | [CONTRIBUTOR]      | [KIND_PARTITION]                | [ARM_TABLE]                   | [PORT_MINT]                        | [CUSTODY]                   |
| :-----: | :----------------- | :------------------------------ | :---------------------------- | :--------------------------------- | :-------------------------- |
|  [01]   | `Rasm` kernel      | `rasm.kernel.*`                 | `KernelInstruments.Arms`      | `KernelInstruments.Telemetry`      | host root + plugin capsules |
|  [02]   | `Rasm.Bim`         | `rasm.bim.*`                    | `BimInstruments.Arms`         | `BimInstruments.Telemetry`         | host root                   |
|  [03]   | `Rasm.Element`     | `rasm.element.*`                | `GraphInstruments.Arms`       | `GraphInstruments.Telemetry`       | host root                   |
|  [04]   | `Rasm.Grasshopper` | `rasm.grasshopper.*`            | `CanvasInstruments.Arms`      | `CanvasInstruments.Telemetry`      | Grasshopper ALC capsule     |
|  [05]   | `Rasm.Materials`   | `rasm.materials.*`              | `MaterialsInstrumentFan.Arms` | `MaterialsInstrumentFan.Telemetry` | host root                   |
|  [06]   | `Rasm.Rhino`       | `rasm.rhino.<domain>.<measure>` | `RhinoInstruments.Arms`       | `RhinoInstruments.Telemetry`       | Rhino ALC capsule           |
|  [07]   | `Rasm.Fabrication` | `rasm.fabrication.*`            | `FabricationInstruments.Arms` | `FabricationInstruments.Telemetry` | host root                   |
|  [08]   | `Rasm.Persistence` | `store.*` census                | `StoreInstruments.Arms`       | `StoreInstruments.Telemetry`       | host root                   |

The `Rasm.Rhino` partition carries document, command, and tenant attribution tags on its receipt-kind rows; the kernel row's hook-tap subscriber registers on the hook rail beside `Tap` so kernel `SignalFact` facts project through the same fan; a roster name not yet landed sibling-side is the mount contract that package's contributor card realizes.

## [04]-[PROVIDER_LIFETIME]

- Owner: `PluginTelemetryHost` — the zero-host telemetry capsule for Rhino/GH plugin processes; one instance per plugin `AssemblyLoadContext` — the Grasshopper canvas ALC and the Rhino plugin ALC each open their own capsule, the custody the `[03]` contributor roster names.
- Entry: `PluginTelemetryHost.Open(AssemblyLoadContext alc, HostProfile profile, Action<ResourceBuilder> identity, string queueRoot)` — builds the minimal per-ALC service provider, the explicit tracer and meter providers, the per-capsule `OfflineQueue`, and arms the unload hook; `identity` arrives as the `ResourceIdentity.Compose` product, so the detector rows ride the capsule resource exactly as the hosted root.
- Auto: `new ServiceCollection().AddMetrics()` mints the per-ALC `IMeterFactory`, so two co-resident plugins minting a `Rasm.Compute` meter in one `Rhino.exe` stay isolated by provider scope; `AssemblyLoadContext.Unloading` drives `ForceFlush` then `Dispose` on both providers, then the mini provider, then the offline queue — so a plugin unload never drops the tail of an export batch and an undeliverable tail persists to the queue for the next session's replay; the exemplar filter pins `ExemplarFilterType.TraceBased` so any measurement recorded inside an active span carries its trace and span id with zero wiring; the tracer builder carries the same `AddBaggageActivityProcessor(SignalGovernance.PromotedBaggage)` promotion row the hosted root binds, so tenant attribution holds on plugin spans.
- Packages: OpenTelemetry, OpenTelemetry.Extensions, OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.PersistentStorage.FileSystem, Microsoft.Extensions.DependencyInjection, LanguageExt.Core, BCL inbox.
- Growth: a new plugin-visible meter or source is one `AddMeter`/`AddSource` row in `Open`; a new resource dimension is one detector or identity line inside `ResourceIdentity.Compose`; `FlushBound` is the one unload-flush policy value and the queue caps ride `OfflineQueuePolicy.Canonical`.
- Boundary: the provider — never the `Meter` or `ActivitySource` — is the disposable owner, and the capsule is the enforcing structure behind the process-static-meter prohibition: every meter reaches the process through a factory whose lifetime is the ALC; service-modality processes take the host-owned `SignalGovernance.Govern` path instead, so exactly one provider owner exists per process shape; OTLP egress is HTTP+protobuf with endpoint, headers, and protocol bound from `OTEL_EXPORTER_OTLP_*`, and the histogram default aggregation rides `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION=base2_exponential_bucket_histogram` with the `[01]` bucket advice as the explicit-bucket fallback; the per-capsule `OfflineQueue` is the disconnected-fleet durability leg — the dominant Rhino/GH deployment shape loses its collector routinely, so failed batches persist under the capsule's queue root and replay on reconnect, already-redacted bytes only.

```csharp signature
public sealed class PluginTelemetryHost : IDisposable {
    static readonly TimeSpan FlushBound = TimeSpan.FromSeconds(5);

    readonly ServiceProvider services;
    readonly TracerProvider tracing;
    readonly MeterProvider metrics;
    readonly OfflineQueue queue;

    PluginTelemetryHost(ServiceProvider services, TracerProvider tracing, MeterProvider metrics, OfflineQueue queue) =>
        (this.services, this.tracing, this.metrics, this.queue) = (services, tracing, metrics, queue);

    public IMeterFactory Meters => services.GetRequiredService<IMeterFactory>();

    public static PluginTelemetryHost Open(AssemblyLoadContext alc, HostProfile profile, Action<ResourceBuilder> identity, string queueRoot) {
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
        var host = new PluginTelemetryHost(services, tracing, metrics, OfflineQueue.Open(queueRoot, OfflineQueuePolicy.Canonical));
        alc.Unloading += _ => host.Dispose();
        return host;
    }

    public void Dispose() {
        ignore(tracing.ForceFlush((int)FlushBound.TotalMilliseconds));
        ignore(metrics.ForceFlush((int)FlushBound.TotalMilliseconds));
        tracing.Dispose();
        metrics.Dispose();
        services.Dispose();
        queue.Dispose();
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
