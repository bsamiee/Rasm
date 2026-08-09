# [APPHOST_DOMAIN_INSTRUMENTS]

One table-driven roster projects the receipt fan into `System.Diagnostics.Metrics` instruments, so every operational metric is a projection of a typed receipt, never a parallel truth minted at a call site. Owned axes: the AppHost `InstrumentSpec` roster with its dimension-slot vocabulary, the board-and-objective pack over those rows, the envelope-to-instrument projection fold over the kernel `ReceiptFan`, the contributed-port merge, the per-ALC provider-lifetime capsule for zero-host plugin processes, and the instrument observation rail — libraries emit through `Meter`, `ActivitySource`, and `ILogger` alone, and every OpenTelemetry SDK member on this page composes at a composition root.

Settled composition: `InstrumentSpec` with its `InstrumentKind` and `MeasureForm` axes, `InstrumentSet`, `Buckets`, `LevelCells`, `InstrumentArm`/`ReceiptFan`, `TelemetryContributorPort`, and `TelemetryIdentity.Metered` arrive from the kernel signal capsule `Rasm/Domain/telemetry#INSTRUMENT_MECHANISM` — this page composes instances and laces app identity, never re-declares the mechanism; `TelemetrySource`, `CorrelationId`, and `ReceiptEnvelope` from `Rasm/Domain/telemetry#CAUSAL_FRAME`; `Sli`, `LevelBreach`, `Objective`, `PanelSpec`, `PanelKind`, `BoardPack`, and the multi-window burn table from `Rasm/Domain/telemetry#SLO_ALGEBRA`, so the AppHost declares instances alone and mints no severity, burn, polarity, or panel vocabulary; the observe tap that feeds the projection from Observability/hooks#HOOK_RAIL; the trace-based exemplar row, the `SignalGovernance.Views` per-instrument stream projection, the `SpanBatch`/`ReaderCadence` wire squares, the `LogPipeline.Owner` export arbitration, and the `Correlation.Spine` propagator composite from Observability/telemetry#SIGNAL_GOVERNANCE. Metric names are dotted `rasm.<domain>.<measure>`, units are UCUM (`s`, `By`, `1`, `{thing}`), never pre-baked `_total`/unit suffixes; instrumentation scope name is the emitting package id, version-stamped and schema-pinned through the contributor port's `SchemaUrl` coordinate, identical across tracer, meter, and logger; Prometheus translation standardizes on `NoUTF8EscapingWithSuffixes` so dotted names survive byte-identical from every runtime.

## [01]-[INDEX]

- [02]-[INSTRUMENT_CATALOG]: AppHost instrument roster, cost-vector derivation, GenAI rows, level cells, the board pack, and the contributor port.
- [03]-[RECEIPT_PROJECTION]: One projection fold from the receipt fan onto the mounted instruments, and the contributed-port merge.
- [04]-[PROVIDER_LIFETIME]: Per-ALC `IMeterFactory` capsule with unload-ordered flush and dispose.
- [05]-[OBSERVATION_RAIL]: `MetricCollector<T>` assertion rail and the out-of-process live-read boundary.

## [02]-[INSTRUMENT_CATALOG]

- Owner: `HostInstruments` — the ONE AppHost roster of `InstrumentSpec` values: the spine rows (log flush, redaction, drain) and the domain rows in one declaration, the dimension-slot consts every `[03]` arm writes through, the `BoardPack` of panels and reliability objectives over those same rows, and the contributor-port mint beside them.
- Cases: incident-buffer flush counts off `IncidentBuffers.Flush` and sink-loss counts off the `SpineLossFold` fact, the two log-plane rows whose producing arms sit at Observability/telemetry#LOG_PROJECTION; masked-value counts off the exported support manifest, the one plane carrying both a mask and its classification; hop attempt and duration rows off `HopReceipt`; stale-binding and degradation-level rows read the composition's `LevelCells` scalars, the capability-roster row reading the keyed family the frozen capability registry projects off its own surface index at composition; broker delivery-outcome counts off `DeliveryReceipt`; command-admission counts and per-unit spend rows off `CommandReceipt`, the spend family derived from the `CostUnit` vocabulary; lifecycle-transition counts off the `Phase` hook tap; benchmark duration and regression rows off `BenchmarkReceipt`; live-wire rejection counts off the inbound coercion seam and write-back disposition counts off `WriteReceipt`; fleet-wave counts off `RollAnnotationWire`; machine-observation counts off the decode lane; durable-egress batch and byte counts off `OtlpOfflineFact`, one population partitioned on signal and disposition; the two GenAI semconv rows `gen_ai.client.token.usage` and `gen_ai.client.operation.duration` stamped by the governed model loop.
- Entry: `HostInstruments.Rows` — the declaration roster, each row naming its kind, measurement form, and dimension slots so the bind body derives and no create is spelled here; `HostInstruments.Board` — the kernel pack of panels and reliability objectives over those same rows, handed outward on the port rather than reached by name from a composition root; `HostInstruments.Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl)` — the one `TelemetryContributorPort` carrying the row set, its semconv schema coordinate, and that pack, mounted and proved beside every sibling contributor port at `[03]-[RECEIPT_PROJECTION]`.
- Auto: the spend rows derive from `CostUnit.Items` and read each row's own `Ucum` column, so the unit vocabulary answers once at its owner and a second UCUM map here forks on any admission; a histogram row naming a `Buckets` policy ships `InstrumentAdvice<T>` explicit-bucket boundaries at creation — the fallback a backend without exponential histograms reads — while a bare `Distribution` row keeps the base2-exponential default the `[04]` provider rows pin; a keyed level family names its tag once at the factory, which seats it as the row's own leading dimension, so per-key cardinality rides one instrument, a panel breaks on the key with no second declaration, and a per-key instrument mint is the deleted form; that leading dimension declares the key the family MAY carry rather than one every entry holds, so a producer whose group has no key writes the same row untagged and the declaration reads as its own absence arm — one roster answers a partitioned and an unpartitioned composition exactly as one `Dimensions` set answers a tenanted and an untenanted write — and the untagged entry is that family's absent-key PARTITION, never a total over its keyed ones — a total there doubles the population they already carry; a reliability target over a tag-partitioned counter reads the good values off that counter's declared dimension, so an availability objective mints no numerator twin; each panel omits its widget so `PanelKind.For` derives the canonical one from the named row's measurement shape, and each objective omits its window so the kernel compliance default applies.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: one domain metric is one `InstrumentSpec` factory call in this roster and one projection arm at `[03]-[RECEIPT_PROJECTION]`; a per-unit family derives from its owning vocabulary, never hand-enumerated rows; a level is one `set.Level` write at its producing arm and one `Level` or `Levels` declaration; a new tag key is one slot const both the row's `Dimensions` and its arm read, concatenating the one package head; one board tile is one `PanelSpec` row and one reliability target one `Objective` row on the same pack.
- Boundary: every row binds through the composition's minted meter, so no instrument outlives its `IMeterFactory` owner; `LevelCells` reaches a pulled row at bind time through the kernel spec's own bind derivation, so no row closes over the cell and one declaration mounts against any composition — three arms write the levels the gauges read and each spells ONE call, so a pulled row is current at every collection and no process-static cell exists: the degradation tap writes `fan.Set.Level(HealthLevel, (double)reading.Level.Rank)` at `[03]`'s own `Tap`, the Agent/capability registry mount writes the keyed `CapabilityRoster` family off its surface index, and the Wire/livewire binding fold writes `fan.Set.Level(BindingStale, stale)` on every binding-state commit, `stale` being the count its own state map holds in a stale or faulted case — a level written on ENTRY to that state alone decays into a reading no recovery ever clears; the drain band writes its distribution row at the same altitude, one `fan.Set.Write(DrainDuration, consumed.TotalSeconds, InstrumentSet.Tags((BandSlot, band.Key)))` per completed drain step at Runtime/lifecycle#DRAIN_CONDUCTOR, so the reliability objective over that series grades a measured population instead of reading permanently healthy over an empty one — the roster family is the composition-freeze projection its owning registry carries, never a per-admission push a registration fold strands mid-composition; the GenAI rows keep the semconv spelling rather than the `rasm.*` namespace because the convention owns the name, and their `gen_ai.token.type` dimension carries the input/output split the reasoning loop stamps; the declared `Dimensions` ARE the exported tag vocabulary — the governance view predicate at Observability/telemetry#SIGNAL_GOVERNANCE projects each stream onto this row's own list beside the one tenancy key under one series budget, so an undeclared tag reaches no exporter and a row admits an evidence string only by declaring it here, where this roster's classification grades it — a declared key a given entry omits is absent from that entry's tag set, which the one predicate row shapes exactly as it shapes the keyed entries, so a sometimes-absent dimension mints no second stream and the whole family stays inside one budget; the pack declares HERE rather than beside the alert engine because a panel and an objective name mounted instruments while a health rule names a `HealthSignal`, so the two never share an evidence plane — objectives compile to `AlertSpec` rows the deploy plane provisions off a metric series, and the alert engine folds the in-process degradation reading, so a burn arm on either side mints the second grader both boundaries forbid; the pack leaves this roster only on the contributor port, so the AppHost proves at `[03]-[RECEIPT_PROJECTION]` through the same fold every sibling pack rides and no composition root spells this static field; admission resolves against the port's declaration rather than a mounted handle set, so a plugin-ALC contributor's pack is provable at the same seam.

