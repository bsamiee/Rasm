# [APPHOST_DOMAIN_INSTRUMENTS]

One measure roster declares every `System.Diagnostics.Metrics` instrument this platform writes, and every producing fold writes its own row through the one mounted set, so an operational metric is the observation of a typed result at the site that produced it, never a parallel truth minted beside it. Owned axes: the `AppHostSlot` dimension vocabulary, the `AppHostMeasure` roster whose rows carry their own `InstrumentSpec` beside the board tile and reliability grade that name them, the contributed-port mount, the `ProviderProgram` both provider owners bind, and the instrument observation rail — libraries emit through `Meter`, `ActivitySource`, and `ILogger` alone, and every OpenTelemetry SDK member on this page composes at a composition root.

Settled composition: `InstrumentSpec` with its `InstrumentKind` and `MeasureForm` axes, `InstrumentSet`, `Buckets`, `LevelCells`, and `TelemetryIdentity.Metered` arrive from `Rasm/Domain/instrument`; `TelemetryContributorPort` from `Rasm/Domain/telemetry#CONTRIBUTE`; `TelemetrySource`, `CorrelationId`, and `TenantContext` from `Rasm/Domain/frame`; `Sli`, `LevelBreach`, `Objective`, `PanelSpec`, `PanelKind`, and `BoardPack` from `Rasm/Domain/objective`, so this page declares instances alone and mints no severity, burn, polarity, or panel vocabulary; `Cell.Seat` and `Transition` from `Rasm/Domain/rails#TRANSITION`; `TelemetryDomain`, `SignalGovernance.Rostered`/`.Views`, `LogPipeline.Owner`, `Correlation.Spine`, `ResourceIdentity.Compose`, and `TelemetryComposition.Signals` from Observability/telemetry#SIGNAL_GOVERNANCE. Metric names are dotted `rasm.<domain>.<measure>` derived off `TelemetryDomain.AppHost`, units are UCUM (`s`, `By`, `1`, `{thing}`), never pre-baked `_total`/unit suffixes; instrumentation scope is the emitting package's own `TelemetrySource` row, version-stamped and schema-pinned through the kernel `TelemetryIdentity.SchemaUrl` const the mint stamps, identical across tracer, meter, and logger; Prometheus translation standardizes on `NoUTF8EscapingWithSuffixes` so dotted names survive byte-identical from every runtime.

## [01]-[INDEX]

- [02]-[INSTRUMENT_CATALOG]: the slot vocabulary, the `AppHostMeasure` roster, the derived board pack, and the contributor port.
- [03]-[INSTRUMENT_MOUNT]: the contributor mount fold and the law seating every write at its producing fold.
- [04]-[PROVIDER_LIFETIME]: the `ProviderProgram` both provider owners bind and the per-ALC capsule for zero-host plugin processes.
- [05]-[OBSERVATION_RAIL]: `MetricCollector<T>` assertion rail and the out-of-process live-read boundary.

## [02]-[INSTRUMENT_CATALOG]