```csharp signature
public static class HostInstruments {
    // One head for the package's whole vocabulary: every measure name and every dimension key concatenates it at
    // compile time off the estate namespace const, so the prefix is stated once for the whole branch and a rename
    // moves resource, instrument, and dimension together. The segment is the `TelemetryDomain` roster's own, and
    // `SignalGovernance.Rostered` proves it when the contributed port mounts.
    private const string Head = TelemetryDomain.Prefix + "apphost.";

    // One slot const per tag key: a row's declared Dimensions and the [03] arm that writes the tag read the
    // same symbol, so a rename cannot leave a view keyed on a dimension no arm stamps. Keys carry the package
    // head because `outcome`, `case`, and `verdict` are concepts four packages tag and a bare noun collides
    // on the second. Semconv owns `gen_ai.token.type`, which therefore keeps its own spelling.
    public const string HopSlot = Head + "hop";
    public const string OutcomeSlot = Head + "outcome";
    public const string ChannelSlot = Head + "channel";
    public const string TxnSlot = Head + "txn";
    public const string BindingSlot = Head + "binding";
    public const string StrategySlot = Head + "strategy";
    public const string VerdictSlot = Head + "verdict";
    public const string MachineSlot = Head + "machine";
    public const string SuiteSlot = Head + "suite";
    public const string CaseSlot = Head + "case";
    public const string SurfaceSlot = Head + "surface";
    public const string BandSlot = Head + "drain.band";
    public const string ClassSlot = Head + "data.class";
    public const string FromSlot = Head + "phase.from";
    public const string ToSlot = Head + "phase.to";
    public const string TriggerSlot = Head + "phase.trigger";
    public const string SignalSlot = Head + "signal";
    public const string DispositionSlot = Head + "otlp.disposition";
    public const string ScopeSlot = Head + "buffer.scope";
    public const string SinkSlot = Head + "sink";
    public const string LossSlot = Head + "loss.kind";
    public const string TokenTypeSlot = "gen_ai.token.type";

    public const string LogsFlushed = Head + "logs.flushed";
    public const string LogsLost = Head + "logs.lost";
    public const string RedactionTags = Head + "redaction.tags";
    public const string DrainDuration = Head + "drain.duration";
    public const string HopAttempts = Head + "hop.attempts";
    public const string HopDuration = Head + "hop.duration";
    public const string BindingStale = Head + "binding.stale";
    public const string DeliveryOutcomes = Head + "delivery.outcomes";
    public const string CommandAdmissions = Head + "command.admissions";
    public const string CapabilityRoster = Head + "capability.roster";
    public const string LifecycleTransitions = Head + "lifecycle.transitions";
    public const string HealthLevel = Head + "health.level";
    public const string BenchmarkDuration = Head + "benchmark.duration";
    public const string BenchmarkRegressions = Head + "benchmark.regressions";
    public const string WireRejections = Head + "wire.rejections";
    public const string WriteDispositions = Head + "write.dispositions";
    public const string FleetWaves = Head + "fleet.waves";
    public const string MachineObservations = Head + "machine.observations";
    public const string OtlpOffline = Head + "otlp.offline";
    public const string OtlpOfflineBytes = Head + "otlp.offline.size";
    public const string GrantSpendPrefix = Head + "grant.spend.";
    public const string ModelTokenUsage = "gen_ai.client.token.usage";
    public const string ModelOperationDuration = "gen_ai.client.operation.duration";

    // One spelling of the per-unit name: the roster hands a vocabulary row's key and the [03] arm hands the
    // wire slot name, so a prefix concatenation at either site is the fork this projection deletes.
    public static string GrantSpend(string unit) => $"{GrantSpendPrefix}{unit}";

    // Items-derived roster materializes on first read: an eager field initializer folds another type's
    // vocabulary before its own static construction is protected, capturing an empty roster.
    private static readonly Lazy<Seq<InstrumentSpec>> Roster = new(static () => Seq(
        // Flushes count SCOPES, not records: `LogBuffer.Flush` publishes no count and replays through a
        // provider-owned `IBufferedLogger` this branch never holds, so a record unit here would name a
        // measurement no surface takes. Scope presence is measured, and which hold an incident drained is what
        // the count is read for.
        InstrumentSpec.Count(LogsFlushed, "{flush}", "incident buffer flushes by held scope", MeasureForm.Whole, ScopeSlot),
        InstrumentSpec.Count(LogsLost, "{record}", "log records lost by sink and failure kind", MeasureForm.Whole, SinkSlot, LossSlot),
        InstrumentSpec.Count(RedactionTags, "{value}", "values this branch's egress seam masked, by classification", MeasureForm.Whole, ClassSlot),
        InstrumentSpec.Distribution(DrainDuration, "s", "drain-band fold duration per band", MeasureForm.Real, BandSlot),
        InstrumentSpec.Count(HopAttempts, "{attempt}", "outbound hop attempts by hop kind and outcome", MeasureForm.Whole, HopSlot, OutcomeSlot),
        InstrumentSpec.Advised(HopDuration, "s", "outbound hop wall duration per attempt", MeasureForm.Real, Buckets.HopSeconds, HopSlot, OutcomeSlot),
        InstrumentSpec.Level(BindingStale, "{binding}", "external bindings in stale or faulted state", MeasureForm.Whole),
        InstrumentSpec.Count(DeliveryOutcomes, "{delivery}", "broker deliveries by channel and outcome", MeasureForm.Whole, ChannelSlot, OutcomeSlot),
        InstrumentSpec.Count(CommandAdmissions, "{command}", "command dispatches by transaction disposition", MeasureForm.Whole, TxnSlot),
        // Descriptors carry a REQUIRED surface key, so this family writes no untagged entry — one keyed row
        // whose declared key every entry holds, and [03]'s governance view shapes it identically to a family
        // that sometimes omits one.
        InstrumentSpec.Levels(CapabilityRoster, "{descriptor}", "live capability descriptors by admitting surface", MeasureForm.Whole, SurfaceSlot),
        InstrumentSpec.Count(LifecycleTransitions, "{transition}", "lifecycle phase commits by from, to, and trigger", MeasureForm.Whole, FromSlot, ToSlot, TriggerSlot),
        InstrumentSpec.Level(HealthLevel, "1", "derived degradation level rank, zero full through four suspended", MeasureForm.Whole),
        InstrumentSpec.Advised(BenchmarkDuration, "s", "gated benchmark median wall duration per case", MeasureForm.Real, Buckets.BenchSeconds, SuiteSlot, CaseSlot),
        InstrumentSpec.Count(BenchmarkRegressions, "{run}", "benchmark gate verdicts past budget by suite, case, and verdict", MeasureForm.Whole, SuiteSlot, CaseSlot, VerdictSlot),
        InstrumentSpec.Count(WireRejections, "{rejection}", "live-wire inbound values rejected at coercion or staleness", MeasureForm.Whole),
        InstrumentSpec.Count(WriteDispositions, "{write}", "write-back transactions by binding", MeasureForm.Whole, BindingSlot),
        InstrumentSpec.Count(FleetWaves, "{wave}", "fleet rollout waves by strategy, channel, and verdict", MeasureForm.Whole, StrategySlot, ChannelSlot, VerdictSlot),
        InstrumentSpec.Count(MachineObservations, "{observation}", "decoded machine-telemetry observations by machine", MeasureForm.Whole, MachineSlot),
        // Durable-egress rows partition on ONE disposition dimension: per-outcome instruments mount six
        // streams whose denominator no consumer reconstructs, and byte count is a second UNIT over the
        // same population rather than a second population.
        InstrumentSpec.Count(OtlpOffline, "{batch}", "otlp export batches by signal and durable-queue disposition", MeasureForm.Whole, SignalSlot, DispositionSlot),
        InstrumentSpec.Count(OtlpOfflineBytes, "By", "otlp payload bytes crossing the durable queue by signal and disposition", MeasureForm.Whole, SignalSlot, DispositionSlot),
        InstrumentSpec.Advised(ModelTokenUsage, "{token}", "model tokens consumed per operation by token type", MeasureForm.Whole, Buckets.TokenCounts, TokenTypeSlot),
        InstrumentSpec.Advised(ModelOperationDuration, "s", "governed model operation duration", MeasureForm.Real, Buckets.ModelSeconds))
        + CostUnit.Items.AsIterable().Map(static unit => InstrumentSpec.Count(
            GrantSpend(unit.Key), unit.Ucum, $"cost debited from the {unit.Key} balance", MeasureForm.Whole)).ToSeq(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static Seq<InstrumentSpec> Rows => Roster.Value;

    // Reliability and board policy travel WITH the roster they name: every objective series and every panel
    // break key resolves against the rows above, so a renamed row, a break key no arm stamps, and an
    // objective declared in the wrong statistical role each refuse inside InstrumentFan.Mount before the
    // first meter mints, while the descriptor is still editable. Widgets stay absent because each row's own measurement shape
    // derives one; windows pass Duration.Zero to take the kernel compliance default, so no window literal
    // is restated here and a tuned discipline is one BurnRow edit every objective re-derives.
    public static readonly BoardPack Board = new(
        Wire: "apphost.instrument", // the provenance key the deploy tuple admits this projection under; pack and key are one value
        Panels: Seq(
            PanelSpec.Of("outbound hops", HopAttempts, HopSlot, OutcomeSlot),
            PanelSpec.Of("hop latency", HopDuration, HopSlot, OutcomeSlot),
            PanelSpec.Of("drain duration", DrainDuration, BandSlot),
            PanelSpec.Of("broker deliveries", DeliveryOutcomes, ChannelSlot, OutcomeSlot),
            PanelSpec.Of("command admissions", CommandAdmissions, TxnSlot),
            PanelSpec.Of("capability roster", CapabilityRoster, SurfaceSlot),
            PanelSpec.Of("lifecycle transitions", LifecycleTransitions, FromSlot, ToSlot, TriggerSlot),
            PanelSpec.Of("degradation level", HealthLevel),
            PanelSpec.Of("stale bindings", BindingStale),
            PanelSpec.Of("redacted values", RedactionTags, ClassSlot),
            PanelSpec.Of("log loss", LogsLost, SinkSlot, LossSlot),
            PanelSpec.Of("benchmark regressions", BenchmarkRegressions, SuiteSlot, CaseSlot, VerdictSlot),
            PanelSpec.Of("fleet waves", FleetWaves, StrategySlot, ChannelSlot, VerdictSlot),
            PanelSpec.Of("model tokens", ModelTokenUsage, TokenTypeSlot),
            PanelSpec.Of("durable otlp queue", OtlpOffline, SignalSlot, DispositionSlot)),
        Objectives: Seq(
            // Availability partitions the outcome dimension the counter already carries; a delivered-only twin
            // counter beside it doubles the mounted series and strands its denominator on any arm edit.
            Objective.Create("apphost.hop.availability", new Sli.Partition(HopAttempts, OutcomeSlot, Seq(HopOutcomeWire.Delivered)), 0.99d, Duration.Zero),
            Objective.Create("apphost.delivery.availability", new Sli.Partition(DeliveryOutcomes, OutcomeSlot, Seq(HopOutcomeWire.Delivered)), 0.99d, Duration.Zero),
            Objective.Create("apphost.hop.latency", new Sli.Latency(HopDuration, Duration.FromSeconds(1), 0.99d), 0.99d, Duration.Zero),
            Objective.Create("apphost.drain.latency", new Sli.Latency(DrainDuration, Duration.FromSeconds(30), 0.95d), 0.99d, Duration.Zero),
            Objective.Create("apphost.model.latency", new Sli.Latency(ModelOperationDuration, Duration.FromSeconds(30), 0.95d), 0.95d, Duration.Zero),
            // Rank rises as capability degrades, so the ceiling polarity reads the level in its own unit.
            Objective.Create("apphost.degradation", new Sli.Saturation(HealthLevel, DegradationLevel.ReducedRemote.Rank, LevelBreach.Ceiling), 0.99d, Duration.Zero)));

    // Rows, the platform's own trace planes, and the pack over them leave as ONE downward fact, so the mounting
    // root proves this pack in the same fold every sibling contributor's rides and reaches no AppHost type by
    // name. The outbox drain plane is this package's only span custody — every other host bracket opens under a
    // kernel domain the band already holds — so one plane row carries it and the relay's `Band` binds from the
    // composition that admitted it, never from a second source this page would otherwise mint.
    public static TelemetryContributorPort Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl) =>
        new(Scope: TelemetrySource.AppHost.Key, Version: version, Instruments: Rows,
            Planes: Seq(OutboxRelay.Scope), SchemaUrl: schemaUrl, Board: Board);
}
```