- Owner: `AppHostSlot` `[SmartEnum<string>]` — the dimension-key vocabulary a row's `Dimensions` and its `[03]` write both spell; `Indicator` `[Union]` — the metric-free reliability shape; `Reliability` — the objective row a measure carries; `AppHostMeasure` `[SmartEnum<string>]` — the ONE AppHost roster, each row carrying its own `InstrumentSpec` beside the optional board tile and reliability grade that name it, with `Rows`, `Board`, `Spend`, and the contributor-port mint all DERIVING from `Items`.
- Cases: incident-buffer flush counts written by `IncidentBuffers.Flush` and sink-loss counts by `SerilogSinks`, the two log-plane rows whose producing arms sit at Observability/telemetry#LOG_PROJECTION; masked-value counts written by `SupportCapture` off the exported manifest, the one plane carrying both a mask and its classification; hop attempt and duration rows written by `OutboundSurface.Dispatch` off the measured dial; stale-binding and degradation-level rows read the composition's `LevelCells` scalars, the capability-roster row reading the keyed family the frozen capability registry projects off its own surface index at composition; broker delivery-outcome counts written by `DeliveryFanout.Fan` per dialed leg; command-admission counts and per-unit spend rows written by `CommandDispatch` off the committed `CommandResult`, the spend family derived from the `CostUnit` vocabulary; lifecycle-transition counts written by `Lifecycle.Settle`; benchmark duration and regression rows written by `BenchmarkGate.Gate`; live-wire rejection counts written at the inbound coercion seam and write-back disposition counts at the write-back mint; fleet-wave counts written at the wave annotation; machine-observation counts at the decode lane; durable-egress batch and byte counts written by `OtlpOfflineQueue`, one population partitioned on signal and disposition; the two GenAI semconv rows `gen_ai.client.token.usage` and `gen_ai.client.operation.duration` written by the governed model loop.
- Entry: `AppHostMeasure.Rows` is the declaration roster — every `Items` row's own `Row` beside the `CostUnit`-derived spend family; `AppHostMeasure.Spend(CostUnit)` resolves one spend row; `AppHostMeasure.Board` is the pack the panel and grade columns derive, handed outward on the port rather than reached by name from a composition root; `AppHostMeasure.Telemetry(string version)` is the one `TelemetryContributorPort` carrying that row set and that pack — its semconv coordinate is the kernel `TelemetryIdentity.SchemaUrl` const the mint stamps — mounted and proved beside every sibling contributor port at `[03]-[INSTRUMENT_MOUNT]`.
- Auto: every branch-owned key resolves `TelemetryDomain.AppHost` rather than concatenating a local head const, so `SignalGovernance.Rostered` grades a name this roster derived from the very row it grades against and a rename moves resource, instrument, and dimension together; the spend rows derive from `CostUnit.Items` and read each row's own `Ucum`, so the unit vocabulary answers once at its owner; a distribution row naming a `Buckets` policy ships `InstrumentAdvice<T>` explicit boundaries at creation — the fallback a backend without exponential histograms reads — while a bare distribution keeps the base2-exponential default the `[04]` program pins; a keyed level family names its tag once at its factory, which the kernel spec seats as the row's own leading dimension, so per-key cardinality rides one instrument, a panel breaks on the key with no second declaration, and a per-key instrument mint is the deleted form; that leading dimension declares the key the family MAY carry rather than one every entry holds, so a producer whose group has no key writes the same row untagged and the declaration reads as its own absence arm — one roster answers a partitioned and an unpartitioned composition exactly as one `Dimensions` set answers a tenanted and an untenanted write — and the untagged entry is that family's absent-key PARTITION, never a total over its keyed ones; a reliability target over a tag-partitioned counter reads the good values off that counter's declared dimension, so an availability objective mints no numerator twin; each panel omits its widget so `PanelKind.For` derives the canonical one from the row's own measurement shape, and each objective omits its window so the kernel compliance default applies.
- Law: NAMED LOSS — compile-time head concatenation. Every measure name and dimension key now assembles at type init through `TelemetryDomain.AppHost.Measure(segment)` rather than as a `const` fold, so no name is a compile-time constant and no attribute literal can name one. What replaces it is stronger: the head has ONE owner, the roster that grades names and the roster that mints them are the same value, and a domain rename that once left this page's head stale now cannot compile past its own row.
- Law: NAMED LOSS — a panel breaking on a proper SUBSET of its row's dimensions, and an objective over a metric other than its own row. `PanelSpec.By` derives from `Row.Dimensions` and `Sli`'s metric from `Row.Name`, because every tile broke on the whole declared set and every indicator named its own row — a hand-spelled break key or metric name is a mirror that drifts, and `Slo.Admit` catches it only after a boot. A narrower break lands as a `By` column on the measure when a consumer proves one; a two-metric `Sli.Ratio` lands as a `BoardPack` row on the port, where a cross-row indicator belongs.
- Law: `Indicator` is metric-FREE by construction, so the objective a row declares can name no series but that row's. The kernel `Sli` family stays five-cased; this roster instantiates the three shapes its rows measure and the two it does not are reachable through the port's own pack.
- Growth: one domain metric is one `AppHostMeasure` row and one `set.Write` at the fold that produces the fact; a per-unit family derives from its owning vocabulary, never hand-enumerated rows; a level is one `set.Level` write at its producing arm; a new tag key is one `AppHostSlot` row both the measure's `Dimensions` and its write read; one board tile is one `panel:` argument and one reliability target one `grade:` argument on the row that carries the series.
- Boundary: every row binds through the composition's minted meter, so no instrument outlives its `IMeterFactory` owner; `LevelCells` reaches a pulled row at bind time through the kernel spec's own bind derivation, so no row closes over the cell and one declaration mounts against any composition — three arms write the levels the gauges read and each spells ONE call, so a pulled row is current at every collection and no process-static cell exists: `DegradationCell` writes `signals.Level(AppHostMeasure.HealthLevel.Row, reading.Level.Rank)` on every committed reading, the Agent/capability registry mount writes the keyed `CapabilityRoster` family off its surface index, and the Wire/livewire binding fold writes the stale-binding level on every binding-state commit, that count being what its own state map holds in a stale or faulted case — a level written on ENTRY to that state alone decays into a reading no recovery ever clears; the drain band writes its distribution row at the same altitude, one `set.Write(AppHostMeasure.DrainDuration.Row, consumed.TotalSeconds, InstrumentSet.Tags((AppHostSlot.Band, band.Key)))` per completed drain step at Runtime/lifecycle#DRAIN_CONDUCTOR, so the reliability objective over that series grades a measured population instead of reading permanently healthy over an empty one — the roster family is the composition-freeze projection its owning registry carries, never a per-admission push a registration fold strands mid-composition; the GenAI rows keep the semconv spelling rather than the `rasm.*` namespace because the convention owns the name, and their `gen_ai.token.type` dimension carries the input/output split the reasoning loop stamps; the declared `Dimensions` ARE the exported tag vocabulary — the governance view predicate at Observability/telemetry#SIGNAL_GOVERNANCE projects each stream onto this row's own list beside the one tenancy key under one series budget, so an undeclared tag reaches no exporter and a row admits an evidence string only by declaring it here, where this roster's classification grades it — a declared key a given entry omits is absent from that entry's tag set, which the one predicate row shapes exactly as it shapes the keyed entries, so a sometimes-absent dimension mints no second stream and the whole family stays inside one budget; the pack declares HERE rather than beside the alert engine because a panel and an objective name mounted instruments while a health rule names a `HealthSignal`, so the two never share an evidence plane — objectives compile to `AlertSpec` rows the deploy plane provisions off a metric series, and the alert engine folds the in-process degradation reading, so a burn arm on either side mints the second grader both boundaries forbid; the pack leaves this roster only on the contributor port, so the AppHost proves at `[03]-[INSTRUMENT_MOUNT]` through the same fold every sibling pack rides and no composition root spells this static field; admission resolves against the port's declaration rather than a mounted handle set, so a plugin-ALC contributor's pack is provable at the same seam.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Threading;
using NodaTime;
using Thinktecture;

namespace Rasm.AppHost.Observability;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AppHostSlot {
    public static readonly AppHostSlot Hop = Branch("hop");
    public static readonly AppHostSlot Outcome = Branch("outcome");
    public static readonly AppHostSlot Channel = Branch("channel");
    public static readonly AppHostSlot Txn = Branch("txn");
    public static readonly AppHostSlot Binding = Branch("binding");
    public static readonly AppHostSlot Strategy = Branch("strategy");
    public static readonly AppHostSlot Verdict = Branch("verdict");
    public static readonly AppHostSlot Machine = Branch("machine");
    public static readonly AppHostSlot Suite = Branch("suite");
    public static readonly AppHostSlot Case = Branch("case");
    public static readonly AppHostSlot Surface = Branch("surface");
    public static readonly AppHostSlot Band = Branch("drain.band");
    public static readonly AppHostSlot Class = Branch("data.class");
    public static readonly AppHostSlot From = Branch("phase.from");
    public static readonly AppHostSlot To = Branch("phase.to");
    public static readonly AppHostSlot Trigger = Branch("phase.trigger");
    public static readonly AppHostSlot Signal = Branch("signal");
    public static readonly AppHostSlot Disposition = Branch("otlp.disposition");
    public static readonly AppHostSlot Scope = Branch("buffer.scope");
    public static readonly AppHostSlot Sink = Branch("sink");
    public static readonly AppHostSlot Loss = Branch("loss.kind");
    public static readonly AppHostSlot Topic = Branch("topic");
    public static readonly AppHostSlot TokenType = new("gen_ai.token.type");

    private static AppHostSlot Branch(string segment) => new(TelemetryDomain.AppHost.Measure(segment));
}

[Union]
public abstract partial record Indicator {
    private Indicator() { }

    public sealed record Availability(AppHostSlot By, Seq<string> Good) : Indicator;
    public sealed record Latency(Duration Ceiling, double Quantile) : Indicator;
    public sealed record Saturation(double Bound, LevelBreach Breach) : Indicator;

    public Sli Over(string metric) => Switch(
        state: metric,
        availability: static (series, row) => (Sli)new Sli.Partition(series, row.By.Key, row.Good),
        latency: static (series, row) => new Sli.Latency(series, row.Ceiling, row.Quantile),
        saturation: static (series, row) => new Sli.Saturation(series, row.Bound, row.Breach));
}