## [03]-[RECEIPT_PROJECTION]

- Owner: `InstrumentFan` — the AppHost kind constants, the AppHost arm table, and the composition entries over the kernel `ReceiptFan`.
- Entry: `InstrumentFan.Mount(IMeterFactory factory, CorrelationId root, LevelCells cells, Seq<FrozenDictionary<string, InstrumentArm>> contributed, params ReadOnlySpan<TelemetryContributorPort> contributors)` returns `Fin<ReceiptFan>`, proves the whole merged kind partition across the AppHost table and every contributed table and gates every contributor through `SignalGovernance.Rostered` before a meter exists, and mints each admitted contributor's meter through the kernel `TelemetryIdentity.Metered` — the meter-only mint, because span custody is the kernel band's and a paired `ActivitySource` this root never admits is a leaked source; the port's plain `Scope` string names the meter, its `SchemaUrl` coordinate stamps `MeterOptions.TelemetrySchemaUrl`, and this composition laces the boot correlation as the one meter tag — hands every `(meter, port.Instruments)` pair to the kernel `InstrumentSet.Of` beside the one composition `LevelCells` — the MOUNTED half alone, because a port's `Published` rows already carry handles on a meter their own load context owns and a second bind here mints a second stream per name —  folds each port's argument-free `Admit` ahead of the first mint so every carried `BoardPack` proves against its own declaring port — a self-minted roster included — and hands the already-proved kind-arm tables to `ReceiptFan.Of`, so contribution is downward, self-identifying, and self-proving: a contributor never names a platform type, this root reaches no package's pack field by name, and it maps the scope strings into its `AddMeter`/`AddSource` admission; `InstrumentFan.Project(ReceiptFan fan, ReceiptEnvelope envelope)` folds one envelope into instrument writes on the kernel rail; `InstrumentFan.Tap(HookRail rail, ReceiptFan fan)` mounts the fan's three hook subscriptions at composition.
- Auto: `Tap` subscribes the Observability/hooks#HOOK_RAIL points — the `Receipt` tap projects every registered envelope kind through the arm table, the `Phase` tap counts `rasm.apphost.lifecycle.transitions` per commit, and the `Degradation` tap folds `DegradationReading.Level.Rank` into the health level the `[02]` gauge reads — so every settled projection rides the hook rail with zero call-site metering.
- Law: `Project` brackets each envelope in its own `TenantContext.Stamp`, so every arm reads the tenant the sink recorded.
- Receipt: none — the fan is a projection of receipts; a metric minted beside the fan is a second truth.
- Packages: Rasm, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new projected receipt is one kind constant, one kind-keyed table row, and its instrument row at `[02]`; a sibling package's projected slot is one row in its contributed arm table; a contributor's first board is one `Board` value on its own port with this fold untouched; an unmapped kind projects nothing and stays receipt-only by declaration.
- Boundary: the kind constants are the canonical spellings — every emitting page passes its `InstrumentFan` constant to `ReceiptSinkPort.Send` (`HopKind` at Wire/outbound, `DeliveryKind` at the delivery fan, `SweepKind` at Wire/outbox, `CommandKind` at Agent/capability, `ModelKind` at Agent/reasoning, `BenchmarkKind` at Observability/benchmarks, `WireKind` at the Wire/livewire inbound rejection seam, `WriteKind` at the Wire/livewire write-back mint, `RollKind` at the Sandbox/provisioning fleet-wave annotation, `ObservationKind` at the Wire/livewire machine decode lane, `OfflineKind` at the Observability/telemetry#SIGNAL_GOVERNANCE durable OTLP queue, `LossKind` at that page's `SpineLossFold` listener, `SupportKind` at the Observability/bundles#MANIFEST_RECEIPT export) — and the kind registry is per-package: the AppHost table owns AppHost kinds, a contributed table owns its own slot spellings, and sibling fans project their kinds typed pre-envelope (AppUi over its evidence union, Compute over `ComputeReceipt`, Grasshopper over `GhEvidence`, Element over `ElementFact`) — so one envelope kind projects in exactly one arm and a `Send` kind outside every package fan is receipt-only by declaration; payload field reads stay inside the table arms — the one place wire names meet instrument writes — and a projection arm never re-validates the payload the typed owner already admitted; a wire `Duration` field crosses as its NodaTime roundtrip text and `Seconds` is the one arm-side decode.

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
    public const string OfflineKind = "otlp-offline";
    public const string LossKind = "log-loss";
    public const string SupportKind = "support-bundle";

    // One (wire field, semconv token type) row per column of the usage family: a third token type is one row,
    // never a third hand-spelled write pair on a family every column shares.
    private static readonly Seq<(string Field, string Type)> TokenColumns =
        Seq(("inputTokens", "input"), ("outputTokens", "output"));

    // Every arm returns the kernel write rail, so a refused measurement reaches the subscribing seam instead of
    // dying at the delegate; multi-write arms chain on Bind, so the first refusal names the offending row.
    internal static readonly FrozenDictionary<string, InstrumentArm> Table =
        new Dictionary<string, InstrumentArm> {
            // Fin.Succ heads the shared-tag arms to capture the payload reads BEFORE the first write, so both
            // writes carry one materialized tag set and neither re-reads a field the other already decoded.
            [HopKind] = static (set, payload) =>
                from tags in Fin.Succ(InstrumentSet.Tags(
                    (HostInstruments.HopSlot, payload.GetProperty("hop").GetString()),
                    (HostInstruments.OutcomeSlot, payload.GetProperty("outcome").GetString())))
                from _ in set.Write(HostInstruments.HopAttempts, payload.GetProperty("attempts").GetInt64(), tags)
                from done in set.Write(HostInstruments.HopDuration, payload.GetProperty("elapsedSeconds").GetDouble(), tags)
                select done,
            [DeliveryKind] = static (set, payload) =>
                set.Write(HostInstruments.DeliveryOutcomes, 1L, InstrumentSet.Tags(
                    (HostInstruments.ChannelSlot, payload.GetProperty("channel").GetString()),
                    (HostInstruments.OutcomeSlot, payload.GetProperty("outcome").GetString()))),
            // Charged slots are the CostUnit vocabulary's own key set, so the spend fold reads the payload
            // rather than re-enumerating a roster the receipt already carries.
            [CommandKind] = static (set, payload) =>
                set.Write(HostInstruments.CommandAdmissions, 1L, InstrumentSet.Tags(
                    (HostInstruments.TxnSlot, payload.GetProperty("txn").GetProperty("kind").GetString())))
                    .Bind(_ => toSeq(payload.GetProperty("charged").EnumerateObject())
                        .TraverseM(slot => set.Write(HostInstruments.GrantSpend(slot.Name), slot.Value.GetInt64())).As())
                    .Map(static _ => unit),
            [ModelKind] = static (set, payload) =>
                TokenColumns.TraverseM(column => set.Write(
                    HostInstruments.ModelTokenUsage, payload.GetProperty(column.Field).GetInt64(),
                    InstrumentSet.Tags((HostInstruments.TokenTypeSlot, column.Type)))).As()
                    .Bind(_ => set.Write(HostInstruments.ModelOperationDuration, Seconds(payload.GetProperty("elapsed")))),
            [BenchmarkKind] = static (set, payload) =>
                from tags in Fin.Succ(InstrumentSet.Tags(
                    (HostInstruments.SuiteSlot, payload.GetProperty("suite").GetString()),
                    (HostInstruments.CaseSlot, payload.GetProperty("case").GetString())))
                from _ in set.Write(HostInstruments.BenchmarkDuration, Seconds(payload.GetProperty("median")), tags)
                // Passing verdicts mint no regression point, so the population stays the gate's failures alone.
                from done in payload.GetProperty("verdict").GetString() is { } verdict && verdict != BenchmarkVerdict.Pass.Key
                    ? set.Write(HostInstruments.BenchmarkRegressions, 1L, [.. tags, new(HostInstruments.VerdictSlot, verdict)])
                    : Fin.Succ(unit)
                select done,
            // Wire rejection payloads carry fault text alone; the write,
            // roll, and observation arms read their receipt fields through the same camelCase wire names.
            [WireKind] = static (set, _) =>
                set.Write(HostInstruments.WireRejections, 1L),
            [WriteKind] = static (set, payload) =>
                set.Write(HostInstruments.WriteDispositions, 1L, InstrumentSet.Tags(
                    (HostInstruments.BindingSlot, payload.GetProperty("bindingId").GetString()))),
            [RollKind] = static (set, payload) =>
                set.Write(HostInstruments.FleetWaves, 1L, InstrumentSet.Tags(
                    (HostInstruments.StrategySlot, payload.GetProperty("strategy").GetString()),
                    (HostInstruments.ChannelSlot, payload.GetProperty("channel").GetString()),
                    (HostInstruments.VerdictSlot, payload.GetProperty("verdict").GetString()))),
            [ObservationKind] = static (set, payload) =>
                set.Write(HostInstruments.MachineObservations, 1L, InstrumentSet.Tags(
                    (HostInstruments.MachineSlot, payload.GetProperty("machine").GetString()))),
            // Queue evidence never depends on the egress it repairs, so the offline fact rides the receipt
            // rail like every other arm and its writes ride whatever provider the composition already owns:
            // a direct meter write mints the second truth this fan deletes, and a log line rides the very
            // leg whose disposition it reports as down.
            [OfflineKind] = static (set, payload) =>
                from tags in Fin.Succ(InstrumentSet.Tags(
                    (HostInstruments.SignalSlot, payload.GetProperty("signal").GetString()),
                    (HostInstruments.DispositionSlot, payload.GetProperty("disposition").GetString())))
                from _ in set.Write(HostInstruments.OtlpOffline, 1L, tags)
                from done in set.Write(HostInstruments.OtlpOfflineBytes, payload.GetProperty("bytes").GetInt64(), tags)
                select done,
            // Sink loss rides the receipt rail for the same reason the offline fact does: the evidence must not
            // travel the leg it reports as down, and a log line about a failed log sink is exactly that leg.
            // The count is the failed batch's own record count, so a listener reporting a drop of zero records
            // writes zero rather than one — a per-failure tally would price a queue overflow like a single throw.
            [LossKind] = static (set, payload) =>
                set.Write(HostInstruments.LogsLost, payload.GetProperty("count").GetInt64(), InstrumentSet.Tags(
                    (HostInstruments.SinkSlot, payload.GetProperty("sink").GetString()),
                    (HostInstruments.LossSlot, payload.GetProperty("kind").GetString()))),
            // Redaction counts are the MANIFEST's own per-entry evidence, the one plane on this branch where a
            // masked value and its classification are both known: the generated tag path redacts inside the
            // provider and publishes neither. One point per entry keyed on the entry's classification, so a
            // bundle whose every artifact masked nothing writes nothing rather than a zero-valued row.
            [SupportKind] = static (set, payload) =>
                toSeq(payload.GetProperty("manifest").GetProperty("entries").EnumerateArray())
                    .Filter(static entry => entry.GetProperty("redactions").GetInt32() > 0)
                    .TraverseM(entry => set.Write(
                        HostInstruments.RedactionTags, entry.GetProperty("redactions").GetInt64(),
                        InstrumentSet.Tags((HostInstruments.ClassSlot, entry.GetProperty("classification").GetString()))))
                    .As().Map(static _ => unit),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // One meter per contributor port and one frozen mount across all of them is the kernel's own multi-mount
    // entry — a local fold over pre-bound pairs re-mints the ByName/Rows/Cells derivation the capsule already
    // owns. All THREE proof legs run BEFORE the first mint, because a meter minted for a port carrying an
    // unrostered segment, an unprovable pack, or a kind another table already claims is already registered when
    // the refusal lands: `Partition` grades the merged kind space, `Rostered` grades the dimension vocabulary,
    // and `Admit` grades each carried pack against the port's OWN declaration, so a contributor publishing
    // self-minted rows proves exactly as a mounted one does and every refusal names its descriptor while the
    // composition is still editable.
    public static Fin<ReceiptFan> Mount(IMeterFactory factory, CorrelationId root, LevelCells cells,
        Seq<FrozenDictionary<string, InstrumentArm>> contributed, params ReadOnlySpan<TelemetryContributorPort> contributors) =>
        from arms in Partition([Table, .. contributed])
        from rostered in Iterable<TelemetryContributorPort>.FromSpan(contributors).ToSeq()
            .TraverseM(SignalGovernance.Rostered).As()
        from _ in rostered.TraverseM(static port => port.Admit()).As()
        from ports in Fin.Succ(rostered.Map(port => (
            Port: port,
            Meter: TelemetryIdentity.Metered(factory, port.Scope, port.Version, port.SchemaUrl,
                new KeyValuePair<string, object?>(CorrelationId.Slot, root.ToString())))).Strict())
        select ReceiptFan.Of(
            InstrumentSet.Of(cells, [.. ports.Map(static row => (row.Meter, row.Port.Instruments))]),
            [.. arms]);

    // The frozen kind merge is composition-fatal by THROW, so the one-fan-per-kind partition proves on the rail
    // ahead of every mint: a duplicate discovered inside `ReceiptFan.Of` leaves each contributor's meter already
    // registered against a fan that never forms, and the process then carries orphan streams no dispose reaches.
    // The refusal names EVERY colliding kind, so one boot reads its whole partition defect rather than six.
    static Fin<Seq<FrozenDictionary<string, InstrumentArm>>> Partition(Seq<FrozenDictionary<string, InstrumentArm>> tables) =>
        toSeq(tables.Bind(static table => toSeq(table.Keys))
                .GroupBy(static kind => kind, StringComparer.Ordinal)
                .Where(static group => group.Count() > 1)
                .Select(static group => group.Key))
            .Match(
                Empty: () => Fin.Succ(tables),
                More: duplicated => Fin.Fail<Seq<FrozenDictionary<string, InstrumentArm>>>(new Fault.InvalidValue(
                    Label: string.Join(',', duplicated.Order(StringComparer.Ordinal)),
                    Requirement: "exactly one contributing arm table per receipt kind")));

    // Arms materialize tag sets through `InstrumentSet.Tags`, which folds AMBIENT tenancy in, while hook taps
    // drain on whatever thread their emitter released. Bracketing the envelope's own frame here makes every
    // arm attribute evidence to whichever tenant the sink stamped, never to tenancy a draining thread carried.
    // Seating one bracket at the fan rather than per arm covers every contributed table, where a per-arm stamp
    // leaves whichever arm forgot it silently mis-attributing.
    public static Fin<Unit> Project(ReceiptFan fan, ReceiptEnvelope envelope) {
        using IDisposable partition = envelope.Tenant.Stamp();
        return fan.Project(envelope.Kind, envelope.Payload);
    }

    // Every arm hands its Fin STRAIGHT to the capsule's typed-rail Observe, so a refused write parks as an
    // IsolatedFault beside every other tap fault; wrapping one in IO.lift and ignoring the rail re-mints the
    // lift the capsule already owns and swallows the refusal it exists to surface.
    public static IO<Seq<IDisposable>> Tap(HookRail rail, ReceiptFan fan) =>
        IO.lift(() => Seq<IDisposable>(
            rail.Receipt.Observe(envelope => Project(fan, envelope)),
            rail.Phase.Observe(value => fan.Set.Write(HostInstruments.LifecycleTransitions, 1L, InstrumentSet.Tags(
                (HostInstruments.FromSlot, value.From.Key),
                (HostInstruments.ToSlot, value.To.Key),
                (HostInstruments.TriggerSlot, value.Trigger)))),
            rail.Degradation.Observe(reading => fan.Set.Level(HostInstruments.HealthLevel, (double)reading.Level.Rank))));

    // NodaTime Duration crosses the wire as JsonRoundtrip text; the one arm-side decode to seconds. A
    // malformed or absent value raises through the hook's own fork shield and parks as an IsolatedFault,
    // which is why no arm-local fallback exists — a coerced zero would record a false measurement.
    static double Seconds(JsonElement element) =>
        DurationPattern.JsonRoundtrip.Parse(element.GetString() ?? string.Empty).Value.TotalSeconds;
}
```

[CONTRIBUTED_ARMS]: every emitting package contributes through the kernel port shape — a wire-borne contributor mounts one kind-arm table beside its port, a typed-fold contributor projects pre-envelope and contributes rows alone, and each port carries its own semconv schema coordinate beside whatever board pack its rows declare, which `Mount` proves against that port's own declaration roster. Host custody names where the contributor's meters live — hosted roots ride `SignalGovernance.Govern`, plugin-hosted processes ride the `PluginTelemetryHost` per-ALC capsule.

| [INDEX] | [CONTRIBUTOR]      | [PROJECTION]                           | [PORT_MINT]                        | [CUSTODY]      |
| :-----: | :----------------- | :------------------------------------- | :--------------------------------- | :------------- |
|  [01]   | `Rasm` kernel      | typed fold — `TelemetrySink.Tap`       | `KernelInstruments.Telemetry`      | composing root |
|  [02]   | `Rasm.Element`     | typed fold — `GraphInstrument`         | `ElementInstruments.Telemetry`     | composing root |
|  [03]   | `Rasm.Bim`         | typed fold — `BimTelemetry.Tap`        | `BimTelemetry.Telemetry`           | composing root |
|  [04]   | `Rasm.Materials`   | receipt arms at this fan               | `MaterialsInstruments.Telemetry`   | composing root |
|  [05]   | `Rasm.Fabrication` | `FabricationInstruments.Arms`/`.Facts` | `FabricationInstruments.Telemetry` | host root      |
|  [06]   | `Rasm.Persistence` | `StoreInstruments.Arms`                | `StoreInstruments.Telemetry`       | host root      |
|  [07]   | `Rasm.Compute`     | typed fold — `ComputeInstrumentFan`    | `ReceiptSurface.Telemetry`         | host root      |
|  [08]   | `Rasm.AppUi`       | typed fold — `EvidenceFan`             | `AppUiTelemetry.Contribute`        | host root      |
|  [09]   | `Rasm.Rhino`       | observe taps on the mount registry     | `RhinoInstruments.Telemetry`       | plugin ALC     |
|  [10]   | `Rasm.Grasshopper` | typed fold — `GhInstruments.Project`   | `PlatformTelemetry.Open`           | plugin ALC     |

## [04]-[PROVIDER_LIFETIME]

- Owner: `PluginTelemetryHost` — the zero-host trace-and-metric capsule for Rhino/GH plugin processes; one instance per plugin `AssemblyLoadContext` — the Grasshopper canvas ALC and the Rhino plugin ALC each open their own capsule, the custody the `[03]` contributor roster names.
- Entry: `PluginTelemetryHost.Open(AssemblyLoadContext alc, ResolvedProfile resolved, Seq<TelemetryContributorPort> contributors, Seq<InstrumentRule> suppressed, params ReadOnlySpan<KeyValuePair<string, object>> extra)` — builds the minimal per-ALC service provider under its enablement rows, the explicit tracer and meter providers, and the unload hook; the resolved row, the contributed roster, the capsule's own suppression rows, and the root's discriminator rows are the whole input, so the capsule derives its identity delegate and its view projection from their one owner and no caller hand-threads a resource lambda or a view row.
- Auto: `AddMetrics(services, builder => …)` mints the per-ALC `IMeterFactory` AND folds the capsule's enablement rows in the same call, so two co-resident plugins minting a `Rasm.Compute` meter in one `Rhino.exe` stay isolated by provider scope and each decides its own published set; every meter mint on that factory reaches it through `MeterFactoryExtensions.Create(factory, name)`, the name-shaped form, so no site re-spells a `MeterOptions` construction the kernel `TelemetryIdentity.Metered` already owns for the schema-stamped case; a capsule that must silence a series it does not own carries `InstrumentRule` rows scoped `MeterScope.Local` — the selector matching factory-minted meters alone, so a rule can never reach a ctor-constructed foreign meter — and `DisableMetrics(meterName, instrumentName, listenerName, MeterScope.Local)` is that suppression, which lets a plugin ALC drop a cardinality-hostile stream WITHOUT editing the contributor roster that declares it, since the roster is the shared truth every host reads and the enablement rows are per-capsule policy; `AssemblyLoadContext.Unloading` drives bounded `ForceFlush` then `Dispose` on both providers before the mini provider; the exemplar filter pins `ExemplarFilterType.TraceBased` so any measurement recorded inside an active span carries its trace and span id with zero wiring; the tracer builder carries the same `AddBaggageActivityProcessor(SignalGovernance.PromotedBaggage)` promotion row the hosted root binds, so tenant attribution holds on plugin spans; `Sdk.SetDefaultTextMapPropagator(Correlation.Spine)` seats the process propagator because no hosted root runs here to seat it; the governance `Views` projection, `SpanBatch`, `ReaderCadence`, and delta temporality rows fold here as VALUES read off their one owner, so a capsule and a hosted root cannot disagree on series budget, tag projection, batch squares, or wire temporality and a tuning edit moves both.
- Packages: OpenTelemetry, OpenTelemetry.Extensions, OpenTelemetry.Exporter.OpenTelemetryProtocol, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Diagnostics, LanguageExt.Core, BCL inbox.
- Growth: a new plugin-visible meter or source is one `AddMeter`/`AddSource` row in `Open`; a new resource dimension is one detector row inside `ResourceIdentity.Compose` or one `extra` row a plugin root supplies; a new stream shape is one `Dimensions` edit at the declaring roster both compositions inherit through the same view predicate; a per-capsule suppression is one `InstrumentRule` row in `suppressed`; `FlushBound` is the one unload-flush policy value.
- Boundary: enablement and shaping are two planes that never substitute for each other — an `InstrumentRule` decides whether an instrument PUBLISHES at all on this factory, while an `AddView` row decides how a published stream is shaped, so silencing a series through a `Drop` view still pays its instrument's recording cost and gating a mount through a rule still leaves every view row intact for the next capsule; the rules scope `MeterScope.Local` by declaration because a `Global` rule reaches ctor-constructed meters in the host process this capsule does not own; the provider — never the `Meter` or `ActivitySource` — is the disposable owner, and the capsule is the enforcing structure behind the process-static-meter prohibition: every meter reaches the process through a factory whose lifetime is the ALC; service-modality processes take the host-owned `SignalGovernance.Govern` path instead, so exactly one provider owner exists per process shape; egress rides the one `LogPipeline.Owner` arbitration the hosted root reads, so a capsule in a host session that bound no collector provider builds both providers and registers no exporter — an unconditional exporter opens a collector socket every reader cadence against a door that does not answer, at the attended sample ratio of 1.0 where every span it recorded is a span it then ships; OTLP egress reads the estate wire pins as values — `SignalGovernance.WireProtocol`/`WireCompression` on both exporter arms — while endpoint and headers stay the deploy plane's `OTEL_EXPORTER_OTLP_*` rows, so this capsule and the hosted root cannot disagree on wire shape and neither leans on a key the deploy plane never publishes; histogram aggregation rides the same `SignalGovernance.Views` projection this capsule already binds, seating the base2-exponential configuration on every advice-free distribution with the kernel `Buckets` advice as the explicit-bucket re-arm, because no environment key on this SDK reaches that preference and a view is its one seat; this capsule opens NO durable queue — a blob file outliving the load context that wrote it replays through a provider the unload already disposed, so an unloadable capsule's failed batches die with it by design and durable egress stays the hosted root's; logs remain on the host `ILogger` projection and continuous profiling remains the process-wide Pyroscope agent, so this capsule claims only the two providers it owns.

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

    public static PluginTelemetryHost Open(AssemblyLoadContext alc, ResolvedProfile resolved,
        Seq<TelemetryContributorPort> contributors, Seq<InstrumentRule> suppressed,
        params ReadOnlySpan<KeyValuePair<string, object>> extra) {
        // One AddMetrics call mints the factory AND folds this capsule's enablement rows: a rule appended past
        // provider construction is read by nobody, so publication policy and factory lifetime land together.
        // MeterScope.Local pins the match to factory-minted meters, keeping the rule inside this load context.
        var services = new ServiceCollection()
            .AddMetrics(metrics => suppressed.Fold(metrics, static (builder, rule) =>
                builder.DisableMetrics(rule.MeterName, rule.InstrumentName, rule.ListenerName, MeterScope.Local)))
            .BuildServiceProvider();
        // Extra rows ride the ONE identity owner: two capsules co-resident in one host process separate by
        // resource attribute, so the plugin discriminator each root stamps has to reach `Compose` or the two
        // report one identity and their series merge under a single emitter.
        var identity = ResourceIdentity.Compose(resolved, extra);
        // Identical view derivation to the hosted root's: one predicate resolving each published instrument
        // against the roster that declared it, so a capsule cannot mount the tenant dimension without the
        // series budget and undeclared tags inside a host process nobody restarts.
        var views = SignalGovernance.Views(contributors);
        // Propagation defaults are PROCESS-wide and no hosted root runs in a plugin process, so the SDK's
        // own equivalent-but-distinct composite answers every foreign library reading the propagator seat.
        Sdk.SetDefaultTextMapPropagator(Correlation.Spine);
        // Egress rides the ONE arbitration the hosted root reads: exporting unconditionally opens a collector
        // socket every cadence inside a desktop session that bound no provider, at the attended ratio of 1.0
        // where every recorded span is a span the capsule then ships at a door that does not answer.
        var pipeline = LogPipeline.Owner(resolved.Profile);
        // Kernel rows are minted whole, so both signals admit the roster entire; foreign instrumentation
        // scopes the hosted root admits stay out of a plugin ALC, which instruments no host transport.
        var tracing = pipeline.Switch(
                state: Sdk.CreateTracerProviderBuilder()
                    .ConfigureResource(identity)
                    .AddSource([.. TelemetrySource.Items.Select(static row => row.Key)])
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(TelemetrySignal.Trace.Ratio(resolved.Profile))))
                    .AddBaggageActivityProcessor(SignalGovernance.PromotedBaggage),
                serilogProjection: static builder => builder,
                otelExport: static builder => builder.AddOtlpExporter(static otlp => {
                    (otlp.Protocol, otlp.Compression) = (SignalGovernance.WireProtocol, SignalGovernance.WireCompression);
                    otlp.BatchExportProcessorOptions = SignalGovernance.SpanBatch;
                }))
            .Build();
        var metrics = pipeline.Switch(
                state: Sdk.CreateMeterProviderBuilder()
                    .ConfigureResource(identity)
                    .AddMeter([.. TelemetrySource.Items.Select(static row => row.Key)])
                    .SetExemplarFilter(ExemplarFilterType.TraceBased)
                    .AddView(views),
                serilogProjection: static builder => builder,
                otelExport: static builder => builder.AddOtlpExporter(static (otlp, reader) => {
                    (otlp.Protocol, otlp.Compression) = (SignalGovernance.WireProtocol, SignalGovernance.WireCompression);
                    reader.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                    reader.PeriodicExportingMetricReaderOptions = SignalGovernance.ReaderCadence;
                }))
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
- Entry: `new MetricCollector<T>(factory.Create(TelemetrySource.AppHost.Key), instrumentName, time)` — the meter-plus-name overload over the meter the test's own `IMeterFactory` mints, so the collector and the mounted fan share one provider scope; the package takes no `IMeterFactory` because the factory's product IS the meter this argument carries; `GetMeasurementSnapshot()` yields the indexable measurement list assertions fold over, and `WaitForMeasurementsAsync` is the bounded gate an asynchronously emitted row waits on, under a token or a wall timeout.
- Auto: a factory-scoped collector isolates parallel tests observing one meter name; the injected `FakeTimeProvider` stamps every collected measurement, so a captured timestamp is a pure function of the advance sequence.
- Packages: Microsoft.Extensions.Diagnostics.Testing, Microsoft.Extensions.TimeProvider.Testing, BCL inbox.
- Growth: one collector per asserted instrument; a multi-instrument assertion is one collector row per instrument, never a shared listener.
- Boundary: `T` is the row's own `MeasureForm` — the collector admits `long` and `double` beside the other primitive numerics and throws at construction on any other argument, so a spec reads the mounted row rather than the value it expects; `RecordObservableInstruments()` is mandatory on every pulled row — `Level`, `Levels`, `Total`, and `Balance` observe at collection cadence and emit nothing until asked, so an assertion over an `InstrumentSet.Level` write reads an empty snapshot without it while a pushed row needs no pull; the scope-keyed overload resolves a meter the spec never holds and a null scope binds the process-global meter, so both forms run non-parallel by declaration while the factory-minted meter is the parallel-safe binding; `dotnet-counters` attaches by PID and live-reads every `rasm.*` meter with no exporter — a free out-of-process debugging surface over the identical instruments, a tool boundary and never a code dependency; deep EventPipe capture stays the Observability/bundles support-capture lane.

## [06]-[RESEARCH]

(none)