public sealed record Reliability(string Name, Indicator Shape, double Target);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AppHostMeasure {
    public static readonly AppHostMeasure LogsFlushed = Count(
        "logs.flushed", "{flush}", "incident buffer flushes by held scope", Seq(AppHostSlot.Scope));
    public static readonly AppHostMeasure LogsLost = Count(
        "logs.lost", "{record}", "log records lost by sink and failure kind", Seq(AppHostSlot.Sink, AppHostSlot.Loss),
        panel: "log loss");
    public static readonly AppHostMeasure BusDropped = Count(
        "bus.dropped", "{event}", "in-process bus loss by topic and drop class (conservation account)", Seq(AppHostSlot.Topic, AppHostSlot.Class),
        panel: "bus loss");
    public static readonly AppHostMeasure RedactionTags = Count(
        "redaction.tags", "{value}", "values this branch's egress seam masked, by classification", Seq(AppHostSlot.Class),
        panel: "redacted values");
    public static readonly AppHostMeasure DrainDuration = Elapsed(
        "drain.duration", "drain-band fold duration per band", Buckets.FoldSeconds, Seq(AppHostSlot.Band),
        panel: "drain duration",
        grade: new Reliability("drain.latency", new Indicator.Latency(Duration.FromSeconds(30), 0.95d), 0.99d));
    public static readonly AppHostMeasure HopAttempts = Count(
        "hop.attempts", "{attempt}", "outbound hop attempts by hop kind and outcome", Seq(AppHostSlot.Hop, AppHostSlot.Outcome),
        panel: "outbound hops",
        grade: new Reliability(
            "hop.availability",
            new Indicator.Availability(AppHostSlot.Outcome, Seq(HopVerdict.Delivered.Key)),
            0.99d));
    public static readonly AppHostMeasure HopDuration = Elapsed(
        "hop.duration", "outbound hop wall duration per attempt", Buckets.HopSeconds, Seq(AppHostSlot.Hop, AppHostSlot.Outcome),
        panel: "hop latency",
        grade: new Reliability("hop.latency", new Indicator.Latency(Duration.FromSeconds(1), 0.99d), 0.99d));
    public static readonly AppHostMeasure BindingStale = Level(
        "binding.stale", "{binding}", "external bindings in stale or faulted state", panel: "stale bindings");
    public static readonly AppHostMeasure DeliveryOutcomes = Count(
        "delivery.outcomes", "{delivery}", "broker deliveries by channel and outcome", Seq(AppHostSlot.Channel, AppHostSlot.Outcome),
        panel: "broker deliveries",
        grade: new Reliability(
            "delivery.availability",
            new Indicator.Availability(AppHostSlot.Outcome, Seq(HopVerdict.Delivered.Key)),
            0.99d));
    public static readonly AppHostMeasure CommandAdmissions = Count(
        "command.admissions", "{command}", "command dispatches by transaction disposition", Seq(AppHostSlot.Txn),
        panel: "command admissions");
    public static readonly AppHostMeasure CapabilityRoster = Levels(
        "capability.roster", "{descriptor}", "live capability descriptors by admitting surface", AppHostSlot.Surface,
        panel: "capability roster");
    public static readonly AppHostMeasure LifecycleTransitions = Count(
        "lifecycle.transitions", "{transition}", "lifecycle phase commits by from, to, and trigger",
        Seq(AppHostSlot.From, AppHostSlot.To, AppHostSlot.Trigger), panel: "lifecycle transitions");
    public static readonly AppHostMeasure HealthLevel = Level(
        "health.level", "1", "derived degradation level rank, zero full through four suspended", panel: "degradation level",
        grade: new Reliability("degradation",
            new Indicator.Saturation(DegradationLevel.ReducedRemote.Rank, LevelBreach.Ceiling), 0.99d));
    public static readonly AppHostMeasure BenchmarkDuration = Elapsed(
        "benchmark.duration", "gated benchmark median wall duration per case", Buckets.BenchSeconds,
        Seq(AppHostSlot.Suite, AppHostSlot.Case));
    public static readonly AppHostMeasure BenchmarkRegressions = Count(
        "benchmark.regressions", "benchmark gate verdicts past budget by suite, case, and verdict",
        Seq(AppHostSlot.Suite, AppHostSlot.Case, AppHostSlot.Verdict), unit: "{run}", panel: "benchmark regressions");
    public static readonly AppHostMeasure WireRejections = Count(
        "wire.rejections", "{rejection}", "live-wire inbound values rejected at coercion or staleness", Seq<AppHostSlot>());
    public static readonly AppHostMeasure WriteDispositions = Count(
        "write.dispositions", "{write}", "write-back transactions by binding", Seq(AppHostSlot.Binding));
    public static readonly AppHostMeasure FleetWaves = Count(
        "fleet.waves", "{wave}", "fleet rollout waves by strategy, channel, and verdict",
        Seq(AppHostSlot.Strategy, AppHostSlot.Channel, AppHostSlot.Verdict), panel: "fleet waves");
    public static readonly AppHostMeasure MachineObservations = Count(
        "machine.observations", "{observation}", "decoded machine-telemetry observations by machine", Seq(AppHostSlot.Machine));
    public static readonly AppHostMeasure OtlpOffline = Count(
        "otlp.offline", "{batch}", "otlp export batches by signal and durable-queue disposition",
        Seq(AppHostSlot.Signal, AppHostSlot.Disposition), panel: "durable otlp queue");
    public static readonly AppHostMeasure OtlpOfflineBytes = Count(
        "otlp.offline.size", "By", "otlp payload bytes crossing the durable queue by signal and disposition",
        Seq(AppHostSlot.Signal, AppHostSlot.Disposition));
    public static readonly AppHostMeasure ModelTokenUsage = Semconv(
        "gen_ai.client.token.usage", InstrumentKind.Distribution, MeasureForm.Whole, "{token}",
        "model tokens consumed per operation by token type", Seq(AppHostSlot.TokenType), Some(Buckets.TokenCounts),
        panel: "model tokens");
    public static readonly AppHostMeasure ModelOperationDuration = Semconv(
        "gen_ai.client.operation.duration", InstrumentKind.Distribution, MeasureForm.Real, Buckets.Seconds,
        "governed model operation duration", Seq<AppHostSlot>(), Some(Buckets.ModelSeconds),
        grade: new Reliability("model.latency", new Indicator.Latency(Duration.FromSeconds(30), 0.95d), 0.95d));

    public InstrumentSpec Row { get; }
    public Option<string> Panel { get; }
    public Option<Reliability> Grade { get; }

    public static Seq<InstrumentSpec> Rows => Roster.Value;

    public static InstrumentSpec Spend(CostUnit unit) => Spends.Value[unit];

    public static BoardPack Board => Pack.Value;

    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: TelemetrySource.AppHost, Version: version, Instruments: Rows,
            Planes: Seq(OutboxRelay.Scope), Board: Some(Board));

    private static AppHostMeasure Count(
        string segment, string unit, string description, Seq<AppHostSlot> dimensions,
        Option<string> panel = default, Option<Reliability> grade = default) =>
        Branch(segment, InstrumentKind.Count, MeasureForm.Whole, unit, description, dimensions, None, None, panel, grade);

    private static AppHostMeasure Elapsed(
        string segment, string description, Buckets bounds, Seq<AppHostSlot> dimensions,
        Option<string> panel = default, Option<Reliability> grade = default) =>
        Branch(segment, InstrumentKind.Distribution, MeasureForm.Real, Buckets.Seconds, description, dimensions,
            Some(bounds), None, panel, grade);

    private static AppHostMeasure Level(
        string segment, string unit, string description,
        Option<string> panel = default, Option<Reliability> grade = default) =>
        Branch(segment, InstrumentKind.Level, MeasureForm.Whole, unit, description, Seq<AppHostSlot>(), None, None, panel, grade);

    private static AppHostMeasure Levels(
        string segment, string unit, string description, AppHostSlot tag,
        Option<string> panel = default, Option<Reliability> grade = default) =>
        Branch(segment, InstrumentKind.Levels, MeasureForm.Whole, unit, description, Seq(tag), None, Some(tag.Key), panel, grade);

    private static AppHostMeasure Branch(
        string segment, InstrumentKind kind, MeasureForm form, string unit, string description,
        Seq<AppHostSlot> dimensions, Option<Buckets> bounds, Option<string> tag,
        Option<string> panel, Option<Reliability> grade) =>
        Semconv(TelemetryDomain.AppHost.Measure(segment), kind, form, unit, description, dimensions, bounds, tag, panel, grade);

    private static AppHostMeasure Semconv(
        string name, InstrumentKind kind, MeasureForm form, string unit, string description,
        Seq<AppHostSlot> dimensions, Option<Buckets> bounds, Option<string> tag = default,
        Option<string> panel = default, Option<Reliability> grade = default) =>
        new(name, InstrumentSpec.Create(name, kind, form, unit, description,
                dimensions.Map(static slot => slot.Key).Strict(), bounds, tag, None),
            panel, grade);

    private static readonly Lazy<FrozenDictionary<CostUnit, InstrumentSpec>> Spends = new(
        static () => CostUnit.Items.ToFrozenDictionary(static unit => unit, static unit => InstrumentSpec.Create(
            TelemetryDomain.AppHost.Measure($"grant.spend.{unit.Key}"), InstrumentKind.Count, MeasureForm.Whole,
            unit.Ucum, $"cost debited from the {unit.Key} balance", Seq<string>(), None, None, None)),
        LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<Seq<InstrumentSpec>> Roster = new(
        static () => toSeq(Items).Map(static row => row.Row).Concat(toSeq(Spends.Value.Values)).Strict(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<BoardPack> Pack = new(
        static () => new BoardPack(
            Wire: TelemetryDomain.AppHost.Measure("instrument"),
            Panels: toSeq(Items).Bind(static row => row.Panel.Map(title =>
                new PanelSpec(Title: title, Instrument: row.Row.Name, By: row.Row.Dimensions, Widget: None)).ToSeq()).Strict(),
            Objectives: toSeq(Items).Bind(static row => row.Grade.Map(grade => Objective.Create(
                $"{TelemetryDomain.AppHost.Key}.{grade.Name}", grade.Shape.Over(row.Row.Name), grade.Target, Duration.Zero))
                .ToSeq()).Strict()),
        LazyThreadSafetyMode.ExecutionAndPublication);
}
```

## [03]-[INSTRUMENT_MOUNT]

- Owner: `InstrumentFault` `[Union]` riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Instrument`); `InstrumentMount` — the ONE contributor mount fold, gating every port through governance and admitting its pack before the first meter exists.
- Cases: `InstrumentFault` = `Flush`.
- Entry: `InstrumentMount.Mount(IMeterFactory factory, CorrelationId root, LevelCells cells, params ReadOnlySpan<TelemetryContributorPort> contributors)` returns `Fin<InstrumentSet>` — the mounted set every producing fold on this branch writes through, seated once on `Observability/telemetry#SIGNAL_GOVERNANCE` `TelemetryComposition.Signals` and threaded from there.
- Auto: `Mount` gates every contributor through `SignalGovernance.Rostered` and folds each port's argument-free `Admit` BEFORE the first meter exists, so a port carrying an unrostered segment or an unprovable pack refuses while the composition is still editable rather than after its meter is registered; it mints each admitted contributor's meter through the kernel `TelemetryIdentity.Metered` — the meter-only mint, because span custody is the kernel band's and a paired `ActivitySource` this root never admits is a leaked source; the port's typed `Scope` names the meter, the kernel `TelemetryIdentity.SchemaUrl` const stamps `MeterOptions.TelemetrySchemaUrl`, and this composition laces the boot correlation as the one meter tag — then hands every `(meter, port.Instruments)` pair to `InstrumentSet.Of` beside the one composition `LevelCells`, the MOUNTED half alone, because a port's `Published` rows already carry handles on a meter their own load context owns and a second bind mints a second stream per name.
- Law: the row partition proves ONCE, at the kernel `InstrumentSet.Of`, which refuses naming every row declared twice across the contributed meters; a local pre-partition fold over the ports is the second grader this page deleted.
- Law: every measurement is written by the fold that produced the fact, through the mounted set it was handed — `OutboundSurface.Dispatch` writes the hop rows, `DeliveryFanout.Fan` the delivery outcome, `CommandDispatch` the admission and spend rows, the reasoning loop the two GenAI rows, `BenchmarkGate.Gate` the benchmark rows, `LiveWire` the rejection, disposition, observation, and stale-binding rows, `FleetRoll` the wave row, `OtlpOfflineQueue` the two egress rows, `SerilogSinks` the log-loss row, `IncidentBuffers.Flush` the flush row, `SupportCapture` the redaction row, `Dropped` the bus-loss row, `Lifecycle.Settle` the transition row, `DrainConductor.Step` the drain distribution, `DegradationCell` the level gauge, and the `Agent/capability` registry mount the keyed roster family. A projection fold decoding a fact stream into writes beside the producers is the second truth `libs/.planning/RULINGS.md` `[02]` forecloses.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new domain metric is one `AppHostMeasure` row at `[02]` and one `set.Write`/`Level` at its producing fold; a sibling package's rows arrive on its own `TelemetryContributorPort` and mount through this fold untouched; a contributor's first board is one `Board` value on its own port.
- Boundary: the set leaves this fold once, onto `TelemetryComposition.Signals`, and reaches every producer by composition — no producer resolves a meter by name, opens an `InstrumentSet` of its own, or reads a handle a port already published; a write on a row the composition never mounted refuses on the kernel rail at the producing site, which is where an unmeasured fact should surface.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Thinktecture;

namespace Rasm.AppHost.Observability;

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InstrumentFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Instrument;
    private InstrumentFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record Flush : InstrumentFault { public Flush(string signal) : base(signal) { } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class InstrumentMount {
    public static Fin<InstrumentSet> Mount(
        IMeterFactory factory, CorrelationId root, LevelCells cells, params ReadOnlySpan<TelemetryContributorPort> contributors) =>
        from rostered in Iterable<TelemetryContributorPort>.FromSpan(contributors).ToSeq()
            .Traverse(SignalGovernance.Rostered).As().ToFin()
        from _ in rostered.TraverseM(static port => port.Admit()).As()
        from mounted in InstrumentSet.Of(cells, [.. rostered.Map(port => (
            TelemetryIdentity.Metered(factory, port.Scope, port.Version,
                new KeyValuePair<string, object?>(CorrelationId.Slot, root.ToString())),
            port.Instruments))])
        select mounted;
}
```

## [04]-[PROVIDER_LIFETIME]

- Owner: `ProviderProgram` — the ONE provider policy both owners bind: batch squares, reader cadence, temporality, exemplar filter, propagator, promoted baggage, and the two OTLP wire pins as VALUES on one record, with a bind member per provider builder; `PluginTelemetryHost` — the zero-host trace-and-metric capsule for Rhino/GH plugin processes, one instance per plugin `AssemblyLoadContext`.
- Entry: `ProviderProgram.Canonical` is the estate program; `Bind(TracerProviderBuilder, …)` and `Bind(MeterProviderBuilder, views)` seat every column on their respective builder; `Egress(OtlpExporterOptions)` stamps the wire pins. `PluginTelemetryHost.Open(AssemblyLoadContext alc, ResolvedProfile resolved, Seq<TelemetryContributorPort> contributors, Seq<InstrumentRule> suppressed, params ReadOnlySpan<KeyValuePair<string, object>> extra)` builds the per-ALC provider under its enablement rows, both providers, and the unload hook.
- Law: the plugin capsule and the hosted root bind ONE `ProviderProgram` value, so they cannot disagree on series budget, batch squares, wire temporality, propagation, or exemplar policy — the prior form spelled eight identical rows at each owner and asserted in prose that they agreed. `SignalGovernance.Govern` binds the same value.
- Auto: `AddMetrics(services, builder => …)` mints the per-ALC `IMeterFactory` AND folds the capsule's enablement rows in the same call, so two co-resident plugins minting one meter name in one `Rhino.exe` stay isolated by provider scope and each decides its own published set; every meter mint on that factory reaches it through `MeterFactoryExtensions.Create(factory, name)`, so no site re-spells a `MeterOptions` construction the kernel `TelemetryIdentity.Metered` already owns for the schema-stamped case; a capsule that must silence a series it does not own carries `InstrumentRule` rows scoped `MeterScope.Local` — the selector matching factory-minted meters alone — and `DisableMetrics(meterName, instrumentName, listenerName, MeterScope.Local)` is that suppression, which drops a cardinality-hostile stream WITHOUT editing the contributor roster that declares it; `Sdk.SetDefaultTextMapPropagator(program.Propagator)` seats the process propagator because no hosted root runs here to seat it; the exemplar row pins trace-based sampling so any measurement recorded inside an active span carries its trace and span id with zero wiring; the tracer builder carries the same baggage promotion the hosted root binds, so tenant attribution holds on plugin spans.
- Law: disposal seats ONCE through the kernel `Cell.Seat` transition, so the second caller reads `Ceded` and returns rather than re-flushing a disposed provider; both `ForceFlush` verdicts RAIL — a flush that reported false dropped telemetry the unload will never re-send, so the capsule's `Release` returns `Fin<Unit>` accumulating one `InstrumentFault.Flush` per signal that could not drain inside the bound.
- Packages: OpenTelemetry, OpenTelemetry.Extensions, OpenTelemetry.Exporter.OpenTelemetryProtocol, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Diagnostics, LanguageExt.Core, BCL inbox.
- Growth: a new plugin-visible meter or source is one `AddMeter`/`AddSource` row in `Open`; a new resource dimension is one detector row inside `ResourceIdentity.Compose` or one `extra` row a plugin root supplies; a new stream shape is one `Dimensions` edit at the declaring measure both compositions inherit through the same view predicate; a per-capsule suppression is one `InstrumentRule` row; a tuned provider policy is one `ProviderProgram` column both owners re-read.
- Boundary: enablement and shaping are two planes that never substitute for each other — an `InstrumentRule` decides whether an instrument PUBLISHES at all on this factory, while an `AddView` row decides how a published stream is shaped, so silencing a series through a `Drop` view still pays its instrument's recording cost and gating a mount through a rule still leaves every view row intact for the next capsule; the rules scope `MeterScope.Local` by declaration because a `Global` rule reaches ctor-constructed meters in the host process this capsule does not own; the provider — never the `Meter` or `ActivitySource` — is the disposable owner, and the capsule is the enforcing structure behind the process-static-meter prohibition: every meter reaches the process through a factory whose lifetime is the ALC; service-modality processes take the host-owned `SignalGovernance.Govern` path instead, so exactly one provider owner exists per process shape; egress rides the one `LogPipeline.Owner` arbitration the hosted root reads, so a capsule in a host session that bound no collector provider builds both providers and registers no exporter — an unconditional exporter opens a collector socket every reader cadence against a door that does not answer, at the attended sample ratio of 1.0 where every span it recorded is a span it then ships; endpoint and headers stay the deploy plane's `OTEL_EXPORTER_OTLP_*` rows while protocol and compression ride the program, so neither owner leans on a key the deploy plane never publishes; histogram aggregation rides the same `SignalGovernance.Views` projection this capsule binds, seating the base2-exponential configuration on every advice-free distribution with the kernel `Buckets` advice as the explicit-bucket re-arm, because no environment key on this SDK reaches that preference and a view is its one seat; this capsule opens NO durable queue — a blob file outliving the load context that wrote it replays through a provider the unload already disposed, so an unloadable capsule's failed batches die with it by design and durable egress stays the hosted root's; logs remain on the host `ILogger` projection and continuous profiling remains the process-wide Pyroscope agent.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.Loader;

namespace Rasm.AppHost.Observability;

// --- [POLICIES] ------------------------------------------------------------------------
public sealed record ProviderProgram(
    BatchExportProcessorOptions<Activity> SpanBatch,
    BatchExportLogRecordProcessorOptions LogBatch,
    PeriodicExportingMetricReaderOptions ReaderCadence,
    MetricReaderTemporalityPreference Temporality,
    ExemplarFilterType Exemplar,
    TextMapPropagator Propagator,
    Predicate<string> Baggage,
    OtlpExportProtocol Protocol,
    OtlpExportCompression Compression,
    Duration FlushBound) {
    public static readonly ProviderProgram Canonical = new(
        SpanBatch: new() { MaxQueueSize = 4096, MaxExportBatchSize = 512, ScheduledDelayMilliseconds = 5_000, ExporterTimeoutMilliseconds = 30_000 },
        LogBatch: new() { MaxQueueSize = 8192, MaxExportBatchSize = 1024, ScheduledDelayMilliseconds = 2_000, ExporterTimeoutMilliseconds = 30_000 },
        ReaderCadence: new() { ExportIntervalMilliseconds = 60_000, ExportTimeoutMilliseconds = 30_000 },
        Temporality: MetricReaderTemporalityPreference.Delta,
        Exemplar: ExemplarFilterType.TraceBased,
        Propagator: Correlation.Spine,
        Baggage: static key => key is TenantContext.TenantSlot or CorrelationId.Slot,
        Protocol: OtlpExportProtocol.HttpProtobuf,
        Compression: OtlpExportCompression.GZip,
        FlushBound: Duration.FromSeconds(5));

    public TracerProviderBuilder Bind(TracerProviderBuilder builder) =>
        builder.AddBaggageActivityProcessor(Baggage);

    public MeterProviderBuilder Bind(MeterProviderBuilder builder, Func<Instrument, MetricStreamConfiguration?> views) =>
        builder.SetExemplarFilter(Exemplar).AddView(views);

    public Unit Egress(OtlpExporterOptions otlp) =>
        ((otlp.Protocol, otlp.Compression) = (Protocol, Compression), unit).Item2;

    public int FlushMillis => (int)FlushBound.TotalMilliseconds;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PluginTelemetryHost {
    private readonly Atom<Option<Unit>> released = Atom(Option<Unit>.None);
    private readonly ServiceProvider services;
    private readonly TracerProvider tracing;
    private readonly MeterProvider metrics;

    private PluginTelemetryHost(ServiceProvider services, TracerProvider tracing, MeterProvider metrics) =>
        (this.services, this.tracing, this.metrics) = (services, tracing, metrics);

    public IMeterFactory Meters => services.GetRequiredService<IMeterFactory>();

    public static PluginTelemetryHost Open(
        AssemblyLoadContext alc, ResolvedProfile resolved, Seq<TelemetryContributorPort> contributors,
        Seq<InstrumentRule> suppressed, params ReadOnlySpan<KeyValuePair<string, object>> extra) {
        ProviderProgram program = ProviderProgram.Canonical;
        var services = new ServiceCollection()
            .AddMetrics(metrics => suppressed.Fold(metrics, static (builder, rule) =>
                builder.DisableMetrics(rule.MeterName, rule.InstrumentName, rule.ListenerName, MeterScope.Local)))
            .BuildServiceProvider();
        var identity = ResourceIdentity.Compose(resolved, extra);
        var views = SignalGovernance.Views(contributors);
        Sdk.SetDefaultTextMapPropagator(program.Propagator);
        var pipeline = LogPipeline.Owner(resolved.Profile);
        var tracing = pipeline.Switch(
                state: (Program: program, Identity: identity, Profile: resolved.Profile),
                state2: program.Bind(Sdk.CreateTracerProviderBuilder()
                    .ConfigureResource(identity)
                    .AddSource([.. TelemetrySource.Items.Select(static row => row.Key)])
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(TelemetrySignal.Trace.Ratio(resolved.Profile))))),
                serilogProjection: static (_, builder) => builder,
                otelExport: static (bind, builder) => builder.AddOtlpExporter(otlp => {
                    otlp.BatchExportProcessorOptions = bind.Program.SpanBatch;
                    ignore(bind.Program.Egress(otlp));
                }))
            .Build();
        var metrics = pipeline.Switch(
                state: program,
                state2: program.Bind(Sdk.CreateMeterProviderBuilder()
                    .ConfigureResource(identity)
                    .AddMeter([.. TelemetrySource.Items.Select(static row => row.Key)]), views),
                serilogProjection: static (_, builder) => builder,
                otelExport: static (held, builder) => builder.AddOtlpExporter((otlp, reader) => {
                    reader.TemporalityPreference = held.Temporality;
                    reader.PeriodicExportingMetricReaderOptions = held.ReaderCadence;
                    ignore(held.Egress(otlp));
                }))
            .Build();
        var host = new PluginTelemetryHost(services, tracing, metrics);
        alc.Unloading += _ => ignore(host.Release(ProviderProgram.Canonical));
        return host;
    }

    public Fin<Unit> Release(ProviderProgram program) =>
        Cell.Seat(released, static () => unit) is Transition<Option<Unit>>.Committed
            ? (Drained(tracing.ForceFlush(program.FlushMillis), TelemetrySignal.Trace),
               Drained(metrics.ForceFlush(program.FlushMillis), TelemetrySignal.Metric))
                .Apply(static (_, _) => unit).As().ToFin()
                .Bind(_ => Torn())
            : Fin.Succ(unit);

    private static Validation<Error, Unit> Drained(bool flushed, TelemetrySignal signal) =>
        flushed
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new InstrumentFault.Flush(signal.Key));

    private Fin<Unit> Torn() {
        tracing.Dispose();
        metrics.Dispose();
        services.Dispose();
        return Fin.Succ(unit);
    }
}
```

## [05]-[OBSERVATION_RAIL]

- Owner: `MetricCollector<T>` — the assertion tap composed directly at test sites; the package surface is the rail, and construction or snapshot forwarding around it is the rename adapter the prohibitions delete.
- Entry: `new MetricCollector<T>(factory.Create(TelemetrySource.AppHost.Key), instrumentName, time)` — the meter-plus-name overload over the meter the test's own `IMeterFactory` mints, so the collector and the mounted set share one provider scope; the package takes no `IMeterFactory` because the factory's product IS the meter this argument carries; `GetMeasurementSnapshot()` yields the indexable measurement list assertions fold over, and `WaitForMeasurementsAsync` is the bounded gate an asynchronously emitted row waits on, under a token or a wall timeout.
- Auto: a factory-scoped collector isolates parallel tests observing one meter name; the injected `FakeTimeProvider` stamps every collected measurement, so a captured timestamp is a pure function of the advance sequence; the asserted name reads `AppHostMeasure.<Row>.Row.Name` rather than a literal, so a spec cannot outlive the row it grades.
- Packages: Microsoft.Extensions.Diagnostics.Testing, Microsoft.Extensions.TimeProvider.Testing, BCL inbox.
- Growth: one collector per asserted instrument; a multi-instrument assertion is one collector row per instrument, never a shared listener.
- Boundary: `T` is the row's own `MeasureForm` — the collector admits `long` and `double` beside the other primitive numerics and throws at construction on any other argument, so a spec reads the mounted row rather than the value it expects; `RecordObservableInstruments()` is mandatory on every pulled row — `Level`, `Levels`, `Total`, and `Balance` observe at collection cadence and emit nothing until asked, so an assertion over an `InstrumentSet.Level` write reads an empty snapshot without it while a pushed row needs no pull; the scope-keyed overload resolves a meter the spec never holds and a null scope binds the process-global meter, so both forms run non-parallel by declaration while the factory-minted meter is the parallel-safe binding; `dotnet-counters` attaches by PID and live-reads every `rasm.*` meter with no exporter — a free out-of-process debugging surface over the identical instruments, a tool boundary and never a code dependency; deep EventPipe capture stays the Observability/bundles support-capture lane.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
